using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MvvmHelpers;
using Realms;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class ContainersViewModel : ViewModelBase
	{
		public ObservableRangeCollection<ASECloudBlobContainer> Containers { get; } = new ObservableRangeCollection<ASECloudBlobContainer>();
		public ObservableRangeCollection<ASECloudBlobContainer> SortedContainers { get; } = new ObservableRangeCollection<ASECloudBlobContainer>();
		public ObservableRangeCollection<Grouping<string, ASECloudBlobContainer>> ContainersGrouped { get; } = new ObservableRangeCollection<Grouping<string, ASECloudBlobContainer>>();

		bool storageAccountsExist;
		public bool StorageAccountsExist
		{
			get { return storageAccountsExist; }
			set { SetProperty(ref storageAccountsExist, value); }
		}

		bool noContainersFound;
		public bool NoContainersFound
		{
			get { return noContainersFound; }
			set { SetProperty(ref noContainersFound, value); }
		}

		string noContainersFoundMessage;
		public string NoContainersFoundMessage
		{
			get { return noContainersFoundMessage; }
			set { SetProperty(ref noContainersFoundMessage, value); }
		}

		public ContainersViewModel(INavigation navigation, IUserDialogs userDialogs) : base(navigation, userDialogs)
		{
			storageAccountsExist = false;
			NoContainersFoundMessage = "No Containers Found";
		}

		public override void onAppearing()
		{
			base.onAppearing();
			MessagingService.Current.Unsubscribe<MessageArgsDeleteContainer>(MessageKeys.DeleteContainer);			
		}

		ICommand loadContainersCommand;
		public ICommand LoadContainersCommand =>
			loadContainersCommand ?? (loadContainersCommand = new Command<bool>(async (f) => await ExecuteLoadContainersAsync()));

		async Task<bool> ExecuteLoadContainersAsync(bool force = false)
		{
			if (IsBusy)
				return false;
			var realm = App.GetRealm();
			try
			{
				IsBusy = true;
				NoContainersFound = false;

				var cte = realm.All<RealmCloudBlobContainer>();
				if (cte.Count() > 0 && force == false)
				{
					var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);
					List<ASECloudBlobContainer> aseContainers = new List<ASECloudBlobContainer>();
					if (storageAccounts.Count() > 0)
					{
						foreach (var container in cte)
						{
							StorageAccountsExist = true;
							var storageAccount = storageAccounts.Where((arg) => arg.Name == container.StorageAccountName).FirstOrDefault();

							if (storageAccount != null)
							{
								var te = new CloudBlobContainer(new Uri(container.ContainerUri),
																new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccount.Name, storageAccount.PrimaryKey));
								aseContainers.Add(new ASECloudBlobContainer(te, storageAccount.Name));
							}

						}
						Containers.Clear();
						Containers.AddRange(aseContainers);
					}
				}
				else {
					var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);

					Containers.Clear();
					foreach (var account in storageAccounts)
					{
						string connectionString = Constants.StorageConnectionString;
						connectionString = connectionString.Replace("<ACCOUNTNAME>", account.Name);
						connectionString = connectionString.Replace("<ACCOUNTKEY>", account.PrimaryKey);						
						CloudStorageAccount sa = CloudStorageAccount.Parse(connectionString);
						var blobClient = sa.CreateCloudBlobClient();
                        var containers = await blobClient.ListContainersAsync();
                        List<ASECloudBlobContainer> aseContainers = new List<ASECloudBlobContainer>();
						for (int i = 0; i < containers.Count; i++)
							aseContainers.Add(new ASECloudBlobContainer(containers[i]));
						aseContainers.All(c => { c.StorageAccountName = account.Name; return true; });
						Containers.AddRange(aseContainers);
					}
					if (storageAccounts.Count() > 0)
						StorageAccountsExist = true;
					else
						StorageAccountsExist = false;

					await realm.WriteAsync(temprealm =>
					{
						temprealm.RemoveAll<RealmCloudBlobContainer>();
						foreach (var con in Containers)
							temprealm.Add(new RealmCloudBlobContainer(con.ContainerName, con.StorageAccountName, con.BaseContainer.Uri.ToString()));
					});
					realm.All<RealmCloudBlobContainer>().SubscribeForNotifications((sender, changes, error) =>
					{
						Console.WriteLine("Change to CloudBlobContainers");
					});
				}

				SortContainers();

				if (Containers.Count == 0)
				{					
					NoContainersFound = true;
				}
				else
				{
					NoContainersFound = false;
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadContainersAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
			return true;
		}

		public void AddContainer(ASECloudBlobContainer aseCloudBlobContainer)
		{
			Containers.Add(aseCloudBlobContainer);
			SortContainers();
		}

		private void SortContainers()
		{
			SortedContainers.AddRange(Containers.OrderBy(con => con.BaseContainer.Name));
			ContainersGrouped.ReplaceRange(Containers.FilterByStorageAccount());
			if (Containers.Count > 0)
				NoContainersFound = false;
			else
				NoContainersFound = true;
		}

		ICommand forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
		forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadContainersAsync(true);
		}

		ICommand itemTapCommand;
		public ICommand ItemTapCommand =>
		itemTapCommand ?? (itemTapCommand = new Command<ASECloudBlobContainer>(async (c) => ExecuteTapContainerCommandAsync(c)));
		async Task ExecuteTapContainerCommandAsync(ASECloudBlobContainer container)
		{
			if (container == null)
				return;			
            MessagingService.Current.Subscribe<MessageArgsDeleteContainer>(MessageKeys.DeleteContainer, async (m, argsDeleteContainer) =>
			{
				Console.WriteLine("Delete containerX: " + argsDeleteContainer.ContainerName);
				Navigation.PopAsync();
				IProgressDialog deletingDialog = UserDialogs.Loading("Deleting Container");
				deletingDialog.Show();
				try
				{
					var aseContainer = Containers.Where(c => c.ContainerName == argsDeleteContainer.ContainerName &&
														c.StorageAccountName == argsDeleteContainer.StorageAccountName).FirstOrDefault();
					if (aseContainer == null)
						return;

					await aseContainer.BaseContainer.DeleteAsync();

					App.Logger.Track(AppEvent.DeleteContainer.ToString());

					Containers.Remove(aseContainer);
					SortContainers();
					var realm = App.GetRealm();
					await realm.WriteAsync(temprealm =>
					{

						temprealm.Remove(temprealm.All<RealmCloudBlobContainer>()
										 .Where(c => c.ContainerName == argsDeleteContainer.ContainerName
												&& c.StorageAccountName == argsDeleteContainer.StorageAccountName).First());
					});
				}
				catch (Exception ex)
				{
					Logger.Report(ex, "Method", "HandleMessageArgsDeleteContainer");
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

			var blobsPage = new BlobsPage(container);
			App.Logger.TrackPage(AppPage.Blobs.ToString());
			await NavigationService.PushAsync(Navigation, blobsPage);
		}
	}
}
