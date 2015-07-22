using System;
using System.Data;
using System.Xml.Linq;
using System.Linq;
using System.Linq.Expressions;
using Android.Bluetooth;
using Java.Util;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;

namespace wincom.mobile.erp
{
	public class PrintInvHelper
	{
		string pathToDatabase;
		string USERID;
		AdPara apara;
		CompanyInfo compinfo;
		string msg;
		string sdcard="";
		//StringBuilder errmsg = new StringBuilder ();

		public PrintInvHelper (string adpathToDatabase,string userid)
		{
			pathToDatabase = adpathToDatabase;
			USERID = userid;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			compinfo =DataHelper.GetCompany(pathToDatabase);
			sdcard = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.Path, "erpdata\\log.txt");
		}

		private void PrintLine(string text,Stream mmOutputStream)
		{
			byte[] cc = Encoding.ASCII.GetBytes (text);
			mmOutputStream.Write (cc, 0, cc.Length);

		}

		#region Print Receipt
		public string OpenBTAndPrint(BluetoothSocket mmSocket,BluetoothDevice mmDevice,Invoice inv,InvoiceDtls[] list,int noofcopy )
		{
			string msg = "";
			string text = "";
			PrintInvoice (inv, list, ref text);
			BluetoothPrint (mmSocket, mmDevice, text, noofcopy);
			return msg;
		}

		public string OpenBTAndPrintSO(BluetoothSocket mmSocket,BluetoothDevice mmDevice,SaleOrder so,SaleOrderDtls[] list,int noofcopy )
		{
			string msg = "";
			string text = "";
			PrintSO (so, list, ref text);
			BluetoothPrint (mmSocket, mmDevice, text, noofcopy);
			return msg;
		}

		public string OpenBTAndPrintDO(BluetoothSocket mmSocket,BluetoothDevice mmDevice,DelOrder doOrder,DelOrderDtls[] list,int noofcopy )
		{
			string msg = "";
			string text = "";
			PrintDO ( doOrder, list, ref text);
			BluetoothPrint (mmSocket, mmDevice, text, noofcopy);
			return msg;
		}

		void PrintInvoice (Invoice inv, InvoiceDtls[] list, ref string test)
		{
			PrintCompHeader (ref test);
			PrintCustomer (ref test, inv.custcode);
			PrintHeader (ref test, inv);
			string dline = "";
			double ttlAmt = 0;
			double ttltax = 0;
			int count = 0;
			foreach (InvoiceDtls itm in list) {
				count += 1;
				dline = dline + PrintDetail (itm, count);
				ttlAmt = ttlAmt + itm.netamount;
				ttltax = ttltax + itm.tax;
			}
			test += dline;
			PrintTotal (ref test, ttlAmt, ttltax);
			PrintTaxSumm (ref test, list);
			PrintFooter (ref test);
			test += "\nTHANK YOU\n\n\n\n";
		}

		void PrintSO (SaleOrder so, SaleOrderDtls[] list, ref string test)
		{
			PrintCompHeader (ref test);
			PrintCustomer (ref test, so.custcode);
			PrintSOHeader (ref test, so);
			string dline = "";
			double ttlAmt = 0;
			double ttltax = 0;
			int count = 0;
			foreach (SaleOrderDtls itm in list) {
				count += 1;
				dline = dline + PrintSODetail (itm, count);
				ttlAmt = ttlAmt + itm.netamount;
				ttltax = ttltax + itm.tax;
			}
			test += dline;
			PrintTotal (ref test, ttlAmt, ttltax);
			PrintSOTaxSumm (ref test, list);
			PrintFooter (ref test);
			test += "\nTHANK YOU\n\n\n\n";
		}

		void PrintDO (DelOrder doorder, DelOrderDtls[] list, ref string test)
		{
			PrintCompHeader (ref test);
			PrintCustomer (ref test, doorder.custcode);
			PrintDOHeader (ref test, doorder);
			string dline = "";
			double ttlAmt = 0;
			double ttltax = 0;
			int count = 0;
			foreach (DelOrderDtls itm in list) {
				count += 1;
				dline = dline + PrintDODetail (itm, count);
				ttlAmt = ttlAmt + itm.qty;
			}
			test += dline;
			PrintDOTotal (ref test, ttlAmt);
			PrintFooter (ref test);
			test += "\nTHANK YOU\n\n\n\n";
		}


		private bool TrytoConnect(BluetoothSocket mmSocket)
		{
			bool TrytoConnect = true;
			int count = 0;
			while (TrytoConnect) {
				try {
					Thread.Sleep(400);
					mmSocket.Connect ();
					TrytoConnect = false;
				} catch {
					count += 1;
					if (count==5)
						TrytoConnect = false;
				}
			}
			return !TrytoConnect;
		}

	
		public void BluetoothPrint(BluetoothSocket mmSocket,BluetoothDevice mmDevice,string text,int noofcopy)
		{
			msg = "";
			Stream mmOutputStream;
			//TextView txtv = FindViewById<TextView> (Resource.Id.textView2);
			try {
				UUID uuid = UUID.FromString ("00001101-0000-1000-8000-00805F9B34FB");
				mmSocket = mmDevice.CreateInsecureRfcommSocketToServiceRecord (uuid);
				if (mmSocket == null) {
					msg =  "Error creating sockect";
					return;
				}
				if (mmDevice.BondState == Bond.Bonded) {
					TrytoConnect(mmSocket);
					Thread.Sleep (300);
					mmOutputStream = mmSocket.OutputStream;
					byte[] charfont;
					charfont = new Byte[] { 27, 64 }; //Char font 9x17
					mmOutputStream.Write(charfont, 0, charfont.Length);
					if (apara.PaperSize=="58mm")
					{
						charfont = new Byte[] { 27, 33, 1 }; //Char font 9x17
						mmOutputStream.Write(charfont, 0, charfont.Length);
					}

					if (apara.PaperSize=="80mm")
					{
						charfont = new Byte[] { 27, 33, 0 }; //Char font 12x24
						mmOutputStream.Write(charfont, 0, charfont.Length);
					}
					charfont = new Byte[] { 28, 38 };
					mmOutputStream.Write(charfont, 0, charfont.Length);
//					charfont = new Byte[] { 28, 67,0,48 };
//					mmOutputStream.Write(charfont, 0, charfont.Length);
					byte[] cc = Encoding.GetEncoding("GB18030").GetBytes(text);
					for (int i=0; i<noofcopy;i++)
					{
						int rem;
						int result =Math.DivRem(cc.Length, 2048, out rem);
						int pos =0;
						for(int line= 0;line<result;line++)
						{
							IsStreamCanWrite (mmOutputStream);
							mmOutputStream.Write (cc, pos, 2048);
							pos += 2048;
						}
						if (rem >0)
							mmOutputStream.Write (cc, pos, rem);
						Thread.Sleep (3000);
					}
					Thread.Sleep (300);
					charfont = new Byte[] { 28, 46 };
					mmOutputStream.Write(charfont, 0, charfont.Length);
					mmOutputStream.Close ();
					mmSocket.Close ();
					msg ="Printing....";
				} else {
					msg= "Device not connected";	
				}
			} catch (Exception ex) {
				msg = ex.Message;
			}
		}

