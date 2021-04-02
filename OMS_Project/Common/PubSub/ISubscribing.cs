using System.ServiceModel;

namespace Common.PubSub
{
	[ServiceContract(CallbackContract = typeof(IPubSubClient))]
	public interface ISubscribing
	{
		[OperationContract(IsOneWay = true)]
		void Subscribe(ETopic topic);

		[OperationContract(IsOneWay = true)]
		void Unsubscribe(ETopic topic);

		[OperationContract(IsOneWay = true)]
		void Disconnect();
	}
}
