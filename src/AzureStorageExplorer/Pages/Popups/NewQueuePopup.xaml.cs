using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class NewQueuePopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		NewQueueViewModel ViewModel => vm ?? (vm = BindingContext as NewQueueViewModel);
		NewQueueViewModel vm;

		public NewQueuePopup(QueuesViewModel queuesVM)
		{
			InitializeComponent();
			BindingContext = vm = new NewQueueViewModel(Navigation, UserDialogs.Instance, queuesVM);
		}

		void clickedCancel(object sender, System.EventArgs e)
		{
			PopupNavigation.PopAsync();
		}
	}
}
