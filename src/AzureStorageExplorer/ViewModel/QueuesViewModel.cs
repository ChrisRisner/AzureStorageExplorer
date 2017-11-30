using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MvvmHelpers;
using Realms;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class QueuesViewModel : ViewModelBase
	{
		public ObservableRangeCollection<ASECloudQueue> Queues { get; } = new ObservableRangeCollection<ASECloudQueue>();
		public ObservableRangeCollection<ASECloudQueue> SortedQueues { get; } = new ObservableRangeCollection<ASECloudQueue>();
		public ObservableRangeCollection<Grouping<string, ASECloudQueue>> QueuesGrouped { get; } = new ObservableRangeCollection<Grouping<string, ASECloudQueue>>();

		bool storageAccountsExist;
		public bool StorageAccountsExist
		{
			get { return storageAccountsExist; }
			set { SetProperty(ref storageAccountsExist, value); }
		}

		bool noQueuesFound;
		public bool NoQueuesFound
		{
			get { return noQueuesFound; }
			set { SetProperty(ref noQueuesFound, value); }
		}

		string noQueuesFoundMessage;
		public string NoQueuesFoundMessage
		{
			get { return noQueuesFoundMessage; }
			set { SetProperty(ref noQueuesFoundMessage, value); }
		}

		public QueuesViewModel(INavigation navigation, IUserDialogs userDialogs) : base(navigation, userDialogs)
		{
			storageAccountsExist = false;
			NoQueuesFoundMessage = "No Queues Found";
		}

		public override void onAppearing()
		{
			base.onAppearing();
			MessagingService.Current.Unsubscribe<MessageArgsDeleteContainer>(MessageKeys.DeleteQueue);
		}

		ICommand loadQueuesCommand;
		public ICommand LoadQueuesCommand =>
			loadQueuesCommand ?? (loadQueuesCommand = new Command<bool>(async (f) => await ExecuteLoadQueuesAsync()));

		async Task<bool> ExecuteLoadQueuesAsync(bool force = false)
		{
			if (IsBusy)
				return false;
			
			var realm = App.GetRealm();
			try
			{
				IsBusy = true;
				NoQueuesFound = false;


				var queues = realm.All<RealmCloudQueue>();
				if (queues.Count() > 0 && force == false)
				{
                    //Load from local containers
					var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);
					List<ASECloudQueue> aseQueues = new List<ASECloudQueue>();
					if (storageAccounts.Count() > 0)
					{
						foreach (var queue in queues)
						{
							StorageAccountsExist = true;
							var storageAccount = storageAccounts.Where((arg) => arg.Name == queue.StorageAccountName).FirstOrDefault();

							if (storageAccount != null)
							{
								var te = new CloudQueue(new Uri(queue.QueueUri),
																new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccount.Name, storageAccount.PrimaryKey));
								aseQueues.Add(new ASECloudQueue(te, storageAccount.Name));
							}
						}
						Queues.Clear();
						Queues.AddRange(aseQueues);

					}
				}
				else
				{
                    var storageAccounts = realm.All<StorageAccountExt>().Where(sa => sa.IsStorageAccountOn);
                    Queues.Clear();
					foreach (var account in storageAccounts)
					{
						string connectionString = Constants.StorageConnectionString;
						connectionString = connectionString.Replace("<ACCOUNTNAME>", account.Name);
						connectionString = connectionString.Replace("<ACCOUNTKEY>", account.PrimaryKey);
						Console.WriteLine("Connecting with: " + connectionString);
						CloudStorageAccount sa = CloudStorageAccount.Parse(connectionString);
						var queueClient = sa.CreateCloudQueueClient();
                        var fetchedQueues = await queueClient.ListQueuesAsync();
                        List<ASECloudQueue> aseQueues = new List<ASECloudQueue>();
						for (int i = 0; i < fetchedQueues.Count; i++)
							aseQueues.Add(new ASECloudQueue(fetchedQueues[i]));
						aseQueues.All(c => { c.StorageAccountName = account.Name; return true; });
						Queues.AddRange(aseQueues);
					}
					if (storageAccounts.Count() > 0)
						StorageAccountsExist = true;
					else
						StorageAccountsExist = false;


					await realm.WriteAsync(temprealm =>
					{
						temprealm.RemoveAll<RealmCloudQueue>();
					    foreach (var que in Queues)
						    temprealm.Add(new RealmCloudQueue(que.QueueName, que.StorageAccountName, que.BaseQueue.Uri.ToString()));
					});
					realm.All<RealmCloudQueue>().SubscribeForNotifications((sender, changes, error) =>
					{
						Console.WriteLine("Change to RealmCloudQueues");
					});
				}


				SortQueues();
				if (Queues.Count == 0)
				{					
					NoQueuesFound = true;
				}
				else
				{
					NoQueuesFound = false;
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadQueuesAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
			return true;
		}

		private void SortQueues()
		{
			SortedQueues.AddRange(Queues.OrderBy(con => con.BaseQueue.Name));
			QueuesGrouped.ReplaceRange(Queues.FilterByStorageAccount());
			if (Queues.Count > 0)
				NoQueuesFound = false;
			else
				NoQueuesFound = true;
		}

		public void AddQueue(ASECloudQueue aseCloudQueue)
		{
			Queues.Add(aseCloudQueue);
			SortQueues();
		}

		ICommand forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
		forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadQueuesAsync(true);
		}

		ICommand itemTapCommand;
		public ICommand ItemTapCommand =>
		itemTapCommand ?? (itemTapCommand = new Command<ASECloudQueue>(async (c) => ExecuteTapQueueCommandAsync(c)));

		async Task ExecuteTapQueueCommandAsync(ASECloudQueue queue)
		{
			if (queue == null)
				return;

			MessagingService.Current.Subscribe<MessageArgsDeleteQueue>(MessageKeys.DeleteQueue, async (m, argsDeleteQueue) =>
			{
				Navigation.PopAsync();
				IProgressDialog deletingDialog = UserDialogs.Loading("Deleting Queue");
				deletingDialog.Show();
				try
				{
					var aseQueue = Queues.Where(c => c.QueueName == argsDeleteQueue.QueueName &&
														c.StorageAccountName == argsDeleteQueue.StorageAccountName).FirstOrDefault();
					if (aseQueue == null)
						return;

					await aseQueue.BaseQueue.DeleteAsync();
					App.Logger.Track(AppEvent.DeleteQueue.ToString());

					Queues.Remove(aseQueue);
					SortQueues();
					var realm = App.GetRealm();
					await realm.WriteAsync(temprealm =>
					{

						temprealm.Remove(temprealm.All<RealmCloudQueue>()
						                 .Where(c => c.QueueName == argsDeleteQueue.QueueName
												&& c.StorageAccountName == argsDeleteQueue.StorageAccountName).First());
					});
				}
				catch (Exception ex)
				{
					Logger.Report(ex, "Method", "HandleMessageArgsDeleteQueue");
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
            var queueMessagesPage = new QueueMessagesPage(queue);
			App.Logger.TrackPage(AppPage.QueueMessages.ToString());
			await NavigationService.PushAsync(Navigation, queueMessagesPage);
		}
	}
}
