
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
	[Activity (Label = "NEW INVOICE")]			
	public class CreateInvoice : Activity,IEventListener
	{
		string pathToDatabase;
		List<Trader> items = null;
		ArrayAdapter<String> dataAdapter;
		ArrayAdapter dataAdapter2;
		DateTime _date ;
		AdPara apara = null;
		Spinner spinner;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.CreateInvoice);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);

			// Create your application here
			_date = DateTime.Today;
			spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			Spinner spinnerType = FindViewById<Spinner> (Resource.Id.newinv_type);
			Button butSave = FindViewById<Button> (Resource.Id.newinv_bsave);
			Button butNew = FindViewById<Button> (Resource.Id.newinv_cancel);
			Button butFind = FindViewById<Button> (Resource.Id.newinv_bfind);
			spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
			butSave.Click += butSaveClick;
			butNew.Click += butCancelClick;
			TextView invno =  FindViewById<TextView> (Resource.Id.newinv_no);
			invno.Text = "AUTO";
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
 			trxdate.Text = _date.ToString ("dd-MM-yyyy");
			trxdate.Click += delegate(object sender, EventArgs e) {
				ShowDialog (0);
			};
			butFind.Click+= (object sender, EventArgs e) => {
				ShowCustLookUp();
			};

			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				items = db.Table<Trader> ().ToList<Trader> ();
			}

			List<string> icodes = new List<string> ();
			foreach (Trader item in items) {
				icodes.Add (item.CustCode+" | "+item.CustName);
			}

			dataAdapter = new ArrayAdapter<String>(this,Resource.Layout.spinner_item, icodes);
			dataAdapter2 =ArrayAdapter.CreateFromResource (
							this, Resource.Array.trxtype, Resource.Layout.spinner_item);

			// Drop down layout style - list view with radio button
			dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			dataAdapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

			// attaching data adapter to spinner
			spinner.Adapter =dataAdapter;
			spinnerType.Adapter =dataAdapter2;
		}

		public override void OnBackPressed() {
			// do nothing.
		}

		private void butSaveClick(object sender,EventArgs e)
		{
			int count1 = 0;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				count1 = db.Table<Item>().Count ();
			}
			if (count1 > 0)
				CreateNewInvoice ();
			else {
				Toast.MakeText (this,"Please download Master Item before proceed... ", ToastLength.Long).Show ();	
			}
		}

		private void butCancelClick(object sender,EventArgs e)
		{
			base.OnBackPressed();
		}

		private void spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;

			string txt = spinner.GetItemAtPosition (e.Position).ToString();
			string[] codes = txt.Split (new char[]{ '|' });
			if (codes.Length == 0)
				return;
			
			Trader item =items.Where (x => x.CustCode ==codes[0].Trim()).FirstOrDefault ();
			if (item != null) {
				TextView name = FindViewById<TextView> (Resource.Id.newinv_custname);
				name.Text = item.CustName;
			}

		}
		[Obsolete]
		protected override Dialog OnCreateDialog (int id)
		{
			return new DatePickerDialog (this, HandleDateSet, _date.Year,
				_date.Month - 1, _date.Day);
		}

		void HandleDateSet (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			_date = e.Date;
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			trxdate.Text = _date.ToString ("dd-MM-yyyy");
		}


		void ShowItemEntry (Invoice inv, string[] codes)
		{
			var intent = new Intent (this, typeof(EntryActivity));
			intent.PutExtra ("invoiceno", inv.invno);
			intent.PutExtra ("customer", codes [1].Trim ());
			intent.PutExtra ("itemuid", "-1");
			intent.PutExtra ("editmode", "NEW");
			StartActivity (intent);
		}

		private void CreateNewInvoice()
		{
			Invoice inv = new Invoice ();
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			DateTime invdate = Utility.ConvertToDate (trxdate.Text);
			DateTime tmr = invdate.AddDays (1);
			AdNumDate adNum= DataHelper.GetNumDate (pathToDatabase, invdate);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			Spinner spinner2 = FindViewById<Spinner> (Resource.Id.newinv_type);
			TextView txtinvno =FindViewById<TextView> (Resource.Id.newinv_no);
			TextView custname = FindViewById<TextView> (Resource.Id.newinv_custname);
			string prefix = apara.Prefix.Trim ().ToUpper ();
			if (spinner.SelectedItem == null) {
				Toast.MakeText (this, "No Customer code selected...", ToastLength.Long).Show ();			
				spinner.RequestFocus ();
				return;			
			}

			string[] codes = spinner.SelectedItem.ToString().Split (new char[]{ '|' });
			if (codes.Length == 0)
				return;
			
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
//				var invrec =db.Table<Invoice> ().Where (x => x.invdate>= invdate && x.invdate < tmr).OrderByDescending(x=>x.invno).ToList();
//				string invno = "";
//				if (invrec.Count == 0) {
//					invno = prefix + invdate.ToString("yyMMdd") + "1".PadLeft (4, '0');
//				} else {
//					string tmp = invrec [0].invno;
//					string runno= tmp.Substring (tmp.Length - 4, 4);
//					int no = Convert.ToInt32 (runno)+1;
//					invno =prefix + invdate.ToString("yyMMdd") + no.ToString().PadLeft (4, '0');
//				}
				string invno = "";
				int runno = adNum.RunNo + 1;
				invno =prefix + invdate.ToString("yyMM") + runno.ToString().PadLeft (4, '0');
				txtinvno.Text= invno;
				inv.invdate = invdate;
				inv.trxtype = spinner2.SelectedItem.ToString ();
				inv.created = DateTime.Now;
				inv.invno = invno;
				inv.description = custname.Text;
				inv.amount = 0;
				inv.custcode = codes [0].Trim ();
				inv.isUploaded = false;
				db.Insert (inv);
				adNum.RunNo += 1;
				if (adNum.ID >= 0)
					db.Update (adNum);
				else
					db.Insert (adNum);
			}

			ShowItemEntry (inv, codes);
		}

		void ShowCustLookUp()
		{
			var dialog = TraderDialog.NewInstance();
			dialog.Show(FragmentManager, "dialog");
		}

		void SetSelectedItem(string text)
		{
			int position=dataAdapter.GetPosition (text);
			spinner.SetSelection (position);
		}

		public event nsEventHandler eventHandler;

		public void FireEvent(object sender,EventParam eventArgs)
		{
			if (eventHandler != null)
				eventHandler (sender, eventArgs);
		}

		public void PerformEvent(object sender, EventParam e)
		{
			switch (e.EventID) {
			case 101:
				RunOnUiThread (() => SetSelectedItem(e.Param["SELECTED"].ToString()));
				break;
			}
		}
	}
}

