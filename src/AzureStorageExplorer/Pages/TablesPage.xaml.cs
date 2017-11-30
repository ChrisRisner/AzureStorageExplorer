using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class TablesPage : ContentPage
	{
		TablesViewModel ViewModel => vm ?? (vm = BindingContext as TablesViewModel);
		TablesViewModel vm;

		public TablesPage()
		{
			InitializeComponent();
			BindingContext = vm = new TablesViewModel(Navigation, UserDialogs.Instance, this);

            if (Device.RuntimePlatform != Device.iOS)
				tbiAddTable.Icon = "toolbar_new.png";

			tbiAddTable.Command = new Command(async () =>
				{
					if (vm.IsBusy)
						return;
					if (!vm.StorageAccountsExist)
						return;
					App.Logger.TrackPage(AppPage.NewTable.ToString());
					var page = new NewTablePopup(vm);
					await PopupNavigation.PushAsync(page);
				});


			lvTables.ItemSelected += (sender, e) =>
			{
				lvTables.SelectedItem = null;
			};
		}



		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdatePage();
			vm.onAppearing();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			vm.onDisappearing();
		}

		void UpdatePage(bool forceRefresh = false)
		{
			if (vm?.Tables.Count == 0 || forceRefresh)
			{
				vm?.LoadTablesCommand?.Execute(forceRefresh);
			}
		}
	}
}
