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

    [Designer(typeof(TessaractDesigner)), DisplayName("Tessaract")]
    public class Tessaract : NativeActivity<bool>
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        System.Collections.ObjectModel.Collection<Variable> variables = null;

        [RequiredArgument]
        [Category("Input")]
        [DefaultValue(null)]
        public InArgument<String> TargetFilePathForTextFile { get; set; }

        [RequiredArgument]
        [Category("Input")]
        [DefaultValue(null)]
        public InArgument<String> TargetFilePathForPDFFile { get; set; }


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

            var targetFile = TargetFilePathForTextFile.Get(context);
            int counter = 0;
            while (true)
            {
                counter++;
                if (File.Exists(targetFile))
                {
                    var directory = Path.GetDirectoryName(targetFile);
                    var fileName = Path.GetFileNameWithoutExtension(targetFile);
                    var extension = Path.GetExtension(targetFile);
                    targetFile = Path.Combine(directory, fileName + counter + extension);
                }
                else
                {
                    break;
                }
            }


            try
            {
                TempData.Instance.TempPath = Path.GetTempPath();
                using (PDFDoc doc = PDFDoc.Open(fileSystemItem.FullPath))
                {
                    if (doc.GetText() == string.Empty)
                    {
                        doc.Ocr(OcrMode.Tesseract, "tur", WriteTextMode.Word);
                        doc.Save(TargetFilePathForPDFFile.Get(context));
                        var ocrText = doc.GetText();
                        File.WriteAllText(TargetFilePathForTextFile.Get(context), ocrText);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.AddFileSystemItemLog(fileSystemItemId, fileSystemItem.SourceId, sessionId, FileSystemItemLogTypeEnum.StoppedWithError, $"Exception has been thrown in OCR operation. FileSystemItemId: {fileSystemItemId}", ex);
                return;
            }


            LogHelper.AddFileSystemItemLog(fileSystemItem.Id, fileSystemItem.SourceId, sessionId, FileSystemItemLogTypeEnum.TesseractOcrOk);


            Result.Set(context, true);

        }

    }
}
