using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class TableCell : ViewCell
	{
		readonly INavigation navigation;
		public TableCell(INavigation navigation = null)
		{
			View = new TableCellView();
			this.navigation = navigation;

		}
	}

	public partial class TableCellView : ContentView
	{
		public TableCellView()
		{
			InitializeComponent();
		}
	}
}
