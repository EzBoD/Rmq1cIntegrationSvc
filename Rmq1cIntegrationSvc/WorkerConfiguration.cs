using Microsoft.Win32;
using System;

namespace Rmq1cIntegrationSvc
{
    class WorkerConfiguration
    {
        private readonly string registryKeyPath = "";
        public Logger logger;
        public bool Enabled { get; set; }
        public string RmqServer { get; set; } = "";
        public int RmqPort { get; set; } = 5672;
        public string RmqLogin { get; set; } = "";
        public string RmqPassword { get; set; } = "";
        public string ConsumeQueue { get; set; } = "";
        public string PublishQueue { get; set; } = "";
        public string PublishExchange { get; set; } = "";
        public string PublishRoutingKey { get; set; } = "";
        public string HttpRequestUri { get; set; } = "";
        public string HttpLogin { get; set; } = "";
        public string HttpPassword { get; set; } = "";
        
        /// <summary>
        /// Инициализация параметров обработчика
        /// </summary>
        /// <param name="registryKeyPath">Раздел реестра для хранения настроек.</param>
        /// <param name="logger">Объект для записи лога.</param>
        public WorkerConfiguration(string registryKeyPath, Logger logger)
        {
            this.registryKeyPath = registryKeyPath;
            this.logger = logger;
        }
        /// <summary>
        /// Считывает параметры обработчика из реестра
        /// если настроек нет, то создает соответствующий раздел реестра
        /// </summary>
        public void ReadConfiguration()
        {
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(registryKeyPath, true);

                if (registryKey != null)
                {
                    Enabled = (string)registryKey.GetValue("Enabled", "0") == "1";
                    RmqServer = (string)registryKey.GetValue("RmqServer", "localhost");
                    RmqPort = (int)registryKey.GetValue("RmqPort", 5672);
                    RmqLogin = (string)registryKey.GetValue("RmqLogin", "");
                    RmqPassword = (string)registryKey.GetValue("RmqPassword", "");
                    ConsumeQueue = (string)registryKey.GetValue("ConsumeQueue", "");
                    PublishQueue = (string)registryKey.GetValue("PublishQueue", "");
                    PublishExchange = (string)registryKey.GetValue("PublishExchange", "");
                    PublishRoutingKey = (string)registryKey.GetValue("PublishRoutingKey", "");
                    HttpRequestUri = (string)registryKey.GetValue("HttpRequestUri", "");
                    HttpLogin = (string)registryKey.GetValue("HttpLogin", "");
                    HttpPassword = (string)registryKey.GetValue("HttpPassword", "");
                    registryKey.Close();
                }
                else
                {
                    logger.WriteEntry("Настройки не обнаружены (отсутствует раздел реестра).");
                    WriteDefaultConfiguration();
                }
            }
            catch (Exception e)
            {
                logger.WriteEntry($"Ошибка при загрузке настроек: \n{e}");
            }
        }
        /// <summary>
        /// Создает раздел реестра с настройками по умолчанию
        /// </summary>
        private void WriteDefaultConfiguration()
        {
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(registryKeyPath, true);

                registryKey.SetValue("Enabled", "0", RegistryValueKind.String);
                registryKey.SetValue("RmqServer", RmqServer, RegistryValueKind.String);
                registryKey.SetValue("RmqPort", RmqPort, RegistryValueKind.DWord);
                registryKey.SetValue("RmqLogin", RmqLogin, RegistryValueKind.String);
                registryKey.SetValue("RmqPassword", RmqPassword, RegistryValueKind.String);
                registryKey.SetValue("ConsumeQueue", ConsumeQueue, RegistryValueKind.String);
                registryKey.SetValue("PublishQueue", PublishQueue, RegistryValueKind.String);
                registryKey.SetValue("PublishExchange", PublishExchange, RegistryValueKind.String);
                registryKey.SetValue("PublishRoutingKey", PublishRoutingKey, RegistryValueKind.String);
                registryKey.SetValue("HttpRequestUri", HttpRequestUri, RegistryValueKind.String);
                registryKey.SetValue("HttpLogin", HttpLogin, RegistryValueKind.String);
                registryKey.SetValue("HttpPassword", HttpPassword, RegistryValueKind.String);

                registryKey.Close();
                
                logger.WriteEntry($"Создан раздел реестра: {registryKey.Name}. Заполните настройки.");
            }
            catch (Exception e)
            {
                logger.WriteEntry($"Не удалось создать раздел реестра с настройками по умолчанию:\n{e}");
            }
        }
    }
}