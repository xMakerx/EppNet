/////////////////////////////////////////////
/// Filename: ManagedDisruptor.cs
/// Date: July 14, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using Disruptor;
using Disruptor.Dsl;

using EppNet.Logging;
using EppNet.Node;

using System;
using System.Threading.Tasks;

namespace EppNet.Processes
{

    public class ManagedDisruptor<T> : Disruptor<T>, ILoggable, INodeDescendant where T : class
    {

        public ILoggable Notify { get => this; }
        public NetworkNode Node { get; }

        /// <summary>
        /// See <see cref="Disruptor{T}.Disruptor(Func{T}, int)"/>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="eventFactory"></param>
        /// <param name="ringBufferSize"></param>
        public ManagedDisruptor(NetworkNode node, Func<T> eventFactory, int ringBufferSize) : base(eventFactory, ringBufferSize)
        {
            this.Node = node;
        }

        /// <summary>
        /// See <see cref="Disruptor{T}.Disruptor(Func{T}, int, IWaitStrategy)"/>
        /// </summary>

        public ManagedDisruptor(NetworkNode node, Func<T> eventFactory, int ringBufferSize, IWaitStrategy waitStrategy) : base(eventFactory, ringBufferSize, waitStrategy)
        {
            this.Node = node;
        }

        /// <summary>
        /// See <see cref="Disruptor{Task}.Disruptor(Func{Task}, int, TaskScheduler, ProducerType, IWaitStrategy)"/>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="eventFactory"></param>
        /// <param name="ringBufferSize"></param>
        /// <param name="taskScheduler"></param>
        /// <param name="producerType"></param>
        /// <param name="waitStrategy"></param>

        public ManagedDisruptor(NetworkNode node, Func<T> eventFactory, int ringBufferSize, TaskScheduler taskScheduler, ProducerType producerType, IWaitStrategy waitStrategy)
            : base(eventFactory, ringBufferSize, taskScheduler, producerType, waitStrategy)
        {
            this.Node = node;
        }

    }

}