using System;
namespace AzureStorageExplorer
{
	public static class FileSizeHelper
	{
		public static string GetHumanizedSizeFromBytes(long length)
		{
			if (length > 1024000000000)
			{
				return String.Format("{0:0.0} TB", ((double)length / 1024000000000.0));
			}
			else if (length > 1024000000)
			{
				return String.Format("{0:0.0} GB", ((double)length / 1024000000.0));
			}
			else if (length > 1024000)
			{
				return String.Format("{0:0.0} MB", ((double)length / 1024000.0));
			}
			else if (length > 1024)
			{
				return String.Format("{0:0.0} KB", ((double)length / 1024.0));
			}
			else
			{
				return length + " B";
			}
		}
	}
}
