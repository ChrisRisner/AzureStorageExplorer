using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
#if __ANDROID__
using Android.Content;
#elif __IOS__
using Foundation;
using MobileCoreServices;
using AzureStorageExplorer.iOS;
using UIKit;
#endif
using FormsToolkit;

using Microsoft.WindowsAzure.Storage.Blob;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;


namespace AzureStorageExplorer
{
	public class BlobDetailsViewModel : ViewModelBase
	{
		CloudBlob blob;
		public CloudBlob Blob
		{
			get { return blob; }
			set { SetProperty(ref blob, value); }
		}

		public string LastModifiedLocalTime
		{
			get
			{
				if (Blob.Properties.LastModified.HasValue)
				{
					return Blob.Properties.LastModified.Value.ToLocalTime().ToString();
				}
				else return "Not set";
			}
		}

		public string BlobSize
		{
			get
			{
				var length = blob.Properties.Length;
				return FileSizeHelper.GetHumanizedSizeFromBytes(length);
			}
		}

		public BlobDetailsViewModel(INavigation navigation, CloudBlob blob) : base(navigation)
		{
			Blob = blob;
			BlobFileExtension = System.IO.Path.GetExtension(blob.Name);
			BlobFileType = MediaTypeMapper.GetFileTypeFromExtension(BlobFileExtension);
		}

		public bool HasBlobBeenSaved 
		{
			get
			{
				var filePath = GetBlobFilePath();
				if (File.Exists(filePath))
					return true;
				return false;
			}
		}

		public bool IsBlobSavedAndImage
		{
			get
			{
				if (HasBlobBeenSaved && BlobFileType == FileType.Image)
					return true;
				return false;
			}
		}	

		public ImageSource BlobImageSource
		{
			get
			{
				if (IsBlobSavedAndImage)
				{					
					ImageSource source = ImageSource.FromFile(GetBlobFilePath());
					return source;	
				}
				return null;
			}
		}

		private string BlobFileExtension;

		private FileType blobFileType;
		private FileType BlobFileType
		{
			get { return blobFileType; }
			set { blobFileType = value; }
		}



		private string GetContainerFilePath()
		{
			string containerName = Blob.Container.Name;
			string documentsPath = "";
#if __ANDROID__
			documentsPath = Android.OS.Environment.ExternalStorageDirectory.ToString();
#elif __IOS__
			documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#endif
			var filePath = documentsPath + "/AzureStorageExplorer/" + containerName;
			return filePath;
		}

		private string GetBlobFilePath()
		{
			string blobName = Blob.Name;
			return GetContainerFilePath() + "/" + blobName;
		}

		ICommand forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
		forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadBlobDetailsAsync(true);
		}

		ICommand loadBlobDetailsCommand;
		public ICommand LoadBlobDetailsCommand =>
			loadBlobDetailsCommand ?? (loadBlobDetailsCommand = new Command<bool>(async (f) => await ExecuteLoadBlobDetailsAsync()));

