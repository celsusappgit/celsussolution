using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public class ProcessHelper
    {
        public List<string> ErrorDatas { get; set; }
        public List<string> OutputDatas { get; set; }
        public string FileName { get; set; }
        public string Arguments { get; set; }
        public Exception Exception { get; set; }
        public void RunProcess(bool runAsAdmin = false)
        {
            ErrorDatas = new List<string>();
            OutputDatas = new List<string>();
            Exception = null;

            Process process = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = FileName,
                Arguments = Arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            if (runAsAdmin)
            {
                processStartInfo.Verb = "runas";
            }
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    ErrorDatas.Add(e.Data);
                }
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    OutputDatas.Add(e.Data);
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
                Exception = ex;
                return;
            }


        }
    }
}
