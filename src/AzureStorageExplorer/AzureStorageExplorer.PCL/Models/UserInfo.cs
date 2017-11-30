using System;
using Realms;

namespace AzureStorageExplorer.PCL
{
	public class UserInfo : RealmObject
	{

		public string DisplayableName
		{
			get;
			set;
		}

		public UserInfo(string displayableName)
		{
			DisplayableName = displayableName;
		}

		public UserInfo()
		{
		}
	}
}
