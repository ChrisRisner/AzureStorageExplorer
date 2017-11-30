using System;
namespace AzureStorageExplorer.Interfaces
{
	public interface ILaunchTwitter
	{
		bool OpenUserName(string username);
		bool OpenStatus(string statusId);
	}
}
