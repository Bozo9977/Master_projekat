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
    public class UpdateDiscreteHandler : IHandleMessages<DiscreteUpdated>
    {
        public Task Handle(DiscreteUpdated message, IMessageHandlerContext context)
        {
            using (JSONParser jP = new JSONParser())
            {
                jP.AddDiscretePoint(message);
            }

            return context.Reply("Ok");
        }
    }
}
