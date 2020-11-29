using System.Dynamic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Crip.AspNetCore.Logging.Example.Web
{
    public class NamedHttpClient
    {
        private readonly HttpClient _client;

        public NamedHttpClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<object> Post()
        {
            var model = new
            {
                bar1 = "bazz1",
                bar2 = "bazz2",
            };

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

            var response = await _client.PostAsync("post?foo1=bar1&foo2=bar2", content);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ExpandoObject>(body);
        }
    }
}