using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Mono.Options;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json;

namespace GenLibraryList
{
    class Program
    {
        // ReSharper disable once AsyncConverter.AsyncMethodNamingHighlighting
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var slnFilePath = "";
                var externalList = "";
                var outputFile = "";
                {
                    var options = new OptionSet
                    {
                        {"sln=", "Visual Studio solution file.", v => slnFilePath = v},
                        {"external=", "External libraries url list file.", v => externalList = v},
                        {"output=", "Output file.", v => outputFile = v}
                    };
                    options.Parse(args);
                }

                var libraries = new ConcurrentBag<Library>();
                {
                    if (string.IsNullOrEmpty(externalList) == false && File.Exists(externalList))
                    {
                        var urls = File.ReadAllLines(externalList).Where(x => string.IsNullOrEmpty(x) == false);

                        await urls.ForEachAsync(async x =>
                        {
                            libraries.Add(await GithubPackageGenerator.MakeAsync(x).ConfigureAwait(false));
                        }).ConfigureAwait(false);
                    }

                    {
                        var nugetIds = await MakeNugetPackageIdsAsync(slnFilePath).ConfigureAwait(false);
                        await nugetIds.ForEachAsync(async x =>
                        {
                            libraries.Add(await NugetPackageGenerator.MakeAsync(x).ConfigureAwait(false));
                        }).ConfigureAwait(false);
                    }

                }

                var json = JsonConvert.SerializeObject(libraries.OrderBy(x => x.Name), Formatting.Indented);

                if (string.IsNullOrEmpty(outputFile))
                    Console.WriteLine(json);
                else
                    File.WriteAllText(outputFile, json);

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return 1;
            }
        }

        private static async Task<string[]> MakeNugetPackageIdsAsync(string slnFilePath)
        {
            var packages = new List<string>();

            using (var ws = MSBuildWorkspace.Create())
            {
                var sln = await ws.OpenSolutionAsync(slnFilePath).ConfigureAwait(false);

                foreach (var proj in sln.Projects)
                {
                    // プロジェクトファイルから作る
                    packages.AddRange(
                        XDocument.Load(proj.FilePath)
                            .Descendants("PackageReference")
                            .Select(x => x.Attribute("Include")?.Value));


                    // packages.configファイルから作る
                    {
                        var projDir = Path.GetDirectoryName(proj.FilePath);

                        var packagesConfig = Path.Combine(projDir, "packages.config");

                        if (File.Exists(packagesConfig))
                        {
                            packages.AddRange(
                                XDocument.Load(packagesConfig)
                                    .Descendants("package")
                                    .Select(x => x.Attribute("id")?.Value));
                        }
                    }
                }
            }

            return packages
                .Distinct()
                .ToArray();
        }
    }
}