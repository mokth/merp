using System;
using Android.App;
using Android.Runtime;
using System.IO;

namespace wincom.mobile.erp
{
	[Application]
	public class GlobalvarsApp:Application
	{
		public string COMPANY_CODE;
		public string BRANCH_CODE;
		public string USERID_CODE;
		public string DATABASE_PATH;
		public bool ISLOGON;

		public GlobalvarsApp(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}

		public override void OnCreate()
		{
			var documents = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			DATABASE_PATH = Path.Combine (documents, "db_adonet.db");
			base.OnCreate();
		}

	}
}

