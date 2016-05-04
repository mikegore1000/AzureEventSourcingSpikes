using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace EventSourcedAggregateSpikes
{
    class Program
    {
        static void Main()
        {
            var stream = new object[] { };

            // Just comment/uncomment the aggregate approach you want to run...
            // SampleBasicAggregate agg = new SampleBasicAggregate(stream);
            SampleBasicAggregateWithMemento agg = new SampleBasicAggregateWithMemento(stream);

            agg.AckowledgeOrderPlaced();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Unapplied events:");
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var e in agg.UnappliedEvents)
            {
                Console.WriteLine(e);
            }
            
            Console.ReadLine();
        }
    }

    // Projection should only allow reads, aggregates allow reads & writes
    public abstract class ProjectionRepository<T>
    {
        private readonly Func<IEnumerable<object>, T> factoryFunc;

        protected ProjectionRepository(Func<IEnumerable<object>, T> factoryFunc)
        {
            this.factoryFunc = factoryFunc;
        }

        public T Get(string streamId)
        {
            // TODO: Get events from event store
            var events = new object[] { };

            return this.factoryFunc(events);
        }
    }

    public abstract class AggregateRepository<T> : ProjectionRepository<T> where T : IAggregate
    {
        protected AggregateRepository(Func<IEnumerable<object>, T> factoryFunc) : base(factoryFunc)
        {
        }

        // TODO: May make more sense for the aggregate to know it's own id
        public void Save(string streamId, T aggregate)
        {
            // TODO: Update stream - would possibly want some way of updating the version in the aggregate, though this would suggest that its possible to save again (not the best idea)
        }
    }

    // Only way to remove the boilerplate for each repository here is to ensure we have a consistent, public method for applying events to the stream (could make this
    // inherit from the interface in an explicit way - would have to assume the constructor had no params though, which is the nice thing about taking in the event stream
    public class SampleBasicAggregateRepository : AggregateRepository<SampleBasicAggregate>
    {
        public SampleBasicAggregateRepository() : base(events => new SampleBasicAggregate(events))
        {
        }
    }

    public interface IAggregate
    {
        IEnumerable<object> UnappliedEvents { get; }

        int Version { get; }
    }

    // NOTE: Do we even need a base class for something so basic?  Apart from tracking the current stream revision and
    // unapplied events there isn't really anything else to do.  Why abstract this into a NuGet package?
    public class SampleBasicAggregate : IAggregate
    {
        private bool orderPlaced;
        private bool orderAccepted;
        private bool fraudCheckPassed;
        
        #region "Framework" bits

        private readonly List<object> unappliedEvents = new List<object>();

        public SampleBasicAggregate(IEnumerable<object> events)
        {
            foreach (var e in events)
            {
                When((dynamic)e);
                Version++;
            }
        }

        public IEnumerable<object> UnappliedEvents => unappliedEvents.AsReadOnly();

        public int Version { get; }
    
        private void Apply(object @event)
        {
            unappliedEvents.Add(@event);
            When((dynamic)@event);
        }

        #endregion

        // Not a great aggregate method, but just highlighting all the framework parts
        public void AckowledgeOrderPlaced()
        {
            if (!orderPlaced)
            {
                Apply(new OrderPlaced());
            }
        }

        // C# 6 makes the projection methods less tedious, however there could still be many events.
        // Whether we need a memento or anything else though should be context sensitive - if we have a basic aggregate
        // with few events to project then why bother with a memento?  Worth pointing out mementos though are very good if snapshots
        // are used.
        public void When(OrderPlaced e) => orderPlaced = true;

        public void When(OrderAccepted e) => orderAccepted = true;

        public void When(FraudCheckPassed e) => fraudCheckPassed = true;

        public void When(FraudCheckBypassed e) => fraudCheckPassed = true;

        public void When(FraudCheckFailed e) => fraudCheckPassed = false;
    }

    public class SampleBasicAggregateWithMemento : IAggregate
    {
        private readonly BasicMemento state = new BasicMemento();

        #region "Framework" bits

        private readonly List<object> unappliedEvents = new List<object>();

        public SampleBasicAggregateWithMemento(IEnumerable<object> events)
        {
            foreach (var e in events)
            {
                state.When((dynamic)e);
                Version++;
            }
        }

        // Would still hold the events here, unapplied events are part of the aggregate, not the memento
        public IEnumerable<object> UnappliedEvents => unappliedEvents.AsReadOnly();

        public int Version { get; }

        private void Apply<T>(T @event)
        {
            unappliedEvents.Add(@event);
            state.When((dynamic)@event);
        }

        #endregion

        // Not a great aggregate method, but just highlighting all the framework parts
        public void AckowledgeOrderPlaced()
        {
            if (!state.OrderPlaced)
            {
                Apply(new OrderPlaced());
            }
        }
    }

    // Would want to properly model state using value objects, domain objects etc.
    internal class BasicMemento
    {
        // State
        internal bool OrderPlaced { get; private set; }
        internal bool OrderAccepted { get; private set; }
        internal bool FraudCheckPassed { get; private set; }

        // State hydration methods
        internal void When(OrderPlaced e) => OrderPlaced = true;

        internal void When(OrderAccepted e) => OrderAccepted = true;

        internal void When(FraudCheckPassed e) => FraudCheckPassed = true;

        internal void When(FraudCheckBypassed e) => FraudCheckPassed = true;

        internal void When(FraudCheckFailed e) => FraudCheckPassed = false;
    }

    #region Events

    public class OrderPlaced { }
    public class OrderAccepted { }
    public class FraudCheckPassed { }
    public class FraudCheckFailed { }
    public class FraudCheckBypassed { }
    public class PaymentIntentAdded { }
    public class PaymentIntentAccepted { }
    public class PaymentIntentGuaranteed { }
    public class BuyersRemorseExpired { }
    public class PaymentExpired { }

    #endregion
}
