using System;
using SQLite;

namespace wincom.mobile.erp
{
	public class CompanyInfo 
	{
		[PrimaryKey]
		public string RegNo{ get; set;}
		public string CompanyName{ get; set;}
		public string Addr1 { get; set;}
		public string Addr2 { get; set;}
		public string Addr3 { get; set;}
		public string Addr4 { get; set;}
		public string Fax{ get; set;}
		public string Tel{ get; set;}
		public string GSTNo{ get; set;}
		public string HomeCurr{ get; set;}
		public bool IsInclusive{ get; set;}
		public int SalesTaxDec{ get; set;}
		public bool  AllowEdit { get; set;}
		public bool  AllowDelete { get; set;}
		public bool  ShowTime { get; set;}
		public string WCFUrl { get ; set;}
		public string SupportContat { get; set;}
		public bool  NotEditAfterPrint { get; set;}
		public bool  AllowClrTrxHis { get; set;}

	}
}

