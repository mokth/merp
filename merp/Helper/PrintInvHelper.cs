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

namespace wincom.mobile.erp
{
	public class PrintInvHelper
	{
		string pathToDatabase;
		string USERID;
		AdPara apara;
		CompanyInfo compinfo;
		string msg;
		public PrintInvHelper (string adpathToDatabase,string userid)
		{
			pathToDatabase = adpathToDatabase;
			USERID = userid;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			compinfo =DataHelper.GetCompany(pathToDatabase);
		}

		private void PrintLine(string text,Stream mmOutputStream)
		{
			byte[] cc = Encoding.ASCII.GetBytes (text);
			mmOutputStream.Write (cc, 0, cc.Length);
		}

		#region Print Receipt
		public string OpenBTAndPrint(BluetoothSocket mmSocket,BluetoothDevice mmDevice,Invoice inv,InvoiceDtls[] list,int noofcopy )
		{string msg = "";
//			if (apara.PrinterName.ToUpper().Contains ("PT-I")) {
//				msg =BlueToothPT(mmSocket, mmDevice, inv, list, noofcopy);
//			} 
//			if (apara.PrinterName.ToUpper().Contains ("BLUETOOTH"))  {
//				msg =BluetoothMini (mmSocket, mmDevice, inv, list, noofcopy);
//			}

			BluetoothMini (mmSocket, mmDevice, inv, list, noofcopy);

			return msg;
		}

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
					return;
				}
				//txtv.Text = mmDevice.BondState.ToString ();
				if (mmDevice.BondState == Bond.Bonded) {
					mmSocket.Connect ();
					Thread.Sleep (300);
					//mmInputStream = mmSocket.InputStream;
					mmOutputStream = mmSocket.OutputStream;
					byte[] charfont;
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
					byte[] cc =ASCIIEncoding.ASCII.GetBytes(test);

					for (int i=0; i<noofcopy;i++)
					{
//						int reminder;
//						int x = Math.DivRem(cc.Length, 256, out reminder);
//						int pos =0;
//						for(int n=0;n<x;n++)
//						{
//							mmOutputStream.Write(cc,pos,256);
//							Thread.Sleep (100);
//							pos = pos+256;
//						}
//						if (reminder>0)
//							mmOutputStream.Write(cc,cc.Length-reminder-1,reminder);

						int nwait=0;
						while(true)
						{
							if (mmOutputStream.CanWrite)
								break;
							else 
							{
								nwait+=1;
								if (nwait>10)
									break;
							}
							Thread.Sleep (300);
						}
						//mmOutputStream.BeginWrite(cc,0,cc.Length,
						mmOutputStream.Write (cc, 0, cc.Length);
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

		//Bluetooth Printer -PT-II and PT-III
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

		static string PrintDetail(InvoiceDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string[] strlist = desc.Split (new char[] { ' ' });
			string line = "";
			int counter = 0;
			foreach (string s in strlist) {
				if (line.Length + s.Length > 42) {
					counter = counter + 1;
					if (counter == 1) {
						string pline = line.Trim ().PadRight (42, ' ');
						test += pline + "\n";
					} else if (counter == 2) {
						string pline = line.Trim ().PadRight (42, ' ');
					} else {
						string pline = line.Trim ().PadRight (42, ' ');
						test += pline + "\n";
					}
					line = s;
				} else {
					line = line + s;
				}
			}

			string pline2 = line+ "\n";
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
			string[] titles = apara.ReceiptTitle.Split (new char[]{ ',', '|', '/' });
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
			test += "ISSUED BY: " + userid.ToUpper()+"\n";
			test += "------------------------------------------\n";
			test += "DESCRIPTION                               \n";
			test += "NO       QTY  U/PRICE    TAX AMT    AMOUNT\n";
			test += "------------------------------------------\n";
			/*
            DESCRIPTION       QTY    U/PRICE    AMOUNT
			                  TAXGR  TAX AMT  
			------------------------------------------
			xxxxxxxxxxxxxxxxx xxxxx xxxxxxxx xxxxxxxxx
			xxxxxxxxxxxxxxxxx xxxxx xxxxxxxx xxxxxxxxx
			12345678901234567 12345 12345678 123456789
		  */
		}

		void PrintTotal (ref string test,double ttlAmt,double ttlTax)
		{

			test += "------------------------------------------\n";
			test += "               TOTAL EXCL GST "+Math.Round(ttlAmt,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "               TOTAL TAX      "+Math.Round(ttlTax,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "               TOTAL INCL GST "+Math.Round(ttlAmt+ttlTax,2).ToString("n2").PadLeft (12, ' ')+"\n";
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
					msg =  "Error creating sockect";
					return msg;
				}
				if (mmDevice.BondState == Bond.Bonded) {
					mmSocket.Connect ();
					Thread.Sleep (300);
					mmOutputStream = mmSocket.OutputStream;
					byte[] charfont;
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
					int nwait=0;
					while(true)
					{
						if (mmOutputStream.CanWrite)
							break;
						else 
						{
							nwait+=1;
							if (nwait>10)
								break;
						}
						Thread.Sleep (300);
					}
					string text= GetInvoiceSumm(printDate1,printDate2);
					byte[] cc = ASCIIEncoding.ASCII.GetBytes(text);
					mmOutputStream.Write (cc, 0, cc.Length);
					Thread.Sleep (3000);
					mmOutputStream.Flush();
					mmOutputStream.Close();
					mmSocket.Close();
				}

			}catch(Exception ex)
			{
				msg = ex.Message;
			}

			return msg;
		}
		private string GetInvoiceSumm(DateTime printdate1,DateTime printdate2 )
		{
			string text = "";
			bool isSamedate = printdate1==printdate2;
			text += "------------------------------------------\n";
			text += compinfo.CompanyName.ToUpper()+"\n";
			text += "USER ID  : "+USERID+"\n";
			text += "PRINT ON : "+DateTime.Now.ToString("dd-MM-yyyy hh:mm tt")+"\n";
			if (isSamedate)
				text += "DAILTY SUMMARY ON " + printdate1.ToString ("yy-MM-yyyy") + "\n";
			else {
				text += "DAILTY SUMMARY ON " + printdate1.ToString ("yy-MM-yyyy")+" - "+ printdate2.ToString ("yy-MM-yyyy")+ "\n";
			}
			text += "------------------------------------------\n";
			text += "NO  INVOICE NO   TYPE     TAX AMT   AMOUNT\n";
			text += "------------------------------------------\n";
			Invoice[] invs = { };
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<Invoice> ()
					.Where(x=>x.invdate>=printdate1 && x.invdate<=printdate2)
					.OrderBy (x => x.invdate).ToList<Invoice> ();
				invs = new Invoice[list.Count];
				list.CopyTo (invs);
			}

			var grp= from inv in invs 
					  group inv by inv.invdate 
				      into g select new {key=g.Key,results=g};

			double ttltax = 0;
			double ttlamt = 0;
			double subttltax = 0;
			double subttlamt = 0;
			string line = "";
			int cont = 0;
			foreach (var g in grp) {
				var list = g.results.OrderBy (x => x.invno);
				subttltax = 0;
				subttlamt = 0;
				cont = 0;
				if (!isSamedate) {
					text = text + g.key.ToString ("dd-MM-yyyy") + "\n";
					text = text +"---------- \n";
				}
				foreach (Invoice inv in list) {
					cont += 1;
					ttltax += inv.taxamt;
					ttlamt += inv.amount;
					subttltax += inv.taxamt;
					subttlamt += inv.amount;
					line = (cont.ToString ()+".").PadRight (4, ' ')+
						   inv.invno.PadRight (13, ' ') + 
						   inv.trxtype.PadRight (8, ' ') + 
						   inv.taxamt.ToString ("n2").PadLeft (9, ' ') + 
						   inv.amount.ToString ("n2").PadLeft (8, ' ')+"\n";
					text = text + line;
				}
				if (!isSamedate) {
					text = text +PrintSubTotal (subttltax, subttlamt);
				}
			}

			text += "------------------------------------------\n";
			text += "TOTAL TAX    :" + ttltax.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += "TOTAL AMOUNT :" + ttlamt.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += "------------------------------------------\n\n\n\n";
			return text;
		}

		string PrintSubTotal(double ttltax,double ttlamt)
		{
			string text ="";
			text += "------------------------------------------\n";
			text += " SUB TOTAL TAX    :" + ttltax.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += " SUB TOTAL AMOUNT :" + ttlamt.ToString ("n2").PadLeft (13, ' ')+"\n";
			//text += "------------------------------------------\n";
			return text;				
		}
	}




}

