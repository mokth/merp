using System;
using System.Collections.Generic;

namespace wincom.mobile.erp
{	
		public class EventManager : IEventHandle
		{
			private static List<IEventListener> listerners = new List<IEventListener>();

			public EventManager()
			{
				eventHandler += new nsEventHandler(PerformEvent);
			}

			#region IEventManager Members

			public void AddListener(IEventListener listener)
			{
				listener.eventHandler += new nsEventHandler(listener.PerformEvent);
				listerners.Add(listener);
			}

			public void RemoveListener(IEventListener listener)
			{
				try
				{
					listerners.Remove(listener);
				}
				catch { }
			}

			public void RemoveAllListener()
			{
				for (int i = 0; i < listerners.Count; i++)
				{
					RemoveListener(listerners[i]);
				}

				listerners.Clear();
			}

			#endregion

			#region IEventListerner Members

			public void FireEvent(object sender, EventParam eventArgs)
			{
				eventHandler(sender, eventArgs);
			}

			public void PerformEvent(object sender, EventParam e)
			{
				for (int i = 0; i < listerners.Count; i++)
				{
					listerners[i].FireEvent(sender, e);
				}
			}

			public event nsEventHandler eventHandler;

			#endregion


	}

}

