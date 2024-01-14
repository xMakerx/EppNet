//////////////////////////////////////////////
/// <summary>
/// Filename: DisruptorExtensions.cs
/// Date: January 14, 2024
/// Author: Maverick Liberty
/// </summary>
//////////////////////////////////////////////

using Disruptor;
using Disruptor.Dsl;

using EppNet.Processes.Events;

using System;

using Notify = EppNet.Utilities.LoggingExtensions;

namespace EppNet.Source.Processes
{

    public static class DisruptorExtensions
    {

        /// <summary>
        /// Tries to fetch a <see cref="RingBufferEvent"/> from the Disruptor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="disruptor"></param>
        /// <param name="event">A valid <see cref="RingBufferEvent"/> instance or NULL</param>
        /// <returns>Whether or not an event was obtained</returns>

        public static bool TryGetEvent<T>(this Disruptor<T> disruptor, out T @event) where T : RingBufferEvent
        {
            RingBuffer buffer = disruptor.RingBuffer;
            bool retrieved = buffer.TryNext(out long sequenceId);
            @event = null;

            if (!retrieved)
            {
                Notify.Error($"Failed to prepare event for ring buffer. Buffer full! Increase limit from: {buffer.BufferSize}");
                return false;
            }

            try
            {
                // Great! We fetched an event!
                @event = disruptor[sequenceId];

                // Provide the event with the fetched sequence id.
                @event._Internal_Preinitialize(sequenceId);
            }
            catch (Exception e)
            {
                Notify.Error($"Failed to prepare event for ring buffer. Error: {e.Message}\nStack Trace: {e.StackTrace}");
            }

            return retrieved;
        }

        /// <summary>
        /// Tries to publish a <see cref="RingBufferEvent"/> to the Disruptor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="disruptor"></param>
        /// <param name="event"></param>
        /// <returns>Whether or not the <see cref="RingBufferEvent"/> was successfully published</returns>

        public static bool TryPublishEvent<T>(this Disruptor<T> disruptor, T @event) where T : RingBufferEvent
        {
            if (@event == null)
            {
                Notify.Error("Tried to publish a NULL event. Did you forget to obtain one?");
                return false;
            }

            if (@event.Disposed)
            {
                Notify.Error("Tried to publish a disposed event. Did you forget to initialize it?");
                return false;
            }

            try
            {
                // Let's try to publish this event.
                disruptor.RingBuffer.Publish(@event.SequenceID);
                Notify.Debug($"Published {@event.GetType().Name} with Sequence ID {@event.SequenceID}");
                return true;
            }
            catch (Exception e)
            {
                // Something went wrong somewhere
                Notify.Error($"Failed to publish event. Error: {e.Message}\nStack Trace: {e.StackTrace}");
                return false;
            }

        }

    }

}
