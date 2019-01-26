using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;

namespace GenLibraryList
{
    internal class GithubPackageGenerator
    {
        private static readonly GitHubClient _githubClient = new GitHubClient(new ProductHeaderValue("GenLibraryList"));

        static GithubPackageGenerator()
        {
            try
            {
                // アクセストークンが存在すれば利用する。
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var configFile = Path.Combine(homeDir, "GenLibraryList.config");
                var config = JsonConvert.DeserializeObject<GenLibraryListConfig>(File.ReadAllText(configFile));

                _githubClient.Credentials = new Credentials(config.GithubAccessToken);
            }
            catch
            {
                // ignored
            }
        }

        internal static async Task<Library> MakeAsync(string url)
        {
            var parts = url.Split("/");

            var owner = parts[parts.Length - 2];
            var name = parts[parts.Length - 1];

            var repos = await _githubClient.Repository.Get(owner, name).ConfigureAwait(false);

            string ownerName = null;
            {
                try
                {
                    var ownerUser = await _githubClient.User.Get(owner).ConfigureAwait(false);
                    ownerName = ownerUser.Name;
                }
                catch
                {
                    // ignored
                }
            }

            return new Library
            {
                Name = repos.Name,
                Author = ownerName ?? owner,
                Summary = repos.Description,
                Url = url
            };
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class GenLibraryListConfig
        {
            public string GithubAccessToken { get; set; }
        }
    }
}