using System;
using System.Collections.Generic;
using FormsToolkit;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class SubscriptionsPage : ContentPage
	{
		SubscriptionsViewModel vm;

		public SubscriptionsPage()
		{
			InitializeComponent();
			BindingContext = vm = new SubscriptionsViewModel(Navigation);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdatePage();
		}

		void UpdatePage(bool forceRefresh = false)
		{
			if (vm?.Subscriptions.Count == 0 || forceRefresh)
			{
				vm?.LoadSubscriptionsCommand?.Execute(forceRefresh);
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
