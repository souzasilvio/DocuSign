using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Security;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace AppEnvio
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                Console.WriteLine("Digite 1 para teste de processo de assinatura.");
                Console.WriteLine("Digite 2 para teste de Visualização (Similar ao antigo comprova) .");
                string opcao = Console.ReadLine();
                var apiClient = new ApiClient();
                var urlwebhook = System.Configuration.ConfigurationManager.AppSettings["WebHookUrl"];
                Console.WriteLine("\nSending an envelope with one document. This takes about 15 seconds...");

                if (opcao == "4")
                {
                    AssinarDocumento();
                }

                if (opcao == "3")
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "DemoDocuSign.pdf");
                    var pdf = DSHelper.ReadContent(path);
                    var arquivo = SendEnvelope.AssinarDocumento(pdf);
                    EnvelopeSummary result = new SendEnvelope(apiClient).Send(urlwebhook, "1", arquivo);

                }
                if (opcao == "1" || opcao == "2")
                {
                    EnvelopeSummary result = new SendEnvelope(apiClient).Send(urlwebhook, opcao);
                    Console.WriteLine("\nDone. Envelope status: {0}. Envelope ID: {1}", result.Status, result.EnvelopeId);
                }
                //Console.WriteLine("\n\nList the envelopes in the account...");
                //EnvelopesInformation envelopesList = new ListEnvelopes(apiClient).List();
                //List<Envelope> envelopes = envelopesList.Envelopes;

                //if (envelopesList != null && envelopes.Count > 2)
                //{
                //    Console.WriteLine("Results for {0} envelopes were returned. Showing the first two: ", envelopes.Count);
                //    envelopesList.Envelopes = new List<Envelope>() {
                //        envelopes[0],
                //        envelopes[1]
                //    };
                //}

                //DSHelper.PrintPrettyJSON(envelopesList);
            }
            catch (ApiException e)
            {
                Console.WriteLine("\nDocuSign Exception!");

                // Special handling for consent_required
                String message = e.Message;
                if (!String.IsNullOrWhiteSpace(message) && message.Contains("consent_required"))
                {
                    String consent_url = String.Format("\n    {0}/oauth/auth?response_type=code&scope={1}&client_id={2}&redirect_uri={3}",
                        DSConfig.AuthenticationURL, DSConfig.PermissionScopes, DSConfig.ClientID, DSConfig.OAuthRedirectURI);

                    Console.WriteLine("C O N S E N T   R E Q U I R E D");
                    Console.WriteLine("Ask the user who will be impersonated to run the following url: ");
                    Console.WriteLine(consent_url);
                    Console.WriteLine("\nIt will ask the user to login and to approve access by your application.");
                    Console.WriteLine("Alternatively, an Administrator can use Organization Administration to");
                    Console.WriteLine("pre-approve one or more users.");
                }
                else
                {
                    Console.WriteLine("    Reason: {0}", e.ErrorCode);
                    Console.WriteLine("    Error Reponse: {0}", e.ErrorContent);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Done. Hit enter to exit...");
            Console.ReadKey();
        }


        public static void AssinarDocumento()
        {
            var arquivoEnt = $"d:\\temp\\modelo.pdf";
            var pdf = DSHelper.ReadContent(arquivoEnt);
            var arquivo = $"d:\\temp\\modeloOut.pdf";
            float x;
            float y;
            Stream pfxStream = File.OpenRead("MRV ENGENHARIA E PARTICIPAÇÕES S.A..pfx");
            //Creates a certificate instance from PFX file with private key.
            PdfCertificate pdfCert = new PdfCertificate(pfxStream, "zzzzz");

            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(pdf);
          
            var lista = new Dictionary<int, List<Syncfusion.Drawing.RectangleF>>();
            loadedDocument.FindText("Assinado:", out lista);
        
            foreach (var item in lista)
            {
                x = item.Value[0].X + 100;
                y = item.Value[0].Y;
                var page = loadedDocument.Pages[item.Key] as PdfLoadedPage;             

                //aplica logo da assinatura em todas as paginas
                if (page != null)
                {
                    Stream seloStream = File.OpenRead("SeloMrv.jpg");
                    PdfBitmap signatureImage = new PdfBitmap(seloStream);
                    PdfGraphics gfx = page.Graphics;
                    gfx.DrawImage(signatureImage, x, y, 90, 80);
                }

                //Applica o certificado somente na ultima pagina
                if (item.Value == lista[lista.Keys.Count - 1])
                {
                    //Creates a signature field.
                    PdfSignatureField signatureField = new PdfSignatureField(page, "AssinaturaMRV");
                    signatureField.Bounds = new Syncfusion.Drawing.RectangleF(x, item.Value[0].Y, 50, 50);
                    signatureField.Signature = new PdfSignature(page, "MRV Engenharia");
                    //Adds certificate to the signature field.
                    signatureField.Signature.Certificate = pdfCert;
                    signatureField.Signature.Reason = "Assinado pela MRV Engenharia";

                    //Adds the field.
                    loadedDocument.Form.Fields.Add(signatureField);
                }
            }
            //Saves the certified PDF document.
            using (FileStream fileOut = new FileStream(arquivo, FileMode.Create))
            {
                loadedDocument.Save(fileOut);
                loadedDocument.Close(true);
            }
            //return arquivo;

        }
    }
}
