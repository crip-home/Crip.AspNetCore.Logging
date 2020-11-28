using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Crip.AspNetCore.Tests
{
    public class TestHttpHandler : DelegatingHandler
    {
        private readonly HttpResponseMessage _returnMessage;

        public TestHttpHandler(HttpResponseMessage returnMessage)
        {
            _returnMessage = returnMessage;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken ct)
        {
            return Task.Factory.StartNew(() => _returnMessage, ct);
        }
    }
}
