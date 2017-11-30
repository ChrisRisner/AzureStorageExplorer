using System;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using FormsToolkit.Droid;
using Refractored.XamForms.PullToRefresh.Droid;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Acr.UserDialogs;
using Plugin.Permissions;
using Xamarin.Forms;

namespace AzureStorageExplorer.Droid
{
	[Activity(Label = "AzureStorageExplorer.Droid", 
	          Icon = "@drawable/ic_launcher", 
	          LaunchMode = LaunchMode.SingleTask,
	          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;
			base.OnCreate(bundle);
			global::Xamarin.Forms.Forms.Init(this, bundle);
			Toolkit.Init();
			PullToRefreshLayoutRenderer.Init();
			ImageCircle.Forms.Plugin.Droid.ImageCircleRenderer.Init();
			UserDialogs.Init(() => (Activity)Forms.Context);
			if (!string.IsNullOrWhiteSpace(ApiKeys.MobileCenterId) && ApiKeys.MobileCenterId != nameof(ApiKeys.MobileCenterId))
			{
				MobileCenter.Start(ApiKeys.MobileCenterId, typeof(Analytics), typeof(Crashes));
			}

			LoadApplication(new App());            			
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		private void CheckForUpdates()
		{
			
		}

		private void UnregisterManagers()
		{
			
		}

		protected override void OnPause()
		{
			base.OnPause();
			UnregisterManagers();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			UnregisterManagers();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
		}
	}
}

