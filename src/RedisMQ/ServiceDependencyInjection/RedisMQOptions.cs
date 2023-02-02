using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using RedisMQ.Messages;
using StackExchange.Redis;

namespace RedisMQ
{
    /// <summary>
    /// 初始化配置
    /// </summary>
    public class RedisMQOptions
    {

        public RedisMQOptions()
        {
            DefaultGroupName =  Assembly.GetEntryAssembly()?.GetName().Name.ToLower();
            JsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = false
            };
        }
        /// <summary>
        /// System.Text.Json序列化配置
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; set; } 
        
        /// <summary>
        ///  StackExchange.Redis 配置
        /// </summary>
        public ConfigurationOptions? Configuration { get; set; }

        /// <summary>
        ///  每次从Redis拉取的消息数量
        /// </summary>
        public uint StreamEntriesCount { get; set; } = 10;

        /// <summary>
        ///  Redis连接池数量
        /// </summary>
        public uint ConnectionPoolSize { get; set; } = 10;
        /// <summary>
        /// Topic命名前缀
        /// </summary>
        public string? TopicNamePrefix { get; set; } = "redismq:topic";
        /// <summary>
        /// 消费者线程数量
        /// </summary>
        public int ConsumerThreadCount { get; set; } = 5;

        public string? GroupNamePrefix { get; set; } = "redismq:group";
        public string DefaultGroupName { get; set; }
        public string Version { get; set; } = "v1";
        // 多少个线程用于发送消息
        public int ProducerThreadCount { get; set; } = 1;

        /// <summary>
        /// 扩展 不要手动添加！
        /// </summary> 
        /// <value>The extensions.</value>
        public IList<IRedisMQOptionsExtension> Extensions { get; } = new List<IRedisMQOptionsExtension>();
        
        /// <summary>
        /// 失败重试次数 超过此次数将会转移到死信队列
        /// 当为0时不会转移到死信队列
        /// </summary>
        public uint FailedRetryCount { get; set; } = 3;

        /// <summary>
        /// 失败重试间隔时间 有一定延迟 单位秒
        /// 重试与<see cref="FailedRetryInterval"/>属性配合使用,两者取最大值为重试间隔
        /// </summary>
        public int FailedRetryInterval { get; set; } = 10;
        
        /// <summary>
        /// 消费消息时加锁时间,单位秒
        /// 会影响重试间隔
        /// </summary>
        public int LockMessageSecond { get; set; } = 10;
        /// <summary>
        /// 超过<see cref="FailedRetryCount"/>次数后将会通知
        /// </summary>
        public Action<TransportMessage> FailedThresholdCallback { get; set; }

        /// <summary>
        /// 每个Topic使用队列的最大的长度，超出后队列最前面的消息将会被删除
        /// </summary>
        public int MaxQueueLength { get; set; } = 100_000;
    }
}