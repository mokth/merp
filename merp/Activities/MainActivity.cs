using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using WcfServiceItem;
using Android.Telephony;
using Android.Accounts;
using Android.Text;



namespace wincom.mobile.erp
{
	[Activity (Label = "WINCOM M-ERP",Icon = "@drawable/icon")]
	public class MainActivity :Activity
	{
		//List<Item> items = null;
		public static readonly EndpointAddress EndPoint = new EndpointAddress("http://www.wincomcloud.com/Wfc/Service1.svc");
		private Service1Client _client;
		string pathToDatabase;
		string COMPCODE;
		string BRANCODE;
		string USERID;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
				return;
			}
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			GetDBPath ();
			InitializeServiceClient ();

			Button but = FindViewById<Button> (Resource.Id.butSecond);
			but.Click += butClick;
			Button butInvlist = FindViewById<Button> (Resource.Id.butInvlist);
			butInvlist.Click+= ButInvlist_Click;
			Button but2 = FindViewById<Button> (Resource.Id.butDown);
			but2.Click += butDownClick;
			Button butMItem = FindViewById<Button> (Resource.Id.butMaster);
			butMItem.Click += butMasterClick;
			Button butCust = FindViewById<Button> (Resource.Id.butDownCust);
			butCust.Click += butDownCustClick;
			Button butCustProf = FindViewById<Button> (Resource.Id.butCustProf);
			butCustProf.Click += butCustomerClick;
			Button butlogOff = FindViewById<Button> (Resource.Id.butOut);
			butlogOff.Click += ButlogOff_Click;
			Button butAbt = FindViewById<Button> (Resource.Id.butAbout);
			butAbt.Click+= ButAbt_Click;
			//Button butbackdb = FindViewById<Button> (Resource.Id.butbackdb);
			//butbackdb.Click += butBackUpDb;	
			//Button butsetting = FindViewById<Button> (Resource.Id.butsetting);
			//butsetting.Click += butSysSetting;

			Button butupload = FindViewById<Button> (Resource.Id.butupload);
			butupload.Click += butUploadBills;
			//Button butExitOnly = FindViewById<Button> (Resource.Id.butExitOnly);
		}

		void ButAbt_Click (object sender, EventArgs e)
		{
			CompanyInfo comp= DataHelper.GetCompany (pathToDatabase);
			View messageView = LayoutInflater.Inflate(Resource.Layout.About, null, false);

			// When linking text, force to always use default color. This works
			// around a pressed color state bug.
			TextView textView = (TextView) messageView.FindViewById(Resource.Id.about_credits);
			TextView textDesc = (TextView) messageView.FindViewById(Resource.Id.about_descrip);
			//textDesc.Text = Html.FromHtml (Resources.GetString(Resource.String.app_descrip))..ToString();
			textView.Text = "For inquiry, please contact " + comp.SupportContat;

			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetIcon(Resource.Drawable.Icon);
			builder.SetTitle(Resource.String.app_name);
			builder.SetView(messageView);
			builder.Create();
			builder.Show();
		}

		void ButInvlist_Click (object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(InvoiceAllActivity));

			StartActivity(intent);
		}

		public override bool  OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.MainMenu,menu);
			return base.OnPrepareOptionsMenu(menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
			case Resource.Id.mmenu_back:
				BackupDatabase ();
				return true;
//			case Resource.Id.mmenu_Reset:
//				//do something
//				return true;
			case Resource.Id.mmenu_setting:
				StartActivity (typeof(SettingActivity));
				return true;
			case Resource.Id.mmenu_logoff:
				RunOnUiThread(() =>ExitAndLogOff()) ;
				return true;
			case Resource.Id.mmenu_downcompinfo:
				DownloadCompInfo ();
				return true;
			}
		
			return base.OnOptionsItemSelected(item);
		}

		void GetDBPath ()
		{
			COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			BRANCODE = ((GlobalvarsApp)this.Application).BRANCH_CODE;
			USERID = ((GlobalvarsApp)this.Application).USERID_CODE;
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
		}

		protected override void OnResume()
		{
			base.OnResume();
			GetDBPath ();
		}

		void butSysSetting(object sender,EventArgs e)
		{
			StartActivity (typeof(SettingActivity));
		}

		void BackupDatabase ()
		{
			var sdcard = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.Path, "erpdata");
			if (!Directory.Exists (sdcard)) {
				Directory.CreateDirectory (sdcard);
			}
			string filename = Path.Combine (sdcard,"erplite"+ DateTime.Now.ToString("yyMMddHHmm") +".db");
			if (File.Exists (pathToDatabase)) {
				File.Copy (pathToDatabase, filename, true);
			}
		}

		void butBackUpDb(object sender,EventArgs e)
		{
			BackupDatabase ();
		}

		void butUploadBills(object sender,EventArgs e)
		{
			UploadBillsToServer();
		}

		void ExitApp ()
		{
			//var intent = new Intent (this, typeof(LoginActivity));
			//StartActivity (intent);
			((GlobalvarsApp)this.Application).ISLOGON = false;
			Finish ();
			Android.OS.Process.KillProcess (Android.OS.Process.MyPid ());
			Parent.Finish ();
			Intent intent = new Intent(Intent.ActionMain);
 			intent.AddCategory(Intent.CategoryHome);
			intent.SetFlags(ActivityFlags.NewTask);
			StartActivity(intent);

		}

		void ExitAndLogOff ()
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<AdUser> ().ToList<AdUser> ();
				if (list2.Count > 0) {
					list2 [0].Islogon = false;
					db.Update (list2 [0], typeof(AdUser));
				}
			}
			RunOnUiThread (() => ExitApp ());
		}

		void ButlogOff_Click (object sender, EventArgs e)
		{
			RunOnUiThread (() => ExitApp ());

		}

