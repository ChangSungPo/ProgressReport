using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressReport
{
    public class FileGrouper
    {
        private readonly ILogger<FileGrouper> _logger;
        public FileGrouper(ILogger<FileGrouper> logger)
        {
            _logger = logger;
        }

        public Dictionary<string, List<string>> GroupFileByTeacherName(string[] filePaths)
        {
            try
            {
                Dictionary<string, List<string>> fileGroupByTeacherName = new Dictionary<string, List<string>>();

                foreach (var file in filePaths)
                {
                    string fileName = GetFileName(file);
                    string teacherName = GetTeacherName(fileName);

                    if (fileGroupByTeacherName.ContainsKey(teacherName))
                    {
                        fileGroupByTeacherName[teacherName].Add(file);
                    }
                    else
                    {
                        fileGroupByTeacherName[teacherName] = new List<string> { file };
                    }
                }

                foreach (var item in fileGroupByTeacherName)
                {
                    _logger.LogInformation($"Teacher {item.Key} contains {item.Value.Count()} excel files");
                }

                return fileGroupByTeacherName;

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error");

                throw;
            }

        }

        // Method to get file name from the file path
        string GetFileName(string filePath)
        {
            var segments = filePath.Split("\\");
            return segments.Length > 2 ? segments[2] : string.Empty;
        }

        // Method to get teacher name from the file name
        string GetTeacherName(string fileName)
        {
            return fileName.Length >= 10 ? fileName.Substring(7, 3) : string.Empty;
        }
    }
}
