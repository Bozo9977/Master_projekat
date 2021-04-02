using System.ServiceModel;

namespace Common.PubSub
{
	[ServiceContract]
	public interface IPublishing
	{
		[OperationContract]
		void Publish(PubSubMessage m);
	}
}
