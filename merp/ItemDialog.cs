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
	public class ItemDialog : DialogFragment
	{
		ListView listView ;
		List<Item> listData = new List<Item> ();
		string pathToDatabase;
		GenericListAdapter<Item> adapter; 
		EditText  txtSearch;
		Button butSearch;
		SetViewDlg viewdlg;
		string _selectedItem;

		public static ItemDialog NewInstance()
		{
			var dialogFragment = new ItemDialog();
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
				adapter = new GenericListAdapter<Item> (this.Activity, listData,Resource.Layout.ItemCodeDtlList, viewdlg);
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
			List<Item> found = PerformSearch (txtSearch.Text);
			adapter = new GenericListAdapter<Item> (this.Activity, found,Resource.Layout.ItemCodeDtlList, viewdlg);
			listView.Adapter = adapter;
		}

		void TxtSearch_TextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			FindItemByText ();
		}

		void ListView_Click (object sender, AdapterView.ItemClickEventArgs e)
		{
			Item itm = adapter[e.Position];
			if (itm != null) {
				_selectedItem = itm.ICode + " | " + itm.IDesc;
				Hashtable param = new Hashtable ();
				param.Add ("SELECTED", _selectedItem);
				EventParam p = new EventParam (EventID.ICODE_SELECTED, param);
				EventManagerFacade.Instance.GetEventManager ().PerformEvent (this.Activity, p);
			}
			this.Dialog.Dismiss ();
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

		List<Item> PerformSearch (string constraint)
		{
			List<Item>  results = new List<Item>();
			if (constraint != null) {
				var searchFor = constraint.ToString ().ToUpper();
				Console.WriteLine ("searchFor:" + searchFor);

				foreach(Item itm in listData)
				{
					if (itm.ICode.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
					if (itm.IDesc.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
				}
			}
			return results;
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

