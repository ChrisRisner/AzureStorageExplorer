using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AzureStorageExplorer.Interfaces;
using MvvmHelpers;
using Plugin.Share;
using Plugin.Share.Abstractions;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class ViewModelBase : BaseViewModel
	{
		protected INavigation Navigation { get; }
		protected IUserDialogs UserDialogs { get; }

		public ViewModelBase(INavigation navigation = null)
		{
			Navigation = navigation;
		}

		public ViewModelBase(INavigation navigation = null, IUserDialogs userDialogs = null)
		{
			Navigation = navigation;
			UserDialogs = userDialogs;
		}

		public static void Init(bool mock = true)
		{
#if ENABLE_TEST_CLOUD && !DEBUG

#else
			//if mock
			//DependencyService.Register<ISignOnClient, XamarinSignOnClient>();
#endif
		}

		protected ILogger Logger { get; } = DependencyService.Get<ILogger>();
		protected IToast Toast { get; } = DependencyService.Get<IToast>();
		protected ISignOnClient SignOnClient { get; } = DependencyService.Get<ISignOnClient>();

		public Settings Settings
		{
			get { return Settings.Current; }
		}

		public virtual void onAppearing() { }
		public virtual void onDisappearing() { }

		ICommand launchBrowserCommand;
		public ICommand LaunchBrowserCommand =>
		launchBrowserCommand ?? (launchBrowserCommand = new Command<string>(async (t) => await ExecuteLaunchBrowserAsync(t)));

		async Task ExecuteLaunchBrowserAsync(string arg)
		{
			if (IsBusy)
				return;

			if (!arg.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !arg.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
				arg = "http://" + arg;
            
            Logger.Track(ASELoggerKeys.LaunchedBrowser, "Url", arg);

			var lower = arg.ToLowerInvariant();
            if (Device.RuntimePlatform == Device.iOS && lower.Contains("twitter.com"))
			{
				try
				{
					var id = arg.Substring(lower.LastIndexOf("/", StringComparison.Ordinal) + 1);
					var launchTwitter = DependencyService.Get<ILaunchTwitter>();
					if (lower.Contains("/status/"))
					{
						if (launchTwitter.OpenStatus(id))
							return;
					}
					else
					{
						if (launchTwitter.OpenUserName(id))
							return;
					}
				}
				catch
				{
				}
			}

			try
			{
				await CrossShare.Current.OpenBrowser(arg, new BrowserOptions
				{
					ChromeShowTitle = true,
					ChromeToolbarColor = new ShareColor
					{
						A = 255,
						R = 118,
						G = 53,
						B = 235
					},
                    UseSafariReaderMode = true,
					UseSafariWebViewController = true
				});
			}
			catch
			{
			}
		}

		public string VersionNumber
		{
			get
			{
#if __ANDROID__
				return global::Android.App.Application.Context.PackageManager.GetPackageInfo(global::Android.App.Application.Context.PackageName, 0).VersionName.ToString();
#elif __IOS__

                return Foundation.NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
#else
                return "N/A"; 
#endif

			}
		}
	}
}
