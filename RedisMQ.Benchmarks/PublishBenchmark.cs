using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using RedisMQ;
using RedisMQ.Benchmarks;
using RedisMQ.Messages;
using RedisMQ.RedisStream;
using RedisMQ.Serialization;
using RedisMQ.Serialization.MessagePack;
using RedisMQ.Transport;
using StackExchange.Redis;

[MemoryDiagnoser]
public class TestBenchmarks
{
    private ServiceProvider _provider;
    private IRedisPublisher _publisher;
    private IDispatcher _dispacher;
    private ISerializer _serializer;
    private string _rawData;
    private static ConnectionMultiplexer _redisClient = ConnectionMultiplexer.Connect("localhost:6379");

    private static ConnectionMultiplexer[] _connections = new ConnectionMultiplexer[5]
    {
        ConnectionMultiplexer.Connect("localhost:6379"), ConnectionMultiplexer.Connect("localhost:6379"),
        ConnectionMultiplexer.Connect("localhost:6379"), ConnectionMultiplexer.Connect("localhost:6379"),
        ConnectionMultiplexer.Connect("localhost:6379")
    };

    private ServiceCollection _services;
    private ServiceProvider _providerWithMsgPack;
    private IRedisPublisher _publisherWithMsgPack;
    private ISerializer _serializerMsgPack;


    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceCollection();
        _services.AddRedisMQ(action =>
        {
            action.Configuration = ConfigurationOptions.Parse("localhost:6379");
        });
        _services.AddLogging();
        _provider = _services.BuildServiceProvider();
        _publisher = _provider.GetRequiredService<IRedisPublisher>();
        _dispacher = _provider.GetRequiredService<IDispatcher>();
        _serializer = _provider.GetRequiredService<ISerializer>();
        _rawData = _serializer.Serialize(new Message(new Dictionary<string, string?>(), new TestTransDto()));

        
        var _services2 = new ServiceCollection();
        _services2.AddRedisMQ(action =>
        {
            action.Configuration = ConfigurationOptions.Parse("localhost:6379");
            action.UseMessagePack();
        });
        _services2.AddLogging();
        _providerWithMsgPack=_services2.BuildServiceProvider();
        _publisherWithMsgPack= _providerWithMsgPack.GetRequiredService<IRedisPublisher>();
        _serializerMsgPack= _providerWithMsgPack.GetRequiredService<ISerializer>();
    }

    // [Benchmark]
    // public void SerializeTest()
    // {
    //     var msg=new Message();
    //     msg.Headers.Add("test","test");
    //     msg.Headers.Add("test2","test2");
    //     msg.Headers.Add("test3","test3");
    //     msg.Value = new TestTransDto();
    //     var _=_serializer.Serialize( msg);
    // }
    // [Benchmark]
    // public void SerializeMsgPackTest()
    // {
    //     var msg=new Message();
    //     msg.Headers.Add("test","test");
    //     msg.Headers.Add("test2","test2");
    //     msg.Headers.Add("test3","test3");
    //     msg.Value = new TestTransDto();
    //     var _=_serializerMsgPack.Serialize( msg);
    // }
    [Benchmark()]
    public void Publish_1000()
    {
        List<Task> tasks = new();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(_publisher.PublishAsync("test2", new TestTransDto()));
        }

        _publisher.PublishAsync("test2", new TestTransDto());
        Task.WaitAll(tasks.ToArray());
    }
    [Benchmark]
    public void Publish_1000_MessagePack()
    {
        List<Task> tasks = new();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(_publisherWithMsgPack.PublishAsync("test_msgpack", new TestTransDto()));
        }
    
        Task.WaitAll(tasks.ToArray());
    }
    // [Benchmark]
    public void Publish_WithoutSerialization_1000()
    {
        IRedisStreamManager redisStreamManager = _provider.GetRequiredService<IRedisStreamManager>();
        NameValueEntry[] valueEntry = new NameValueEntry[1] { new NameValueEntry("body", _rawData) };
    
        List<Task> tasks = new();
        for (int i = 0; i < 1000; i++)
        {
            redisStreamManager.PublishAsync("test2", valueEntry);
        }
    
        Task.WaitAll(tasks.ToArray());
    }
    
    [Benchmark(Baseline = true)]
    public void StreamAdd_1000()
    {
        
        List<Task> t = new();
        for (int i = 0; i < 1000; i++)
        {
            t.Add(_redisClient.GetDatabase().StreamAddAsync("test_stream", "foo_name", "bar_value"));
        }
    
        Task.WaitAll(t.ToArray());
    }
    [Benchmark]
    public void StreamAdd_1000_Json()
    {
        List<Task> t = new();
       
        for (int i = 0; i < 1000; i++)
        {
            var msg=new Message();
            msg.Headers.Add("test","test");
            msg.Headers.Add("test2","test2");
            msg.Headers.Add("test3","test3");
            msg.Value = new TestTransDto();
            var json=_serializer.Serialize( msg);
            t.Add(_redisClient.GetDatabase().StreamAddAsync("test_stream", "foo_name", json));
        }
    
        Task.WaitAll(t.ToArray());
    }
    [Benchmark]
    public void StreamAdd_1000_MsgPack()
    {
        List<Task> t = new();
       
        for (int i = 0; i < 1000; i++)
        {
            var msg=new Message();
            msg.Headers.Add("test","test");
            msg.Headers.Add("test2","test2");
            msg.Headers.Add("test3","test3");
            msg.Value = new TestTransDto();
            var json=_serializerMsgPack.Serialize( msg);
            t.Add(_redisClient.GetDatabase().StreamAddAsync("test_stream", "foo_name", json));
        }
    
        Task.WaitAll(t.ToArray());
    }
   
}