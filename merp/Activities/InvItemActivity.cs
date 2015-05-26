
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
	[Activity (Label = "INVOICE ITEM(S)")]			
	public class InvItemActivity : Activity
	{
		ListView listView ;
		List<InvoiceDtls> listData = new List<InvoiceDtls> ();
		string pathToDatabase;
		string invno ="";
		string CUSTCODE ="";
		string CUSTNAME ="";
		CompanyInfo comp;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.InvDtlView);
			invno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			CUSTCODE = Intent.GetStringExtra ("custcode") ?? "AUTO";
			Button butNew= FindViewById<Button> (Resource.Id.butnewItem); 
			butNew.Click += (object sender, EventArgs e) => {
				NewItem(invno);
			};

			Button butInvBack= FindViewById<Button> (Resource.Id.butInvItmBack); 
			butInvBack.Click += (object sender, EventArgs e) => {
				StartActivity(typeof(InvoiceActivity));
			};


			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			listView.ItemClick += OnListItemClick;
			//listView.Adapter = new CusotmItemListAdapter(this, listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemView, viewdlg);
		}
		public override void OnBackPressed() {
			// do nothing.
		}

//		protected override void OnSaveInstanceState (Bundle outState)
//		{
//			outState.PutInt ("click_count", _counter);
//
//			// always call the base implementation!
//			base.OnSaveInstanceState (outState);    
//		}

		private void SetViewDelegate(View view,object clsobj)
		{
			InvoiceDtls item = (InvoiceDtls)clsobj;
			string sqty =item.qty==0?"": item.qty.ToString ();
			string sprice =item.price==0?"": item.price.ToString ("n2");

			view.FindViewById<TextView> (Resource.Id.invitemcode).Text = item.icode;
			view.FindViewById<TextView> (Resource.Id.invitemdesc).Text = item.description;
			view.FindViewById<TextView> (Resource.Id.invitemqty).Text = sqty;
			view.FindViewById<TextView> (Resource.Id.invitemtaxgrp).Text = item.taxgrp;
			view.FindViewById<TextView> (Resource.Id.invitemtax).Text = item.tax.ToString ("n2");
			view.FindViewById<TextView> (Resource.Id.invitemprice).Text = sprice;
			view.FindViewById<TextView> (Resource.Id.invitemamt).Text = item.netamount.ToString ("n2");


		}

		protected override void OnResume()
		{
			base.OnResume();
			invno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			CUSTCODE = Intent.GetStringExtra ("custcode") ?? "AUTO";
			listData = new List<InvoiceDtls> ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemView, viewdlg);
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
			InvoiceDtls item = listData.ElementAt (e.Position);
			if (item.icode.IndexOf ("TOTAL") > -1)
				return;
			
			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupItem);
			if (!comp.AllowEdit) {
				menu.Menu.RemoveItem (Resource.Id.popedit);
			}
			if (!comp.AllowDelete) {
				menu.Menu.RemoveItem (Resource.Id.popdelete);
			}
			menu.MenuItemClick += (s1, arg1) => {
				if (arg1.Item.TitleFormatted.ToString().ToLower()=="add")
				{
					NewItem(item.invno);
				}else if (arg1.Item.TitleFormatted.ToString().ToLower()=="edit")
				{
					Edit(item);
				} else if (arg1.Item.TitleFormatted.ToString().ToLower()=="delete")
				{
					Delete(item);
				}
			
			};
			menu.Show ();

		}

		void Delete(InvoiceDtls inv)
		{
			var builder = new AlertDialog.Builder(this);
			builder.SetMessage("Confimr to Delete?");
			builder.SetPositiveButton("YES", (s, e) => { DeleteItem(inv); });
			builder.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
			builder.Create().Show();
		}
		void DeleteItem(InvoiceDtls inv)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<InvoiceDtls>().Where(x=>x.invno==invno&& x.ID==inv.ID).ToList<InvoiceDtls>();
				if (list.Count > 0) {
					db.Delete (list [0]);
					var arrlist= listData.Where(x=>x.invno==invno&& x.ID==inv.ID).ToList<InvoiceDtls>();
					if (arrlist.Count > 0) {
						listData.Remove (arrlist [0]);
						SetViewDlg viewdlg = SetViewDelegate;
						listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemView, viewdlg);
					}
				}
			}
		}
		void Edit(InvoiceDtls inv)
		{
			var intent = new Intent(this, typeof(EntryActivity));
			intent.PutExtra ("invoiceno",inv.invno );
			intent.PutExtra ("itemuid",inv.ID.ToString() );
			intent.PutExtra ("editmode","EDIT" );
			intent.PutExtra ("customer",CUSTNAME );
			intent.PutExtra ("custcode",CUSTCODE );
			StartActivity(intent);
		}

		private void NewItem(string invNo)
		{
			var intent = new Intent(this, typeof(EntryActivity));
			intent.PutExtra ("invoiceno",invNo );
			intent.PutExtra ("itemuid","-1");
			intent.PutExtra ("editmode","NEW" );
			intent.PutExtra ("customer",CUSTNAME );
			intent.PutExtra ("custcode",CUSTCODE );
			StartActivity(intent);
		}

		void populate(List<InvoiceDtls> list)
		{

			var documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			pathToDatabase = Path.Combine(documents, "db_adonet.db");
			comp = DataHelper.GetCompany (pathToDatabase);
			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list1 = db.Table<Invoice>().Where(x=>x.invno==invno).ToList<Invoice>();
				var list2 = db.Table<InvoiceDtls>().Where(x=>x.invno==invno).ToList<InvoiceDtls>();
				var list3 = db.Table<Trader>().Where(x=>x.CustCode==CUSTCODE).ToList<Trader>();

				double ttlamt = 0;
				double ttltax = 0;
				if (list3.Count > 0) {
					CUSTNAME = list3 [0].CustName;
				}
				foreach(var item in list2)
				{
					ttlamt = ttlamt + item.netamount;
					ttltax = ttltax + item.tax;
					list.Add(item);
				}
				if (list1.Count > 0) {
					list1 [0].amount = ttlamt;
					db.Update (list1 [0]);
				}
				InvoiceDtls inv1 = new InvoiceDtls ();
				inv1.icode = "TAX";
				inv1.netamount = ttltax;
				InvoiceDtls inv2 = new InvoiceDtls ();
				inv2.icode = "AMOUNT";
				inv2.netamount = ttlamt;

				list.Add (inv1);
				list.Add (inv2);
			}
		}
	}
}

