using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	public abstract class InvoiceRepository	{

		readonly List<Invoice> invoices;

		public InvoiceRepository() {
			this.invoices = new List<Invoice>();
		}

		public List<Invoice> Orders {
			get { return invoices; }
		}
	}

}

