using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class SubscriptionCell : ViewCell
	{
		readonly INavigation navigation;
		public SubscriptionCell(INavigation navigation = null)
		{			
			View = new SubscriptionCellView();
			this.navigation = navigation;

		}
	}

	public partial class SubscriptionCellView : ContentView
	{
		public SubscriptionCellView()
		{
			InitializeComponent();
		}
	}
}
