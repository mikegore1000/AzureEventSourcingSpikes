using System;
using System.Collections.Generic;
using EventSourcedAggregateSpikes.Domain;
using NEventStore;

namespace EventSourcedAggregateSpikes.Persistence
{
    public abstract class AggregateRepository<T> : ProjectionRepository<T> where T : IAggregate
    {
        protected AggregateRepository(IStoreEvents eventStore, Func<string, IEnumerable<object>, T> factoryFunc) : base(eventStore, factoryFunc)
        {
        }

        public void Save(T aggregate)
        {
            using (var stream = GetEventStreamForSave(aggregate))
            {
                AppendUncomittedEventsToStream(aggregate, stream);
                SaveChanges(stream);
            }
        }

        private static void SaveChanges(IEventStream stream)
        {
            try
            {
                stream.CommitChanges(Guid.NewGuid());
            }
                // TODO: Make the error handling more robust
            catch
            {
                stream.ClearChanges();
                throw;
            }
        }

        private static void AppendUncomittedEventsToStream(T aggregate, IEventStream stream)
        {
            foreach (var e in aggregate.UnappliedEvents)
            {
                stream.Add(new EventMessage { Body =  e});
            }
        }

        // TODO: More efficient to make the repository transient so we can keep hold of instances we've already pulled out to avoid another ES query
        // NEventStore doesn't allow you to just append without loading a stream
        // Another approach would be that the aggregate just holds onto the underlying stream - however this pollutes the aggregate with our
        // event sourcing implementation specifics.
        // TODO: Check this will actually kick off the optimistic concurrency properly too!  Would be good to see how our current approach handles this
        private IEventStream GetEventStreamForSave(T aggregate)
        {
            return EventStore.OpenStream(Bucket.Default, aggregate.StreamId, 0, aggregate.Version);
        }
    }
}