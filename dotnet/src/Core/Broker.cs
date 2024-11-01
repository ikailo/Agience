using GuerrillaNtp;
using Microsoft.IdentityModel.Tokens;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Formatter;
using MQTTnet.Diagnostics;
using Timer = System.Timers.Timer;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Agience.Core.Models.Messages;

namespace Agience.Core
{
    public class Broker
    {
        internal event Func<Task> Disconnected
        {
            add => _mqttClient.DisconnectedAsync += async (args) => await value();
            remove => _mqttClient.DisconnectedAsync -= async (args) => await value();
        }

        NtpClient? _ntpClient;

        string? _customNtpHost = null;

        //https://www.ntppool.org/zone/@
        List<string> ntpHosts = new() {
            "pool.ntp.org",
            "north-america.pool.ntp.org",
            "europe.pool.ntp.org",
            "asia.pool.ntp.org",
            "south-america.pool.ntp.org",
            "africa.pool.ntp.org",
            "oceania.pool.ntp.org"};

        public string Timestamp => (_ntpClient?.Last ?? throw new InvalidOperationException()).Now.UtcDateTime.ToString(TIME_FORMAT);

        private Timer _ntpTimer = new Timer(TimeSpan.FromDays(1).TotalMilliseconds); // Synchronize daily

        public bool IsConnected => _mqttClient.IsConnected;

        private const string MESSAGE_TYPE_KEY = "message.type";
        private const string TIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fff";

        private readonly IMqttClient _mqttClient;
        private readonly Dictionary<string, List<CallbackContainer>> _callbacks = new();

        private readonly ILogger<Broker> _logger;        

        internal Broker(ILogger<Broker> logger, string? customNtpHost = null)
        {
            _logger = logger;
            _customNtpHost = customNtpHost;
            _mqttClient = new MqttFactory().CreateMqttClient(new MqttNetLogger() { IsEnabled = true });
            _mqttClient.ApplicationMessageReceivedAsync += _client_ApplicationMessageReceivedAsync;
        }

        internal async Task Connect(string token, string brokerUri)
        {
            await StartNtpClock();

            _logger.LogInformation($"Broker Connected Status: {IsConnected}");

            if (!_mqttClient.IsConnected)
            {
                _logger.LogInformation($"Connecting to {brokerUri}");

                var options = new MqttClientOptionsBuilder()
                    .WithWebSocketServer(configure => { configure.WithUri(brokerUri); })
                    .WithTlsOptions(configure => { configure.UseTls(true); })
                    .WithCredentials(token, "<no_password>")
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .WithoutThrowOnNonSuccessfulConnectResponse()
                    .WithTimeout(TimeSpan.FromSeconds(300))
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
                    .WithSessionExpiryInterval(60)
                    .WithCleanStart()
                    .Build();

                await _mqttClient.ConnectAsync(options);

                if (_mqttClient.IsConnected)
                {
                    _logger.LogInformation($"Broker Connected");
                }
                else
                {
                    _logger.LogInformation($"Broker Connection Failed");
                }                
            }
        }

        private Task _client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic;
            var callbackTopic = topic.Substring(topic.IndexOf('/') + 1); // Remove the SenderId segment

