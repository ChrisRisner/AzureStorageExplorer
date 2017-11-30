using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Queue;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class DeleteQueueMessagePopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		DeleteQueueMessageViewModel ViewModel => vm ?? (vm = BindingContext as DeleteQueueMessageViewModel);
		DeleteQueueMessageViewModel vm;

		public DeleteQueueMessagePopup(CloudQueueMessage queueMessage)
		{
			InitializeComponent();
			BindingContext = vm = new DeleteQueueMessageViewModel(Navigation, queueMessage);
		}

		void clickedCancel(object sender, System.EventArgs e)
		{
			PopupNavigation.PopAsync();
		}
	}
}
