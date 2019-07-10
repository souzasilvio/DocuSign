using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AppWebHook.Helper;
using AppWebHook.Service;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using DocuSign.eSign.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;

namespace AppWebHook.Controllers
{
    [EnableCors("AllowAnyOrigin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ContratoDigitalController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private IConfiguration Config;
        private IServiceApi ApiService;

        public ContratoDigitalController(IHostingEnvironment hostingEnvironment, IConfiguration config, IServiceApi api)
        {
            _hostingEnvironment = hostingEnvironment;
            Config = config;
            ApiService = api;
        }


     
        /// <summary>
        /// Url de webhook a ser chamada pela docusign
        /// </summary>
        [AllowAnonymous]
        [HttpPost("atualizastatus")]
        public void AtualizaStatus()
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(Request.Body);

            var mgr = new XmlNamespaceManager(xmldoc.NameTable);
            mgr.AddNamespace("a", "http://www.docusign.net/API/3.0");

            XmlNode envelopeStatus = xmldoc.SelectSingleNode("//a:EnvelopeStatus", mgr);
            //Statuss dos recipientes
            XmlNodeList recipientStatuses = envelopeStatus.SelectNodes("//a:RecipientStatus", mgr);
            XmlNode envelopeId = envelopeStatus.SelectSingleNode("//a:EnvelopeID", mgr);
            XmlNode statusEvelop = envelopeStatus.SelectSingleNode("./a:Status", mgr);
            //Grava xml de retorno - para tempo de desenvolvimento apenas
            if (envelopeId != null)
            {
                string file = $"{_hostingEnvironment.ContentRootPath}/Documents/" +
                    envelopeId.InnerText + "_" + statusEvelop.InnerText + "_" + Guid.NewGuid() + ".xml";
                System.IO.File.WriteAllText(file, xmldoc.OuterXml);
                StorageHelper.UploadFile(Config, file);
            }

            string signer_name = "";
            string signer_email = "";
            string signer_status = "";
            string signer_RoutingOrder = "";            
            if (recipientStatuses != null && recipientStatuses.Count > 0)
            {
                foreach (XmlNode recipientStatus in recipientStatuses)
                {
                    switch (recipientStatus.FirstChild.InnerText)
                    {

                        case "Signer":
                            signer_name = recipientStatus.SelectSingleNode("//a:UserName", mgr).InnerText;
                            signer_email = recipientStatus.SelectSingleNode("//a:Email", mgr).InnerText;
                            signer_status = recipientStatus.SelectSingleNode("//a:Status", mgr).InnerText;
                            signer_RoutingOrder = recipientStatus.SelectSingleNode("//a:RoutingOrder", mgr).InnerText;
                            //Caso necessário algum tratamento por cada status de signatario, faça aqui.

                            break;                       
                    }
                }
            }

            //Documento completo, baixar o documento e armazenar
            if (statusEvelop.InnerText == "Completed")
            {
                // Loop through the DocumentPDFs element, storing each document.
                //OBS: Nao tera node DocumentPDFs se webhook configurado para nao envia o documento
                //Como boa pratica o serviço deve baixar o documento e armazenar em repositorio interno
                XmlNode docs = xmldoc.SelectSingleNode("//a:DocumentPDFs", mgr);
                if (docs != null)
                {
                    GravaDocumentosRecebidos(docs, envelopeId);
                }
                else
                {
                    //Faz o download de documentos
                    if (envelopeId != null)
                        ObterDocumentos(envelopeId.InnerText);
                }
            }
        }

        private void GravaDocumentosRecebidos(XmlNode docs, XmlNode envelopeId)
        {
            foreach (XmlNode doc in docs.ChildNodes)
            {
                string documentName = doc.ChildNodes[0].InnerText; // pdf.SelectSingleNode("//a:Name", mgr).InnerText;
                string documentId = doc.ChildNodes[2].InnerText; // pdf.SelectSingleNode("//a:DocumentID", mgr).InnerText;
                string byteStr = doc.ChildNodes[1].InnerText; // pdf.SelectSingleNode("//a:PDFBytes", mgr).InnerText;
                string arquivoAssinado = $"{_hostingEnvironment.ContentRootPath}/Documents/" + envelopeId.InnerText + "_" + documentId + "_" + documentName;
                System.IO.File.WriteAllText(arquivoAssinado, byteStr);
                StorageHelper.UploadFile(Config, arquivoAssinado);
            }
        }

        private void ObterDocumentos(string idEnvelope)
        {
            EnvelopesApi envelopeApi = new EnvelopesApi(ApiService.ApiClient.Configuration);
            EnvelopeDocumentsResult results = envelopeApi.ListDocuments(ApiService.AccountID, idEnvelope);
            foreach (EnvelopeDocument doc in results.EnvelopeDocuments)
            {
                System.IO.Stream documentoPdf = envelopeApi.GetDocument(ApiService.AccountID, idEnvelope, doc.DocumentId);
                StorageHelper.UploadFileFromStream(Config, documentoPdf, $"`{idEnvelope}{doc.Name}.pdf");
            }
        }
    }
}
