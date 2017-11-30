using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage.File;
using MvvmHelpers;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class FilesViewModel : ViewModelBase
	{
		//long byteCount = 0;

		public ObservableRangeCollection<IListFileItem> Files { get; } = new ObservableRangeCollection<IListFileItem>();
		public ObservableRangeCollection<IListFileItem> SortedFiles { get; } = new ObservableRangeCollection<IListFileItem>();

        ASECloudFileShare fileShare;
		public ASECloudFileShare FileShare
		{
			get { return fileShare; }
			set { SetProperty(ref fileShare, value); }
		}

        bool noFilesFound;
        public bool NoFilesFound
		{
			get { return noFilesFound; }
			set { SetProperty(ref noFilesFound, value); }
		}

        string noFilesFoundMessage;
        public string NoFilesFoundMessage
		{
			get { return noFilesFoundMessage; }
			set { SetProperty(ref noFilesFoundMessage, value); }
		}

        int fileAndDirectoryCount;
        public int FileAndDirectoryCount
		{
			get { return fileAndDirectoryCount; }
			set { SetProperty(ref fileAndDirectoryCount, value); }
		}        	

        public FilesViewModel(INavigation navigation, IUserDialogs userDialogs, ASECloudFileShare fileShare) : base(navigation, userDialogs)
        {
            FileShare = fileShare;
			NoFilesFoundMessage = "No Files Found";
		}

		public override void onAppearing()
		{
			base.onAppearing();
            MessagingService.Current.Unsubscribe<MessageArgsDeleteFile>(MessageKeys.DeleteFile);
		}


        ICommand loadFilesCommand;
        public ICommand LoadFilesCommand =>
			loadFilesCommand ?? (loadFilesCommand = new Command<bool>(async (f) => await ExecuteLoadFilesAsync()));

        async Task<bool> ExecuteLoadFilesAsync(bool force = false)
		{
			if (IsBusy)
				return false;

			try
			{
				IsBusy = true;
				NoFilesFound = false;

                if (!fileShare.BaseFileShare.Properties.Quota.HasValue) {
					await fileShare.BaseFileShare.FetchAttributesAsync();
					OnPropertyChanged("FileShare");
                }

                Files.Clear();
                var files = await FileShare.BaseFileShare.GetRootDirectoryReference().ListFilesAndDirectoriesAsync();

                FileAndDirectoryCount = files.Count();
                foreach (var file in files)
                {
                    Files.Add(file);
                }
                SortFiles();

                if (Files.Count == 0)
                {
                    NoFilesFoundMessage = "No Files Found";
                    NoFilesFound = true;
                }
                else
                {
                    NoFilesFound = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Report(ex, "Method", "ExecuteLoadFilesAsync");
                MessagingService.Current.SendMessage(MessageKeys.Error, ex);
            }
            finally
            {
                IsBusy = false;
            }
            return true;
        }

        private void SortFiles()
        {
            SortedFiles.ReplaceRange(Files.OrderBy(p => p.Share.Name));

            if (SortedFiles.Count > 0)
                NoFilesFound = false;
            else
                NoFilesFound = true;
		}

		ICommand forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
        forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

		async Task ExecuteForceRefreshCommandAsync()
        {
            await ExecuteLoadFilesAsync(true);
        }

        ICommand itemTapCommand;
        public ICommand ItemTapCommand =>
        itemTapCommand ?? (itemTapCommand = new Command<IListFileItem>(async (c) => ExecuteTapFileCommandAsync(c)));

        async Task ExecuteTapFileCommandAsync(IListFileItem fileItem)
        {
            if (fileItem == null)
                return;

            MessagingService.Current.Subscribe<MessageArgsDeleteFile>(MessageKeys.DeleteFile, async (m, argsDeleteFile) =>
            {
                Navigation.PopAsync();
                IProgressDialog deletingDialog = UserDialogs.Loading("Deleting File");
                deletingDialog.Show();
                try
                {
                    var file = Files.Where(f => f.Share.Name == argsDeleteFile.FileName).FirstOrDefault();
                    if (file == null)
                        return;
                    await FileShare.BaseFileShare.GetRootDirectoryReference().GetFileReference(file.Share.Name).DeleteAsync();

                    App.Logger.Track(AppEvent.DeleteFile.ToString());
                    Files.Remove(file);
                    FileAndDirectoryCount--;
                    SortFiles();
                }
                catch (Exception ex)
                {
                    Logger.Report(ex, "Method", "HandleMessageArgsDeleteFile");
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
        }

        ICommand deleteFileShareCommand;
        public ICommand DeleteFileShareCommand =>
        deleteFileShareCommand ?? (deleteFileShareCommand = new Command(async () => await ExecuteDeleteFileShareCommandAsync()));

        async Task ExecuteDeleteFileShareCommandAsync()
        {
			if (IsBusy)
				return;
			App.Logger.TrackPage(AppPage.DeleteFileShare.ToString());
		}

        ICommand addFileCommand;
        public ICommand AddFileCommand =>
        addFileCommand ?? (addFileCommand = new Command(async () => await ExecuteAddFileCommandAsync()));

        async Task ExecuteAddFileCommandAsync()
        {
            if (IsBusy)
                return;

            App.Logger.TrackPage(AppPage.NewFile.ToString());
        }

        public void AddFile(CloudFile file)
		{
			Files.Add(file);
            SortFiles();
			FileAndDirectoryCount = Files.Count;
		}
	}
}
