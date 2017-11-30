using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class DeleteTablePopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		DeleteTableViewModel ViewModel => vm ?? (vm = BindingContext as DeleteTableViewModel);
		DeleteTableViewModel vm;

		public DeleteTablePopup(string tableName, string storageAccountName)
		{
			InitializeComponent();
			BindingContext = vm = new DeleteTableViewModel(Navigation, tableName, storageAccountName);
		}

		void clickedCancel(object sender, System.EventArgs e)
		{
			PopupNavigation.PopAsync();
		}
	}
}
