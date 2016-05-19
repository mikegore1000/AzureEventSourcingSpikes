using System.Collections.Generic;

namespace EventSourcedAggregateSpikes.Core
{
    public interface IEventStore
    {
        EventStream Load(string streamId);

        void Append(string streamId, int expectedVersion, IEnumerable<object> uncommittedEvents);
    }
}