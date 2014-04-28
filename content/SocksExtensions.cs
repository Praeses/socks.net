using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Web.Script.Serialization;


namespace System
{

    public static partial class SocksExtensions
    {


        public static ActionResult Pdf(this System.Web.Mvc.Controller controller, string view, PdfSettings settings = null)
        {
            return Pdf(controller, view, null, settings: settings);
        }


        public static ActionResult Pdf(this System.Web.Mvc.Controller controller, string view, Object model, PdfSettings settings = null)
        {
            var data = new Dictionary<string, string>();

            if (settings == null) settings = new PdfSettings();
            if (settings.Landscape) data.Add("orientation", "Landscape");

            var html = controller.RenderViewToString(view, model, settings);

            Stream pdf = toPdf( html, settings );
            //use reflection to call protected method :(
            var obj = controller.GetType().InvokeMember("File"
                , (System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                , Type.DefaultBinder
                , controller
                , new object[] { pdf, @"application/pdf" });

            return (ActionResult)obj;
        }





        public static string RenderViewToString(this System.Web.Mvc.Controller controller, string partialPath, object model, PdfSettings settings)
        {
            if (string.IsNullOrEmpty(partialPath))
                partialPath = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                string layoutPath = GetLayout(controller, partialPath);
                var viewResult = ViewEngines.Engines.FindView(controller.ControllerContext, partialPath, layoutPath);
                if (viewResult.View == null) return "";
                ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                foreach (var item in viewContext.Controller.ViewData.ModelState)
                    if (!viewContext.ViewData.ModelState.Keys.Contains(item.Key))
                        viewContext.ViewData.ModelState.Add(item);

                viewResult.View.Render(viewContext, sw);

                var html = sw.GetStringBuilder().ToString();
                html = InlineCss(html);
                html = InlineJs(html);
                html = InlineImg(html);
                html = IncludeSockStyles(html);
                html = IncludeSockJavascript(html, settings);
                return html;
            }
        }


        private static string InlineCss(string html)
        {
            Match match = null;
            var rx = new Regex(@"<link[^>]*href=""([^h][^t][^t][^p][^""]*.css)""[^>]*>", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            match = rx.Match(html);
            while ( match.Success )
            {
                var path = HttpContext.Current.Server.MapPath(match.Groups[1].ToString());
                var content = File.ReadAllText(path);
                html = html.Replace(match.ToString(), string.Format(@"<style>{0}</style>", content));
                match = rx.Match(html);
            }
            return html;
        }

        private static string InlineJs(string html)
        {
            Match match = null;
            var rx = new Regex(@"<scipt[^>]*src=""([^""]*.js)""[^>]*>", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            match = rx.Match(html);
            while (match.Success)
            {
                var path = HttpContext.Current.Server.MapPath(match.Groups[1].ToString());
                var content = File.ReadAllText(path);
                html = html.Replace(match.ToString(), string.Format(@"<script>{0}</script>", content));
                match = rx.Match(html);
            }
            return html;
        }


        private static string InlineImg(string html)
        {
            Match match = null;
            var rx = new Regex(@"<img[^>]*src=""([^d][^a][^t][^a][^:][^""]*)""[^>]*>", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            match = rx.Match(html);
            while (match.Success)
            {
                var path = HttpContext.Current.Server.MapPath(match.Groups[1].ToString());
                var content =  Convert.ToBase64String(File.ReadAllBytes(path));
                html = html.Replace(match.ToString(), string.Format(@"<img src=""data:image/gif;base64,{0}"" />", content));
                match = rx.Match(html);
            }
            return html;
        }

        private static string IncludeSockStyles(string html)
        {
            Match match = null;
            var rx = new Regex(@"< *head *>", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            match = rx.Match(html);
            if (match.Success)
            {
                var path = tools_path() + "socks.css";
                var content = "<style>" + File.ReadAllText(path) + "</style>";
                html = html.Replace(match.ToString(), match.ToString() + content);
            }
            return html;
        }

        private static string IncludeSockJavascript(string html, PdfSettings settings)
        {
            string pdf_settings = new JavaScriptSerializer().Serialize(settings);
            var path = tools_path() + "socks.js";
            var content = "<script type='text/javascript'>";
            if (!settings.Landscape) content += "var pdf_settings=" + pdf_settings + ";";
            content += File.ReadAllText(path) + "</script>";
            return html + content;
        }





        private static string GetLayout(System.Web.Mvc.Controller controller, string partialPath)
        {
            try
            {
                var view_context  = ViewEngines.Engines.FindView(controller.ControllerContext, partialPath, null);
                var view = (System.Web.Mvc.RazorView)view_context.View;
                var path = HttpContext.Current.Server.MapPath(view.ViewPath);
                var content = File.ReadAllText(path);
                var rx =  new Regex("Layout\\s*=\\s*\"[^\"]*\"", RegexOptions.IgnoreCase);
                var match = rx.Match(content);
                return match.ToString().Split('\"')[1];
            }
            catch
            { return "~/Views/Shared/_Layout.cshtml"; } //default layout
        }




        
        private static string tools_path(){
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../packages");
            path = Directory.GetDirectories(path).Where(x => x.Contains("Socks.net"))
                .Select(x => new DirectoryInfo(x)).OrderBy(x => x.LastWriteTime).Last().FullName;
            path = Path.Combine(path, "./tools/");
            return path;
        } 

        //calls the lib to do the convert
        private static Stream toPdf(string html, PdfSettings settings)
        {
            var temp = Path.GetTempFileName();
            var source = temp + ".html";
            var desc = temp + ".pdf";
            try
            {
                File.WriteAllText(source, html);
                var args = BuildArgs(source, desc, settings);

                ProcessStartInfo psi = new ProcessStartInfo(tools_path() + "wkhtmltopdf.exe", string.Join(" ", args))
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                Process process = Process.Start(psi);
                process.WaitForExit();

                return new MemoryStream(File.ReadAllBytes(desc));
            }
            finally {
                if (File.Exists(source)) File.Delete(source);
                if (File.Exists(desc)) File.Delete(desc);
            }
        }



        private static List<string> BuildArgs(string source, string desc, PdfSettings settings)
        {
            var args = new List<string>();
            //NOTE: header and footers cannot be suppered using header-html 
            // untill wkhtmltopdf is patched  :(
            //if( has_header) args.Add("--header-html " + header);
            //if (has_footer) args.Add("--footer-html " + footer);
            args.Add("--page-height " + settings.PageHeight + "in");
            args.Add("--page-width " + settings.PageWidth + "in" );
            //if (settings.PageSize != null) args.Add("--page-size " + settings.PageSize);
            args.Add("--margin-left 0"); //done in css
            args.Add("--margin-right 0"); //done in css
            args.Add("--margin-top 0"); //done in css
            args.Add("--margin-bottom 0"); //done in css
            if (settings.Landscape) args.Add("--orientation Landscape" );
            args.Add(source);
            args.Add(desc);
            return args;
        }

    }



    public class PdfSettings
    {
        public string MarginLeft = "0.75in";
        public string MarginRight = "0.75in";
        public string MarginTop = "0.75in";
        public string MarginBottom = "0.75in";
        public bool Landscape = false;
        //public string PageSize = null; //Letter, Legal, A1, ...
        public decimal PageWidth = 8.5m;
        public decimal PageHeight = 11m;

        public string MarginAll { set {
            MarginLeft = MarginRight = MarginTop = MarginBottom = value;
        } }
    }

}
