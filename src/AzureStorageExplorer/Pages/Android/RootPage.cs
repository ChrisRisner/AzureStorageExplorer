using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FormsToolkit;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class RootPageAndroid : MasterDetailPage
	{
		Dictionary<int, ASENavigationPage> pages;

		public RootPageAndroid()
		{
			pages = new Dictionary<int, ASENavigationPage>();
			Master = new MenuPage(this);

			pages.Add(0, new ASENavigationPage(new StorageAccountsPage()));

			Detail = pages[0];

			MessagingService.Current.Subscribe(MessageKeys.NavigateToSubscriptions, async m =>
		    {
				await NavigateAsync((int)AppPage.Subscriptions);
			});

			MessagingService.Current.Subscribe(MessageKeys.NavigateToStorageAccounts, async m =>
		    {
				await NavigateAsync((int)AppPage.StorageAccounts);
			});
		}

		public async Task NavigateAsync(int menuId)
		{
			ASENavigationPage newPage = null;
			if (!pages.ContainsKey(menuId))
			{
				//only cache specific pages
				switch (menuId)
				{
					case (int)AppPage.StorageAccounts: 
						pages.Add(menuId, new ASENavigationPage(new StorageAccountsPage()));
						break;
					case (int)AppPage.Subscriptions: 
						pages.Add(menuId, new ASENavigationPage(new SubscriptionsPage()));
						break;
					case (int)AppPage.Containers:
						pages.Add(menuId, new ASENavigationPage(new ContainersPage()));
						break;
					case (int)AppPage.Queues:
						pages.Add(menuId, new ASENavigationPage(new QueuesPage()));
						break;
					case (int)AppPage.Tables:
						newPage = new ASENavigationPage(new TablesPage());
						break;
					case (int)AppPage.FileShares:
						newPage = new ASENavigationPage(new FileSharesPage());
						break;
					case (int)AppPage.Settings: 
						newPage = new ASENavigationPage(new SettingsPage());
						break;					
				}
			}

			if (newPage == null)
				newPage = pages[menuId];

			if (newPage == null)
				return;

			//if we are on the same tab and pressed it again.
			if (Detail == newPage)
			{
				await newPage.Navigation.PopToRootAsync();
			}

			Detail = newPage;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			var signOnClient = DependencyService.Get<ISignOnClient>();
			if (!signOnClient.IsUserAuthenticated())
			{
				if (!await signOnClient.SilentlyRefreshUser())
					MessagingService.Current.SendMessage(MessageKeys.NavigateLogin);
			}
			if (Settings.Current.ForceUserLogin)
				MessagingService.Current.SendMessage(MessageKeys.NavigateLogin);
		}
	}
}
