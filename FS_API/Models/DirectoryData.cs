using System;


namespace FyleSystem_API.Models
{
    public class DirectoryData
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public int CountFilesLess10Mb { get; set; }
        public int CountFiles10_50Mb { get; set; }
        public int CountFilesMore100Mb { get; set; } 
    }
}