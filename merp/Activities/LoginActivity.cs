using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.ServiceModel;
using System.IO;
using WcfServiceItem;
using Android.Util;

namespace wincom.mobile.erp
{
	[Activity (Label = "M-ERP", MainLauncher = true,NoHistory=true, Theme="@style/android:Theme.Holo.Light.NoActionBar" )]			
	public class LoginActivity : Activity,IEventListener
	{
		private Service1Client _client;
		string pathToDatabase;
		static volatile bool _donwloadPro = false;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.SignIn);
			// Create your application here
			EventManagerFacade.Instance.GetEventManager().AddListener(this);

			//Button import = FindViewById<Button>(Resource.Id.logimport);

			//import.Click+= (object sender, EventArgs e) => {
			//	ImportDatabase();
			//};	
			Button login = FindViewById<Button>(Resource.Id.login);
			Button bexit = FindViewById<Button>(Resource.Id.exit);
			EditText txtcode = FindViewById<EditText> (Resource.Id.login_code);
			bexit.Click += (object sender, EventArgs e) => {
				Finish();
				Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
			};
			//InitializeServiceClient();
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;

			AdUser user=null;
			//SQLiteConnection...CreateFile(pathToDatabase);
			if (!File.Exists (pathToDatabase)) {
				createTable (pathToDatabase);
			}
			user = DataHelper.GetUser (pathToDatabase);
//			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
//				var list2 = db.Table<AdUser> ().ToList<AdUser> ();
//				if (list2.Count > 0) {
//					user = list2 [0];
//				}
//			}


