using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public enum RolesHelperStatusEnum
    {
        CannotGetRoles = 0,
        RolesGetted = 1
    }
    public class RolesHelper : BaseModel<RolesHelper>, MustInit
    {
        #region Members

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private bool isInitted = false;
        private List<ServerRoleDto> allRoles;

        #endregion

        #region Properties

        RolesHelperStatusEnum status;
        public RolesHelperStatusEnum Status
        {
            get
            {
                return status;
            }
            set
            {
                if (Equals(value, status)) return;
                status = value;
                NotifyPropertyChanged(() => Status);
                NotifyPropertyChanged(() => DatabaseRoleCount);
                //NotifyPropertyChanged(() => DatabaseRoleComputerIP);
                NotifyPropertyChanged(() => DatabaseRole);
                //NotifyPropertyChanged(() => DatabaseRoleComputerName);
                NotifyPropertyChanged(() => IndexerRoleCount);
                //NotifyPropertyChanged(() => IndexerRoleComputerIP);
                //NotifyPropertyChanged(() => IndexerRoleComputerName);
                NotifyPropertyChanged(() => IndexerRoles);
                SetupHelper.Instance.Invalidate();
                //RolesHelper.Instance.Invalidate();

            }
        }

        public int? DatabaseRoleCount
        {
            get
            {
                if (Status == RolesHelperStatusEnum.CannotGetRoles)
                {
                    return null;
                }
                if (allRoles == null)
                {
                    return null;
                }
                return allRoles.Count(x => x.ServerRoleEnum == ServerRoleEnum.Database && x.IsActive == true);
            }
        }

        public int? IndexerRoleCount
        {
            get
            {
                if (Status == RolesHelperStatusEnum.CannotGetRoles)
                {
                    return null;
                }
                if (allRoles == null)
                {
                    return null;
                }
                return allRoles.Count(x => x.ServerRoleEnum == ServerRoleEnum.Indexer && x.IsActive == true);
            }
        }

        //public string DatabaseRoleComputerIP
        //{
        //    get
        //    {
        //        if (Status == RolesHelperStatusEnum.CannotGetRoles)
        //        {
        //            return null;
        //        }
        //        if (allRoles == null)
        //        {
        //            return null;
        //        }
        //        var server = allRoles.Find(x => x.ServerRoleEnum == ServerRoleEnum.Database);
        //        if (server == null)
        //        {
        //            return null;
        //        }
        //        return server.ServerIP;
        //    }
        //}

        public ComputerInformationModel DatabaseRole
        {
            get
            {
                if (Status == RolesHelperStatusEnum.CannotGetRoles)
                {
                    return null;
                }
                if (allRoles == null)
                {
                    return null;
                }
                var servers = allRoles.Where(x => x.ServerRoleEnum == ServerRoleEnum.Database && x.IsActive == true).ToList(); ;
                if (servers == null || servers.Count == 0)
                {
                    return null;
                }
                return servers.Select(x => new ComputerInformationModel() { ServerId = x.ServerId, ServerIP = x.ServerIP, ServerName = x.ServerName }).FirstOrDefault();
            }
        }

        //public string DatabaseRoleComputerName
        //{
        //    get
        //    {
        //        if (Status == RolesHelperStatusEnum.CannotGetRoles)
        //        {
        //            return null;
        //        }
        //        if (allRoles == null)
        //        {
        //            return null;
        //        }
        //        var server = allRoles.Find(x => x.ServerRoleEnum == ServerRoleEnum.Database);
        //        if (server == null)
        //        {
        //            return null;
        //        }
        //        return server.ServerName;
        //    }
        //}

        public List<ComputerInformationModel> IndexerRoles
        {
            get
            {
                if (Status == RolesHelperStatusEnum.CannotGetRoles)
                {
                    return null;
                }
                if (allRoles == null)
                {
                    return null;
                }
                var servers = allRoles.Where(x => x.ServerRoleEnum == ServerRoleEnum.Indexer && x.IsActive == true).ToList(); ;
                if (servers == null)
                {
                    return null;
                }
                return servers.Select(x => new ComputerInformationModel() { ServerId = x.ServerId, ServerIP = x.ServerIP, ServerName = x.ServerName }).ToList();
            }
        }

        //public string IndexerRoleComputerIP
        //{
        //    get
        //    {
        //        if (Status == RolesHelperStatusEnum.CannotGetRoles)
        //        {
        //            return null;
        //        }
        //        if (allRoles == null)
        //        {
        //            return null;
        //        }
        //        var server = allRoles.Find(x => x.ServerRoleEnum == ServerRoleEnum.Indexer);
        //        if (server == null)
        //        {
        //            return null;
        //        }
        //        return server.ServerIP;
        //    }
        //}

        public bool IsIndexerRoleThisComputer
        {
            get
            {
                if (IndexerRoles == null)
                {
                    return false;
                }
                if (IndexerRoles.Count(x => string.Compare(x.ServerId, ComputerHelper.Instance.ServerId, true) == 0) > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsDatabaseRoleThisComputer
        {
            get
            {
                if (DatabaseRole==null)
                {
                    return false;
                }
                if (string.Compare(DatabaseRole.ServerId, ComputerHelper.Instance.ServerId, true) == 0)
                {
                    return true;
                }
                return false;
            }
        }

        //public string IndexerRoleComputerName
        //{
        //    get
        //    {
        //        if (Status == RolesHelperStatusEnum.CannotGetRoles)
        //        {
        //            return null;
        //        }
        //        if (allRoles == null)
        //        {
        //            return null;
        //        }
        //        var server = allRoles.Find(x => x.ServerRoleEnum == ServerRoleEnum.Indexer);
        //        if (server == null)
        //        {
        //            return null;
        //        }
        //        return server.ServerName;
        //    }
        //}

        #endregion

        //public RolesHelper()
        //{
        //    DatabaseHelper.Instance.PropertyChanged += Instance_PropertyChanged;
        //}

        //private async void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName== "Status")
        //    {
        //        await GetRoles();
        //    }

        //}

        internal async void Invalidate()
        {
            await GetRoles();
        }

        public async Task<bool> AddServerRole(ServerRoleEnum serverRoleEnum)
        {
            if (DatabaseHelper.Instance.Status == DatabaseHelperStatusEnum.CelsusDatabaseVersionOk)
            {

            }
            else
            {
                return false;
            }
            var serverRoleDto = new ServerRoleDto
            {
                ServerId = ComputerHelper.Instance.ServerId,
                ServerIP = ComputerHelper.Instance.IPAddress,
                ServerName = Environment.MachineName,
                ServerRoleEnum = serverRoleEnum,
                IsActive = true
            };

            try
            {
                using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                {
                    var oldRoles = await context.ServerRoles.Where(x => x.ServerRoleEnum == serverRoleEnum && x.IsActive == true).ToListAsync();
                    if (oldRoles.Count == 0)
                    {
                        context.ServerRoles.Add(serverRoleDto);
                        await context.SaveChangesAsync();
                    }
                    else if (oldRoles.Count == 1)
                    {
                        oldRoles.First().IsActive = false;
                        context.Entry(oldRoles.First()).State = System.Data.Entity.EntityState.Modified;

                        context.ServerRoles.Add(serverRoleDto);

                        await context.SaveChangesAsync();
                    }
                    else if (oldRoles.Count > 1)
                    {
                        var toDelete = oldRoles.Take(oldRoles.Count - 1).ToList();
                        foreach (var item in toDelete)
                        {
                            item.IsActive = false;
                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        }

                        oldRoles.Last().ServerId = serverRoleDto.ServerId;
                        oldRoles.Last().ServerIP = serverRoleDto.ServerIP;
                        oldRoles.Last().ServerName = serverRoleDto.ServerName;
                        oldRoles.Last().ServerRoleEnum = serverRoleDto.ServerRoleEnum;
                        context.Entry(oldRoles.Last()).State = System.Data.Entity.EntityState.Modified;

                        await context.SaveChangesAsync();
                    }

                    await GetRoles();
                    NotifyPropertyChanged("");

                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured EnsureDatabase.");
            }

            return false;
        }

        private async Task GetRoles()
        {
            if (DatabaseHelper.Instance.Status == DatabaseHelperStatusEnum.CelsusDatabaseVersionOk)
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                    {
                        allRoles = await context.ServerRoles.Where(x => x.IsActive == true).ToListAsync();
                    }
                    Status = RolesHelperStatusEnum.RolesGetted;
                }

                catch (Exception ex)
                {
                    Status = RolesHelperStatusEnum.CannotGetRoles;
                    logger.Error(ex, $"Error occured connecting SQL Server.");
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
        }

        public async void Init()
        {
            if (isInitted)
            {
                return;
            }
            await GetRoles();
            isInitted = true;
        }
    }
}
