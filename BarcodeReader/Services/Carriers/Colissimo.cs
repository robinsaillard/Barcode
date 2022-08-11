using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TNTServices.ServiceReference;

namespace BarcodeReader.Services.Carriers
{
    class Colissimo
    {
        public string Account { get; set; }
        public string Password { get; set; }

        public Colissimo(string account, string password)
        {
            Password = password;
            Account = account;
        }

        public void Execute()
        {
            var url = "https://ws.colissimo.fr/sls-ws/SlsServiceWSRest/2.0/generateLabel";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.Accept = "*/*";
            httpRequest.ContentType = "application/json";

            JObject json  = new JObject();
            json["contractNumber"] = Account;
            json["password"] = Password;
            JObject outputFormat = new JObject();
            outputFormat["x"] = 0;
            outputFormat["y"] = 0;
            outputFormat["outputPrintingType"] = "ZPL_10x15_203dpi";
            json["outputFormat"] = outputFormat;

            //service
            JObject service = new JObject();
            service["productCode"] = "DOM";
            service["depositDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            service["orderNumber"] = "REF TEST";
            service["commercialName"] = "CAPSKA";

            JObject parcel = new JObject();
            parcel["weight"] = 3;

            //sender
            JObject address1 = new JObject();
            
            address1["companyName"] = "companyName";
            address1["line0"] = "line0";
            address1["line1"] = "line1";
            address1["line2"] = "13 Allée de cindais";
            address1["line3"] = "line3";
            address1["countryCode"] = "FR";
            address1["city"] = "Saint André Sur Orne";
            address1["zipCode"] = "14320";

            JObject sender = new JObject();
            sender["senderParcelRef"] = "REF TEST";
            sender["address"] = address1;

            //receiver
            JObject address2 = new JObject();

            address2["lastName"] = "Saillard";
            address2["firstName"] = "Robin";
            address2["line1"] = "line1";
            address2["line2"] = "5 promenade des squares";
            address2["line3"] = "line3";
            address2["countryCode"] = "FR";
            address2["city"] = "Hérouville Saint clair";
            address2["zipCode"] = "14200";
            address2["mobileNumber"] = "0624506933";
            address2["email"] = "robin.saillard@hotmail.fr";

            JObject receveir = new JObject();
            receveir["address"] = address2;
            //letter
            JObject letter = new JObject();
            letter["service"] = service;
            letter["parcel"] = parcel;
            letter["sender"] = sender;
            letter["addressee"] = receveir;

            json["letter"] = letter;

            var data = JsonConvert.SerializeObject(json);

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                string start = "\u0010CT~~CD,~CC^~CT~";
                string end = "^XZ";
                var resultat = Between(result, start, end);

                string jStart = "{\"messages\":";
                string jEnd = "\"value\":\"\"}]}}}";
                var jsonRes = Between(result, jStart, jEnd);

                var jsonTxt = JObject.Parse(jsonRes);
                var labelV2Response = jsonTxt.GetValue("labelV2Response");
                var parcelNumber = labelV2Response["parcelNumber"].ToString();
                var test = "";
            }

            Console.WriteLine(httpResponse.StatusCode);

        }

        public string Between(string STR, string FirstString, string LastString)
        {
            string FinalString;
            int Pos1 = STR.IndexOf(FirstString);
            int Pos2 = STR.IndexOf(LastString) + LastString.Length;
            FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            return FinalString;
        }
    }
}
