using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class NewQueueViewModel : ViewModelBase
	{
		public ObservableRangeCollection<StorageAccountExt> StorageAccounts { get; } = new ObservableRangeCollection<StorageAccountExt>();

		private QueuesViewModel queuesVM = null;

		string queueName;
		public string QueueName
		{
			get { return queueName; }
			set
			{
				value = value.ToLower();
				SetProperty(ref queueName, value);
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
				if (!string.IsNullOrEmpty(QueueName))
					return true;
				return false;
			}
		}

		public NewQueueViewModel(INavigation navigation, IUserDialogs userDialogs, QueuesViewModel queuesVM) : base(navigation, userDialogs)
		{
			this.queuesVM = queuesVM;
			var realm = App.GetRealm();
			var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);
			StorageAccounts.ReplaceRange(storageAccounts);

		}

		ICommand createQueueCommand;
		public ICommand CreateQueueCommand =>
		createQueueCommand ?? (createQueueCommand = new Command(async () => await ExecuteCreateQueueAsync()));

		async Task ExecuteCreateQueueAsync()
		{
			if (IsBusy)
				return;

			if (!ReadyToSave)
			{
				//This should never happen as the save button should be disabled
				Logger.Report(new Exception("Create queue called when ReadyToSave was false"), "Method", "ExecuteCreateQueueAsync");
				return;
			}
            if (queueName.Length < 3 || queueName.Length > 63 ||
				!Regex.IsMatch(queueName, @"^[a-z0-9]+(-[a-z0-9]+)*$"))
			{
				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
				{
					Title = "Queue name is invalid",
					Message = "Queue names must be between 3 and 63 chars, only contain lowercase letters, numbers, and hyphens, must begin with a number or letter, must not contain consecutive hyphens, or end with a hyphen.",
					Cancel = "OK"
				});
				return;
			}


			IProgressDialog savingDialog = UserDialogs.Loading("Saving Queue");
			savingDialog.Show();
            try
			{
				IsBusy = true;
				string connectionString = Constants.StorageConnectionString;
				connectionString = connectionString.Replace("<ACCOUNTNAME>", SelectedStorageAccount.Name);
				connectionString = connectionString.Replace("<ACCOUNTKEY>", SelectedStorageAccount.PrimaryKey);

				CloudStorageAccount sa = CloudStorageAccount.Parse(connectionString);
				var queueClient = sa.CreateCloudQueueClient();

				CloudQueue queue = queueClient.GetQueueReference(QueueName);
				if (queue == null)
					Console.WriteLine("Queue is null");
				if (await queue.ExistsAsync())
				{
					MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
					{
						Title = "Queue Exists",
						Message = "A queue with the name \"" + QueueName + "\" already exists in this storage account.",
						Cancel = "OK"
					});
					return;
				}
				else
				{
					await queue.CreateAsync();
					var realm = App.GetRealm();
					var storageAccountName = selectedStorageAccount.Name;
                    realm.Write(() =>
					{
						realm.Add(new RealmCloudQueue(queue.Name,
													  selectedStorageAccount.Name,
													  queue.Uri.ToString()));
					});

					if (queuesVM != null)
					{
						queuesVM.AddQueue(new ASECloudQueue(queue, selectedStorageAccount.Name));
						App.Logger.Track(AppEvent.CreatedQueue.ToString());
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
				Logger.Report(ex, "Method", "ExecuteCreateQueueAsync");
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
