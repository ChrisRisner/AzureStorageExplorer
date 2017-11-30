using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class QueueCell : ViewCell
	{
		readonly INavigation navigation;
		public QueueCell(INavigation navigation = null)
		{
			View = new QueueCellView();
			this.navigation = navigation;

		}
	}

	public partial class QueueCellView : ContentView
	{
		public QueueCellView()
		{
			InitializeComponent();
		}
	}
}
