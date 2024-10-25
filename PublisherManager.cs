using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Protocol;
namespace MqttLibrary;
public class PublishManager : ClientBase {
    private readonly MqttRpcClient? rpcClient;
    public PublishManager(ManagedMqttClient client, MqttConfig config) : base(client, config) { }
    public PublishManager(MqttConfig config) : base(config) 
    {
        var rpcOptions = new MqttRpcClientOptionsBuilder().Build();
        rpcClient = new MqttRpcClient(_client.InternalClient, rpcOptions);
    }
    /// <summary>
    /// Publish a message to a topic
    /// </summary>
    /// <param name="_builder"></param>
    /// <returns></returns>
    public async Task PublishAsync(
        MqttApplicationMessageBuilder _builder
    )
    {
        await _client.EnqueueAsync(_builder.Build());
    }
    /// <summary>
    /// Publish a message to a topic
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_topic"></param>
    /// <param name="_payload"></param>
    /// <param name="_retainFlag"></param>
    /// <param name="_qos"></param>
    /// <returns></returns>
    public async Task PublishObjectAsync<T>(
        string _topic,
        T _payload,
        bool _retainFlag = false, 
        MqttQualityOfServiceLevel _qos = MqttQualityOfServiceLevel.AtMostOnce
    )
    {
        var jsonPayload = JsonSerializer.Serialize(_payload);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(jsonPayload)
            .WithContentType("json")
            .WithQualityOfServiceLevel(_qos)
            .WithRetainFlag(_retainFlag)
            .Build();
        await _client.EnqueueAsync(message);
    }
    /// <summary>
    /// Publish a message to a topic
    /// </summary>
    /// <param name="_topic"></param>
    /// <param name="_retainFlag"></param>
    /// <param name="_qos"></param>
    /// <returns></returns>
    public async Task PublishNoPayloadAsync(
        string _topic, 
        bool _retainFlag = false, 
        MqttQualityOfServiceLevel _qos = MqttQualityOfServiceLevel.AtMostOnce
    ) 
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(new byte[] { 0 })
            .WithContentType("0")
            .WithQualityOfServiceLevel(_qos)
            .WithRetainFlag(_retainFlag)
            .Build();
        await _client.EnqueueAsync(message);
    }
    /// <summary>
    /// Publish a binary payload to a topic
    /// </summary>
    /// <param name="_topic"></param>
    /// <param name="_payload"></param>
    /// <param name="_retainFlag"></param>
    /// <param name="_qos"></param>
    /// <returns></returns>
    public async Task PublishBinaryAsync(
        string _topic, 
        byte[] _payload, 
        bool _retainFlag = false, 
        MqttQualityOfServiceLevel _qos = MqttQualityOfServiceLevel.AtLeastOnce
    ) 
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(_payload)
            .WithContentType("bytes")
            .WithQualityOfServiceLevel(_qos)
            .WithRetainFlag(_retainFlag)
            .Build();
        await _client.EnqueueAsync(message);
    }
    /// <summary>
    /// Publish a string payload to a topic
    /// </summary>
    /// <param name="_topic"></param>
    /// <param name="_payload"></param>
    /// <param name="_retainFlag"></param>
    /// <param name="_qos"></param>
    /// <returns></returns>
    public async Task PublishStringAsync(
        string _topic, 
        string _payload, 
        bool _retainFlag = false, 
        MqttQualityOfServiceLevel _qos = MqttQualityOfServiceLevel.AtLeastOnce
    ) 
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(_payload)
            .WithContentType("string")
            .WithQualityOfServiceLevel(_qos)
            .WithRetainFlag(_retainFlag)
            .Build();
        await _client.EnqueueAsync(message);
    }

    #region Publisher RPC topic
    /// <summary>
    /// Send rpc call with string payload
    /// </summary>
    /// <param name="_topic"></param>
    /// <param name="_payload"></param>
    /// <param name="_secondsTimeout"></param>
    /// <param name="_qos"></param>
    /// <returns></returns>
    public async Task<byte[]> RpcCallString(
        string _topic, 
        string _payload = " ", 
        int _secondsTimeout = 5,
        MqttQualityOfServiceLevel _qos =  MqttQualityOfServiceLevel.ExactlyOnce
    )
    {
        var timeout = TimeSpan.FromSeconds(_secondsTimeout);
        return await rpcClient!.ExecuteAsync(timeout, _topic, _payload,_qos);
    }
    /// <summary>
    /// Send rpc call with byta array payload
    /// </summary>
    /// <param name="_topic"></param>
    /// <param name="_payload"></param>
    /// <param name="_secondsTimeout"></param>
    /// <param name="_qos"></param>
    /// <returns></returns>
    public async Task<byte[]> RpcCallBinary(
        string _topic, 
        byte[] _payload, 
        int _secondsTimeout = 5,
        MqttQualityOfServiceLevel _qos =  MqttQualityOfServiceLevel.ExactlyOnce
    )
    {
         var timeout = TimeSpan.FromSeconds(_secondsTimeout);
        return await rpcClient!.ExecuteAsync(timeout, _topic, _payload, MqttQualityOfServiceLevel.ExactlyOnce);
    }
    /// <summary>
    /// Send rpc call without payload (send a zero byte as payload)
    /// </summary>
    /// <param name="_topic"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public async Task<byte[]> RpcCallNoPayload(
        string _topic, 
        int _secondsTimeout = 5,
        MqttQualityOfServiceLevel _qos =  MqttQualityOfServiceLevel.ExactlyOnce
    ) 
    {
        var timeout = TimeSpan.FromSeconds(_secondsTimeout);
        return await rpcClient!.ExecuteAsync(timeout, _topic, new byte[] { 0 }, _qos);
    }

    
    #endregion
}