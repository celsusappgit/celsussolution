using Celsus.DataLayer;
using Celsus.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types.Workflow
{
    [Workson("PDF")]
    [LocKey("PdfToText")]
    public class PdfToTextWorkflow : CodeWorkflowBase, ICodeWorkflow
    {
        
        public List<MyArgument> GetArgumentsList()
        {
            return new List<MyArgument>()
            {
                new MyArgument() { Name="ArgFileSystemId", ArgumentType = typeof(int), IsOptional = false },
                new MyArgument() { Name="CreateSearchablePDF", ArgumentType = typeof(bool?), IsOptional = true }
            };
        }

        public List<MyOption> GetOptionsList()
        {
            return new List<MyOption>()
            {
                new MyOption() { Name="OptCreateSearchablePDF", OptionType = typeof(bool?), IsOptional = true }
            };
        }

        public void SetArguments(List<MyArgument> arguments)
        {
            this.arguments = arguments;
        }

        public void SetOptions(List<MyOption> options)
        {
            this.options = options;
        }

        
        public async Task<bool> Run()
        {


            var imageMagickPath = SettingsHelper.Instance.ImageMagickPath;
            var tesseractPath = SettingsHelper.Instance.TesseractPath;
            var xPdfToolsPath = SettingsHelper.Instance.XPdfToolsPath;
            var connectionString = SettingsHelper.Instance.ConnectionString;

            var argFileIdValue = GetArgument<int>("ArgFileSystemId");
            var optCreateSearchablePDFValue = GetOption<bool?>("OptCreateSearchablePDF");
            var varSourceIdValue = GetVariable<int>("VarSourceId");

            FileSystemItemDto fileSystemItem = null;

            try
            {
                using (var sqlDbContext = new SqlDbContext(connectionString))
                {
                    fileSystemItem = await sqlDbContext.FileSystemItems.FirstOrDefaultAsync(x => x.SourceId == varSourceIdValue && x.Id == argFileIdValue);
                }
            }
            catch (Exception ex)
            {
                //LogHelper.AddSessionLog(SessionLogTypeEnum.ActivityError, sessionId, $"Exception has been thrown when getting fileSystemItem. FileSystemItemId: {fileSystemItemId}", ex);
                return false;
            }

            //if (fileSystemItem.FullPath.IndexOf("Kurul") >= 0)
            //{
            //    var f = 1;
            //}

            var dummyTextFile = FileHelper.GetUnusedFileName(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");

            var inputPdfFile = fileSystemItem.FullPath;

            if (fileSystemItem.FullPath.Any(c => c > 255))
            {
                var dummyInputFile = FileHelper.GetUnusedFileName(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
                File.Copy(fileSystemItem.FullPath, dummyInputFile);
                inputPdfFile = dummyInputFile;
            }


            try
            {

                List<string> errorDatas = new List<string>();
                List<string> outputDatas = new List<string>();

                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(xPdfToolsPath, @"xpdf-tools-win-4.00\bin64\pdftotext.exe"),
                    Arguments = $"-eol dos -enc UTF-8 \"{inputPdfFile}\" \"{dummyTextFile}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,

                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        errorDatas.Add(e.Data);
                    }
                };
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        outputDatas.Add(e.Data);
                    }
                };
                process.EnableRaisingEvents = true;
                process.StartInfo = processStartInfo;
                var cmd = processStartInfo.FileName + " " + processStartInfo.Arguments;
                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    return false;
                }

                if (errorDatas.Count > 0)
                {
                    Debug.WriteLine("TTTT " + string.Join(",", errorDatas.ToArray()));
                    return false;
                }
                if (outputDatas.Count > 0)
                {
                    Debug.WriteLine("OOOO " + string.Join(",", outputDatas.ToArray()));
                }
            }
            catch (Exception ex)
            {
                //LogHelper.AddFileSystemItemLog(fileSystemItemId, fileSystemItem.SourceId, sessionId, FileSystemItemLogTypeEnum.StoppedWithError, $"Exception has been thrown in OCR operation. FileSystemItemId: {fileSystemItemId}", ex);
                return false;
            }

            string textData = string.Empty;
            if (File.Exists(dummyTextFile))
            {
                textData = File.ReadAllText(dummyTextFile);
            }

            if (string.IsNullOrWhiteSpace(textData) == false && textData.Length > 10)
            {
                // zaten searchable pdf
                var clearTextDto = new ClearTextDto
                {
                    FileSystemItemId = fileSystemItem.Id,
                    TextInFile = textData,
                };
                try
                {
                    using (var sqlDbContext = new SqlDbContext(connectionString))
                    {
                        sqlDbContext.ClearTexts.Add(clearTextDto);
                        await sqlDbContext.SaveChangesAsync();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    //LogHelper.AddSessionLog(SessionLogTypeEnum.ActivityError, sessionId, $"Exception has been thrown when getting fileSystemItem. FileSystemItemId: {fileSystemItemId}", ex);
                    return false;
                }

            }

            var dummyTiffFile = FileHelper.GetUnusedFileName(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tiff");



            try
            {

                List<string> errorDatas = new List<string>();
                List<string> outputDatas = new List<string>();

                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(imageMagickPath, @"convert.exe"),
                    Arguments = $"-density 300 \"{fileSystemItem.FullPath}\" -depth 8 -strip -background white -alpha off \"{dummyTiffFile}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,

                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        errorDatas.Add(e.Data);
                    }
                };
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        outputDatas.Add(e.Data);
                    }
                };
                process.EnableRaisingEvents = true;
                process.StartInfo = processStartInfo;
                var cmd = processStartInfo.FileName + " " + processStartInfo.Arguments;
                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    //logger.Error(ex, $"Process error convert.");
                    return false;
                }

                if (errorDatas.Count > 0)
                {
                    Debug.WriteLine("TTTT " + string.Join(",", errorDatas.ToArray()));
                    
                }
                if (outputDatas.Count > 0)
                {
                    Debug.WriteLine("OOOO " + string.Join(",", outputDatas.ToArray()));
                }

            }
            catch (Exception ex)
            {
                //LogHelper.AddFileSystemItemLog(fileSystemItemId, fileSystemItem.SourceId, sessionId, FileSystemItemLogTypeEnum.StoppedWithError, $"Exception has been thrown in OCR operation. FileSystemItemId: {fileSystemItemId}", ex);
                return false;
            }

            var tessData = Path.Combine(tesseractPath, "tessdata");
            var targetFilePathForFile = FileHelper.GetUnusedFileName(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {

                List<string> errorDatas = new List<string>();
                List<string> outputDatas = new List<string>();

                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(tesseractPath, @"tesseract.exe"),
                    Arguments = $"\"{dummyTiffFile}\" \"{targetFilePathForFile}\" --tessdata-dir \"{tessData}\" -l tur",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,

                };
                if (optCreateSearchablePDFValue.HasValue && optCreateSearchablePDFValue.Value == true)
                {
                    processStartInfo.Arguments = $"\"{dummyTiffFile}\" \"{targetFilePathForFile}\" --tessdata-dir \"{tessData}\" -l tur PDF";
                }
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        errorDatas.Add(e.Data);
                    }
                };
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        outputDatas.Add(e.Data);
                    }
                };
                process.EnableRaisingEvents = true;
                process.StartInfo = processStartInfo;
                var cmd = processStartInfo.FileName + " " + processStartInfo.Arguments;
                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    //logger.Error(ex, $"Process error tesseract.");
                    return false;
                }

                if (errorDatas.Count > 0)
                {
                    Debug.WriteLine("TTTT " + string.Join(",", errorDatas.ToArray()));
                }
                if (outputDatas.Count > 0)
                {
                    Debug.WriteLine("OOOO " + string.Join(",", outputDatas.ToArray()));
                }

            }
            catch (Exception ex)
            {
                //LogHelper.AddFileSystemItemLog(fileSystemItemId, fileSystemItem.SourceId, sessionId, FileSystemItemLogTypeEnum.StoppedWithError, $"Exception has been thrown in OCR operation. FileSystemItemId: {fileSystemItemId}", ex);
                return false;
            }

            if (optCreateSearchablePDFValue.HasValue && optCreateSearchablePDFValue.Value == true)
            {

            }

            return true;
        }

        
    }
}
