using System;
using System.Linq;
using WcfServiceItem;
using System.Collections.Generic;
using Android.App;
using SQLite;
using Android.Widget;
using System.Collections;

namespace wincom.mobile.erp
{
	public class DownloadHelper:Activity
	{
		Service1Client _client;
		WCFHelper _wfc = new WCFHelper();

		public Activity CallingActivity=null;
		public OnUploadDoneDlg Downloadhandle;

		public DownloadHelper ()
		{			
		}

		public void startDownloadItem()
		{
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;

			_client = _wfc.GetServiceClient ();	
			_client.GetItemCodesCompleted+= ClientOnGetItemCompleted;
			_client.GetItemCodesAsync (comp, brn);
		}

		public void startDownloadCustomer()
		{
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			_client = _wfc.GetServiceClient ();	
			_client.GetCustomersCompleted+= ClientOnGetCustomerCompleted;
			_client.GetCustomersAsync (comp, brn);
		}

		public  void startDownloadCompInfo()
		{
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;
			_client = _wfc.GetServiceClient ();	
			_client.GetCompProfileCompleted += ClientOnCompProfCompleted;
			_client.GetCompProfileAsync (comp,brn,userid);
		}

		public  void startDownloadCompInfoEx()
		{
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;
			_client = _wfc.GetServiceClient ();	
			_client.GetCompProfileCompleted += ClientOnCompProfCompletedEx;
			_client.GetCompProfileAsync (comp,brn,userid);
		}

		public void startLogin(string userid, string passw, string code)
		{
			_client = _wfc.GetServiceClient ();	
			_client.LoginCompleted += ClientOnLoginCompleted;
			_client.LoginAsync (userid, passw, code);
		}

		private void ClientOnGetItemCompleted(object sender, GetItemCodesCompletedEventArgs e)
		{
			List<ItemCode> list = new List<ItemCode> ();
			string msg = null;

			if ( e.Error != null)
			{
				msg =  e.Error.Message;
			}
			else if ( e.Cancelled)
			{
				msg = "Request was cancelled.";
			}
			else
			{
				list =  e.Result.ToList<ItemCode>();
				RunOnUiThread (() => InsertItemIntoDb (list));
			}

			if (msg!=null)
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
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
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
			}
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
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
			}
		}

		private void ClientOnLoginCompleted(object sender, LoginCompletedEventArgs e)
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

				RunOnUiThread (() => InsertItemIntoDb (e.Result.ToString()));
			}
			if (!success) {
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
			}
		}

		private void ClientOnCompProfCompletedEx(object sender,GetCompProfileCompletedEventArgs e)
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
				RunOnUiThread (() => InsertCompProfIntoDbEx( pro));
			}
			if (!success) {
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
			}
		}

		private void InsertCompProfIntoDb(CompanyProfile pro)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			DataHelper.InsertCompProfIntoDb (pro, pathToDatabase);
			Downloadhandle.Invoke(CallingActivity,0,"Successfully download.");

		}

		private void InsertItemIntoDb(List<ItemCode> list)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
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
		//	DownloadCOmpleted ("Successfully downloaded.");
		//	EnableButDonwICode ();
			Downloadhandle.Invoke(CallingActivity,list.Count,"Successfully download.");
		}

		private void InsertCustomerIntoDb(List<Customer> list)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
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
			Downloadhandle.Invoke(CallingActivity,list.Count,"Successfully download.");

		}

		private void InsertItemIntoDb(string result)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			string[] para = result.Split (new char[]{ '|' });
			if (para [0] != "OK") {
				Downloadhandle.Invoke(CallingActivity,0,"Fail to Login.");
				return;
			}
			EditText passw =  CallingActivity.FindViewById<EditText> (Resource.Id.login_password);
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
			((GlobalvarsApp)CallingActivity.Application).USERID_CODE = para [1];
			((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE = para [2];
			((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE = para [3];
			//DownloadCOmpleted ("Successfully Logon.");
			Downloadhandle.Invoke(CallingActivity,0,"Successfully Logon.");
			FireEvent (EventID.LOGIN_SUCCESS);
		}

		private void InsertCompProfIntoDbEx(CompanyProfile pro)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			DataHelper.InsertCompProfIntoDb (pro, pathToDatabase);
			Downloadhandle.Invoke(CallingActivity,0,"Successfully Download Company Profile.");
			FireEvent (EventID.LOGIN_DOWNCOMPRO);
		}

		void FireEvent (int eventID)
		{
			Hashtable param = new Hashtable ();
			EventParam p = new EventParam (eventID, param);
			EventManagerFacade.Instance.GetEventManager ().PerformEvent (CallingActivity, p);
		}
	}
}

