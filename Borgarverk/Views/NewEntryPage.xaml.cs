﻿using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Borgarverk
{
	public partial class NewEntryPage : ContentPage
	{
		public NewEntryPage()
		{
			InitializeComponent();
			BindingContext = new EntryViewModel();
		}
	}
}
