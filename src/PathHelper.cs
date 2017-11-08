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
        private readonly string temp_path = null;

        private PathHelper() {
            temp_path = Path.GetTempPath();
            this.ExtractAssets();
        }

        ~PathHelper()
        {
            if (Directory.Exists(temp_path)) {
                Directory.Delete(temp_path, true);
            }
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
            if (source.StartsWith("~/")) source = source.Remove(0, 2);
            if (source.StartsWith("/")) source = source.Remove(0, 1);
            
            var webRoot = HostingEnvironment.WebRootPath;
            var path = System.IO.Path.Combine(webRoot, source);
            if (File.Exists(path)) return path;

            //try without wwwroot if file not found
            path = System.IO.Path.Combine(webRoot, "..");
            path = System.IO.Path.Combine(path, source);

            return path;
        }



        public string tools_path()
        {
            return temp_path;
        }




        private static string[] ASSETS = new string[]{
            "jquery-1.12.4.min.js",
            "screen.css",
            "socks.css",
            "socks.js",
            "wkhtmltopdf.exe"
        };

        private void ExtractAssets() {
            foreach (var asset in ASSETS) {
                ExtractAsset(asset);
            }
        }

        private void ExtractAsset(string asset)
        {
            var path = Path.Combine(tools_path(), asset);
            if (File.Exists(path) == true)
            {
                File.Delete(path);
            }
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Socks.Core.tools." + asset;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (FileStream fs = new FileStream(path, FileMode.Create))
                stream.CopyTo(fs);
        }


    }



}
