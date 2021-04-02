using System.ServiceModel;

namespace Common.PubSub
{
	public interface IPubSubClient
	{
		[OperationContract(IsOneWay = true)]
		void Receive(PubSubMessage m);
	}
}
