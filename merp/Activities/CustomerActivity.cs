

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
	
	[Activity (Label = "CUSTOMER PROFILE")]			
	public class CustomerActivity : Activity
	{
		List<Trader> listData = new List<Trader> ();
		ListView listView ;
		string pathToDatabase;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.ListCustView);
			// Create your application here
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.CustList);
			int cc =listView.ChildCount;

			Button butInvBack= FindViewById<Button> (Resource.Id.butCustBack); 
			butInvBack.Click += (object sender, EventArgs e) => {
				base.OnBackPressed();
			};

			listView.ItemClick+= ListView_ItemClick; ;
		    //istView.Adapter = new CusotmCustomerListAdapter(this, listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<Trader> (this, listData, Resource.Layout.ListCustDtlView, viewdlg);
		}

		void ListView_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			Trader item = listData.ElementAt (e.Position);
			var intent = new Intent (this, typeof(TraderActivity));
			intent.PutExtra ("custcode", item.CustCode);
			StartActivity (intent);
		}
		private void SetViewDelegate(View view,object clsobj)
		{
			Trader item = (Trader)clsobj;
			view.FindViewById<TextView> (Resource.Id.custcode).Text = item.CustCode;
			view.FindViewById<TextView> (Resource.Id.custname).Text = item.CustName;

		}
		protected override void OnResume()
		{
			base.OnResume();
			listData = new List<Trader> ();
			populate (listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<Trader> (this, listData, Resource.Layout.ListCustDtlView, viewdlg);
		}

		void populate(List<Trader> list)
		{

			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{

				var list2 = db.Table<Trader>().ToList<Trader>();
				foreach(var item in list2)
				{
					list.Add(item);
				}

			}
		}

	}
}

