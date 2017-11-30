using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using MvvmHelpers;
using Realms;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class StorageAccountsViewModel : ViewModelBase
	{
		ISignOnClient signOnClient;

		public ObservableRangeCollection<StorageAccountExt> StorageAccounts { get; } = new ObservableRangeCollection<StorageAccountExt>();
		public ObservableRangeCollection<Grouping<string, StorageAccountExt>> StorageAccountsGrouped { get; } = new ObservableRangeCollection<Grouping<string, StorageAccountExt>>();

		bool noStorageAccountsFound;
		public bool NoStorageAccountsFound
		{
			get { return noStorageAccountsFound; }
			set { SetProperty(ref noStorageAccountsFound, value); }
		}

		string noStorageAccountsFoundMessage;
		public string NoStorageAccountsFoundMessage
		{
			get { return noStorageAccountsFoundMessage; }
			set { SetProperty(ref noStorageAccountsFoundMessage, value); }
		}

		public StorageAccountsViewModel(INavigation navigation) : base(navigation)
		{
			signOnClient = DependencyService.Get<ISignOnClient>();
		}

		ICommand loadStorageAccountsCommand;
		public ICommand LoadStorageAccountsCommand =>
			loadStorageAccountsCommand ?? (loadStorageAccountsCommand = new Command<bool>(async (f) => await ExecuteLoadStorageAccountsAsync()));

		async Task<bool> ExecuteLoadStorageAccountsAsync(bool force = false)
		{
			if (IsBusy)
				return false;
            try
			{
				IsBusy = true;
				NoStorageAccountsFound = false;
                var realm = App.GetRealm();
				if (force)
				{
					var subs = realm.All<Subscription>().AsRealmCollection();
					foreach (var sub in subs.Where(sub => sub.IsSubOn == false))
					{
						realm.Write(() =>
						{
							var sas = realm.All<StorageAccountExt>().Where(sa => sa.SubscriptionName == sub.Name);
							realm.RemoveRange(sas);
						});
						     
					}
					foreach (var sub in subs.Where(sub => sub.IsSubOn))
					{
						await AzureManagementHelper.GetStorageAccountsForSubscription(
							sub.Id, signOnClient.GetTokenForTenant(sub.TenantId), sub.Name, sub.TenantId);
					}
				}


				var storageAccounts = realm.All<StorageAccountExt>();
				storageAccounts.SubscribeForNotifications((sender, changes, error) =>
				{
					Console.WriteLine("Change to StorageAccount");
				});

				StorageAccounts.ReplaceRange(storageAccounts.OrderBy(sa => sa.Name));
				StorageAccountsGrouped.ReplaceRange(StorageAccounts.FilterBySubscription());

				if (StorageAccounts.Count == 0)
				{
					NoStorageAccountsFoundMessage = "No Storage Accounts Found";
					NoStorageAccountsFound = true;
				}
				else
				{
					NoStorageAccountsFound = false;
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadStorageAccountsAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
			return true;
		}

		ICommand forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
		forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadStorageAccountsAsync(true);
		}
	}
}
