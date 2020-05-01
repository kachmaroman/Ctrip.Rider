using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Google.Maps.Android;
using Java.Util;
using Newtonsoft.Json;

namespace Ctrip.Rider.Helpers
{
    public class MapFunctionHelper
    {
	    public string mapkey;
	    public GoogleMap map;
	    public double distance;
        public double duration;
        public string distanceString;
        public string durationString;
        public Marker pickupMarker;
        public Marker driverLocationMarker;
        public bool isRequestingDirection;

        public MapFunctionHelper(string mapkey, GoogleMap map)
        {
            this.mapkey = mapkey;
            this.map = map;
        }

        public string GetGeoCodeUrl(double lat, double lng)
        {
            return "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + mapkey;
        }

        public async Task<string> GetGeoJsonAsync(string url)
        {
	        HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);

            return await client.GetStringAsync(url);
        }

        public async Task<string> FindCordinateAddress(LatLng position)
        {
            string url = GetGeoCodeUrl(position.Latitude, position.Longitude);
           
            string placeAddress = string.Empty;

            //Check for Internet connection
            string json = await GetGeoJsonAsync(url);

            if (string.IsNullOrEmpty(json))
            {
	            return placeAddress;
            }

            GeocodingParser geoCodeData = JsonConvert.DeserializeObject<GeocodingParser>(json);

            if (geoCodeData.status.Contains("ZERO"))
            {
	            return placeAddress;
            }

            if (geoCodeData.results[0] != null)
            {
	            placeAddress = geoCodeData.results[0].formatted_address;
            }

            return placeAddress;
        }

        public async Task<string> GetDirectionJsonAsync(LatLng location, LatLng destination)
        {
            //Origin of route
            string str_origin = "origin=" + location.Latitude + "," + location.Longitude;

            //Destination of route
            string str_destination = "destination=" + destination.Latitude + "," + destination.Longitude;

            //mode
            string mode = "mode=driving";

            //Building the parameters to the webservice
            string parameters = str_origin + "&" + str_destination + "&" + "&" + mode + "&key=";

            //Output format
            string output = "json";

            string key = mapkey;

            //Building the final url string
            string url = "https://maps.googleapis.com/maps/api/directions/" + output + "?" + parameters + key;

            return await GetGeoJsonAsync(url);

        }

        public void DrawTripOnMap(string json)
        {
	        DirectionParser directionData = JsonConvert.DeserializeObject<DirectionParser>(json);

            //Decode Encoded Route
            var points = directionData.routes[0].overview_polyline.points;
            IList<LatLng> line = PolyUtil.Decode(points);

            ArrayList routeList = new ArrayList();

            foreach (LatLng item in line)
            {
                routeList.Add(item);
            }

            //Draw Polylines on Map
            PolylineOptions polylineOptions = new PolylineOptions()
                .AddAll(routeList)
                .InvokeWidth(10)
                .InvokeColor(Color.Teal)
                .InvokeStartCap(new SquareCap())
                .InvokeEndCap(new SquareCap())
                .InvokeJointType(JointType.Round)
                .Geodesic(true);

            Android.Gms.Maps.Model.Polyline mPolyline = map.AddPolyline(polylineOptions);

            LatLng firstpoint = line.First();
            LatLng lastpoint = line.Last();

            MarkerOptions pickupMarkerOptions = new MarkerOptions();
            pickupMarkerOptions.SetPosition(firstpoint);
            pickupMarkerOptions.SetTitle("Pickup");
            pickupMarkerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));

            MarkerOptions destinationMarkerOptions = new MarkerOptions();
            destinationMarkerOptions.SetPosition(lastpoint);
            destinationMarkerOptions.SetTitle("Destination");
            destinationMarkerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));

            MarkerOptions driverMarkerOptions = new MarkerOptions();
            driverMarkerOptions.SetPosition(firstpoint);
            driverMarkerOptions.SetTitle("Current Location");
            driverMarkerOptions.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.position));
            driverMarkerOptions.Visible(false);

            pickupMarker = map.AddMarker(pickupMarkerOptions);
            Marker destinationMarker = map.AddMarker(destinationMarkerOptions);
            driverLocationMarker = map.AddMarker(driverMarkerOptions);

            //Get Trip Bounds
            double southlng = directionData.routes[0].bounds.southwest.lng;
            double southlat = directionData.routes[0].bounds.southwest.lat;
            double northlng = directionData.routes[0].bounds.northeast.lng;
            double northlat = directionData.routes[0].bounds.northeast.lat;

            LatLng southwest = new LatLng(southlat, southlng);
            LatLng northeast = new LatLng(northlat, northlng);
            LatLngBounds tripBound = new LatLngBounds(southwest, northeast);

            map.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(tripBound, 100));
            map.SetPadding(40, 40, 40, 40);
            pickupMarker.ShowInfoWindow();

            duration = directionData.routes[0].legs[0].duration.value;
            distance = directionData.routes[0].legs[0].distance.value;
            durationString = directionData.routes[0].legs[0].duration.text;
            distanceString = directionData.routes[0].legs[0].distance.text;
        }

        public double GetEstimatedFare()
        {
	        const double minFare = 40;
            double baseFare = 30; //UAH 
            double distanceFare = 5; //UAH per kilometer
            double timeFare = 3; //UAH per minute

            double kmFare = (distance / 1000) * distanceFare;
            double minsFare = (duration / 60) * timeFare;

            double amount = baseFare + kmFare + minsFare;
            double fare = Math.Floor(amount / 10) * 10;

            return fare < minFare ? minFare : fare;
        }

        public string GetDuration()
        {
            return durationString;
        }

        //public async void UpdateDriverLocationToPickUp(LatLng firstposition, LatLng secondposition)
        //{
        //    if (!isRequestingDirection)
        //    {
        //        isRequestingDirection = true;
        //        string json = await GetDirectionJsonAsync(firstposition, secondposition);
        //        var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);
        //        string duration = directionData.routes[0].legs[0].duration.text;
        //        pickupMarker.Title = "Pickup Location";
        //        pickupMarker.Snippet = "Your Driver is " + duration + " Away";
        //        pickupMarker.ShowInfoWindow();
        //        isRequestingDirection = false;
        //    }


        //}

        //public void UpdateDriverArrived()
        //{
        //    pickupMarker.Title = "Pickup Location";
        //    pickupMarker.Snippet = "Your Driver has Arrived";
        //    pickupMarker.ShowInfoWindow();
        //}

        //public async void UpdateLocationToDestination(LatLng firstposition, LatLng secondposition)
        //{
        //    driverLocationMarker.Visible = true;
        //    driverLocationMarker.Position = firstposition;
        //    map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(firstposition, 15));

        //    if (!isRequestingDirection)
        //    {
        //        //Check Connection
        //        isRequestingDirection = true;
        //        string json = await GetDirectionJsonAsync(firstposition, secondposition);
        //        var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);
        //        string duration = directionData.routes[0].legs[0].duration.text;
        //        driverLocationMarker.Title = "Current Location";
        //        driverLocationMarker.Snippet = "Your Destination is " + duration + " Away";
        //        driverLocationMarker.ShowInfoWindow();
        //        isRequestingDirection = false;
        //    }

        //}
    }
}