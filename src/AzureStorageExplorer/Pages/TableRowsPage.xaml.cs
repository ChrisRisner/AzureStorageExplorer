using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class TableRowsPage : ContentPage
	{
		TableRowsViewModel ViewModel => vm ?? (vm = BindingContext as TableRowsViewModel);
		TableRowsViewModel vm;

		public TableRowsPage(ASECloudTable table)
		{
			InitializeComponent();
			BindingContext = new TableRowsViewModel(Navigation, UserDialogs.Instance, table);

			Title = table.BaseTable.Name;
			ViewModel.LoadTableRowsCommand.Execute(false);

            if (Device.RuntimePlatform == Device.Android)
			{
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = "Add Row",
					Command = vm.AddTableRowCommand

				});
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = "Delete Table",
					Command = vm.DeleteTableCommand
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
							string[] items = new[] { "Add Row", "Delete Table" };

							var action = await DisplayActionSheet("Options", "Cancel", null, items);
							if (action == items[0])
								vm.AddTableRowCommand.Execute(null);
							else if (action == items[1])
								vm.DeleteTableCommand.Execute(null);
						})
				});
			}

			lvTableRows.ItemSelected += (sender, e) =>
			{
				lvTableRows.SelectedItem = null;
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
				vm?.LoadTableRowsCommand?.Execute(forceRefresh);
			}
		}
	}
}
