using System;
using System.Collections.Generic;


namespace FyleSystem_API.Models
{
    public class Explorer 
    {
        public string ParentDirPath { get; set; }
        public string CurrentPath { get; set; }
        public DirectoryData DirInfo { get; set; }
        public List<DirectoryData> DirectoryList { get; set; }
        public List<FileData> FileList { get; set; }
        public List<ServerDrive> DriveList { get; set; }

        public Explorer()
        {
            DirInfo = new DirectoryData();
            DirectoryList  = new List<DirectoryData>();
            FileList = new List<FileData>();
            DriveList = new List<ServerDrive>();
        }
    }
}