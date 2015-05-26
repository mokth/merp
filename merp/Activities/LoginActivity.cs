
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
	public class LoginActivity : Activity
	{
		public static readonly EndpointAddress EndPoint = new EndpointAddress("http://www.wincomcloud.com/Wfc/Service1.svc");
		private Service1Client _client;
		string pathToDatabase;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.SignIn);
			// Create your application here

			Button login = FindViewById<Button>(Resource.Id.login);
			Button bexit = FindViewById<Button>(Resource.Id.exit);
			EditText txtcode = FindViewById<EditText> (Resource.Id.login_code);
			bexit.Click += (object sender, EventArgs e) => {
				Finish();
				Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
			};
			InitializeServiceClient();
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;

			//SQLiteConnection...CreateFile(pathToDatabase);
			if (!File.Exists (pathToDatabase)) {
				createTable (pathToDatabase);
			} 

			AdUser user=null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<AdUser> ().ToList<AdUser> ();
				if (list2.Count > 0) {
					user = list2 [0];
				}
			}

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
			}
		}
		private void LoginIntoCloud()
		{
			EditText userid = FindViewById<EditText> (Resource.Id.login_userName);
			EditText passw = FindViewById<EditText> (Resource.Id.login_password);
			EditText code = FindViewById<EditText> (Resource.Id.login_code);
			Button login = FindViewById<Button>(Resource.Id.login);
			login.Enabled = false;
			login.Text = "Please wait...";
			_client.LoginAsync (userid.Text, passw.Text, code.Text);
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

		void UpdateLogin(AdUser user)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				db.Update (user);
			}
		}
		private void ClientOnLoginCompleted(object sender, LoginCompletedEventArgs e)
		{
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				Log.Error ("Login",e.Error.Message);
				Log.Error ("Login",e.Error.StackTrace);
				success = false;
			}
			else if ( e.Cancelled)
			{
				msg = "Request was cancelled.";
				success = false;
			}
			else
			{
				
				RunOnUiThread (() => InsertItemIntoDb (e.Result.ToString()));
			}
			if (!success) {
				RunOnUiThread (() => DownloadCOmpleted (msg));
			}
		}

		private void ClientOnCompProfCompleted(object sender,GetCompProfileCompletedEventArgs e)
		{
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				Log.Error ("Login",e.Error.Message);
				Log.Error ("Login",e.Error.StackTrace);
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
		private void InitializeServiceClient()
		{
			BasicHttpBinding binding = CreateBasicHttp();

			_client = new Service1Client(binding, EndPoint);
			_client.LoginCompleted+= ClientOnLoginCompleted;
			_client.GetCompProfileCompleted+=ClientOnCompProfCompleted;

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
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			Button login = FindViewById<Button>(Resource.Id.login);
			login.Enabled = true;
			login.Text = "LOGIN";
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

		private void InsertItemIntoDb(string result)
		{
			string[] para = result.Split (new char[]{ '|' });
			if (para [0] != "OK") {
				DownloadCOmpleted ("Fail to logon.");
				return;
			}
			EditText passw = FindViewById<EditText> (Resource.Id.login_password);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<AdUser>().ToList<AdUser>();
				list2.RemoveAll (x => x.UserID == para[1]);
				AdUser user = new AdUser ();
				user.BranchCode = para [3];
				user.CompCode = para [2];
				user.Islogon = true;
				user.Password = passw.Text;
				user.UserID = para [1];
				db.Insert (user);
			}
			((GlobalvarsApp)this.Application).USERID_CODE = para [1];
			((GlobalvarsApp)this.Application).COMPANY_CODE = para [2];
			((GlobalvarsApp)this.Application).BRANCH_CODE = para [3];
			DownloadCOmpleted ("Successfully Logon.");
			_client.GetCompProfileAsync(para[2],para[3],para [1]);

		}

		private void InsertCompProfIntoDb(CompanyProfile pro)
		{
			DataHelper.InsertCompProfIntoDb (pro, pathToDatabase);
			DownloadCOmpleted ("Successfully Download Company Profile.");
			string comp =((GlobalvarsApp)this.Application).COMPANY_CODE;
			string bran = ((GlobalvarsApp)this.Application).BRANCH_CODE;
			ShowMainActivity (comp,bran);
		}
	}
}

