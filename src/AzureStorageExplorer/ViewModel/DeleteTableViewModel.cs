using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FormsToolkit;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class DeleteTableViewModel : ViewModelBase
	{
		string tableName = null;
		string storageAccountName = null;

		public string TableName
		{
			get
			{
				return this.tableName;
			}
		}

		string tableNameConfirmation;
		public string TableNameConfirmation
		{
			get { return tableNameConfirmation; }
			set
			{
				value = value.ToLower();
				SetProperty(ref tableNameConfirmation, value);
				OnPropertyChanged("ReadyToDelete");
			}
		}

		public bool ReadyToDelete
		{
			get
			{
				if (TableNameConfirmation == tableName)
					return true;
				return false;
			}
		}

		public DeleteTableViewModel(INavigation navigation, string tableName, string storageAccountName) : base(navigation)
		{
			this.tableName = tableName;
			this.storageAccountName = storageAccountName;
		}


		ICommand deleteTableCommand;
		public ICommand DeleteTableCommand =>
		deleteTableCommand ?? (deleteTableCommand = new Command(async () => await ExecuteDeleteTableCommandlAsync()));

		async Task ExecuteDeleteTableCommandlAsync()
		{
			MessagingService.Current.SendMessage(MessageKeys.DeleteTable, new MessageArgsDeleteTable(tableName, storageAccountName));
			await PopupNavigation.PopAsync();
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
