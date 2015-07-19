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

			AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
			Button but = FindViewById<Button> (Resource.Id.butSecond);
			but.Click += butClick;
			Button butInvlist = FindViewById<Button> (Resource.Id.butInvlist);
			butInvlist.Click+= ButInvlist_Click;
			Button butdown = FindViewById<Button> (Resource.Id.butDown);
			butdown.Click += butDownloadItems;
			Button butMItem = FindViewById<Button> (Resource.Id.butMaster);
			butMItem.Click += butMasterClick;
			Button butSett = FindViewById<Button> (Resource.Id.butsetting);
			butSett.Click += butSetting;
//			Button butCustProf = FindViewById<Button> (Resource.Id.butCustProf);
//			butCustProf.Click+=  butCustomerClick;
			Button butlogOff = FindViewById<Button> (Resource.Id.butOut);
			butlogOff.Click += ButlogOff_Click;
			Button butAbt = FindViewById<Button> (Resource.Id.butAbout);
			butAbt.Click+= ButAbt_Click;
//			Button butupload = FindViewById<Button> (Resource.Id.butupload);
//			butupload.Click += butUploadBills;
			//Button butExitOnly = FindViewById<Button> (Resource.Id.butExitOnly);
//			Button butsumm = FindViewById<Button> (Resource.Id.butInvsumm);
//			butsumm.Click+= (object sender, EventArgs e) => {
//				StartActivity(typeof(PrintSumm));
//			};

			Button butCNNote = FindViewById<Button> (Resource.Id.butcnnote);
			butCNNote.Click+= ButCNNote_Click;
//			Button butCNNoteList = FindViewById<Button> (Resource.Id.butCNlist);
//			butCNNoteList.Click+= ButCNNoteList_Click;
		}

		void ButCNNoteList_Click (object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(CNAllActivity));

			StartActivity(intent);
		}

		void ButCNNote_Click (object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(CNNoteActivity));

			StartActivity(intent);
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
				db.DeleteAll<InvoiceDtls> ();
				db.DeleteAll<Invoice> ();
				db.DeleteAll<CNNoteDtls> ();
				db.DeleteAll<CNNote> ();
//				var list2 = db.Table<Invoice> ().Where (x => x.isUploaded == true).ToList<Invoice> ();
//				db.RunInTransaction (() => {
//					foreach (var item in list2) {
//						var listitm = db.Table<InvoiceDtls> ().Where (x => x.invno == item.invno).ToList<InvoiceDtls> ();
//						foreach(var iitem in listitm){
//							db.Delete(iitem);
//						}
//						db.Delete(item);
//					}
//				});
//
//				var list3 = db.Table<CNNote> ().Where (x => x.isUploaded == true).ToList<CNNote> ();
//				db.RunInTransaction (() => {
//					foreach (var item in list3) {
//						var listitm = db.Table<CNNoteDtls> ().Where (x => x.cnno == item.cnno).ToList<CNNoteDtls> ();
//						foreach(var iitem in listitm){
//							db.Delete(iitem);
//						}
//						db.Delete(item);
//					}
//				});

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
			download.startDownloadCompInfo();
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


