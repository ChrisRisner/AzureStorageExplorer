using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class DeleteBlobPopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		DeleteBlobViewModel ViewModel => vm ?? (vm = BindingContext as DeleteBlobViewModel);
		DeleteBlobViewModel vm;

		public DeleteBlobPopup(CloudBlob blob)
		{
			InitializeComponent();
			BindingContext = vm = new DeleteBlobViewModel(Navigation, blob);
		}

		void clickedCancel(object sender, System.EventArgs e)
		{
			PopupNavigation.PopAsync();
		}
	}
}
