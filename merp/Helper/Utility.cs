using System;

namespace wincom.mobile.erp
{
	public class Utility
	{
		public static DateTime ConvertToDate(string sdate)
		{
			DateTime date = DateTime.Today;  
			string[] para = sdate.Split(new char[]{'-'});
			if (para.Length > 2) {
				int yy = Convert.ToInt32 (para [2]);
				int mm = Convert.ToInt32 (para [1]);
				int dd = Convert.ToInt32 (para [0]);

				date = new DateTime (yy, mm, dd);
			}

			return date;
		}
	}
}

