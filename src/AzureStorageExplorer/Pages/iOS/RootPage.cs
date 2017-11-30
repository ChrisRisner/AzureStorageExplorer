using System;
using Xamarin.Forms;
using FormsToolkit;

namespace AzureStorageExplorer
{
	public class RootPageiOS : TabbedPage
	{
		public RootPageiOS()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			Children.Add(new ASENavigationPage(new StorageAccountsPage()));
			Children.Add(new ASENavigationPage(new ContainersPage()));
            Children.Add(new ASENavigationPage(new TablesPage()));
            Children.Add(new ASENavigationPage(new QueuesPage()));
            Children.Add(new ASENavigationPage(new MorePage()));

			MessagingService.Current.Subscribe(MessageKeys.NavigateToSubscriptions,  m =>
			{
				Console.WriteLine("Navigate to subs");
				CurrentPage = Children[(int) AppPage.Subscriptions];
			});

			MessagingService.Current.Subscribe(MessageKeys.NavigateToStorageAccounts,  m =>
			{
				Console.WriteLine("Navigate to storage accounts");
				CurrentPage = Children[(int)AppPage.StorageAccounts];
			});
		}



		protected override void OnCurrentPageChanged()
		{
			base.OnCurrentPageChanged();
			switch (Children.IndexOf(CurrentPage))
			{
				case 0:
					App.Logger.TrackPage(AppPage.StorageAccounts.ToString());
					break;
				case 1:
					App.Logger.TrackPage(AppPage.Subscriptions.ToString());
					break;
				case 2:
					App.Logger.TrackPage(AppPage.Containers.ToString());
					break;
				case 3:
					App.Logger.TrackPage(AppPage.Tables.ToString());
					break;
				case 4:
					App.Logger.TrackPage(AppPage.MorePage.ToString());
					break;
			}
		}

		public void NavigateAsync(AppPage menuId)
		{
			switch ((int)menuId)
			{
				case (int)AppPage.StorageAccounts: CurrentPage = Children[0]; break;
				case (int)AppPage.Subscriptions: CurrentPage = Children[1]; break;
				case (int)AppPage.Containers: CurrentPage = Children[2]; break;
				case (int)AppPage.Tables: CurrentPage = Children[3]; break;
				case (int)AppPage.MorePage: CurrentPage = Children[4]; break;
			}
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
