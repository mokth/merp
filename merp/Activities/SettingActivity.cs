
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
	[Activity (Label = "SETTINGS")]			
	public class SettingActivity : Activity
	{
		string pathToDatabase;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.AdPara);
			Button butSave = FindViewById<Button> (Resource.Id.ad_bSave);
			Button butCancel = FindViewById<Button> (Resource.Id.ad_Cancel);
			butSave.Click += butSaveClick;
			butCancel.Click += butCancelClick;
			LoadData ();
			// Create your application here
		}

		private void LoadData()
		{
			TextView txtprinter =FindViewById<TextView> (Resource.Id.txtad_printer);
			TextView txtprefix =FindViewById<TextView> (Resource.Id.txtad_prefix);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			AdPara apra = new AdPara ();
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list  = db.Table<AdPara> ().ToList<AdPara> ();
				if (list.Count > 0) {
					apra = list [0];
					txtprefix.Text = apra.Prefix;
					txtprinter.Text = apra.PrinterName;
				} else {
					txtprefix.Text = "CS";
					txtprinter.Text = "PT-II";
				}
			}
		}
		private void butSaveClick(object sender,EventArgs e)
		{
			TextView txtprinter =FindViewById<TextView> (Resource.Id.txtad_printer);
			TextView txtprefix =FindViewById<TextView> (Resource.Id.txtad_prefix);

			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			AdPara apra = new AdPara ();
			apra.Prefix = txtprefix.Text.ToUpper();
			apra.PrinterName = txtprinter.Text.ToUpper();
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list  = db.Table<AdPara> ().ToList<AdPara> ();
				if (list.Count == 0) {
					db.Insert (apra);		
				} else {
					apra = list [0];
					apra.Prefix = txtprefix.Text.ToUpper();
					apra.PrinterName = txtprinter.Text.ToUpper();
					db.Update (apra);
				}
			}
			base.OnBackPressed();
		}

		private void butCancelClick(object sender,EventArgs e)
		{
			base.OnBackPressed();
		}
	}
}

