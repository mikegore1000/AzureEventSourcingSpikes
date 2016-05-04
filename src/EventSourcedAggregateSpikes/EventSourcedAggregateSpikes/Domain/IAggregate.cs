using System.Collections.Generic;

namespace EventSourcedAggregateSpikes.Domain
{
    public interface IAggregate
    {
        string StreamId { get; }

        IEnumerable<object> UnappliedEvents { get; }

        int Version { get; }
    }
}