using System;
using SQLite;

namespace wincom.mobile.erp
{
	public class AdPara
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string PrinterName{ get; set;}
		public string Prefix{ get; set;}
		public string PaperSize{ get; set;}
		public string Warehouse{ get; set;}
		public string ReceiptTitle{ get; set;}
		public int RunNo{ get; set;}
		//Version 2 added
		public int CNRunNo{ get; set;}
		public int SORunNo{ get; set;}
		public int DORunNo{ get; set;}
		public string CNPrefix{ get; set;}
		public string DOPrefix{ get; set;}
		public string SOPrefix{ get; set;}
	}

	public class AdNumDate
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public int Year{ get; set;}
		public int Month{ get; set;}
		public int RunNo{ get; set;}
		//Version 2 added
		public string TrxType{ get; set;}

	}
}

