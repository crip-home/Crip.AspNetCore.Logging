using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Crip.AspNetCore.Tests
{
    public static class TestExtensions
    {
        public static string LoadResource(this object src, string resource)
        {
            var asm = src.GetType().Assembly;
            var name =
                asm.GetManifestResourceNames().First(n => n.Contains(resource)) ??
                throw new Exception($"Could not find {resource} manifest file.");

            using Stream stream = asm.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static T LoadJsonResource<T>(this object src, string resource)
            => JsonConvert.DeserializeObject<T>(src.LoadResource(resource));
    }
}