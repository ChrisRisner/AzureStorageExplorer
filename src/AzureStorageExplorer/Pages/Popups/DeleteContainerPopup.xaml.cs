using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class DeleteContainerPopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		DeleteContainerViewModel ViewModel => vm ?? (vm = BindingContext as DeleteContainerViewModel);
		DeleteContainerViewModel vm;

		string containerName;
		string storageAccountName;


		public DeleteContainerPopup(string containerName, string storageAccountName)
		{
			InitializeComponent();
			this.containerName = containerName;
			this.storageAccountName = storageAccountName;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			BindingContext = vm = new DeleteContainerViewModel(Navigation, containerName, storageAccountName);
		}

		void clickedCancel(object sender, System.EventArgs e)
		{
			PopupNavigation.PopAsync();
		}
	}
}
