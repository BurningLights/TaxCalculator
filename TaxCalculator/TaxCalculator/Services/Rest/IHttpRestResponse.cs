using System.Threading.Tasks;

namespace TaxCalculator.Services.Rest
{
    internal interface IHttpRestResponse
    {
        int StatusCode { get; }
        bool IsSuccess { get; }
        string CodeReaseon { get; }


        Task<string> GetBodyAsync();
    }
}
