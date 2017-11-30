using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
    public partial class FilesPage : ContentPage
	{
		FilesViewModel ViewModel => vm ?? (vm = BindingContext as FilesViewModel);
		FilesViewModel vm;

        public FilesPage(ASECloudFileShare fileShare)
		{
			InitializeComponent();
            BindingContext = new FilesViewModel(Navigation, UserDialogs.Instance, fileShare);

            Title = fileShare.FileShareName;
			ViewModel.LoadFilesCommand.Execute(false);

			if (Device.RuntimePlatform == Device.Android)
			{
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = "Add File",
					Command = vm.AddFileCommand

				});
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = "Delete File Share",
					Command = vm.DeleteFileShareCommand
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
						string[] items = new[] { "Add File", "Delete File Share" };

						var action = await DisplayActionSheet("Options", "Cancel", null, items);
						if (action == items[0])
							vm.AddFileCommand.Execute(null);
						else if (action == items[1])
							vm.DeleteFileShareCommand.Execute(null);
					})
				});
			}

			lvFiles.ItemSelected += (sender, e) =>
			{
				lvFiles.SelectedItem = null;
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
				vm?.LoadFilesCommand?.Execute(forceRefresh);
			}
		}
	}
}
