using Celsus.Activities.Helpers;
using Celsus.DataLayer;
using Celsus.Types;
using Celsus.Types.NonDatabase;
using Clock.Pdf;
using Clock.Utils;
using System;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Activities.Validation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
namespace Celsus.Activities
{
    [Designer(typeof(TesseractExeDesigner)), DisplayName("TesseractExe")]
    public class TesseractExe : NativeActivity<bool>
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

            var targetFilePathForTextFile = TargetFilePathForTextFile.Get(context);
            var targetFilePathForPDFFile = TargetFilePathForPDFFile.Get(context);

            targetFilePathForTextFile = FileHelper.GetUnusedFileName(Path.GetDirectoryName(targetFilePathForTextFile), Path.GetFileName(targetFilePathForTextFile));

            var dummyTiffFile = FileHelper.GetUnusedFileName(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tiff");

            // C:\Users\efeo\Downloads\ImageMagick-7.0.8-12-portable-Q16-x64\convert.exe -density 300 c:\AkisLog\A.pdf -depth 8 -strip -background white -alpha off c:\AkisLog\a.tiff
            try
            {
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Users\efeo\Downloads\ImageMagick-7.0.8-12-portable-Q16-x64\convert.exe",
                    Arguments = $"-density 300 {fileSystemItem.FullPath} -depth 8 -strip -background white -alpha off {dummyTiffFile}"
                };
                process.StartInfo = processStartInfo;
                try
                {
                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Process error convert.");
                    return;
                }

            }
            catch (Exception ex)
            {
                LogHelper.AddFileSystemItemLog(fileSystemItemId, fileSystemItem.SourceId, sessionId, FileSystemItemLogTypeEnum.StoppedWithError, $"Exception has been thrown in OCR operation. FileSystemItemId: {fileSystemItemId}", ex);
                return;
            }

            var canOsman = false;
            if (File.Exists(dummyTiffFile))
            {
                try
                {
                    var fileInfo = new FileInfo(dummyTiffFile);
                    if (fileInfo.Length > 0)
                    {
                        canOsman = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Check file error.");
                    return;
                }
            }

            if (canOsman == false)
            {

            }

            var tessData = "";

            // "C:\Program Files (x86)\Tesseract-OCR\tesseract.exe" "c:\AkisLog\c.tiff" "c:\AkisLog\c" --tessdata-dir "C:\Program Files (x86)\Tesseract-OCR\tessdata" -l tur pdf
            try
            {
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe",
                    Arguments = $"\"{dummyTiffFile}\" \"{targetFilePathForPDFFile}\" --tessdata-dir \"{tessData}\" -l tur pdf"
                };
                process.StartInfo = processStartInfo;
                try
                {
                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Process error tesseract.");
                    return;
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
