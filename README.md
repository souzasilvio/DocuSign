# DocuSign
Projeto para testes DocuSign
* Detalhamento da solução
# AppEnvio
Console para disparo de documento para assinatura para 2 ou 3 signatários.
 - Informe no config do console os e-mails a serem usados no teste.
 - Informe as chaves DS_CLIENT_ID, DS_IMPERSONATED_USER_GUID e DS_PRIVATE_KEY referente a sua conta docusign
 - Informe na chave WebHookUrl a url de seu webhook. Vide projeto exemplo AppWebHook.

# AppWebHook
Esse projeto demonstra um exemplo de implementação de um webhook. 
O webhook e eventos de retorno desejados são configurados no app de envio. Vide metodo AdicionaWebHook na classe AppEnvio.SendEnvelope
Para configurar e publicar essa Api:
 - Informe os parãmetros de conexão docusign em appsettings.json. Issso é necessário para download do documento finalizado. 
 - Informe a key docusign em PrivKey.pem
 - Crie uma conta de storage no azure e informe o string de conexão em  appsettings.json.
 - Crie um blob storage para armazenar os documentos concluidos na docusign e informe o nome dele na chave NomeContainerStorage.
 - Compile e publique sua api no Azure ou em qualquer lugar com endpoint publico. Informe a url no formato https://website/api/contratodigial/atualizastatus na chave AppWebHook do AppEnvio.
 
#Informações adicionais
1. Durante o envio de um envelope pode configurar um webhook e os enventos de envelope e de signatarios que desejamos receber. Observe no metodo AdicionaWebHook do AppEnvio que nesse exemplo deixamos os enventos de completed, declined e comentamos alguns não desejados.
2. Ainda na configuração do WebHook, configuramos nesse exemplo que a docusing não deve documentos pdf na mensagem. Parâmetros IncludeDocuments = false e IncludeCertificateOfCompletion=false. Nosso exemplo de webhook trata documentos enviados na mensagem quando esses parâmetros estão igual a true ou faz o download na docusign que é o cenários recomendado.

#Teste dos Apps de demo.
Após configurados e com o AppWebHook publicado no Azure realize o envio de documentos executando o AppEnvio.
Faça a assinatura do documento e depois verifique as mensagens gravadas no seu blobstorage.
