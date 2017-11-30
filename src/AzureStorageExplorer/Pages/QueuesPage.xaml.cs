using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class QueuesPage : ContentPage
	{
		QueuesViewModel ViewModel => vm ?? (vm = BindingContext as QueuesViewModel);
		QueuesViewModel vm;

		public QueuesPage()
		{
			InitializeComponent();
			BindingContext = vm = new QueuesViewModel(Navigation, UserDialogs.Instance);

            if (Device.RuntimePlatform != Device.iOS)
				tbiAddQueue.Icon = "toolbar_new.png";

			tbiAddQueue.Command = new Command(async () =>
				{
					if (vm.IsBusy)
						return;
					if (!vm.StorageAccountsExist)
						return;
					App.Logger.TrackPage(AppPage.NewQueue.ToString());
					var page = new NewQueuePopup(vm);
					await PopupNavigation.PushAsync(page);
				});

			lvQueues.ItemSelected += (sender, e) =>
				{					
					lvQueues.SelectedItem = null;
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
			if (vm?.Queues.Count == 0 || forceRefresh)
			{			
				vm?.LoadQueuesCommand?.Execute(forceRefresh);
			}
		}
	}
}
