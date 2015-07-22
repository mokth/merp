using System;
using System.Linq;
using Android.Runtime;
using WcfServiceItem;

namespace wincom.mobile.erp
{
	public class DataHelper
	{
		public static AdUser GetUser(string pathToDatabase)
		{
		AdUser user=null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<AdUser> ().ToList<AdUser> ();
				if (list2.Count > 0) {
					user = list2 [0];
				}
			}
		
			return user;
		}

		public static int GetLastInvRunNo(string pathToDatabase, DateTime invdate )
		{
			DateTime Sdate = invdate.AddDays (1 - invdate.Day);
			DateTime Edate = new DateTime (invdate.Year, invdate.Month, DateTime.DaysInMonth (invdate.Year, invdate.Month));
			int runno = -1;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<Invoice> ().Where(x=>x.invdate>=Sdate && x.invdate<=Edate)
						    .OrderByDescending(x=>x.invdate)
					        .ToList<Invoice> ();
				if (list2.Count > 0) {
					string invno =list2[0].invno;
					if (invno.Length > 5)
					{
						string srunno = invno.Substring(invno.Length - 4);
						runno = Convert.ToInt32(srunno);
					}
				}
			}

			return runno;
		}

		public static int GetLastSORunNo(string pathToDatabase, DateTime sodate )
		{
			DateTime Sdate = sodate.AddDays (1 - sodate.Day);
			DateTime Edate = new DateTime (sodate.Year, sodate.Month, DateTime.DaysInMonth (sodate.Year, sodate.Month));
			int runno = -1;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<SaleOrder> ().Where(x=>x.sodate>=Sdate && x.sodate<=Edate)
					.OrderByDescending(x=>x.sodate)
					.ToList<SaleOrder> ();
				if (list2.Count > 0) {
					string sono =list2[0].sono;
					if (sono.Length > 5)
					{
						string srunno = sono.Substring(sono.Length - 4);
						runno = Convert.ToInt32(srunno);
					}
				}
			}

			return runno;
		}

		public static int GetLastDORunNo(string pathToDatabase, DateTime dodate )
		{
			DateTime Sdate = dodate.AddDays (1 - dodate.Day);
			DateTime Edate = new DateTime (dodate.Year, dodate.Month, DateTime.DaysInMonth (dodate.Year, dodate.Month));
			int runno = -1;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<DelOrder> ().Where(x=>x.dodate>=Sdate && x.dodate<=Edate)
					.OrderByDescending(x=>x.dodate)
					.ToList<DelOrder> ();
				if (list2.Count > 0) {
					string dono =list2[0].dono;
					if (dono.Length > 5)
					{
						string srunno = dono.Substring(dono.Length - 4);
						runno = Convert.ToInt32(srunno);
					}
				}
			}

			return runno;
		}

		public static int GetLastCNRunNo(string pathToDatabase, DateTime invdate )
		{
			DateTime Sdate = invdate.AddDays (1 - invdate.Day);
			DateTime Edate = new DateTime (invdate.Year, invdate.Month, DateTime.DaysInMonth (invdate.Year, invdate.Month));
			int runno = -1;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<CNNote> ().Where(x=>x.invdate>=Sdate && x.invdate<=Edate)
					.OrderByDescending(x=>x.invdate)
					.ToList<CNNote> ();
				if (list2.Count > 0) {
					string invno =list2[0].cnno;
					if (invno.Length > 5)
					{
						string srunno = invno.Substring(invno.Length - 4);
						runno = Convert.ToInt32(srunno);
					}
				}
			}

			return runno;
		}


		public static bool GetSaleOrderPrintStatus(string pathToDatabase,string sono)
		{
			bool iSPrinted = false;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var info = db.Table<CompanyInfo> ().FirstOrDefault ();
				if (info.NotEditAfterPrint) {
					var list = db.Table<SaleOrder> ().Where (x => x.sono == sono).ToList ();
					if (list.Count > 0) {
						iSPrinted = list [0].isPrinted; 				
					}
				}
			}
			return iSPrinted;
		}

		public static bool GetDelOderPrintStatus(string pathToDatabase,string dono)
		{
			bool iSPrinted = false;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var info = db.Table<CompanyInfo> ().FirstOrDefault ();
				if (info.NotEditAfterPrint) {
					var list = db.Table<DelOrder> ().Where (x => x.dono == dono).ToList ();
					if (list.Count > 0) {
						iSPrinted = list [0].isPrinted; 				
					}
				}
			}
			return iSPrinted;
		}

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
					cprof.RegNo = pro.RegNo;
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

