using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System;
using System.Text;

namespace MqttLibrary
{
    public class SubscriberManager : ClientBase
    {
        public SubscriberManager(ManagedMqttClient client, MqttConfig config) : base(client, config) { }
        public SubscriberManager(MqttConfig config) : base(config) { }
        /// <summary>
        /// Add a handler to a topic in delegate
        /// </summary>
        /// <param name="_topic"></param>
        /// <param name="_action"></param>
        public void AddHandler(
            string _topic, 
            Func<MqttApplicationMessage, Task> _action
        )
        {
            Handlers.Add(_topic, _action);
        } 
        
        /// <summary>
        /// Start the program loop without rpc support
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public async Task StartAsync()
        {
            _client.ApplicationMessageReceivedAsync += Client_ApplicationMessageReceivedAsync;
            _client.ApplicationMessageReceivedAsync += Client_ConnectedAsync;

            await ConnectAsync();
            await SubscribeAsync(Handlers.Keys.ToArray());
        }

        /// <summary>
        /// Start the program loop with rpc support
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task Client_ConnectedAsync(EventArgs args)
        {
            await LogAsync!.Invoke("Connected to broker. Waiting for commands ...", LogType.System);
        }
        /// <summary>
        /// Handle the incoming message
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Obsolete]
        private async Task Client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            string topic = arg.ApplicationMessage.Topic;

            if (topic.StartsWith("MQTTnet.RPC/")) return;

            var type = arg.ApplicationMessage.ContentType;
            string payloadString = Encoding.ASCII.GetString(arg.ApplicationMessage.Payload);

            if (type == "string" || type == "json")
                LogAsync?.Invoke($"Topic: {topic} | Payload: {payloadString}", LogType.Command);
            else
                await LogAsync!.Invoke(topic, LogType.Command);

            try
            {
                Handlers[topic]?.Invoke(arg.ApplicationMessage);
            }
            catch (System.Exception e)
            {

                // catch any exception so that program does not crash
                await LogAsync!.Invoke(e.Message, LogType.Error);
            }
        }
        /// <summary>
        ///  Subscirbe to a single topic
        /// </summary>
        /// <param name="_topic"></param>
        /// <param name="_qos"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(
            string _topic,
            MqttQualityOfServiceLevel _qos = MqttQualityOfServiceLevel.AtLeastOnce
        )
        {
            var topicFilter = new MqttTopicFilterBuilder()
                 .WithTopic(_topic)
                 .WithQualityOfServiceLevel(_qos)
                 .Build();
            await _client.SubscribeAsync(new[] { topicFilter });
        }
        /// <summary>
        /// Subscribe to multiple normal topics
        /// </summary>
        /// <param name="_topics"></param>
        /// <param name="_qos"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(
            string[] _topics,
            MqttQualityOfServiceLevel _qos = MqttQualityOfServiceLevel.AtLeastOnce
        )
        {
            List<MqttTopicFilter> topicFilters = new();
            foreach (var topic in _topics)
            {
                topicFilters.Add(new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(_qos)
                    .Build());
            }
            await _client.SubscribeAsync(topicFilters);
        }
        /// <summary>
        /// Unsubscribe from a topic
        /// </summary>
        /// <param name="_topic"></param>
        /// <returns></returns>
        public async Task UnsubscribeAsync(
            string _topic
        )
        {
            await _client.UnsubscribeAsync(_topic);
        }


        #region Subcriber RPC topic
        /// <summary>
        /// Add a RPC handler to a topic in delegate
        /// </summary>
        /// <param name="_topic"></param>
        /// <param name="_action"></param>
        public void AddRPCHandler(
            string _topic, 
           Func<MqttApplicationMessage, byte[]> _action
        )
        {
            RpcHandlers.Add(_topic, _action);
        }
        /// <summary>
        /// Start the program loop with rpc support
        /// </summary>
        /// <param name="_qos"></param>
        /// <returns></returns>
        [Obsolete]
        public async Task StartAsnycWithRPC(
            MqttQualityOfServiceLevel _qos = MqttQualityOfServiceLevel.AtMostOnce
        )
        {
            _client.ApplicationMessageReceivedAsync += Client_ApplicationMessageReceivedAsync;
            _client.ApplicationMessageReceivedAsync += Client_ApplicationMessageReceivedRpcAsync;
            _client.ConnectedAsync += Client_ConnectedAsync;

            await ConnectAsync();

            await SubscribeAsync(Handlers.Keys.ToArray());
            await SubscribeRpcAsync(RpcHandlers.Keys.ToArray());
        }
        /// <summary>
        /// Handle the incoming rpc message
        /// </summary>
        /// <param name="_arg"></param>
        /// <returns></returns>
        private async Task Client_ApplicationMessageReceivedRpcAsync(
            MqttApplicationMessageReceivedEventArgs _arg)
        {
            var topic = _arg.ApplicationMessage.Topic;
            if(!topic.StartsWith("MQTTnet.RPC/")) return;
            var index = topic.LastIndexOf('/');
            var command = topic[(index + 1)..];

            var handler = RpcHandlers[command];
            if (handler != null) {
                var responsePayload = handler?.Invoke(_arg.ApplicationMessage);
                var responseTopic = $"{topic}/response";
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(responseTopic)
                    .WithPayload(responsePayload)
                    .Build();

                await _client.EnqueueAsync(message);
            }
        }
        /// <summary>
        ///  Subscribe to multiple rpc topics
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="qos"></param>
        /// <returns></returns>
        public async Task SubscribeRpcAsync(string[] commands, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.ExactlyOnce)
        {
            List<MqttTopicFilter> rpcFilters = new();
            foreach (var command in commands)
            {
                rpcFilters.Add(new MqttTopicFilterBuilder()
                    .WithTopic($"MQTTnet.RPC/+/{command}")
                    .WithQualityOfServiceLevel(qos)
                    .Build());
            }
            await _client.SubscribeAsync(rpcFilters);
        }
        #endregion

    }

}
