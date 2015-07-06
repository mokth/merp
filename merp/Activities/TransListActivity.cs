
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
	[Activity (Label = "TransListActivity")]			
	public class TransListActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.Translist);
			Button butInvlist = FindViewById<Button> (Resource.Id.butInvlist);
			butInvlist.Click+= ButInvlist_Click;

			Button butCNNoteList = FindViewById<Button> (Resource.Id.butCNlist);
			butCNNoteList.Click+= ButCNNoteList_Click;

			Button butsumm = FindViewById<Button> (Resource.Id.butInvsumm);
			butsumm.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(PrintSumm));
			};

			Button butback = FindViewById<Button> (Resource.Id.butMain);
			butback.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(MainActivity));
			};
		}

		void ButInvlist_Click (object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(InvoiceAllActivity));

			StartActivity(intent);
		}

		void ButCNNoteList_Click (object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(CNAllActivity));

			StartActivity(intent);
		}
	}
}

