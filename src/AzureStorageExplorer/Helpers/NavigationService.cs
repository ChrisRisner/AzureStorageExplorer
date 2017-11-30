using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	/// <summary>
	/// Helper navigation service to use so we don't push multiple pages at the same time.
	/// </summary>
	public static class NavigationService
	{
		static bool navigating;
		/// <summary>
		/// PUsh a page async
		/// </summary>
		/// <returns>awaitable task.</returns>
		/// <param name="navigation">Navigation.</param>
		/// <param name="page">Page.</param>
		/// <param name="animate">If set to <c>true</c> animate.</param>
		public static async Task PushAsync(INavigation navigation, Page page, bool animate = true)
		{
			try
			{
				if (navigating)
					return;

				navigating = true;
				await navigation.PushAsync(page, animate);
				navigating = false;
			}
			catch (Exception ex)
			{
				App.Logger.Report(ex);
			}
		}

		/// <summary>
		/// Push a page modal async
		/// </summary>
		/// <returns>awaitable task.</returns>
		/// <param name="navigation">Navigation.</param>
		/// <param name="page">Page.</param>
		/// <param name="animate">If set to <c>true</c> animate.</param>
		public static async Task PushModalAsync(INavigation navigation, Page page, bool animate = true)
		{
			if (navigating)
				return;

			navigating = true;
			await navigation.PushModalAsync(page, animate);
			navigating = false;
		}

		public static async Task PushPopupAsync(INavigation navigation, PopupPage page, bool animate = true)
		{
			if (navigating)
				return;

			navigating = true;
			await navigation.PushPopupAsync(page, animate);
			navigating = false;
		}
	}
}
