using System.Collections.Generic;
using EventSourcedAggregateSpikes.Core;

namespace EventSourcedAggregateSpikes
{
    public class OrderAggregate
    {
        private readonly List<object> uncommittedEvents = new List<object>();

        private bool orderPlaced;
        private bool orderAccepted;
        private bool fraudCheckPassed;

        public OrderAggregate(IEnumerable<object> events)
        {
            foreach (var e in events)
            {
                Apply((dynamic)e);
            }
        }

        #region Further simplified the "framework"

        public IEnumerable<object> UncommittedEvents => uncommittedEvents.AsReadOnly();

        private void Raise(object @event)
        {
            Apply((dynamic)@event);
            uncommittedEvents.Add(@event);
        }

        #endregion

        // Not a great aggregate method, but just highlighting all the framework parts
        public void AckowledgeOrderPlaced()
        {
            if (!orderPlaced)
            {
                Raise(new OrderPlaced());
            }
        }

        // C# 6 makes the projection methods less tedious, however there could still be many events.
        // Whether we need a memento or anything else though should be context sensitive - if we have a basic aggregate
        // with few events to project then why bother with a memento?  Worth pointing out mementos though are very good if snapshots
        // are used.
        private void Apply(OrderPlaced e) => orderPlaced = true;

        private void Apply(OrderAccepted e) => orderAccepted = true;

        private void Apply(FraudCheckPassed e) => fraudCheckPassed = true;

        private void Apply(FraudCheckBypassed e) => fraudCheckPassed = true;

        private void Apply(FraudCheckFailed e) => fraudCheckPassed = false;
    }
}