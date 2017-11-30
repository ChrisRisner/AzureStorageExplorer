using System;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class ASENavigationPage : NavigationPage
	{
		private Page rootPage = null;

		public ASENavigationPage(Page root) : base(root)
		{
			Init();
			Title = root.Title;
			Icon = root.Icon;
			rootPage = root;
		}

		public ASENavigationPage()
		{
			Init();
		}

		public Page GetRootPage()
		{
			return rootPage;
		}

		void Init()
		{
            if (Device.RuntimePlatform == Device.iOS)
			{				
			}
			else
			{
				BarBackgroundColor = (Color)Application.Current.Resources["Primary"];
				BarTextColor = (Color)Application.Current.Resources["NavigationText"];
			}
		}
	}
}
