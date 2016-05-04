using System;
using System.Collections.Generic;

namespace EventSourcedAggregateSpikes.Persistence
{
    public abstract class ProjectionRepository<T>
    {
        private readonly Func<string, IEnumerable<object>, T> factoryFunc;

        protected ProjectionRepository(Func<string, IEnumerable<object>, T> factoryFunc)
        {
            this.factoryFunc = factoryFunc;
        }

        public T Get(string streamId)
        {
            // TODO: Get events from event store
            var events = new object[] { };

            return factoryFunc(streamId, events);
        }
    }
}