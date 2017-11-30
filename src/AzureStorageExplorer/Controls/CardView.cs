﻿using System;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class CardView : Frame
	{
		public CardView()
		{
			Padding = 0;
            if (Device.RuntimePlatform == Device.iOS)
			{
				HasShadow = false;
				OutlineColor = Color.Transparent;
				BackgroundColor = Color.Transparent;
			}
		}
	}
}
