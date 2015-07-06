
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace wincom.mobile.erp
{
	public class Trader
	{
		[PrimaryKey]
		public string CustCode{ get; set;}
		public string CustName{ get; set;}
		public string Addr1{ get; set;}
		public string Addr2{ get; set;}
		public string Addr3{ get; set;}
		public string Addr4{ get; set;}
		public string Tel{ get; set;}
		public string Fax{ get; set;}
		public string gst{ get; set;}

		public string PayCode{ get; set;}
	}
}

