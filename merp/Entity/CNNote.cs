using System;
using SQLite;

namespace wincom.mobile.erp
{//Version 2 added
	public class CNNote
	{
		[PrimaryKey] 
		public string cnno  { get; set; }
		public string invno  { get; set; }
		public string trxtype  { get; set; }
		public DateTime invdate  { get; set; }
		public DateTime created  { get; set; }
		public double amount { get; set; }
		public double taxamt { get; set; }
		public string custcode { get; set; }
		public string description { get; set; }
		public DateTime uploaded  { get; set; }
		public bool isUploaded  { get; set; }
		public bool isPrinted  { get; set; }
	}
	//Version 2 added
	public class CNNoteDtls
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string cnno  { get; set; }
		public double amount { get; set; }
		public double netamount { get; set; }
		public string icode { get; set; }
		public string description { get; set; }
		public double qty { get; set; }
		public double price { get; set; }
		public string taxgrp { get; set; }
		public double tax { get; set; }
		public bool isincludesive { get; set; }
	}
}

