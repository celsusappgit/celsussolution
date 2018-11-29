using Celsus.Client.Shared.Types;
using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celsus.Service
{
    internal class BootStrap
    {
        public List<SessionLogDto> sessionLogs = new List<SessionLogDto>();
        public List<SourceProcessor> SourceProcessors { get; private set; }

        public BootStrap()
        {
            SourceProcessors = new List<SourceProcessor>();
        }
        public void Start()
        {
            var sessionId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            SettingsHelper.Instance.Init();

            List<SourceDto> sources = null;

            try
            {
                using (var context = new SqlDbContext(SettingsHelper.Instance.ConnectionString))
                {
                    sources = context.Sources.Where(x => x.IsActive == true).ToList();
                }
            }
            catch (Exception ex)
            {
                AddSessionLog(new SessionLogDto() { Exception = ex.ToString(), Message = "Cannot get Sources from database. Ref:1", SessionId = sessionId, LogDate = DateTime.UtcNow, SessionLogTypeEnum = SessionLogTypeEnum.Error });
                return;
            }

            if (sources.Count() == 0)
            {
                AddSessionLog(new SessionLogDto() { Message = "No Sources defined in database.", SessionId = sessionId, LogDate = DateTime.UtcNow, SessionLogTypeEnum = SessionLogTypeEnum.Warning });
                return;
            }


            AddSessionLog(new SessionLogDto() { Message = $"Sources to process ({sources.Count}): {string.Join(",", sources.Select(x => x.Name).ToArray())}.", SessionId = sessionId, LogDate = DateTime.UtcNow, SessionLogTypeEnum = SessionLogTypeEnum.Info });

            foreach (var item in sources)
            {
                var sourceProcessor = new SourceProcessor(item.Id, sessionId);
                SourceProcessors.Add(sourceProcessor);
            }

            foreach (var sourceProcessor in SourceProcessors)
            {
                sourceProcessor.Start();
            }
        }

        private async void AddSessionLog(SessionLogDto sessionLogDto)
        {
            var connectionString = SettingsHelper.Instance.ConnectionString;

            sessionLogs.Add(sessionLogDto);
            try
            {
                using (var sqlDbContext = new SqlDbContext(connectionString))
                {
                    sqlDbContext.SessionLogs.Add(sessionLogDto);
                    await sqlDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void Stop()
        {

        }
    }
}