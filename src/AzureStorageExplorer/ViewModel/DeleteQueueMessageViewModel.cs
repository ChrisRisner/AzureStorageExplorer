using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage.Queue;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class DeleteQueueMessageViewModel : ViewModelBase
	{
		CloudQueueMessage queueMessage;

		public CloudQueueMessage QueueMessage
		{
			get { return this.queueMessage; }
			set { SetProperty(ref queueMessage, value); }
		}

		string textConfirmation;
		public string TextConfirmation
		{
			get { return textConfirmation; }
			set
			{
				SetProperty(ref textConfirmation, value);
				OnPropertyChanged("ReadyToDelete");
			}
		}

		public bool ReadyToDelete
		{
			get
			{
				if (!string.IsNullOrEmpty(TextConfirmation) && TextConfirmation.ToLower() == "yes")
					return true;
				return false;
			}
		}

		public DeleteQueueMessageViewModel(INavigation navigation, CloudQueueMessage queueMessage) : base(navigation)
		{
			this.QueueMessage = queueMessage;
		}

		ICommand deleteQueueMessageCommand;
		public ICommand DeleteQueueMessageCommand =>
		deleteQueueMessageCommand ?? (deleteQueueMessageCommand = new Command(async () => await ExecuteDeleteQueueMessageCommandlAsync()));

		async Task ExecuteDeleteQueueMessageCommandlAsync()
		{
			MessagingService.Current.SendMessage(MessageKeys.DeleteQueueMessage, new MessageArgsDeleteQueueMessage(queueMessage.Id));
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
