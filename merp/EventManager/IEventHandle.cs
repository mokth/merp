using System;
using System.Collections;

namespace wincom.mobile.erp
{
	public delegate void nsEventHandler(object sender, EventParam e);



	public interface IEventListener
	{
		event nsEventHandler eventHandler;
		void FireEvent(object sender, EventParam eventArgs);
		void PerformEvent(object sender, EventParam e);
	}

	public interface IEventHandle : IEventListener
	{
		void AddListener(IEventListener listener);
		void RemoveListener(IEventListener listener);
		void RemoveAllListener();
	}
}

