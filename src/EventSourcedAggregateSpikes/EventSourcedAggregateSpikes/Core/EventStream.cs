using System;
using System.Collections.Generic;

namespace EventSourcedAggregateSpikes.Core
{
    public class EventStream
    {
        public EventStream(string streamId, int version, IEnumerable<object> events)
        {
            Id = streamId;
            Version = version;
            Events = events;
        }

        public string Id { get; set; }

        public int Version { get; }

        public IEnumerable<object> Events { get; }
    }
}