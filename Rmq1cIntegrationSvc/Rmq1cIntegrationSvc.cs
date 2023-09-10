using System.ServiceProcess;

namespace Rmq1cIntegrationSvc
{
    public partial class Rmq1cIntegrationSvc : ServiceBase
    {
        // Для записи сообщений
        readonly Logger logger = new Logger();

        // Путь к настройкам в реестре
        // {Компьютер\HKEY_LOCAL_MACHINE\SOFTWARE}
        // в x64 путь будет {Компьютер\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node}
        const string rmq1c = @"Software\EzBoD\Rmq1cIntegrationSvc";

        // Настройки обработчика
        WorkerConfiguration configuration;
        
        // Обработчик
        Worker worker;

        public Rmq1cIntegrationSvc()
        {
            InitializeComponent();

            configuration = new WorkerConfiguration(rmq1c, logger);
        }
        protected override void OnStart(string[] args)
        {
            configuration.ReadConfiguration();
            if (configuration.Enabled)
            {
                worker = new Worker(configuration);
            }
        }
        protected override void OnStop()
        {
            worker?.StopWorker();
        }
    }
}