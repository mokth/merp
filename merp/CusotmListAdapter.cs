using System;
using Android.Widget;
using Android.App;
using System.Collections.Generic;
using Android.Views;

namespace wincom.mobile.erp
{
//	public class CusotmListAdapter : BaseAdapter<Invoice>
//	{
//		Activity context;
//		List<Invoice> list;
//
//		public CusotmListAdapter (Activity _context, List<Invoice> _list)
//			:base()
//		{
//			this.context = _context;
//			this.list = _list;
//		}
//
//		public override int Count {
//			get { return list.Count; }
//		}
//
//		public override long GetItemId (int position)
//		{
//			return position;
//		}
//
//		public override Invoice this[int index] {
//			get { return list [index]; }
//		}
//
//		public override View GetView (int position, View convertView, ViewGroup parent)
//		{
//			View view = convertView; 
//
//			// re-use an existing view, if one is available
//			// otherwise create a new one
//			if (view == null)
//				view = context.LayoutInflater.Inflate (Resource.Layout.ListItemRow, parent, false);
//
//			Invoice item = this [position];
//			view.FindViewById<TextView> (Resource.Id.invdate).Text = item.invdate.ToString ("dd-MM-yy");
//			view.FindViewById<TextView> (Resource.Id.invno).Text = item.invno;
//			view.FindViewById<TextView>(Resource.Id.Description).Text = item.description;
//			view.FindViewById<TextView> (Resource.Id.Amount).Text = item.amount.ToString("n2");
//
//
//			return view;
//		}
//	}
//
//
//	public class CusotmItemListAdapter : BaseAdapter<InvoiceDtls>
//	{
//		Activity context;
//		List<InvoiceDtls> list;
//
//		public CusotmItemListAdapter (Activity _context, List<InvoiceDtls> _list)
//			:base()
//		{
//			this.context = _context;
//			this.list = _list;
//		}
//
//		public override int Count {
//			get { return list.Count; }
//		}
//
//		public override long GetItemId (int position)
//		{
//			return position;
//		}
//
//		public override InvoiceDtls this[int index] {
//			get { return list [index]; }
//		}
//
//		public override View GetView (int position, View convertView, ViewGroup parent)
//		{
//			View view = convertView; 
//
//			// re-use an existing view, if one is available
//			// otherwise create a new one
//			if (view == null)
//				view = context.LayoutInflater.Inflate (Resource.Layout.InvDtlItemView, parent, false);
//
//			InvoiceDtls item = this [position];
//			if (item != null) {
//				view.FindViewById<TextView> (Resource.Id.invitemcode).Text = item.icode;
//				view.FindViewById<TextView> (Resource.Id.invitemdesc).Text = item.description;
//				view.FindViewById<TextView> (Resource.Id.invitemqty).Text = item.qty.ToString ();
//				view.FindViewById<TextView> (Resource.Id.invitemprice).Text = item.price.ToString ("n2");
//				view.FindViewById<TextView> (Resource.Id.invitemamt).Text = item.amount.ToString ("n2");
//			}
//			return view;
//		}
//	}
//
//	public class CusotmMasterItemListAdapter : BaseAdapter<Item>
//	{
//		Activity context;
//		List<Item> list;
//
//		public CusotmMasterItemListAdapter (Activity _context, List<Item> _list)
//			:base()
//		{
//			this.context = _context;
//			this.list = _list;
//		}
//
//		public override int Count {
//			get { return list.Count; }
//		}
//
//		public override long GetItemId (int position)
//		{
//			return position;
//		}
//
//		public override Item this[int index] {
//			get { return list [index]; }
//		}
//
//		public override View GetView (int position, View convertView, ViewGroup parent)
//		{
//			View view = convertView; 
//
//			// re-use an existing view, if one is available
//			// otherwise create a new one
//			if (view == null)
//				view = context.LayoutInflater.Inflate (Resource.Layout.ItemCodeDtlList, parent, false);
//
//			Item item = this [position];
//			if (item != null) {
//				view.FindViewById<TextView> (Resource.Id.icodecode).Text = item.ICode;
//				view.FindViewById<TextView> (Resource.Id.icodedesc).Text = item.IDesc;
//				view.FindViewById<TextView> (Resource.Id.icodeprice).Text = item.Price.ToString ("n3");
//				view.FindViewById<TextView> (Resource.Id.icodetax).Text = item.taxgrp;
//				view.FindViewById<TextView> (Resource.Id.icodetaxper).Text = item.tax.ToString ("n2");
//				view.FindViewById<TextView> (Resource.Id.icodeinclusive).Text = (item.isincludesive)?"INC":"EXC";
//			}
//			return view;
//		}
//	}
//
//	public class CusotmCustomerListAdapter : BaseAdapter<Trader>
//	{
//		Activity context;
//		List<Trader> list;
//
//		public CusotmCustomerListAdapter (Activity _context, List<Trader> _list)
//			:base()
//		{
//			this.context = _context;
//			this.list = _list;
//		}
//
//		public override int Count {
//			get { return list.Count; }
//		}
//
//		public override long GetItemId (int position)
//		{
//			return position;
//		}
//
//		public override Trader this[int index] {
//			get { return list [index]; }
//		}
//
//		public override View GetView (int position, View convertView, ViewGroup parent)
//		{
//			View view = convertView; 
//
//			// re-use an existing view, if one is available
//			// otherwise create a new one
//			if (view == null)
//				view = context.LayoutInflater.Inflate (Resource.Layout.ListCustDtlView, parent, false);
//
//			Trader item = this [position];
//			if (item != null) {
//				view.FindViewById<TextView> (Resource.Id.custcode).Text = item.CustCode;
//				view.FindViewById<TextView> (Resource.Id.custname).Text = item.CustName;
//			
//			}
//			return view;
//		}
//	}
}

