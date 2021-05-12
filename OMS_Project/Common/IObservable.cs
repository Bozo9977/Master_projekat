using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public interface IObservable<TMessage>
	{
		bool Subscribe(IObserver<TMessage> observer);
		bool Unsubscribe(IObserver<TMessage> observer);
	}
}
