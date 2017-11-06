using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Socksnet
{


    internal class PathHelper
    {

        private PathHelper() {
            _instance.ExtractAssets();
        }

        private static PathHelper _instance;

        public static PathHelper Instance { get
            {
                if (_instance == null) _instance = new PathHelper();               
                return _instance;
            }
        }

        public IHostingEnvironment HostingEnvironment { get;set; }



        public string BuildPath(string source)
        {
            var webRoot = HostingEnvironment.WebRootPath;
            if (source.StartsWith("~/")) source = source.Remove(0, 2);
            if (source.StartsWith("/")) source = source.Remove(0, 1);
            var path = System.IO.Path.Combine(webRoot, source);
            return path;
        }



        public string tools_path()
        {
            //from nuget
            var from_nuget = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ThirdPartyDLLs/Socks.Core/");
            if (Directory.Exists(from_nuget)) return from_nuget;

            //from included project
            var from_project = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools/");
            if (Directory.Exists(from_project)) return from_project;

            return AppDomain.CurrentDomain.BaseDirectory;
        }




        private static string[] ASSETS = new string[]{
            "jquery-1.12.4.min.js",
            "screen.css",
            "socks.css",
            "socks.js",
            "socks.min.js",
            "wkhtmltopdf.exe"
        };

        private void ExtractAssets() {
            var asset_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ThirdPartyDLLs/Socks.Core/");
            if (!Directory.Exists(asset_path)) {
                Directory.CreateDirectory(asset_path);
            }
            foreach (var asset in ASSETS) {
                ExtractAsset(asset_path, asset);
            }
        }

        private void ExtractAsset( string asset_path, string asset)
        {
            var path = Path.Combine(asset_path, asset);
            if (File.Exists(path) == false) {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Socks.Core.tools." + asset;

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (FileStream fs = new FileStream(path, FileMode.Create) )
                        stream.CopyTo(fs);
            }

        }


    }



}
