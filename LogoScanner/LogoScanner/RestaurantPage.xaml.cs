﻿using System;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace LogoScanner
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RestaurantPage : TabbedPage
    {
        private string reviewNo;

        public RestaurantPage()
        {
            InitializeComponent();
        }

        public class RestService
        {
            readonly HttpClient _client;

            public RestService()
            {
                _client = new HttpClient();
            }
        }

        protected override async void OnAppearing() // when page loads
        {
            base.OnAppearing();

            var request = await Requests.ConnectToResDiary(); // connect to resdiary api

            if (request.status.Equals("Success")) // if connection to api is successful
            {
                GetRestaurantData("https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/CairncrossCafe/Summary?numberOfReviews=5", request.message);
            }
            else
            {
                await DisplayAlert("Error", request.message, "OK"); // displays an error message to the user
            }
        }

        private async void GetRestaurantData(string url, string token)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                //Get the Results from the API Call
                var contents = await response.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(contents);

                //Parse the API Call and split the JSon object into the various variables.
                NameLabel.Text = (result["Name"] == null || string.IsNullOrEmpty(result["Name"].ToString()))
                                ? "Restaurant Name" : result["Name"].ToString();

                //AddressLabel.Text = (result["FullAddress"] == null || string.IsNullOrEmpty(result["FullAddress"].ToString()))
                                //? "Address" : result["FullAddress"].ToString();

                Logo.Source = (result["LogoUrl"] == null || string.IsNullOrEmpty(result["LogoUrl"].ToString()))
                                ? "Logo" : result["LogoUrl"].ToString();

                reviewNo = (result["NumberOfReviews"] == null || string.IsNullOrEmpty(result["NumberOfReviews"].ToString()))
                                ? "Number of Reviews" : result["NumberOfReviews"].ToString();

                int stars = (int) Math.Round(Double.Parse(result["AverageReviewScore"].ToString()), 0, MidpointRounding.AwayFromZero);

                StarLabel.Text += (result["AverageReviewScore"] == null || string.IsNullOrEmpty(result["AverageReviewScore"].ToString()))
                                ? "No Average Review Score" : String.Concat(Enumerable.Repeat("★", stars));

                string Times = (result["AvailableTimeSlots"] == null || string.IsNullOrEmpty(result["AvailableTimeSlots"].ToString()))
                                ? "No Available TimeSlots" : result["AvailableTimeSlots"].ToString();

                CuisinesLabel.Text = (result["CusineTypes"] == null || string.IsNullOrEmpty(result["CusineTypes"].ToString()))
                                ? "No Set Cusine Types" : result["CusineTypes"].ToString();

                PriceLabel.Text += (result["PricePoint"] == null || string.IsNullOrEmpty(result["PricePoint"].ToString()))
                                ? "No Price Point" : String.Concat(Enumerable.Repeat("£", Int32.Parse(result["PricePoint"].ToString())));


                double latitude = Convert.ToDouble(result["Latitude"].ToString());
                double longitude = Convert.ToDouble(result["Longitude"].ToString());
                string name = result["Name"].ToString();

                var pin = new Pin()
                {
                    Position = new Position(latitude, longitude),
                    Label = name,
                };

                MapArea.Pins.Add(pin);
                MapArea.MoveToRegion(new MapSpan(new Position(latitude, longitude), 0.01, 0.01));

                // open up directions to restaurant in map app when map area is clicked
                MapArea.MapClicked += async (object sender, MapClickedEventArgs e) =>
                {
                    await Xamarin.Essentials.Map.OpenAsync(
                        new Location(latitude, longitude),
                        new MapLaunchOptions { Name = name, NavigationMode = NavigationMode.Default }
                    );
                };
            }
        }
    }
}
