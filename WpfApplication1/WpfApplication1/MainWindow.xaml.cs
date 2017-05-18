using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApplication1.GeocodeService;
using WpfApplication1.ImageryService;
using WpfApplication1.ServiceService;
using System.Text.RegularExpressions;
using WpfApplication1.RouteService;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        //The Geocode Service
        private String GeocodeAddress(string address)
        {
            string results = "";
            string key = "ApwndqJgdJ9M1Bpb7d_ihBwXW-J0N3HdXrZvFZqvFtmeYN5DewRoGPI7czgFo5Sh";
            GeocodeRequest geocodeRequest = new GeocodeRequest();

            // Set the credentials using a valid Bing Maps key
            geocodeRequest.Credentials = new GeocodeService.Credentials();
            geocodeRequest.Credentials.ApplicationId = key;

            // Set the full address query
            geocodeRequest.Query = address;

            // Set the options to only return high confidence results 
            ConfidenceFilter[] filters = new ConfidenceFilter[1];
            filters[0] = new ConfidenceFilter();
            filters[0].MinimumConfidence = GeocodeService.Confidence.High;

            // Add the filters to the options
            GeocodeOptions geocodeOptions = new GeocodeOptions();
            geocodeOptions.Filters = filters;
            geocodeRequest.Options = geocodeOptions;

            // Make the geocode request
            GeocodeServiceClient geocodeService = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            GeocodeResponse geocodeResponse = geocodeService.Geocode(geocodeRequest);

            if (geocodeResponse.Results.Length > 0)
                results = String.Format("Latitude: {0}\nLongitude: {1}",
                  geocodeResponse.Results[0].Locations[0].Latitude,
                  geocodeResponse.Results[0].Locations[0].Longitude);
            else
                results = "No Results Found";

            return results;
        }

        private void Geocode_Click(object sender, RoutedEventArgs e)
        {
            labelResults.Visibility = Visibility.Visible;
            imageResults.Visibility = Visibility.Hidden;
           
            labelResults.Content = GeocodeAddress(textInput.Text);

        }


        //The Imagery Service
        private string GetImagery(string locationString)
        {
            string key = "ApwndqJgdJ9M1Bpb7d_ihBwXW-J0N3HdXrZvFZqvFtmeYN5DewRoGPI7czgFo5Sh";
            MapUriRequest mapUriRequest = new MapUriRequest();

            // Set credentials using a valid Bing Maps key
            mapUriRequest.Credentials = new ImageryService.Credentials();
            mapUriRequest.Credentials.ApplicationId = key;

            // Set the location of the requested image
            mapUriRequest.Center = new ImageryService.Location();
            string[] digits = locationString.Split(',');
            mapUriRequest.Center.Latitude = double.Parse(digits[0].Trim());
            mapUriRequest.Center.Longitude = double.Parse(digits[1].Trim());

            // Set the map style and zoom level
            MapUriOptions mapUriOptions = new MapUriOptions();
            mapUriOptions.Style = MapStyle.AerialWithLabels;
            mapUriOptions.ZoomLevel = 17;

            // Set the size of the requested image in pixels
            mapUriOptions.ImageSize = new ImageryService.SizeOfint();
            mapUriOptions.ImageSize.Height = 200;
            mapUriOptions.ImageSize.Width = 300;

            mapUriRequest.Options = mapUriOptions;

            //Make the request and return the URI
            ImageryServiceClient imageryService = new ImageryServiceClient("BasicHttpBinding_IImageryService");
            MapUriResponse mapUriResponse = imageryService.GetMapUri(mapUriRequest);
            return mapUriResponse.Uri;
        }

        private void RequestImage_Click(object sender, RoutedEventArgs e)
        {
            labelResults.Visibility = Visibility.Hidden;
            imageResults.Visibility = Visibility.Visible;

            //Get URI and set image
            String imageURI = GetImagery(textInput.Text);
            imageResults.Source = new BitmapImage(new Uri(imageURI));
        }

        //The Search Service

        private string SearchKeywordLocation(string keywordLocation)
        {
            String results = "";
            String key = "ApwndqJgdJ9M1Bpb7d_ihBwXW-J0N3HdXrZvFZqvFtmeYN5DewRoGPI7czgFo5Sh";
            SearchRequest searchRequest = new SearchRequest();

            // Set the credentials using a valid Bing Maps key
            searchRequest.Credentials = new ServiceService.Credentials();
            searchRequest.Credentials.ApplicationId = key;

            //Create the search query
            StructuredSearchQuery ssQuery = new StructuredSearchQuery();
            string[] parts = keywordLocation.Split(';');
            ssQuery.Keyword = parts[0];
            ssQuery.Location = parts[1];
            searchRequest.StructuredQuery = ssQuery;

            //Define options on the search
            searchRequest.SearchOptions = new SearchOptions();
            searchRequest.SearchOptions.Filters =
                new FilterExpression()
                {
                    PropertyId = 3,
                    CompareOperator = CompareOperator.GreaterThanOrEquals,
                    FilterValue = 8.16
                };

            //Make the search request 
            SearchServiceClient searchService = new SearchServiceClient("BasicHttpBinding_ISearchService");
            SearchResponse searchResponse = searchService.Search(searchRequest);

            //Parse and format results
            if (searchResponse.ResultSets[0].Results.Length > 0)
            {
                StringBuilder resultList = new StringBuilder("");
                for (int i = 0; i < searchResponse.ResultSets[0].Results.Length; i++)
                {
                    resultList.Append(String.Format("{0}. {1}\n", i + 1,
                        searchResponse.ResultSets[0].Results[i].Name));
                }

                results = resultList.ToString();
            }
            else
                results = "No results found";

            return results;
        }


        private void Search_Click(object sender, RoutedEventArgs e)
        {
            labelResults.Visibility = Visibility.Visible;
            imageResults.Visibility = Visibility.Hidden;
            labelResults.Content = SearchKeywordLocation(textInput.Text);

        }

        //The Route Service

        private string CreateRoute(string waypointString)
        {
            string results = "";
            string key = "ApwndqJgdJ9M1Bpb7d_ihBwXW-J0N3HdXrZvFZqvFtmeYN5DewRoGPI7czgFo5Sh";
            RouteRequest routeRequest = new RouteRequest();

            // Set the credentials using a valid Bing Maps key
            routeRequest.Credentials = new RouteService.Credentials();
            routeRequest.Credentials.ApplicationId = key;

            //Parse user data to create array of waypoints
            string[] points = waypointString.Split(';');
            Waypoint[] waypoints = new Waypoint[points.Length];

            int pointIndex = -1;
            foreach (string point in points)
            {
                pointIndex++;
                waypoints[pointIndex] = new Waypoint();
                string[] digits = point.Split(','); waypoints[pointIndex].Location = new RouteService.Location();
                waypoints[pointIndex].Location.Latitude = double.Parse(digits[0].Trim());
                waypoints[pointIndex].Location.Longitude = double.Parse(digits[1].Trim());

                if (pointIndex == 0)
                    waypoints[pointIndex].Description = "Start";
                else if (pointIndex == points.Length)
                    waypoints[pointIndex].Description = "End";
                else
                    waypoints[pointIndex].Description = string.Format("Stop #{0}", pointIndex);
            }

            routeRequest.Waypoints = waypoints;

            // Make the calculate route request
            RouteServiceClient routeService = new RouteServiceClient("BasicHttpBinding_IRouteService");
            RouteResponse routeResponse = routeService.CalculateRoute(routeRequest);

            // Iterate through each itinerary item to get the route directions
            StringBuilder directions = new StringBuilder("");

            if (routeResponse.Result.Legs.Length > 0)
            {
                int instructionCount = 0;
                int legCount = 0;

                foreach (RouteLeg leg in routeResponse.Result.Legs)
                {
                    legCount++;
                    directions.Append(string.Format("Leg #{0}\n", legCount));

                    foreach (ItineraryItem item in leg.Itinerary)
                    {
                        instructionCount++;
                        directions.Append(string.Format("{0}. {1}\n",
                            instructionCount, item.Text));
                    }
                }
                //Remove all Bing Maps tags around keywords.  
                //If you wanted to format the results, you could use the tags
                Regex regex = new Regex("<[/a-zA-Z:]*>",
                  RegexOptions.IgnoreCase | RegexOptions.Multiline);
                results = regex.Replace(directions.ToString(), string.Empty);
            }
            else
                results = "No Route found";

            return results;
        }


        private void Route_Click(object sender, RoutedEventArgs e)
        {
            labelResults.Visibility = Visibility.Visible;
            imageResults.Visibility = Visibility.Hidden;
            labelResults.Content = CreateRoute(textInput.Text);
        }
    }
}