			if (user != null) {
				((GlobalvarsApp)this.Application).USERID_CODE = user.UserID;
				if (user.Islogon) {
					ShowMainActivity (user.CompCode, user.BranchCode);		
					return;
				} else {
					txtcode.Enabled = false;
					txtcode.SetBackgroundColor (Android.Graphics.Color.Gray); 
					login.Click += (object sender, EventArgs e) => {
						LoginLocal(user);
					};
				}
			} else {
				
				login.Click += (object sender, EventArgs e) => {
					LoginIntoCloud();
				};
			}

		}

		void AlertShow(string text)
		{
			AlertDialog.Builder alert = new AlertDialog.Builder (this);

			alert.SetMessage (text);
			RunOnUiThread (() => {
				alert.Show();
			} );

		}
		private void UpdateDatbase()
		{
			try {
				using (var conn = new SQLite.SQLiteConnection (pathToDatabase)) {
			
					var num = conn.ExecuteScalar<Int32> ("SELECT count(name) FROM sqlite_master WHERE type='table' and name='CNNote'", new object[]{ });
					int count = Convert.ToInt32 (num);
					if (count > 0)
						return;

					conn.CreateTable<CNNote> ();
					conn.CreateTable<CNNoteDtls> ();
					conn.DropTable<AdPara> ();
					conn.CreateTable<AdPara> ();
					conn.DropTable<AdNumDate> ();
					conn.CreateTable<AdNumDate> ();
					conn.DropTable<CompanyInfo> ();
					conn.CreateTable<CompanyInfo> ();
					conn.DropTable<Trader> ();
					conn.CreateTable<Trader> ();
					conn.DropTable<AdUser> ();
					conn.CreateTable<AdUser> ();

					string sql = @"ALTER TABLE Invoice RENAME TO sqlitestudio_temp_table;
									CREATE TABLE Invoice (invno varchar PRIMARY KEY NOT NULL, trxtype varchar, invdate bigint, created bigint, amount float, taxamt float, custcode varchar, description varchar, uploaded bigint, isUploaded integer, isPrinted integer);
									INSERT INTO Invoice (invno, trxtype, invdate, created, amount, taxamt, custcode, description, uploaded, isUploaded,isPrinted) SELECT invno, trxtype, invdate, created, amount, taxamt, custcode, description, uploaded, isUploaded,0 FROM sqlitestudio_temp_table;
									DROP TABLE sqlitestudio_temp_table";
					string[] sqls = sql.Split (new char[]{ ';' });
					foreach (string ssql in sqls) {
						conn.Execute (ssql, new object[]{ });
					}
				}
			} catch (Exception ex) {
				AlertShow (ex.Message);
			}
		}

		void ImportDatabase ()
		{
			var sdcard = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.Path, "erpdata");
			string filename = Path.Combine (sdcard,"erplite.db");
			File.Copy(filename, pathToDatabase, true);
		}

		void ExitApp ()
		{
			((GlobalvarsApp)this.Application).ISLOGON = false;
			Finish ();
			Android.OS.Process.KillProcess (Android.OS.Process.MyPid ());

		}

		void createTable(string pathToDatabase)
		{
			using (var conn= new SQLite.SQLiteConnection(pathToDatabase))
			{
				conn.CreateTable<Item>();
				conn.CreateTable<Invoice>();
				conn.CreateTable<InvoiceDtls>();
				conn.CreateTable<Trader> ();
				conn.CreateTable<AdUser> ();
				conn.CreateTable<CompanyInfo> ();
				conn.CreateTable<AdPara> ();
				conn.CreateTable<AdNumDate> ();
				conn.CreateTable<CNNote>();
				conn.CreateTable<CNNoteDtls>();
				conn.CreateTable<SaleOrder>();
				conn.CreateTable<SaleOrderDtls>();
				conn.CreateTable<DelOrder>();
				conn.CreateTable<DelOrderDtls>();
			}
		}

		private void BeforeReLoginToCloud(AdUser user)
		{
			EditText userid = FindViewById<EditText> (Resource.Id.login_userName);
			EditText passw = FindViewById<EditText> (Resource.Id.login_password);
			EditText code = FindViewById<EditText> (Resource.Id.login_code);
			userid.Text = user.UserID;
			passw.Text = user.Password;
			code.Text = user.CompCode;
			userid.Enabled = false;
			code.Enabled = false;
		}

		private void LoginIntoCloud()
		{
			EditText userid = FindViewById<EditText> (Resource.Id.login_userName);
			EditText passw = FindViewById<EditText> (Resource.Id.login_password);
			EditText code = FindViewById<EditText> (Resource.Id.login_code);
			Button login = FindViewById<Button>(Resource.Id.login);
			login.Enabled = false;
			login.Text = "Please wait...";
			//_client.LoginAsync (userid.Text, passw.Text, code.Text);
			DownloadHelper download= new DownloadHelper();
			download.Downloadhandle = LoginDoneDlg; 
			download.DownloadAllhandle =LoginDoneDlgEx; 
			download.CallingActivity = this;
			download.startLogin(userid.Text, passw.Text, code.Text);
		}

		private void LoginDoneDlg(Activity callingAct,int count,string msg)
		{
			Button login = FindViewById<Button>(Resource.Id.login);
			login.Enabled = true;
			login.Text = "LOGIN";
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
		}

		private void LoginDoneDlgEx(Activity callingAct,int count,string msg)
		{
			Button login = FindViewById<Button>(Resource.Id.login);
			login.Enabled = false;
			//login.Text = "LOGIN";
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
		}

		private void LoginLocal(AdUser user)
		{
			EditText userid = FindViewById<EditText> (Resource.Id.login_userName);
			EditText passw = FindViewById<EditText> (Resource.Id.login_password);

			if (user.UserID.ToUpper () == userid.Text.ToUpper ()) {
				if (user.Password == passw.Text) {
					user.Islogon = true;
					UpdateLogin (user);
					ShowMainActivity (user.CompCode, user.BranchCode);	
				} else {
					DownloadCOmpleted ("Login fail....");
				}
			}else DownloadCOmpleted ("Login fail....");

		}

		void DownloadCOmpleted (string msg)
		{
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			Button login = FindViewById<Button> (Resource.Id.login);
			login.Enabled = true;
			login.Text = "LOGIN";
		}

		void UpdateLogin(AdUser user)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				db.Update (user);
			}
		}

		void ShowMainActivity (string comp,string bran)
		{
			var intent = new Intent (this, typeof(MainActivity));
			//intent.PutExtra ("COMPCODE",comp );
			//intent.PutExtra ("BRANCH",bran );
			((GlobalvarsApp)this.Application).COMPANY_CODE = comp;
			((GlobalvarsApp)this.Application).BRANCH_CODE = bran;
			((GlobalvarsApp)this.Application).ISLOGON = true;
			StartActivity (intent);
		}

		public event nsEventHandler eventHandler;
	
		public void FireEvent(object sender,EventParam eventArgs)
		{
			if (eventHandler != null)
				eventHandler (sender, eventArgs);
		}


		public void PerformEvent(object sender, EventParam e)
		{
			
			switch (e.EventID) {
			case EventID.LOGIN_DOWNCOMPLETE:
				string comp =((GlobalvarsApp)this.Application).COMPANY_CODE;
				string bran = ((GlobalvarsApp)this.Application).BRANCH_CODE;
				ShowMainActivity (comp,bran);
				break;
			case EventID.LOGIN_SUCCESS:
				if (!_donwloadPro) {
					DownloadHelper download = new DownloadHelper ();
					download.Downloadhandle = LoginDoneDlg; 
					download.DownloadAllhandle = LoginDoneDlgEx; 
					download.CallingActivity = this;
					download.StartDownloadAll ();
					_donwloadPro = true;
				}
				break;
			}
		}



	}
}

