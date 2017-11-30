using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Webkit;
using AzureStorageExplorer.Droid;
using Java.IO;
using Xamarin.Forms;


[assembly: Dependency(typeof(FilePicker))]
namespace AzureStorageExplorer.Droid
{
	public class FilePicker : IFilePicker
	{
		private Context context;
		private int requestId;
		private TaskCompletionSource<FileData> completionSource;

		public FilePicker()
		{
			this.context = Android.App.Application.Context;
		}

		public async Task<FileData> PickFile()
		{
			var media = await TakeMediaAsync("file/*", Intent.ActionGetContent);

			return media;
		}

		private Task<FileData> TakeMediaAsync(string type, string action)
		{
			int id = GetRequestId();

			var ntcs = new TaskCompletionSource<FileData>(id);
			if (Interlocked.CompareExchange(ref this.completionSource, ntcs, null) != null)
				throw new InvalidOperationException("Only one operation can be active at a time");

			try
			{
				Intent pickerIntent = new Intent(this.context, typeof(FilePickerActivity));
				pickerIntent.SetFlags(ActivityFlags.NewTask);
                this.context.StartActivity(pickerIntent);
                EventHandler<EventArgs> cancelledHandler = null;
				EventHandler<FilePickerEventArgs> handler = null;

				handler = (s, e) =>
				{
					var tcs = Interlocked.Exchange(ref this.completionSource, null);
                    FilePickerActivity.FilePicked -= handler;
                    tcs.SetResult(new FileData()
					{
						DataArray = e.FileByte,
						FileName = e.FileName
					});
				};

				cancelledHandler = (s, e) =>
				{
					var tcs = Interlocked.Exchange(ref completionSource, null);
                    FilePickerActivity.FilePickCancelled -= cancelledHandler;
                    tcs?.SetResult(null);
				};

				FilePickerActivity.FilePickCancelled += cancelledHandler;
				FilePickerActivity.FilePicked += handler;

			}
			catch (Exception exAct)
			{
				System.Diagnostics.Debug.Write(exAct);
			}

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


	}


	[Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [Android.Runtime.Preserve(AllMembers = true)]
	public class FilePickerActivity : Activity
	{
		private Context context;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			this.context = Android.App.Application.Context;

			Bundle b = (savedInstanceState ?? Intent.Extras);

			Intent intent = new Intent(Intent.ActionGetContent);
			intent.SetType("*/*");

			intent.AddCategory(Intent.CategoryOpenable);
			try
			{
				StartActivityForResult(Intent.CreateChooser(intent, "Selecione o arquivo a ser enviado"),
					  0);
			}
			catch (System.Exception exAct)
			{
				System.Diagnostics.Debug.Write(exAct);
			}
		}

		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (resultCode == Result.Canceled)
			{
				OnFilePickCancelled();
				Finish();
			}
			else
			{
				System.Diagnostics.Debug.Write(data.Data);
				try
				{
					var _uri = data.Data;
                    string filePath = IOUtil.getPath(this.context, _uri);
                    if (string.IsNullOrEmpty(filePath))
						filePath = _uri.Path;
                    var file = IOUtil.readFile(filePath);
                    var fileName = GetFileName(this.context, _uri);
                    OnFilePicked(new FilePickerEventArgs(file, fileName));
				}
				catch (System.Exception readEx)
				{
					OnFilePickCancelled();
					System.Diagnostics.Debug.Write(readEx);
				}
				finally
				{
					Finish();
				}
			}
		}

		string GetFileName(Context context, Android.Net.Uri uri)
		{
            String[] projection = { MediaStore.MediaColumns.DisplayName };
            ContentResolver cr = context.ContentResolver;
			string name = "";
			ICursor metaCursor = cr.Query(uri, projection, null, null, null);
			if (metaCursor != null)
			{
				try
				{
					if (metaCursor.MoveToFirst())
					{
						name = metaCursor.GetString(0);
					}
				}
				finally
				{
					metaCursor.Close();
				}
			}
			return name;
		}

		internal static event EventHandler<FilePickerEventArgs> FilePicked;
		internal static event EventHandler<EventArgs> FilePickCancelled;

