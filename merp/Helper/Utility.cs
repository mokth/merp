using System;
using Android.Bluetooth;
using Android.Widget;
using Android.Content;
using Android.App;

namespace wincom.mobile.erp
{
	public class Utility:Activity
	{
		public static DateTime ConvertToDate(string sdate)
		{
			DateTime date = DateTime.Today;  
			string[] para = sdate.Split(new char[]{'-'});
			if (para.Length > 2) {
				int yy = Convert.ToInt32 (para [2]);
				int mm = Convert.ToInt32 (para [1]);
				int dd = Convert.ToInt32 (para [0]);

				date = new DateTime (yy, mm, dd);
			}

			return date;
		}

		public  BluetoothDevice FindBTPrinter(string printername,ref string msg){
			BluetoothAdapter mBluetoothAdapter =null;
			BluetoothDevice mmDevice=null;
			try{
				mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

				if (mBluetoothAdapter ==null)
				{
					msg = "Can not Find Bluetooth device,try again.";	
					return  mmDevice;
				}
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
							break;
						}
					}
				}
				msg = "found device " +mmDevice.Name;
				//Toast.MakeText(this, "found device " +mmDevice.Name, ToastLength.Long).Show ();	

			}catch(Exception ex) {
				
				//Toast.MakeText (this, "Error in Bluetooth device. Try again.", ToastLength.Long).Show ();	
				msg = "Error in Bluetooth device. Try again.";
			}

			return mmDevice;
		}
	}
}

