using System.Threading.Tasks;

namespace TaxCalculator.Rest
{
    internal interface IHttpRestResponse
    {
        int StatusCode { get; }
        bool IsSuccess { get; }
        string CodeReason { get; }


        Task<string> GetBodyAsync();
    }
}
