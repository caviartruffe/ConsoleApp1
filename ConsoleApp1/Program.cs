using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Reflection;

namespace manage
{
    internal class ApplicationConfig
    {
        public List<string> ExtensionsOffice { get; set; } = new();
        public List<string> ExtensionsAutoCad { get; set; } = new();
        public List<string> ExtensionsIcadMx { get; set; } = new();
    }

    internal class Program
    {
        // log4net 
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("manage.json", optional: false, reloadOnChange: true)
                .Build();
            // セクションを指定してクラスに一括バインド
            var appConfig = config.GetSection("ApplicationConfig").Get<ApplicationConfig>();


            // 最初にlog4net初期化
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly()!);
            var fileInfo = new FileInfo("log4net.config");
            if (!fileInfo.Exists)
            {
                throw new Exception("ManageProgram: Error: log4net.config not found.");
            }
            XmlConfigurator.Configure(logRepository, fileInfo);

            _logger.Info("管理プログラム開始");

            //FileUtil.ScanRegFolder();
            Console.WriteLine("Hello, World!");

            _logger.Info("管理プログラム終了");
        }
    }
}
