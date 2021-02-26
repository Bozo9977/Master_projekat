using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Common.WCF
{
	public class Client<IContract>
	{
		ChannelFactory<IContract> factory;
		public string EndpointName { get; private set; }

		public Client(string endpointName)
		{
			EndpointName = endpointName;
		}

		public bool Connect()
		{
			try
			{
				factory = new ChannelFactory<IContract>(EndpointName);
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
