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

namespace wincom.mobile.erp
{
	/// <summary>
	/// Information about Device (PhoneNumber Imei
	/// </summary>
	public class PhoneTool
	{
		// http://mono-for-android.1047100.n5.nabble.com/Device-ID-is-NOT-unique-in-Android-td4869634.html
		// Nope the Android ID is not unique - and on some hardware it's possible 
		// to get a null back.  There are a couple of threads about this on the 
		// Android Developers mailing list.  Long story short - it can't be 
		// relied on as a unique id. 

		// http://stackoverflow.com/questions/2785485/is-there-a-unique-android-device-id
		// http://stackoverflow.com/questions/2480288/get-phone-number-in-android-sdk

		//String myIMSI = Android.OS.Environment.SystemProperties.get(android.telephony.TelephonyProperties.PROPERTY_IMSI);
		// within my emulator it returns:   310995000000000

		//String myIMEI = android.os.SystemProperties.get(android.telephony.TelephonyProperties.PROPERTY_IMEI);
		// within my emulator it returns:   000000000000000
		//Parsed in 0.030 seconds, using GeSHi 1.0.8.4

		// http://android-developers.blogspot.com/2011/03/identifying-app-installations.html
		// http://mono-for-android.1047100.n5.nabble.com/getting-IMEI-td4616158.html


		// five different ID types:
		//	http://www.pocketmagic.net/2011/02/android-unique-device-id/#.Uda0f4V_B8E
		// 
		// IMEI (only for Android devices with Phone use; needs android.permission.READ_PHONE_STATE)
		// Pseudo-Unique ID (for all Android devices)
		// Android ID (can be null, can change upon factory reset, can be altered on rooted phone)
		// WLAN MAC Address string (needs android.permission.ACCESS_WIFI_STATE)
		// BT MAC Address string (devices with Bluetooth, needs android.permission.BLUETOOTH)

		// http://lists.ximian.com/pipermail/monodroid/2011-May/004705.html

		public string Device()
		{
			return Android.OS.Build.Device;
		}

		public string Manufacturer()
		{
			return Android.OS.Build.Manufacturer;
		}

		public string Brand()
		{
			return Android.OS.Build.Brand;
		}

		public string Product()
		{
			return Android.OS.Build.Product;
		}

		public string Model()
		{
			return Android.OS.Build.Model;
		}

		public string Serial()
		{
			# if __ANDROID_4__ || __ANDROID_5__ || __ANDROID_6__ || __ANDROID_7__ || __ANDROID_8__
			return  "N/A";
			# else
			// Not in MfA ATM (new in 2.3)
			return Android.OS.Build.Serial;
			# endif
		}

		// http://stackoverflow.com/questions/2002288/static-way-to-get-context-on-android
		private Android.Content.Context Context = Android.App.Application.Context;

		public string UDID()
		{
			return Android.Provider.Settings.Secure.GetString
				(
					Android.App.Application.Context.ContentResolver
					, Android.Provider.Settings.Secure.AndroidId
				);
		}

		// Permission READ_PHONE_STATE
		Android.Telephony.TelephonyManager telephony_manager;

		public string PhoneNumber()
		{
			telephony_manager =
				(Android.Telephony.TelephonyManager)
				Context.GetSystemService(Android.Content.Context.TelephonyService);

			return telephony_manager.Line1Number;
		}

		public string DeviceIdIMEI()
		{
			Context = Android.App.Application.Context;

			telephony_manager =
				(Android.Telephony.TelephonyManager)
				Context.GetSystemService(Android.Content.Context.TelephonyService);
			return telephony_manager.DeviceId;
		}

