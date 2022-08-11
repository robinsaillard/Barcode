using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using TNTServices.ServiceReference;

namespace BarcodeReader.Services.Carriers
{
    class Tnt
    {
        ServicesClient client;

        public string Username { get; set; }
        public string Password { get; set; }
        public string Account { get; set; }
        public Tnt(string username, string password, string account)
        {
            Username = username;
            Password = password;
            Account = account;
        }
        public void ExpeditionCreation(
            string customerRef,
            string adresse1,
            string adresse2,
            string city, 
            string zipCode,
            string firstname, 
            string lastname,
            string email,
            string phoneNumber,
            int quantity,
            double weight,
            string serviceCode = "J",
            string labelFormat = "ZPL,DPI_200",
            string accessCode = "",
            string floorNumber = "",
            string buldingId = "",
            string instructions = "",
            string sendNotification = "0",
            string nameSociete = "",
            string saturdayDelivery = "0",
            adresseType adresseTypeSender = adresseType.ENTERPRISE, 
            string codeSenderPex = "",
            adresseType adresseTypeReceiver = adresseType.INDIVIDUAL,
            string codeReceiverPexXett = "",
            option? optionPriority = null
        )
        {

            client = ConnexionSoap(); 
            expeditionCreationParameter parameters = new expeditionCreationParameter();
            parcelRequest[] listParcels = new parcelRequest[quantity];
            for (int i = 0; i < quantity; i++)
            {
                double weightParcel = Math.Round((weight / quantity), 2);
                parcelRequest parcelRequest = new parcelRequest()
                {
                    customerReference = customerRef,
                    weight = weightParcel.ToString().Replace(",", "."),
                    sequenceNumber = (i + 1).ToString(),
                    insuranceAmount = "",
                    priorityGuaranteeSpecified = false,
                    comment = "",
                };
                if (optionPriority != null)
                {
                    parcelRequest.priorityGuarantee = (option)optionPriority; // PTY --> option prioritaire || GUE --> option guarantee
                }
                listParcels[i] = parcelRequest;
            }
           
            parcelsRequest parcelsRequest = new parcelsRequest()
            {
                parcelRequest = listParcels
            };

            parameters.parcelsRequest = parcelsRequest;
            parameters.accountNumber = Account;
            parameters.labelFormat = labelFormat;
            sender2 sender = new sender2()
            {
                address1 = "13 Allée de cindais",
                address2 = "",
                city = "Saint André sur Orne",
                zipCode = "14320",
                contactFirstName = "Arnaud",
                contactLastName = "Françoise",
                emailAddress = "afrancoise@capska.fr",
                name = "CAPSKA",
                faxNumber = "",
                phoneNumber = "0231532070",
                type = adresseTypeSender
            };
            if (adresseTypeSender == adresseType.DEPOT)
            {
                sender.typeId = codeSenderPex;
            }
            parameters.sender = sender;
            parameters.shippingDate = DateTime.Now.ToString("yyyy-MM-dd");

            receiver2 receiver = new receiver2()
            {
                address1 = adresse1,
                address2 = adresse2,
                type = adresseTypeReceiver,
                name = nameSociete != "" ?  nameSociete : firstname + " " + lastname,
                zipCode = zipCode,
                city = city,
                contactFirstName = firstname,
                contactLastName = lastname,
                accessCode = accessCode,
                floorNumber = floorNumber,
                buldingId = buldingId,
                instructions = instructions,
                phoneNumber = phoneNumber,
                sendNotification = sendNotification,
                emailAddress = email
            };
            if (adresseTypeReceiver == adresseType.DEPOT || adresseTypeReceiver == adresseType.DROPOFFPOINT)
            {
                receiver.typeId = codeReceiverPexXett;
            }
            parameters.receiver = receiver;
            parameters.serviceCode = serviceCode;
            parameters.quantity = parcelsRequest.parcelRequest.Length.ToString();
            parameters.saturdayDelivery = saturdayDelivery; //livraison le samedi

            var response = client.expeditionCreation(parameters);
            var test = response.PDFLabels;
            string s = Encoding.UTF8.GetString(test, 0, test.Length);
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
                writer.Write(s);
                writer.Flush();

                // Close Connection
                writer.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                var coucou2 = "";
            }
            var coucou = "";

        }

        public ServicesClient ConnexionSoap()
        {

            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportWithMessageCredential);
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            // Rest of your binding configuration comes here

            // Custom binding to have access to more configuration details of basic binding
            CustomBinding customBinding = new CustomBinding(binding);
            SecurityBindingElement element = customBinding.Elements.Find<SecurityBindingElement>();
            // Remove security timestamp because it is not used by your original binding
            element.IncludeTimestamp = false;

            EndpointAddress address = new EndpointAddress("https://www.tnt.fr/service/");

            ServicesClient client = new ServicesClient(customBinding, address);
            client.ClientCredentials.UserName.UserName = Username;
            client.ClientCredentials.UserName.Password = Password;
            return client;
        }
    }
}
