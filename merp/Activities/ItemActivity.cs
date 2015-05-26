
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
	[Activity (Label = "ITEM INFO")]			
	public class ItemActivity : Activity
	{
		string pathToDatabase;
		string ICODE;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.ItemInfo);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			ICODE= Intent.GetStringExtra ("icode") ?? "AUTO";
			Button but = FindViewById<Button> (Resource.Id.btnItem_OK);
			but.Click+= (object sender, EventArgs e) => 
			{
				base.OnBackPressed();
			};
			LoadData ();
			// Create your application here
		}

		private void LoadData()
		{
			Item itm = new Item ();
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<Item>().Where(x=>x.ICode==ICODE).ToList<Item>();
				if (list.Count > 0) {
					itm = list [0];
				}
			}

			DisplayData (itm); 
		}

		private void DisplayData(Item itm)
		{
			TextView code = FindViewById<TextView> (Resource.Id.txtitem_code);
			TextView name = FindViewById<TextView> (Resource.Id.txtitem_desc);
			TextView price = FindViewById<TextView> (Resource.Id.txtitem_price);
			TextView tax = FindViewById<TextView> (Resource.Id.txtitem_tax);
			TextView taxgrp= FindViewById<TextView> (Resource.Id.txtitem_taxgrop);
			TextView incl= FindViewById<TextView> (Resource.Id.txtitem_incl);
			code.Text = itm.ICode;
			name.Text = itm.IDesc;
			price.Text = itm.Price.ToString ("n3");
			tax.Text= itm.tax.ToString ("n2");
			taxgrp.Text = itm.taxgrp;
			if (itm.isincludesive)
				incl.Text = "YES";
			else incl.Text = "NO";
		}
	}
}

