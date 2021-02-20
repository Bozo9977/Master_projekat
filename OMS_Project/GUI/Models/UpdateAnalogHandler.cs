using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messages.Commands;
using Messages.Events;
using NServiceBus.Logging;

namespace GUI.Models
{
    public class UpdateAnalogHandler : IHandleMessages<AnalogUpdated>
    {
        public Task Handle(AnalogUpdated message, IMessageHandlerContext context)
        {
            

            return context.Publish(message);
        }
    }
}
