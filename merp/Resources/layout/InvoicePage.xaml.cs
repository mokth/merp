using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using System.IO;


namespace wincom.mobile.erp
{
	public partial class InvoicePage : ContentPage
	{
		string pathToDatabase;
		Invoice[] invlist = null;
		public InvoicePage ()
		{
			InitializeComponent ();
			var documents = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			pathToDatabase = Path.Combine (documents, "db_adonet.db");
			populate ();
			BindingContext = invlist;
		}

		void populate()
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<Invoice> ().Where (x => x.isUploaded == false).ToList<Invoice> ();
				list2.CopyTo (invlist);
			}

		}
	}
}