		public string OpenBTAndPrintCN(BluetoothSocket mmSocket,BluetoothDevice mmDevice,CNNote inv,CNNoteDtls[] list,int noofcopy )
		{string msg = "";
			
			BluetoothMiniCN (mmSocket, mmDevice, inv, list, noofcopy);

			return msg;
		}

		private bool PrintCNInvoice(CNNote cn,ref string  test,ref double ttlAmt,ref double ttltax)
		{
			bool IsfoundInvoice =false;
			InvoiceDtls[] list =null;
			Invoice inv=null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var lsinv= db.Table<Invoice> ().Where (x => x.invno==cn.invno).ToList<Invoice>();
				if (lsinv.Count > 0) {
					IsfoundInvoice =true;
					inv = lsinv [0];
					var ls = db.Table<InvoiceDtls> ().Where (x => x.invno == cn.invno).ToList<InvoiceDtls> ();
					list = new InvoiceDtls[ls.Count];
					ls.CopyTo (list);
				}
			}


			if (inv != null) {
				PrintInvoice (inv, list, ref test);
				foreach(InvoiceDtls itm in list)
				{
					ttlAmt = ttlAmt+ itm.netamount;
					ttltax = ttltax+itm.tax;
				}
			}

			return IsfoundInvoice;
		}

		static bool IsStreamCanWrite (Stream mmOutputStream)
		{
			int nwait = 0;
			bool isReady = false;
			while (true) {
				if (mmOutputStream.CanWrite) {
					isReady = true;
					break;
				}
				else {
					nwait += 1;
					if (nwait > 10)
						break;
				}
				Thread.Sleep (300);
			}
			return isReady;
		}

		public void BluetoothMiniCN(BluetoothSocket mmSocket,BluetoothDevice mmDevice,CNNote inv,CNNoteDtls[] list,int noofcopy )
		{
			msg = "";
			Stream mmOutputStream;
			try {
				UUID uuid = UUID.FromString ("00001101-0000-1000-8000-00805F9B34FB");

				mmSocket = mmDevice.CreateInsecureRfcommSocketToServiceRecord (uuid);
				if (mmSocket == null) {
					msg =  "Error creating sockect";
					return;
				}
				if (mmDevice.BondState == Bond.Bonded) {
					TrytoConnect(mmSocket);
					Thread.Sleep (300);
					mmOutputStream = mmSocket.OutputStream;
					byte[] charfont;
					charfont = new Byte[] { 27, 64 }; //Char font 9x17
					mmOutputStream.Write(charfont, 0, charfont.Length);
					if (apara.PaperSize=="58mm")
					{
						charfont = new Byte[] { 27, 33, 1 }; //Char font 9x17
						//charfont = new Byte[] { 27, 77, 1 }; //Char font 9x17
						mmOutputStream.Write(charfont, 0, charfont.Length);
					}

					if (apara.PaperSize=="80mm")
					{
						charfont = new Byte[] { 27, 33, 0 }; //Char font 12x24
						//charfont = new Byte[] { 27, 77, 1 }; //Char font 9x17
						mmOutputStream.Write(charfont, 0, charfont.Length);
					}
					charfont = new Byte[] { 28, 38 };
					mmOutputStream.Write(charfont, 0, charfont.Length);
//					charfont = new Byte[] { 28, 67,0,48 };
//					mmOutputStream.Write(charfont, 0, charfont.Length);
					string test = "";
					double invTtlAmt =0;
					double invTtlTax =0;
					bool IsfoundInvoice =PrintCNInvoice(inv,ref test,ref invTtlAmt,ref invTtlTax);
					PrintCompHeader (ref test);
					PrintCustomer (ref test,inv.custcode);
					PrintCNHeader (ref test,inv);
					string dline="";
					double ttlAmt =0;
					double ttltax =0;
					int count =0;
					foreach(CNNoteDtls itm in list)
					{
						count+=1;
						dline =dline+PrintCNDetail (itm,count);
						ttlAmt = ttlAmt+ itm.netamount;
						ttltax = ttltax+itm.tax;
					}
					test += dline;
					PrintTotal (ref test,ttlAmt,ttltax);

					PrintCNTaxSumm(ref test,list );
					PrintFooter (ref test);
					if (IsfoundInvoice)
					{
						test += "\nTHANK YOU\n\n";
						PrintTotal (ref test,ttlAmt, ttltax, invTtlAmt,invTtlTax);	
						test += "\n\n\n";
					}else test += "\nTHANK YOU\n\n\n\n";

					//byte[] cc =ASCIIEncoding.ASCII.GetBytes(test);
					byte[] cc = Encoding.GetEncoding("GB18030").GetBytes(test);
					int bLen= cc.Length;

					for (int i=0; i<noofcopy;i++)
					{
						int rem;
						int result =Math.DivRem(cc.Length, 2048, out rem);
						int pos =0;
						for(int line= 0;line<result;line++)
						{
							IsStreamCanWrite (mmOutputStream);
							mmOutputStream.Write (cc, pos, 2048);
							pos += 2048;
						}
						if (rem >0)
							mmOutputStream.Write (cc, pos, rem);
						mmOutputStream.Flush();
						//mmOutputStream.BeginWrite(cc,0,cc.Length,
						//mmOutputStream.Write (cc, 0, cc.Length);
						Thread.Sleep (3000);
						//mmOutputStream.Flush ();
					}
					Thread.Sleep (300);
					mmOutputStream.Close ();
					//mmInputStream.Close();
					mmSocket.Close ();
					msg ="Printing....";
				} else {
					//txtv.Text = "Device not connected";
					msg= "Device not connected";	
				}


			} catch (Exception ex) {
				msg = ex.Message;
			}

			//	return msg;
		}


		static string PrintDetail(InvoiceDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper().Trim()+ "\n";
			pline2 = pline2 + count.ToString ().PadRight (3, ' ');
			if (itm.qty < 0) {
				string sqty = "(EX)"+itm.qty.ToString ().Trim ()  ;
				pline2 = pline2 +sqty.PadLeft (9, ' ')+" ";
			}else  pline2 = pline2 + itm.qty.ToString ().PadLeft (9, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.price, 2).ToString ("n2").PadLeft (8, ' ')+" ";
			string stax=Math.Round (itm.tax, 2).ToString ("n2") +" "+ itm.taxgrp;
			pline2 = pline2 + stax.PadLeft (10, ' ') + " ";
			pline2 = pline2 + Math.Round (itm.netamount, 2).ToString ("n2").PadLeft (9, ' ');
			test += pline2 + "\n";
					
			return test;
		}

		static string PrintSODetail(SaleOrderDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper().Trim()+ "\n";
			pline2 = pline2 + count.ToString ().PadRight (3, ' ');
			if (itm.qty < 0) {
				string sqty = "(EX)"+itm.qty.ToString ().Trim ()  ;
				pline2 = pline2 +sqty.PadLeft (9, ' ')+" ";
			}else  pline2 = pline2 + itm.qty.ToString ().PadLeft (9, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.price, 2).ToString ("n2").PadLeft (8, ' ')+" ";
			string stax=Math.Round (itm.tax, 2).ToString ("n2") +" "+ itm.taxgrp;
			pline2 = pline2 + stax.PadLeft (10, ' ') + " ";
			pline2 = pline2 + Math.Round (itm.netamount, 2).ToString ("n2").PadLeft (9, ' ');
			test += pline2 + "\n";

			return test;
		}

		static string PrintDODetail(DelOrderDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper ().Trim ();
			string scount = count.ToString ().PadRight (4, ' ');
			if (pline2.Length > 33) {
			
				string[] strs = pline2.Split (new char[]{ ' ' });
				string tmp = "";

				string sqty = itm.qty.ToString ("n").PadLeft (5, ' ');
				foreach (string s in strs) {
					if ((tmp + s + " ").Length > 33) {
						test  = test + scount + tmp.PadRight (33, ' ') + sqty+"\n"; 
						scount = "".PadRight (4, ' ');
						sqty = "".PadRight (5, ' ');
						tmp = s+" ";
					} else {
						tmp = tmp + s+" ";
					}
				}
				test = test + "".PadRight (4, ' ') + tmp + "\n";

			} else {
				test = count.ToString ().PadRight (4, ' ') + pline2.PadRight (33, ' ')+scount+ "\n";

			}

			return test;
		}


		static string PrintCNDetail(CNNoteDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper().Trim()+ "\n";
			pline2 = pline2 + count.ToString ().PadRight (3, ' ');
			if (itm.qty < 0) {
				string sqty = "(EX)"+itm.qty.ToString ().Trim ()  ;
				pline2 = pline2 +sqty.PadLeft (9, ' ')+" ";
			}else  pline2 = pline2 + itm.qty.ToString ().PadLeft (9, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.price, 2).ToString ("n2").PadLeft (8, ' ')+" ";
			string stax=Math.Round (itm.tax, 2).ToString ("n2") +" "+ itm.taxgrp;
			pline2 = pline2 + stax.PadLeft (10, ' ') + " ";
			pline2 = pline2 + Math.Round (itm.netamount, 2).ToString ("n2").PadLeft (9, ' ');
			test += pline2 + "\n";

			return test;
		}

		void PrintCompHeader (ref string test)
		{
			CompanyInfo comp= DataHelper.GetCompany (pathToDatabase);
			if (comp == null)
				return;
			string tel = string.IsNullOrEmpty (comp.Tel) ? " " : comp.Tel.Trim ();
			string fax = string.IsNullOrEmpty (comp.Fax) ? " " : comp.Fax.Trim ();
			string addr1 =string.IsNullOrEmpty (comp.Addr1) ? "" : comp.Addr1.Trim ();
			string addr2 =string.IsNullOrEmpty (comp.Addr2) ? "" : comp.Addr2.Trim ();
			string addr3 =string.IsNullOrEmpty (comp.Addr3) ? "" : comp.Addr3.Trim ();
			string addr4 =string.IsNullOrEmpty (comp.Addr4) ? "" : comp.Addr4.Trim ();
			string gst =string.IsNullOrEmpty (comp.GSTNo) ? "" : comp.GSTNo.Trim ();
			if ((comp.CompanyName.Trim ().Length + comp.RegNo.Trim ().Length + 2) > 42) {
				test += comp.CompanyName.Trim () + "\n";
				test += "(" + comp.RegNo.Trim () + ")\n";
			} else {
				test += comp.CompanyName.Trim ()+"(" + comp.RegNo.Trim () + ")\n";
			}
			if (addr1!="")
				test += comp.Addr1.Trim () + "\n";	
			if (addr2!="")
				test += comp.Addr2.Trim () + "\n";	
			if (addr3!="")
				test += comp.Addr3.Trim () + "\n";	
			if (addr4!="")
				test += comp.Addr4.Trim () + "\n";	

			test += "TEL:" + tel+"  FAX:"+fax+"\n";
			test += "GST NO:" + gst+"\n";

		}

		void PrintCustomer (ref string test,string custcode)
		{
			Trader comp = DataHelper.GetTrader (pathToDatabase, custcode);
			test += "------------------------------------------\n";
			test += "CUSTOMER\n";
			string tel = string.IsNullOrEmpty (comp.Tel) ? " " : comp.Tel.Trim ();
			string fax = string.IsNullOrEmpty (comp.Fax) ? " " : comp.Fax.Trim ();
			string addr1 =string.IsNullOrEmpty (comp.Addr1) ? "" : comp.Addr1.Trim ();
			string addr2 =string.IsNullOrEmpty (comp.Addr2) ? "" : comp.Addr2.Trim ();
			string addr3 =string.IsNullOrEmpty (comp.Addr3) ? "" : comp.Addr3.Trim ();
			string addr4 =string.IsNullOrEmpty (comp.Addr4) ? "" : comp.Addr4.Trim ();
			string gst =string.IsNullOrEmpty (comp.gst) ? "" : comp.gst.Trim ();

			test += comp.CustName.Trim () + "\n";
			if (addr1!="")
				test += comp.Addr1.Trim () + "\n";	
			if (addr2!="")
				test += comp.Addr2.Trim () + "\n";	
			if (addr3!="")
				test += comp.Addr3.Trim () + "\n";	
			if (addr4!="")
				test += comp.Addr4.Trim () + "\n";	

			test += "TEL:" + tel+"  FAX:"+fax+"\n";
			test += "GST NO:" + gst+"\n";
			test += "------------------------------------------\n";
		}
		void PrintFooter (ref string test)
		{
			test += "\n\n\n\n";
			test += "------------------------------------------\n";
			test += "     RECEIVED BY (COMPANY CHOP AND SIGN)  \n";

		
		}
		void PrintHeader (ref string test,Invoice inv)
		{
			string userid = USERID;
			string[] titles = apara.ReceiptTitle.Split (new char[]{ ',', '|'});
			string title1 = "";
			string title2 = "";

			if (titles.Length ==1)
				title1 = titles [0].ToUpper ();
			if (titles.Length > 1) {
				title1 = titles [0].ToUpper ();
				title2 = titles [1].ToUpper ();
			}
			if (titles.Length == 0 || title1=="")
				title1 = "TAX INVOICE";
				
			string date = DateTime.Now.ToString ("dd-MM-yyyy");
			string datetime = DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt");
			if (compinfo.ShowTime) {
				test += datetime+title1.PadLeft(41-datetime.Length,' ')+"\n";
			} else {
				test += date+title1.PadLeft(41-date.Length,' ')+"\n";
			}
			//test += DateTime.Now.ToString ("dd-MM-yyyy")+"TAX INVOICE".PadLeft(31,' ')+"\n";
			string recno = "RECPT NO : " + inv.invno.Trim();
			//test += "RECPT NO : " + inv.invno+"\n";
			test += recno+title2.PadLeft(41-recno.Length,' ')+"\n";
			string issueline = "ISSUED BY: " + userid.ToUpper ();
			int templen = 41 - issueline.Length;
			string term = "("+((inv.trxtype.IndexOf("CASH")>-1)?"COD":"TERM")+")"; 
			issueline = issueline + term.PadLeft (templen, ' ')+"\n";
			test += issueline;// "ISSUED BY: " + userid.ToUpper()+"\n";
			test += "------------------------------------------\n";
			test += "DESCRIPTION                               \n";
			test += "NO       QTY  U/PRICE    TAX AMT    AMOUNT\n";
			test += "------------------------------------------\n";

		}

		void PrintSOHeader (ref string test,SaleOrder so)
		{
			string userid = USERID;
			string title1 = "SALES ORDER";
			string date = DateTime.Now.ToString ("dd-MM-yyyy");
			string datetime = DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt");
			if (compinfo.ShowTime) {
				test += datetime+title1.PadLeft(41-datetime.Length,' ')+"\n";
			} else {
				test += date+title1.PadLeft(41-date.Length,' ')+"\n";
			}
			string recno = "SALES ORDER NO : " + so.sono.Trim();
			test += recno+"\n";
			test += "CUST PO NO : " + so.custpono+"\n";
			string issueline = "ISSUED BY: " + userid.ToUpper ();
			issueline = issueline +"\n";
			test += issueline;// "ISSUED BY: " + userid.ToUpper()+"\n";
			if (so.remark.Length > 1) {
				test += "REMARK:\n";
				test += so.remark+"\n";
			}
			test += "------------------------------------------\n";
			test += "DESCRIPTION                               \n";
			test += "NO       QTY  U/PRICE    TAX AMT    AMOUNT\n";
			test += "------------------------------------------\n";
		
		}

		void PrintDOHeader (ref string test,DelOrder doOrder)
		{
			string userid = USERID;
			string title1 = "DELIVERY ORDER";
			string date = DateTime.Now.ToString ("dd-MM-yyyy");
			string datetime = DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt");
			if (compinfo.ShowTime) {
				test += datetime+title1.PadLeft(41-datetime.Length,' ')+"\n";
			} else {
				test += date+title1.PadLeft(41-date.Length,' ')+"\n";
			}
			string recno = "DELIVERY ORDER NO : " + doOrder.dono.Trim();
			test += recno+"\n";
			string issueline = "ISSUED BY: " + userid.ToUpper ();
			issueline = issueline +"\n";
			test += issueline;// "ISSUED BY: " + userid.ToUpper()+"\n";
			if (doOrder.remark.Length > 1) {
				test += "REMARK:\n";
				test += doOrder.remark+"\n";
			}
			test += "------------------------------------------\n";
			test += "NO  DESCRIPTION                       QTY \n";
			test += "------------------------------------------\n";
			     //  1234
				//	  	 12345678901234567890123456789012312345
				//
		}

		void PrintCNHeader (ref string test,CNNote inv)
		{
			string userid = USERID;
			string[] titles = apara.ReceiptTitle.Split (new char[]{ ',', '|'});
			string title1 = "";
			string title2 = "";
			title1 = "CREDIT NOTE";

			string date = DateTime.Now.ToString ("dd-MM-yyyy");
			string datetime = DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt");
			if (compinfo.ShowTime) {
				test += datetime+title1.PadLeft(41-datetime.Length,' ')+"\n";
			} else {
				test += date+title1.PadLeft(41-date.Length,' ')+"\n";
			}
			//test += DateTime.Now.ToString ("dd-MM-yyyy")+"TAX INVOICE".PadLeft(31,' ')+"\n";
			string recno = "CREDIT NOTE NO : " + inv.cnno.Trim();
			//test += "RECPT NO : " + inv.invno+"\n";
			test += recno+title2.PadLeft(41-recno.Length,' ')+"\n";
			string issueline = "ISSUED BY: " + userid.ToUpper ();
			int templen = 41 - issueline.Length;
			string term = "("+((inv.trxtype.IndexOf("CASH")>-1)?"COD":"TERM")+")"; 
			issueline = issueline + term.PadLeft (templen, ' ')+"\n";
			test += issueline;// "ISSUED BY: " + userid.ToUpper()+"\n";
			test += "------------------------------------------\n";
			test += "DESCRIPTION                               \n";
			test += "NO       QTY  U/PRICE    TAX AMT    AMOUNT\n";
			test += "------------------------------------------\n";

		}

		void PrintTotal (ref string test,double ttlAmt,double ttlTax)
		{

			test += "------------------------------------------\n";
			test += "               TOTAL EXCL GST "+Math.Round(ttlAmt,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "               TOTAL TAX      "+Math.Round(ttlTax,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "               TOTAL INCL GST "+Math.Round(ttlAmt+ttlTax,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "------------------------------------------\n";
		}

		void PrintDOTotal (ref string test,double ttlQty)
		{

			test += "------------------------------------------\n";
			test += "                   TOTAL QTY  "+ttlQty.ToString("n").PadLeft (12, ' ')+"\n";
			test += "------------------------------------------\n";
		}
		void PrintTotal (ref string test,double cnttlAmt,double cnttlTax,double InvttlAmt,double invttlTax)
		{
			double ttlCollect = (InvttlAmt + invttlTax) - (cnttlAmt + cnttlTax);
			test += "------------------------------------------\n";
			test += "  TOTAL C/NOTE AMOUNT  : "+Math.Round(cnttlAmt+cnttlTax,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "  TOTAL INVOICE AMOUNT : "+Math.Round(InvttlAmt+invttlTax,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "  TOTAL COLLECT AMOUNT : "+Math.Round(ttlCollect,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "------------------------------------------\n";
		}

		void PrintTaxSumm(ref string test,InvoiceDtls[] list )
		{
			List<Item> list2 = new List<Item> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				list2 = db.Table<Item> ().ToList<Item> ();
			}
			var grp = from p in list
				group p by p.taxgrp into g
				select new {taxgrp = g.Key, ttltax = g.Sum (x => x.tax),ttlAmt = g.Sum (v => v.netamount)};

			test += "SUMMARY\n";
			test += "-------------------------------\n";
			test += "TAX            AMOUNT   TAX AMT\n";
			test += "-------------------------------\n";
			//       123456789 12345678901 123456789 
			string pline="";
			foreach (var g in grp) {
				var list3 =list2.Where (x => x.taxgrp == g.taxgrp).ToList ();
				if (list3.Count > 0) {
					string stax = g.taxgrp.Trim () + " @ " + list3 [0].tax.ToString () + "%";
					pline = pline + stax.PadRight (10,' ');
				} else pline = pline + g.taxgrp.Trim().PadRight (10, ' ');
				pline = pline + g.ttlAmt.ToString("n2").PadLeft(11, ' ')+" ";
				pline = pline + g.ttltax.ToString("n2").PadLeft(9, ' ');
				test += pline + "\n";
				pline = "";
			}
			test += "-------------------------------\n";
		}

		void PrintSOTaxSumm(ref string test,SaleOrderDtls[] list )
		{
			List<Item> list2 = new List<Item> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				list2 = db.Table<Item> ().ToList<Item> ();
			}
			var grp = from p in list
				group p by p.taxgrp into g
				select new {taxgrp = g.Key, ttltax = g.Sum (x => x.tax),ttlAmt = g.Sum (v => v.netamount)};

			test += "SUMMARY\n";
			test += "-------------------------------\n";
			test += "TAX            AMOUNT   TAX AMT\n";
			test += "-------------------------------\n";
			//       123456789 12345678901 123456789 
			string pline="";
			foreach (var g in grp) {
				var list3 =list2.Where (x => x.taxgrp == g.taxgrp).ToList ();
				if (list3.Count > 0) {
					string stax = g.taxgrp.Trim () + " @ " + list3 [0].tax.ToString () + "%";
					pline = pline + stax.PadRight (10,' ');
				} else pline = pline + g.taxgrp.Trim().PadRight (10, ' ');
				pline = pline + g.ttlAmt.ToString("n2").PadLeft(11, ' ')+" ";
				pline = pline + g.ttltax.ToString("n2").PadLeft(9, ' ');
				test += pline + "\n";
				pline = "";
			}
			test += "-------------------------------\n";
		}

		void PrintCNTaxSumm(ref string test,CNNoteDtls[] list )
		{
			List<Item> list2 = new List<Item> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				list2 = db.Table<Item> ().ToList<Item> ();
			}
			var grp = from p in list
				group p by p.taxgrp into g
				select new {taxgrp = g.Key, ttltax = g.Sum (x => x.tax),ttlAmt = g.Sum (v => v.netamount)};

			test += "SUMMARY\n";
			test += "-------------------------------\n";
			test += "TAX            AMOUNT   TAX AMT\n";
			test += "-------------------------------\n";
			//       123456789 12345678901 123456789 
			string pline="";
			foreach (var g in grp) {
				var list3 =list2.Where (x => x.taxgrp == g.taxgrp).ToList ();
				if (list3.Count > 0) {
					string stax = g.taxgrp.Trim () + " @ " + list3 [0].tax.ToString () + "%";
					pline = pline + stax.PadRight (10,' ');
				}else pline = pline + g.taxgrp.Trim().PadRight (10, ' ');
				pline = pline + g.ttlAmt.ToString("n2").PadLeft(11, ' ')+" ";
				pline = pline + g.ttltax.ToString("n2").PadLeft(9, ' ');
				test += pline + "\n";
				pline = "";
			}
			test += "-------------------------------\n";
		}

		#endregion Print Receipt

		public string PrintInvSumm(BluetoothSocket mmSocket,BluetoothDevice mmDevice,DateTime printDate1,DateTime printDate2)
		{
			msg = "";
			Stream mmOutputStream;
			try {
				UUID uuid = UUID.FromString ("00001101-0000-1000-8000-00805F9B34FB");
				mmSocket = mmDevice.CreateInsecureRfcommSocketToServiceRecord (uuid);
				if (mmSocket == null) {
					msg = "Error creating sockect";
					return msg;
				}
				if (mmDevice.BondState == Bond.Bonded) {
					mmSocket.Connect ();
					Thread.Sleep (300);
					mmOutputStream = mmSocket.OutputStream;
					byte[] charfont;
					if (apara.PaperSize == "58mm") {
						charfont = new Byte[] { 27, 33, 1 }; //Char font 9x17
						mmOutputStream.Write (charfont, 0, charfont.Length);
					}

					if (apara.PaperSize == "80mm") {
						charfont = new Byte[] { 27, 33, 0 }; //Char font 12x24
						mmOutputStream.Write (charfont, 0, charfont.Length);
					}

					string text = GetInvoiceSumm (printDate1, printDate2);
					//byte[] cc = ASCIIEncoding.ASCII.GetBytes(text);

					byte[] cc = Encoding.GetEncoding ("GB18030").GetBytes (text);
					int rem;
					int result = Math.DivRem (cc.Length, 2048, out rem);
					int pos = 0;
					for (int line = 0; line < result; line++) {
						IsStreamCanWrite (mmOutputStream);
						mmOutputStream.Write (cc, pos, 2048);
						pos += 2048;
					}
					if (rem > 0)
						mmOutputStream.Write (cc, pos, rem);
					

					//mmOutputStream.Write (cc, 0, cc.Length);
					Thread.Sleep (3000);
					mmOutputStream.Flush ();
					mmOutputStream.Close ();
					mmSocket.Close ();
				}

			} catch (Exception ex) {
				msg = ex.Message;
			}

			return msg;
		}

		void PrintSummHeader (DateTime printdate1, DateTime printdate2, ref string text, bool isSamedate)
		{
			text += "------------------------------------------\n";
			text += compinfo.CompanyName.ToUpper () + "\n";
			text += "USER ID  : " + USERID + "\n";
			text += "PRINT ON : " + DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt") + "\n";
			if (isSamedate)
				text += "DAILTY SUMMARY ON " + printdate1.ToString ("yy-MM-yyyy") + "\n";
			else {
				text += "DAILTY SUMMARY ON " + printdate1.ToString ("yy-MM-yyyy") + " - " + printdate2.ToString ("yy-MM-yyyy") + "\n";
			}
			text += "------------------------------------------\n";
			text += "NO  INVOICE NO   TYPE     TAX AMT   AMOUNT\n";
			text += "------------------------------------------\n";
		}

		Invoice[] GetInvoices (DateTime printdate1, DateTime printdate2)
		{
			Invoice[] invs =  {

			};
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<Invoice> ().Where (x => x.invdate >= printdate1 && x.invdate <= printdate2).OrderBy (x => x.invdate).ToList<Invoice> ();
				invs = new Invoice[list.Count];
				list.CopyTo (invs);
			}
			return invs;
		}

		private string GetInvoiceSumm(DateTime printdate1,DateTime printdate2 )
		{
			string text = "";
			bool isSamedate = printdate1==printdate2;
			PrintSummHeader (printdate1, printdate2, ref text, isSamedate);
			var invs = GetInvoices (printdate1, printdate2);

			var grp= from inv in invs 
					  group inv by inv.invdate 
				      into g select new {key=g.Key,results=g};

			double ttlcash = 0;
			double ttlInv = 0;
			double ttltax = 0;
			double ttlamt = 0;
			double subttltax = 0;
			double subttlamt = 0;
			bool multiType = false;
			string line = "";
			int cont = 0;
			foreach (var g in grp) { //group by date
				var list = g.results.OrderBy (x => x.invno);
				var typgrp = from ty in list
				             group ty by ty.trxtype	into tg
				             select new {key = tg.Key,results = tg};
			
				if (!isSamedate) {
					text = text + g.key.ToString ("dd-MM-yyyy") + "\n";
					text = text +"---------- \n";
				}
				multiType = (typgrp.Count() > 1);
				foreach (var tygrp in typgrp) {  //group by trxtype
					text = text +"[ "+ tygrp.key.ToUpper() + " ]\n";
					var list2 = tygrp.results.OrderBy (x => x.invno);
					subttltax = 0;
					subttlamt = 0;
					cont = 0;
					foreach (Invoice inv in list2) {
						cont += 1;
						ttltax += inv.taxamt;
						ttlamt += inv.amount;
						subttltax += inv.taxamt;
						subttlamt += inv.amount;
						if (tygrp.key.ToUpper () == "CASH") {
							ttlcash = ttlcash + inv.amount + inv.taxamt;
						}else ttlInv = ttlInv + inv.amount + inv.taxamt;
						line = (cont.ToString () + ".").PadRight (4, ' ') +
						inv.invno.PadRight (13, ' ') +
						inv.trxtype.PadRight (8, ' ') +
						inv.taxamt.ToString ("n2").PadLeft (9, ' ') +
						inv.amount.ToString ("n2").PadLeft (8, ' ') + "\n";
						text = text + line;
					}
					//if (multiType) {
						text = text + PrintSubTotal (subttltax, subttlamt);
				//	}
				}
			}

			double ttl = ttlamt + ttltax;
			text += "------------------------------------------\n";
			text += "TOTAL TAX     :" + ttltax.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += "TOTAL AMOUNT  :" + ttlamt.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += "      TOTAL   :" + ttl.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += "------------------------------------------\n";
			text += "SUMMARY\n";
			text += "TOTAL CASH    :" + ttlcash.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += "TOTAL INVOICE :" + ttlInv.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += "------------------------------------------\n\n\n\n";

			return text;
		}

		string PrintSubTotal(double ttltax,double ttlamt)
		{
			double ttl = ttlamt + ttltax;
			string text ="";
			text += "------------------------------------------\n";
			text += " SUB TOTAL TAX    :" + ttltax.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += " SUB TOTAL AMOUNT :" + ttlamt.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += "        SUB TOTAL :" + ttl.ToString ("n2").PadLeft (13, ' ')+"\n";
			//text += "------------------------------------------\n";
			return text;				
		}
	}

	/*//Bluetooth Printer -PT-II and PT-III
		public string BlueToothPT(BluetoothSocket mmSocket,BluetoothDevice mmDevice,Invoice inv,InvoiceDtls[] list,int noofcopy )
		{
			string msg = "";
			Stream mmOutputStream;
			//TextView txtv = FindViewById<TextView> (Resource.Id.textView2);
			try {
				UUID uuid = UUID.FromString ("00001101-0000-1000-8000-00805F9B34FB");

				mmSocket = mmDevice.CreateInsecureRfcommSocketToServiceRecord (uuid);
				if (mmSocket == null) {
					//txtv.Text = "Error creating sockect";
					msg =  "Error creating sockect";
					return msg;
				}
				//txtv.Text = mmDevice.BondState.ToString ();
				if (mmDevice.BondState == Bond.Bonded) {
					mmSocket.Connect ();
					Thread.Sleep (300);
					//mmInputStream = mmSocket.InputStream;
					mmOutputStream = mmSocket.OutputStream;
//					byte[] charLarge = { 0x1d, 0x21, 0x10 };//this command for T9
//					mmOutputStream.Write (charLarge, 0, charLarge.Length);
//					byte[] charLarge3 = { 0x1b, 0x61, 0x01 };//aligment left
//					mmOutputStream.Write (charLarge3, 0, charLarge3.Length);
//					//mmOutputStream.Write (bb, 0, bb.Length);
//					byte[] charLarge4 = { 0x1b, 0x61, 0x00 };//this command for T9
//					mmOutputStream.Write (charLarge4, 0, charLarge4.Length);
//					byte[] charLarge1 = { 0x1d, 0x21, 0 };//this command for T9
//					mmOutputStream.Write (charLarge1, 0, charLarge1.Length);
					string test = "";

					PrintCompHeader (ref test);
					PrintCustomer (ref test,inv.custcode);
					PrintHeader (ref test,inv);
					string dline="";
					double ttlAmt =0;
					double ttltax =0;
					int count =0;
					foreach(InvoiceDtls itm in list)
					{
						count+=1;
						dline =dline+PrintDetail (itm,count);
						ttlAmt = ttlAmt+ itm.netamount;
						ttltax = ttltax+itm.tax;
					}
					test += dline;
					PrintTotal (ref test,ttlAmt,ttltax);

					PrintTaxSumm(ref test,list );
					PrintFooter (ref test);
					test += "\nTHANK YOU\n\n\n\n";
					byte[] cc = Encoding.ASCII.GetBytes (test);
					for (int i=0; i<noofcopy;i++)
					{
						mmOutputStream.Write (cc, 0, cc.Length);
						mmOutputStream.Flush ();
						Thread.Sleep (300);
					}
					mmOutputStream.Close ();
					//mmInputStream.Close();
					mmSocket.Close ();
					msg ="Printing....";
				} else {
					//txtv.Text = "Device not connected";
					msg= "Device not connected";	
				}


			} catch (Exception ex) {
				msg = ex.Message;
			}

			return msg;
		}
	 */

	/*
	//Bluetooth Printer -Mini Bluetooth Printer
	public void BluetoothMini(BluetoothSocket mmSocket,BluetoothDevice mmDevice,Invoice inv,InvoiceDtls[] list,int noofcopy)
	{
		msg = "";
		Stream mmOutputStream;
		//TextView txtv = FindViewById<TextView> (Resource.Id.textView2);
		try {
			UUID uuid = UUID.FromString ("00001101-0000-1000-8000-00805F9B34FB");

			mmSocket = mmDevice.CreateInsecureRfcommSocketToServiceRecord (uuid);
			if (mmSocket == null) {
				//txtv.Text = "Error creating sockect";
				msg =  "Error creating sockect";
				//errmsg.Append(msg);
				return;
			}
			//txtv.Text = mmDevice.BondState.ToString ();
			if (mmDevice.BondState == Bond.Bonded) {
				//errmsg.Append("Start to connect\n");

				//mmSocket.Connect ();
				TrytoConnect(mmSocket);

				Thread.Sleep (300);
				//mmInputStream = mmSocket.InputStream;
				//errmsg.Append("connected\n");
				mmOutputStream = mmSocket.OutputStream;
				byte[] charfont;
				charfont = new Byte[] { 27, 64 }; //Char font 9x17
				mmOutputStream.Write(charfont, 0, charfont.Length);
				if (apara.PaperSize=="58mm")
				{
					charfont = new Byte[] { 27, 33, 1 }; //Char font 9x17
					//charfont = new Byte[] { 27, 77, 1 }; //Char font 9x17
					mmOutputStream.Write(charfont, 0, charfont.Length);
				}

				if (apara.PaperSize=="80mm")
				{
					charfont = new Byte[] { 27, 33, 0 }; //Char font 12x24
					//charfont = new Byte[] { 27, 77, 1 }; //Char font 9x17
					mmOutputStream.Write(charfont, 0, charfont.Length);
				}
				charfont = new Byte[] { 28, 38 };
				mmOutputStream.Write(charfont, 0, charfont.Length);
				charfont = new Byte[] { 28, 67,0,48 };
				mmOutputStream.Write(charfont, 0, charfont.Length);
				//charfont = new Byte[] { 27, 82, 15 }; //Char font 9x17
				//	mmOutputStream.Write(charfont, 0, charfont.Length);
				string test = "";
				//errmsg.Append("Set printer\n");
				PrintInvoice (inv, list, ref test);
				//var encoding = Encoding.GetEncoding(936);
				//byte[] source = Encoding.Unicode.GetBytes(text);
				//byte[] converted = Encoding.Convert(Encoding.Unicode, encoding, source);
				//errmsg.Append("Start Endcoidng\n");
				byte[] cc = Encoding.GetEncoding("GB18030").GetBytes(test);
				for (int i=0; i<noofcopy;i++)
				{
					int rem;
					int result =Math.DivRem(cc.Length, 2048, out rem);
					int pos =0;
					for(int line= 0;line<result;line++)
					{
						IsStreamCanWrite (mmOutputStream);
						mmOutputStream.Write (cc, pos, 2048);
						pos += 2048;
					}
					if (rem >0)
						mmOutputStream.Write (cc, pos, rem);
					//mmOutputStream.BeginWrite(cc,0,cc.Length,
					//mmOutputStream.Write (cc, 0, cc.Length);
					Thread.Sleep (3000);
					//mmOutputStream.Flush ();
				}
				Thread.Sleep (300);
				charfont = new Byte[] { 28, 46 };
				mmOutputStream.Write(charfont, 0, charfont.Length);
				mmOutputStream.Close ();
				//mmInputStream.Close();
				mmSocket.Close ();
				msg ="Printing....";
			} else {
				//txtv.Text = "Device not connected";
				msg= "Device not connected";	
				//errmsg.Append(msg);
			}
		} catch (Exception ex) {
			msg = ex.Message;
		}
	}

	public void BluetoothMiniSO(BluetoothSocket mmSocket,BluetoothDevice mmDevice,SaleOrder inv,SaleOrderDtls[] list,int noofcopy)
	{
		msg = "";
		Stream mmOutputStream;
		//TextView txtv = FindViewById<TextView> (Resource.Id.textView2);
		try {
			UUID uuid = UUID.FromString ("00001101-0000-1000-8000-00805F9B34FB");
			mmSocket = mmDevice.CreateInsecureRfcommSocketToServiceRecord (uuid);
			if (mmSocket == null) {
				msg =  "Error creating sockect";
				return;
			}
			if (mmDevice.BondState == Bond.Bonded) {
				TrytoConnect(mmSocket);
				Thread.Sleep (300);
				mmOutputStream = mmSocket.OutputStream;
				byte[] charfont;
				charfont = new Byte[] { 27, 64 }; //Char font 9x17
				mmOutputStream.Write(charfont, 0, charfont.Length);
				if (apara.PaperSize=="58mm")
				{
					charfont = new Byte[] { 27, 33, 1 }; //Char font 9x17
					mmOutputStream.Write(charfont, 0, charfont.Length);
				}

				if (apara.PaperSize=="80mm")
				{
					charfont = new Byte[] { 27, 33, 0 }; //Char font 12x24
					mmOutputStream.Write(charfont, 0, charfont.Length);
				}
				charfont = new Byte[] { 28, 38 };
				mmOutputStream.Write(charfont, 0, charfont.Length);
				charfont = new Byte[] { 28, 67,0,48 };
				mmOutputStream.Write(charfont, 0, charfont.Length);
				string test = "";
				PrintSO (inv, list, ref test);
				byte[] cc = Encoding.GetEncoding("GB18030").GetBytes(test);
				for (int i=0; i<noofcopy;i++)
				{
					int rem;
					int result =Math.DivRem(cc.Length, 2048, out rem);
					int pos =0;
					for(int line= 0;line<result;line++)
					{
						IsStreamCanWrite (mmOutputStream);
						mmOutputStream.Write (cc, pos, 2048);
						pos += 2048;
					}
					if (rem >0)
						mmOutputStream.Write (cc, pos, rem);
					Thread.Sleep (3000);
				}
				Thread.Sleep (300);
				charfont = new Byte[] { 28, 46 };
				mmOutputStream.Write(charfont, 0, charfont.Length);
				mmOutputStream.Close ();
				mmSocket.Close ();
				msg ="Printing....";
			} else {
				msg= "Device not connected";	
			}
		} catch (Exception ex) {
			msg = ex.Message;
		}
	}
	*/
}

