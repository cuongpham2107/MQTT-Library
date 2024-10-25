# Thư viện MQTT net 
* Là một thư viện hỗ sợ làm việc với MQTTnet một cách dễ dàng*

*** 
## How to use library

### Install
```bash
# Cài đặt package từ NuGet
dotnet add package MqttLibraryManager --version 1.0.0
```
### Publisher
```csharp
using MqttLibrary;
public class Program {    
    private static async Task Main(string[] args) {
        MqttConfig mqttConfig = new MqttConfig();
        PublishManager publisher = new(mqttConfig);
        await publisher.ConnectAsync();

        Console.WriteLine("Type Publisher: ")
        string? typePublisher = Console.ReadLine();
        if(typePublisher != "rpc") {
            while(true) {
                Console.WriteLine("Enter your topic:");
                string? topic = Console.ReadLine();
                Console.WriteLine("Enter your payload:");
                string? payload = Console.ReadLine();
                // Publish a topic
                await publisher.PublishStringAsync(topic!, payload!);
            }   
        }
        else
        {
            //Publisher RPC a topic
            while (true) {
                Console.Write("> ");
                var command = Console.ReadLine();
                try {
                    var res = await publisher.RpcCallNoPayload($"{command}");
                    await publisher.PublishNoPayloadAsync($"{command}");
                    Console.WriteLine(Encoding.ASCII.GetString(res));
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }

        }     
    }
}
```


### Subcriber

```csharp

using MqttLibrary;
public class Program {    
    
    private static async Task Main(string[] args) {
        MqttConfig mqttConfig = new MqttConfig();

        SubscriberManager subscriberManager = new(mqttConfig);

        subscriberManager.LogAsync += (s, l) => Task.Run(() => Console.WriteLine(s));

        subscriberManager.Handlers["hello"] = async e => {
            Console.WriteLine(Encoding.UTF8.GetString(e.Payload));
            await Task.CompletedTask;
        };
        subscriberManager.RpcHandlers["ping"] = e => Encoding.UTF8.GetBytes("pong");
        await subscriberManager.StartAsnycWithRPC();        
        Thread.Sleep(-1);
    }

}

```


### LogAsnyc

```csharp
/// <summary>
/// Delegate Log a message
/// </summary>
public Func<string, LogType, Task>? LogAsync;

```

### Delegate

```csharp
/// <summary>
/// Handlers for each topic
/// </summary>
public Dictionary<string, Func<MqttApplicationMessage, Task>> Handlers { get; set; } = new();
/// <summary>
/// RPC Handlers for each topic
/// </summary>
public Dictionary<string, Func<MqttApplicationMessage, byte[]>> RpcHandlers { get; set; } = new();

```

