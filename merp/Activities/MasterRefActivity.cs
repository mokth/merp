
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
	[Activity (Label = "MASTER")]			
	public class MasterRefActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.MasterRef);

			Button butCustProf = FindViewById<Button> (Resource.Id.butCustProf);
			butCustProf.Click+=  butCustomerClick;

			Button butMItem = FindViewById<Button> (Resource.Id.butMaster);
			butMItem.Click += butMasterClick;

			Button butback = FindViewById<Button> (Resource.Id.butMain);
			butback.Click+= (object sender, EventArgs e) => {
				base.OnBackPressed();
			};
		}

		private void butMasterClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(MasterItemActivity));

			StartActivity(intent);
		}
		private void butCustomerClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(CustomerActivity));

			StartActivity(intent);
		}
	}
}

