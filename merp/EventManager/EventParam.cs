using System;
using System.Collections;

namespace wincom.mobile.erp
{
	public class EventParam : EventArgs
	{
		private Hashtable _param = new Hashtable();

		public Hashtable Param
		{
			get { return _param; }
			set { _param = value; }
		}

		int _eventID;

		public int EventID
		{
			get { return _eventID; }
			set { _eventID = value; }
		}

		public EventParam(int eventID, Hashtable param)
		{
			_param = param;
			_eventID = eventID;
		}

	}
}

