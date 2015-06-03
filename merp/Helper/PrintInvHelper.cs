using System;
using System.Linq;
using Android.Bluetooth;
using Java.Util;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	public class PrintInvHelper
	{
		string pathToDatabase;
		string USERID;
		public PrintInvHelper (string adpathToDatabase,string userid)
		{
			pathToDatabase = adpathToDatabase;
			USERID = userid;
		}

		public string OpenBTAndPrint(BluetoothSocket mmSocket,BluetoothDevice mmDevice,Invoice inv,InvoiceDtls[] list,int noofcopy )
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
					byte[] charLarge = { 0x1d, 0x21, 0x10 };//this command for T9

					mmOutputStream.Write (charLarge, 0, charLarge.Length);
					byte[] charLarge3 = { 0x1b, 0x61, 0x01 };//this command for T9
					mmOutputStream.Write (charLarge3, 0, charLarge3.Length);
					//mmOutputStream.Write (bb, 0, bb.Length);
					byte[] charLarge4 = { 0x1b, 0x61, 0x00 };//this command for T9
					mmOutputStream.Write (charLarge4, 0, charLarge4.Length);
					byte[] charLarge1 = { 0x1d, 0x21, 0 };//this command for T9
					mmOutputStream.Write (charLarge1, 0, charLarge1.Length);
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
			test += DateTime.Now.ToString ("dd-MM-yyyy")+"TAX INVOICE".PadLeft(31,' ')+"\n";
			test += "RECPT NO : " + inv.invno+"\n";
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

	}
}

