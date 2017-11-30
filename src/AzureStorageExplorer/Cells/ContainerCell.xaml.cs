using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class ContainerCell : ViewCell
	{
		readonly INavigation navigation;
		public ContainerCell(INavigation navigation = null)
		{
			View = new ContainerCellView();
			this.navigation = navigation;

		}
	}

	public partial class ContainerCellView : ContentView
	{
		public ContainerCellView()
		{
			InitializeComponent();
		}
	}
}
