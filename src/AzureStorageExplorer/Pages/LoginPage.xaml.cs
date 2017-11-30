using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class LoginPage : ContentPage
	{
		LoginViewModel vm;
		ImageSource placeholder;

		public LoginPage()
		{
			InitializeComponent();
			BindingContext = vm = new LoginViewModel(Navigation);			
			CircleImageAvatar.Source = placeholder = ImageSource.FromFile("logo_storage.png");
		}

		protected override bool OnBackButtonPressed()
		{
			if (Settings.Current.FirstRun)
				return true;

			return base.OnBackButtonPressed();
		}
	}
}
