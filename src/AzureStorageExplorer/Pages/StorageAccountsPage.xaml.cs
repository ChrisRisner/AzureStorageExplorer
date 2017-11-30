using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class StorageAccountsPage : ContentPage
	{
		StorageAccountsViewModel vm;

		public StorageAccountsPage()
		{
			InitializeComponent();
			BindingContext = vm = new StorageAccountsViewModel(Navigation);
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();


		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdatePage();
        }

		void UpdatePage(bool forceRefresh = false)
		{
			if (vm?.StorageAccounts.Count == 0 || forceRefresh || vm?.StorageAccounts[0].Realm.IsClosed == true)
			{
				vm?.LoadStorageAccountsCommand?.Execute(forceRefresh);
			}
		}

		void ListViewTapped(object sender, ItemTappedEventArgs e)
		{
			//Deselects row and prevents highlighting
			if (e == null) return;
			((ListView)sender).SelectedItem = null; 
		}
	}
}
