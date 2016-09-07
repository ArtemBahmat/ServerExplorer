using System;
using System.Web.Http;
using FyleSystem_API.Models;


namespace FyleSystem_API.Controllers
{
    public class ExplorerController : ApiController
    {
        [HttpGet]
        public Explorer GetBaseExplorer()
        {
            string baseDirectoryPath = System.AppDomain.CurrentDomain.BaseDirectory;
            DataManager dataManager = new DataManager();
            Explorer explorer = dataManager.GetExplorer(baseDirectoryPath);
            return explorer;
        }

        [HttpPost]
        public Explorer GetUpdatedExplorer([FromBody]string path)
        {
            DataManager dataManager = new DataManager();
            Explorer explorer = dataManager.GetExplorer(path);
            return explorer;
        }
    }
}
