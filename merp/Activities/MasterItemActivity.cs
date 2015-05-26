
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
using System.IO;

namespace wincom.mobile.erp
{
	[Activity (Label = "MASTER ITEM CODE")]			
	public class MasterItemActivity : Activity
	{
		ListView listView ;
		List<Item> listData = new List<Item> ();
		string pathToDatabase;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.ItemCodeList);
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.ICodeList);

			Button butInvBack= FindViewById<Button> (Resource.Id.butICodeBack); 
			butInvBack.Click += (object sender, EventArgs e) => {
				base.OnBackPressed();
			};
			SetViewDlg viewdlg = SetViewDelegate;
			//listView.Adapter = new CusotmMasterItemListAdapter(this, listData);

			listView.Adapter = new GenericListAdapter<Item> (this, listData, Resource.Layout.ItemCodeDtlList, viewdlg);
			listView.ItemClick+= ListView_Click;
		}

		void ListView_Click (object sender, AdapterView.ItemClickEventArgs e)
		{
			Item itm = listData.ElementAt (e.Position);
			var intent = new Intent (this, typeof(ItemActivity));
			intent.PutExtra ("icode", itm.ICode);
			StartActivity (intent);
		}

		
		private void SetViewDelegate(View view,object clsobj)
	    {
			Item item = (Item)clsobj;
			view.FindViewById<TextView> (Resource.Id.icodecode).Text = item.ICode;
			view.FindViewById<TextView> (Resource.Id.icodedesc).Text = item.IDesc;
			view.FindViewById<TextView> (Resource.Id.icodeprice).Text = item.Price.ToString ("n3");
			view.FindViewById<TextView> (Resource.Id.icodetax).Text = item.taxgrp;
			view.FindViewById<TextView> (Resource.Id.icodetaxper).Text = item.tax.ToString ("n2");
			view.FindViewById<TextView> (Resource.Id.icodeinclusive).Text = (item.isincludesive)?"INC":"EXC";
		}
		protected override void OnResume()
		{
			base.OnResume();
			listData = new List<Item> ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.ICodeList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<Item> (this, listData, Resource.Layout.ItemCodeDtlList, viewdlg);
		}
		void populate(List<Item> list)
		{

			var documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			pathToDatabase = Path.Combine(documents, "db_adonet.db");

			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{

				var list2 = db.Table<Item>().ToList<Item>();
				foreach(var item in list2)
				{
					list.Add(item);
				}

			}
		}
	}
}

