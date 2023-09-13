# Введение 
Rmq1cIntegrationSvc - служба Windows выполняющая следующую задачу:
1. Получает сообщения из очереди RabbitMQ
2. Направляет полученные сообщения в http-сервис 1C
3. Возвращает ответ 1С в очередь RabbitMQ

# Начало работы
1.	Процесс установки

%windir%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe [service path]\Rmq1cIntegrationSvc.exe

2.	Настройка

При первом запуске служба создаст пустые параметры в реестре
__\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\EzBoD\Rmq1cIntegrationSvc__

3. Описание параметров:

| Параметр          | Описание                                     |
| ----------------- |----------------------------------------------|
| Enabled           | Отвечает за активность службы (1 - включена) |
| RmqServer         | Адрес сервера RabbitMQ                       |
| RmqPort           | Порт сервера RabbitMQ                        |
| RmqLogin          | Имя пользователя RabbitMQ                    |
| RmqPassword       | Пароль пользователя RabbitMQ                 |
| ConsumeQueue      | Входящая очередь                             |
| PublishQueue      | Очередь для ответа                           |
| PublishExchange   | Точка обмена ответной очереди                |
| PublishRoutingKey | Ключ маршрутизации ответной очереди          |
| HttpRequestUri    | Адрес http-сервиса 1С                        |
| HttpLogin         | Логин пользователя 1С                        |
| HttpPassword      | Пароль пользователя 1С                       |
