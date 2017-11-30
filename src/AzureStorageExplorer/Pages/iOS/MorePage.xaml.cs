using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using AzureStorageExplorer.Pages;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class MorePage : ContentPage
	{
		MorePageViewModel ViewModel => vm ?? (vm = BindingContext as MorePageViewModel);
		MorePageViewModel vm;

		public MorePage()
		{
			InitializeComponent();
			BindingContext = vm = new MorePageViewModel(Navigation, UserDialogs.Instance);

			lvSettings.ItemSelected +=  async(sender, e) =>
			{
				var item = lvSettings.SelectedItem as MenuItem;
                    if(item == null)
                        return;
                    switch(item.Parameter)
                    {
	                    case "about":
						    App.Logger.TrackPage(AppPage.About.ToString());
						    var page = new AboutPage();
                            await NavigationService.PushAsync(Navigation, page);
	                        break;
                        case "logout":
						vm.LogoutCommand.Execute(null);
                            break;
                        case "deletelocalblobs":
							vm.DeleteLocalBlobsCommand.Execute(null);
                            break;                       
                    }

				lvSettings.SelectedItem = null; 
			};

			lvStorageFeatures.ItemSelected += async(sender, e) =>
                {
                    var item = lvStorageFeatures.SelectedItem as MenuItem;
                    if(item == null)
                        return;
                    Page page = null;
                    switch(item.Parameter)
                    {                        
                        case "files":
							App.Logger.TrackPage(AppPage.FileShares.ToString());
							page = new FileSharesPage();
                            break;
                        case "subscriptions":
                            App.Logger.TrackPage(AppPage.Subscriptions.ToString());
                            page = new SubscriptionsPage();
                            break;
                    }

                    if(page == null)
                        return;
                    await NavigationService.PushAsync(Navigation, page);

				lvStorageFeatures.SelectedItem = null; 
			};
		}
	}
}