		private static void OnFilePickCancelled()
		{
			FilePickCancelled?.Invoke(null, null);
		}

		private static void OnFilePicked(FilePickerEventArgs e)
		{
			var picked = FilePicked;
			if (picked != null)
				picked(null, e);
		}
	}

	public class IOUtil
	{

		public static String getPath(Context context, Android.Net.Uri uri)
		{

			bool isKitKat = Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat;

			// DocumentProvider
			if (isKitKat && DocumentsContract.IsDocumentUri(context, uri))
			{
				// ExternalStorageProvider
				if (isExternalStorageDocument(uri))
				{
					String docId = DocumentsContract.GetDocumentId(uri);
					String[] split = docId.Split(':');
					String type = split[0];

					if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
					{
						return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
					}
				}
				// DownloadsProvider
				else if (isDownloadsDocument(uri))
				{

					String id = DocumentsContract.GetDocumentId(uri);
					Android.Net.Uri contentUri = ContentUris.WithAppendedId(
							Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

					return getDataColumn(context, contentUri, null, null);
				}
				// MediaProvider
				else if (isMediaDocument(uri))
				{
					String docId = DocumentsContract.GetDocumentId(uri);
					String[] split = docId.Split(':');
					String type = split[0];

					Android.Net.Uri contentUri = null;
					if ("image".Equals(type))
					{
						contentUri = MediaStore.Images.Media.ExternalContentUri;
					}
					else if ("video".Equals(type))
					{
						contentUri = MediaStore.Video.Media.ExternalContentUri;
					}
					else if ("audio".Equals(type))
					{
						contentUri = MediaStore.Audio.Media.ExternalContentUri;
					}

					String selection = "_id=?";
					String[] selectionArgs = new String[] {
					split[1]
			};

					return getDataColumn(context, contentUri, selection, selectionArgs);
				}
			}
			// MediaStore (and general)
			else if ("content".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
			{
				return getDataColumn(context, uri, null, null);
			}
			// File
			else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
			{
				return uri.Path;
			}

			return null;
		}

		public static String getDataColumn(Context context, Android.Net.Uri uri, String selection,
		String[] selectionArgs)
		{

			ICursor cursor = null;
			String column = "_data";
			String[] projection = {
				column
			};

			try
			{
				cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs,
						null);
				if (cursor != null && cursor.MoveToFirst())
				{
					int column_index = cursor.GetColumnIndexOrThrow(column);
					return cursor.GetString(column_index);
				}
			}
			finally
			{
				if (cursor != null)
					cursor.Close();
			}
			return null;
		}

		/**
 * @param uri The Uri to check.
 * @return Whether the Uri authority is ExternalStorageProvider.
 */
		public static bool isExternalStorageDocument(Android.Net.Uri uri)
		{
			return "com.android.externalstorage.documents".Equals(uri.Authority);
		}

		/**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is DownloadsProvider.
         */
		public static bool isDownloadsDocument(Android.Net.Uri uri)
		{
			return "com.android.providers.downloads.documents".Equals(uri.Authority);
		}

		/**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is MediaProvider.
         */
		public static bool isMediaDocument(Android.Net.Uri uri)
		{
			return "com.android.providers.media.documents".Equals(uri.Authority);
		}

		public static byte[] readFile(String file)
		{
			try
			{
				return readFile(new Java.IO.File(file));
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Write(ex);
				return new byte[0];
			}
		}

		public static byte[] readFile(Java.IO.File file)
		{
			// Open file
			RandomAccessFile f = new RandomAccessFile(file, "r");
			try
			{
				// Get and check length
				long longlength = f.Length();
				int length = (int)longlength;
				if (length != longlength)
					throw new Java.IO.IOException("Tamanho do arquivo excede o permitido!");
				// Read file and return data
				byte[] data = new byte[length];
				f.ReadFully(data);
				return data;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Write(ex);
				return new byte[0];
			}
			finally
			{
				f.Close();
			}
		}

		public static string GetMimeType(string url)
		{
			String type = null;
			String extension = MimeTypeMap.GetFileExtensionFromUrl(url);

			if (extension != null)
			{
				type = MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);
			}

			return type;
		}
	}
}
