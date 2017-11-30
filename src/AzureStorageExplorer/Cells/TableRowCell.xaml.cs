using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class TableRowCell : ViewCell
	{
		readonly INavigation navigation;
		public TableRowCell(INavigation navigation = null)
		{
			View = new TableRowCellView();
			this.navigation = navigation;

		}
	}

	public partial class TableRowCellView : ContentView
	{
		public TableRowCellView()
		{
			InitializeComponent();
		}
	}
}
