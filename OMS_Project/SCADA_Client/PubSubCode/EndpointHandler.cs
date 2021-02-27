using Messages.Commands;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Client.PubSubCode
{
    public class EndpointHandler : IDisposable
    {
		public IEndpointInstance EndpointInstance { get; set; }


		public async Task AsyncEndpointCreate()
		{
			var endpointConfiguration = new EndpointConfiguration("SCADA_Service");

			/*
             * LearningTransport - starter transport for learning purposes
             * (other transports can be attained through nugget)
             */
			var transport = endpointConfiguration.UseTransport<LearningTransport>();

			endpointConfiguration.PurgeOnStartup(true);

			var routing = transport.Routing();

			/* Start the endpoint */
			EndpointInstance = await Endpoint.Start(endpointConfiguration)
				.ConfigureAwait(false);
		}

        public async void Dispose()
        {
			await EndpointInstance.Stop().ConfigureAwait(false);
        }
    }
}
