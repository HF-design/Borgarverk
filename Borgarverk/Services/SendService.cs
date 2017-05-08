﻿using System;
using System.Collections.Generic;
using Borgarverk.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text;

namespace Borgarverk
{
	public class SendService : ISendService
	{
		// token: 56a88b8eb1f26a81cd4a8341b03ed5485a40ae5f

		InfoFactory factory = new InfoFactory();

		public string EntryToJSon(EntryModel entry)
		{
			var json = JsonConvert.SerializeObject(entry);
			return json;
		}

		public string EntriesToJson(List<EntryModel> entries)
		{
			var json = JsonConvert.SerializeObject(entries);
			return json;
		}

		public async Task<Boolean> SendEntry(EntryModel entry)
		{
			// setup http client
			var client = new HttpClient(new HttpClientHandler
			{
				UseProxy = false
			});
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "b0132e2c143281b242ccd9465a7d817e808df500");
			var number = "ZI-K22";

			// Get mobileId
			var getAddress = $"https://floti.trackwell.com/api/mobiles/{number}/";
			var mobileId = "";
			Debug.WriteLine("Headers FIRST");
			Debug.WriteLine(client.DefaultRequestHeaders);
			try
			{
				var response = await client.GetAsync(getAddress);
				if (response.IsSuccessStatusCode)
				{
					var resContent = await response.Content.ReadAsStringAsync();
					mobileId = JsonConvert.DeserializeObject<IdModel>(resContent).Id;
				}
				else
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}


			// Post entry
			var postAddress = "https://floti.trackwell.com/api/roadwork/";
			var uri = new Uri(postAddress);

			entry.TimeSent = DateTime.Now;
			var info = factory.ConstructInfo(entry, mobileId); 

			var json = JsonConvert.SerializeObject(info);
			var buffer = System.Text.Encoding.UTF8.GetBytes(json);
			var byteContent = new ByteArrayContent(buffer);
			Debug.WriteLine("JSON");
			Debug.WriteLine(json);
			Debug.WriteLine("Uri");
			Debug.WriteLine(uri);
			Debug.WriteLine("Headers");
			Debug.WriteLine(client.DefaultRequestHeaders);
			Debug.WriteLine("Byte Content");
			Debug.WriteLine(byteContent);
			try
			{
				var response = await client.PostAsync(uri, byteContent);
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}

