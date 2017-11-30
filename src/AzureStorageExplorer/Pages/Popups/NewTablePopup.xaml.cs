using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class NewTablePopup : Rg.Plugins.Popup.Pages.PopupPage
	{
		NewTableViewModel ViewModel => vm ?? (vm = BindingContext as NewTableViewModel);
		NewTableViewModel vm;

		public NewTablePopup(TablesViewModel tablesVM)
		{
			InitializeComponent();
			BindingContext = vm = new NewTableViewModel(Navigation, UserDialogs.Instance, tablesVM);
		}

		void clickedCancel(object sender, System.EventArgs e)
		{
			PopupNavigation.PopAsync();
		}
	}
}
