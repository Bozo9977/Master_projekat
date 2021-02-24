using NServiceBus;

namespace Messages.Events
{
    public class AnalogUpdated : IEvent
    {
        public string Name { get; set; }
        public float Value { get; set; }
        public ushort Address { get; set; }
    }
}
