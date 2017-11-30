using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class NewTableViewModel : ViewModelBase
	{
		public ObservableRangeCollection<StorageAccountExt> StorageAccounts { get; } = new ObservableRangeCollection<StorageAccountExt>();

		private TablesViewModel tablesVM = null;

		string tableName;
		public string TableName
		{
			get { return tableName; }
			set
			{
				SetProperty(ref tableName, value);
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
				if (!string.IsNullOrEmpty(TableName))
					return true;
				return false;
			}
		}

		public NewTableViewModel(INavigation navigation, IUserDialogs userDialogs, TablesViewModel tablesVM) : base(navigation, userDialogs)
		{
			this.tablesVM = tablesVM;
			var realm = App.GetRealm();
			var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);
			StorageAccounts.ReplaceRange(storageAccounts);

		}

		ICommand creatTableCommand;
		public ICommand CreateTableCommand =>
		creatTableCommand ?? (creatTableCommand = new Command(async () => await ExecuteCreateTableAsync()));

		async Task ExecuteCreateTableAsync()
		{
			if (IsBusy)
				return;

			if (!ReadyToSave)
			{
				Logger.Report(new Exception("Create table called when ReadyToSave was false"), "Method", "ExecuteCreateTableAsync");
				return;
			}
			if (tableName.Length < 3 || tableName.Length > 63 ||
				!Regex.IsMatch(tableName, @"^[A-Za-z][A-Za-z0-9]*$"))
			{
				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
				{
					Title = "Table name is invalid",
					Message = "Table names must be between 3 and 63 chars, only contain letters and numbers, and start with a letter.",
					Cancel = "OK"
				});
				return;
			}
            IProgressDialog savingDialog = UserDialogs.Loading("Saving Table");
			savingDialog.Show();
            try
			{
				IsBusy = true;
				string connectionString = Constants.StorageConnectionString;
				connectionString = connectionString.Replace("<ACCOUNTNAME>", SelectedStorageAccount.Name);
				connectionString = connectionString.Replace("<ACCOUNTKEY>", SelectedStorageAccount.PrimaryKey);
                CloudStorageAccount sa = CloudStorageAccount.Parse(connectionString);
				var tableClient = sa.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference(tableName);
                if (table == null)
					Console.WriteLine("Table is null");
				if (await table.ExistsAsync())
				{
					MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
					{
						Title = "Table Exists",
						Message = "A table with the name \"" + TableName + "\" already exists in this storage account.",
						Cancel = "OK"
					});
					return;
				}
				else
				{
					await table.CreateAsync();
                    var realm = App.GetRealm();
					var storageAccountName = selectedStorageAccount.Name;
                    realm.Write(() =>
					{
						realm.Add(new RealmCloudTable(table.Name,
													  selectedStorageAccount.Name,
													  table.Uri.ToString()));
					});
                    if (tablesVM != null)
					{
						tablesVM.AddTable(new ASECloudTable(table, selectedStorageAccount.Name));
						App.Logger.Track(AppEvent.CreatedTable.ToString());
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
				Logger.Report(ex, "Method", "ExecuteCreateTableAsync");
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
