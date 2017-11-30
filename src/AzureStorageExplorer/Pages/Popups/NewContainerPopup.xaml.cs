using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class NewContainerPopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		NewContainerViewModel ViewModel => vm ?? (vm = BindingContext as NewContainerViewModel);
		NewContainerViewModel vm;

		public NewContainerPopup(ContainersViewModel containersVM)
		{
			InitializeComponent();
			BindingContext = vm = new NewContainerViewModel(Navigation, UserDialogs.Instance, containersVM);
		}

		void clickedCancel(object sender, System.EventArgs e)
		{
			PopupNavigation.PopAsync();
		}
	}
}
