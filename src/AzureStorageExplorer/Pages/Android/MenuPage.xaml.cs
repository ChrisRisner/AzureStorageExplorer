﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class MenuPage : ContentPage
	{
		RootPageAndroid root;
		public MenuPage(RootPageAndroid root)
		{
			this.root = root;
			InitializeComponent();

			NavView.NavigationItemSelected += async (sender, e) =>
			{
				this.root.IsPresented = false;

				await Task.Delay(225);
				await this.root.NavigateAsync(e.Index);
			};
		}
	}
}
