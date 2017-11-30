using System;
using AzureStorageExplorer.PCL;
using Realms;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class AzureStorageExplorerPage : ContentPage
	{
		public AzureStorageExplorerPage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			var realm = Realm.GetInstance();
			var accounts = realm.All<StorageAccountExt>();
			foreach (var acc in accounts)
			{
				Console.WriteLine("Account found: " + acc.Name);
			}
		}
	}
}

