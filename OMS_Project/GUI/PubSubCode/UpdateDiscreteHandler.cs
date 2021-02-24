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
            //create a point
            DiscretePoint p = new DiscretePoint();
            p.Name = message.Name;
            p.Value = message.Value;
            p.Address = message.Address;


            using (JSONParser jP = new JSONParser())
            {
                jP.AddDiscretePoint(p);
            }

            return context.Reply("Ok");
        }
    }
}
