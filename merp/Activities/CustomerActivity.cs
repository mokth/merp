

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
		GenericListAdapter<Trader> adapter; 
		EditText  txtSearch;
		SetViewDlg viewdlg;
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

			Button butInvBack= FindViewById<Button> (Resource.Id.butCustBack); 
			txtSearch= FindViewById<EditText > (Resource.Id.txtSearch);
			butInvBack.Click += (object sender, EventArgs e) => {
				base.OnBackPressed();
			};

			listView.ItemClick+= ListView_ItemClick; ;
		    //istView.Adapter = new CusotmCustomerListAdapter(this, listData);
			viewdlg = SetViewDelegate;
			adapter = new GenericListAdapter<Trader> (this, listData, Resource.Layout.ListCustDtlView, viewdlg);
			listView.Adapter = adapter;
			txtSearch.TextChanged+= TxtSearch_TextChanged;
		}

		void FindItemByText ()
		{
			List<Trader> found = PerformSearch (txtSearch.Text);
			adapter = new GenericListAdapter<Trader> (this, found, Resource.Layout.ListCustDtlView, viewdlg);
			listView.Adapter = adapter;
		}

		void TxtSearch_TextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			FindItemByText ();
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
			if (txtSearch.Text != "") {
				FindItemByText ();
				return;
			}
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


		List<Trader> PerformSearch (string constraint)
		{
			List<Trader>  results = new List<Trader>();
			if (constraint != null) {
				var searchFor = constraint.ToString ().ToUpper();
				Console.WriteLine ("searchFor:" + searchFor);

				foreach(Trader itm in listData)
				{
					if (itm.CustCode.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.CustName.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
				}


			}
			return results;
		}

	}
}

