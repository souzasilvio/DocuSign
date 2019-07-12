using System;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System.Text;
using System.Collections.Generic;

namespace AppEnvio
{
    /// <summary>
    /// Send an envelope with a signer and cc recipient; with three docs:
    /// an HTML, a Word, and a PDF doc.
    /// Anchor text positioning is used for the fields.
    /// </summary>
    internal class SendEnvelope : ExampleBase
    {
        private const String DOC_2_DOCX = "World_Wide_Corp_Battle_Plan_Trafalgar.docx";
        private const String DOC_3_PDF = "World_Wide_Corp_lorem.pdf";

        public static string ENVELOPE_1_DOCUMENT_1
        {
            get => "<!DOCTYPE html>" +
            "<html>" +
            "    <head>" +
            "      <meta charset=\"UTF-8\">" +
            "    </head>" +
            "    <body style=\"font-family:sans-serif;margin-left:2em;\">" +
            "    <h1 style=\"font-family: 'Trebuchet MS', Helvetica, sans-serif;" +
            "         color: darkblue;margin-bottom: 0;\">World Wide Corp</h1>" +
            "    <h2 style=\"font-family: 'Trebuchet MS', Helvetica, sans-serif;" +
            "         margin-top: 0px;margin-bottom: 3.5em;font-size: 1em;" +
            "         color: darkblue;\">Order Processing Division</h2>" +
            "  <h4>Ordered by " + DSConfig.Signer1Name + "</h4>" +
            "    <p style=\"margin-top:0em; margin-bottom:0em;\">Email: " + DSConfig.Signer1Email + "</p>" +
            "    <p style=\"margin-top:0em; margin-bottom:0em;\">Copy to: " + DSConfig.Cc1Name + ", " + DSConfig.Cc1Email + "</p>" +
            "    <p style=\"margin-top:3em;\">" +
            "  Candy bonbon pastry jujubes lollipop wafer biscuit biscuit. Topping brownie sesame snaps" +
            " sweet roll pie. Croissant danish biscuit soufflé caramels jujubes jelly. Dragée danish caramels lemon" +
            " drops dragée. Gummi bears cupcake biscuit tiramisu sugar plum pastry." +
            " Dragée gummies applicake pudding liquorice. Donut jujubes oat cake jelly-o. Dessert bear claw chocolate" +
            " cake gummies lollipop sugar plum ice cream gummies cheesecake." +
            "    </p>" +
            "    <!-- Note the anchor tag for the signature field is in white. -->" +
            "    <h3 style=\"margin-top:3em;\">Agreed: <span style=\"color:white;\">**signature_1**</span></h3>" +
            "    </body>" +
            "</html>";
        }

