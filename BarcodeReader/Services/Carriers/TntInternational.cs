using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;


namespace BarcodeReader.Services.Carriers
{
    class TntInternational
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Account { get; set; }

        public TntInternational(string username, string password, string account)
        {
            Username = username;
            Password = password;
            Account = account;
        }

        public void CreateLabel()
        {
            string accessCodeReply;
            string accessCode;
            string resultReply = "";
            string connoteReply;
            try
            {
                string xml_in = Consignement(2, 10.0, 2.0);
                accessCodeReply = CallWebservice(xml_in);
                if (accessCodeReply.StartsWith("COMPLETE:"))
                {
                    accessCode = accessCodeReply.Substring(9);
                }
                else
                {
                    throw new ArgumentException(string.Format("Webservice call ended in an error: {0}", accessCodeReply));
                }
                resultReply = CallWebservice(string.Format("GET_RESULT:{0}", accessCode));
             
            }
            catch (System.Exception ex)
            {
                //write exception message to console
                Console.WriteLine(ex.Message.ToString());
                if (ex.InnerException.Message != string.Empty)
                {
                    Console.WriteLine(ex.InnerException.Message.ToString());
                }
            }
            if(resultReply != "")
            {
                XDocument xmlDoc = XDocument.Load(new StringReader(resultReply));
                string connumber = xmlDoc.Element("document").Element("CREATE").Element("CONNUMBER").Value;
                string resultString = Regex.Match(connumber, @"\d+").Value;
                var xml_label = GetXml(2, resultString, "REF-TEST");

                var xml_wb = ConnectWebService(Username, Password, xml_label);
                //GetHtmlRender(xml_wb);
                XDocument labelResponseXml = XDocument.Parse(xml_wb);
                LabelTntInternational label = new LabelTntInternational(labelResponseXml);
                Dictionary<int, string> labelTab = label.CreateLabel();
                var asString = string.Join(Environment.NewLine, labelTab.Select(x => x.Value));


                string ipAddress = "192.168.1.156";
                int port = 9100;
                try
                {
                    // Open connection
                    TcpClient client = new TcpClient();
                    client.Connect(ipAddress, port);

                    // Write ZPL String to connection
                    System.IO.StreamWriter writer =
                    new System.IO.StreamWriter(client.GetStream());
                    writer.Write(asString);
                    writer.Flush();

                    // Close Connection
                    writer.Close();
                    client.Close();
                }
                catch (Exception ex)
                {
                    var coucou2 = "";
                }

            }
           
        }