		// Another way is to use /sys/class/android_usb/android0/iSerial in an App with no 
		// permissions whatsoever.
		//
		//	user@creep:~$ adb shell ls -l /sys/class/android_usb/android0/iSerial
		//	-rw-r--r-- root     root         4096 2013-01-10 21:08 iSerial
		//	user@creep:~$ adb shell cat /sys/class/android_usb/android0/iSerial
		//	0A3CXXXXXXXXXX5
		//	To do this in java one would just use a FileInputStream to open the iSerial 
		//	file and read out the characters. Just be sure you wrap it in an exception 
		//	handler because not all devices have this file.
		//	
		//	At least the following devices are known to have this file world-readable:
		//	
		//	Galaxy Nexus
		//	Nexus S
		//	Motorola Xoom 3g
		//	Toshiba AT300
		//	HTC One V
		//	Mini MK802
		//	Samsung Galaxy S II
		//	http://insitusec.blogspot.com/2013/01/leaking-android-hardware-serial-number.html
		public string ISerial()
		{
			string retval = "";

			try
			{
				string filename = @"/sys/class/android_usb/android0/iSerial";
				using (System.IO.TextReader reader = System.IO.File.OpenText(filename))
				{
					retval = reader.ReadToEnd();
				}
			}
			catch (System.Exception exc)
			{
				retval = exc.ToString() + " : " + exc.Message;
			}

			return retval;
		}

		public string MACAddress()
		{
			string retval = "N/A";
			try
			{
				System.Net.NetworkInformation.NetworkInterface[] nics;
				nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

				String macs = string.Empty;
				foreach (System.Net.NetworkInformation.NetworkInterface adapter in nics)
				{
					if (macs == String.Empty)// only return MAC Address from first card  
					{
						System.Net.NetworkInformation.IPInterfaceProperties properties;

						properties = adapter.GetIPProperties();
						macs = adapter.GetPhysicalAddress().ToString();
					}
				}

				retval = macs;
			}
			catch (System.Exception exc)
			{
				retval = exc.ToString() + " : " + exc.Message;
			}

			return retval;
		}

		// requires ACCESS_WIFI_STATE in Properties
		Android.Net.Wifi.WifiManager wifi_manager = null;

		public string MACAddressWiFi()
		{
			wifi_manager =
				(Android.Net.Wifi.WifiManager)
				Context.GetSystemService(Android.Content.Context.WifiService);

			string mac_address = wifi_manager.ConnectionInfo.MacAddress;
			if (!wifi_manager.IsWifiEnabled)
			{
				mac_address = "DISABLED";
			}
			if (mac_address == "")
			{
				mac_address = "UNKNOWN";
			}

			//Log.Debug
			//	(
			//	  "INFO", "{0} {1} {2} {3} {4} {5} {6} {7}"
			//	, id, device, model, manufacturer, brand, deviceId, serialNumber, mac_address
			//	);
			// Simulator: 9774d56d682e549c generic sdk unknown generic 000000000000000 89014103211118510720 DISABLED

			return mac_address;
		}



		// http://developer.android.com/guide/topics/connectivity/bluetooth.html

		/// <summary>
		/// Unhandled Exception:
		/// 
		/// Java.Lang.SecurityException: 
		/// Need BLUETOOTH permission: Neither user 10122 nor current process has \
		/// android.permission.BLUETOOTH.
		/// </summary>
		/// <returns></returns>
		public string MACAddressBluetooth()
		{
			string retval = "N/A";

			Android.Bluetooth.BluetoothAdapter bt_adapter;
			bt_adapter = Android.Bluetooth.BluetoothAdapter.DefaultAdapter;

			// if device does not support Bluetooth
			if (bt_adapter == null)
			{
				retval = "device does not support bluetooth";
			}
			else
			{
				retval = bt_adapter.Address;
			}


			return retval;
		}

		public string BluetoothLocalName()
		{
			string retval = "";

			Android.Bluetooth.BluetoothAdapter bt_adapter;
			bt_adapter = Android.Bluetooth.BluetoothAdapter.DefaultAdapter;

			// if device does not support Bluetooth
			if (bt_adapter == null)
			{
				retval = "device does not support bluetooth";
			}
			else
			{
				retval = bt_adapter.Name;
			}


			return retval;
		}
	}
}