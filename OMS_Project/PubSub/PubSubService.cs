using Common.PubSub;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PubSub
{
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	class PubSubService : IPublishing, ISubscribing
	{
		static Dictionary<IPubSubClient, List<ETopic>> clients = new Dictionary<IPubSubClient, List<ETopic>>();

		public void Publish(PubSubMessage m)
		{
			lock(clients)
			{
				foreach(KeyValuePair<IPubSubClient, List<ETopic>> client in clients)
				{
					if(!client.Value.Contains(m.Topic))
					{
						continue;
					}

					try
					{
						client.Key.Receive(m);
					}
					catch(Exception e)
					{ }
				}
			}
		}

		public void Subscribe(ETopic topic)
		{
			IPubSubClient client = OperationContext.Current.GetCallbackChannel<IPubSubClient>();
			List<ETopic> topics;

			lock(clients)
			{
				if(clients.TryGetValue(client, out topics))
				{
					if(!topics.Contains(topic))
					{
						topics.Add(topic);
					}
				}
				else
				{
					clients.Add(client, new List<ETopic>() { topic });
				}
			}
		}

		public void Unsubscribe(ETopic topic)
		{
			IPubSubClient client = OperationContext.Current.GetCallbackChannel<IPubSubClient>();
			List<ETopic> topics;

			lock(clients)
			{
				if(clients.TryGetValue(client, out topics))
				{
					topics.Remove(topic);

					if(topics.Count == 0)
					{
						clients.Remove(client);
					}
				}
			}
		}

		public void Disconnect()
		{
			IPubSubClient client = OperationContext.Current.GetCallbackChannel<IPubSubClient>();

			lock(clients)
			{
				clients.Remove(client);
			}
		}
	}
}
