using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Common.WCF
{
	public class DuplexClient<IContract, ICallback>
	{
		DuplexChannelFactory<IContract> factory;
		object instance;
		public string EndpointName { get; private set; }

		public DuplexClient(string endpointName, object callbackInstance)
		{
			EndpointName = endpointName;
			instance = callbackInstance;
		}

		public bool Connect()
		{
			try
			{
				factory = new DuplexChannelFactory<IContract>(instance, EndpointName);
			}
			catch(Exception e)
			{
				return false;
			}

			return true;
		}

		public bool Disconnect()
		{
			try
			{
				factory.Close();
			}
			catch(Exception e)
			{
				return false;
			}

			return true;
		}

		public bool Call<TReturn>(Func<IContract, TReturn> f, out TReturn result)
		{
			result = default;

			try
			{
				result = f(factory.CreateChannel());
			}
			catch(Exception e)
			{
				return false;
			}

			return true;
		}
	}
}
