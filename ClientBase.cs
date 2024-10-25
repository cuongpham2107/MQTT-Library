
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
namespace MqttLibrary
{
    public enum LogType { Information, Error, System, Command }
    public class ClientBase
    {
        protected readonly ManagedMqttClient _client;
        protected readonly MqttConfig _config;
        public ManagedMqttClient Client => _client;
        public ClientBase(ManagedMqttClient client, MqttConfig config)
        {
            _client = client;
            _config = config;
        }
        public ClientBase(MqttConfig config)
        {
            _config = config;
            _client = (ManagedMqttClient)new MqttFactory().CreateManagedMqttClient();
        }
        
        /// <summary>
        /// Delegate Log a message
        /// </summary>
        public Func<string, LogType, Task>? LogAsync;
        /// <summary>
        /// Handlers for each topic
        /// </summary>
        public Dictionary<string, Func<MqttApplicationMessage, Task>> Handlers { get; set; } = new();
        /// <summary>
        /// RPC Handlers for each topic
        /// </summary>
        public Dictionary<string, Func<MqttApplicationMessage, byte[]>> RpcHandlers { get; set; } = new();
        public async Task ConnectAsync()
        {
            var optionBuilder = new MqttClientOptionsBuilder()
                    .WithTcpServer(_config.URL, _config.PORT)
                    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                    .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                    .WithCleanSession();
            var options = optionBuilder.Build();
            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(options)
                .Build();
            await _client.StartAsync(managedOptions);
        }
        public async Task DisconnectAsync()
        {
            await _client.StopAsync();
        }
    }
}