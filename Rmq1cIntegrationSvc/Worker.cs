using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rmq1cIntegrationSvc
{
    class Worker
    {
        private readonly WorkerConfiguration configuration;
        private IModel transportModel;
        private string transportTag = string.Empty;

        /// <summary>
        /// Инициализация обработчика
        /// </summary>
        /// <param name="configuration">Параметры обработчика (параметры очередей, сервиса и т.п.) </param>
        public Worker(WorkerConfiguration configuration)
        {
            this.configuration = configuration;
            if (configuration.Enabled)
            {
                StartWorker();
            }
        }
        /// <summary>
        /// Создает подписку на новые сообщения в очереди
        /// Запоминает идентификатор подписки {transportTag} (для возможности отключения)
        /// </summary>
        public void StartWorker()
        {
            transportTag = string.Empty;
            configuration.logger.WriteEntry("\n\n* Starting transporting messages to 1C...");
            try
            {

                transportModel = CreateModel();

                var consumer = new EventingBasicConsumer(transportModel);
                consumer.Received += ProcessMessage;

                transportTag = transportModel.BasicConsume(configuration.ConsumeQueue, true, consumer);
            }
            catch (Exception e)
            {
                configuration.logger.WriteEntry($"StartWorker error:\n{e}");
            }
        }
        /// <summary>
        /// Обрабатывает полученные сообщения из очереди:
        /// 1. Пересылвает содержимое сообщения в 1С через http-сервис
        /// 2. Если ответ 1С содержит данные, то публикует их в ответной очереди
        /// </summary>
        public void ProcessMessage(object model, BasicDeliverEventArgs ea)
        {
            byte[] body = ea.Body.ToArray();
            string answer = PostMessageTo1C(Encoding.UTF8.GetString(body)).Result;
            if (!string.IsNullOrEmpty(answer))
            {
                PublishMessage(answer);
            }
        }
        /// <summary>
        /// Отправляет сообщение в 1С в виде http-POST запроса
        /// </summary>
        /// <returns>Возвращает ответ 1С</returns>
        public async Task<string> PostMessageTo1C(string message)
        {
            string result = string.Empty;

            HttpClientHandler httpClientHandler = new HttpClientHandler();
            if (string.IsNullOrEmpty(configuration.HttpLogin))
            {
                httpClientHandler.Credentials = CredentialCache.DefaultCredentials;
            }
            else
            {
                httpClientHandler.Credentials = new NetworkCredential(configuration.HttpLogin, configuration.HttpPassword);
            };
            HttpClient httpClient = new HttpClient(httpClientHandler);
            HttpRequestMessage httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(configuration.HttpRequestUri),
                Content = new StringContent(message, Encoding.UTF8, "application/json")
            };
            try
            {
                using (var httpResponse = await httpClient.SendAsync(httpRequest))
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                        result = await httpResponse.Content.ReadAsStringAsync();
                }
                httpRequest.Dispose();
                httpClient.Dispose();
            }
            catch (Exception e)
            {
                configuration.logger.WriteEntry($"PostMessageTo1C error:\n{e}");
            }

            return result;
        }
        /// <summary>
        /// Завершает прием сообщений из очереди
        /// </summary>
        public void StopWorker()
        {
            if (string.IsNullOrEmpty(transportTag))
                return;
            if (transportModel == null)
                return;
            try
            {
                transportModel.BasicCancel(transportTag);
                transportModel.Dispose();
                transportModel = null;
                transportTag = string.Empty;
                configuration.logger.WriteEntry("Consume is stopped.");
            }
            catch (Exception e)
            {
                configuration.logger.WriteEntry($"StopWorker error:\n{e}");
            }
        }
        /// <summary>
        /// Создает соединение к серверу RMQ
        /// </summary>
        private IModel CreateModel()
        {
            if (string.IsNullOrEmpty(configuration.RmqServer))
            {
                throw new Exception("Empty Server name!");
            }
            ConnectionFactory factory = new ConnectionFactory
            {
                UserName = configuration.RmqLogin,
                Password = configuration.RmqPassword,
                VirtualHost = "/",
                HostName = configuration.RmqServer,
                Port = configuration.RmqPort

            };
            IConnection connection = factory.CreateConnection();
            return connection.CreateModel();
        }
        /// <summary>
        /// Публикует (отправляет) ответ 1С в "ответную" очередь
        /// </summary>
        private void PublishMessage(string message)
        {
            try
            {
                byte[] body = Encoding.UTF8.GetBytes(message);
                transportModel.BasicPublish(configuration.PublishExchange, configuration.PublishRoutingKey, null, body);
            }
            catch (Exception e)
            {
                configuration.logger.WriteEntry($"PublishMessage error:\n{e}");
            }
        }
    }
}