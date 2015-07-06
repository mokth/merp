using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.IO;
using Android.Graphics.Drawables;
using System.Collections;

namespace wincom.mobile.erp
{
	public class InvoiceDialog : DialogFragment
	{
		ListView listView ;
		List<Invoice> listData = new List<Invoice> ();
		string pathToDatabase;
		GenericListAdapter<Invoice> adapter; 
		EditText  txtSearch;
		Button butSearch;
		SetViewDlg viewdlg;
		string _selectedItem;

		public static InvoiceDialog NewInstance()
		{
			var dialogFragment = new InvoiceDialog();
			return dialogFragment;
		}

		public string SelectedItem
		{
			get { return _selectedItem;}
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}
		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{ 
			base.OnCreate(savedInstanceState);
			// Begin building a new dialog.
			var builder = new AlertDialog.Builder(Activity);
			//Get the layout inflater
			var inflater = Activity.LayoutInflater;
			populate (listData);
			viewdlg = SetViewDelegate;
			var view = inflater.Inflate(Resource.Layout.ItemCodeList, null);
			listView = view.FindViewById<ListView> (Resource.Id.ICodeList);
			if (listView != null) {
				adapter = new GenericListAdapter<Invoice> (this.Activity, listData,Resource.Layout.ListItemRow, viewdlg);
				listView.Adapter = adapter;
				listView.ItemClick += ListView_Click;
				txtSearch= view.FindViewById<EditText > (Resource.Id.txtSearch);
				butSearch= view.FindViewById<Button> (Resource.Id.butICodeBack); 
				butSearch.Text = "SEARCH";
				butSearch.SetCompoundDrawables (null, null, null, null);
				butSearch.Click+= ButSearch_Click;
				//txtSearch.TextChanged += TxtSearch_TextChanged;
				builder.SetView (view);
				builder.SetPositiveButton ("CANCEL", HandlePositiveButtonClick);
			}
			var dialog = builder.Create();
			//Now return the constructed dialog to the calling activity
			return dialog;
		}

		void ButSearch_Click (object sender, EventArgs e)
		{
			FindItemByText ();
		}

		private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
		{
			var dialog = (AlertDialog) sender;
			dialog.Dismiss();
		}
		private void  HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
		{
			var dialog = (AlertDialog) sender;
			dialog.Dismiss();
		}
		void FindItemByText ()
		{
			List<Invoice> found = PerformSearch (txtSearch.Text);
			adapter = new GenericListAdapter<Invoice> (this.Activity, found,Resource.Layout.ListItemRow, viewdlg);
			listView.Adapter = adapter;
		}

		void TxtSearch_TextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			FindItemByText ();
		}

		void ListView_Click (object sender, AdapterView.ItemClickEventArgs e)
		{
			Invoice itm = adapter[e.Position];
			if (itm != null) {
				_selectedItem = itm.invno ;
				Hashtable param = new Hashtable ();
				param.Add ("SELECTED", _selectedItem);
				EventParam p = new EventParam (EventID.INVNO_SELECTED, param);
				EventManagerFacade.Instance.GetEventManager ().PerformEvent (this.Activity, p);
			}
			this.Dialog.Dismiss ();
		}

		private void SetViewDelegate(View view,object clsobj)
		{
			Invoice item = (Invoice)clsobj;
			view.FindViewById<TextView> (Resource.Id.invdate).Text = item.invdate.ToString ("dd-MM-yy");
			view.FindViewById<TextView> (Resource.Id.invno).Text = item.invno;
			view.FindViewById<TextView> (Resource.Id.trxtype).Text = item.trxtype;
			view.FindViewById<TextView>(Resource.Id.invcust).Text = item.description;
			//view.FindViewById<TextView> (Resource.Id.Amount).Text = item.amount.ToString("n2");
			view.FindViewById<TextView> (Resource.Id.TaxAmount).Text = item.taxamt.ToString("n2");
			double ttl = item.amount + item.taxamt;
			view.FindViewById<TextView> (Resource.Id.TtlAmount).Text =ttl.ToString("n2");
			ImageView img = view.FindViewById<ImageView> (Resource.Id.printed);
			if (!item.isPrinted)
				img.Visibility = ViewStates.Invisible;
		}

		List<Invoice> PerformSearch (string constraint)
		{
			List<Invoice>  results = new List<Invoice>();
			if (constraint != null) {
				var searchFor = constraint.ToString ().ToUpper();

				foreach(Invoice itm in listData)
				{
					if (itm.invno.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
				}
			}
			return results;
		}

		void populate(List<Invoice> list)
		{

			var documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			pathToDatabase = Path.Combine(documents, "db_adonet.db");

			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{

				var list2 = db.Table<Invoice>().ToList<Invoice>();
				foreach(var item in list2)
				{
					list.Add(item);
				}
			}
		}

	}
}

