using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Queue;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class QueueMessageDetailsPage : ContentPage
	{
		QueueMessageDetailsViewModel ViewModel => vm ?? (vm = BindingContext as QueueMessageDetailsViewModel);
		QueueMessageDetailsViewModel vm;

		public QueueMessageDetailsPage(CloudQueueMessage message, ASECloudQueue queue)
		{
			InitializeComponent();
			BindingContext = vm = new QueueMessageDetailsViewModel(Navigation, message, queue);

            if (Device.RuntimePlatform == Device.Android)
			{
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = "Delete Queue Message",
					Command = vm.DeleteQueueMessageCommand
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
							string[] items = new[] { "Delete Queue Message" };

							var action = await DisplayActionSheet("Options", "Cancel", null, items);
							if (action == items[0])
							vm.DeleteQueueMessageCommand.Execute(null);
                        })
				});
			}
		}
	}
}
