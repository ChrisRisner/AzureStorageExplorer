using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using AzureStorageExplorer.ViewModel;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage.Blob;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
    public partial class FileSharesPage : ContentPage
    {
        
        FileSharesViewModel vm;

        public FileSharesPage()
        {
            InitializeComponent();
            BindingContext = vm = new FileSharesViewModel(Navigation, UserDialogs.Instance);

            if (Device.RuntimePlatform != Device.iOS)
                tbiAddFileShare.Icon = "toolbar_new.png";

            tbiAddFileShare.Command = new Command(async () =>
                {
                    if (vm.IsBusy)
                        return;
                    if (!vm.StorageAccountsExist)
                        return;
                    App.Logger.TrackPage(AppPage.NewFileShare.ToString());
                    var page = new NewFileSharePopup(vm);
                    await PopupNavigation.PushAsync(page);
                });

            lvFileShares.ItemSelected += (sender, e) =>
                {                    
                    lvFileShares.SelectedItem = null;
                };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdatePage();
            vm.onAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            vm.onDisappearing();
        }

        void UpdatePage(bool forceRefresh = false)
        {
            if (vm?.FileShares.Count == 0 || forceRefresh)
            {
                vm?.LoadFileSharesCommand?.Execute(forceRefresh);
            }
        }
    }
}