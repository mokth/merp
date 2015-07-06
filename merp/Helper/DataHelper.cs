using System;
using System.Linq;
using Android.Runtime;
using WcfServiceItem;

namespace wincom.mobile.erp
{
	public class DataHelper
	{
		public static bool GetInvoicePrintStatus(string pathToDatabase,string invno)
		{
			bool iSPrinted = false;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var info = db.Table<CompanyInfo> ().FirstOrDefault ();
				if (info.NotEditAfterPrint) {
					var list = db.Table<Invoice> ().Where (x => x.invno == invno).ToList ();
					if (list.Count > 0) {
						iSPrinted = list [0].isPrinted; 				
					}
				}
			}
			return iSPrinted;
		}

		public static bool GetCNNotePrintStatus(string pathToDatabase,string cnno)
		{
			bool iSPrinted = false;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var info = db.Table<CompanyInfo> ().FirstOrDefault ();
				if (info.NotEditAfterPrint) {
					var list = db.Table<CNNote> ().Where (x => x.cnno == cnno).ToList ();
					if (list.Count > 0) {
						iSPrinted = list [0].isPrinted; 				
					}
				}
			}
			return iSPrinted;
		}

		public static CompanyInfo GetCompany(string pathToDatabase)
		{
			CompanyInfo info = null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				info = db.Table<CompanyInfo> ().FirstOrDefault ();
			}
			return info;
		}

		public static Trader GetTrader(string pathToDatabase,string custcode)
		{
			Trader info = null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				info = db.Table<Trader> ().Where(x=>x.CustCode==custcode).FirstOrDefault ();
			}
			return info;
		}

		public static AdPara GetAdPara(string pathToDatabase)
		{
			AdPara info = null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				info = db.Table<AdPara> ().FirstOrDefault ();
			}
			if (info == null) {
				info = new AdPara ();
			}
			if (string.IsNullOrEmpty (info.Prefix))
				info.Prefix = "CS";
			if (string.IsNullOrEmpty (info.PrinterName))
				info.PrinterName = "PT-II";
			return info;
		}

		public static AdNumDate GetNumDate(string pathToDatabase,DateTime trxdate)
		{
			AdNumDate info = null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<AdNumDate> ().Where (x =>x.TrxType=="INV" && x.Year == trxdate.Year && x.Month == trxdate.Month).ToList<AdNumDate> ();
				if (list.Count > 0)
					info = list [0];
				else {
					info = new AdNumDate ();
					info.Year = trxdate.Year;
					info.Month = trxdate.Month;
					info.RunNo = 0;
					info.TrxType = "INV";
					info.ID = -1;

				}
			}

			
			return info;
		}

		public static AdNumDate GetNumDate(string pathToDatabase,DateTime trxdate,string trxtype)
		{
			AdNumDate info = null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<AdNumDate> ().Where (x =>x.TrxType==trxtype && x.Year == trxdate.Year && x.Month == trxdate.Month).ToList<AdNumDate> ();
				if (list.Count > 0)
					info = list [0];
				else {
					info = new AdNumDate ();
					info.Year = trxdate.Year;
					info.Month = trxdate.Month;
					info.RunNo = 0;
					info.TrxType = trxtype;
					info.ID = -1;
				}
			}


			return info;
		}

		public  static void InsertCompProfIntoDb(CompanyProfile pro,string pathToDatabase)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<CompanyInfo>().ToList<CompanyInfo>();
				var list3 = db.Table<AdPara>().ToList<AdPara>();
				var list4 = db.Table<AdNumDate> ().Where (x => x.Year == DateTime.Now.Year && x.Month == DateTime.Now.Month).ToList<AdNumDate> ();

				CompanyInfo cprof = null;
				if (list2.Count > 0) {
					cprof = list2 [0];
				} else {
					cprof = new CompanyInfo ();
				}

				cprof.Addr1 = pro.Addr1;
				cprof.Addr2= pro.Addr2;
				cprof.Addr3 = pro.Addr3;
				cprof.Addr4 = pro.Addr4;
				cprof.CompanyName = pro.CompanyName;
				cprof.Fax = pro.Fax;
				cprof.GSTNo = pro.GSTNo;
				cprof.HomeCurr = pro.HomeCurr;
				cprof.IsInclusive = pro.IsInclusive;
				cprof.RegNo = pro.RegNo;
				cprof.SalesTaxDec = pro.SalesTaxDec;
				cprof.AllowDelete = pro.AllowDelete;
				cprof.AllowEdit = pro.AllowEdit;
				cprof.WCFUrl = pro.WCFUrl;
				cprof.SupportContat = pro.SupportContat;
				cprof.ShowTime = pro.ShowPrintTime;
				cprof.AllowClrTrxHis = pro.AllowClrTrxHis;
				cprof.NotEditAfterPrint = pro.NoEditAfterPrint;

				cprof.Tel = pro.Tel;
				if (list2.Count==0)
					db.Insert (cprof);
				else
					db.Update (cprof);

				AdPara apara=null;
				if (list3.Count == 0) {
					apara= new AdPara ();
				} else {
					apara = list3 [0];
				}
				apara.Prefix = pro.Prefix;
				apara.RunNo = pro.RunNo;
				apara.Warehouse = pro.WareHouse;
				//new added V2
				apara.CNPrefix = pro.CNPrefix;
				apara.CNRunNo = pro.CNRunNo;
				apara.DOPrefix = pro.DOPrefix;
				apara.DORunNo = pro.DORunNo;
				apara.SOPrefix = pro.SOPrefix;
				apara.SORunNo = pro.SORunNo;


				if (list3.Count == 0) {
					apara.ReceiptTitle = "TAX INVOICE";
					db.Insert (apara); 
				} else {
					db.Update (apara);
				}

				AdNumDate info = null;
				if (list4.Count == 0)
				{
					info = new AdNumDate ();
					info.Year = DateTime.Now.Year;
					info.Month = DateTime.Now.Month;
					info.RunNo = pro.RunNo;
					info.TrxType = "INV";
					db.Insert (info);
				}
			}

		}
	}
}