		async Task<bool> ExecuteLoadBlobDetailsAsync(bool force = false)
		{
			if (IsBusy)
				return false;

			if (Blob != null && !string.IsNullOrEmpty(Blob.Name) && force == false)
				return false;

			try
			{
				IsBusy = true;
				Blob = blob.Container.GetBlobReference(blob.Name);
				await Blob.FetchAttributesAsync();
				OnPropertyChanged("BlobSize");
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadBlobDetailsAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
			return true;
		}

		ICommand downloadBlobCommand;
		public ICommand DownloadBlobCommand =>
		downloadBlobCommand ?? (downloadBlobCommand = new Command(async () => await ExecuteDownloadBlobCommandAsync()));

		async Task ExecuteDownloadBlobCommandAsync()
		{
			if (IsBusy)
				return;			

			if (this.Blob.Properties.Length / 1024000.0 > Constants.MaxMbDownloadSize)
			{
				App.Logger.Track(AppEvent.BlobTooLargeForDownload.ToString(), "BlobSize", this.Blob.Properties.Length.ToString());
				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
				{
					Title = "The Blob is too Large",
					Message = "ASE currently has a size restriction for downloading blobs of " + Constants.MaxMbDownloadSize + " MB.",
					Cancel = "OK"
				});
				return;
			}
			
			App.Logger.Track(AppEvent.DownloadBlob.ToString());
			IsBusy = true;

			try
			{
				var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
				if (status != PermissionStatus.Granted)
				{
					if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
					{
						MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
						{
							Title = "Need storage access",
							Message = "In order to save Blobs, access to external storage is required.",
							Cancel = "OK"
						});
					}

					var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage });
					status = results[Permission.Storage];
				}

				if (status == PermissionStatus.Granted)
				{
#if __ANDROID__


					//ToDO: move this to blob download so we're not getting file access here
					//Create a directory for the Azure Storage Explorer files if it doesn't exist
					var filePath = Android.OS.Environment.ExternalStorageDirectory.ToString();
					filePath += "/" + "AzureStorageExplorer";
					if (!Directory.Exists(filePath))
						Directory.CreateDirectory(filePath);
					
#endif
				}
				else if (status != PermissionStatus.Unknown)
				{
					MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
					{
						Title = "Storage access denied",
						Message = "Unable to save to external storage.",
						Cancel = "OK"
					});
				}
			}
			catch (Exception ex)
			{
				App.Logger.Track(AppPage.BlobDetails.ToString(), "Error", ex.Message);
			}

            var containerPath = GetContainerFilePath();
			if (!Directory.Exists(containerPath))
				Directory.CreateDirectory(containerPath);

			var blobPath = GetBlobFilePath();

			if (!File.Exists(blobPath))
			{
				await Blob.DownloadToFileAsync(blobPath, FileMode.CreateNew);
				OnPropertyChanged("HasBlobBeenSaved");
				OnPropertyChanged("IsBlobSavedAndImage");
				OnPropertyChanged("BlobImageSource");
			}


			IsBusy = false;

			MessagingService.Current.SendMessage(MessageKeys.BlobDownloaded, new MessageArgsBlobDownloaded(blobPath));
			return;
		}

		ICommand deleteBlobCommand;
		public ICommand DeleteBlobCommand =>
		deleteBlobCommand ?? (deleteBlobCommand = new Command(async () => await ExecuteDeleteBlobCommandAsync()));

		async Task ExecuteDeleteBlobCommandAsync()
		{
			if (IsBusy)
				return;

			App.Logger.TrackPage(AppEvent.DeleteBlob.ToString());
			var page = new DeleteBlobPopup(Blob);
			await PopupNavigation.PushAsync(page);
		}

		ICommand shareBlobCommand;
		public ICommand ShareBlobCommand =>
		shareBlobCommand ?? (shareBlobCommand = new Command(async () => await ExecuteShareBlobCommandAsync()));

		async Task ExecuteShareBlobCommandAsync()
		{
			if (IsBusy)
				return;

			App.Logger.Track(AppEvent.ShareBlob.ToString());


			if (HasBlobBeenSaved)
			{
				try
				{
#if __ANDROID__

			   string mimeType = MediaTypeMapper.GetMimeType(BlobFileExtension);
			   Android.Net.Uri uri = Android.Net.Uri.Parse("file:///" + GetBlobFilePath());
			   Intent intent = new Intent(Intent.ActionView);
			   intent.SetDataAndType(uri, mimeType);
			   intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);
			   try
			   {
				   Xamarin.Forms.Forms.Context.StartActivity(intent);
			   }
			   catch (Exception ex)
			   {
                        MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
                        {
                            Title = "Unable to Share",
                            Message = "No applications available to open files of type " + BlobFileExtension,
                            Cancel = "OK"
                        });
                        throw ex;
			   }

#elif __IOS__


					var extensionClassRef = new NSString(UTType.TagClassFilenameExtension);
					var mimeTypeClassRef = new NSString(UTType.TagClassMIMEType);

					var uti = NativeTools.UTTypeCreatePreferredIdentifierForTag(extensionClassRef.Handle, new NSString(BlobFileExtension).Handle, IntPtr.Zero);					
					//TODO: open file based off of type
					var fi = NSData.FromFile(GetBlobFilePath());
					var item = NSObject.FromObject(fi);


					var activityItems = new[] { item };



					var activityController = new UIActivityViewController(activityItems, null);

					var topController = UIApplication.SharedApplication.KeyWindow.RootViewController;

					while (topController.PresentedViewController != null)
					{
						topController = topController.PresentedViewController;
					}

					topController.PresentViewController(activityController, true, () => { });

#endif
				}
				catch (Exception ex)
				{
					App.Logger.Report(ex, Severity.Error);
				}
			}
		}
	}
}