            if (_callbacks.TryGetValue(callbackTopic, out var callbackContainers))
            {
                var messageType = Enum.TryParse<BrokerMessageType>(
                    args.ApplicationMessage.UserProperties.FirstOrDefault(x => x.Name == MESSAGE_TYPE_KEY)?.Value.ToString(), out var parsedMessageType) ?
                    parsedMessageType :
                    BrokerMessageType.UNKNOWN;

                var message = new BrokerMessage()
                {
                    Type = messageType,
                    Topic = args.ApplicationMessage.Topic //,
                    //Payload = args.ApplicationMessage.ConvertPayloadToString()
                };

                switch (messageType)
                {
                    case BrokerMessageType.EVENT:
                        message.Data = args.ApplicationMessage.ConvertPayloadToString();
                        break;
                    case BrokerMessageType.INFORMATION:
                        var payloadString = args.ApplicationMessage.ConvertPayloadToString();
                        message.Information = JsonSerializer.Deserialize<Information>(payloadString);
                        break;
                }

                foreach (var container in callbackContainers)
                {
                    if (container.Callback != null)
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Run(() => container.Callback(message));
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "An error occurred in the callback.");
                                throw;
                            }
                        });
                    }
                }
            }
            return Task.CompletedTask;
        }

        internal async Task Subscribe(string topic, Func<BrokerMessage, Task> callback)
        {
            if (!_mqttClient.IsConnected) throw new InvalidOperationException("Not Connected");

            var callbackTopic = topic.Substring(topic.IndexOf('/') + 1); // Remove the SenderId segment

            var container = new CallbackContainer(callback);
            if (!_callbacks.ContainsKey(callbackTopic))
            {
                _callbacks[callbackTopic] = new List<CallbackContainer>();
            }
            _callbacks[callbackTopic].Add(container);

            var options = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic, MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            await _mqttClient.SubscribeAsync(options);
        }

        internal async Task Disconnect()
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.TryDisconnectAsync();
            }
        }

        internal async Task Unsubscribe(string topic)
        {
            var callbackTopic = topic.Substring(topic.IndexOf('/') + 1);

            _callbacks.Remove(callbackTopic);

            await _mqttClient.UnsubscribeAsync(callbackTopic);
        }

        internal void Publish(BrokerMessage message)
        {
            PublishAsync(message).ContinueWith(task =>
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    throw task.Exception;
                }
            }, TaskScheduler.Current);
        }

        internal async Task PublishAsync(BrokerMessage message)
        {
            if (_mqttClient.IsConnected)
            {
                string payload = string.Empty;

                switch (message.Type)
                {
                    case BrokerMessageType.EVENT:
                        payload = message.Data?.ToString() ?? string.Empty;
                        break;
                    case BrokerMessageType.INFORMATION:
                        payload = JsonSerializer.Serialize(message.Information);
                        break;
                }

                var mqMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(message.Topic ?? throw new ArgumentNullException(nameof(message.Topic)))
                    .WithPayload(payload)
                    .WithRetainFlag(false)
                    .WithUserProperty(MESSAGE_TYPE_KEY, message.Type.ToString())
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                    .Build();

                await _mqttClient.PublishAsync(mqMessage);
            }
        }

        private class MqttNetLogger : IMqttNetLogger
        {
            public bool IsEnabled { get; internal set; }

            public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
            {
                // TODO: Write to real logger
                // Console.WriteLine($"{logLevel}: {source} - {message}");
            }
        }

        private async Task StartNtpClock()
        {
            await QueryNtpWithBackoff(); // Query now

            _ntpTimer.Elapsed += async (sender, args) => { await QueryNtpWithBackoff(); };
            _ntpTimer.AutoReset = true;
            _ntpTimer.Start();
        }

        private async Task QueryNtpWithBackoff(double maxDelaySeconds = 32)
        {
            //Using a custom host from the settings, instead of the pre-defined list.
            if (!_customNtpHost.IsNullOrEmpty())
            {

                if (!string.IsNullOrEmpty(_customNtpHost) && !_customNtpHost.ToLower().EndsWith("pool.ntp.org"))
                    throw new ArgumentException("The CustomNtpHost must end with `pool.ntp.org`.");

                ntpHosts.Clear();
                ntpHosts.Add(_customNtpHost);
            }

            var delay = TimeSpan.FromSeconds(1);
            var currentNtpHostIndex = 1;
            while (true)
            {
                var ntpHpst = ntpHosts[currentNtpHostIndex - 1];
                try
                {
                    _ntpClient = new(ntpHpst);
                    _logger.LogInformation($"NTP Querying host {ntpHpst}");
                    _ntpClient.Query();
                    _logger.LogInformation($"Connected to {ntpHpst}. NTP Time: {Timestamp}");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"NTP Query to host {ntpHpst} failed");

                    var startNewCycle = currentNtpHostIndex == ntpHosts.Count();

                    currentNtpHostIndex = startNewCycle ? 1 : currentNtpHostIndex + 1;

                    if (startNewCycle)
                    {
                        _logger.LogInformation($"Trying again a NTP connection in {delay.TotalSeconds} seconds.\r\n{ex.Message}");
                        await Task.Delay(delay);
                        delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, maxDelaySeconds));
                    }
                }
            }
        }

    }

    internal class CallbackContainer
    {
        //public Guid Id { get; } = Guid.NewGuid(); // Unique identifier for the callback
        public Func<BrokerMessage, Task> Callback { get; set; }
        public CallbackContainer(Func<BrokerMessage, Task> callback)
        {
            Callback = callback;
        }
    }
}
