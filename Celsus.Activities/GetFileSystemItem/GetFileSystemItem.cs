using Celsus.Activities.Helpers;
using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Activities.Validation;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Celsus.Activities
{
    [Designer(typeof(GetFileSystemItemDesigner)), DisplayName("Get File System Item")]
    public class GetFileSystemItem : NativeActivity<FileSystemItemDto>
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        System.Collections.ObjectModel.Collection<Variable> variables = null;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            variables = metadata.GetVariablesWithReflection();
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return false;
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            WorkflowDataContext dataContext = context.DataContext;
            PropertyDescriptorCollection propertyDescriptorCollection = dataContext.GetProperties();
            string sessionId = string.Empty;
            int fileSystemItemId = 0;
            foreach (PropertyDescriptor propertyDesc in propertyDescriptorCollection)
            {
                if (propertyDesc.Name == "ArgSessionId")
                {
                    sessionId = propertyDesc.GetValue(dataContext) as string;
                    break;
                }
            }
            foreach (PropertyDescriptor propertyDesc in propertyDescriptorCollection)
            {
                if (propertyDesc.Name == "ArgFileSystemItemId")
                {
                    fileSystemItemId = (int)propertyDesc.GetValue(dataContext);
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                LogHelper.AddGeneralLog(GeneralLogTypeEnum.ActivityError, $"SessionId is null.");
                return;
            }

            if (fileSystemItemId == 0)
            {
                LogHelper.AddSessionLog(SessionLogTypeEnum.ActivityError, sessionId, $"FileSystemItemId is null.");
                return;
            }

            FileSystemItemDto fileSystemItem = null;

            try
            {
                using (var sqlDbContext = new SqlDbContext())
                {
                    fileSystemItem = sqlDbContext.FileSystemItems.FirstOrDefault(x => x.Id == fileSystemItemId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.AddSessionLog(SessionLogTypeEnum.ActivityError, sessionId, $"Exception has been thrown when getting fileSystemItem. FileSystemItemId: {fileSystemItemId}", ex);
                return;
            }


            Result.Set(context, fileSystemItem);

        }
    }
}
