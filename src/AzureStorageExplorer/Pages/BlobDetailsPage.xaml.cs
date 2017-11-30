using System;
using System.Collections.Generic;
using System.IO;

#if __ANDROID__
using Android.Content;
using Android.Webkit;
#endif
using FormsToolkit;

using Microsoft.WindowsAzure.Storage.Blob;
using System.Linq;
using Xamarin.Forms.PlatformConfiguration;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;



#if __IOS__
using UIKit;
using CoreGraphics;
using QuickLook;
using MobileCoreServices;
using AzureStorageExplorer.iOS;
using Xamarin.Forms.Platform.iOS;
using Foundation;
#endif
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public partial class BlobDetailsPage : ContentPage
	{
		BlobDetailsViewModel ViewModel => vm ?? (vm = BindingContext as BlobDetailsViewModel);
		BlobDetailsViewModel vm;

		public BlobDetailsPage(CloudBlob blob)
		{
			InitializeComponent();
			BindingContext = new BlobDetailsViewModel(Navigation, blob);

			ViewModel.LoadBlobDetailsCommand.Execute(false);

			if (Device.RuntimePlatform == Device.Android)
			{
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = vm.HasBlobBeenSaved ? "Share Blob" :
													"Download Blob",
					Command = vm.HasBlobBeenSaved ? vm.ShareBlobCommand :
					            vm.DownloadBlobCommand
				});
				ToolbarItems.Add(new ToolbarItem
				{
					Order = ToolbarItemOrder.Secondary,
					Text = "Delete Blob",
					Command = vm.DeleteBlobCommand
				});

			}
			else if (Device.RuntimePlatform == Device.iOS)
			{
				ToolbarItems.Add(new ToolbarItem
				{
					Text = "More",
					Icon = "toolbar_overflow.png",
					Command = new Command(async () =>
						{
							string[] items = vm.HasBlobBeenSaved ?
											   new[] { "Share Blob", "Delete Blob" } :
												new[] { "Download Blob", "Delete Blob" };

							var action = await DisplayActionSheet("Options", "Cancel", null, items);
							if (action == items[0])
							{
								if (vm.HasBlobBeenSaved)
									vm.ShareBlobCommand.Execute(null);
								else
									vm.DownloadBlobCommand.Execute(null);
							}
							else if (action == items[1])
								vm.DeleteBlobCommand.Execute(null);


						})
				});
			}
		}

		protected override void OnAppearing()
		{			
			base.OnAppearing();
            			
			MessagingService.Current.Subscribe<MessageArgsBlobDownloaded>(MessageKeys.BlobDownloaded, (m, argsBlobDownloaded) =>
		   	{
                if (vm != null && Device.RuntimePlatform == Device.Android && vm.HasBlobBeenSaved)
			   {
				   ToolbarItems[0].Text = "Share Blob";
			   }					  
		   });
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			MessagingService.Current.Unsubscribe(MessageKeys.BlobDownloaded);
		}
	}
}