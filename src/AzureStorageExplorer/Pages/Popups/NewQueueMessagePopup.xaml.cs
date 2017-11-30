using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class NewQueueMessagePopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		NewQueueMessageViewModel ViewModel => vm ?? (vm = BindingContext as NewQueueMessageViewModel);
		NewQueueMessageViewModel vm;

		public NewQueueMessagePopup(QueueMessagesViewModel queueMessagesVM)
		{
			InitializeComponent();
			BindingContext = vm = new NewQueueMessageViewModel(Navigation, UserDialogs.Instance, queueMessagesVM);
		}

		void clickedCancel(object sender, System.EventArgs e)
		{
			PopupNavigation.PopAsync();
		}
	}
}