        /// <summary>
        /// This class create and send envelope
        /// </summary>
        /// <param name="apiClient"></param>
        public SendEnvelope(ApiClient apiClient) : base(apiClient)
        {
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        internal EnvelopeSummary Send(string webhookurl, string opcao)
        {
            CheckToken();

            EnvelopeDefinition envelope = this.CreateEvelope(opcao);
            //Adiciona a configuração do webhook no envelope
            if(!string.IsNullOrEmpty(webhookurl))
            {
                AdicionaWebHook(envelope, webhookurl);
            }
            EnvelopesApi envelopeApi = new EnvelopesApi(ApiClient.Configuration);
            EnvelopeSummary results = envelopeApi.CreateEnvelope(AccountID, envelope);            
            return results;
        }
        /// <summary>
        /// This method creates the envelope request body 
        /// </summary>
        /// <returns></returns>
        private EnvelopeDefinition CreateEvelope(string opcao)
        {
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
            {
                EmailSubject = "POC MRV - Favor assinar o documento"
            };
            //define um guid para identificador do documento
            var contratoid = Guid.NewGuid();
            Document doc3 = CreateDocumentFromTemplate("1", $"contrato_{contratoid}", "pdf",
                DSHelper.ReadContent(DOC_3_PDF));

            // The order in the docs array determines the order in the envelope
            envelopeDefinition.Documents = new List<Document>() { doc3 };
            //Se testando modo de Visualização
            if (opcao == "2")
            {
                Console.WriteLine("Documento será enviado para cliente e ele precisa apenas visualizar...");
                var destinaratios = CreateDelivery(DSConfig.Signer1Email, DSConfig.Signer1Name, 1, 1);
                envelopeDefinition.Recipients = new Recipients
                {
                    CertifiedDeliveries = new List<CertifiedDelivery>() { destinaratios }
                };
                envelopeDefinition.Status = "sent";
                return envelopeDefinition;
            }
            else
            {

                // create a signer recipient to sign the document, identified by name and email
                // We're setting the parameters via the object creation
                Signer signer1 = CreateSigner(DSConfig.Signer1Email, DSConfig.Signer1Name, 1, 1);
                Signer signer2 = CreateSigner(DSConfig.Signer2Email, DSConfig.Signer2Name, 2, 1);
                //Signer signer3 = CreateSigner(DSConfig.Signer3Email, DSConfig.Signer3Name, 3, 2);

                // routingOrder (lower means earlier) determines the order of deliveries
                // to the recipients. Parallel routing order is supported by using the
                // same integer as the order for two or more recipients.

                // create a cc recipient to receive a copy of the documents, identified by name and email
                // We're setting the parameters via setters
                //CarbonCopy cc1 = CreateCarbonCopy();
                // Create signHere fields (also known as tabs) on the documents,
                // We're using anchor (autoPlace) positioning
                //
                // The DocuSign platform seaches throughout your envelope's
                // documents for matching anchor strings. So the
                // sign_here_2 tab will be used in both document 2 and 3 since they
                // use the same anchor string for their "signer 1" tabs.
                SignHere signHere1 = CreateSignHere("**signature_1**", "pixels", "20", "10");
                SignHere signHere2 = CreateSignHere("**signature_2**", "pixels", "60", "10");
                //SignHere signHere3 = CreateSignHere("**signature_3**", "pixels", "100", "10");

                // Tabs are set per recipient / signer
                SetSignerTabs(signer1, signHere1);
                SetSignerTabs(signer2, signHere2);
                //SetSignerTabs(signer3, signHere3);
                // Add the recipients to the envelope object
                Recipients recipients = CreateRecipients(new List<Signer>() { signer1, signer2 });
                envelopeDefinition.Recipients = recipients;
                // Request that the envelope be sent by setting |status| to "sent".
                // To request that the envelope be created as a draft, set to "created"
                envelopeDefinition.Status = "sent";

                return envelopeDefinition;
            }
        }
        /// <summary>
        /// This method creates Recipients instance and populates its signers and carbon copies
        /// </summary>
        /// <param name="signer">Signer instance</param>
        /// <param name="cc">CarbonCopy array</param>
        /// <returns></returns>
        private Recipients CreateRecipients(List<Signer> signers, params CarbonCopy[] cc)
        {
            Recipients recipients = new Recipients
            {
                Signers = signers,
                CarbonCopies = new List<CarbonCopy>(cc)
            };

            return recipients;
        }
        /// <summary>
        /// This method create Tabs
        /// </summary>
        /// <param name="signer">Signer instance to be set tabs</param>
        /// <param name="signers">SignHere array</param>
        private void SetSignerTabs(Signer signer, params SignHere[] signers)
        {
            signer.Tabs = new Tabs
            {
                SignHereTabs = new List<SignHere>(signers)
            };
        }
        /// <summary>
        /// This method create SignHere anchor
        /// </summary>
        /// <param name="anchorPattern">anchor pattern</param>
        /// <param name="anchorUnits">anchor units</param>
        /// <param name="anchorXOffset">anchor x offset</param>
        /// <param name="anchorYOffset">anchor y offset</param>
        /// <returns></returns>
        private SignHere CreateSignHere(String anchorPattern, String anchorUnits, String anchorXOffset, String anchorYOffset)
        {
            return new SignHere()
            {
                AnchorString = anchorPattern,
                AnchorUnits = anchorUnits,
                AnchorXOffset = anchorXOffset,
                AnchorYOffset = anchorYOffset
            };
        }
        /// <summary>
        /// This method creates CarbonCopy instance and populate its members
        /// </summary>
        /// <returns>CarbonCopy instance</returns>
        private CarbonCopy CreateCarbonCopy()
        {
            return new CarbonCopy
            {
                Email = DSConfig.Cc1Email,
                Name = DSConfig.Cc1Name,
                RoutingOrder = "2",
                RecipientId = "2"
            };
        }
        /// <summary>
        /// This method creates Signer instance and populates its members
        /// </summary>
        /// <returns>Signer instance</returns>
        private Signer CreateSigner(string email, string nome, int recipientId, int RoutingOrder)
        {
            return new Signer
            {
                Email = email,
                Name = nome,
                RecipientId = $"{recipientId}",
                RoutingOrder = $"{RoutingOrder}"
            };
        }

        /// <summary>
        /// Cria um destinarario que precisa apenas receber e visualizar o documento
        /// </summary>
        /// <param name="email"></param>
        /// <param name="nome"></param>
        /// <param name="recipientId"></param>
        /// <param name="RoutingOrder"></param>
        /// <returns></returns>
        private CertifiedDelivery CreateDelivery(string email, string nome, int recipientId, int RoutingOrder)
        {
            return new CertifiedDelivery
            {
                Email = email,
                Name = nome,
                RecipientId = $"{recipientId}",
                RoutingOrder = $"{RoutingOrder}"
            };
        }
        /// <summary>
        /// This method create document from byte array template
        /// </summary>
        /// <param name="documentId">document id</param>
        /// <param name="fileName">file name</param>
        /// <param name="fileExtension">file extention</param>
        /// <param name="templateContent">file content byte array</param>
        /// <returns></returns>
        private Document CreateDocumentFromTemplate(String documentId, String fileName, String fileExtension, byte[] templateContent)
        {
            Document document = new Document();

            String base64Content = Convert.ToBase64String(templateContent);

            document.DocumentBase64 = base64Content;
            // can be different from actual file name
            document.Name = fileName;
            // Source data format. Signed docs are always pdf.
            document.FileExtension = fileExtension;
            // a label used to reference the doc
            document.DocumentId = documentId;

            return document;
        }


        private void AdicionaWebHook(EnvelopeDefinition envelope, string webhook_url)
        {
            //Vide melhores praticas em https://www.docusign.com/blog/dsdev-webhook-listeners-part-1/
            var envelope_events = new List<EnvelopeEvent>();

            //EnvelopeEvent envelope_event1 = new EnvelopeEvent();
            //envelope_event1.EnvelopeEventStatusCode = "sent";
            //envelope_events.Add(envelope_event1);

            //EnvelopeEvent envelope_event2 = new EnvelopeEvent();
            //envelope_event2.EnvelopeEventStatusCode = "delivered";
            //envelope_events.Add(envelope_event2);

            EnvelopeEvent envelope_event3 = new EnvelopeEvent();
            envelope_event3.EnvelopeEventStatusCode = "completed";
            envelope_events.Add(envelope_event3);

            EnvelopeEvent envelope_event4 = new EnvelopeEvent();
            envelope_event4.EnvelopeEventStatusCode = "declined";
            envelope_events.Add(envelope_event4);

            EnvelopeEvent envelope_event5 = new EnvelopeEvent();
            envelope_event5.EnvelopeEventStatusCode = "voided";
            envelope_events.Add(envelope_event5);

            List<RecipientEvent> recipient_events = new List<RecipientEvent>();
            //RecipientEvent recipient_event1 = new RecipientEvent();
            //recipient_event1.RecipientEventStatusCode = "Sent";

            //recipient_events.Add(recipient_event1);
            //RecipientEvent recipient_event2 = new RecipientEvent();
            //recipient_event2.RecipientEventStatusCode = "Delivered";
            //recipient_events.Add(recipient_event2);

            RecipientEvent recipient_event3 = new RecipientEvent();
            recipient_event3.RecipientEventStatusCode = "Completed";
            recipient_events.Add(recipient_event3);

            RecipientEvent recipient_event4 = new RecipientEvent();
            recipient_event4.RecipientEventStatusCode = "Declined";
            recipient_events.Add(recipient_event4);

            //RecipientEvent recipient_event5 = new RecipientEvent();
            //recipient_event5.RecipientEventStatusCode = "AuthenticationFailed";
            //recipient_events.Add(recipient_event5);

            //RecipientEvent recipient_event6 = new RecipientEvent();
            //recipient_event6.RecipientEventStatusCode = "AutoResponded";
            //recipient_events.Add(recipient_event6);

            EventNotification event_notification = new EventNotification();
            event_notification.Url = webhook_url;
            event_notification.LoggingEnabled = "true";
            event_notification.RequireAcknowledgment = "true";
            event_notification.UseSoapInterface = "false";
            event_notification.IncludeCertificateWithSoap = "false";
            event_notification.SignMessageWithX509Cert = "false";

            ///False para servico baixar doc na DocuSign
            event_notification.IncludeDocuments = "false";

            event_notification.IncludeEnvelopeVoidReason = "true";
            event_notification.IncludeTimeZone = "true";
            event_notification.IncludeSenderAccountAsCustomField = "true";
            event_notification.IncludeDocumentFields = "false";
            event_notification.IncludeCertificateOfCompletion = "false";
            event_notification.EnvelopeEvents = envelope_events;
            event_notification.RecipientEvents = recipient_events;

            envelope.EventNotification = event_notification;
        }
    }
}
