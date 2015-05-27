using System;

namespace wincom.mobile.erp
{
	public  class EventManagerFacade
	{
		static EventManagerFacade instance = null;
		private static EventManager _eventmgr;
		static EventManagerFacade()
		{

		}

		public static EventManagerFacade Instance
		{
			get {
				if (instance == null) {
					instance = new EventManagerFacade ();
				}
				return instance;
			}
		}

		public IEventHandle GetEventManager()
		{
			if (_eventmgr == null) {
				_eventmgr = new EventManager ();
			}

			return _eventmgr;
		}
	}
}

