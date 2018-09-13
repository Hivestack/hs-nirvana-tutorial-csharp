using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace hsnirvanaapitutorial
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hivestack - nirvana sample calls");

            //http client
            HttpClient client = new HttpClient();

            // screen UUID is the unique ID (a GUID) that corresponds to each screen or player 
            string screenUUID = System.Configuration.ConfigurationManager.AppSettings["Screen UUID"];

            string environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string baseURL = System.Configuration.ConfigurationManager.AppSettings["Base URL"];

            // Get all possible creatives the screen might play in the next hour - call this once every hour
            // Download and cache the creatives from the url
            // This will be used in PlayCreative() to check if the creative has to be downloaded again or if it has been already cached.
            var getCreativeToCacheURL = String.Format(baseURL + screenUUID + "/creatives", environment);
            var getPossibleCreativesInNextHour = GetAllPossibleCreativesToCache(client, getCreativeToCacheURL).Result;

            // staging is the testing environment
            // Make an adrequest to get which creative to play from adserver
            var adRequestURL = String.Format(baseURL + screenUUID + "/schedulevast", environment);
            var adResponse = AdRequest(client, adRequestURL).Result;

            if (adResponse.Length > 0)
            {
                Console.WriteLine("Parsing Vast");
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(adResponse);

                String creativeFileURL = "";
                XmlNode creativeFileURLNode = doc.DocumentElement.SelectSingleNode("/VAST/Ad/InLine/Creatives/Creative/Linear/MediaFiles/MediaFile");
                if (creativeFileURLNode != null)
                {
                    creativeFileURL = creativeFileURLNode.InnerText.Replace("\n\r", "").Trim();
                    Console.WriteLine("Creative file url - {0} ", creativeFileURL);
                }

                String playConfirmationURL = "";
                XmlNode playConfirmationURLNode = doc.DocumentElement.SelectSingleNode("/VAST/Ad/InLine/Impression");
                if (playConfirmationURLNode != null)
                {
                    playConfirmationURL = playConfirmationURLNode.InnerText.Replace("\n\r", "").Trim();
                    Console.WriteLine("Play confirmation url - {0} ", playConfirmationURL);
                }

                if (creativeFileURL != "" && playConfirmationURL != "")
                {
                    // play the creative and call the confirmation url
                    bool hasPlayed = PlayCreative(creativeFileURL).Result;

                    if (hasPlayed)
                    {
                        var confirmationResponse = ConfirmPlay(client, playConfirmationURL).Result;
                        Console.WriteLine(confirmationResponse);
                    }
                }

            }

            else
            {
                Console.WriteLine("Unsuccessful Response from Adserver");
            }

        }

        static async Task<String> AdRequest(HttpClient client, String adRequestURL)
        {
            Console.WriteLine("Sending Ad Request");

            HttpResponseMessage response = new HttpResponseMessage();
            String responseData = "";
            try
            {
                response = await client.GetAsync(adRequestURL);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured, {0}", e);
            }
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success: AdRequest returned a statuscode, {0}", response.StatusCode.ToString());
                responseData = await response.Content.ReadAsStringAsync();
            }
            else
            {
                Console.WriteLine("Error occured, AdRequest returned a statuscode, {0}", response.StatusCode.ToString());
            }

            return responseData;

        }

        static async Task<bool> PlayCreative(string creativeFileURL)
        {
            Console.WriteLine("Fetching creative");

            // check to see if the creativeFileURL is already cached if not, get the creative from creativeFileURL
            // getCreativeFromURL()
            // InnerPlay()

            Console.WriteLine("Playing creative - 6 seconds simulation");

            // 6000 milliseconds
            await Task.Delay(6000);

            Console.WriteLine("Done Playing creative for 6 seconds");

            return true;
        }

        static async Task<String> ConfirmPlay(HttpClient client, String confirmationURL)
        {

            Console.WriteLine("Sending Play Confirmation");

            HttpResponseMessage response = new HttpResponseMessage();
            String responseData = "";
            try
            {
                response = await client.GetAsync(confirmationURL);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured, {0}", e);
            }
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success: Play Confirmation returned a statuscode, {0}", response.StatusCode.ToString());
                responseData = await response.Content.ReadAsStringAsync();
            }
            else
            {
                Console.WriteLine("Error occured, Play Confirmation returned a statuscode, {0}", response.StatusCode.ToString());
            }

            return responseData;

        }

        static async Task<String> GetAllPossibleCreativesToCache(HttpClient client, String getCreativeToCacheURL)
        {

            Console.WriteLine("Getting all possible creatives available for play");

            HttpResponseMessage response = new HttpResponseMessage();
            String responseData = "";
            try
            {
                response = await client.GetAsync(getCreativeToCacheURL);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured, {0}", e);
            }
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success: Get all possible creatives returned a statuscode, {0}", response.StatusCode.ToString());
                responseData = await response.Content.ReadAsStringAsync();
            }
            else
            {
                Console.WriteLine("Error occured, Get all possible creatives  returned a statuscode, {0}", response.StatusCode.ToString());
            }

            return responseData;

            // convert this responseData string to Json list to get the details
            // Sample data format:
            // [
            //      {
            //          "url":  "https://d2uh8d3nrw9okj.cloudfront.net/6f6ef189-e7c2-49a6-8ac5-f251e320b1b8/creative.jpg",
            //          "creative_properties": {},
            //      },
            //
            //      {
            //          "url": "https://d2uh8d3nrw9okj.cloudfront.net/9b0fb17e-9da3-472b-93fa-82076dd69f36/creative.jpg",
            //          "creative_properties": {},
            //      }
            //
            // ]

        }

    }
}
