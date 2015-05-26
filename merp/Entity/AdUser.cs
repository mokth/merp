using System;
using SQLite;

namespace wincom.mobile.erp
{
	public class AdUser
	{
		[PrimaryKey]
		public string UserID{ get; set;}
		public string CompCode{ get; set;}
		public string BranchCode{ get; set;}
		public string Password { get; set;}
		public bool Islogon { get; set;}

	}
}

