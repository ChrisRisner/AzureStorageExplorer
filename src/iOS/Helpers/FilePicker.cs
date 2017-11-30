using System;
using System.Threading;
using System.Threading.Tasks;
using AzureStorageExplorer.iOS;
using Foundation;
using MobileCoreServices;
using Rg.Plugins.Popup.IOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(FilePicker))]
namespace AzureStorageExplorer.iOS
{
	public class FilePicker : NSObject, IFilePicker, IUIDocumentMenuDelegate
	{
		private int requestId;
		private TaskCompletionSource<FileData> completionSource;

		public EventHandler<FilePickerEventArgs> handler
		{
			get;
			set;

		}

		public async Task<FileData> PickFile()
		{
			var media = await TakeMediaAsync();
			return media;
		}

		private Task<FileData> TakeMediaAsync()
		{
			int id = GetRequestId();

			var ntcs = new TaskCompletionSource<FileData>(id);
			if (Interlocked.CompareExchange(ref this.completionSource, ntcs, null) != null)
				throw new InvalidOperationException("Only one operation can be active at a time");
            
			var allowedUTIs = new string[] {				
				"public.data"
			};

			UIDocumentMenuViewController importMenu = 
				new UIDocumentMenuViewController(allowedUTIs, UIDocumentPickerMode.Import);
			importMenu.Delegate = this;
            importMenu.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            Popup.PLATFORM_RENDERER.PresentViewController(importMenu, true, null);
            UIPopoverPresentationController presPopover = importMenu.PopoverPresentationController;
            if (presPopover != null)
			{
				presPopover.SourceView = Popup.PLATFORM_RENDERER.View;
				presPopover.PermittedArrowDirections = UIPopoverArrowDirection.Down;
			}

			handler = null;

			handler = (s, e) =>
			{
				var tcs = Interlocked.Exchange(ref this.completionSource, null);

				tcs.SetResult(new FileData
				{
					DataArray = e.FileByte,
					FileName = e.FileName
				});
			};


			return completionSource.Task;
		}

		private int GetRequestId()
		{
			int id = this.requestId;
			if (this.requestId == Int32.MaxValue)
				this.requestId = 0;
			else
				this.requestId++;

			return id;
		}

		public void DidPickDocumentPicker(UIDocumentMenuViewController documentMenu, UIDocumentPickerViewController documentPicker)
		{
			documentPicker.DidPickDocument += DocumentPicker_DidPickDocument;
			documentPicker.WasCancelled += DocumentPicker_WasCancelled;
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(documentPicker, true, null);
		}

		public void WasCancelled(UIDocumentMenuViewController documentMenu)
		{
			var tcs = Interlocked.Exchange(ref completionSource, null);
			tcs?.SetResult(null);
		}

		void DocumentPicker_DidPickDocument(object sender, UIDocumentPickedEventArgs e)
		{
			var securityEnabled = e.Url.StartAccessingSecurityScopedResource();
            var doc = new UIDocument(e.Url);
            var data = NSData.FromUrl(e.Url);
            byte[] dataBytes = new byte[data.Length];
            System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));
            string filename = doc.LocalizedName;
			var pathname = doc.FileUrl?.ToString();
            if (filename == null)
			{
				var filesplit = pathname?.LastIndexOf('/') ?? 0;
                filename = pathname?.Substring(filesplit + 1);
				string normalizedFilename = filename.Replace("%20", " ");
				OnFilePicked(new FilePickerEventArgs(dataBytes, normalizedFilename));
			}
			else
			{
				OnFilePicked(new FilePickerEventArgs(dataBytes, filename));
			}

		}

		private void OnFilePicked(FilePickerEventArgs e)
		{
			var picked = handler;
			if (picked != null)
				picked(null, e);
		}

		public void DocumentPicker_WasCancelled(object sender, EventArgs e)
		{
			var tcs = Interlocked.Exchange(ref completionSource, null);
			tcs?.SetResult(null);
		}
	}
}
