using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class DeleteQueuePopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		DeleteQueueViewModel ViewModel => vm ?? (vm = BindingContext as DeleteQueueViewModel);
		DeleteQueueViewModel vm;

		public DeleteQueuePopup(string queueName, string storageAccountName)
		{
			InitializeComponent();
			BindingContext = vm = new DeleteQueueViewModel(Navigation, queueName, storageAccountName);
		}

		void clickedCancel(object sender, System.EventArgs e)
		{
			PopupNavigation.PopAsync();
		}
	}
}
