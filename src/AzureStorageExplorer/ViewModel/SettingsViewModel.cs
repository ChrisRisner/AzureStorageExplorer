using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using AzureStorageExplorer.Pages;
using FormsToolkit;
using MvvmHelpers;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Realms;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class SettingsViewModel : ViewModelBase
	{
		public ObservableRangeCollection<MenuItem> AboutItems { get; } = new ObservableRangeCollection<MenuItem>();
		public ObservableRangeCollection<MenuItem> TechnologyItems { get; } = new ObservableRangeCollection<MenuItem>();

		ISignOnClient signOnClient;

		public SettingsViewModel(INavigation navigation) : base(navigation)
		{
			signOnClient = DependencyService.Get<ISignOnClient>();

			AboutItems.AddRange(new[]
				{
					new MenuItem { Name = "Created by Chris Risner", Command=LaunchBrowserCommand, Parameter="http://chrisrisner.com" },
                    new MenuItem { Name = "Version: " + VersionNumber},
				});

			TechnologyItems.AddRange(new[]
				{
					new MenuItem { Name = "ACR User Dialogs", Command=LaunchBrowserCommand, Parameter="https://github.com/aritchie/userdialogs" },
					new MenuItem { Name = "Fody", Command=LaunchBrowserCommand, Parameter="http://github.com/Fody/Fody"},
                    new MenuItem { Name = "Fresh Essentials", Command=LaunchBrowserCommand, Parameter="https://github.com/XAM-Consulting/FreshEssentials"},
					new MenuItem { Name = "Connectivity Plugin", Command=LaunchBrowserCommand, Parameter="https://github.com/jamesmontemagno/Xamarin.Plugins/tree/master/Connectivity"},
					new MenuItem { Name = "Current Activity Plugin", Command=LaunchBrowserCommand, Parameter="http://www.github.com/jamesmontemagno/Xamarin.Plugins"},
                    new MenuItem { Name = "Modern HTTP Client", Command=LaunchBrowserCommand, Parameter="https://github.com/paulcbetts/ModernHttpClient"},
					new MenuItem { Name = "Humanizer", Command=LaunchBrowserCommand, Parameter="https://github.com/Humanizr/Humanizer"},
				    new MenuItem { Name = "Image Circles", Command=LaunchBrowserCommand, Parameter="https://github.com/jamesmontemagno/Xamarin.Plugins/tree/master/ImageCircle"},    
                    new MenuItem { Name = "Json.NET", Command=LaunchBrowserCommand, Parameter="https://github.com/JamesNK/Newtonsoft.Json"},	
                    new MenuItem { Name = "Mvvm Helpers", Command=LaunchBrowserCommand, Parameter="https://github.com/jamesmontemagno/mvvm-helpers"},    
                    new MenuItem { Name = "Permissions Plugin", Command=LaunchBrowserCommand, Parameter="https://github.com/jamesmontemagno/Xamarin.Plugins/tree/master/Permissions"},
                    new MenuItem { Name = "Realm", Command=LaunchBrowserCommand, Parameter="http://github.com/realm/realm-dotnet"},
					new MenuItem { Name = "Share Plugin", Command=LaunchBrowserCommand, Parameter="https://github.com/jguertl/SharePlugin"},
					new MenuItem { Name = "Pull to Refresh Layout", Command=LaunchBrowserCommand, Parameter="https://github.com/jamesmontemagno/Xamarin.Forms-PullToRefreshLayout"},
					new MenuItem { Name = "Settings Plugin", Command=LaunchBrowserCommand, Parameter="https://github.com/jamesmontemagno/Xamarin.Plugins/tree/master/Settings"},
					new MenuItem { Name = "Toolkit for Xamarin.Forms", Command=LaunchBrowserCommand, Parameter="https://github.com/jamesmontemagno/xamarin.forms-toolkit"},
					new MenuItem { Name = "Xamarin.Forms", Command=LaunchBrowserCommand, Parameter="http://xamarin.com/forms"},
					new MenuItem { Name = "Visual Studio Mobile Center", Command=LaunchBrowserCommand, Parameter="https://www.visualstudio.com/vs/mobile-center/"}
					});
		}

		ICommand aboutCommand;
		public ICommand AboutCommand =>
		aboutCommand ?? (aboutCommand = new Command(async () => await ExecuteAboutAsync()));

		async Task ExecuteAboutAsync()
        {
			App.Logger.TrackPage(AppPage.About.ToString());
			var page = new AboutPage();
			await NavigationService.PushAsync(Navigation, page);
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

