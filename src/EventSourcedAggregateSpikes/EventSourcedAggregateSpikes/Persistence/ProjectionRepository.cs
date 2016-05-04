using System;
using System.Collections.Generic;
using System.Linq;
using NEventStore;

namespace EventSourcedAggregateSpikes.Persistence
{
    public abstract class ProjectionRepository<T>
    {
        private readonly Func<string, IEnumerable<object>, T> factoryFunc;

        protected IStoreEvents EventStore { get; }

        protected ProjectionRepository(IStoreEvents eventStore, Func<string, IEnumerable<object>, T> factoryFunc)
        {
            EventStore = eventStore;
            this.factoryFunc = factoryFunc;
        }

        public T Get(string streamId, int fromRevision = int.MinValue, int toRevision = int.MaxValue)
        {
            using (var stream = EventStore.OpenStream(Bucket.Default, streamId, fromRevision, toRevision))
            {
                return factoryFunc(streamId, stream.CommittedEvents.Select(x => x.Body));
            }
        }
    }
}