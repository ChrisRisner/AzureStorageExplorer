using System;
using System.Collections.Generic;
using FormsToolkit;
using Realms;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class SettingsPage : ContentPage
	{
		SettingsViewModel vm;

		public SettingsPage()
		{
			InitializeComponent();
			BindingContext = vm = new SettingsViewModel(Navigation);

            ListViewAbout.ItemTapped += (sender, e) => ListViewAbout.SelectedItem = null;
            ListViewTechnology.ItemTapped += (sender, e) => ListViewTechnology.SelectedItem = null;
		}
	}
}
