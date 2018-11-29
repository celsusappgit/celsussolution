using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types.NonDatabase
{
    public class FileHelper
    {
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

        public static  string GetUnusedFolderName(string folderPath, string desiredFolderName)
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
            DirectoryInfo dirA = new DirectoryInfo(pathA);
            DirectoryInfo dirB = new DirectoryInfo(pathB);

            if (dirA.Exists==false || dirB.Exists==false)
            {
                return false;
            }
            // Take a snapshot of the file system.  
            IEnumerable<FileInfo> list1 = dirA.GetFiles("*.*", SearchOption.AllDirectories);
            IEnumerable<FileInfo> list2 = dirB.GetFiles("*.*", SearchOption.AllDirectories);

            //A custom file comparer defined below  
            FileCompare myFileCompare = new FileCompare();

            // This query determines whether the two folders contain  
            // identical file lists, based on the custom file comparer  
            // that is defined in the FileCompare class.  
            // The query executes immediately because it returns a bool.  
            bool areIdentical = list1.SequenceEqual(list2, myFileCompare);

            if (areIdentical == true)
            {
                Console.WriteLine("the two folders are the same");
                return true;
            }
            else
            {
                return false;
                Console.WriteLine("The two folders are not the same");
            }

            // Find the common files. It produces a sequence and doesn't   
            // execute until the foreach statement.  
            var queryCommonFiles = list1.Intersect(list2, myFileCompare);

            if (queryCommonFiles.Count() > 0)
            {
                Console.WriteLine("The following files are in both folders:");
                foreach (var v in queryCommonFiles)
                {
                    Console.WriteLine(v.FullName); //shows which items end up in result list  
                }
            }
            else
            {
                Console.WriteLine("There are no common files in the two folders.");
            }

            // Find the set difference between the two folders.  
            // For this example we only check one way.  
            var queryList1Only = (from file in list1 select file).Except(list2, myFileCompare);

            Console.WriteLine("The following files are in list1 but not list2:");
            foreach (var v in queryList1Only)
            {
                Console.WriteLine(v.FullName);
            }

            // Keep the console window open in debug mode.  
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

        }

        class FileCompare : System.Collections.Generic.IEqualityComparer<FileInfo>
        {
            public FileCompare() { }

            public bool Equals(FileInfo f1, FileInfo f2)
            {
                return (f1.Name == f2.Name &&
                        f1.Length == f2.Length);
            }

            // Return a hash that reflects the comparison criteria. According to the   
            // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must  
            // also be equal. Because equality as defined here is a simple value equality, not  
            // reference identity, it is possible that two or more objects will produce the same  
            // hash code.  
            public int GetHashCode(FileInfo fi)
            {
                string s = String.Format("{0}{1}", fi.Name, fi.Length);
                return s.GetHashCode();
            }
        }

    }
}
