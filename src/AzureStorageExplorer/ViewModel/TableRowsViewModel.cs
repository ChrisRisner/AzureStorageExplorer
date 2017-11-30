using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage.Table;
using MvvmHelpers;
using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;
using Rg.Plugins.Popup.Services;

namespace AzureStorageExplorer
{
	public class TableRowsViewModel : ViewModelBase
	{
		int recordsToLoadPerTime = 10;

		public ObservableRangeCollection<DynamicTableEntity> TableRows { get; } = new ObservableRangeCollection<DynamicTableEntity>();
		public ObservableRangeCollection<DynamicTableEntity> SortedTableRows { get; } = new ObservableRangeCollection<DynamicTableEntity>();

		ASECloudTable table;
		public ASECloudTable Table
		{
			get { return table; }
			set { SetProperty(ref table, value); }
		}

		bool noTableRowsFound;
		public bool NoTableRowsFound
		{
			get { return noTableRowsFound; }
			set { SetProperty(ref noTableRowsFound, value); }
		}

		string noTableRowsFoundMessage;
		public string NoTableRowsFoundMessage
		{
			get { return noTableRowsFoundMessage; }
			set { SetProperty(ref noTableRowsFoundMessage, value); }
		}

		int rowsLoadedCount;
		public int RowsLoadedCount
		{
			get { return rowsLoadedCount; }
			set { SetProperty(ref rowsLoadedCount, value); }
		}

		public TableRowsViewModel(INavigation navigation, IUserDialogs userDialogs, ASECloudTable table) : base(navigation, userDialogs)
		{
			Table = table;
			NoTableRowsFoundMessage = "No Queue Messages Found";
			RowsLoadedCount = 0;
		}

		public override void onAppearing()
		{
			base.onAppearing();
			MessagingService.Current.Unsubscribe<MessageArgsDeleteTableRow>(MessageKeys.DeleteTableRow);
		}


		ICommand loadTableRowsCommand;
		public ICommand LoadTableRowsCommand =>
			loadTableRowsCommand ?? (loadTableRowsCommand = new Command<bool>(async (f) => await ExecuteLoadTableRowsAsync()));

		async Task<bool> ExecuteLoadTableRowsAsync(bool force = false)
		{
			if (IsBusy)
				return false;

			try
			{
				IsBusy = true;
				NoTableRowsFound = false;

				if (force)
				{

				}
                TableRows.Clear();

				// Initialize a default TableQuery to retrieve all the entities in the table.
				TableQuery tableQuery = new TableQuery().Select(new string[] { "PartitionKey", "RowKey" });
				tableQuery.TakeCount = RowsLoadedCount > recordsToLoadPerTime ? RowsLoadedCount : recordsToLoadPerTime;
				var result = await Table.BaseTable.ExecuteQuerySegmentedAsync(tableQuery, null);
				TableRows.AddRange(result);
				RowsLoadedCount = result.Count();
				SortTableRows();
                if (SortedTableRows.Count == 0)
				{
					NoTableRowsFoundMessage = "No Rows Found";
					NoTableRowsFound = true;
				}
				else
				{
					NoTableRowsFound = false;
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadTableRowsAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
			return true;
		}

		ICommand loadMoreRowsCommand;
		public ICommand LoadMoreRowsCommand =>
		loadMoreRowsCommand ?? (loadMoreRowsCommand = new Command(async () => await ExecuteLoadMoreRowsAsync()));

		async Task<bool> ExecuteLoadMoreRowsAsync()
		{
			if (IsBusy)
				return false;

			try
			{
				IsBusy = true;
                // Initialize a default TableQuery to retrieve all the entities in the table.
				TableQuery tableQuery = new TableQuery().Select(new string[] { "PartitionKey", "RowKey" });
				tableQuery.TakeCount = RowsLoadedCount + recordsToLoadPerTime;
				var result = await Table.BaseTable.ExecuteQuerySegmentedAsync(tableQuery, null);
				TableRows.AddRange(result);
				if (RowsLoadedCount != result.Count())
					RowsLoadedCount = result.Count();
				else
				{
					Toast.SendToast("There are no more records to load.");
				}
                SortTableRows();

			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadMoreRowsAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
			return true;
		}

		private void SortTableRows()
		{
			SortedTableRows.ReplaceRange(TableRows.OrderBy(p => p.PartitionKey));

			if (SortedTableRows.Count > 0)
				NoTableRowsFound = false;
			else
				NoTableRowsFound = true;
		}

		ICommand forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
		forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadTableRowsAsync(true);
		}

		ICommand itemTapCommand;
		public ICommand ItemTapCommand =>
		itemTapCommand ?? (itemTapCommand = new Command<DynamicTableEntity>(async (c) => ExecuteTapTableRowCommandAsync(c)));

		async Task ExecuteTapTableRowCommandAsync(DynamicTableEntity tableEntity)
		{
			if (tableEntity == null)
				return;            
		}

		ICommand deleteTableCommand;
		public ICommand DeleteTableCommand =>
		deleteTableCommand ?? (deleteTableCommand = new Command(async () => await ExecuteDeleteTableCommandAsync()));

		async Task ExecuteDeleteTableCommandAsync()
		{
			if (IsBusy)
				return;

			App.Logger.TrackPage(AppPage.DeleteTable.ToString());
			var page = new DeleteTablePopup(Table.TableName, Table.StorageAccountName);
			await PopupNavigation.PushAsync(page);

		}

		ICommand addTableRowCommand;
		public ICommand AddTableRowCommand =>
		addTableRowCommand ?? (addTableRowCommand = new Command(async () => await ExecuteAddTableRowCommandAsync()));

		async Task ExecuteAddTableRowCommandAsync()
		{
			if (IsBusy)
				return;

			App.Logger.TrackPage(AppPage.NewTableRow.ToString());
		}

		public void AddTableRow(DynamicTableEntity tableEntity)
		{
			TableRows.Add(tableEntity);
			SortTableRows();
		}
	}
}
