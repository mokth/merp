using System;
using Android.Widget;
using Android.App;
using System.Collections.Generic;
using Android.Views;
using Android.Graphics;

namespace wincom.mobile.erp
{
	public delegate void SetViewDlg(View view,object clsobj);

	public class GenericListAdapter<T> : BaseAdapter<T>
	{
		

		Activity context;
		List<T> list;
		int _viewID;
		SetViewDlg _dlg; 

		public GenericListAdapter (Activity _context, List<T> _list,int viewID,SetViewDlg viewdlg)
			:base()
		{
			this.context = _context;
			this.list = _list;
			_viewID = viewID;
			_dlg = viewdlg;
		}

		public override int Count {
			get { return list.Count; }
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override T this[int index] {
			get { return list [index]; }
		}

		static void SetChildFontColor (View view,Color col)
		{
			ViewGroup lview = (ViewGroup)view;
			for (int i = 0; i < lview.ChildCount; i++) {
				var tview = lview.GetChildAt (i);
				Android.Util.Log.Debug ("Test",tview.GetType ().ToString ());
				if (tview.GetType () == typeof(TextView)) {
					TextView v = (TextView)tview;
					v.SetTextColor (col);
				} else
					SetChildFontColor (tview, col);
			}
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			View view = convertView; 

			// re-use an existing view, if one is available
			// otherwise create a new one
			if (view == null)
				view = context.LayoutInflater.Inflate (_viewID, parent, false);

			T item = this [position];
			if (item != null) {
				_dlg.Invoke (view, item);
			}


			if (position % 2 == 1) {
				view.SetBackgroundResource (Resource.Drawable.listview_selector_even);
				SetChildFontColor (view,Color.White);
			} else {
				view.SetBackgroundResource (Resource.Drawable.listview_selector_odd);
				SetChildFontColor (view,Color.Black);
			}
			return view;
		}
	}
}

