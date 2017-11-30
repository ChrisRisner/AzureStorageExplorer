using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MvvmHelpers;
using Realms;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class TablesViewModel : ViewModelBase
	{
		public ObservableRangeCollection<ASECloudTable> Tables { get; } = new ObservableRangeCollection<ASECloudTable>();
		public ObservableRangeCollection<ASECloudTable> SortedTables { get; } = new ObservableRangeCollection<ASECloudTable>();
		public ObservableRangeCollection<Grouping<string, ASECloudTable>> TablesGrouped { get; } = new ObservableRangeCollection<Grouping<string, ASECloudTable>>();

		bool storageAccountsExist;
		public bool StorageAccountsExist
		{
			get { return storageAccountsExist; }
			set { SetProperty(ref storageAccountsExist, value); }
		}

		bool noTablesFound;
		public bool NoTablesFound
		{
			get { return noTablesFound; }
			set { SetProperty(ref noTablesFound, value); }
		}

		string noTablesFoundMessage;
		public string NoTablesFoundMessage
		{
			get { return noTablesFoundMessage; }
			set { SetProperty(ref noTablesFoundMessage, value); }
		}

		private ContentPage testPage = null;

		public TablesViewModel(INavigation navigation, IUserDialogs userDialogs, ContentPage contentPage) : base(navigation, userDialogs)
		{
			storageAccountsExist = false;
			NoTablesFoundMessage = "No Tables Found";
			testPage = contentPage;
		}

		public override void onAppearing()
		{
			base.onAppearing();
			MessagingService.Current.Unsubscribe<MessageArgsDeleteTable>(MessageKeys.DeleteTable);
		}

		ICommand loadTablesCommand;
		public ICommand LoadTablesCommand =>
			loadTablesCommand ?? (loadTablesCommand = new Command<bool>(async (f) => await ExecuteLoadTablesAsync()));

		async Task<bool> ExecuteLoadTablesAsync(bool force = false)
		{
			if (IsBusy)
				return false;
			var realm = App.GetRealm();
			try
			{
				IsBusy = true;
				NoTablesFound = false;


				var realmCloudTables = realm.All<RealmCloudTable>();
				if (realmCloudTables.Count() > 0 && force == false)
				{
					Console.WriteLine("Load from local tables");

					var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);
					List<ASECloudTable> aseTables = new List<ASECloudTable>();
					if (storageAccounts.Count() > 0)
					{
						foreach (var realmCloudTable in realmCloudTables)
						{
							StorageAccountsExist = true;
							var storageAccount = storageAccounts.Where((arg) => arg.Name == realmCloudTable.StorageAccountName).FirstOrDefault();

							if (storageAccount != null)
							{
								var te = new CloudTable(new Uri(realmCloudTable.TableUri),
																new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccount.Name, storageAccount.PrimaryKey));
								aseTables.Add(new ASECloudTable(te, storageAccount.Name));
							}
						}
						Tables.Clear();
						Tables.AddRange(aseTables);
					}
				}
				else
				{
					var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);

					Tables.Clear();
					foreach (var account in storageAccounts)
					{
						string connectionString = Constants.StorageConnectionString;
						connectionString = connectionString.Replace("<ACCOUNTNAME>", account.Name);
						connectionString = connectionString.Replace("<ACCOUNTKEY>", account.PrimaryKey);
						CloudStorageAccount sa = CloudStorageAccount.Parse(connectionString);
						var tableClient = sa.CreateCloudTableClient();
						var tables = await tableClient.ListTablesAsync();

						List<ASECloudTable> aseTables = new List<ASECloudTable>();
						for (int i = 0; i < tables.Count; i++)
							aseTables.Add(new ASECloudTable(tables[i]));

						aseTables.All(c => { c.StorageAccountName = account.Name; return true; });
						Tables.AddRange(aseTables);
					}
					if (storageAccounts.Count() > 0)
						StorageAccountsExist = true;
					else
						StorageAccountsExist = false;

					await realm.WriteAsync(temprealm =>
					{
						temprealm.RemoveAll<RealmCloudTable>();
						foreach (var table in Tables)
							temprealm.Add(new RealmCloudTable(table.TableName, table.StorageAccountName, table.BaseTable.Uri.ToString()));
					});
					realm.All<RealmCloudTable>().SubscribeForNotifications((sender, changes, error) =>
					{
						Console.WriteLine("Change to CloudTables");
					});
				}
                SortTables();
                if (Tables.Count == 0)
				{
					NoTablesFound = true;
				}
				else
				{
					NoTablesFound = false;
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadTablesAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
			return true;
		}

		public void AddTable(ASECloudTable aseCloudTable)
		{
			Tables.Add(aseCloudTable);
			SortTables();
		}

		private void SortTables()
		{
			SortedTables.AddRange(Tables.OrderBy(con => con.BaseTable.Name));
			TablesGrouped.ReplaceRange(Tables.FilterByStorageAccount());
			if (Tables.Count > 0)
				NoTablesFound = false;
			else
				NoTablesFound = true;
		}

		ICommand forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
		forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadTablesAsync(true);
		}

		ICommand itemTapCommand;
		public ICommand ItemTapCommand =>
		itemTapCommand ?? (itemTapCommand = new Command<ASECloudTable>(async (c) => ExecuteTapContainerCommandAsync(c)));
		async Task ExecuteTapContainerCommandAsync(ASECloudTable table)
		{
			if (table == null)
				return;

			MessagingService.Current.Subscribe<MessageArgsDeleteTable>(MessageKeys.DeleteTable, async (m, argsDeleteTable) =>
			{
				Navigation.PopAsync();
				IProgressDialog deletingDialog = UserDialogs.Loading("Deleting Table");
				deletingDialog.Show();
				try
				{                					
					var aseTable = Tables.Where(t => t.TableName == argsDeleteTable.TableName &&
					                            t.StorageAccountName == argsDeleteTable.StorageAccountName).FirstOrDefault();
					if (aseTable == null)
						return;
                    await aseTable.BaseTable.DeleteIfExistsAsync();
                    App.Logger.Track(AppEvent.DeleteTable.ToString());
                    Tables.Remove(aseTable);
					SortTables();
					var realm = App.GetRealm();
					await realm.WriteAsync(temprealm =>
					{
                        temprealm.Remove(temprealm.All<RealmCloudTable>()
						                 .Where(t => t.TableName == argsDeleteTable.TableName
						                        && t.StorageAccountName == argsDeleteTable.StorageAccountName).First());
					});
				}
				catch (Exception ex)
				{
					Logger.Report(ex, "Method", "HandleMessageArgsDeleteTable");
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

			var tableRowsPage = new TableRowsPage(table);
			App.Logger.TrackPage(AppPage.TableRows.ToString());
			await testPage.Navigation.PushAsync(tableRowsPage);
		}
	}
}
