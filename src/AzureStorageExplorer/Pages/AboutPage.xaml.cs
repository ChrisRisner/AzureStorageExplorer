using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using AzureStorageExplorer.ViewModel;
using Xamarin.Forms;

namespace AzureStorageExplorer.Pages
{
    public partial class AboutPage : ContentPage
    {
		AboutViewModel ViewModel => vm ?? (vm = BindingContext as AboutViewModel);
		AboutViewModel vm;

        public AboutPage()
        {
            InitializeComponent();
            BindingContext = new AboutViewModel(Navigation, UserDialogs.Instance);
        }
    }
}
