using System;
using Acr.UserDialogs;
using AzureStorageExplorer.PCL;
using MvvmHelpers;
using Xamarin.Forms;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Rg.Plugins.Popup.Services;

namespace AzureStorageExplorer.ViewModel
{
	public class NewFileShareViewModel : ViewModelBase
	{
		public ObservableRangeCollection<StorageAccountExt> StorageAccounts { get; } = new ObservableRangeCollection<StorageAccountExt>();

        private FileSharesViewModel fileSharesVM = null;

        string fileShareName;
		public string FileShareName
		{
            get { return fileShareName; }
			set
			{
				value = value.ToLower();
                SetProperty(ref fileShareName, value);
				OnPropertyChanged("ReadyToSave");
			}
		}

		StorageAccountExt selectedStorageAccount;
		public StorageAccountExt SelectedStorageAccount
		{
			get { return selectedStorageAccount; }
			set
			{
				SetProperty(ref selectedStorageAccount, value);
				OnPropertyChanged("ReadyToSave");
			}
		}


		public bool ReadyToSave
		{
			get
			{ 
                if (!string.IsNullOrEmpty(FileShareName))
					return true;
				return false;
			}
		}

        public NewFileShareViewModel(INavigation navigation, IUserDialogs userDialogs, FileSharesViewModel fileSharesVM) : base(navigation, userDialogs)
        {
            this.fileSharesVM = fileSharesVM;
			var realm = App.GetRealm();
			var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);
			StorageAccounts.ReplaceRange(storageAccounts);

		}

        ICommand createFileShareCommand;
        public ICommand CreateFileShareCommand =>
		createFileShareCommand ?? (createFileShareCommand = new Command(async () => await ExecuteCreateFileShareAsync()));

        async Task ExecuteCreateFileShareAsync()
		{
			if (IsBusy)
				return;

			if (!ReadyToSave)
			{
				//This should never happen as the save button should be disabled
				Logger.Report(new Exception("Create file share called when ReadyToSave was false"), "Method", "ExecuteCreateFileShareAsync");
				return;
			}

            if (fileShareName.Length < 3 || fileShareName.Length > 63 ||
				!Regex.IsMatch(fileShareName, @"^[a-z0-9]+(-[a-z0-9]+)*$"))
			{
				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
				{
					Title = "File Share name is invalid",
					Message = "File Share names must be between 3 and 63 chars, only contain lowercase letters, numbers, and hyphens, must begin with a number or letter, must not contain consecutive hyphens, or end with a hyphen.",
					Cancel = "OK"
				});
				return;
			}


			IProgressDialog savingDialog = UserDialogs.Loading("Saving File Share");
			savingDialog.Show();

            try
			{
				IsBusy = true;
				string connectionString = Constants.StorageConnectionString;
				connectionString = connectionString.Replace("<ACCOUNTNAME>", SelectedStorageAccount.Name);
				connectionString = connectionString.Replace("<ACCOUNTKEY>", SelectedStorageAccount.PrimaryKey);

				CloudStorageAccount sa = CloudStorageAccount.Parse(connectionString);
                var fileClient = sa.CreateCloudFileClient();

                CloudFileShare fileShare = fileClient.GetShareReference(FileShareName);
				if (fileShare == null)
					Console.WriteLine("File Share is null");
				if (await fileShare.ExistsAsync())
				{
					MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
					{
						Title = "File Share Exists",
						Message = "A file share with the name \"" + FileShareName + "\" already exists in this storage account.",
						Cancel = "OK"
					});
					return;
				}
				else
				{
                    await fileShare.CreateAsync();
					var realm = App.GetRealm();
					var storageAccountName = selectedStorageAccount.Name;
                    realm.Write(() =>
					{
                        realm.Add(new RealmCloudFileShare(fileShare.Name,
													  selectedStorageAccount.Name,
                                                      fileShare.Uri.ToString()));
					});

                    if (fileSharesVM != null)
					{
                        fileSharesVM.AddFileShare(new ASECloudFileShare(fileShare, selectedStorageAccount.Name));
                        App.Logger.Track(AppEvent.CreatedFileShare.ToString());
					}
					//This is here and in finally so we'll dismiss this before popping the page so the
					//Loader doesn't stay longer than the popup
					if (savingDialog != null)
					{
						if (savingDialog.IsShowing)
							savingDialog.Hide();
						savingDialog.Dispose();
					}
					await PopupNavigation.PopAsync();
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteCreateFileShareAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
				if (savingDialog != null)
				{
					if (savingDialog.IsShowing)
						savingDialog.Hide();
					savingDialog.Dispose();
				}
			}
			return;
		}

		ICommand cancelCommand;
		public ICommand CancelCommand =>
		cancelCommand ?? (cancelCommand = new Command(async () => await ExecuteCancelAsync()));

		async Task ExecuteCancelAsync()
		{
			await PopupNavigation.PopAsync();
		}
	}
}
