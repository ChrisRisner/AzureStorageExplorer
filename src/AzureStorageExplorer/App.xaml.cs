using System;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Xamarin.Forms;
using FormsToolkit;
using System.Collections.Generic;
using AzureStorageExplorer.PCL;
using Realms;
using Acr.UserDialogs;
#if __IOS__
using Rg.Plugins.Popup.IOS;
#endif

namespace AzureStorageExplorer
{
	public partial class App : Application
	{
		public static App current;

		private static Realm realm;

		public static Realm GetRealm()
		{
			if (realm == null || realm.IsClosed)
				realm = Realm.GetInstance();
			return realm;
		}

		public List<SubscriptionResponse.Subscription> Subscriptions = new List<SubscriptionResponse.Subscription>();

		public App()
		{
			current = this;

			InitializeComponent();
			ViewModelBase.Init();
            switch (Device.RuntimePlatform)
			{
				case Device.Android:
					MainPage = new RootPageAndroid();
					break;
				case Device.iOS:
					MainPage = new ASENavigationPage(new RootPageiOS());
					break;
				default:
					throw new NotImplementedException();
			}
		}

		static ILogger logger;
		public static ILogger Logger => logger ?? (logger = DependencyService.Get<ILogger>());

		bool firstRun = Settings.Current.FirstRun;

		protected override void OnStart()
		{
			// Handle when your app starts
			OnResume();
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
			CrossConnectivity.Current.ConnectivityChanged -= ConnectivityChanged;

			MessagingService.Current.Unsubscribe(MessageKeys.NavigateLogin);
			MessagingService.Current.Unsubscribe<MessagingServiceAlert>(MessageKeys.Message);
			MessagingService.Current.Unsubscribe<MessagingServiceQuestion>(MessageKeys.Question);

			if (realm != null)
				realm.Dispose();
		}


		protected override void OnResume()
		{
			// Handle when your app resumes
			Settings.Current.IsConnected = CrossConnectivity.Current.IsConnected;
			CrossConnectivity.Current.ConnectivityChanged += ConnectivityChanged;

			MessagingService.Current.Subscribe<MessagingServiceAlert>(MessageKeys.Message, async (m, info) =>
			{
				var task = Application.Current?.MainPage?.DisplayAlert(info.Title, info.Message, info.Cancel);
				if (task == null)
					return;
				await task;
				info?.OnCompleted?.Invoke();
			});

			MessagingService.Current.Subscribe<MessagingServiceQuestion>(MessageKeys.Question, async (m, q) =>
				{
					var task = Application.Current?.MainPage?.DisplayAlert(q.Title, q.Question, q.Positive, q.Negative);
					if (task == null)
						return;
					var result = await task;
					q?.OnCompleted?.Invoke(result);
				});

			MessagingService.Current.Subscribe(MessageKeys.NavigateLogin, async m =>
			{
				if (Device.RuntimePlatform == Device.Android)
				{
					((RootPageAndroid)MainPage).IsPresented = false;
				}
				Page page = null;

                if (Settings.Current.FirstRun && Device.RuntimePlatform == Device.Android)
					page = new ASENavigationPage(new LoginPage());
				else
					page = new ASENavigationPage(new LoginPage());

				var nav = Application.Current?.MainPage?.Navigation;
				if (nav == null)
					return;

				await NavigationService.PushModalAsync(nav, page);
			});

			try
			{
				
				if (firstRun)
				{
					DependencyService.Get<ISignOnClient>().LogoutAsync();
				}
                if (firstRun || Device.RuntimePlatform != Device.iOS)
					return;
                var mainNav = MainPage as NavigationPage;
				if (mainNav == null)
					return;
                var rootPage = mainNav.CurrentPage as RootPageiOS;
				if (rootPage == null)
					return;
                var rootNav = rootPage.CurrentPage as NavigationPage;
				if (rootNav == null)
					return;
			}
			catch
			{
			}
			finally
			{
				firstRun = false;
				Settings.Current.FirstRun = false;

#if __IOS__
			Popup.Init();
#endif
			}
		}

		protected async void ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
		{
			//save current state and then set it
			var connected = Settings.Current.IsConnected;
			Settings.Current.IsConnected = e.IsConnected;
			if (connected && !e.IsConnected)
			{
				//we went offline, should alert the user and also update ui (done via settings)
				var task = Application.Current?.MainPage?.DisplayAlert("Offline", "Uh Oh, It looks like you have gone offline. Please check your internet connection to get the latest data and enable syncing data.", "OK");
				if (task != null)
					await task;
			}
		}
	}
}

