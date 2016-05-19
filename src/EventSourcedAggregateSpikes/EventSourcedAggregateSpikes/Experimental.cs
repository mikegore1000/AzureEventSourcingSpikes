using System;
using System.Collections.Generic;

namespace EventSourcedAggregateSpikes
{
    public class TestState
    {
        public bool OrderAccepted { get; set; }
    }

    public class TestAggregate
    {
        private Dictionary<Type, IEventApplier<TestState>> eventAppliers = new Dictionary<Type, IEventApplier<TestState>>
        {
            { typeof(OrderAccepted), new OrderAcceptedEventApplier() }
        };
    }

    internal class OrderAcceptedEventApplier : EventApplier<TestState, OrderAccepted>
    {
        protected override void Apply(TestState state, OrderAccepted @event)
        {
            state.OrderAccepted = true;
        }
    }

    internal abstract class EventApplier<TState, TEvent> : IEventApplier<TState>
    {
        public void Apply(TState state, object @event)
        {
            if (@event is TEvent)
            {
                Apply(state, (TEvent)@event);
            }
        }

        protected abstract void Apply(TState state, TEvent @event);
    }

    public interface IEventApplier<TState>
    {
        void Apply(TState state, object @event);
    }
}