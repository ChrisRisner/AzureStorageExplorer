using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MvvmHelpers;
using Realms;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class NewContainerViewModel : ViewModelBase
	{
		public ObservableRangeCollection<StorageAccountExt> StorageAccounts { get; } = new ObservableRangeCollection<StorageAccountExt>();
		public string[] AccessTypes { get; } = new string[] { "Private", "Blob", "Container" };

		private ContainersViewModel containersVM = null;

		string containerName;
		public string ContainerName
		{
			get { return containerName; }
			set
			{
				value = value.ToLower();
				SetProperty(ref containerName, value);
				OnPropertyChanged("ReadyToSave");
			}
		}

		StorageAccountExt selectedStorageAccount;
		public StorageAccountExt SelectedStorageAccount
		{
			get { return selectedStorageAccount; }
			set { 
				SetProperty(ref selectedStorageAccount, value);  
				OnPropertyChanged("ReadyToSave");
			}
		}

		string selectedAccessType;
		public string SelectedAccessType
		{
			get { return selectedAccessType; }
			set { 
				SetProperty(ref selectedAccessType, value); 
				OnPropertyChanged("ReadyToSave");
			}
		}

        public bool ReadyToSave
		{
			get {
				if (!string.IsNullOrEmpty(SelectedAccessType) && SelectedStorageAccount != null && !string.IsNullOrEmpty(ContainerName))
					return true;
				return false;			
			}
		}

		public NewContainerViewModel(INavigation navigation, IUserDialogs userDialogs, ContainersViewModel containersVM) : base(navigation, userDialogs)
		{
			this.containersVM = containersVM;
			var realm = App.GetRealm();
			var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);
			StorageAccounts.ReplaceRange(storageAccounts);
		}

		ICommand createContainerCommand;
		public ICommand CreateContainerCommand =>
		createContainerCommand ?? (createContainerCommand = new Command(async () => await ExecuteCreateContainerAsync()));

		async Task ExecuteCreateContainerAsync()
		{
			if (IsBusy)
				return;

			if (!ReadyToSave)
			{
				//This should never happen as the save button should be disabled
				Logger.Report(new Exception("Create container called when ReadyToSave was false"), "Method", "ExecuteCreateContainerAsync");
				return;
			}

			if (containerName.Length < 3 || containerName.Length > 63 ||
				!Regex.IsMatch(containerName, @"^[a-z0-9]+(-[a-z0-9]+)*$"))
			{
				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
				{
					Title = "Container name is invalid",
					Message = "Container names must be between 3 and 63 chars, only contain lowercase letters, numbers, and hyphens, must begin with a number or letter, must not contain consecutive hyphens, or end with a hyphen.",
					Cancel = "OK"
				});
				return;
			}

            IProgressDialog savingDialog = UserDialogs.Loading("Saving Container");
			savingDialog.Show();

            try
			{
				IsBusy = true;
				string connectionString = Constants.StorageConnectionString;
				connectionString = connectionString.Replace("<ACCOUNTNAME>", SelectedStorageAccount.Name);
				connectionString = connectionString.Replace("<ACCOUNTKEY>", SelectedStorageAccount.PrimaryKey);

				CloudStorageAccount sa = CloudStorageAccount.Parse(connectionString);
				var blobClient = sa.CreateCloudBlobClient();

				CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
				if (container == null)
					Console.WriteLine("Container is null");
				if (await container.ExistsAsync())
				{
					MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
					{
						Title = "Container Exists",
						Message = "A container with the name \"" + ContainerName + "\" already exists in this storage account.",
						Cancel = "OK"
					});
					return;
				}
				else
				{
					await container.CreateAsync();
					if (SelectedAccessType != "Private")
					{
						if (SelectedAccessType == "Container")
						{
							await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });
						}
						else if (SelectedAccessType == "Blob")
						{
							await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
						}
					}
					var realm = App.GetRealm();
					var storageAccountName = selectedStorageAccount.Name;					

					realm.Write(() =>
					{
						realm.Add(new RealmCloudBlobContainer(container.Name,
						                                      selectedStorageAccount.Name,
															  container.Uri.ToString()));
					});

					if (containersVM != null)
					{
						containersVM.AddContainer(new ASECloudBlobContainer(container, selectedStorageAccount.Name));
						App.Logger.Track(AppEvent.CreatedContainer.ToString());
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
				Logger.Report(ex, "Method", "ExecuteCreateContainerAsync");
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