//		void createTable(string pathToDatabase)
//		{
//			using (var conn= new SQLite.SQLiteConnection(pathToDatabase))
//			{
//				conn.CreateTable<Item>();
//				conn.CreateTable<Invoice>();
//				conn.CreateTable<InvoiceDtls>();
//				conn.CreateTable<Trader> ();
//			}
//		}

		private void butClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(InvoiceActivity));

			StartActivity(intent);
		}

		private void butMasterClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(MasterItemActivity));

			StartActivity(intent);
		}
		private void butCustomerClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(CustomerActivity));

			StartActivity(intent);
		}

		private void butDownClick(object sender,EventArgs e)
		{
			Button but = (Button)sender;
			but.Enabled = false;
			but.Text = "Downloading Master Item...";
			_client.GetItemCodesAsync (COMPCODE, BRANCODE);
		}

		private void butDownCustClick(object sender,EventArgs e)
		{
			Button but = (Button)sender;
			but.Enabled = false;
			but.Text = "Downloading Customer...";
			_client.GetCustomersAsync (COMPCODE, BRANCODE);
		}

		private void InitializeServiceClient()
		{
			BasicHttpBinding binding = CreateBasicHttp();

			_client = new Service1Client(binding, EndPoint);
			_client.GetItemCodesCompleted+= ClientOnGetItemCompleted;
			_client.GetCustomersCompleted+= ClientOnGetCustomerCompleted;
			_client.UploadOutletBillsCompleted+= ClientOnUploadOutletBillsCompleted;
			_client.GetCompProfileCompleted += ClientOnCompProfCompleted;
				
		}

		private static BasicHttpBinding CreateBasicHttp()
		{
			BasicHttpBinding binding = new BasicHttpBinding
			{
				Name = "basicHttpBinding",
				MaxBufferSize = 2147483647,
				MaxReceivedMessageSize = 2147483647
			};
			TimeSpan timeout = new TimeSpan(0, 0, 30);
			binding.SendTimeout = timeout;
			binding.OpenTimeout = timeout;
			binding.ReceiveTimeout = timeout;
			return binding;
		}

		void DownloadCOmpleted (string msg)
		{
			Button but2 = FindViewById<Button> (Resource.Id.butDown);
			but2.Text = "DOWNLOAD ITEM";

					//AlertDialog.Builder alert = new AlertDialog.Builder (this);
					//alert.SetTitle (msg);
					//alert.Show();
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	

		}

		private void ClientOnUploadOutletBillsCompleted(object sender, UploadOutletBillsCompletedEventArgs e)
		{
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				success = false;
			}
			else if ( e.Cancelled)
			{
				msg = "Request was cancelled.";
				success = false;
			}
			else
			{
				msg = e.Result.ToString ();
				if (msg == "OK") {
					RunOnUiThread (() => UpdateUploadStat());
				}
			}

		   RunOnUiThread (() => DownloadCOmpleted (msg));

		}
		private void ClientOnGetItemCompleted(object sender, GetItemCodesCompletedEventArgs e)
		{
			List<ItemCode> list = new List<ItemCode> ();
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				success = false;
			}
			else if ( e.Cancelled)
			{
				msg = "Request was cancelled.";
				success = false;
			}
			else
			{
				list =  e.Result.ToList<ItemCode>();
				RunOnUiThread (() => InsertItemIntoDb (list));
			}
			if (!success) {
				RunOnUiThread (() => DownloadCOmpleted (msg));
			}
		}

		private void ClientOnGetCustomerCompleted(object sender, GetCustomersCompletedEventArgs e)
		{
			List<Customer> list = new List<Customer> ();
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				success = false;
			}
			else if ( e.Cancelled)
			{
				msg = "Request was cancelled.";
				success = false;
			}
			else
			{
				list =  e.Result.ToList<Customer>();
				RunOnUiThread (() => InsertCustomerIntoDb (list));
			}
			if (!success) {
				RunOnUiThread (() => DownloadCOmpleted (msg));
			}
		}

		void EnableButDonwICode ()
		{
			Button but2 = FindViewById<Button> (Resource.Id.butDown);
			but2.Enabled = true;
			but2.Text = "DOWNLOAD MASTER ITEM";
		}

		void EnableButDonwCust ()
		{
			Button but2 = FindViewById<Button> (Resource.Id.butDownCust);
			but2.Enabled = true;
			but2.Text = "DOWNLOAD CUSTOMER";
		}

		private void InsertItemIntoDb(List<ItemCode> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
		    {
				var list2 = db.Table<Item>().ToList<Item>();
				foreach (ItemCode item in list) {
					Item itm = new Item ();
					itm.ICode = item.ICode;
					itm.IDesc = item.IDesc;
					itm.Price = item.Price;
					itm.tax = item.tax;
					itm.taxgrp = item.taxgrp;
					itm.isincludesive = item.isincludesive;

					if (list2.Where (x => x.ICode == itm.ICode).ToList ().Count () == 0) {
						db.Insert (itm);
					} else
						db.Update (itm);
				}
			}
			DownloadCOmpleted ("Successfully downloaded.");
			EnableButDonwICode ();

		}

		private void InsertCustomerIntoDb(List<Customer> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<Trader>().ToList<Trader>();
				foreach (Customer item in list) {
					Trader itm = new Trader ();
					itm.CustCode = item.CustomerCode;
					itm.CustName = item.CustomerName;
					itm.Addr1 = item.Addr1;
					itm.Addr2 = item.Addr2;
					itm.Addr3 = item.Addr3;
					itm.Addr4 = item.Addr4;
					itm.Tel = item.Tel;
					itm.Fax = item.Fax;
					itm.gst = item.Gst;

				
					if (list2.Where (x => x.CustCode == itm.CustCode).ToList ().Count () == 0) {
						db.Insert (itm);
					} else
						db.Update (itm);
				}
			}
			DownloadCOmpleted ("Successfully downloaded.");
			EnableButDonwCust ();

		}

		private void UploadBillsToServer()
		{
			PhoneTool ptool = new PhoneTool ();
			string phone = ptool.PhoneNumber ();
			string serial =ptool.DeviceIdIMEI();
			string comp =((GlobalvarsApp)this.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)this.Application).BRANCH_CODE;
			List<OutLetBill> listbills= GetBills();
			_client.UploadOutletBillsAsync (listbills.ToArray(),comp,brn,serial,phone);
		}
		private void UpdateUploadStat()
		{
			DateTime now = DateTime.Now;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list1 = db.Table<Invoice> ().ToList<Invoice>().Where(x => x.isUploaded == false).ToList<Invoice>();
				foreach (Invoice inv in list1) {
					inv.isUploaded = true;
					inv.uploaded = now;
				}	
				db.UpdateAll (list1);
			}
		}
		public override void OnBackPressed() {
			// do nothing.
		}

		private List<OutLetBill> GetBills()
		{
			string comp =((GlobalvarsApp)this.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)this.Application).BRANCH_CODE;
			string userid =((GlobalvarsApp)this.Application).USERID_CODE;

			List<OutLetBill> bills = new List<OutLetBill> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list1 = db.Table<Invoice> ().Where(x=>x.isUploaded==false).ToList<Invoice> ();
				var list2 = db.Table<InvoiceDtls> ().ToList<InvoiceDtls> ();
								
				foreach (Invoice inv in list1) {
					var list3 = list2.Where (x => x.invno == inv.invno).ToList<InvoiceDtls> ();
					foreach (InvoiceDtls invdtl in list3) {
						OutLetBill bill = new OutLetBill ();
						bill.UserID = userid;
						bill.BranchCode = brn;
						bill.CompanyCode = comp;
						bill.Created = inv.created;
						bill.CustCode = inv.custcode;
						bill.ICode = invdtl.icode;
						bill.InvDate = inv.invdate;
						bill.InvNo = inv.invno;
						bill.IsInclusive = invdtl.isincludesive;
						bill.Amount = invdtl.amount;
						bill.NetAmount = invdtl.netamount;
						bill.TaxAmt = invdtl.tax;
						bill.TaxGrp = invdtl.taxgrp;
						bill.UPrice = invdtl.price;
						bill.Qty = invdtl.qty;
						bills.Add (bill);
					}
				}
			}

			return bills;
		}

		private void DownloadCompInfo()
		{
			_client.GetCompProfileAsync (COMPCODE, BRANCODE, USERID);
		}

		private void ClientOnCompProfCompleted(object sender,GetCompProfileCompletedEventArgs e)
		{
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				success = false;
			}
			else if ( e.Cancelled)
			{
				msg = "Request was cancelled.";
				success = false;
			}
			else
			{
				CompanyProfile pro = (CompanyProfile)e.Result;
				RunOnUiThread (() => InsertCompProfIntoDb( pro));
			}
			if (!success) {
				RunOnUiThread (() => DownloadCOmpleted (msg));
			}
		}

		private void InsertCompProfIntoDb(CompanyProfile pro)
		{
			DataHelper.InsertCompProfIntoDb (pro, pathToDatabase);
			DownloadCOmpleted ("Successfully Download Company Profile.");
		
		}
	}
}


