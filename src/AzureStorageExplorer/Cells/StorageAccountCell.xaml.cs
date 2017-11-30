using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class StorageAccountCell : ViewCell
	{
		readonly INavigation navigation;
		public StorageAccountCell(INavigation navigation = null)
		{
			View = new StorageAccountCellView();
			this.navigation = navigation;

		}
	}

	public partial class StorageAccountCellView : ContentView
	{
		public StorageAccountCellView()
		{
			InitializeComponent();
		}
	}
}
