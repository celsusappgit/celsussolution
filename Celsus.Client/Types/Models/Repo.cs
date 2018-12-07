using Celsus.Client.Shared.Types;
using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Celsus.Client.Types.Models
{
    public class Repo : BaseModel<Repo>, MustInit
    {
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        static SemaphoreSlim semaphoreGetAll = new SemaphoreSlim(1, 1);

        private bool isInitted = false;

        ObservableCollection<SourceModel> sources;
        public ObservableCollection<SourceModel> Sources
        {
            get
            {
                return sources;
            }
            set
            {
                if (Equals(value, sources)) return;
                sources = value;
                NotifyPropertyChanged(() => Sources);
            }
        }

        
        ObservableCollection<WorkflowModel> workflows;
        public ObservableCollection<WorkflowModel> Workflows
        {
            get
            {
                return workflows;
            }
            set
            {
                if (Equals(value, workflows)) return;
                workflows = value;
                NotifyPropertyChanged(() => Workflows);
            }
        }

        ObservableCollection<SourceDto> internalSources;
        private ObservableCollection<SourceDto> InternalSources
        {
            get
            {
                return internalSources;
            }
            set
            {
                if (Equals(value, internalSources)) return;
                if (internalSources != null)
                {
                    internalSources.CollectionChanged -= Sources_CollectionChanged;
                }
                internalSources = value;
                if (internalSources != null)
                {
                    internalSources.CollectionChanged += Sources_CollectionChanged;
                }
                RebuildSources();
                NotifyPropertyChanged(() => InternalSources);
            }
        }

        ObservableCollection<WorkflowDto> internalWorkflows;
        private ObservableCollection<WorkflowDto> InternalWorkflows
        {
            get
            {
                return internalWorkflows;
            }
            set
            {
                if (Equals(value, internalWorkflows)) return;
                if (internalWorkflows != null)
                {
                    internalWorkflows.CollectionChanged -= Workflows_CollectionChanged;
                }
                internalWorkflows = value;
                if (internalWorkflows != null)
                {
                    internalWorkflows.CollectionChanged += Workflows_CollectionChanged;
                }
                RebuildWorkflows();
                NotifyPropertyChanged(() => InternalWorkflows);
            }
        }

        private void RebuildSources()
        {
            Sources = new ObservableCollection<SourceModel>();
            foreach (var internalSource in InternalSources)
            {
                var newSourceModel = new SourceModel
                {
                    SourceDto = internalSource
                };
                Sources.Add(newSourceModel);
            }
        }

        private void RebuildWorkflows()
        {
            Workflows = new ObservableCollection<WorkflowModel>();
            foreach (var internalWorkflow in InternalWorkflows)
            {
                var newWorkflowModel = new WorkflowModel
                {
                    WorkflowDto = internalWorkflow
                };
                Workflows.Add(newWorkflowModel);
            }
        }

        private void Sources_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var internalSource in e.NewItems)
                {
                    var newSourceModel = new SourceModel
                    {
                        SourceDto = (SourceDto)internalSource
                    };
                    Sources.Add(newSourceModel);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var internalSource in e.OldItems)
                {
                    var oldItem = Sources.SingleOrDefault(x => x.SourceDto.Id == ((SourceDto)internalSource).Id);
                    Sources.Remove(oldItem);
                }
            }
        }

        private void Workflows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var internalWorkflow in e.NewItems)
                {
                    var newWorkflowModel = new WorkflowModel
                    {
                        WorkflowDto = (WorkflowDto)internalWorkflow
                    };
                    Workflows.Add(newWorkflowModel);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var internalWorkflow in e.OldItems)
                {
                    var oldItem = Workflows.SingleOrDefault(x => x.WorkflowDto.Id == ((WorkflowDto)internalWorkflow).Id);
                    Workflows.Remove(oldItem);
                }
            }
        }

        public async Task<bool> UpdateWorkflow(WorkflowDto workflowDto)
        {
            try
            {
                using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                {
                    context.Entry(workflowDto).State = EntityState.Modified;
                    var saveResult = await context.SaveChangesAsync();
                    var old = InternalWorkflows.SingleOrDefault(x => x.Id == workflowDto.Id);
                    var oldIndex = InternalWorkflows.IndexOf(old);
                    InternalWorkflows.RemoveAt(oldIndex);
                    InternalWorkflows.Insert(oldIndex, workflowDto);
                    return true;
                }
            }

            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured connecting SQL Server.");
            }
            finally
            {
            }
            return false;
        }

        public async Task<bool> AddWorkflow(WorkflowDto workflowDto)
        {
            try
            {
                using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                {
                    context.Workflows.Add(workflowDto);
                    await context.SaveChangesAsync();
                    InternalWorkflows.Add(workflowDto);
                    return true;
                }
            }

            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured connecting SQL Server.");
            }
            finally
            {
            }
            return false;
        }

        public async void Init()
        {
            if (isInitted)
            {
                return;
            }
            await semaphoreSlim.WaitAsync();
            try
            {
                await GetAll();
                isInitted = true;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public Repo()
        {
            DatabaseHelper.Instance.PropertyChanged += DatabaseHelper_PropertyChanged;
        }

        private async void DatabaseHelper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await GetAll();
        }

        private async Task GetAll()
        {
            await GetSources();
            await GetWorkflows();
        }
        private async Task GetSources()
        {
            if (DatabaseHelper.Instance.Status == DatabaseHelperStatusEnum.CelsusDatabaseVersionOk)
            {
                await semaphoreGetAll.WaitAsync();
                try
                {
                    using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                    {
                        InternalSources = new ObservableCollection<SourceDto>(await context.Sources.ToListAsync());
                    }
                }

                catch (Exception ex)
                {
                    logger.Error(ex, $"Error occured connecting SQL Server.");
                }
                finally
                {
                    semaphoreGetAll.Release();
                }
            }
        }

        private async Task GetWorkflows()
        {
            if (DatabaseHelper.Instance.Status == DatabaseHelperStatusEnum.CelsusDatabaseVersionOk)
            {
                await semaphoreGetAll.WaitAsync();
                try
                {
                    using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                    {
                        InternalWorkflows = new ObservableCollection<WorkflowDto>(await context.Workflows.ToListAsync());
                    }
                }

                catch (Exception ex)
                {
                    logger.Error(ex, $"Error occured connecting SQL Server.");
                }
                finally
                {
                    semaphoreGetAll.Release();
                }
            }
        }

        public async Task<bool> AddSource(SourceDto sourceDto)
        {
            try
            {
                using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                {
                    context.Sources.Add(sourceDto);
                    await context.SaveChangesAsync();
                    InternalSources.Add(sourceDto);
                    return true;
                }
            }

            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured connecting SQL Server.");
            }
            finally
            {
            }
            return false;
        }

        public async Task<bool> UpdateSource(SourceDto newSourceDto)
        {
            try
            {
                using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                {
                    context.Entry(newSourceDto).State = EntityState.Modified;
                    var saveResult = await context.SaveChangesAsync();
                    var old = InternalSources.SingleOrDefault(x => x.Id == newSourceDto.Id);
                    var oldIndex = InternalSources.IndexOf(old);
                    InternalSources.RemoveAt(oldIndex);
                    InternalSources.Insert(oldIndex, newSourceDto);
                    return true;
                }
            }

            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured connecting SQL Server.");
            }
            finally
            {
            }
            return false;
        }
    }
}
