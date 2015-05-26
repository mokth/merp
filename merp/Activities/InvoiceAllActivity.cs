using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Bluetooth;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using SQLite;
using System.Threading;
using Java.Util;

namespace wincom.mobile.erp
{
	[Activity (Label = "INVOICE LIST")]			
	public class InvoiceAllActivity : Activity
	{
		ListView listView ;
		List<Invoice> listData = new List<Invoice> ();
		string pathToDatabase;
		BluetoothAdapter mBluetoothAdapter;
		BluetoothSocket mmSocket;
		BluetoothDevice mmDevice;
		//Thread workerThread;
		Stream mmOutputStream;
		AdPara apara=null;
		CompanyInfo compinfo;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			// Create your application here
			SetContentView (Resource.Layout.ListView);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
//			TableLayout tlay = FindViewById<TableLayout> (Resource.Id.tableLayout1);
//			tlay.Visibility = ViewStates.Invisible;
			Button butNew= FindViewById<Button> (Resource.Id.butnewInv); 
			butNew.Visibility = ViewStates.Invisible;
			Button butInvBack= FindViewById<Button> (Resource.Id.butInvBack); 
			butInvBack.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(MainActivity));
			};

			listView.ItemClick += OnListItemClick;
			listView.ItemLongClick += OnListItemLongClick;
			//listView.Adapter = new CusotmListAdapter(this, listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<Invoice> (this, listData, Resource.Layout.ListItemRow, viewdlg);

		}

		private void SetViewDelegate(View view,object clsobj)
		{
			Invoice item = (Invoice)clsobj;
			view.FindViewById<TextView> (Resource.Id.invdate).Text = item.invdate.ToString ("dd-MM-yy");
			view.FindViewById<TextView> (Resource.Id.invno).Text = item.invno;
			view.FindViewById<TextView>(Resource.Id.invcust).Text = item.description;
			view.FindViewById<TextView> (Resource.Id.Amount).Text = item.amount.ToString("n2");

		}

		protected override void OnResume()
		{
			base.OnResume();
			listData = new List<Invoice> ();
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<Invoice> (this, listData, Resource.Layout.ListItemRow, viewdlg);
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
			Invoice item = listData.ElementAt (e.Position);
			var intent = new Intent(this, typeof(InvItemHisActivity));
			intent.PutExtra ("invoiceno",item.invno );
			intent.PutExtra ("custcode",item.custcode );
			StartActivity(intent);
		}

		void OnListItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e) {
			Invoice item = listData.ElementAt (e.Position);
			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupInv);
			menu.Menu.RemoveItem (Resource.Id.popInvdelete);
			menu.Menu.RemoveItem (Resource.Id.popInvadd);
			menu.MenuItemClick += (s1, arg1) => {
				
				if (arg1.Item.TitleFormatted.ToString ().ToLower () == "print") {
					PrintInv (item);	
				} 
			};
			menu.Show ();
		}

		void populate(List<Invoice> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<Invoice>().Where(x=>x.isUploaded==true).ToList<Invoice>();
				foreach(var item in list2)
				{
					list.Add(item);
				}

			}
			compinfo = DataHelper.GetCompany (pathToDatabase);
		}


		void PrintInv(Invoice inv)
		{
			Toast.MakeText (this, "print....", ToastLength.Long).Show ();	
			InvoiceDtls[] list;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var ls= db.Table<InvoiceDtls> ().Where (x => x.invno==inv.invno).ToList<InvoiceDtls>();
				list = new InvoiceDtls[ls.Count];
				ls.CopyTo (list);
			}
			mmDevice = null;
			findBTPrinter ();
			if (mmDevice != null) {
				StartPrint (inv, list);
			}
		}

		void StartPrint(Invoice inv,InvoiceDtls[] list )
		{
			string userid = ((GlobalvarsApp)this.Application).USERID_CODE;
			PrintInvHelper prnHelp = new PrintInvHelper (pathToDatabase, userid);
			string msg =prnHelp.OpenBTAndPrint (mmSocket, mmDevice, inv, list);
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
						if (dev.Name.IndexOf(printername) >= 0)
						{
							mmDevice = dev;
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

