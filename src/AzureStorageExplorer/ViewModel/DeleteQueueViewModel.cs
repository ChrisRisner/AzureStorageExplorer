using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FormsToolkit;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class DeleteQueueViewModel : ViewModelBase
	{
		string queueName = null;
		string storageAccountName = null;

		public string QueueName
		{
			get
			{
				return this.queueName;
			}
		}

		string queueNameConfirmation;
		public string QueueNameConfirmation
		{
			get { return queueNameConfirmation; }
			set
			{
				value = value.ToLower();
				SetProperty(ref queueNameConfirmation, value);
				OnPropertyChanged("ReadyToDelete");
			}
		}

		public bool ReadyToDelete
		{
			get
			{
				if (QueueNameConfirmation == queueName)
					return true;
				return false;
			}
		}

		public DeleteQueueViewModel(INavigation navigation, string queueName, string storageAccountName) : base(navigation)
		{
			this.queueName = queueName;
			this.storageAccountName = storageAccountName;
		}


		ICommand deleteQueueCommand;
		public ICommand DeleteQueueCommand =>
		deleteQueueCommand ?? (deleteQueueCommand = new Command(async () => await ExecuteDeleteQueueCommandlAsync()));

		async Task ExecuteDeleteQueueCommandlAsync()
		{
			MessagingService.Current.SendMessage(MessageKeys.DeleteQueue, new MessageArgsDeleteQueue(queueName, storageAccountName));
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
