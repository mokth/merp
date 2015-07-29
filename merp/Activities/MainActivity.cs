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
using Android.Content.PM;
using System.Net;

namespace wincom.mobile.erp
{
	//[Activity (Label = "WINCOM M-ERP",Icon = "@drawable/icon")]
	[Activity (Icon = "@drawable/icon")]
	public class MainActivity :Activity
	{
		//List<Item> items = null;

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
			if (this.BaseContext.PackageName.ToLower().Contains ("demo")) {
				this.Window.SetTitle ("WINCOM M-ERP DEMO");
			}else  Window.SetTitle ("WINCOM M-ERP");
		
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			GetDBPath ();

			AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
			Button buttrans = FindViewById<Button> (Resource.Id.butTrans);
			buttrans.Click+= buttrans_Click;
			Button butInvlist = FindViewById<Button> (Resource.Id.butInvlist);
			butInvlist.Click+= ButInvlist_Click;
			Button butdown = FindViewById<Button> (Resource.Id.butDown);
			butdown.Click += butDownloadItems;
			Button butMItem = FindViewById<Button> (Resource.Id.butMaster);
			butMItem.Click += butMasterClick;
			Button butSett = FindViewById<Button> (Resource.Id.butsetting);
			butSett.Click += butSetting;
			Button butlogOff = FindViewById<Button> (Resource.Id.butOut);
			butlogOff.Click += ButlogOff_Click;
			Button butAbt = FindViewById<Button> (Resource.Id.butAbout);
			butAbt.Click+= ButAbt_Click;

		}

		void buttrans_Click (object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(TransactionsActivity));

			StartActivity(intent);
		}



		void ButAbt_Click (object sender, EventArgs e)
		{
			CompanyInfo comp= DataHelper.GetCompany (pathToDatabase);
			View messageView = LayoutInflater.Inflate(Resource.Layout.About, null, false);
			PackageInfo pInfo = PackageManager.GetPackageInfo (PackageName, 0);
			// When linking text, force to always use default color. This works
			// around a pressed color state bug.
			TextView textView = (TextView) messageView.FindViewById(Resource.Id.about_credits);
			TextView textDesc = (TextView) messageView.FindViewById(Resource.Id.about_descrip);
			TextView textVer = (TextView) messageView.FindViewById(Resource.Id.about_ver);
			//textDesc.Text = Html.FromHtml (Resources.GetString(Resource.String.app_descrip))..ToString();
			textView.Text = "For inquiry, please contact " + comp.SupportContat;
			textVer .Text = "Build Version : "+pInfo.VersionName;
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetIcon(Resource.Drawable.Icon);
			builder.SetTitle(Resource.String.app_name);
			builder.SetView(messageView);
			builder.Create();
			builder.Show();
		}

		void AndroidEnvironment_UnhandledExceptionRaiser (object sender, RaiseThrowableEventArgs e)
		{
			Toast.MakeText (this, e.Exception.Message, ToastLength.Long).Show ();	
			StartActivity (typeof(MainActivity));
			Finish ();
		}

		protected override void Dispose(bool disposing)
		{
			AndroidEnvironment.UnhandledExceptionRaiser -=AndroidEnvironment_UnhandledExceptionRaiser;
			base.Dispose(disposing);
		}

		void ButInvlist_Click (object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(TransListActivity));

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
			case Resource.Id.mmenu_downdb:
				var builderd = new AlertDialog.Builder(this);
				builderd.SetMessage("Confirm to download database from server ? All local data will be overwritten by the downloaded data.");
				builderd.SetPositiveButton("OK", (s, e) => { DownlooadDb ();;});
				builderd.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
				builderd.Create().Show();
				return true; 
			case Resource.Id.mmenu_setting:
				StartActivity (typeof(SettingActivity));
				return true;
			case Resource.Id.mmenu_logoff:
				RunOnUiThread(() =>ExitAndLogOff()) ;
				return true;
			case Resource.Id.mmenu_downcompinfo:
				DownloadCompInfo ();
				return true;
			case Resource.Id.mmenu_clear:
				var builder = new AlertDialog.Builder(this);
				builder.SetMessage("Confirm to CLEAR ? All transaction will be deleted. Deleted records are not recoverable.");
				builder.SetPositiveButton("OK", (s, e) => { ClearPostedInv () ;});
				builder.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
				builder.Create().Show();

				return true;
			}
		
			return base.OnOptionsItemSelected(item);
		}

		void DownlooadDb()
		{
			try {
				//backup db first before upload
				BackupDatabase();

				WebClient myWebClient = new WebClient ();
				var sdcard = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.Path, "erpdata");
				string filename = COMPCODE + "_" + BRANCODE + "_" + USERID + "_erplite.db";
				string url = WCFHelper.GetDownloadDBUrl () + filename;
				string localfilename = Path.Combine (sdcard, "erplite.db");
				if (File.Exists(localfilename))
					File.Delete(localfilename);

				myWebClient.DownloadFile (url, localfilename);  
				File.Copy (localfilename, pathToDatabase, true);

				//delete the file after downloaded
				string urldel = WCFHelper.GeUploadDBUrl()+"/afterdownload.aspx?ID="+filename;
				WebRequest request = HttpWebRequest.Create(urldel);
				request.GetResponse();

				Toast.MakeText (this, "Successfully download db file from server!", ToastLength.Long).Show ();	
			} catch (Exception ex)
			{
				Toast.MakeText (this, "Error downloading db file from server!", ToastLength.Long).Show ();	
			}
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

		void ClearPostedInv()
		{
			CompanyInfo para = DataHelper.GetCompany (pathToDatabase);
			if (!para.AllowClrTrxHis) {
				Toast.MakeText (this, "Access denied...", ToastLength.Long).Show ();	
				return;
			}
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<Invoice> ().Where (x => x.isUploaded == true).ToList<Invoice> ();
				db.RunInTransaction (() => {
					foreach (var item in list2) {
						var listitm = db.Table<InvoiceDtls> ().Where (x => x.invno == item.invno).ToList<InvoiceDtls> ();
						foreach(var iitem in listitm){
							db.Delete(iitem);
						}
						db.Delete(item);
					}
				});

				var list3 = db.Table<CNNote> ().Where (x => x.isUploaded == true).ToList<CNNote> ();
				db.RunInTransaction (() => {
					foreach (var item in list3) {
						var listitm = db.Table<CNNoteDtls> ().Where (x => x.cnno == item.cnno).ToList<CNNoteDtls> ();
						foreach(var iitem in listitm){
							db.Delete(iitem);
						}
						db.Delete(item);
					}
				});


				var list4 = db.Table<SaleOrder> ().Where (x => x.isUploaded == true).ToList<SaleOrder> ();
				db.RunInTransaction (() => {
					foreach (var item in list4) {
						var listitm = db.Table<SaleOrderDtls> ().Where (x => x.sono == item.sono).ToList<SaleOrderDtls> ();
						foreach(var iitem in listitm){
							db.Delete(iitem);
						}
						db.Delete(item);
					}
				});

				var list5 = db.Table<DelOrder> ().Where (x => x.isUploaded == true).ToList<DelOrder> ();
				db.RunInTransaction (() => {
					foreach (var item in list5) {
						var listitm = db.Table<DelOrderDtls> ().Where (x => x.dono == item.dono).ToList<DelOrderDtls> ();
						foreach(var iitem in listitm){
							db.Delete(iitem);
						}
						db.Delete(item);
					}
				});


				Toast.MakeText (this, "Transaction clear...", ToastLength.Long).Show ();	
			}
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
				UploadToErpHostForSupport (filename);
			}
		}

		private void UploadToErpHostForSupport(string filename)
		{
			WebClient myWebClient = new WebClient();
			try{

				myWebClient.QueryString["COMP"] = COMPCODE;
				myWebClient.QueryString["BRAN"] = BRANCODE;
				myWebClient.QueryString["USER"] = USERID;
				byte[] responseArray = myWebClient.UploadFile( WCFHelper.GeUploadDBUrl(),filename);
				//Console.WriteLine("\nResponse Received.The contents of the file uploaded are:\n{0}",
				//	System.Text.Encoding.ASCII.GetString(responseArray));
			}catch {

			}
		}

		void butBackUpDb(object sender,EventArgs e)
		{
			BackupDatabase ();
		}

		void butUploadBills(object sender,EventArgs e)
		{
			Button butupload =  FindViewById<Button> (Resource.Id.butupload);
			butupload.Enabled = false;
			//UploadBillsToServer();
			UploadHelper upload= new UploadHelper();
			upload.Uploadhandle = OnUploadDoneDlg; 
			upload.CallingActivity = this;
			upload.startUpload ();		
		}

		void butDownloadItems(object sender,EventArgs e)
		{
			StartActivity (typeof(DownloadActivity));
//			Button butDown =  FindViewById<Button> (Resource.Id.butDown);
//			butDown.Enabled = false;
//			DownloadHelper download= new DownloadHelper();
//			download.Downloadhandle = DownItemsDoneDlg; 
//			download.CallingActivity = this;
//			download.startDownloadItem ();
		}
		void butSetting(object sender,EventArgs e)
		{
			StartActivity (typeof(SettingActivity));
		}
