using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class QueueMessageCell : ViewCell
	{
		readonly INavigation navigation;
		public QueueMessageCell(INavigation navigation = null)
		{
			View = new QueueMessageCellView();
			this.navigation = navigation;

		}
	}

	public partial class QueueMessageCellView : ContentView
	{
		public QueueMessageCellView()
		{
			InitializeComponent();
		}
	}
}
