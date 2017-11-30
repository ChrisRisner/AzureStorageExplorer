using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage.Queue;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class NewQueueMessageViewModel : ViewModelBase
	{
		public string[] ExpiresInPeriods { get; } = new string[] { "Days", "Hours", "Minutes", "Seconds" };
		private TimeSpan MinimumExpirationtime = new TimeSpan(0, 0, 1);
		private TimeSpan MaximumExpirationtime = new TimeSpan(7, 0, 0, 0);

		private QueueMessagesViewModel queueMessagesViewModel = null;

		string messageText;
		public string MessageText
		{
			get { return messageText; }
			set
			{
				SetProperty(ref messageText, value);
				OnPropertyChanged("ReadyToSave");
			}
		}

		string selectedExpirationTimePeriod;
		public string SelectedExpirationTimePeriod
		{
			get { return selectedExpirationTimePeriod; }
			set
			{
				SetProperty(ref selectedExpirationTimePeriod, value);
				OnPropertyChanged("ReadyToSave");
			}
		}

		int expiresInTime;
		public int ExpiresInTime
		{
			get { return expiresInTime; }
			set
			{
				SetProperty(ref expiresInTime, value);
				OnPropertyChanged("ReadyToSave");
			}
		}

		bool encodeMessage;
		public bool EncodeMessage
		{
			get { return encodeMessage; }
			set
			{
				SetProperty(ref encodeMessage, value);
				OnPropertyChanged("ReadyToSave");
			}
		}

		public bool ReadyToSave
		{
			get
			{ 
				return true;
			}
		}

		public NewQueueMessageViewModel(INavigation navigation, IUserDialogs userDialogs, QueueMessagesViewModel queueMessagesVM) : base(navigation, userDialogs)
		{
			this.queueMessagesViewModel = queueMessagesVM;
			ExpiresInTime = 7;
			SelectedExpirationTimePeriod = "Days";
			EncodeMessage = true;
		}

		ICommand createQueueMessageCommand;
		public ICommand CreateQueueMessageCommand =>
		createQueueMessageCommand ?? (createQueueMessageCommand = new Command(async () => await ExecuteCreateQueueMessageAsync()));

		async Task ExecuteCreateQueueMessageAsync()
		{
			if (IsBusy)
				return;

			if (!ReadyToSave)
			{
				//This should never happen as the save button should be disabled
				Logger.Report(new Exception("Create Queue Message called when ReadyToSave was false"), "Method", "ExecuteCreateQueueMessageAsync");
				return;
			}

			//Messages must expire in 1 second to 7 days
			//Calculate time to live
			TimeSpan? messageTTL = null;
			switch (SelectedExpirationTimePeriod)
			{
				case "Days":
					messageTTL = new TimeSpan(ExpiresInTime, 0, 0, 0);
					break;
				case "Hours":
					messageTTL = new TimeSpan(0, ExpiresInTime, 0, 0);
					break;
				case "Minutes":
					messageTTL = new TimeSpan(0, 0, ExpiresInTime, 0);
					break;
				case "Seconds":
					messageTTL = new TimeSpan(0, 0, 0, ExpiresInTime);
					break;
			}
			if (messageTTL < MinimumExpirationtime || messageTTL > MaximumExpirationtime)
			{
				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
				{
					Title = "Expiration time is invalid",
					Message = "Messages must expire in no less than 1 second and no more than 7 days.",
					Cancel = "OK"
				});
				return;
			}            			

			IProgressDialog savingDialog = UserDialogs.Loading("Saving Queue Message");
			savingDialog.Show();

			try
			{
				IsBusy = true;
				var newMessage = new CloudQueueMessage(MessageText);
				await queueMessagesViewModel.Queue.BaseQueue.AddMessageAsync(newMessage, messageTTL, null, null, null);
				if (queueMessagesViewModel != null)
				{
					queueMessagesViewModel.AddQueueMessage(newMessage);
					App.Logger.Track(AppEvent.CreatedQueueMessage.ToString());
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
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteCreateQueueMessageAsync");
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
