using System;
using System.Collections.Generic;
using EventSourcedAggregateSpikes.Domain;

namespace EventSourcedAggregateSpikes.Persistence
{
    public abstract class AggregateRepository<T> : ProjectionRepository<T> where T : IAggregate
    {
        protected AggregateRepository(Func<string, IEnumerable<object>, T> factoryFunc) : base(factoryFunc)
        {
        }

        public void Save(string streamId, T aggregate)
        {
            // TODO: Update stream - would possibly want some way of updating the version in the aggregate, though this would suggest that its possible to save again (not the best idea)
        }
    }
}