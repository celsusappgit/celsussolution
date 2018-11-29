using Celsus.DataLayer;
using Celsus.Types;
using Hangfire;
using NLog;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Celsus.Worker
{
    public class Service
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        int sourceId = 0;
        SourceDto source = null;
        List<FileTypeDto> fileTypes = null;
        string sessionId = string.Empty;

        int ingoredItemBecauseItsDone = 0;
        int ingoredItemBecauseItsRegexDontMatch = 0;
        int processedItem = 0;

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public void Start(string sourceIdString)
        {
            sessionId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            logger.Info("Start");

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                AddGeneralLog(GeneralLogTypeEnum.SourceError, $"SessionId is null.");
                logger.Error("sessionId is null");
                return;
            }

            if (string.IsNullOrWhiteSpace(sourceIdString))
            {
                AddSessionLog(SessionLogTypeEnum.SourceError, $"sourceIdString is null.");
                logger.Error("sourceIdString is null");
                return;
            }
            if (int.TryParse(sourceIdString, out sourceId) == false)
            {
                AddSessionLog(SessionLogTypeEnum.SourceError, $"sourceIdString is not integer. sourceIdString: {sourceIdString}");
                logger.Error($"sourceIdString is not integer. sourceIdString: {sourceIdString}");
                return;
            }

            AddSourceLog(SourceLogTypeEnum.Start);

            try
            {
                using (var context = new SqlDbContext())
                {
                    source = context.Sources.SingleOrDefault(x => x.Id == sourceId);
                    fileTypes = context.FileTypes.Where(x => x.SourceId == sourceId && x.IsActive == true).ToList();
                }
            }
            catch (Exception ex)
            {
                AddSessionLog(SessionLogTypeEnum.SourceError, $"Exception has been thrown when getting source and filetype information. SourceId: {sourceId}", ex);
                logger.Error(ex, $"Exception has been thrown when getting source and filetype information.");
                return;
            }

            if (source == null)
            {
                AddSessionLog(SessionLogTypeEnum.SourceError, $"Source is null for Id: {sourceId}");
                return;
            }

            if (string.IsNullOrWhiteSpace(source.Path))
            {
                AddSessionLog(SessionLogTypeEnum.SourceError, $"Directory path is null for source with Id {source.Id}");
                logger.Error($"Directory path is null for source with Id {source.Id}");
                return;
            }

            var directoryExists = false;
            try
            {
                directoryExists = Directory.Exists(source.Path);
            }
            catch (Exception ex)
            {
                AddSessionLog(SessionLogTypeEnum.SourceError, $"Exception has been thrown when checking directory. Path: {source.Path}", ex);
                logger.Error(ex, $"Exception has been thrown when checking directory. Path: {source.Path}");
                return;
            }

            if (directoryExists == false)
            {
                AddSessionLog(SessionLogTypeEnum.SourceError, $"Directory does not exists for source with Id {source.Id}. Path: {source.Path}");
                logger.Error($"Directory does not exists for source with Id {source.Id}. Path: {source.Path}");
                return;
            }

            if (fileTypes == null || fileTypes.Count == 0)
            {
                AddSessionLog(SessionLogTypeEnum.SourceError, $"There is no file type for source name: {source.Name}, id: {source.Id}");
                return;
            }
            var normalFileTypes = new List<FileTypeDto>();
            foreach (var fileTypeDto in fileTypes)
            {
                if (string.IsNullOrWhiteSpace(fileTypeDto.Regex))
                {
                    logger.Error($"Regex is null for file type with id: {fileTypeDto.Id}");
                    AddSessionLog(SessionLogTypeEnum.SourceError, $"Regex is null for file type with id: {fileTypeDto.Id}, source name: {source.Name}, id: {source.Id}");
                    break;
                }
                if (fileTypeDto.Workflow == null || fileTypeDto.Workflow.Length == 0)
                {
                    logger.Error($"Workflow is null for file type with id: {fileTypeDto.Id}");
                    AddSessionLog(SessionLogTypeEnum.SourceError, $"Workflow is null for file type with id: {fileTypeDto.Id}, source name: {source.Name}, id: {source.Id}");
                    break;
                }
                Regex reg = null;
                try
                {
                    reg = new Regex(fileTypeDto.Regex);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Exception has been thrown when creating regex. Path: {fileTypeDto.Regex}");
                    AddSessionLog(SessionLogTypeEnum.SourceError, $"Exception has been thrown when creating regex. regex: {fileTypeDto.Regex}, source name: {source.Name}, id: {source.Id}");
                    // Upper error, there is no file system item
                    break;
                }
                normalFileTypes.Add(fileTypeDto);
            }
            fileTypes = normalFileTypes;
            if (fileTypes == null || fileTypes.Count == 0)
            {
                AddSessionLog(SessionLogTypeEnum.SourceError, $"There is no regular file type for source name: {source.Name}, id: {source.Id}");
                return;
            }

            AddSessionLog(SessionLogTypeEnum.Info, $"Working path: {source.Path}");

            ProcessDirectory(source.Path, null);

            //string[] directories = null;
            //try
            //{
            //    directories = Directory.GetDirectories(source.Path);
            //}
            //catch (Exception ex)
            //{
            //    AddSessionLog(SessionLogTypeEnum.SourceError, $"Exception has been thrown when getting directories. Path: {source.Path}", ex);
            //    return;
            //}
            //AddSessionLog(SessionLogTypeEnum.Info, $"{directories.Length} directories found in source.");
            //string[] files = null;
            //try
            //{
            //    files = Directory.GetFiles(source.Path);
            //}
            //catch (Exception ex)
            //{
            //    AddSessionLog(SessionLogTypeEnum.SourceError, $"Exception has been thrown when getting files. Path: {source.Path}");
            //    logger.Error(ex, $"Exception has been thrown when getting files. Path: {source.Path}");
            //    return;
            //}
            //AddSessionLog(SessionLogTypeEnum.Info, $"{files.Length} files found in source.");
            //foreach (var directory in directories)
            //{
            //    CheckDirectory(directory, null);
            //}
            //foreach (var file in files)
            //{
            //    CheckFile(file, null);
            //}
            //AddSourceLog(SourceLogTypeEnum.Info, $"{processedItem} files processed succesfully.");
            //AddSourceLog(SourceLogTypeEnum.Info, $"{ingoredItemBecauseItsDone} files ignored because they are processed before.");
            //AddSourceLog(SourceLogTypeEnum.Info, $"{ingoredItemBecauseItsRegexDontMatch} files ignored because their regex don't match any file type item.");
        }

        private void AddSessionLog(SessionLogTypeEnum sessionLogTypeEnum, string message, Exception exception)
        {
            var sessionLogDto = new SessionLogDto
            {
                SessionId = sessionId,
                LogDate = DateTime.UtcNow,
                Message = message,
                SessionLogTypeEnum = sessionLogTypeEnum
            };
            if (exception != null)
            {
                sessionLogDto.Exception = exception.ToString();
            }
            try
            {
                using (var context = new SqlDbContext())
                {
                    context.SessionLogs.Add(sessionLogDto);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when saving sessionLog.");
                return;
            }
        }

        private void AddSessionLog(SessionLogTypeEnum sourceError, string message)
        {
            AddSessionLog(sourceError, message, null);
        }

        private void AddGeneralLog(GeneralLogTypeEnum generalLogTypeEnum, string message, Exception exception)
        {
            var generalLogDto = new GeneralLogDto
            {
                LogDate = DateTime.UtcNow,
                Message = message,
                GeneralLogTypeEnum = generalLogTypeEnum
            };
            if (exception != null)
            {
                generalLogDto.Exception = exception.ToString();
            }
            try
            {
                using (var context = new SqlDbContext())
                {
                    context.GeneralLogs.Add(generalLogDto);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when saving generalLog.");
                return;
            }
        }

        private void AddGeneralLog(GeneralLogTypeEnum generalLogTypeEnum, string message)
        {
            AddGeneralLog(generalLogTypeEnum, message, null);
        }

        private void AddSourceLog(SourceLogTypeEnum sourceLogTypeEnum, string message)
        {
            AddSourceLog(sourceLogTypeEnum, message, null);
        }

        private void AddSourceLog(SourceLogTypeEnum sourceLogTypeEnum, string message, Exception exception)
        {
            var sourceLogDto = new SourceLogDto
            {
                SourceId = sourceId,
                StartDate = DateTime.UtcNow,
                SourceLogTypeEnum = sourceLogTypeEnum,
                SessionId = sessionId
            };
            if (string.IsNullOrWhiteSpace(message) == false)
            {
                sourceLogDto.Message = message;
            }
            if (exception != null)
            {
                sourceLogDto.Exception = exception.ToString();
            }
            try
            {
                using (var context = new SqlDbContext())
                {
                    context.SourceLogs.Add(sourceLogDto);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when saving sourcelog.");
                return;
            }
        }

        private void AddSourceLog(SourceLogTypeEnum sourceLogTypeEnum)
        {
            AddSourceLog(sourceLogTypeEnum, null, null);
        }

        private void AddFileSystemItemLog(int fileSystemItemId, FileSystemItemLogTypeEnum fileSystemItemLogTypeEnum, string message)
        {
            AddFileSystemItemLog(fileSystemItemId, fileSystemItemLogTypeEnum, message, null);
        }

        private void AddFileSystemItemLog(int fileSystemItemId, FileSystemItemLogTypeEnum fileSystemItemLogTypeEnum, string message, Exception exception)
        {
            var fileSystemItemLogDto = new FileSystemItemLogDto
            {
                SourceId = sourceId,
                StartDate = DateTime.UtcNow,
                FileSystemItemId = fileSystemItemId,
                FileSystemItemLogTypeEnum = fileSystemItemLogTypeEnum,
                SessionId = sessionId
            };
            if (string.IsNullOrWhiteSpace(message) == false)
            {
                fileSystemItemLogDto.Message = message;
            }
            if (exception != null)
            {
                fileSystemItemLogDto.Exception = exception.ToString();
            }
            try
            {
                using (var context = new SqlDbContext())
                {
                    if ((int)fileSystemItemLogTypeEnum > 1000)
                    {
                        var fileSystemItem = context.FileSystemItems.FirstOrDefault(x => x.Id == fileSystemItemId);
                        if (fileSystemItem != null)
                        {
                            fileSystemItem.FileSystemItemStatusEnum = FileSystemItemStatusEnum.StopedWithError;
                            context.Entry(fileSystemItem).State = EntityState.Modified;
                        }
                    }
                    context.FileSystemItemLogs.Add(fileSystemItemLogDto);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when saving fileSystemItemLog.");
                return;
            }
        }

        //public async void CheckFile(string filePath, int? fileSystemItemId)
        //{
        //    FileSystemItemDto fileSystemItem = null;
        //    try
        //    {
        //        using (var context = new SqlDbContext())
        //        {
        //            fileSystemItem = await context.FileSystemItems.FirstOrDefaultAsync(x => x.SourceId == sourceId && x.FullPath == filePath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, $"Exception has been thrown when getting fileSystemItem. SourceId: {sourceId}, Path: {filePath}");
        //        return;
        //    }

        //    if (fileSystemItem != null)
        //    {
        //        if (fileSystemItem.FileSystemItemStatusEnum == FileSystemItemStatusEnum.Done)
        //        {
        //            ingoredItemBecauseItsDone++;
        //        }
        //    }
        //    else
        //    {
        //        ProcessFile(filePath);

        //    }
        //}

        public async void ProcessDirectory(string directoryPath, FileSystemItemDto parent)
        {
            FileSystemItemDto fileSystemItem = null;

            try
            {
                using (var context = new SqlDbContext())
                {
                    fileSystemItem = await context.FileSystemItems.FirstOrDefaultAsync(x => x.SourceId == sourceId && x.FullPath == directoryPath);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when getting fileSystemItem. SourceId: {sourceId}, Path: {directoryPath}");
                return;
            }

            if (fileSystemItem == null)
            {
                fileSystemItem = new FileSystemItemDto
                {
                    FileTypeId = null,
                    FullPath = directoryPath,
                    Name = Path.GetDirectoryName(directoryPath),
                    OperationDate = DateTime.UtcNow,
                    SourceId = sourceId,
                    FileSystemItemStatusEnum = FileSystemItemStatusEnum.Started
                };
                if (parent != null)
                {
                    fileSystemItem.ParentId = parent.Id;
                }
                try
                {
                    using (var context = new SqlDbContext())
                    {
                        context.FileSystemItems.Add(fileSystemItem);
                        var saveResult = context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    // Upper error, there is no file system item
                    logger.Error(ex, $"Exception has been thrown when saving fileSystemItem. SourceId: {sourceId}, Path: {directoryPath}");
                    return;
                }
            }
            else
            {
                if (fileSystemItem.FileSystemItemStatusEnum == FileSystemItemStatusEnum.Done)
                {
                    ingoredItemBecauseItsDone++;
                }
                else
                {
                    try
                    {
                        using (var context = new SqlDbContext())
                        {
                            fileSystemItem.FileSystemItemStatusEnum = FileSystemItemStatusEnum.Started;
                            if (parent != null)
                            {
                                fileSystemItem.ParentId = parent.Id;
                            }
                            context.Entry(fileSystemItem).State = EntityState.Modified;
                            var saveResult = context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Upper error, there is no file system item
                        logger.Error(ex, $"Exception has been thrown when updating fileSystemItem. SourceId: {sourceId}, Path: {directoryPath}");
                        return;
                    }

                    string[] files = null;
                    try
                    {
                        files = Directory.GetFiles(directoryPath);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Exception has been thrown when getting files. Path: {directoryPath}");
                        return;
                    }
                    foreach (var file in files)
                    {
                        ProcessFile(file, fileSystemItem);
                    }

                    string[] directories = null;
                    try
                    {
                        directories = Directory.GetDirectories(directoryPath);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Exception has been thrown when getting directories. Path: {directoryPath}");
                        return;
                    }
                    foreach (var directory in directories)
                    {
                        ProcessDirectory(directory, fileSystemItem);
                    }
                }
            }
        }

        //public void WorkOnDirectory(string directoryPath, int? fileSystemItemId)
        //{

        //    if (fileSystemItemId == null)
        //    {
        //        var newFileSystemItem = new FileSystemItemDto
        //        {
        //            FileTypeId = null,
        //            FullPath = directoryPath,
        //            Name = Path.GetDirectoryName(directoryPath),
        //            OperationDate = DateTime.UtcNow,
        //            SourceId = sourceId,
        //            FileSystemItemStatusEnum = FileSystemItemStatusEnum.Started
        //        };
        //        try
        //        {
        //            using (var context = new SqlDbContext())
        //            {
        //                context.FileSystemItems.Add(newFileSystemItem);
        //                var saveResult = context.SaveChanges();
        //                fileSystemItemId = newFileSystemItem.Id;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Upper error, there is no file system item
        //            logger.Error(ex, $"Exception has been thrown when updating fileSystemItem. SourceId: {sourceId}, Path: {directoryPath}");
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        try
        //        {
        //            using (var context = new SqlDbContext())
        //            {
        //                var fileSystemItem = context.FileSystemItems.FirstOrDefault(x => x.Id == fileSystemItemId);
        //                fileSystemItem.FileSystemItemStatusEnum = FileSystemItemStatusEnum.Started;
        //                context.Entry(fileSystemItem).State = EntityState.Modified;
        //                var saveResult = context.SaveChanges();

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error(ex, $"Exception has been thrown when updating fileSystemItem. SourceId: {sourceId}, Path: {directoryPath}");
        //            AddFileSystemItemLog(fileSystemItemId.Value, FileSystemItemLogTypeEnum.StoppedWithError, $"Exception has been thrown when updating fileSystemItem. SourceId: {sourceId}, Path: {directoryPath}", ex);
        //            return;
        //        }
        //    }

        //    string[] files = null;
        //    try
        //    {
        //        files = Directory.GetFiles(directoryPath);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, $"Exception has been thrown when getting files. Path: {directoryPath}");
        //        return;
        //    }
        //    foreach (var file in files)
        //    {
        //        ProcessFile(file, fileSystemItemId);
        //    }
        //}

        public async void ProcessFile(string filePath, FileSystemItemDto parent)
        {
            foreach (var fileTypeDto in fileTypes)
            {
                if (string.IsNullOrWhiteSpace(fileTypeDto.Regex))
                {
                    logger.Error($"Regex is null for file type with id: {fileTypeDto.Id}");
                    // Upper error, there is no file system item
                    return;
                }
                if (fileTypeDto.Workflow == null || fileTypeDto.Workflow.Length == 0)
                {
                    logger.Error($"Workflow is null for file type with id: {fileTypeDto.Id}");
                    // Upper error, there is no file system item
                    return;
                }
                Regex reg = null;
                try
                {
                    reg = new Regex(fileTypeDto.Regex);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Exception has been thrown when creating regex. Path: {fileTypeDto.Regex}");
                    // Upper error, there is no file system item
                    return;
                }

                FileSystemItemDto fileSystemItem = null;

                try
                {
                    using (var context = new SqlDbContext())
                    {
                        fileSystemItem = await context.FileSystemItems.FirstOrDefaultAsync(x => x.SourceId == sourceId && x.FullPath == filePath);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Exception has been thrown when getting fileSystemItem. SourceId: {sourceId}, Path: {filePath}");
                    return;
                }

                if (fileSystemItem == null)
                {
                    fileSystemItem = new FileSystemItemDto
                    {
                        FileTypeId = null,
                        FullPath = filePath,
                        Name = Path.GetFileName(filePath),
                        OperationDate = DateTime.UtcNow,
                        SourceId = sourceId,
                        FileSystemItemStatusEnum = FileSystemItemStatusEnum.Started
                    };
                    if (parent != null)
                    {
                        fileSystemItem.ParentId = parent.Id;
                    }
                    try
                    {
                        using (var context = new SqlDbContext())
                        {
                            context.FileSystemItems.Add(fileSystemItem);
                            var saveResult = context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Upper error, there is no file system item
                        logger.Error(ex, $"Exception has been thrown when saving fileSystemItem. SourceId: {sourceId}, Path: {filePath}");
                        return;
                    }
                }
                else
                {
                    if (fileSystemItem.FileSystemItemStatusEnum == FileSystemItemStatusEnum.Done)
                    {
                        ingoredItemBecauseItsDone++;
                    }
                    else
                    {
                        try
                        {
                            using (var context = new SqlDbContext())
                            {
                                fileSystemItem.FileSystemItemStatusEnum = FileSystemItemStatusEnum.Started;
                                if (parent != null)
                                {
                                    fileSystemItem.ParentId = parent.Id;
                                }
                                context.Entry(fileSystemItem).State = EntityState.Modified;
                                var saveResult = context.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Upper error, there is no file system item
                            logger.Error(ex, $"Exception has been thrown when updating fileSystemItem. SourceId: {sourceId}, Path: {filePath}");
                            return;
                        }

                        var fileSystemItemId = fileSystemItem.Id;

                        if (reg.IsMatch(filePath))
                        {
                            Activity activity = null;
                            try
                            {
                                using (var memoryStream = new MemoryStream(fileTypeDto.Workflow))
                                {
                                    activity = ActivityXamlServices.Load(memoryStream);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, $"Exception has been thrown when creating activity for file type with Id: {fileTypeDto.Id}");
                                AddFileSystemItemLog(fileSystemItemId, FileSystemItemLogTypeEnum.StoppedWithError, $"Exception has been thrown when creating activity for file type with Id: {fileTypeDto.Id}", ex);
                                return;
                            }
                            if (activity == null)
                            {
                                logger.Error($"Activity is null for file type with id: {fileTypeDto.Id}");
                                AddFileSystemItemLog(fileSystemItemId, FileSystemItemLogTypeEnum.StoppedWithError, $"Activity is null for file type with id: {fileTypeDto.Id}");
                                return;
                            }
                            var syncEvent = new AutoResetEvent(false);
                            WorkflowApplication app = null;
                            Dictionary<string, object> inputsClone = new Dictionary<string, object>
                                    {
                                        { "ArgFileSystemItemId", fileSystemItemId },
                                        { "ArgSessionId", sessionId}
                                    };
                            try
                            {
                                app = new WorkflowApplication(activity, inputsClone);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, $"Exception has been thrown when creating WorkflowApplication for file type with Id: {fileTypeDto.Id}");
                                AddFileSystemItemLog(fileSystemItemId, FileSystemItemLogTypeEnum.StoppedWithError, $"Exception has been thrown when creating WorkflowApplication for file type with Id: {fileTypeDto.Id}", ex);
                                return;
                            }
                            app.Aborted = delegate (WorkflowApplicationAbortedEventArgs e)
                            {
                                logger.Info($"Workflow aborted for file type with id: {fileTypeDto.Id} and path: {filePath}");
                                AddFileSystemItemLog(fileSystemItemId, FileSystemItemLogTypeEnum.StoppedWithError, $"Workflow aborted for file type with id: {fileTypeDto.Id} and path: {filePath}");
                            };
                            app.Completed = delegate (WorkflowApplicationCompletedEventArgs e)
                            {

                                logger.Info($"Workflow completed for file type with id: {fileTypeDto.Id} and path: {filePath} and state: {e.CompletionState}");
                                AddFileSystemItemLog(fileSystemItemId, FileSystemItemLogTypeEnum.Finished, $"Workflow completed for file type with id: {fileTypeDto.Id} and path: {filePath} and state: {e.CompletionState}");
                                if (e.CompletionState == ActivityInstanceState.Closed)
                                {
                                    var outputs = "";
                                    if (e.Outputs != null && e.Outputs.Count() > 0)
                                    {
                                        var keys = string.Join(",", e.Outputs.Select(x => x.Key).ToArray());
                                        var values = string.Join(",", e.Outputs.Where(x => x.Value != null).Select(x => x.Value.ToString()).ToArray());
                                        outputs = "Keys:" + keys + ", Values:" + values;
                                    }
                                    // TODO Output kontrol et ona göre DB update et

                                    processedItem++;


                                }

                                syncEvent.Set();
                            };
                            app.Idle = delegate (WorkflowApplicationIdleEventArgs e)
                            {
                                logger.Info($"Workflow idle for file type with id: {fileTypeDto.Id} and path: {filePath}");
                                var bookmarks = "";
                                if (e.Bookmarks != null && e.Bookmarks.Count() > 0)
                                {
                                    bookmarks = string.Join(",", e.Bookmarks.Select(x => x.BookmarkName).ToArray());
                                }
                                var t = "";
                                t += "Idle";
                            };
                            app.Unloaded = delegate (WorkflowApplicationEventArgs e)
                            {
                                //logger.Info($"Workflow unloaded for file type with id: {fileType.Id} and path: {filePath}");
                                syncEvent.Set();
                            };

                            app.OnUnhandledException = delegate (WorkflowApplicationUnhandledExceptionEventArgs e)
                            {
                                logger.Error(e.UnhandledException, $"Workflow OnUnhandledException for file type with id: {fileTypeDto.Id} and path: {filePath}");
                                AddFileSystemItemLog(fileSystemItemId, FileSystemItemLogTypeEnum.StoppedWithError, $"Workflow OnUnhandledException for file type with id: {fileTypeDto.Id} and path: {filePath}", e.UnhandledException);
                                syncEvent.Set();
                                return new UnhandledExceptionAction();
                            };
                            try
                            {
                                app.Run();
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, $"Exception has been thrown when WorkflowApplication.Run for file type with Id: {fileTypeDto.Id}");
                                AddFileSystemItemLog(fileSystemItemId, FileSystemItemLogTypeEnum.StoppedWithError, $"Exception has been thrown when WorkflowApplication.Run for file type with Id: {fileTypeDto.Id}", ex);
                                return;
                            }
                        }
                        else
                        {
                            ingoredItemBecauseItsRegexDontMatch++;
                        }
                    }
                }




            }
        }

        public void Stop() { }
    }

}
