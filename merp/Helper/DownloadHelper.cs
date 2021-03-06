﻿using System;
using System.Linq;
using WcfServiceItem;
using System.Collections.Generic;
using Android.App;
using SQLite;
using Android.Widget;
using System.Collections;

namespace wincom.mobile.erp
{
	public class DownloadHelper:Activity,IEventListener
	{
		Service1Client _client;
		WCFHelper _wfc = new WCFHelper();

		public Activity CallingActivity=null;
		public OnUploadDoneDlg Downloadhandle;
		public OnUploadDoneDlg DownloadAllhandle;
		public volatile static bool _downloadAll = false;
		public volatile static bool _downloadPro = false;
		public volatile static bool _downloadItem = false;
		public volatile static bool _downloadCust = false;

		public DownloadHelper ()
		{
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
		}

		public void StartDownloadAll()
		{
			_downloadAll = true;
			startDownloadCompInfo(true) ;
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

		public  void startDownloadCompInfo(bool isdownloadAll)
		{
			_downloadAll =isdownloadAll;
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;
			_client = _wfc.GetServiceClient ();	
			_client.GetCompProfileCompleted += ClientOnCompProfCompleted;
			_client.GetCompProfileAsync (comp,brn,userid);
		}

//		public  void startDownloadCompInfoEx()
//		{
//			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
//			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
//			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;
//			_client = _wfc.GetServiceClient ();	
//			_client.GetCompProfileCompleted += ClientOnCompProfCompletedEx;
//			_client.GetCompProfileAsync (comp,brn,userid);
//		}

		public void startDownloadRunNoInfo()
		{
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;
			_client = _wfc.GetServiceClient ();	
			_client.GetRunnoCompleted+= _client_GetRunnoCompleted;
			_client.GetRunnoAsync(comp,brn,userid);
		}

	
		public void startLogin(string userid, string passw, string code)
		{
			_client = _wfc.GetServiceClient ();	
			PhoneTool ptool = new PhoneTool ();
			string serial =ptool.DeviceIdIMEI();
			_client.LoginExCompleted += ClientOnLoginCompleted;
			_client.LoginExAsync(userid, passw, code,serial);
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

			if (msg != null) {
				RunOnUiThread (() => Downloadhandle.Invoke (CallingActivity, 0, msg));
				if (_downloadAll) {
					_downloadAll = false;	
					FireEvent (EventID.LOGIN_DOWNCOMPLETE);
				}
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
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
				if (_downloadAll) {
					_downloadAll = false;	
					FireEvent (EventID.LOGIN_DOWNCOMPLETE);
				}
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
				if (_downloadAll) {
					_downloadAll = false;	
					FireEvent (EventID.LOGIN_DOWNCOMPLETE);
				}
			}
		}

		private void ClientOnLoginCompleted(object sender, LoginExCompletedEventArgs e)
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

		void _client_GetRunnoCompleted (object sender, GetRunnoCompletedEventArgs e)
		{
			List<RunnoInfo> list = new List<RunnoInfo> ();
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
				list =  e.Result.ToList<RunnoInfo>();
				RunOnUiThread (() => InsertRunoIntoDb(list));
			}
			if (!success) {
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
				if (_downloadAll) {
					_downloadAll = false;	
					FireEvent (EventID.LOGIN_DOWNCOMPLETE);
				}

			}
		}

