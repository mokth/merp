﻿
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
using Android.Views.InputMethods;

namespace wincom.mobile.erp
{
	[Activity (Label = "INVOICE ITEM ENTRY")]			
	public class EntryActivity : Activity
	{
		string pathToDatabase;
		List<Item> items = null;
		string EDITMODE ="";
		string CUSTOMER ="";
		string CUSTCODE ="";
		string ITEMUID ="";
		string INVOICENO="";
		string FIRSTLOAD="";
		ArrayAdapter<String> dataAdapter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}

			INVOICENO = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			ITEMUID = Intent.GetStringExtra ("itemuid") ?? "AUTO";
			EDITMODE = Intent.GetStringExtra ("editmode") ?? "AUTO";
			CUSTOMER= Intent.GetStringExtra ("customer") ?? "AUTO";
			CUSTCODE= Intent.GetStringExtra ("custcode") ?? "AUTO";
			// Create your application here
			SetContentView (Resource.Layout.Entry);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.txtcode);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			TextView txtInvNo =  FindViewById<TextView> (Resource.Id.txtInvnp);
			TextView txtcust =  FindViewById<TextView> (Resource.Id.txtInvcust);
			txtInvNo.Text = INVOICENO;
			txtcust.Text = CUSTOMER;
			Button but = FindViewById<Button> (Resource.Id.Save);
			Button but2 = FindViewById<Button> (Resource.Id.Cancel);
			spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
			but.Click += butSaveClick;
			but2.Click += butCancelClick;

			qty.EditorAction += HandleEditorAction;
			price.EditorAction += HandleEditorAction; 
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;

			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{

				items = db.Table<Item> ().ToList<Item> ();
			}

			List<string> icodes = new List<string> ();
			foreach (Item item in items) {
				icodes.Add (item.ICode+" | "+item.IDesc);
			}

			dataAdapter = new ArrayAdapter<String>(this,Resource.Layout.spinner_item, icodes);

			// Drop down layout style - list view with radio button
			dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);


			// attaching data adapter to spinner
			spinner.Adapter =dataAdapter;
			if (EDITMODE == "EDIT") {
				FIRSTLOAD="1";
				LoadData (INVOICENO, ITEMUID);
			}
		}

		private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
		{
			e.Handled = false;
			if ((e.ActionId == ImeAction.Done)||(e.ActionId == ImeAction.Next))
			{
				CalAmt ();
				e.Handled = true;   
				//Button butSave = FindViewById<Button> (Resource.Id.Save);
				//butSave.RequestFocus ();
			}
		}
		private void CalAmt()
		{
			EditText ttlamt = FindViewById<EditText> (Resource.Id.txtamount);
			EditText ttltax = FindViewById<EditText> (Resource.Id.txttaxamt);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			EditText taxper = FindViewById<EditText> (Resource.Id.txtinvtaxper);
			CheckBox isincl = FindViewById<CheckBox> (Resource.Id.txtinvisincl);
			TextView txttax =  FindViewById<TextView> (Resource.Id.txttax);
			try{
			double taxval = Convert.ToDouble(taxper.Text);
			double stqQty = Convert.ToDouble(qty.Text);
			double uprice = Convert.ToDouble(price.Text);
			double amount = Math.Round((stqQty * uprice),2);
			double netamount = amount;
			bool taxinclusice = isincl.Checked;
			double taxamt = 0;
			if (taxinclusice) {
				double percent = (taxval/100) + 1;
				double amt2 =Math.Round( amount / percent,2,MidpointRounding.AwayFromZero);
				taxamt = amount - amt2;
				netamount = amount - taxamt;

			} else {
				taxamt = Math.Round(amount * (taxval / 100),2,MidpointRounding.AwayFromZero);
			}
			ttlamt.Text = netamount.ToString("n2");
			ttltax.Text = taxamt.ToString("n2");
			}catch{
			}
		}

		private void LoadData(string invno,string uid)
		{
			TextView txtInvNo =  FindViewById<TextView> (Resource.Id.txtInvnp);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.txtcode);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
			TextView desc =  FindViewById<TextView> (Resource.Id.txtdesc);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			EditText amount = FindViewById<EditText> (Resource.Id.txtamount);
			EditText taxper = FindViewById<EditText> (Resource.Id.txtinvtaxper);
			EditText taxamt = FindViewById<EditText> (Resource.Id.txttaxamt);
			CheckBox isincl = FindViewById<CheckBox> (Resource.Id.txtinvisincl);
			TextView tax =  FindViewById<TextView> (Resource.Id.txttax);

			int id = Convert.ToInt32 (uid);

			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var invlist =db.Table<InvoiceDtls> ().Where (x => x.invno == invno&& x.ID==id).ToList<InvoiceDtls> ();
				if (invlist.Count > 0) {
					InvoiceDtls invItem = invlist [0];
					int index = dataAdapter.GetPosition (invItem.icode + " | " + invItem.description);
					Item item =items.Where (x => x.ICode == invItem.icode).FirstOrDefault ();
					spinner.SetSelection (index);
					qty.Text = invItem.qty.ToString ();
					price.Text = invItem.price.ToString ();
					taxamt.Text = invItem.tax.ToString ();

					tax.Text = item.taxgrp;
					taxper.Text = item.tax.ToString ();
					isincl.Checked = item.isincludesive;
					amount.Text = invItem.amount.ToString ();
					price.Text = invItem.price.ToString ();
				}
			}

		}

		private void butSaveClick(object sender,EventArgs e)
		{
			TextView txtInvNo =  FindViewById<TextView> (Resource.Id.txtInvnp);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.txtcode);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
			TextView desc =  FindViewById<TextView> (Resource.Id.txtdesc);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			EditText taxper = FindViewById<EditText> (Resource.Id.txtinvtaxper);
			CheckBox isincl = FindViewById<CheckBox> (Resource.Id.txtinvisincl);
			TextView txttax =  FindViewById<TextView> (Resource.Id.txttax);
			EditText ttlamt = FindViewById<EditText> (Resource.Id.txtamount);
			EditText ttltax = FindViewById<EditText> (Resource.Id.txttaxamt);
			if (spinner.SelectedItem == null) {
				Toast.MakeText (this, "No Item Code selected...", ToastLength.Long).Show ();			
				spinner.RequestFocus ();
				return;			
			}

			if (string.IsNullOrEmpty(qty.Text)) {
				Toast.MakeText (this, "Qty is blank...", ToastLength.Long).Show ();			
				qty.RequestFocus ();
				return;
			}
			if (string.IsNullOrEmpty(price.Text)) {
				Toast.MakeText (this, "Unit Price is blank...", ToastLength.Long).Show ();			
				price.RequestFocus ();
				return;
			}
			double stqQty = Convert.ToDouble(qty.Text);
			double uprice = Convert.ToDouble(price.Text);
			double taxval = Convert.ToDouble(taxper.Text);
			double amount = Math.Round((stqQty * uprice),2);
			double netamount = amount;
			bool taxinclusice = isincl.Checked;
			double taxamt = 0;
			if (taxinclusice) {
				double percent = (taxval/100) + 1;
				double amt2 =Math.Round( amount / percent,2,MidpointRounding.AwayFromZero);
				taxamt = amount - amt2;
				netamount = amount - taxamt;
			
			} else {
				taxamt = Math.Round(amount * (taxval / 100),2,MidpointRounding.AwayFromZero);
			}

			InvoiceDtls inv = new InvoiceDtls ();
			string[] codedesc = spinner.SelectedItem.ToString ().Split (new char[]{ '|' });
			inv.invno = txtInvNo.Text;
			inv.amount = amount;
			inv.description = desc.Text;
			inv.icode = codedesc [0].Trim();// spinner.SelectedItem.ToString ();
			inv.price = uprice;
			inv.qty = stqQty;
			inv.tax = taxamt;
			inv.taxgrp = txttax.Text;
			inv.netamount = netamount;

			var itemlist = items.Where (x => x.ICode == inv.icode).ToList<Item> ();
			if (itemlist.Count == 0) {
				Toast.MakeText (this, "Invlaid Item Code...", ToastLength.Long).Show ();
				return;
			}

			int id = Convert.ToInt32 (ITEMUID);				
			//inv..title = spinner.SelectedItem.ToString ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var invlist =db.Table<InvoiceDtls> ().Where (x => x.invno == inv.invno&& x.ID==id).ToList<InvoiceDtls> ();
				if (invlist.Count > 0) {
					InvoiceDtls invItem = invlist [0];
					invItem.amount = amount;
					invItem.netamount = netamount;
					invItem.tax = taxamt;
					invItem.taxgrp = txttax.Text;
					invItem.description = desc.Text;
					invItem.icode = spinner.SelectedItem.ToString ();
					invItem.price = uprice;
					invItem.qty = stqQty;
					db.Update (invItem);
				}else db.Insert (inv);
			}

			spinner.SetSelection (-1);
			qty.Text = "";
			//price.Text = "";
			ttltax.Text = "";
			ttlamt.Text = "";
			Toast.MakeText (this, "Item successfully added...", ToastLength.Long).Show ();
		}
		public override void OnBackPressed() {
			// do nothing.
		}

		private void butCancelClick(object sender,EventArgs e)
		{
			string invno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
			   double ttlamt= db.Table<InvoiceDtls> ().Where (x => x.invno == invno).Sum (x => x.netamount);
			   var invlist =db.Table<Invoice> ().Where (x => x.invno == invno).ToList<Invoice> ();
				if (invlist.Count > 0) {
					invlist [0].amount = ttlamt;
					db.Update (invlist [0]);
				}
			}
			//base.OnBackPressed();
			var intent = new Intent(this, typeof(InvItemActivity));
			intent.PutExtra ("invoiceno",INVOICENO );
			intent.PutExtra ("custcode",CUSTCODE );
			StartActivity(intent);
		}
		private void spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;

			string []codedesc = spinner.GetItemAtPosition (e.Position).ToString().Split (new char[]{ '|' });
			string icode = codedesc[0].Trim();
			Item item =items.Where (x => x.ICode == icode).FirstOrDefault ();

			//string toast = string.Format ("The planet is {0}", spinner.GetItemAtPosition (e.Position));
			//Toast.MakeText (this, toast, ToastLength.Long).Show ();
			TextView desc =  FindViewById<TextView> (Resource.Id.txtdesc);
			TextView tax =  FindViewById<TextView> (Resource.Id.txttax);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			EditText taxper = FindViewById<EditText> (Resource.Id.txtinvtaxper);
			CheckBox isincl = FindViewById<CheckBox> (Resource.Id.txtinvisincl);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
			desc.Text = item.IDesc;
			if (FIRSTLOAD=="")
				price.Text = item.Price.ToString ();
			else FIRSTLOAD="";
			tax.Text = item.taxgrp;
			taxper.Text = item.tax.ToString ();
			isincl.Checked = item.isincludesive;
			qty.RequestFocus ();

		}
	}
}

