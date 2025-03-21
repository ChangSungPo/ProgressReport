using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ProgressReport
{
    class ReportGenerator
    {
        private readonly ILogger<ReportGenerator> _logger;
        public ReportGenerator(ILogger<ReportGenerator> logger)
        {
            _logger = logger;
        }

        public void GenerateReport(Dictionary<string, List<string>>? fileGroupByTeacherName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (fileGroupByTeacherName == null || fileGroupByTeacherName.Count == 0)
            {
                _logger.LogInformation("No data to process.");
                return;
            }

            foreach (var (teacherName, fileList) in fileGroupByTeacherName)
            {
                _logger.LogInformation($"{teacherName} process start");
                ProcessTeacherFiles(teacherName, fileList);
            }
        }

        private void ProcessTeacherFiles(string teacherName, List<string> fileList)
        {
            if (fileList == null || fileList.Count == 0)
            {
                _logger.LogInformation($"No files for teacher: {teacherName}");
                return;
            }

            var groupedFilesByYear = fileList
                .Where(IsXlsxFile)
                .OrderBy(GetMonthFromFileName)
                .GroupBy(GetYearFromFileName)
                .ToList();

            foreach (var yearGroup in groupedFilesByYear)
            {
                _logger.LogInformation($"{yearGroup.Key} process start");
                ProcessYearGroup(teacherName, yearGroup.Key, yearGroup);
            }
        }

        private void ProcessYearGroup(string teacherName, string year, IEnumerable<string> yearGroup)
        {
            var patientsYearReport = new Dictionary<string, Dictionary<string, string>>();
            var yearList = new List<string>();

            foreach (var file in yearGroup)
            {
                _logger.LogInformation($"{file} process start");
                ProcessFile(file, yearList, patientsYearReport);
            }

            CreateYearlyReport(teacherName, year, yearList, patientsYearReport);
        }

        private void ProcessFile(string file, List<string> yearList, Dictionary<string, Dictionary<string, string>> patientsYearReport)
        {
            string yearMonth = GetYearMonthFromFileName(file);
            yearList.Add(yearMonth);

            using (var readPackage = new ExcelPackage(new FileInfo(file)))
            {
                foreach (var worksheet in readPackage.Workbook.Worksheets)
                {
                    if (worksheet.Name == "總成績") continue;

                    string patientName = worksheet.Name;
                    string patientScore = worksheet.Cells["AG19"].Text;

                    if (!patientsYearReport.ContainsKey(patientName))
                    {
                        patientsYearReport[patientName] = new Dictionary<string, string>();
                    }
                    patientsYearReport[patientName][yearMonth] = patientScore;
                }
            }
        }

        private void CreateYearlyReport(string teacherName, string year, List<string> yearList, Dictionary<string, Dictionary<string, string>> patientsYearReport)
        {
            using (var writePackage = new ExcelPackage())
            {
                var reportsheet = writePackage.Workbook.Worksheets.Add("Sheet1");
                PopulateReportSheet(reportsheet, yearList, patientsYearReport);

                string reportFileName = $"習慣養成年度統計-{teacherName}{year}.xlsx";
                writePackage.SaveAs(new FileInfo(reportFileName));
            }
        }

        private void PopulateReportSheet(ExcelWorksheet sheet, List<string> yearList, Dictionary<string, Dictionary<string, string>> patientsYearReport)
        {
            int row = 2;

            foreach (var year in yearList)
            {
                int col = 1;
                sheet.Cells[row, col++].Value = year;

                foreach (var patientScoreReport in patientsYearReport)
                {
                    sheet.Cells[1, col].Value = patientScoreReport.Key;
                    sheet.Cells[row, col].Value = patientScoreReport.Value.ContainsKey(year)
                        ? patientScoreReport.Value[year]
                        : string.Empty;
                    col++;
                }
                row++;
            }
        }

        // Helper method to check file extension
        private bool IsXlsxFile(string file)
        {
            return file.Split('.').Length > 1 && file.Split('.')[1] == "xlsx";
        }

        // Helper method to extract the year from the file name
        private string GetYearFromFileName(string file)
        {
            var fileNameWithoutExtension = file.Split('.')[0];
            return fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 5, 3);
        }

        private string GetMonthFromFileName(string file)
        {
            var fileNameWithoutExtension = file.Split('.')[0];
            return fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 2, 2);
        }

        private string GetYearMonthFromFileName(string file)
        {
            var fileNameWithoutExtension = file.Split('.')[0];
            return fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 5, 5);
        }
    }
}
