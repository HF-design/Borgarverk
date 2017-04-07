﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Borgarverk.Models;
using Xamarin.Forms;

namespace Borgarverk.ViewModels
{
	public class EntryListViewModel : INotifyPropertyChanged
	{
		#region private variables
		private ObservableCollection<EntryModel> allEntries;
		private ObservableCollection<EntryModel> entries;
		private ObservableCollection<CarModel> cars;
		private ObservableCollection<StationModel> stations;
		private ISendService sendService;
		private EntryModel selectedEntry;
		private bool isSelected;
		private string searchString = "";
		private INavigation navigation;
		#endregion

		#region events
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		// Ath þurfum navigation ef við ætlum að geta editað færslu
		// en kannski vilja þeir það ekki? spyrjum á fundinum
		public EntryListViewModel(ISendService sService, INavigation navigation)
		{
			this.sendService = sService;
			this.navigation = navigation;
			allEntries = new ObservableCollection<EntryModel>(DataService.GetEntries());
			this.cars = new ObservableCollection<CarModel>(DataService.GetCars());
			this.stations = new ObservableCollection<StationModel>(DataService.GetStations());
			Entries = AllEntries;
			SendAllEntriesCommand = new Command(async () => await SendAllEntries());
			ModifySelectedEntryCommand = new Command(async () => await ModifySelectedEntry());
			DeleteSelectedEntryCommand = new Command(async () => await DeleteSelectedEntry());
			CloseCommand = new Command(() => Close());
			DeleteAllCommand = new Command(async () => await DeleteAll());
			SendEntryCommand = new Command(async () => await SendEntry());
			ButtonColor = "#d0cccc";
			selectedEntry = null;
		}

		#region commands
		public Command SwipeButtonCommand { get; }
		public Command SendAllEntriesCommand { get; }
		public Command ModifySelectedEntryCommand { get; }
		public Command DeleteSelectedEntryCommand { get; }
		public Command CloseCommand { get; }
		public Command DeleteAllCommand { get; }
		public Command SendEntryCommand { get; }
		#endregion

		#region properties
		public ObservableCollection<EntryModel> AllEntries
		{
			get { return allEntries; }
			set
			{
				allEntries = value;
				OnPropertyChanged("AllEntries");
			}
		}

		public ObservableCollection<EntryModel> Entries
		{
			get { return entries; }
			set
			{
				entries = value;
				OnPropertyChanged("Entries");
			}
		}

