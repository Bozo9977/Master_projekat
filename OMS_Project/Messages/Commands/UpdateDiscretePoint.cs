using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages.Commands
{
    public class UpdateDiscretePoint : ICommand
    {
        public string Name { get; set; }
        public short Value { get; set; }
    }
}
