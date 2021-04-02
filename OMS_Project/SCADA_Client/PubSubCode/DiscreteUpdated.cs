using NServiceBus;

namespace Messages.Events
{
    public class DiscreteUpdated : IEvent
    {
        public string Name { get; set; }
        public short Value { get; set; }
        public ushort Address { get; set; }
    }
}