		public EntryModel SelectedEntry { 
			get { return selectedEntry; }
			set
			{
				if (selectedEntry != value)
				{
					if (value == null)
					{
						IsSelected = false;
					}
					else
					{
						IsSelected = true;
					}
					selectedEntry = value;
					OnPropertyChanged("SelectedEntry");
				}
				else
				{
					selectedEntry = null;
					IsSelected = false;
					OnPropertyChanged("SelectedEntry");
				}
			}
		}

		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				if (isSelected != value)
				{
					isSelected = value;
					OnPropertyChanged("IsSelected");
				}
			}
		}

		public string SearchString
		{
			get { return searchString; }
			set
			{
				if (searchString != value)
				{
					searchString = value;
					OnPropertyChanged("SearchString");
					this.Search();
				}
			}
		}

		public string ButtonColor { get; set; }

		public ObservableCollection<CarModel> Cars
		{
			get
			{
				return cars;
			}
			set
			{
				cars = value;
			}
		}

		public ObservableCollection<StationModel> Stations
		{
			get
			{
				return stations;
			}
			set
			{
				stations = value;

			}
		}
		#endregion

		async Task DeleteSelectedEntry()
		{
			var confirm = await Application.Current.MainPage.DisplayAlert("Eyða færslu", "Viltu eyða þessari færslu?", "Já", "Nei");
			if (confirm)
			{
				try
				{
					DataService.DeleteEntry(SelectedEntry.ID);
					allEntries.Remove(selectedEntry);
					IsSelected = false;
					SelectedEntry = null;
				}
				catch
				{
					await Application.Current.MainPage.DisplayAlert("Villa", "Ekki tókst að eyða færslu", "OK");
				}
			}
		}

		async Task ModifySelectedEntry()
		{
			var page = new NewEntryPage(this.sendService, selectedEntry);
			IsSelected = false;
			SelectedEntry = null;
			await this.navigation.PushAsync(page);
		}

		void Close()
		{
			IsSelected = false;
			SelectedEntry = null;
		}

		void Search()
		{
			var temp = AllEntries;
			if (String.IsNullOrEmpty(searchString))
			{
				AllEntries = entries;
			}
			var match = entries.Where(c => c.No.ToLower().Contains(SearchString.ToLower()) || 
			                          c.Station.ToLower().Contains(searchString.ToLower()) ||
			                          c.TimeCreated.ToString().ToLower().Contains(SearchString.ToLower()) ||
			                          c.TarQty.ToString().Contains(SearchString) ||
			                          c.RoadArea.ToString().Contains(SearchString));
			AllEntries = new ObservableCollection<EntryModel>(match);
		}

		async Task DeleteAll()
		{
			// If there are no entries
			if (allEntries.Count == 0)
			{
				return ;
			}

			var confirmed = await Application.Current.MainPage.DisplayAlert("Eyða öllum færslum", "Viltu eyða öllum færslum?", "Já", "Nei");
			if (confirmed)
			{
				DataService.DeleteEntries();
				allEntries.Clear();
			}
		}

		async Task SendAllEntries()
		{
			// If there are no entries
			if (allEntries.Count == 0)
			{
				return;
			}

			var confirmed = await Application.Current.MainPage.DisplayAlert("Senda allar færslur", "Viltu senda allar færslur?", "Já", "Nei");
			if (confirmed)
			{
				List<EntryModel> sentEntries = new List<EntryModel>();
				List<EntryModel> UnSentEntries = new List<EntryModel>();
				/*foreach (var entry in AllEntries)
				{
					if (!entry.Sent)
					{
						var sendResult = sendService.SendEntry(entry);
						if (!sendResult.Result)
						{
							UnSentEntries.Add(entry);
						}
						else
						{
							sentEntries.Add(entry);
						}
					}
				}

				for (var i = sentEntries.Count - 1; i >= 0; i--)
				{
					var tmpEntry = AllEntries[AllEntries.IndexOf(sentEntries[i])];
					tmpEntry.Sent = true;
					tmpEntry.TimeSent = DateTime.Now;
					DataService.UpdateEntry(tmpEntry);
				}

				if (UnSentEntries.Count != 0)
				{
					var msg = String.Format("Ekki tókst að senda {0} færslur, reyndu aftur síðar", UnSentEntries.Count);
					await Application.Current.MainPage.DisplayAlert("Sending Mistókst", msg, "Loka");
				}
				RefreshEntries();*/

				for (var i = allEntries.Count - 1; i >= 0; i--)
				{
					if (!allEntries[i].Sent)
					{
						var e = allEntries[i];
						var sendResult = sendService.SendEntry(e);
						if (!sendResult.Result)
						{
							UnSentEntries.Add(e);
						}
						else
						{
							sentEntries.Add(e);
							e.Sent = true;
							e.TimeSent = DateTime.Now;
							DataService.UpdateEntry(e);
						}
					}
				}

				if (UnSentEntries.Count != 0)
				{
					var msg = String.Format("Ekki tókst að senda {0} færslur, reyndu aftur síðar", UnSentEntries.Count);
					await Application.Current.MainPage.DisplayAlert("Sending Mistókst", msg, "Loka");
				}
				RefreshEntries();
			}
		}

		async Task SendEntry()
		{
			var sendResult = sendService.SendEntry(SelectedEntry);
			if (sendResult.Result)
			{
				SelectedEntry.Sent = true;
				SelectedEntry.TimeSent = DateTime.Now;
				DataService.UpdateEntry(SelectedEntry);
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert("Villa", "Ekki tókst að senda færslu, reyndu aftur síðar", "OK");
			}
			RefreshEntries();
		}

		private void RefreshEntries()
		{
			AllEntries = new ObservableCollection<EntryModel>(DataService.GetEntries());
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var changed = PropertyChanged;
			if (changed != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
