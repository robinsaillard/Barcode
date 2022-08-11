using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BarcodeReader.Services.Carriers
{
    class LabelTntInternational
    {
        public Dictionary<int, string> Label = new Dictionary<int, string>();
        public XDocument Xml; 
        public LabelTntInternational(XDocument xml)
        {
            Xml = xml;  
        }
        
        public void AddLabel(int id)
        {
            Label[id] = " " +
                "^XA" +
                "^FO51,51^GC43,5,B^FS" +            // LOGO
                "^FO90,51^GC43,5,B^FS" +
                "^FO130,51^GC43,5,B^FS" +
                "^FO56,62^A0N,30,30^FD T ^FS" +
                "^FO93,62^A0N,30,30^FD N ^FS" +
                "^FO135,62^A0N,30,30^FD T ^FS" +
                "^FO20,20^GB770,1180,2^FS" +        // Border
                "^FO20,20^GB770,100,2^FS" +
                "^FO20,20^GB180,100,2^FS" +
                "^FO555,20^GB120,100,100^FS" +
                "^FO20,118^GB770,150,2^FS" +
                "^FO405,118^GB385,150,150^FS" +
                "^FO20,266^GB385,100,2^FS" +
                "^FO405,360^CF0,10^FD-------------------------------------------^FS" +
                "^FO405,556^CF0,10^FD-------------------------------------------^FS" +
                "^FO20,500^CF0,10^FD-------------------------------------------^FS" +
                "^FO420,580^CF0,20^FDSort^FS" +
                "^FO405,680^CF0,10^FD-------------------------------------------^FS" +
                "^FO20,364^GB385,320,2^FS" +
                "^FO20,682^GB385,60,2^FS" +
                "^FO220,682^GB185,60,60^FS" +
                "^FO30,750^CF0,20^FDDelivery instructions:^FS" +
                "^FO20,740^GB770,35,2^FS";        
        }

        public Dictionary<int, string> CreateLabel()
        {
            if (Xml != null)
            {
                
                XElement consignment = Xml.Element("labelResponse").Element("consignment");
                //XElement[] pieceLabelData = Xml.Descendants("consignmentLabelData");
                XElement labelData = consignment.Element("consignmentLabelData");
                string consignmentNumber = labelData.Element("consignmentNumber").Value;
                XElement sender = labelData.Element("sender");
                XElement delivery = labelData.Element("delivery");
                XElement account = labelData.Element("account");
                string totalNumberOfPieces = labelData.Element("totalNumberOfPieces").Value;
                string product = labelData.Element("product").Value;
                IEnumerable<XElement> options = labelData.Elements("option");
                string collectionDate = labelData.Element("collectionDate").Value;
                string marketDisplay = labelData.Element("marketDisplay").Value;
                string transportDisplay = labelData.Element("transportDisplay").Value;
                string freeCirculationDisplay = labelData.Element("freeCirculationDisplay").Value;
                string sortSplitText = labelData.Element("sortSplitText").Value;
                string xrayDisplay = labelData.Element("xrayDisplay").Value;
                string depotCode = labelData.Element("originDepot").Element("depotCode").Value;
               // IEnumerable<XElement> transitDepots = consignment.Elements("transitDepots");
                XElement destinationDepot = labelData.Element("destinationDepot");
                string microzone = labelData.Element("microzone").Value;
                string clusterCode = labelData.Element("clusterCode").Value;
                string legalComments = labelData.Element("legalComments").Value;
                string cashAmount = labelData.Element("cashAmount").Value;
                string specialInstructions = labelData.Element("specialInstructions").Value;
                string bulkShipment = labelData.Element("bulkShipment").Value;
                GenerateLabel(consignment, consignmentNumber, sender, delivery, account, product, options, collectionDate, marketDisplay, transportDisplay,
                    freeCirculationDisplay, sortSplitText, xrayDisplay, depotCode, destinationDepot, microzone, clusterCode, legalComments, 
                    cashAmount, specialInstructions, bulkShipment, totalNumberOfPieces);
                
            }
            return Label;
        }

        public void GenerateLabel(XElement consignment, string consignmentNumber, XElement sender, XElement delivery,
            XElement account, string product, IEnumerable<XElement> options, string collectionDate, string marketDisplay, string transportDisplay,
            string freeCirculationDisplay, string sortSplitText, string xrayDisplay, string depotCode,
            XElement destinationDepot, string microzone, string clusterCode, string legalComments, string cashAmount, 
            string specialInstructions, string bulkShipment, string totalNumberOfPieces
            )
        {
            string accountNumberDetail = account.Element("accountNumber").Value;
            DateTime date = DateTime.ParseExact(collectionDate, "yyyy-MM-dd", CultureInfo.GetCultureInfo("en-US"));
            string depotDest = destinationDepot.Element("depotCode").Value;
            string dayDepot = destinationDepot.Element("dueDayOfMonth").Value;
            string postCode = clusterCode;
            if (clusterCode.Length > 3)
            {
                postCode = delivery.Element("postcode").Value;
            }
            int i = 0;
            string optionsText = "";
            foreach(XElement option in options)
            {
                optionsText += " " + option.Attribute("id").Value;
            }
            foreach (XElement piece in consignment.Descendants("pieceLabelData"))
            {
                string pieceNumber = piece.Element("pieceNumber").Value;
                string weightDisplay = piece.Element("weightDisplay").Value;
                string pieceReference = piece.Element("pieceReference").Value;
                string barcode = piece.Element("barcode").Value;
                string twoDBarcode = piece.Element("twoDBarcode").Value;
                AddLabel(i);
                Label[i] +=
                    "^FO120,840^BY3" +  // CODE BARRE
                    "^BCN,200,Y,N,N,A" +
                    "^FD" + barcode + "^FS" +
                    "^FO200,35^CF0,25^FD "+ marketDisplay  + "/"+ transportDisplay + "^FS" +
                    "^FO570,42^FR^A0N,75,75^FD "+ freeCirculationDisplay + " ^FS" +
                    "^FO690,42^A0N,75,75^FD " + sortSplitText + " ^FS" +
                    "^FO30,125^CF0,20^FDCon No. ^FS" +
                    "^FO30,155^CF0,30^FD" + consignmentNumber + "^FS" +
                    "^FO30,195^CF0,20^FDPiece^FS" +
                    "^FO30,225^CF0,30^FD"+ (i + 1).ToString() +" of "+ totalNumberOfPieces  + " ^FS" +
                    "^FO220,195^CF0,20^FDWeight^FS" +
                    "^FO220,225^CF0,30^FD"+ weightDisplay + "^FS" +
                    "^FO420,125^FR^CF0,20^FDService^FS" +
                    "^FO420,155^FR^CF0,25^FD" + product + "^FS" +
                    "^FO420,195^FR^CF0,20^FDOptions^FS" + 
                    "^FO420,225^FR^CF0,25^FD" + optionsText + "^FS" +
                    "^FO30,275^CF0,20^FDCustomer Reference^FS" +
                    "^FO30,302^CF0,20^FD" + pieceReference + "^FS" +
                    "^FO30,330^CF0,20^FDS/R Account No^FS" +
                    "^FO250,330^FR^CF0,25^FD" + accountNumberDetail  + "^FS" +
                    "^FO420,285^CF0,20^FDOrigin^FS" +
                    "^FO520,285^FR^CF0,45^FD" + depotCode + "^FS" +
                    "^FO620,285^CF0,20^FDPickup Date^FS" +
                    "^FO620,315^CF0,20^FD" + date.ToString("dd MMM yyyy", CultureInfo.GetCultureInfo("en-US")) + "^FS" +
                    "^FO420,385^CF0,20^FDRouting^FS" +
                    SetRouting(consignment) +
                    "^FO30,695^CF0,20^FDPostcode /^FS" +
                    "^FO30,715^CF0,20^FDCluster Code^FS" +
                    "^FO278,696^FR^A0N,40,40^FD " + postCode + " ^FS" +
                    "^FO420,695^CF0,20^FDDest^FS" +
                    "^FO420,715^CF0,20^FDDepot^FS" +
                    "^FO520,696^A0N,40,40^FD "+ depotDest +"-" + dayDepot + " ^FS"
                    ;

                Label[i] += SetSender(sender);
                Label[i] += SetDelivery(delivery);
                bool hazardous = false;
                if(hazardous)
                {
                    Label[i] += "^FO250,65^GB200,40,40^FS" +
                        "^FO260,73^FR^CF0,30^FD HAZARDOUS ^FS";
                }
                Label[i] += "^XZ";
               i++;

            }
        }

       public string SetSender(XElement sender)
        {
            string name = sender.Element("name").Value;
            string addressLine1 = sender.Element("addressLine1").Value;
            string addressLine2 = sender.Element("addressLine2").Value;
            string town = sender.Element("town").Value;
            string province = sender.Element("province").Value;
            string postcode = sender.Element("postcode").Value;
            string country = sender.Element("country").Value;
            string resultat = "^FO30,375^CF0,20^FDSender^FS" +
                "^FO30,400^CF0,20^FD" + name + "^FS" +
                "^FO30,425^CF0,20^FD" + addressLine1 + "^FS" +
                "^FO30,450^CF0,20^FD" + town + " "+ postcode + "^FS" +
                "^FO30,475^CF0,20^FD" + country + "^FS";

            return resultat;

        }

        public string SetDelivery(XElement delivery)
        {
            string name = delivery.Element("name").Value;
            string addressLine1 = delivery.Element("addressLine1").Value;
            string addressLine2 = delivery.Element("addressLine2").Value;
            string town = delivery.Element("town").Value;
            string province = delivery.Element("province").Value;
            string phone = "";
            string postcode = delivery.Element("postcode").Value;
            string country = delivery.Element("country").Value;
            string resultat = "^FO30,520^CF0,20^FDReceiver^FS" +
                "^FO30,545^CF0,20^FD" + name + "^FS" +
                "^FO30,570^CF0,20^FD" + phone + "^FS" +
                "^FO30,595^CF0,20^FD" + addressLine1 + "^FS" +
                "^FO30,620^CF0,20^FD" + town + " " + postcode + "^FS" +
                "^FO30,645^CF0,20^FD" + country + "^FS";

            return resultat;
        }

        public string SetRouting(XElement consignment)
        {
            string resultat = "";
            IEnumerable<XElement> depotCodes = consignment.Element("consignmentLabelData").Element("transitDepots").Descendants("depotCode");
            int nbDepotCodes = depotCodes.Count();
            int heightBloc = 180;
            int heightLetter = heightBloc / nbDepotCodes;
            int start = 375;
            foreach (XElement depot in depotCodes)
            {
                string depotValue = depot.Value;
                resultat += "^FO520," + start + "^FR^CF0,"+heightLetter+","+ (heightLetter + 10) + "^FD" + depotValue + "^FS";
                start += heightLetter;
            }

            return resultat;
        }



    }
}
