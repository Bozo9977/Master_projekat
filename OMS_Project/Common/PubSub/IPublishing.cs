using System.ServiceModel;

namespace Common.PubSub
{
	[ServiceContract]
	public interface IPublishing
	{
		[OperationContract(IsOneWay = true)]
		void Publish(PubSubMessage m);
	}
}
