using log4net;
using log4net.Config;
using System.Reflection;

namespace manage
{
    internal class Program
    {
        private static readonly ILog log = LogManager.GetLogger(type: MethodBase.GetCurrentMethod()!.DeclaringType!);

        static void Main(string[] args)
        {
            var decType = System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!;
            ILog logger = LogManager.GetLogger(decType);

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly()!);
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            FileUtil.ScanRegFolder();
            Console.WriteLine("Hello, World!");
        }
    }
}
