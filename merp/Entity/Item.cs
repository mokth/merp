using System;
using SQLite;

namespace wincom.mobile.erp
{
	public class Item
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string ICode{ get; set; }
		public string IDesc { get; set; }
		public double Price { get; set; }
		public double tax { get; set; }
		public string taxgrp { get; set; }
		public bool isincludesive { get; set; }
	}


}
