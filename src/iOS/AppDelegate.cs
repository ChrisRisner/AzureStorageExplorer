using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using FormsToolkit.iOS;
using Foundation;
using Refractored.XamForms.PullToRefresh.iOS;
using Social;
using UIKit;
using Rg.Plugins.Popup.IOS;

namespace AzureStorageExplorer.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{

			var tint = UIColor.FromRGB(118, 53, 235);
			UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(250, 250, 250); //bar background
			UINavigationBar.Appearance.TintColor = tint; //Tint color of button items
			UIBarButtonItem.Appearance.TintColor = tint; //Tint color of button items
            UITabBar.Appearance.TintColor = tint;
            UISwitch.Appearance.OnTintColor = tint;
            UIAlertView.Appearance.TintColor = tint;
            UIView.AppearanceWhenContainedIn(typeof(UIAlertController)).TintColor = tint;
			UIView.AppearanceWhenContainedIn(typeof(UIActivityViewController)).TintColor = tint;
			UIView.AppearanceWhenContainedIn(typeof(SLComposeViewController)).TintColor = tint;

			//#if !ENABLE_TEST_CLOUD

			if (!string.IsNullOrWhiteSpace(ApiKeys.MobileCenterId) && ApiKeys.MobileCenterId != nameof(ApiKeys.MobileCenterId))
			{
				MobileCenter.Start(ApiKeys.MobileCenterId, typeof(Analytics), typeof(Crashes));
			}

			global::Xamarin.Forms.Forms.Init();
			Toolkit.Init();

			// Code for starting up the Xamarin Test Cloud Agent
#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
#endif
			SelectedTabPageRenderer.Initialize();
			PullToRefreshLayoutRenderer.Init();
			ImageCircle.Forms.Plugin.iOS.ImageCircleRenderer.Init();
			new FreshEssentials.iOS.AdvancedFrameRendereriOS();
            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
		}
	}
}

