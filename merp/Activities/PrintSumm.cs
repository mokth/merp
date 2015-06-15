
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
using Android.Bluetooth;

namespace wincom.mobile.erp
{
	[Activity (Label = "PRINT SUMMARY")]			
	public class PrintSumm : Activity
	{
		BluetoothAdapter mBluetoothAdapter;
		BluetoothSocket mmSocket;
		BluetoothDevice mmDevice;
		string pathToDatabase;
		AdPara apara;
		const int DATE_DIALOG_ID1 = 0;
		const int DATE_DIALOG_ID2 = 1;
		DateTime date;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.PrintInvSumm);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			Button butPrint= FindViewById<Button> (Resource.Id.printsumm); 
			Button butInvBack= FindViewById<Button> (Resource.Id.printsumm_cancel); 
			EditText frd = FindViewById<EditText> (Resource.Id.trxdatefr);
			EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			frd.Text = DateTime.Today.ToString ("dd-MM-yyyy");
			frd.Click += delegate(object sender, EventArgs e) {
				ShowDialog (DATE_DIALOG_ID1);
			};
			tod.Text = DateTime.Today.ToString ("dd-MM-yyyy");
			date =  DateTime.Today;
			tod.Click += delegate(object sender, EventArgs e) {
				
				ShowDialog (DATE_DIALOG_ID2);
			};
			butInvBack.Click += (object sender, EventArgs e) => {
				StartActivity(typeof(MainActivity));
			};
			butPrint.Click+= ButPrint_Click;
		}
		protected override Dialog OnCreateDialog (int id)
		{
			switch (id) {
			case DATE_DIALOG_ID1:
				return new DatePickerDialog (this, OnDateSet1, date.Year, date.Month - 1, date.Day); 
			case DATE_DIALOG_ID2:
				return new DatePickerDialog (this, OnDateSet2, date.Year, date.Month - 1, date.Day); 
			}
			return null;
		}
		void OnDateSet1 (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			EditText frd = FindViewById<EditText> (Resource.Id.trxdatefr);
			//EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			frd.Text = e.Date.ToString("dd-MM-yyyy");
		}
		void OnDateSet2 (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			tod.Text = e.Date.ToString("dd-MM-yyyy");
		}

		void ButPrint_Click (object sender, EventArgs e)
		{
			EditText frd = FindViewById<EditText> (Resource.Id.trxdatefr);
			EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			DateTime? sdate=null;
			DateTime? edate=null;
			if (frd.Text != "") {
				sdate = Utility.ConvertToDate (frd.Text);
			} else
				sdate = DateTime.Today;

			if (tod.Text != "") {
				edate =Utility.ConvertToDate (tod.Text);
			}
			if (edate == null)
				edate = sdate;
			PrintInvSumm (sdate.Value, edate.Value);
		}



		void PrintInvSumm(DateTime printdate1,DateTime printdate2)
		{
			mmDevice = null;
			findBTPrinter ();

			if (mmDevice == null)
				return;

			string userid = ((GlobalvarsApp)this.Application).USERID_CODE;
			PrintInvHelper prnHelp = new PrintInvHelper (pathToDatabase, userid);
			string msg = prnHelp.PrintInvSumm(mmSocket, mmDevice,printdate1,printdate2);
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	

		}

		void findBTPrinter(){
			string printername = apara.PrinterName.Trim ().ToUpper ();
		
			try{
				mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

				string txt ="";
				if (!mBluetoothAdapter.Enable()) {
					Intent enableBluetooth = new Intent(
						BluetoothAdapter.ActionRequestEnable);
					StartActivityForResult(enableBluetooth, 0);
				}


				var pair= mBluetoothAdapter.BondedDevices;
				if (pair.Count > 0) {
					foreach (BluetoothDevice dev in pair) {
						Console.WriteLine (dev.Name);
						txt = txt+","+dev.Name;
						if (dev.Name.ToUpper()==printername)
						{
							mmDevice = dev;
							//							File.WriteAllText(addrfile,dev.Address);
							break;
						}
					}
				}
				Toast.MakeText (this, "found device " +mmDevice.Name, ToastLength.Long).Show ();	
				//txtv.Text ="found device " +mmDevice.Name;
			}catch(Exception ex) {

				//txtv.Text = ex.Message;
				Toast.MakeText (this, ex.Message, ToastLength.Long).Show ();	
			}
		}
	}
}

