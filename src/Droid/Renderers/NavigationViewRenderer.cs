using System;
using Android.Widget;
using AzureStorageExplorer.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Support.Design.Widget;
using Android.Runtime;
using FormsToolkit;
using Android.Views;


[assembly: ExportRenderer(typeof(AzureStorageExplorer.NavigationView), typeof(NavigationViewRenderer))]
namespace AzureStorageExplorer.Droid
{
	public class NavigationViewRenderer : ViewRenderer<AzureStorageExplorer.NavigationView, Android.Support.Design.Widget.NavigationView>
	{
		Android.Support.Design.Widget.NavigationView navView;
		ImageView profileImage;
		TextView profileName;
		protected override void OnElementChanged(ElementChangedEventArgs<AzureStorageExplorer.NavigationView> e)
		{

			base.OnElementChanged(e);
			if (e.OldElement != null || Element == null)
				return;
            var view = Inflate(Forms.Context, Resource.Layout.nav_view, null);
			navView = view.JavaCast<Android.Support.Design.Widget.NavigationView>();
            navView.NavigationItemSelected += NavView_NavigationItemSelected;
            Settings.Current.PropertyChanged += SettingsPropertyChanged;
			SetNativeControl(navView);
            var header = navView.GetHeaderView(0);
			profileImage = header.FindViewById<ImageView>(Resource.Id.profile_image);
			profileName = header.FindViewById<TextView>(Resource.Id.profile_name);
            profileImage.Click += (sender, e2) => NavigateToLogin();
			profileName.Click += (sender, e2) => NavigateToLogin();
            UpdateName();
			UpdateImage();
            navView.SetCheckedItem(Resource.Id.nav_storageaccounts);
            if (!Settings.Current.IsLoggedIn)
				navView.SetCheckedItem(Resource.Id.nav_subscriptions);
		}

		void NavigateToLogin()
		{
			if (Settings.Current.IsLoggedIn)
				return;

			AzureStorageExplorer.App.Logger.TrackPage(AppPage.Login.ToString(), "navigation");
		}

		void SettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Settings.Current.Email))
			{
				UpdateName();
				UpdateImage();
			}
		}

		void UpdateName()
		{
			profileName.Text = Settings.Current.UserDisplayName;
		}

		void UpdateImage()
		{
			Koush.UrlImageViewHelper.SetUrlDrawable(profileImage, Settings.Current.UserAvatar, Resource.Drawable.profile_generic);
		}

		public override void OnViewRemoved(Android.Views.View child)
		{
			base.OnViewRemoved(child);
			navView.NavigationItemSelected -= NavView_NavigationItemSelected;
			Settings.Current.PropertyChanged -= SettingsPropertyChanged;
		}

		IMenuItem previousItem;

		void NavView_NavigationItemSelected(object sender, Android.Support.Design.Widget.NavigationView.NavigationItemSelectedEventArgs e)
		{
            if (previousItem != null)
				previousItem.SetChecked(false);

			navView.SetCheckedItem(e.MenuItem.ItemId);

			previousItem = e.MenuItem;

			int id = 0;
			switch (e.MenuItem.ItemId)
			{
				case Resource.Id.nav_storageaccounts:
					id = (int)AppPage.StorageAccounts;
					App.Logger.TrackPage(AppPage.StorageAccounts.ToString());
					break;
				case Resource.Id.nav_subscriptions:
					id = (int)AppPage.Subscriptions;
					App.Logger.TrackPage(AppPage.Subscriptions.ToString());
					break;
				case Resource.Id.nav_containers:
					id = (int)AppPage.Containers;
					App.Logger.TrackPage(AppPage.Containers.ToString());
					break;
				case Resource.Id.nav_queues:
					id = (int)AppPage.Queues;
					App.Logger.TrackPage(AppPage.Queues.ToString());
					break;
				case Resource.Id.nav_tables:
					id = (int)AppPage.Tables;
					App.Logger.TrackPage(AppPage.Tables.ToString());
					break;
				case Resource.Id.nav_files:
					id = (int)AppPage.FileShares;
					App.Logger.TrackPage(AppPage.FileShares.ToString());
					break;
				case Resource.Id.nav_settings:
					id = (int)AppPage.Settings;
					App.Logger.TrackPage(AppPage.Settings.ToString());
					break;				
			}
			this.Element.OnNavigationItemSelected(new AzureStorageExplorer.NavigationItemSelectedEventArgs
			{

				Index = id
			});
		}
	}
}

