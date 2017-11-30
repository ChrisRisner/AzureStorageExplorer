using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace AzureStorageExplorer
{
	/// <summary>
	/// This is the Settings static class that can be used in your Core solution or in any
	/// of your client applications. All settings are laid out the same exact way with getters
	/// and setters. 
	/// </summary>
	public class Settings : INotifyPropertyChanged
	{
		static ISettings AppSettings
		{
			get
			{
				return CrossSettings.Current;
			}
		}

		static Settings settings;

		/// <summary>
		/// Gets or sets the current settings. This should always be used
		/// </summary>
		/// <value>The current.</value>
		public static Settings Current
		{
			get { return settings ?? (settings = new Settings()); }
		}

		const string QueueVisibilityTimeoutSecondsKey = "queue_visibility_timeout";
		static readonly int QueueVisibilityTimeoutSecondsDefault = 1;


		/// <summary>
		/// Gets or sets a value indicating the visibility timeout when loading queue messages
		/// </summary>
		/// <value><c>1</c> by default or set by user</value>
		public int QueueVisibilityTimeoutSeconds
		{
            
            get { return AppSettings.GetValueOrDefault(QueueVisibilityTimeoutSecondsKey, QueueVisibilityTimeoutSecondsDefault); }

			set
			{
                if (AppSettings.AddOrUpdateValue(QueueVisibilityTimeoutSecondsKey, value))
					OnPropertyChanged();
			}
		}


		const string FirstRunKey = "first_run";
		static readonly bool FirstRunDefault = true;
		/// <summary>
		/// Gets or sets a value indicating whether the user wants to see favorites only.
		/// </summary>
		/// <value><c>true</c> if favorites only; otherwise, <c>false</c>.</value>
		public bool FirstRun
		{
			get { return AppSettings.GetValueOrDefault(FirstRunKey, FirstRunDefault); }
			set
			{
				if (AppSettings.AddOrUpdateValue(FirstRunKey, value))
					OnPropertyChanged();
			}
		}

		const string ForceUserLoginKey = "force_user_login";
		static readonly bool ForceUserLoginDefault = false;
		public bool ForceUserLogin
		{
			get
			{
				return AppSettings.GetValueOrDefault(ForceUserLoginKey, ForceUserLoginDefault);
			}
			set
			{
				if (AppSettings.AddOrUpdateValue(ForceUserLoginKey, value))
					OnPropertyChanged();
			}
		}

		const string EmailKey = "email_key";
		readonly string EmailDefault = string.Empty;
		public string Email
		{
			get { return AppSettings.GetValueOrDefault(EmailKey, EmailDefault); }
			set
			{
				if (AppSettings.AddOrUpdateValue(EmailKey, value))
				{
					OnPropertyChanged();
					OnPropertyChanged(nameof(UserAvatar));
					OnPropertyChanged(nameof(IsLoggedIn));
				}
			}
		}

		const string FirstNameKey = "firstname_key";
		readonly string FirstNameDefault = string.Empty;
		public string FirstName
		{
			get { return AppSettings.GetValueOrDefault(FirstNameKey, FirstNameDefault); }
			set
			{
				if (AppSettings.AddOrUpdateValue(FirstNameKey, value))
				{
					OnPropertyChanged();
					OnPropertyChanged(nameof(UserDisplayName));
				}
			}
		}

		const string LastNameKey = "lastname_key";
		readonly string LastNameDefault = string.Empty;
		public string LastName
		{
			get { return AppSettings.GetValueOrDefault(LastNameKey, LastNameDefault); }
			set
			{
				if (AppSettings.AddOrUpdateValue(LastNameKey, value))
				{
					OnPropertyChanged();
					OnPropertyChanged(nameof(UserDisplayName));
				}
			}
		}


		bool isConnected;
		public bool IsConnected
		{
			get { return isConnected; }
			set
			{
				if (isConnected == value)
					return;
				isConnected = value;
				OnPropertyChanged();
			}
		}

		#region Helpers

		public string UserDisplayName => IsLoggedIn ? $"{FirstName} {LastName}" : "Sign In";
		public string UserAvatar => IsLoggedIn ? Gravatar.GetURL(Email) : "profile_generic.png";
		public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Email);


		#endregion

		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName]string name = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		#endregion
	}
}
