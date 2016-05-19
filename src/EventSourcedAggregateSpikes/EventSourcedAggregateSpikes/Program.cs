using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcedAggregateSpikes.Core;

namespace EventSourcedAggregateSpikes
{
    class Program
    {
        static void Main()
        {
            // Imagine this is a standard message handler implementation
            var eventStore = new StubEventStore();
            var stream = eventStore.Load("ORDER-1");

            OrderAggregate agg = new OrderAggregate(stream.Events);
            agg.AckowledgeOrderPlaced();

            WriteUncommittedEvents(agg);

            // Could wrap the store in a repository if it helps, but it would mean embedding version & stream id in the aggregate.
            // This would just be a compositional concern though.
            eventStore.Append(stream.Id, stream.Version, agg.UncommittedEvents); 
            
            Console.ReadLine();
        }

        private static void WriteUncommittedEvents(OrderAggregate agg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Unapplied events:");
            Console.ForegroundColor = ConsoleColor.White;

            foreach (var e in agg.UncommittedEvents)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class StubEventStore : IEventStore
    {
        public EventStream Load(string streamId)
        {
            return new EventStream(streamId, 0, new object[] {});
        }

        public void Append(string streamId, int expectedVersion, IEnumerable<object> uncommittedEvents)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Appended {uncommittedEvents.Count()} event(s) to stream {streamId}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
