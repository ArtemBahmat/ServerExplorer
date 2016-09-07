using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;

namespace FyleSystem_API.Models
{
    public class DataManager
    {
        public const int Mb10 = 10485760;   // 10Mb in Bytes 
        public const int Mb50 = 52428800;   // 50Mb in Bytes
        public const int Mb100 = 104857600; // 100Mb in Bytes
        public Dictionary<string, DirectorySecurity> lockedDirs = new Dictionary<string, DirectorySecurity>();


        public Explorer GetExplorer(string path)
        {
            Explorer explorer = new Explorer();

            explorer.CurrentPath = path;
            explorer.ParentDirPath = GetParentDirPath(path);

            explorer.DriveList = GetFixedDrives();
            explorer.DirInfo = GetFilesGroupedData(path);

            if (Directory.Exists(path))
            {
                try
                {
                    explorer.FileList = GetFileList(path);
                    explorer.DirectoryList = GetDirectoryList(path);
                }
                catch (SystemException)
                {
                    explorer = null;
                }
            }

            if (lockedDirs.Count() > 0)
            {
                SetAccessRulesToDeny();    // return Access Control to last state    
                lockedDirs.Clear();
            }
            return explorer;
        }

        private List<DirectoryData> GetDirectoryList(string path)
        {
            List<DirectoryData> result = new List<DirectoryData>();
            string subPath = String.Empty;
            string[] directoriesArray = Directory.GetDirectories(path);

            foreach (string directory in directoriesArray)
            {
                subPath = directory.Substring(directory.LastIndexOf('\\') + 1, directory.Length - directory.LastIndexOf('\\') - 1);
                result.Add(new DirectoryData { Name = subPath, FullPath = directory });
            }

            return result;
        }

        private List<FileData> GetFileList(string path)
        {
            string subPath = String.Empty;
            List<FileData> result = new List<FileData>();
            string[] filesArray = Directory.GetFiles(path, "*.*");

            foreach (string file in filesArray)
            {
                subPath = file.Substring(file.LastIndexOf('\\') + 1, file.Length - file.LastIndexOf('\\') - 1);
                result.Add(new FileData { Name = subPath, FullPath = file });
            }

            return result;
        }


        private List<ServerDrive> GetFixedDrives()
        {
            List<ServerDrive> fixedDrives = new List<ServerDrive>();
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady == true && d.DriveType == DriveType.Fixed)
                    {
                        fixedDrives.Add(new ServerDrive { Name = d.Name, Label = d.VolumeLabel });
                    }
                }
            }
            catch { }
            return fixedDrives;
        }


        private DirectoryData GetFilesGroupedData(string path, DirectoryData currentDirInfo = null)
        {
            if (currentDirInfo == null)
                currentDirInfo = new DirectoryData();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            IEnumerable<DirectoryInfo> dirs;
            try
            {
                dirs = directoryInfo.EnumerateDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    if (!((dir.Attributes & FileAttributes.System) == FileAttributes.System))
                    {
                        currentDirInfo = GetFilesGroupedData(dir.FullName, currentDirInfo);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                SetAccessRuleToAllow(ex);
                GetFilesGroupedData(path);
            }

            string[] directories = Directory.GetDirectories(path);
            if (directories.Length == 0)                      // if there is NO subdirectory
            {
                if (Directory.GetFiles(path).Length > 0)      // if there are files in the current directory
                {
                    CalculateGroupingFiles(ref currentDirInfo, path);
                }
                else                                          // NO files in the current directory                
                {
                    foreach (string d in directories)         //go to subdirectories of current dir
                    {
                        CalculateGroupingFiles(ref currentDirInfo, d);
                    }
                }
            }
            else                                            // there is subdirectory(es)
            {
                if (Directory.GetFiles(path).Length > 0)      // if there is file in the current directory
                {
                    CalculateGroupingFiles(ref currentDirInfo, path);
                }
            }
            return currentDirInfo;
        }


        private void CalculateGroupingFiles(ref DirectoryData currentDirData, string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            currentDirData.CountFilesLess10Mb = currentDirData.CountFilesLess10Mb +
                   directoryInfo.GetFiles()
                  .Where(file => file.Length <= Mb10).Count();
            currentDirData.CountFiles10_50Mb = currentDirData.CountFiles10_50Mb +
                  directoryInfo.GetFiles()
                  .Where(file => file.Length > Mb10 && file.Length <= Mb50).Count();
            currentDirData.CountFilesMore100Mb = currentDirData.CountFilesMore100Mb +
                  directoryInfo.GetFiles()
                  .Where(file => file.Length > Mb100).Count();
        }


        private void SetAccessRuleToAllow(UnauthorizedAccessException ex)
        {
            // Prepare path (get from error-message)
            string path = ex.Message;
            path = path.Substring(path.IndexOf('"') + 1);
            path = path.Remove(path.IndexOf('"'), path.Length - path.IndexOf('"'));

            DirectorySecurity currentSecurity = Directory.GetAccessControl(path);
            lockedDirs.Add(path, currentSecurity);              // Save the state of AccessControl for the path
            string user = Environment.UserName;
            FileSystemAccessRule rule = new FileSystemAccessRule(user, FileSystemRights.FullControl, AccessControlType.Allow);
            DirectorySecurity security = new DirectorySecurity();
            security.AddAccessRule(rule);
            Directory.SetAccessControl(path, security);
        }

        private void SetAccessRulesToDeny()
        {
            string user = Environment.UserName;

            foreach (KeyValuePair<string, DirectorySecurity> dir in lockedDirs)
            {
                Directory.SetAccessControl(dir.Key, dir.Value);
            }
        }

        private string GetParentDirPath(string path)
        {
            string result = String.Empty;
            string tempPath = String.Empty;

            if (path.Length > 3)               // Check if it's not root
            {
                if (path.EndsWith("\\"))
                    tempPath = path.Substring(0, path.Length - 1);
                result = tempPath.Remove(tempPath.LastIndexOf('\\') + 1);
            }

            return result;
        }
    }
}