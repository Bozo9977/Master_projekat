using GUI.Helpers;
using Messages.Events;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.PubSubCode
{
    public class UpdateAnalogHandler : IHandleMessages<AnalogUpdated>
    {
        public Task Handle(AnalogUpdated message, IMessageHandlerContext context)
        {
            AnalogPoint p = new AnalogPoint();
            p.Name = message.Name;
            p.Value = message.Value;
            p.Address = message.Address;

            using (JSONParser jP = new JSONParser())
            {
                jP.AddAnalogPoint(p);
            }

            return Task.CompletedTask;
        }
    }
}
