using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public class FileHelper
    {
        protected static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static string GetUnusedFileName(string folderPath, string desiredFileName)
        {
            var targetPath = Path.Combine(folderPath, desiredFileName);
            var fileName = Path.GetFileNameWithoutExtension(desiredFileName);
            var extension = Path.GetExtension(desiredFileName);
            int index = 0;
            while (true)
            {
                index++;
                if (File.Exists(targetPath))
                {
                    targetPath = Path.Combine(folderPath, fileName + index + extension);
                }
                else
                {
                    break;
                }
            }
            return targetPath;
        }

        public static string GetUnusedFolderName(string folderPath, string desiredFolderName)
        {
            var targetPath = Path.Combine(folderPath, desiredFolderName);
            int index = 0;
            while (true)
            {
                index++;
                if (Directory.Exists(targetPath))
                {
                    targetPath = Path.Combine(folderPath, desiredFolderName + index);
                }
                else
                {
                    break;
                }
            }
            return targetPath;
        }

        public static bool CompareFolders(string pathA, string pathB)
        {
            DirectoryInfo dirA = null;
            DirectoryInfo dirB = null;
            try
            {
                dirA = new DirectoryInfo(pathA);
                dirB = new DirectoryInfo(pathB);

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"DirectoryInfo error.");
                return false;
            }

            if (dirA.Exists == false || dirB.Exists == false)
            {
                return false;
            }
            IEnumerable<FileInfo> dirAFiles = null;
            IEnumerable<FileInfo> dirBFiles = null;

            try
            {
                dirAFiles = dirA.GetFiles("*.*", SearchOption.AllDirectories);
                dirBFiles = dirB.GetFiles("*.*", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetFiles error.");
                return false;
            }

            FileCompare myFileCompare = new FileCompare();

            bool areIdentical = dirAFiles.SequenceEqual(dirBFiles, myFileCompare);

            if (areIdentical == true)
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        class FileCompare : IEqualityComparer<FileInfo>
        {
            public FileCompare() { }

            public bool Equals(FileInfo fileInfo1, FileInfo fileInfo2)
            {
                return fileInfo1.Name == fileInfo2.Name && fileInfo1.Length == fileInfo2.Length;
            }

            public int GetHashCode(FileInfo fileInfo)
            {
                return string.Format("{0}{1}", fileInfo.Name, fileInfo.Length).GetHashCode();
            }
        }

    }
}
