using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using AzureStorageExplorer.ViewModel;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;


namespace AzureStorageExplorer
{
	public partial class NewFileSharePopup : Rg.Plugins.Popup.Pages.PopupPage
	{
        NewFileShareViewModel ViewModel => vm ?? (vm = BindingContext as NewFileShareViewModel);
		NewFileShareViewModel vm;

		public NewFileSharePopup(FileSharesViewModel fileSharesVM)
		{
			InitializeComponent();
			BindingContext = vm = new NewFileShareViewModel(Navigation, UserDialogs.Instance, fileSharesVM);
		}

		void clickedCancel(object sender, System.EventArgs e)
		{
			PopupNavigation.PopAsync();
		}
	}
}
