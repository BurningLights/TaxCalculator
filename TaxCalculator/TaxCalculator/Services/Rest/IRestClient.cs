using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Services.Http
{
    internal interface IRestClient
    {
        T JsonPostRequestResponse<T>(string uri, object requestBody, IDictionary<string, string> headers = null);
        T JsonGetRequestResponse<T>(string uri, object requestParameters = null, IDictionary<string, string> headers = null);
    }
}
