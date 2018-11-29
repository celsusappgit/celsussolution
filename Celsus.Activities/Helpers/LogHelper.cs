using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Activities.Helpers
{
    public class LogHelper
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void AddGeneralLog(GeneralLogTypeEnum generalLogTypeEnum, string message)
        {
            AddGeneralLog(generalLogTypeEnum, message, null);
        }

        public static void AddGeneralLog(GeneralLogTypeEnum generalLogTypeEnum, string message, Exception exception)
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

        public static void AddSessionLog(SessionLogTypeEnum sessionLogTypeEnum, string sessionId, string message, Exception exception)
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

        

        public static void AddSessionLog(SessionLogTypeEnum sourceError, string sessionId, string message)
        {
            AddSessionLog(sourceError, sessionId, message, null);
        }

        public static void AddFileSystemItemLog(int fileSystemItemId, int sourceId, string sessionId, FileSystemItemLogTypeEnum fileSystemItemLogTypeEnum)
        {
            AddFileSystemItemLog(fileSystemItemId, sourceId, sessionId, fileSystemItemLogTypeEnum, null, null);
        }
        public static void AddFileSystemItemLog(int fileSystemItemId, int sourceId, string sessionId, FileSystemItemLogTypeEnum fileSystemItemLogTypeEnum, string message)
        {
            AddFileSystemItemLog(fileSystemItemId, sourceId, sessionId, fileSystemItemLogTypeEnum, message, null);
        }

        public static void AddFileSystemItemLog(int fileSystemItemId,int sourceId, string sessionId, FileSystemItemLogTypeEnum fileSystemItemLogTypeEnum, string message, Exception exception)
        {
            var fileSystemItemLogDto = new FileSystemItemLogDto
            {
                SourceId = sourceId,
                StartDate = DateTime.UtcNow,
                FileSystemItemId = fileSystemItemId,
                FileSystemItemLogTypeEnum = fileSystemItemLogTypeEnum,
                SessionId = sessionId,
                
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
    }
}
