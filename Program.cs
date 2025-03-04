using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;

namespace ProgressReport
{
    public class Program
    {
        private readonly ILogger<Program> _logger;

        public Program(ILogger<Program> logger)
        {
            _logger = logger;
        }

        static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            var logger = serviceProvider.GetService<ILogger<Program>>();
            var program = new Program(logger);

            program.Run(serviceProvider);
        }

        private void Run(IServiceProvider serviceProvider)
        {
            try
            {
                var fileLocator = serviceProvider.GetService<FileLocator>();
                var fileGrouper = serviceProvider.GetService<FileGrouper>();

                string[] excelFiles = fileLocator?.FindAllFile() ?? Array.Empty<string>();
                var fileGroupByTeacherName = fileGrouper?.GroupFileByTeacherName(excelFiles);


                Console.WriteLine("Press any key for continuing...");
                Console.Read();

            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred.");
            }
        }



        private static IServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(logging =>
            {
                //清除原本的 logging provider
                logging.ClearProviders();
                //設定 logging 的 minimum level 為 Information
                logging.SetMinimumLevel(LogLevel.Information);
                //使用 NLog 作為 logging provider
                logging.AddNLog("nlog.config");
            });
            serviceCollection.AddTransient<FileLocator>();
            serviceCollection.AddTransient<FileGrouper>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}
