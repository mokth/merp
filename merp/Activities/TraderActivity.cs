
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
	[Activity (Label = "CUSTOMER INFO")]			
	public class TraderActivity : Activity
	{
		string pathToDatabase;
		string CUSTCODE;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.TraderInfo);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			CUSTCODE= Intent.GetStringExtra ("custcode") ?? "AUTO";
			Button but = FindViewById<Button> (Resource.Id.btnTrd_OK);
			but.Click+= (object sender, EventArgs e) => 
			{
				base.OnBackPressed();
			};
			LoadData ();
			// Create your application here
		}

		private void LoadData()
		{
			Trader trd = new Trader ();
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<Trader>().Where(x=>x.CustCode==CUSTCODE).ToList<Trader>();
				if (list.Count > 0) {
					trd = list [0];
				}
			}

			DisplayData (trd); 
		}

		private void DisplayData(Trader trd)
		{
			TextView code = FindViewById<TextView> (Resource.Id.txtcust_code);
			TextView name = FindViewById<TextView> (Resource.Id.txtcust_name);
			TextView addr1 = FindViewById<TextView> (Resource.Id.txtcust_addr1);
			TextView addr2 = FindViewById<TextView> (Resource.Id.txtcust_addr2);
			TextView addr3 = FindViewById<TextView> (Resource.Id.txtcust_addr3);
			TextView addr4 = FindViewById<TextView> (Resource.Id.txtcust_addr4);
			TextView tel = FindViewById<TextView> (Resource.Id.txtcust_tel);
			TextView fax = FindViewById<TextView> (Resource.Id.txtcust_fax);
			TextView gst= FindViewById<TextView> (Resource.Id.txtcust_gst);
			code.Text = trd.CustCode;
			name.Text = trd.CustName;
			addr1.Text = trd.Addr1;
			addr2.Text = trd.Addr2;
			addr3.Text = trd.Addr3;
			addr4.Text = trd.Addr4;
			tel.Text = "TEL: "+trd.Tel;
			fax.Text ="FAX: "+ trd.Fax;
			gst.Text ="GST: "+ trd.gst;

		}
	}
}

