using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using MvvmHelpers;
using Realms;
using Xamarin.Forms;

namespace AzureStorageExplorer.ViewModel
{
    public class FileSharesViewModel : ViewModelBase
    {
        public ObservableRangeCollection<ASECloudFileShare> FileShares { get; } = new ObservableRangeCollection<ASECloudFileShare>();
		public ObservableRangeCollection<ASECloudFileShare> SortedFileShares { get; } = new ObservableRangeCollection<ASECloudFileShare>();
		public ObservableRangeCollection<Grouping<string, ASECloudFileShare>> FileSharesGrouped { get; } = new ObservableRangeCollection<Grouping<string, ASECloudFileShare>>();

		bool storageAccountsExist;
		public bool StorageAccountsExist
		{
			get { return storageAccountsExist; }
			set { SetProperty(ref storageAccountsExist, value); }
		}

        bool noFileSharesFound;
        public bool NoFileSharesFound
		{
			get { return noFileSharesFound; }
			set { SetProperty(ref noFileSharesFound, value); }
		}

        string noFileSharesFoundMessage;
        public string NoFileSharesFoundMessage
		{
			get { return noFileSharesFoundMessage; }
			set { SetProperty(ref noFileSharesFoundMessage, value); }
		}

        public FileSharesViewModel(INavigation navigation, IUserDialogs userDialogs) : base(navigation, userDialogs)
        {
			storageAccountsExist = false;
            NoFileSharesFoundMessage = "No Files Found";
		}

        ICommand loadFileSharesCommand;
        public ICommand LoadFileSharesCommand =>
			loadFileSharesCommand ?? (loadFileSharesCommand = new Command<bool>(async (f) => await ExecuteLoadFileSharesAsync()));

        async Task<bool> ExecuteLoadFileSharesAsync(bool force = false)
		{
			if (IsBusy)
				return false;
			
			var realm = App.GetRealm();
			try
			{
				IsBusy = true;
                NoFileSharesFound = false;


                var realmFileShares = realm.All<RealmCloudFileShare>();
				if (realmFileShares.Count() > 0 && force == false)
				{
					var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);
                    List<ASECloudFileShare> aseFileShares = new List<ASECloudFileShare>();
					if (storageAccounts.Count() > 0)
					{
                        foreach (var fShare in realmFileShares)
						{
							StorageAccountsExist = true;
							var storageAccount = storageAccounts.Where((arg) => arg.Name == fShare.StorageAccountName).FirstOrDefault();

							if (storageAccount != null)
							{
                                var te = new CloudFileShare(new Uri(fShare.FileShareUri),
																new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccount.Name, storageAccount.PrimaryKey));
                                aseFileShares.Add(new ASECloudFileShare(te, storageAccount.Name));
							}

						}
                        FileShares.Clear();
                        FileShares.AddRange(aseFileShares);
					}
				}
				else
				{

					var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);

					FileShares.Clear();
					foreach (var account in storageAccounts)
					{
						string connectionString = Constants.StorageConnectionString;
						connectionString = connectionString.Replace("<ACCOUNTNAME>", account.Name);
						connectionString = connectionString.Replace("<ACCOUNTKEY>", account.PrimaryKey);
						CloudStorageAccount sa = CloudStorageAccount.Parse(connectionString);
                        var fileClient = sa.CreateCloudFileClient();
                        var fileShares = await fileClient.ListFileSharesAsync();
                        List<ASECloudFileShare> aseFileShares = new List<ASECloudFileShare>();
                        for (int i = 0; i < fileShares.Count; i++)
                        aseFileShares.Add(new ASECloudFileShare(fileShares[i]));						
                        aseFileShares.All(c => { c.StorageAccountName = account.Name; return true; });
                        FileShares.AddRange(aseFileShares);
					}
					if (storageAccounts.Count() > 0)
						StorageAccountsExist = true;
					else
						StorageAccountsExist = false;
					await realm.WriteAsync(temprealm =>
					{
						temprealm.RemoveAll<RealmCloudFileShare>();
                    foreach (var fShare in FileShares)
                        temprealm.Add(new RealmCloudFileShare(fShare.FileShareName, fShare.StorageAccountName, fShare.BaseFileShare.Uri.ToString()));
					});
					realm.All<RealmCloudFileShare>().SubscribeForNotifications((sender, changes, error) =>
					{
						Console.WriteLine("Change to CloudFileShares");
					});
				}
                SortFileShares();
                if (FileShares.Count == 0)
				{
                    NoFileSharesFound = true;
				}
				else
				{
                    NoFileSharesFound = false;
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadFileSharesAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;          
			}
			return true;
		}

        public void AddFileShare(ASECloudFileShare aseCloudFileShare)
		{
            FileShares.Add(aseCloudFileShare);
			SortFileShares();
		}

        private void SortFileShares()
        {
            SortedFileShares.AddRange(FileShares.OrderBy(con => con.BaseFileShare.Name));
            FileSharesGrouped.ReplaceRange(FileShares.FilterByStorageAccount());
            if (FileShares.Count > 0)
                NoFileSharesFound = false;
            else
                NoFileSharesFound = true;
        }

		ICommand forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
		forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

		async Task ExecuteForceRefreshCommandAsync()
		{
            await ExecuteLoadFileSharesAsync(true);
		}

		ICommand itemTapCommand;
		public ICommand ItemTapCommand =>
        itemTapCommand ?? (itemTapCommand = new Command<ASECloudFileShare>(async (c) => ExecuteTapFileShareCommandAsync(c)));
		async Task ExecuteTapFileShareCommandAsync(ASECloudFileShare fileShare)
		{
			if (fileShare == null)
				return;
            

            MessagingService.Current.Subscribe<MessageArgsDeleteFileShare>(MessageKeys.DeleteFileShare, async (m, argsDeleteFileShare) =>
			{
				Navigation.PopAsync();
				IProgressDialog deletingDialog = UserDialogs.Loading("Deleting FileShare");
				deletingDialog.Show();
				try
				{
                    var aseFileShare = FileShares.Where(fs => fs.FileShareName == argsDeleteFileShare.FileShareName &&
                                                        fs.StorageAccountName == argsDeleteFileShare.StorageAccountName).FirstOrDefault();
					if (aseFileShare == null)
						return;

                    await aseFileShare.BaseFileShare.DeleteAsync();

                    App.Logger.Track(AppEvent.DeleteFileShare.ToString());

                    FileShares.Remove(aseFileShare);
                    SortFileShares();
					var realm = App.GetRealm();
					await realm.WriteAsync(temprealm =>
					{

                        temprealm.Remove(temprealm.All<RealmCloudFileShare>()
                                         .Where(fs => fs.FileShareName == argsDeleteFileShare.FileShareName
                                                && fs.StorageAccountName == argsDeleteFileShare.StorageAccountName).First());
					});
				}
				catch (Exception ex)
				{
					Logger.Report(ex, "Method", "HandleMessageArgsDeleteFileShare");
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

            var filesPage = new FilesPage(fileShare);
            App.Logger.TrackPage(AppPage.Files.ToString());
            await NavigationService.PushAsync(Navigation, filesPage);
			
		}
    }
}
