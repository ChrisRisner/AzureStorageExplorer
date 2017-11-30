using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage.Blob;
using MvvmHelpers;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Realms;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class BlobsViewModel : ViewModelBase
	{
		long byteCount = 0;

		public ObservableRangeCollection<CloudBlob> Blobs { get; } = new ObservableRangeCollection<CloudBlob>();
		public ObservableRangeCollection<CloudBlob> SortedBlobs { get; } = new ObservableRangeCollection<CloudBlob>();

		ASECloudBlobContainer container;
		public ASECloudBlobContainer Container
		{
			get { return container; }
			set { SetProperty(ref container, value); }
		}

		bool noBlobsFound;
		public bool NoBlobsFound
		{
			get { return noBlobsFound; }
			set { SetProperty(ref noBlobsFound, value); }
		}

		string noBlobsFoundMessage;
		public string NoBlobsFoundMessage
		{
			get { return noBlobsFoundMessage; }
			set { SetProperty(ref noBlobsFoundMessage, value); }
		}

		string blobCount;
		public string BlobCount
		{
			get { return blobCount; }
			set { SetProperty(ref blobCount, value); }
		}

		string totalBlobSize;
		public string TotalBlobSize
		{
			get { return totalBlobSize; }
			set { SetProperty(ref totalBlobSize, value); }
		}

		public BlobsViewModel(INavigation navigation, IUserDialogs userDialogs, ASECloudBlobContainer container) : base(navigation, userDialogs)
		{
			Container = container;
			NoBlobsFoundMessage = "No Blobs Found";
		}

		public override void onAppearing()
		{
			base.onAppearing();

			MessagingService.Current.Unsubscribe<MessageArgsDeleteBlob>(MessageKeys.DeleteBlob);		
		}

		ICommand loadBlobsCommand;
		public ICommand LoadBlobsCommand =>
			loadBlobsCommand ?? (loadBlobsCommand = new Command<bool>(async (f) => await ExecuteLoadBlobsAsync()));

		async Task<bool> ExecuteLoadBlobsAsync(bool force = false)
		{
			if (IsBusy)
				return false;

			try
			{
				IsBusy = true;
				NoBlobsFound = false;

				if (!Container.BaseContainer.Properties.PublicAccess.HasValue)
				{
					await container.BaseContainer.FetchAttributesAsync();
					OnPropertyChanged("Container");
				}

				Blobs.Clear();
				var blobs = await Container.BaseContainer.ListBlobsAsync();
				byteCount = 0;
				foreach (var blob in blobs)
				{
					if (blob is CloudBlob)
					{
						Blobs.Add((CloudBlob)blob);
						byteCount += Blobs.Last().Properties.Length;
					}
				}
				BlobCount = Blobs.Count.ToString();
				TotalBlobSize = FileSizeHelper.GetHumanizedSizeFromBytes(byteCount);
				SortBlobs();
                if (Blobs.Count == 0)
				{					
					NoBlobsFound = true;
				}
				else
				{
					NoBlobsFound = false;
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadBlobsAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
			return true;
		}

		private void SortBlobs()
		{
			SortedBlobs.ReplaceRange(Blobs.OrderBy(p => p.Name.ToString()));
			if (Blobs.Count > 0)
				NoBlobsFound = false;
			else
				NoBlobsFound = true;
		}

		ICommand forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
		forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadBlobsAsync(true);
		}

		ICommand itemTapCommand;
		public ICommand ItemTapCommand =>
		itemTapCommand ?? (itemTapCommand = new Command<CloudBlob>(async (c) => ExecuteTapBlobCommandAsync(c)));
		async Task ExecuteTapBlobCommandAsync(CloudBlob blob)
		{
			if (blob == null)
				return;

			MessagingService.Current.Subscribe<MessageArgsDeleteBlob>(MessageKeys.DeleteBlob, async (m, argsDeleteBlob) =>
			{
				Navigation.PopAsync();
				IProgressDialog deletingDialog = UserDialogs.Loading("Deleting Blob");
				deletingDialog.Show();
				try
				{
					var aseBlob = Blobs.Where(b => b.Name == argsDeleteBlob.BlobName &&
										   b.Container.Name == argsDeleteBlob.ContainerName).FirstOrDefault();
					if (aseBlob == null)
						return;
					await aseBlob.DeleteAsync();

					App.Logger.Track(AppEvent.DeleteBlob.ToString());

					Blobs.Remove(aseBlob);

					byteCount -= aseBlob.Properties.Length;
					BlobCount = Blobs.Count.ToString();
					TotalBlobSize = FileSizeHelper.GetHumanizedSizeFromBytes(byteCount);

					SortBlobs();                    					
				}
				catch (Exception ex)
				{
					Logger.Report(ex, "Method", "HandleMessageArgsDeleteBlob");
					MessagingService.Current.SendMessage(MessageKeys.Error, ex);
				}
				finally
				{
					if (deletingDialog != null)
					{
						if (deletingDialog.IsShowing)
							deletingDialog.Hide();
						deletingDialog.Dispose();
					}
				}

			});

			try
			{
				var blobDetailsPage = new BlobDetailsPage(blob);

				App.Logger.TrackPage(AppPage.BlobDetails.ToString());
				await NavigationService.PushAsync(Navigation, blobDetailsPage);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Ex: " + ex.Message);
			}
		}

		ICommand deleteContainerCommand;
		public ICommand DeleteContainerCommand =>
		deleteContainerCommand ?? (deleteContainerCommand = new Command(async () => await ExecuteDeleteContainerCommandAsync()));

		async Task ExecuteDeleteContainerCommandAsync()
		{
			if (IsBusy)
				return;
			
			App.Logger.TrackPage(AppPage.DeleteContainer.ToString());
			var page = new DeleteContainerPopup(Container.ContainerName, Container.StorageAccountName);
            await PopupNavigation.PushAsync(page);
		}

		ICommand addBlobCommand;	
		public ICommand AddBlobCommand =>
		addBlobCommand ?? (addBlobCommand = new Command(async () => await ExecuteAddBlobCommandAsync()));

		async Task ExecuteAddBlobCommandAsync()
		{
			if (IsBusy)
				return;

			IProgressDialog savingDialog = null;

			try
			{
				//Check for Storage Permissions
				var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
				if (status != PermissionStatus.Granted)
				{
					if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
					{
						MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
						{
							Title = "Need storage access",
							Message = "In order to upload new blobs, we need access to your phone's storage.",
							Cancel = "OK"
						});
					}

					var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage });
					status = results[Permission.Storage];
				}

				if (status == PermissionStatus.Granted)
				{
                    var result = await DependencyService.Get<IFilePicker>().PickFile();
					if (result != null)
					{
						savingDialog = UserDialogs.Loading("Saving Blob");
						savingDialog.Show();

						var blob = Container.BaseContainer.GetBlockBlobReference(result.FileName);
						if (await blob.ExistsAsync())
						{
							savingDialog.Hide();
							MessagingService.Current.SendMessage<MessagingServiceQuestion>(MessageKeys.Question, new MessagingServiceQuestion
							{
								Negative = "No",
								Positive = "Yes",
								Question = "A blob with the same name already exists in this Container, would you like to overwrite?",
								Title = "Overwrite blob?",
								OnCompleted = (async (questionResult) =>
								{
									if (questionResult)
									{
										try
										{
											savingDialog.Show();
											await blob.FetchAttributesAsync();
											byteCount -= blob.Properties.Length;
											await blob.UploadFromByteArrayAsync(result.DataArray, 0, result.DataArray.Length);
											byteCount += blob.Properties.Length;
											TotalBlobSize = FileSizeHelper.GetHumanizedSizeFromBytes(byteCount);
											App.Logger.Track(AppEvent.OverwriteBlob.ToString());
									}
										catch (Exception ex)
										{
											Logger.Report(ex);
											MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
											{
												Title = "Unable to Overwrite Blob",
												Message = "There was an issue trying to overwrite blob in storage.  Please try again.",
												Cancel = "OK"
											});
										}
										finally
										{
											if (savingDialog != null)
											{
												if (savingDialog.IsShowing)
													savingDialog.Hide();
												savingDialog.Dispose();
											}
										}
									}
								}
								  )
							});
						}
						else
						{
							await blob.UploadFromByteArrayAsync(result.DataArray, 0, result.DataArray.Length);

							App.Logger.Track(AppEvent.CreatedBlob.ToString());

							Blobs.Add((CloudBlob)blob);
							byteCount += blob.Properties.Length;

							BlobCount = Blobs.Count.ToString();
							TotalBlobSize = FileSizeHelper.GetHumanizedSizeFromBytes(byteCount);

							SortBlobs();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex);
				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
				{
					Title = "Unable to Save Blob",
					Message = "There was an issue trying to save to blob storage.  Please try again.",
					Cancel = "OK"
				});
			}
			finally
			{
				if (savingDialog != null)
				{
					if (savingDialog.IsShowing)
						savingDialog.Hide();
					savingDialog.Dispose();
				}
			}
		}
	}
}
