using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;
//using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProgressReport
{
    public class FileLocator
    {
        private readonly ILogger<FileLocator> _logger;
        public FileLocator(ILogger<FileLocator> logger)
        {
            _logger = logger;
        }


        public string[] FindAllFile()
        {
            try
            {
                string[] excelFiles = [];
                string rootPath = ConfigurationManager.AppSettings.Get("rootPath");
                _logger.Log(LogLevel.Information, "Root path: " + rootPath);

                if (!String.IsNullOrEmpty(rootPath))
                {
                    excelFiles = Directory.GetFiles(rootPath, "*.xlsx", SearchOption.AllDirectories);
                }

                return excelFiles;

            } catch (Exception e)
            {
                _logger.LogError(e, "Error");

                throw;
            }

        }
        
    }
}
