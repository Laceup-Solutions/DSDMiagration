using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace LaceupMigration
{
    public class LocationProvider
    {
        public static async Task<Location?> GetCurrentLocation()
        {
            Location? currentLocation;
            try
            {
                //var request = new GeolocationRequest(GeolocationAccuracy.Medium, new TimeSpan(0, 0, 3));
                //currentLocation = await Geolocation.GetLocationAsync(request);
                currentLocation = await Geolocation.GetLastKnownLocationAsync();
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                currentLocation = null;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                currentLocation = null;
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                currentLocation = null;
            }
            catch (Exception ex)
            {
                // Unable to get location
                currentLocation = null;
            }
            return currentLocation;
        }

        public static async Task<Location?> GetClientLocation(Client selected_client)
        {
            Location? clientLocation = null;
            if (selected_client.Latitude == 0 || selected_client.Longitude == 0)
            {
                try
                {
                    string address = selected_client.ShipToAddress.Replace('|', ' ');
                    var locations = await Geocoding.GetLocationsAsync(address);
                    var location = locations?.FirstOrDefault();

                    if (location != null)
                        clientLocation = location;
                }
                catch (Exception ex)
                {
                    clientLocation = null;
                }
            }
            else
            {
                clientLocation = new Location(selected_client.Latitude, selected_client.Longitude);
            }
            return clientLocation;
        }

        public static async Task<string> CalculateLocation(int clientId, Location? currentLocation)
        {
            try
            {
                double milesFromClient = -1;
                var selected_client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);

                if (selected_client != null && currentLocation != null)
                {
                    Location? clientLocation = null;

                    if (selected_client.Latitude == 0 || selected_client.Longitude == 0)
                    {
                        try
                        {
                            string address = selected_client.ShipToAddress.Replace('|', ' ');
                            var locations = await Geocoding.GetLocationsAsync(address);
                            var location = locations?.FirstOrDefault();

                            if (location != null)
                                clientLocation = location;
                        }
                        catch (Exception ex)
                        {
                            clientLocation = null;
                        }
                    }
                    else
                    {
                        clientLocation = new Location(selected_client.Latitude, selected_client.Longitude);
                    }

                    if (clientLocation != null)
                    {
                        milesFromClient = Location.CalculateDistance(currentLocation, clientLocation, DistanceUnits.Miles);
                    }
                }

                string milesFromClientString = string.Empty;
                if (milesFromClient != -1)
                {
                    if (milesFromClient < 1)
                        milesFromClientString = "Less than 1 mile away";
                    else
                        milesFromClientString = Math.Round(milesFromClient, Config.Round).ToString() + " miles away";
                }

                return milesFromClientString;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
