using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;

namespace AppEnvio
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var apiClient = new ApiClient();

                var urlwebhook = System.Configuration.ConfigurationManager.AppSettings["WebHookUrl"];
                Console.WriteLine("\nSending an envelope with one document. This takes about 15 seconds...");
                EnvelopeSummary result = new SendEnvelope(apiClient).Send(urlwebhook);
                Console.WriteLine("\nDone. Envelope status: {0}. Envelope ID: {1}", result.Status, result.EnvelopeId);

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
    }
}
