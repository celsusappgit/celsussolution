using Celsus.Activities.Helpers;
using Celsus.DataLayer;
using Celsus.Types;
using Clock.Pdf;
using Clock.Utils;
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
    [Designer(typeof(SaveTextDataDesigner)), DisplayName("SaveTextData")]
    public class SaveTextData : NativeActivity<bool>
    {
        System.Collections.ObjectModel.Collection<Variable> variables = null;
        public InArgument<String> SourceFilePath { get; set; }

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

            if (File.Exists(SourceFilePath.Get(context)) == false)
            {
                LogHelper.AddFileSystemItemLog(fileSystemItem.Id, fileSystemItem.SourceId, sessionId, FileSystemItemLogTypeEnum.SaveTextDataFileNotFound, $"File path: {SourceFilePath.Get(context)}.");
                return;
            }
            var content = File.ReadAllText(SourceFilePath.Get(context));

            try
            {
                using (var sqlDbContext = new SqlDbContext())
                {

                    var saveResult1 = sqlDbContext.SaveChanges();

                    var oldItemCount = sqlDbContext.ClearTexts.Count(x => x.FileSystemItemId == fileSystemItemId);
                    if (oldItemCount == 0)
                    {
                        var clearTextDto = new ClearTextDto
                        {
                            FileSystemItemId = fileSystemItemId,
                            TextInFile = content,
                        };
                        sqlDbContext.ClearTexts.Add(clearTextDto);
                        var saveResult2 = sqlDbContext.SaveChanges();
                    }
                    else
                    {
                        var oldClearText = sqlDbContext.ClearTexts.FirstOrDefault(x => x.FileSystemItemId == fileSystemItemId);
                        var sql = "UPDATE ClearText SET TextInFile = @TextInFile WHERE Id = @Id";
                        var executeCount = sqlDbContext.Database.ExecuteSqlCommand(sql, content, oldClearText.Id);
                    }
                }
                LogHelper.AddFileSystemItemLog(fileSystemItem.Id, fileSystemItem.SourceId, sessionId, FileSystemItemLogTypeEnum.SaveTextDataOk);
            }
            catch (Exception ex)
            {
                LogHelper.AddSessionLog(SessionLogTypeEnum.ActivityError, sessionId, $"Exception has been thrown when getting fileSystemItem. FileSystemItemId: {fileSystemItemId}", ex);
                return;
            }
        }
    }
}
