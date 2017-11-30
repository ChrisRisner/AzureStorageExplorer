using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class BlobCell : ViewCell
	{
		readonly INavigation navigation;
		public BlobCell(INavigation navigation = null)
		{
			View = new BlobCellView();
			this.navigation = navigation;

		}
	}

	public partial class BlobCellView : ContentView
	{
		public BlobCellView()
		{
			InitializeComponent();
		}
	}
}
