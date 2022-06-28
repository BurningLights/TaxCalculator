using System.Threading.Tasks;

namespace TaxCalculator.Rest
{
    public interface IHttpRestResponse
    {
        int StatusCode { get; }
        bool IsSuccess { get; }
        string CodeReason { get; }


        Task<string> GetBodyAsync();
    }
}
