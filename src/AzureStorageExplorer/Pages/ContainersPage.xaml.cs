using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage.Blob;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class ContainersPage : ContentPage
	{
		ContainersViewModel vm;

		public ContainersPage()
		{
			InitializeComponent();
			BindingContext = vm = new ContainersViewModel(Navigation, UserDialogs.Instance);

            if (Device.RuntimePlatform != Device.iOS)
				tbiAddContainer.Icon = "toolbar_new.png";
			
			tbiAddContainer.Command = new Command(async () =>
				{
					if (vm.IsBusy)
						return;
					if (!vm.StorageAccountsExist)
						return;
				App.Logger.TrackPage(AppPage.NewContainer.ToString());
					var page = new NewContainerPopup(vm);
				await PopupNavigation.PushAsync(page);
				});


			lvContainers.ItemSelected +=  (sender, e) =>
				{				
					lvContainers.SelectedItem = null;
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
			if (vm?.Containers.Count == 0 || forceRefresh)
			{
				vm?.LoadContainersCommand?.Execute(forceRefresh);
			}
		}
	}
}