        private static string CallWebservice(string body)
        {
            string serverUrl = "https://express.tnt.com/expressconnect/shipping/ship";

            //string serverUrl = "https://iconnection.tnt.com/shippergate2.asp";
            try
            {
                //Call Express Connect
                using (var client = new HttpClient())
                {
                    //define dictionary to send body "xml_in=<..."
                    var values = new Dictionary<string, string>
                            {
                            {"xml_in", body}
                            };
                    //urlencode content
                    var content = new FormUrlEncodedContent(values);
                    //call webservice
                    var result = client.PostAsync(serverUrl, content).Result;
                    return result.Content.ReadAsStringAsync().Result;
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public static string ConnectWebService(string username, string password, string xml)
        {

            string url = "https://express.tnt.com/expresslabel/documentation/getlabel";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.ContentType = "text/xml";

                request.Method = "POST";
                request.Accept = "*/*";
                var encodedStr = Convert.ToBase64String(Encoding.Default.GetBytes(string.Format("{0}:{1}", username, password)));
                var authorizationKey = "Basic" + " " + encodedStr;    // Note: Basic case sensitive
                request.Headers.Add("Authorization", authorizationKey);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                 {
                     streamWriter.Write(xml);
                 }
                
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return streamReader.ReadToEnd();
                }
                   
            } catch (System.Exception)
            {
                throw;
            }
            return null;
        }


        public string GetXml(int quantity, string conNumber, string reference)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
           

            XElement[] consignmentIdentityTab =
            {
                new XElement("consignmentNumber", new XCData(conNumber)),
                new XElement("customerReference", new XCData(reference))
            };
            XElement consignmentIdentity = new XElement("consignmentIdentity", consignmentIdentityTab.Select(x => x));
            XElement collectionDateTime = new XElement("collectionDateTime", new XCData(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")));
            XElement[] senderTab =
            {
                new XElement("name", new XCData(Username)),
                new XElement("addressLine1", new XCData("Allée de Cindais")),
                new XElement("addressLine2", new XCData("")),
                new XElement("addressLine3", new XCData("")),
                new XElement("town", new XCData("ST ANDRE SUR ORNE")),
                new XElement("exactMatch", new XCData("")),
                new XElement("province", new XCData("")),
                new XElement("postcode", new XCData("14320")),
                new XElement("country", new XCData("FR")),
            };
            
            XElement[] deliveryTab =
            {
                new XElement("name", new XCData("Robin Saillard")),
                new XElement("addressLine1", new XCData("Reeperbahn 32")),
                new XElement("addressLine2", new XCData("")),
                new XElement("addressLine3", new XCData("")),
                new XElement("town", new XCData("Ahlbeck")),
                new XElement("exactMatch", new XCData("Y")),
                new XElement("province", new XCData("")),
                new XElement("postcode", new XCData("17375")),
                new XElement("country", new XCData("DE")),
            };
            XElement[] products =
                        {
                new XElement("lineOfBusiness", new XCData("2")),
                new XElement("groupId", new XCData("0")),
                new XElement("subGroupId", new XCData("0")),
                new XElement("id", "EX"),
                new XElement("type", new XCData("N")),
                new XElement("option", new XCData("PR")),
            };
            XElement[] accounts =
                        {
                new XElement("accountNumber", new XCData(Account)),
                new XElement("accountCountry", new XCData("FR")),
            };
            XElement sender = new XElement("sender", senderTab.Select(x => x));
            XElement delivery = new XElement("delivery", deliveryTab.Select(x => x));
            XElement product = new XElement("product", products.Select(x => x));
            XElement account = new XElement("account", accounts.Select(x => x));
            XElement bulkShipment = new XElement("bulkShipment", new XCData("Y")); //optional
            XElement specialInstructions = new XElement("specialInstructions", "Test"); //optional
            XElement cashAmount = new XElement("cashAmount", ""); //optional
            XElement cashCurrency = new XElement("cashCurrency", "GBP"); //optional
            XElement cashType = new XElement("cashType", "0"); //optional
            XElement customControlled = new XElement("customControlled", new XCData("Y")); //optional
            XElement termsOfPayment = new XElement("termsOfPayment", new XCData("S")); //optional
            XElement totalNumberOfPieces = new XElement("totalNumberOfPieces", new XCData(quantity.ToString()));
          
            XElement[] pieceLines =
                       {
                new XElement("identifier", new XCData("1")),
                new XElement("goodsDescription", "piecelinegoods desc"),
            };
            
            XElement[] pieceMeasurementsTab =
                       {
                new XElement("length", new XCData("1.11")),
                new XElement("width", new XCData("1.11")),
                new XElement("height", new XCData("1.11")),
                new XElement("weight", new XCData("1.11")),
            };
            XElement pieceLine = new XElement("pieceLine", pieceLines.Select(x => x));
            XElement pieceMeasurements = new XElement("pieceMeasurements", pieceMeasurementsTab.Select(x => x));
            pieceLine.Add(pieceMeasurements);
            for (int i = 0; i < quantity; i++)
            {
                XElement[] pieces =
                {
                    new XElement("sequenceNumbers", new XCData((i + 1).ToString())),
                    new XElement("pieceReference", new XCData("ref" + (i + 1).ToString())),
                };
                XElement piece = new XElement("pieces", pieces);
                pieceLine.Add(piece);
            }
           
            XElement consignment = new XElement("consignment", consignmentIdentity);
            consignment.SetAttributeValue("key", "CON1");
            consignment.Add(collectionDateTime);
            consignment.Add(sender);
            consignment.Add(delivery);
            consignment.Add(product);
            consignment.Add(account);
            consignment.Add(bulkShipment);
            consignment.Add(specialInstructions);
            consignment.Add(cashAmount);
            consignment.Add(cashCurrency);
            consignment.Add(cashType);
            consignment.Add(customControlled);
            consignment.Add(termsOfPayment);
            consignment.Add(totalNumberOfPieces);
            consignment.Add(pieceLine);
            XElement labelRequest = new XElement("labelRequest", consignment);
            doc.Add(labelRequest);
            return doc.ToString();  
        }

        public string Consignement(int quantity, double weight, double volume)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            XElement[] logins =
            {
                new XElement("COMPANY", Username),
                new XElement("PASSWORD", Password),
                new XElement("APPID", "EC"),
                new XElement("APPVERSION", "3.1"),
            };
            XElement login = new XElement("LOGIN", logins.Select(x => x));

            XElement sender = Address("SENDER", Username, "Allée de cindais", "", "", "ST ANDRE SUR ORNE", "", "14320", "FR", "Arnaud Françoise", 
                "0033", "231532070", "afrancois@capska.fr", "", Account);
            XElement collectionAddress = Address("COLLECTIONADDRESS", Username, "Allée de cindais", "", "", "ST ANDRE SUR ORNE", "", "14320", "FR", "Arnaud Françoise",
                "0033", "231532070", "afrancois@capska.fr");

            TimeSpan time = new TimeSpan(14, 0, 0);
            TimeSpan now = DateTime.Now.TimeOfDay;
            string date = DateTime.Now.ToString("dd/MM/yyyy");
            if (now > time)
            {
                DateTime today = DateTime.Now;
                today = today.AddDays(1);
                date = today.ToString("dd/MM/yyyy");
            }
            

            XElement shipdate = new XElement("SHIPDATE", new XCData(date));
            XElement prefcollecttime = DateFromTo("PREFCOLLECTTIME", "16:00", "18:00");
            XElement altcollecttime = DateFromTo("ALTCOLLECTTIME", "", "");

            XElement collinstructions = new XElement("COLLINSTRUCTIONS", new XCData("use rear gate"));

            XElement collection = new XElement("COLLECTION", collectionAddress);
            collection.Add(shipdate);
            collection.Add(prefcollecttime);
            collection.Add(altcollecttime);
            collection.Add(collinstructions);

            sender.Add(collection);

            XElement conref = new XElement("CONREF", new XCData("REF-TEST"));
            XElement receiver = Address("RECEIVER", "Robin Saillard", "Reeperbahn 32", "", "", "Ahlbeck", "", "17375", "DE", "Robin Saillard",
                "0033", "233399838", "robin.saillard@hotmail.fr");
            XElement delivery = Address("DELIVERY", "Robin Saillard", "Reeperbahn 32", "", "", "Ahlbeck", "", "17375", "DE", "Robin Saillard",
                "0033", "233399838", "robin.saillard@hotmail.fr");
            XElement connumber = new XElement("CONNUMBER", new XCData(""));
            XElement customerref = new XElement("CUSTOMERREF", new XCData("REF-TEST"));
            XElement contype = new XElement("CONTYPE", new XCData("N"));
            XElement paymentind = new XElement("PAYMENTIND", new XCData(""));
            XElement items = new XElement("ITEMS", new XCData(quantity.ToString()));
            XElement totalWeight = new XElement("TOTALWEIGHT", new XCData(weight.ToString()));
            XElement totalVolume = new XElement("TOTALVOLUME", new XCData(volume.ToString()));
            XElement currency = new XElement("CURRENCY", new XCData(""));
            XElement goodsValue = new XElement("GOODSVALUE", new XCData(""));
            XElement hazardous = new XElement("HAZARDOUS", new XCData("Y"));
            XElement unnumber = new XElement("UNNUMBER", new XCData("1845"));
            XElement packinggroup = new XElement("PACKINGGROUP", new XCData("II"));

            XElement insuranceValue = new XElement("INSURANCEVALUE", new XCData(""));
            XElement insuranceCurrency = new XElement("INSURANCECURRENCY", new XCData(""));
            XElement service = new XElement("SERVICE", new XCData("48N"));
            XElement option = new XElement("OPTION", new XCData(""));
            XElement description = new XElement("DESCRIPTION", new XCData("description test"));
            XElement deliveryInst = new XElement("DELIVERYINST", new XCData("description test"));


            XElement detail = new XElement("DETAILS", receiver);
            detail.Add(delivery);
            detail.Add(connumber);
            detail.Add(customerref);
            detail.Add(contype);
            detail.Add(paymentind);
            detail.Add(items);
            detail.Add(totalWeight);
            detail.Add(totalVolume);
            detail.Add(currency);
            detail.Add(goodsValue);
            detail.Add(insuranceValue);
            detail.Add(insuranceCurrency);
            detail.Add(service);
            detail.Add(option);
            detail.Add(description);
            detail.Add(deliveryInst);
            //---- TEST HAZARDOUS ----
           // detail.Add(unnumber);
           // detail.Add(packinggroup);
           // detail.Add(hazardous);

            for (int i = 0; i < quantity; i++)
            {
                XElement[] packages =
                 {
                    new XElement("ITEMS", new XCData((i + 1).ToString())),
                    new XElement("DESCRIPTION", new XCData("Produit desc")),
                    new XElement("LENGTH", new XCData("0.6")),
                    new XElement("HEIGHT", new XCData("0.4")),
                    new XElement("WIDTH", new XCData("0.5")),
                    new XElement("WEIGHT", new XCData("10")),

                };
                XElement package = new XElement("PACKAGE", packages.Select(x => x));
                detail.Add(package);
            }

            XElement consignement = new XElement("CONSIGNMENT", conref);
            consignement.Add(detail);
            XElement activity = GetActivity("REF-TEST", "", "");

            XElement consignementBatch = new XElement("CONSIGNMENTBATCH", sender);
            consignementBatch.Add(consignement);
            
            XElement eshipper = new XElement("ESHIPPER", login);
            eshipper.Add(consignementBatch);
            eshipper.Add(activity);
            doc.Add(eshipper);
            return doc.ToString();
        }

        public XElement Address(string elementName, string compagny, string address1, string address2, string address3, string city, string province, string postcode, string country,
            string name, string dialCode, string phone, string mail, string vat = "",string account = "")
        {
            XElement[] address =
           {
                new XElement("COMPANYNAME", new XCData(compagny)),
                new XElement("STREETADDRESS1", new XCData(address1)),
                new XElement("STREETADDRESS2", new XCData(address2)),
                new XElement("STREETADDRESS3", new XCData(address3)),
                new XElement("CITY", new XCData(city)),
                new XElement("PROVINCE", new XCData(province)),
                new XElement("POSTCODE", new XCData(postcode)),
                new XElement("COUNTRY", new XCData(country)),
                
            };
            XElement addressRes = new XElement(elementName, address.Select(x => x));
            if (account != "")
            {
                addressRes.Add(new XElement("ACCOUNT", new XCData(account)));
            }
            addressRes.Add(new XElement("VAT", new XCData(vat)));
            addressRes.Add(new XElement("CONTACTNAME", new XCData(name)));
            addressRes.Add(new XElement("CONTACTDIALCODE", new XCData(dialCode)));
            addressRes.Add(new XElement("CONTACTTELEPHONE", new XCData(phone)));
            addressRes.Add(new XElement("CONTACTEMAIL", new XCData(mail)));
            return addressRes;
        }

        public XElement DateFromTo(string elementName, string from, string to)
        {
            XElement[] date =
           {
                new XElement("FROM", new XCData(from)),
                new XElement("TO", new XCData(to)),
            };
            return new XElement(elementName, date.Select(x => x));
        }

        public XElement GetActivity(string reference, string emailTo, string emailFrom)
        {
            XElement[] conref =
            {
                new XElement("CONREF", new XCData(reference)),
            };
            XElement create = new XElement("CREATE", conref.Select(x => x));
            XElement book = new XElement("BOOK", conref.Select(x => x));
            book.SetAttributeValue("ShowBookingRef", "Y");
            XElement ship = new XElement("SHIP", conref.Select(x => x));

            XElement connote = new XElement("CONNOTE", conref.Select(x => x));
            XElement label = new XElement("LABEL", conref.Select(x => x));
            XElement manifest = new XElement("MANIFEST", conref.Select(x => x));
            XElement invoice = new XElement("INVOICE", conref.Select(x => x));

            XElement email_to = new XElement("EMAILTO", new XCData(emailTo));
            XElement email_from = new XElement("EMAILFROM", new XCData(emailFrom));

            XElement print = new XElement("PRINT", connote);
            
            print.Add(label);
            print.Add(manifest);
            print.Add(invoice);
            print.Add(email_to);
            print.Add(email_from);

            XElement activity = new XElement("ACTIVITY", create);
            activity.Add(book);
            activity.Add(ship);
            activity.Add(print);
            return activity;
        }
        public string GetHtmlRender(string xml)
        {
            using (StringReader sri = new StringReader(xml)) // xmlInput is a string that contains xml
            {

                using (XmlReader xri = XmlReader.Create(sri))
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load("./Ressources/tnt/HTMLRoutingLabelRenderer.xsl");
                    using (StringWriter sw = new StringWriter())
                    using (XmlWriter xwo = XmlWriter.Create(sw, xslt.OutputSettings)) // use OutputSettings of xsl, so it can be output as HTML
                    {
                        xslt.Transform(xri, xwo);
                        return sw.ToString();
                    }
                }
            }
        }
    }
}
