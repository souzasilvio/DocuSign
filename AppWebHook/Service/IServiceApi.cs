using DocuSign.eSign.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppWebHook.Service
{
    public interface IServiceApi
    {
        string AccountID { get; }
        ApiClient ApiClient { get; }
    }
}