		private void InsertRunoIntoDb(List<RunnoInfo> list)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;

			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<AdNumDate> ().ToList<AdNumDate> ();
				foreach (var runinfo in list) {
				
					var found = list2.Where (x => x.Month == runinfo.Month && x.Year == runinfo.Year && x.TrxType == runinfo.Trxtype).ToList ();
					if (found.Count > 0) {
						found [0].RunNo = runinfo.RunNo;
						db.Update (found [0]);
					} else {
						AdNumDate num = new AdNumDate ();
						num.ID = -1;
						num.Month = runinfo.Month;
						num.Year = runinfo.Year;
						num.RunNo = runinfo.RunNo;
						num.TrxType = runinfo.Trxtype;
						db.Insert (num);
					}
				}
			}
			if (_downloadAll) {
				_downloadPro = true;
				DownloadAllhandle.Invoke (CallingActivity, 0, "Successfully downloaded runing no.");
				FireEvent (EventID.DOWNLOADED_RUNNO);

			} else
				if (CallingActivity!=null)
					Downloadhandle.Invoke (CallingActivity, 0, "Successfully downloaded runing no.");
			
		}
		void AlertShow(string text)
		{
			AlertDialog.Builder alert = new AlertDialog.Builder (this);

			alert.SetMessage (text);
			RunOnUiThread (() => {
				alert.Show();
			} );

		}
		private void InsertCompProfIntoDb(CompanyProfile pro)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			try{
				DataHelper.InsertCompProfIntoDb (pro, pathToDatabase);
			}
			catch(Exception ex) {
				AlertShow (ex.Message + ex.StackTrace);
			}
			//DownloadAllhandle.Invoke(CallingActivity,0,"Successfully downloaded Profile.");
			startDownloadRunNoInfo ();

//			if (_downloadAll) {
//				DownloadAllhandle.Invoke(CallingActivity,0,"Successfully downloaded Profile.");
//				FireEvent (EventID.DOWNLOADED_PROFILE);
//
//			}else Downloadhandle.Invoke(CallingActivity,0,"Successfully downloaded Profile.");
		}

		private void InsertItemIntoDb(List<ItemCode> list)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				db.DeleteAll<Item> ();
				foreach (ItemCode item in list) {
					Item itm = new Item ();
					itm.ICode = item.ICode;
					itm.IDesc = item.IDesc;
					itm.Price = item.Price;
					itm.tax = item.tax;
					itm.taxgrp = item.taxgrp;
					itm.isincludesive = item.isincludesive;

					db.Insert (itm);

				}
			}
		

			if (_downloadAll) {
				_downloadItem = true;
				DownloadAllhandle.Invoke(CallingActivity,list.Count,"Successfully downloaded"+list.Count.ToString()+"Item(s).");
				FireEvent (EventID.DOWNLOADED_ITEM);
			}
			else Downloadhandle.Invoke(CallingActivity,list.Count,"Successfully downloaded"+list.Count.ToString()+"Item(s).");
		}

		private void InsertCustomerIntoDb(List<Customer> list)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				db.DeleteAll<Trader> ();// (list2);
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
					itm.PayCode = item.PayCode;
					db.Insert (itm);

				}
			}

			if (_downloadAll) {
				FireEvent (EventID.LOGIN_DOWNCOMPLETE);
				DownloadAllhandle.Invoke(CallingActivity,list.Count,"Successfully downloaded "+list.Count.ToString()+" customer(s).");
			}else Downloadhandle.Invoke(CallingActivity,list.Count,"Successfully downloaded "+list.Count.ToString()+" customer(s).");

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
			//Downloadhandle.Invoke(CallingActivity,0,"Successfully Logon.");
			FireEvent (EventID.LOGIN_SUCCESS);
		}

		private void InsertCompProfIntoDbEx(CompanyProfile pro)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			DataHelper.InsertCompProfIntoDb (pro, pathToDatabase);

			//FireEvent (EventID.LOGIN_DOWNCOMPRO);
			if (_downloadAll) {
				DownloadAllhandle.Invoke(CallingActivity,0,"Successfully Download Company Profile.");
				FireEvent (EventID.DOWNLOADED_PROFILE);
			}
			else Downloadhandle.Invoke(CallingActivity,0,"Successfully Download Company Profile.");
		}

		void FireEvent (int eventID)
		{
			Hashtable param = new Hashtable ();
			EventParam p = new EventParam (eventID, param);
			EventManagerFacade.Instance.GetEventManager ().PerformEvent (CallingActivity, p);
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
			case EventID.DOWNLOADED_PROFILE:
				startDownloadRunNoInfo ();
				break;
			case EventID.DOWNLOADED_RUNNO:
				if (_downloadAll) {
					if (_downloadPro) {
						_downloadPro = false;
						startDownloadItem ();
					}
				}
				break;
			case EventID.DOWNLOADED_ITEM:
				if (_downloadAll) {
					if (_downloadItem) {
						_downloadItem = false;
						startDownloadCustomer ();
					}
				}
				break;
			
			}
		}
	}
}

