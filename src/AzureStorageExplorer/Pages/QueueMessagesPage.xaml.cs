using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class QueueMessagesPage : ContentPage
	{
		QueueMessagesViewModel ViewModel => vm ?? (vm = BindingContext as QueueMessagesViewModel);
		QueueMessagesViewModel vm;

		public QueueMessagesPage(ASECloudQueue queue)
		{
			InitializeComponent();
			BindingContext = new QueueMessagesViewModel(Navigation, UserDialogs.Instance, queue);

			Title = queue.BaseQueue.Name;
			ViewModel.LoadQueueMessagesCommand.Execute(false);

            if (Device.RuntimePlatform == Device.Android)
			{
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = "Add Message",
					Command = vm.AddQueueMessageCommand

				});
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = "Delete Queue",
					Command = vm.DeleteQueueCommand
				});

			}
            else if (Device.RuntimePlatform == Device.iOS)
			{
				ToolbarItems.Add(new ToolbarItem
				{
					Text = "More",
					Icon = "toolbar_overflow.png",
					Command = new Command(async () =>
						{
							string[] items = new[] { "Add Message", "Delete Queue" };

							var action = await DisplayActionSheet("Options", "Cancel", null, items);
							if (action == items[0])
							vm.AddQueueMessageCommand.Execute(null);
							else if (action == items[1])
							vm.DeleteQueueCommand.Execute(null);


						})
				});
			}

			lvQueueMessages.ItemSelected += (sender, e) =>
			{
				lvQueueMessages.SelectedItem = null;
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
			if (forceRefresh)
			{
				vm?.LoadQueueMessagesCommand?.Execute(forceRefresh);
			}
		}
	}
}
