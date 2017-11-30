using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAzure.Storage.Queue;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class QueueMessageDetailsViewModel : ViewModelBase
	{
		private ASECloudQueue queue;

		CloudQueueMessage queueMessage;
		public CloudQueueMessage QueueMessage
		{
			get { return queueMessage; }
			set { SetProperty(ref queueMessage, value); }
		}

		public QueueMessageDetailsViewModel(INavigation navigation, CloudQueueMessage message, ASECloudQueue queue) : base(navigation)
		{
			QueueMessage = message;
			this.queue = queue;
		}

		public string MessageText
		{
			get
			{
				return QueueMessage.AsString;
			}
		}

		public string InsertionLocalTime
		{
			get
			{
				if (QueueMessage.InsertionTime.HasValue)
				{
					return QueueMessage.InsertionTime.Value.ToLocalTime().ToString();
				}
				else return "Not set";
			}
		}

		public string ExpirationLocalTime
		{
			get
			{
				if (QueueMessage.ExpirationTime.HasValue)
				{
					return QueueMessage.ExpirationTime.Value.ToLocalTime().ToString();
				}
				else return "Not set";
			}
		}

		ICommand deleteQueueMessageCommand;
		public ICommand DeleteQueueMessageCommand =>
		deleteQueueMessageCommand ?? (deleteQueueMessageCommand = new Command(async () => await ExecuteDeleteQueueMessageCommandAsync()));

		async Task ExecuteDeleteQueueMessageCommandAsync()
		{
			if (IsBusy)
				return;

			App.Logger.TrackPage(AppPage.DeleteQueueMessage.ToString());
			var page = new DeleteQueueMessagePopup(QueueMessage);
			await PopupNavigation.PushAsync(page);
		}
	}
}
