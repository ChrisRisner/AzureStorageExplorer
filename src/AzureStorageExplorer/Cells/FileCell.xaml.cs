using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class FileCell : ViewCell
	{
		readonly INavigation navigation;
		public FileCell(INavigation navigation = null)
		{
			View = new TableRowCellView();
			this.navigation = navigation;

		}
	}

	public partial class FileCellView : ContentView
	{
		public FileCellView()
		{
			InitializeComponent();
		}
	}
}
