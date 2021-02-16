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
    public class UpdateAnalogHandler : IHandleMessages<UpdateAnalogPoint>
    {
        public Task Handle(UpdateAnalogPoint message, IMessageHandlerContext context)
        {
            var analogUpdated = new AnalogUpdated
            {
                Name = message.Name,
                Value = message.Value
            };

            return context.Publish(analogUpdated);
        }
    }
}
