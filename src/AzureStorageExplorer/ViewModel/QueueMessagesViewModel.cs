using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage.Queue;
using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class QueueMessagesViewModel : ViewModelBase
	{
		public ObservableRangeCollection<CloudQueueMessage> QueueMessages { get; } = new ObservableRangeCollection<CloudQueueMessage>();
		public ObservableRangeCollection<CloudQueueMessage> SortedQueueMessages { get; } = new ObservableRangeCollection<CloudQueueMessage>();

		ASECloudQueue queue;
		public ASECloudQueue Queue
		{
			get { return queue; }
			set { SetProperty(ref queue, value); }
		}

		bool noQueueMessagesFound;
		public bool NoQueueMessagesFound
		{
			get { return noQueueMessagesFound; }
			set { SetProperty(ref noQueueMessagesFound, value); }
		}

		string noQueueMessagesFoundMessage;
		public string NoQueueMessagesFoundMessage
		{
			get { return noQueueMessagesFoundMessage; }
			set { SetProperty(ref noQueueMessagesFoundMessage, value); }
		}

		int queueMessageCount;
		public int QueueMessageCount
		{
			get { return queueMessageCount; }
			set { SetProperty(ref queueMessageCount, value); }
		}

		public QueueMessagesViewModel(INavigation navigation, IUserDialogs userDialogs, ASECloudQueue queue) : base(navigation, userDialogs)
		{
			Queue = queue;
			NoQueueMessagesFoundMessage = "No Queue Messages Found";
		}

		public override void onAppearing()
		{
			base.onAppearing();
			MessagingService.Current.Unsubscribe<MessageArgsDeleteQueueMessage>(MessageKeys.DeleteQueueMessage);
		}


		ICommand loadQueueMessagesCommand;
		public ICommand LoadQueueMessagesCommand =>
			loadQueueMessagesCommand ?? (loadQueueMessagesCommand = new Command<bool>(async (f) => await ExecuteLoadQueueMessagesAsync()));

		async Task<bool> ExecuteLoadQueueMessagesAsync(bool force = false)
		{
			if (IsBusy)
				return false;

			try
			{
				IsBusy = true;
				NoQueueMessagesFound = false;

				if (!Queue.BaseQueue.ApproximateMessageCount.HasValue || force)
				{
					await queue.BaseQueue.FetchAttributesAsync();
					OnPropertyChanged("Queue");
				}
				if (Queue.BaseQueue.ApproximateMessageCount.Value == 0)
				{
					NoQueueMessagesFound = true;
					return true;
				}


				QueueMessages.Clear();

				var queueMessages = await Queue.BaseQueue.GetMessagesAsync(Queue.BaseQueue.ApproximateMessageCount.Value, 
				                                                           TimeSpan.FromSeconds(Settings.Current.QueueVisibilityTimeoutSeconds), null, null);
				QueueMessageCount = Queue.BaseQueue.ApproximateMessageCount.Value;
				foreach (var queueMessage in queueMessages)
				{					
					QueueMessages.Add(queueMessage);
				}
				SortQueueMessages();
                if (QueueMessages.Count == 0)
				{
					NoQueueMessagesFoundMessage = "No Queue Messages Found";
					NoQueueMessagesFound = true;
				}
				else
				{
					NoQueueMessagesFound = false;
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadQueueMessagesAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
			return true;
		}

		private void SortQueueMessages()
		{
			SortedQueueMessages.ReplaceRange(QueueMessages.OrderBy(p => p.InsertionTime));

			if (SortedQueueMessages.Count > 0)
				NoQueueMessagesFound = false;
			else
				NoQueueMessagesFound = true;
		}

		ICommand forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
		forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadQueueMessagesAsync(true);
		}

		ICommand itemTapCommand;
		public ICommand ItemTapCommand =>
		itemTapCommand ?? (itemTapCommand = new Command<CloudQueueMessage>(async (c) => ExecuteTapQueueMessageCommandAsync(c)));

		async Task ExecuteTapQueueMessageCommandAsync(CloudQueueMessage queueMessage)
		{
			if (queueMessage == null)
				return;

			MessagingService.Current.Subscribe<MessageArgsDeleteQueueMessage>(MessageKeys.DeleteQueueMessage, async (m, argsDeleteQueueMessage) =>
			{
				Navigation.PopAsync();
				IProgressDialog deletingDialog = UserDialogs.Loading("Deleting Queue Message");
				deletingDialog.Show();
				try
				{
					var message = QueueMessages.Where(qm => qm.Id == argsDeleteQueueMessage.QueueId).FirstOrDefault();

					if (message == null)
						return;
					
					await Queue.BaseQueue.DeleteMessageAsync(message);
					App.Logger.Track(AppEvent.DeleteQueueMessage.ToString());

					QueueMessages.Remove(message);
					QueueMessageCount--;


					SortQueueMessages();
				}
				catch (Exception ex)
				{
					Logger.Report(ex, "Method", "HandleMessageArgsDeleteQueueMessage");
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

			try
			{
				var queueMessageDetailsPage = new QueueMessageDetailsPage(queueMessage, queue);

				App.Logger.TrackPage(AppPage.QueueMessageDetails.ToString());
				await NavigationService.PushAsync(Navigation, queueMessageDetailsPage);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Ex: " + ex.Message);
			}
		}

		ICommand deleteQueueCommand;
		public ICommand DeleteQueueCommand =>
		deleteQueueCommand ?? (deleteQueueCommand = new Command(async () => await ExecuteDeleteQueueCommandAsync()));

		async Task ExecuteDeleteQueueCommandAsync()
		{
			if (IsBusy)
				return;

			App.Logger.TrackPage(AppPage.DeleteQueue.ToString());
			var page = new DeleteQueuePopup(Queue.QueueName, Queue.StorageAccountName);
			await PopupNavigation.PushAsync(page);

		}

		ICommand addQueueMessageCommand;
		public ICommand AddQueueMessageCommand =>
		addQueueMessageCommand ?? (addQueueMessageCommand = new Command(async () => await ExecuteAddQueueMessageCommandAsync()));

		async Task ExecuteAddQueueMessageCommandAsync()
		{
			if (IsBusy)
				return;

			App.Logger.TrackPage(AppPage.NewQueueMessage.ToString());

			var page = new NewQueueMessagePopup(this);
			await PopupNavigation.PushAsync(page);

		}

		public void AddQueueMessage(CloudQueueMessage queueMessage)
		{
			QueueMessages.Add(queueMessage);
			SortQueueMessages();
			QueueMessageCount = QueueMessages.Count;
		}
	}
}
