using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using FormsToolkit;
using MvvmHelpers;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class MorePageViewModel : ViewModelBase
	{
		public ObservableRangeCollection<MenuItem> StorageFeatures { get; } = new ObservableRangeCollection<MenuItem>();
		public ObservableRangeCollection<MenuItem> SettingsItems { get; } = new ObservableRangeCollection<MenuItem>();

		ISignOnClient signOnClient;

		public MorePageViewModel(INavigation navigation, IUserDialogs userDialogs) : base(navigation, userDialogs)
		{

			StorageFeatures.AddRange(new[]
				{
					new MenuItem { Name = "Files", Icon = "tab_feed.png", Parameter="files"},
                    new MenuItem { Name = "Subscriptions", Icon = "tab_feed.png", Parameter="subscriptions"},

				});
			SettingsItems.AddRange(new[]
				{
                    new MenuItem { Name = "About", Icon = "tab_feed.png", Parameter="about"},
					new MenuItem { Name = "Log Out", Icon = "tab_feed.png", Parameter="logout"},
					new MenuItem { Name = "Delete Local Blobs", Icon = "tab_feed.png", Parameter="deletelocalblobs"},
				});
			signOnClient = DependencyService.Get<ISignOnClient>();
		}

		ICommand logoutCommand;
		public ICommand LogoutCommand =>
		logoutCommand ?? (logoutCommand = new Command(async () => await ExecuteLogoutAsync()));

		async Task ExecuteLogoutAsync()
		{
			Logger.Track(ASELoggerKeys.Logout);

			MessagingService.Current.SendMessage<MessagingServiceQuestion>(MessageKeys.Question, new MessagingServiceQuestion
			{
				Negative = "Cancel",
				Positive = "Yes",
				Question = "Are you sure you want to logout?",
				Title = "Logout",
				OnCompleted = (async (result) =>
					{
						if (!result)
							return;

						signOnClient.LogoutAsync();
			            var realm = App.GetRealm();

						await realm.WriteAsync(temprealm =>
						{
							temprealm.RemoveAll();
						});

						Settings.FirstName = string.Empty;
						Settings.LastName = string.Empty;
						Settings.Email = string.Empty; //this triggers login text changed!
        				Settings.Current.FirstRun = true;

        				//Remove external storage directory if Android
        				//Todo: Put this in another method possibly or change text on screen to say all data will be deleted
#if __ANDROID__
									var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
									if (status == PermissionStatus.Granted)
									{
										//ToDO: move this to blob download so we're not getting file access here
										//Create a directory for the Azure Storage Explorer files if it doesn't exist
										var filePath = Android.OS.Environment.ExternalStorageDirectory.ToString();
										filePath += "/" + "AzureStorageExplorer";
										if (Directory.Exists(filePath))
											Directory.Delete(filePath, true);
									}

#endif
				        MessagingService.Current.SendMessage(MessageKeys.NavigateLogin);
					})
			});


		}

		ICommand deleteLocalBlobsCommand;
		public ICommand DeleteLocalBlobsCommand =>
		deleteLocalBlobsCommand ?? (deleteLocalBlobsCommand = new Command(async () => await ExecuteDeleteLocalBlobsAsync()));

		async Task ExecuteDeleteLocalBlobsAsync()
		{

			MessagingService.Current.SendMessage<MessagingServiceQuestion>(MessageKeys.Question, new MessagingServiceQuestion
			{
				Negative = "No",
				Positive = "Yes",
				Question = "Are you sure you want to delete any blobs you've downloaded?",
				Title = "Delete Blobs",
				OnCompleted = (async (result) =>
					{
						if (!result)
							return;

#if __ANDROID__
							var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
							if (status == PermissionStatus.Granted)
							{
                                //ToDO: move this to blob download so we're not getting file access here
								//Create a directory for the Azure Storage Explorer files if it doesn't exist
								var filePath = Android.OS.Environment.ExternalStorageDirectory.ToString();
								filePath += "/" + "AzureStorageExplorer";
								if (Directory.Exists(filePath))
									Directory.Delete(filePath, true);
							}
							else
							{
								MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
								{
									Title = "Unable to access Storage",
									Message = "ASE cannot access your external storage to remove any saved blobs.",
									Cancel = "OK"
								});
							}
#elif __IOS__
				var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
						var list = Directory.GetDirectories(documentsPath);

						if (list.Length > 0)
						{
							for (int i = 0; i < list.Length; i++)
							{
								Directory.Delete(list[i], true);
							}
						}
#endif
			})
			});

		}
	}
}
