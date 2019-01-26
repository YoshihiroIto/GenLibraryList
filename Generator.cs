using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GenLibraryList
{
    internal class Generator
    {
        private static readonly HttpClient _client = new HttpClient();

        internal static async Task<Library> MakeNugetPackageAsync(string id)
        {
            var url = $"http://api-v2v3search-0.nuget.org/query?q={id}";

            var jsonText = await _client.GetStringAsync(url).ConfigureAwait(false);
            var json = JsonConvert.DeserializeObject<Rootobject>(jsonText);
            var data = json.data.Single(x => x.id == id);

            return new Library
            {
                Name = data.title,
                Author = string.Join(" ", data.authors),
                Summary = string.IsNullOrEmpty(data.summary) ? data.description : data.summary,
                Url = data.projectUrl
            };
        }

        // ReSharper disable once IdentifierTypo
        public class Rootobject
        {
            public Datum[] data { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public string description { get; set; }
            public string summary { get; set; }
            public string title { get; set; }
            public string projectUrl { get; set; }
            public string[] authors { get; set; }
        }
    }
}