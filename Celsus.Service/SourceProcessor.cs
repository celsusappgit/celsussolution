using Celsus.Client.Shared.Types;
using Celsus.Client.Shared.Types.Workflow;
using Celsus.DataLayer;
using Celsus.Types;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Service
{
    public class SourceProcessor
    {
        private SourceDto source;
        private MyDirectory rootDirectory;
        private List<WorkflowDto> workflows;
        private readonly int sourceId;
        List<MyVariable> globalVariables = new List<MyVariable>();

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SourceProcessor(int id, string sessionId)
        {
            sourceId = id;
            SessionId = sessionId;
        }

        public string SessionId { get; private set; }

        public List<SourceLogDto> sourceLogs = new List<SourceLogDto>();
        private List<FileSystemItemLogDto> fileSystemItemLogs = new List<FileSystemItemLogDto>();

        internal async Task Start()
        {
            IncrementSourceWorkCount();

            try
            {
                using (var context = new SqlDbContext(SettingsHelper.Instance.ConnectionString))
                {
                    source = context.Sources.SingleOrDefault(x => x.Id == sourceId);
                }
            }
            catch (Exception ex)
            {
                AddSourceLog(new SourceLogDto() { Exception = ex.ToString(), Message = "Cannot get source from database", SessionId = SessionId, SourceId = sourceId, SourceLogTypeEnum = SourceLogTypeEnum.StoppedWithError, StartDate = DateTime.UtcNow });
                return;
            }
            try
            {
                using (var context = new SqlDbContext(SettingsHelper.Instance.ConnectionString))
                {
                    workflows = await context.Workflows.Where(x => x.SourceId == sourceId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                AddSourceLog(new SourceLogDto() { Exception = ex.ToString(), Message = "Cannot get workflows for source from database", SessionId = SessionId, SourceId = sourceId, SourceLogTypeEnum = SourceLogTypeEnum.StoppedWithError, StartDate = DateTime.UtcNow });
                return;
            }

            globalVariables = new List<MyVariable>()
                    {
                        new MyVariable() { Name="VarSessionId", VariableType = typeof(string), IsOptional = false, JSonValue=JsonConvert.SerializeObject(SessionId) },
                        new MyVariable() { Name="VarSourceId", VariableType = typeof(int), IsOptional = false,JSonValue = JsonConvert.SerializeObject(sourceId) },
                    };

            rootDirectory = GetDirectory(source.Path, null);

            await ProcessDirectory(rootDirectory, null);

            AddSourceLog(new SourceLogDto() { Message = "Source init completed", SessionId = SessionId, SourceId = sourceId, SourceLogTypeEnum = SourceLogTypeEnum.Info, StartDate = DateTime.UtcNow });

            var fileSystemWatcher = new FileSystemWatcher();
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.Path = source.Path;
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created);
        }

        private async void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            FileAttributes attr = File.GetAttributes(e.FullPath);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                return;
            }
            else
            {

            }

            var connectionString = SettingsHelper.Instance.ConnectionString;

            FileSystemItemDto fileSystemItemForDirectory = null;
            FileSystemItemDto fileSystemItemForParentDirectory = null;

            var directoryName = Path.GetDirectoryName(e.FullPath);
            try
            {
                using (var sqlDbContext = new SqlDbContext(connectionString))
                {
                    fileSystemItemForDirectory = await sqlDbContext.FileSystemItems.FirstOrDefaultAsync(x => x.SourceId == sourceId && x.FullPath == directoryName);
                    if (fileSystemItemForDirectory.ParentId.HasValue)
                    {
                        fileSystemItemForParentDirectory = await sqlDbContext.FileSystemItems.FirstOrDefaultAsync(x => x.Id == fileSystemItemForDirectory.ParentId);
                    }
                }
            }
            catch (Exception ex)
            {
                AddSourceLog(new SourceLogDto() { Exception = ex.ToString(), Message = "Cannot get file system item for source from database", SessionId = SessionId, SourceId = sourceId, SourceLogTypeEnum = SourceLogTypeEnum.StoppedWithError, StartDate = DateTime.UtcNow });
                return;
            }

            var myDirectory = new MyDirectory() { FullPath = directoryName };
            var myFile = new MyFile() { FullPath = e.FullPath, Parent = myDirectory };
            await WorkOnFile(fileSystemItemForDirectory, myFile);
        }

        private void IncrementSourceWorkCount()
        {

        }

        private async Task<bool> ProcessDirectory(MyDirectory directory, int? parentId)
        {
            var connectionString = SettingsHelper.Instance.ConnectionString;

            FileSystemItemDto fileSystemItemForDirectory = null;

            try
            {
                using (var sqlDbContext = new SqlDbContext(connectionString))
                {
                    fileSystemItemForDirectory = await sqlDbContext.FileSystemItems.FirstOrDefaultAsync(x => x.SourceId == sourceId && x.FullPath == directory.FullPath);
                }
            }
            catch (Exception ex)
            {
                AddSourceLog(new SourceLogDto() { Exception = ex.ToString(), Message = "Cannot get file system item for source from database. Ref:1", SessionId = SessionId, SourceId = sourceId, SourceLogTypeEnum = SourceLogTypeEnum.StoppedWithError, StartDate = DateTime.UtcNow });
                return false;
            }

            if (fileSystemItemForDirectory == null)
            {
                fileSystemItemForDirectory = new FileSystemItemDto
                {
                    FileTypeId = null,
                    FullPath = directory.FullPath,
                    Name = Path.GetDirectoryName(directory.FullPath),
                    OperationDate = DateTime.UtcNow,
                    SourceId = sourceId,
                    FileSystemItemStatusEnum = FileSystemItemStatusEnum.Started,
                    ParentId = parentId,
                    IsDirectory = true
                };
                try
                {
                    using (var context = new SqlDbContext(connectionString))
                    {
                        context.FileSystemItems.Add(fileSystemItemForDirectory);
                        var saveResult = context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    AddSourceLog(new SourceLogDto() { Exception = ex.ToString(), Message = "Cannot get save file system item for source to database. Ref:2", SessionId = SessionId, SourceId = sourceId, SourceLogTypeEnum = SourceLogTypeEnum.StoppedWithError, StartDate = DateTime.UtcNow });
                    return false;
                }
            }

            foreach (var myFile in directory.Files)
            {
                await WorkOnFile(fileSystemItemForDirectory, myFile);

                foreach (var subDirectory in directory.Directories)
                {
                    await ProcessDirectory(subDirectory, fileSystemItemForDirectory.Id);
                }
            }

            return true;
        }

        private async Task<bool> WorkOnFile(FileSystemItemDto fileSystemItemForDirectory, MyFile myFile)
        {
            var connectionString = SettingsHelper.Instance.ConnectionString;

            

            FileSystemItemDto fileSystemItem = null;

            try
            {
                using (var sqlDbContext = new SqlDbContext(connectionString))
                {
                    fileSystemItem = await sqlDbContext.FileSystemItems.FirstOrDefaultAsync(x => x.SourceId == sourceId && x.ParentId == fileSystemItemForDirectory.Id && x.FullPath == myFile.FullPath);
                }
            }
            catch (Exception ex)
            {
                AddSourceLog(new SourceLogDto() { Exception = ex.ToString(), Message = "Cannot get file system item for source from database. Ref:2", SessionId = SessionId, SourceId = sourceId, SourceLogTypeEnum = SourceLogTypeEnum.StoppedWithError, StartDate = DateTime.UtcNow });
                return false;
            }

            if (fileSystemItem == null)
            {
                fileSystemItem = new FileSystemItemDto
                {
                    FileTypeId = null,
                    FullPath = myFile.FullPath,
                    Name = Path.GetFileName(myFile.FullPath),
                    OperationDate = DateTime.UtcNow,
                    SourceId = sourceId,
                    FileSystemItemStatusEnum = FileSystemItemStatusEnum.Started,
                    ParentId = fileSystemItemForDirectory.Id,
                    IsDirectory = false
                };
                try
                {
                    using (var context = new SqlDbContext(connectionString))
                    {
                        context.FileSystemItems.Add(fileSystemItem);
                        var saveResult = context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    AddSourceLog(new SourceLogDto() { Exception = ex.ToString(), Message = "Cannot get save file system item for source to database", SessionId = SessionId, SourceId = sourceId, SourceLogTypeEnum = SourceLogTypeEnum.StoppedWithError, StartDate = DateTime.UtcNow });
                    return false;
                }
            }
            else
            {
                if (fileSystemItem.FileSystemItemStatusEnum == FileSystemItemStatusEnum.Done)
                {
                    AddFileSystemItemLog(new FileSystemItemLogDto() { Message = $"File omitted beacuse it is already processed", SessionId = SessionId, StartDate = DateTime.UtcNow, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.Info, FileSystemItemId = fileSystemItem.Id, SourceId = sourceId });
                    return true;
                }
            }

            var workflowsToRun = workflows.Where(x => x.FileType == myFile.FileType).ToList().OrderBy(x => x.OrderNo);

            if (workflowsToRun.Any())
            {
                AddFileSystemItemLog(new FileSystemItemLogDto() { Message = $"Workflows to process ({workflowsToRun.Count()}): {string.Join(",", workflowsToRun.Select(x => x.Name).ToArray())}.", SessionId = SessionId, StartDate = DateTime.UtcNow, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.Info, FileSystemItemId = fileSystemItem.Id, SourceId = sourceId });

                bool hasError = false;
                foreach (var workflow in workflowsToRun)
                {
                    var runResult = await RunWorkflow(workflow, fileSystemItem);
                    if (runResult == false)
                    {
                        hasError = true;
                        //nnn
                        AddFileSystemItemLog(new FileSystemItemLogDto() { Message = $"Workflows processed error {workflow.Name}.", SessionId = SessionId, StartDate = DateTime.UtcNow, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.Error, FileSystemItemId = fileSystemItem.Id, SourceId = sourceId });

                        fileSystemItem.FileSystemItemStatusEnum = FileSystemItemStatusEnum.StopedWithError;

                        try
                        {
                            using (var context = new SqlDbContext(connectionString))
                            {
                                context.Entry(fileSystemItem).State = EntityState.Modified;
                                var saveResult = context.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            AddFileSystemItemLog(new FileSystemItemLogDto() { Exception = ex.ToString(), Message = $"Cannot get update file system item for source to database.", SessionId = SessionId, StartDate = DateTime.UtcNow, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.Info, FileSystemItemId = fileSystemItem.Id, SourceId = sourceId });
                            return false;
                        }

                        break;
                    }
                    else
                    {
                        AddFileSystemItemLog(new FileSystemItemLogDto() { Message = $"Workflows processed {workflow.Name}.", SessionId = SessionId, StartDate = DateTime.UtcNow, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.Info, FileSystemItemId = fileSystemItem.Id, SourceId = sourceId });
                    }
                }
                if (hasError)
                {
                    AddFileSystemItemLog(new FileSystemItemLogDto() { Message = $"Workflows process error.", SessionId = SessionId, StartDate = DateTime.UtcNow, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.StoppedWithError, FileSystemItemId = fileSystemItem.Id, SourceId = sourceId });
                    return false;
                }
                else
                {
                    fileSystemItem.FileSystemItemStatusEnum = FileSystemItemStatusEnum.Done;

                    try
                    {
                        using (var context = new SqlDbContext(connectionString))
                        {
                            context.Entry(fileSystemItem).State = EntityState.Modified;
                            var saveResult = context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        AddFileSystemItemLog(new FileSystemItemLogDto() { Exception = ex.ToString(), Message = $"Cannot get update file system item for source to database.", SessionId = SessionId, StartDate = DateTime.UtcNow, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.Info, FileSystemItemId = fileSystemItem.Id, SourceId = sourceId });
                        return false;
                    }
                }
            }
            else
            {
                AddFileSystemItemLog(new FileSystemItemLogDto() { Message = $"No workflows to process.", SessionId = SessionId, StartDate = DateTime.UtcNow, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.Info, FileSystemItemId = fileSystemItem.Id, SourceId = sourceId });

                fileSystemItem.FileSystemItemStatusEnum = FileSystemItemStatusEnum.Omitted;

                try
                {
                    using (var context = new SqlDbContext(connectionString))
                    {
                        context.Entry(fileSystemItem).State = EntityState.Modified;
                        var saveResult = context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    AddFileSystemItemLog(new FileSystemItemLogDto() { Exception = ex.ToString(), Message = $"Cannot get update file system item for source to database.", SessionId = SessionId, StartDate = DateTime.UtcNow, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.Info, FileSystemItemId = fileSystemItem.Id, SourceId = sourceId });
                    return false;
                }

                return true;
            }

            return true;
        }

        private async void AddSourceLog(SourceLogDto sourceLogDto)
        {
            var connectionString = SettingsHelper.Instance.ConnectionString;

            sourceLogs.Add(sourceLogDto);
            try
            {
                using (var sqlDbContext = new SqlDbContext(connectionString))
                {
                    sqlDbContext.SourceLogs.Add(sourceLogDto);
                    await sqlDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private async Task<bool> RunWorkflow(WorkflowDto workflow, FileSystemItemDto fileSystemItemDto)
        {
            if (string.IsNullOrWhiteSpace(workflow.InternalTypeName) == false)
            {
                Assembly[] assemblies = null;

                try
                {
                    assemblies = AppDomain.CurrentDomain.GetAssemblies();
                }
                catch (Exception ex)
                {
                    AddFileSystemItemLog(new FileSystemItemLogDto() { Message = "Error getting assemblies", Exception = ex.ToString(), FileSystemItemId = fileSystemItemDto.Id, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.StoppedWithError, SessionId = SessionId, SourceId = sourceId, StartDate = DateTime.UtcNow });
                    throw;
                }
                var assembly = assemblies.FirstOrDefault(x => x.FullName.StartsWith("Celsus.Client.Shared"));
                if (assembly == null)
                {
                    AddFileSystemItemLog(new FileSystemItemLogDto() { Message = "Error getting Celsus.Client.Shared assembly", FileSystemItemId = fileSystemItemDto.Id, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.StoppedWithError, SessionId = SessionId, SourceId = sourceId, StartDate = DateTime.UtcNow });
                    throw new Exception();
                }
                Type type = null;
                try
                {
                    type = assembly.GetType(workflow.InternalTypeName);
                }
                catch (Exception ex)
                {
                    AddFileSystemItemLog(new FileSystemItemLogDto() { Message = "Error getting type", Exception = ex.ToString(), FileSystemItemId = fileSystemItemDto.Id, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.StoppedWithError, SessionId = SessionId, SourceId = sourceId, StartDate = DateTime.UtcNow });
                    throw;
                }
                if (type == null)
                {
                    AddFileSystemItemLog(new FileSystemItemLogDto() { Message = $"Cannot get type {workflow.InternalTypeName}", FileSystemItemId = fileSystemItemDto.Id, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.StoppedWithError, SessionId = SessionId, SourceId = sourceId, StartDate = DateTime.UtcNow });
                    throw new Exception();
                }
                ICodeWorkflow workflowToRun = null;
                try
                {
                    workflowToRun = Activator.CreateInstance(type) as ICodeWorkflow;
                }
                catch (Exception ex)
                {
                    AddFileSystemItemLog(new FileSystemItemLogDto() { Message = "Error creating type", Exception = ex.ToString(), FileSystemItemId = fileSystemItemDto.Id, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.StoppedWithError, SessionId = SessionId, SourceId = sourceId, StartDate = DateTime.UtcNow });
                    throw;
                }
                var args = workflowToRun.GetArgumentsList();
                foreach (var arg in args)
                {
                    if (arg.Name == "ArgFileSystemId")
                    {
                        arg.JSonValue = JsonConvert.SerializeObject(fileSystemItemDto.Id);
                    }
                }
                workflowToRun.SetArguments(args);
                workflowToRun.GlobalVariables = globalVariables;
                var runResult = false;
                try
                {
                    runResult = await workflowToRun.Run();
                }
                catch (Exception ex)
                {
                    AddFileSystemItemLog(new FileSystemItemLogDto() { Message = $"Error running workflow {workflow.InternalTypeName}", Exception = ex.ToString(), FileSystemItemId = fileSystemItemDto.Id, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.StoppedWithError, SessionId = SessionId, SourceId = sourceId, StartDate = DateTime.UtcNow });
                    throw;
                }
                if (runResult)
                {
                    //AddFileSystemItemLog(new FileSystemItemLogDto() { Message = $"Workflow completed successfully {workflow.InternalTypeName}", FileSystemItemId = fileSystemItemDto.Id, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.Info, SessionId = SessionId, SourceId = sourceId, StartDate = DateTime.UtcNow });
                    globalVariables = workflowToRun.GlobalVariables;
                }
                else
                {
                    //nnn
                    //AddFileSystemItemLog(new FileSystemItemLogDto() { Message = $"Workflow completed with error {workflow.InternalTypeName}", FileSystemItemId = fileSystemItemDto.Id, FileSystemItemLogTypeEnum = FileSystemItemLogTypeEnum.Error, SessionId = SessionId, SourceId = sourceId, StartDate = DateTime.UtcNow });
                }
                return runResult;
            }
            else
            {
                return true;
            }
        }

        private async void AddFileSystemItemLog(FileSystemItemLogDto fileSystemItemLogDto)
        {
            var connectionString = SettingsHelper.Instance.ConnectionString;

            fileSystemItemLogs.Add(fileSystemItemLogDto);
            try
            {
                using (var sqlDbContext = new SqlDbContext(connectionString))
                {
                    sqlDbContext.FileSystemItemLogs.Add(fileSystemItemLogDto);
                    await sqlDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public MyDirectory GetDirectory(string directoryPath, MyDirectory parentDirectory)
        {
            var myDirectory = new MyDirectory() { FullPath = directoryPath, Parent = parentDirectory };

            string[] files = null;
            try
            {
                files = Directory.GetFiles(directoryPath);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when getting files. Path: {directoryPath}");
                return null;
            }

            myDirectory.Files = files.Select(x => new MyFile() { FullPath = x, Parent = myDirectory }).ToList();

            string[] directories = null;
            try
            {
                directories = Directory.GetDirectories(directoryPath);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when getting directories. Path: {directoryPath}");
                return null;
            }

            myDirectory.Directories = directories.Select(x => new MyDirectory() { FullPath = x, Parent = myDirectory }).ToList();

            foreach (var directory in directories)
            {
                GetDirectory(directory, myDirectory);
            }

            return myDirectory;
        }
    }

    public class MyDirectory
    {
        public MyDirectory Parent { get; set; }
        public string FullPath { get; set; }
        public List<MyFile> Files { get; set; }
        public List<MyDirectory> Directories { get; set; }
    }

    public class MyFile
    {
        public MyDirectory Parent { get; set; }
        public string FullPath { get; set; }
        public string FileType
        {
            get
            {
                return System.IO.Path.GetExtension(FullPath).Replace(".", "").ToUpper();
            }
        }
    }
}