//		void butDownloadCusts(object sender,EventArgs e)
//		{
//			Button butDown =  FindViewById<Button> (Resource.Id.butDownCust);
//			butDown.Enabled = false;
//			DownloadHelper download= new DownloadHelper();
//			download.Downloadhandle =  DownCustDoneDlg; 
//			download.CallingActivity = this;
//			download.startDownloadCustomer ();
//		}

		private void DownloadCompInfo()
		{
			DownloadHelper download= new DownloadHelper();
			download.Downloadhandle = OnDownProfileDoneDlg; 
			download.CallingActivity = this;
			download.startDownloadCompInfo(false);
		}

		private void OnDownProfileDoneDlg(Activity callingAct,int count,string msg)
		{
			Toast.MakeText (this, msg, ToastLength.Long).Show ();
		}

		private void OnUploadDoneDlg(Activity callingAct,int count,string msg)
		{
			Button butupload = callingAct.FindViewById<Button> (Resource.Id.butupload);
			butupload.Enabled = true;
			if (count > 0) {
				string dispmsg = "Total " + count.ToString () + " invoices uploaded.";
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}

		private void DownItemsDoneDlg(Activity callingAct,int count,string msg)
		{
			Button butdown = FindViewById<Button> (Resource.Id.butDown);
			butdown.Enabled = true;
			if (count > 0) {
				string dispmsg = "Total " + count.ToString () + " Items downloaded.";
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}

		private void DownCustDoneDlg(Activity callingAct,int count,string msg)
		{
			Button butdown = FindViewById<Button> (Resource.Id.butDownCust);
			butdown.Enabled = true;
			if (count > 0) {
				string dispmsg = "Total " + count.ToString () + " Customers downloaded.";
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
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

		private void butClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(InvoiceActivity));

			StartActivity(intent);
		}

		private void butMasterClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(MasterRefActivity));

			StartActivity(intent);
		}
		private void butCustomerClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(CustomerActivity));

			StartActivity(intent);
		}

		public override void OnBackPressed() {
			// do nothing.
		}

	}
}


