
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
using Android.Graphics;

namespace wincom.mobile.erp
{
	[Activity (Label = "SETTINGS")]			
	public class SettingActivity : Activity
	{
		string pathToDatabase;
		Spinner spinner;
		Spinner spinBt;
		ArrayAdapter adapter;
		ArrayAdapter adapterBT;
		BluetoothAdapter mBluetoothAdapter;
		List<string> btdevices = new List<string>();
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.AdPara);
			spinner = FindViewById<Spinner> (Resource.Id.txtSize);
			spinBt= FindViewById<Spinner> (Resource.Id.txtprinters);
			Button butSave = FindViewById<Button> (Resource.Id.ad_bSave);
			Button butCancel = FindViewById<Button> (Resource.Id.ad_Cancel);
			butSave.Click += butSaveClick;
			butCancel.Click += butCancelClick;
			RunOnUiThread(()=>{ findBTPrinter ();});

			adapter = ArrayAdapter.CreateFromResource (this, Resource.Array.papersize_array, Android.Resource.Layout.SimpleSpinnerItem);
			adapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinner.Adapter = adapter;
			spinner.ItemSelected+= Spinner_ItemSelected;
			spinBt.ItemSelected+= Spinner_ItemSelected;
			LoadData ();
			// Create your application here
		}

		void SpinBt_ItemClick (object sender,  AdapterView.ItemSelectedEventArgs e)
		{
			string name = adapterBT.GetItem (e.Position).ToString ();
			TextView txtprinter =FindViewById<TextView> (Resource.Id.txtad_printer);
			txtprinter.Text = name;
		}

		void Spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			((TextView)((Spinner)sender).SelectedView).SetTextColor (Color.Black);

		}

		private void LoadData()
		{
			TextView txtprinter =FindViewById<TextView> (Resource.Id.txtad_printer);
			TextView txtprefix =FindViewById<TextView> (Resource.Id.txtad_prefix);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			AdPara apra = new AdPara ();
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list  = db.Table<AdPara> ().ToList<AdPara> ();
				if (list.Count > 0) {
					apra = list [0];
					txtprefix.Text = apra.Prefix;
					txtprinter.Text = apra.PrinterName;
					int position=adapter.GetPosition (apra.PaperSize);
					if (position>0)
						spinner.SetSelection (position);
					else spinner.SetSelection (0);
				} else {
					txtprefix.Text = "CS";
					txtprinter.Text = "PT-II";
				}
			}
		}
		private void butSaveClick(object sender,EventArgs e)
		{
			TextView txtprinter =FindViewById<TextView> (Resource.Id.txtad_printer);
			TextView txtprefix =FindViewById<TextView> (Resource.Id.txtad_prefix);

			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			AdPara apra = new AdPara ();
			apra.Prefix = txtprefix.Text.ToUpper();
			apra.PrinterName = txtprinter.Text.ToUpper();
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list  = db.Table<AdPara> ().ToList<AdPara> ();
				if (list.Count == 0) {
					db.Insert (apra);		
				} else {
					apra = list [0];
					apra.Prefix = txtprefix.Text.ToUpper();
					apra.PrinterName = txtprinter.Text.ToUpper();
					apra.PaperSize = spinner.SelectedItem.ToString ();
					db.Update (apra);
				}
			}
			base.OnBackPressed();
		}

		private void butCancelClick(object sender,EventArgs e)
		{
			base.OnBackPressed();
		}

		private void findBTPrinter(){
			btdevices.Clear ();
		 try{
				mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

				if (!mBluetoothAdapter.Enable()) {
					Intent enableBluetooth = new Intent(
						BluetoothAdapter.ActionRequestEnable);
					StartActivityForResult(enableBluetooth, 0);
				}


				var pair= mBluetoothAdapter.BondedDevices;
				if (pair.Count > 0) {
					foreach (BluetoothDevice dev in pair) {
						btdevices.Add(dev.Name);
					}
				}

				adapterBT = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, btdevices.ToArray());
				adapterBT.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);
				spinBt.Adapter = adapterBT;
				spinBt.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> ( SpinBt_ItemClick );
				//txtv.Text ="found device " +mmDevice.Name;
			}catch(Exception ex) {

			}

		}
	}
}

