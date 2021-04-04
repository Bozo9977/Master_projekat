﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	interface IObserver<TMessage>
	{
		void Notify(TMessage message);
	}
}
