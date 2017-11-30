using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage.Blob;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;


namespace AzureStorageExplorer
{
	public partial class BlobsPage : ContentPage
	{
		BlobsViewModel ViewModel => vm ?? (vm = BindingContext as BlobsViewModel);
		BlobsViewModel vm;

		public BlobsPage(ASECloudBlobContainer container)
		{
			InitializeComponent();
			BindingContext = new BlobsViewModel(Navigation, UserDialogs.Instance, container);

			Title = container.BaseContainer.Name;
			ViewModel.LoadBlobsCommand.Execute(false);

			if (Device.RuntimePlatform == Device.Android)
			{
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = "Add Blob",
					Command = vm.AddBlobCommand

				});
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = "Delete Container",
					Command = vm.DeleteContainerCommand
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
							string[] items = new[] { "Add Blob", "Delete Container" };
							
							var action = await DisplayActionSheet("Options", "Cancel", null, items);
							if (action == items[0])
								vm.AddBlobCommand.Execute(null);
							else if (action == items[1])
							{
								vm.DeleteContainerCommand.Execute(null);

							}
						})
				});
			}

			lvBlobs.ItemSelected += (sender, e) =>
			{
				lvBlobs.SelectedItem = null;
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
				vm?.LoadBlobsCommand?.Execute(forceRefresh);
			}
		}
	}
}
