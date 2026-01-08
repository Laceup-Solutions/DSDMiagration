using System;
using System.Threading.Tasks;


namespace LaceupMigration
{
    public class ServerHelper
    {
        public const string AuthKey = "3FCA6CD6-8F30-4FB9-AF0E-7D60E20B53C9";
        public const string BaseUrl = "https://lcupsystem.laceupsolutions.com/system";

        public static void CheckIfServerChanged()
        {
            try
            {
                Task.Run(async () =>
                {
                    var server_port_config = await GetIdForServer(Config.IPAddressGateway, Config.Port);

                    Config.IPAddressGateway = server_port_config.Item1;
                    Config.Port = server_port_config.Item2;
                });
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }
        public static async Task<Tuple<string, int>> GetIdForServer(string serverName, int port)
        {
            try
            {
                var http = new System.Net.Http.HttpClient();
                var result = await http.GetAsync($"{BaseUrl}/GetServerById?auth={AuthKey}&id={serverName}&port={port}");

                result.EnsureSuccessStatusCode();

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();

                    var parts = content.Split("\n");

                    return new Tuple<string, int>(parts[0], Convert.ToInt32(parts[1]));
                }
                else
                    return new Tuple<string, int>(serverName, port);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);

                return new Tuple<string, int>(serverName, port);
            }
        }

        public static async Task<string> GetIdForServer(string serverName)
        {
            try
            {
                var http = new System.Net.Http.HttpClient();
                var result = await http.GetAsync($"{BaseUrl}/GetServerById?auth={AuthKey}&id={serverName}");

                result.EnsureSuccessStatusCode();

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    return content;
                }
                else
                    return serverName;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);

                return serverName;
            }
        }

        public static string GetServerNumber(string sName)
        {
            if (string.IsNullOrEmpty(sName))
                return sName;
            
            string serverName = sName;

            switch (serverName.ToLowerInvariant())
            {
                case "app.laceupsolutions.com":
                    serverName = "1";
                    break;
                case "demo.laceupsolutions.com":
                    serverName = "2";
                    break;
                case "server18.laceupsolutions.com":
                    serverName = "3";
                    break;
                case "server21.laceupsolutions.com":
                    serverName = "4";
                    break;
                case "server23.laceupsolutions.com":
                    serverName = "5";
                    break;
                case "westshore.laceupsolutions.com":
                    serverName = "6";
                    break;
                case "sandbox.laceupsolutions.com":
                    serverName = "7";
                    break;
                case "server25.laceupsolutions.com":
                    serverName = "8";
                    break;
                case "server33.laceupsolutions.com":
                    serverName = "9";
                    break;
                case "server34.laceupsolutions.com":
                    serverName = "10";
                    break;
                case "office.laceupsolutions.com":
                    serverName = "20";
                    break;
            }

            return serverName;
        }
    }
}