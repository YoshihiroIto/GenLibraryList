using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Mono.Options;

namespace GenLibraryList
{
    class Program
    {
        // ReSharper disable once AsyncConverter.AsyncMethodNamingHighlighting
        public static async Task Main(string[] args)
        {
            var slnFilePath = "";

            var options = new OptionSet
            {
                { "sln=", "Visual Studio solution file.", v => slnFilePath = v }
            };

            options.Parse(args);

            var nugetIds = MakeNugetPackageIds(slnFilePath);

            var i = await Generator.MakeNugetPackageAsync("Mono.Options").ConfigureAwait(false);


        }


        private static IEnumerable<string> MakeNugetPackageIds(string slnFilePath)
        {


        }
    }
}
