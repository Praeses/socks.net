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

            string headerHtml = controller.RenderViewToString(view + ".header", model);
            string footerHtml = controller.RenderViewToString(view + ".footer", model);

            var html = controller.RenderViewToString(view, model);

            Stream pdf = toPdf( html, headerHtml, footerHtml, settings );
            //use reflection to call protected method :(
            var obj = controller.GetType().InvokeMember("File"
                , (System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                , Type.DefaultBinder
                , controller
                , new object[] { pdf, @"application/pdf" });

            return (ActionResult)obj;
        }



        public static string RenderViewToString(this System.Web.Mvc.Controller controller, string partialPath, object model)
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
                return InlineJs(InlineCss(html));
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

        
        private static string wkhtml2pdf_path(){
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../packages");
            path = Directory.GetDirectories(path).Where(x => x.Contains("Socks.net")).Last();
            path = Path.Combine(path, "./content/wkhtmltopdf.exe");
            return path;
        } 

        //calls the lib to do the convert
        private static Stream toPdf(string html, string header, string footer, PdfSettings settings)
        {
            var source = Path.GetTempFileName() + ".html";
            var desc = Path.GetTempFileName() + ".pdf";
            try
            {
                File.WriteAllText(source, html);
                var args = BuildArgs(source, desc, settings);

                ProcessStartInfo psi = new ProcessStartInfo(wkhtml2pdf_path(), string.Join(" ", args))
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
            if (settings.PageHeight != null) args.Add("--page-height " + settings.PageHeight);
            if (settings.PageWidth != null) args.Add("--page-width " + settings.PageWidth);
            if (settings.PageSize != null) args.Add("--page-size " + settings.PageSize);
            if (settings.dpi > 0) args.Add("--dpi " + settings.dpi);
            if (settings.MarginLeft != null) args.Add("--margin-left " + settings.MarginLeft);
            if (settings.MarginRight != null) args.Add("--margin-right " + settings.MarginRight);
            if (settings.MarginTop != null) args.Add("--margin-top " + settings.MarginTop);
            if (settings.MarginBottom != null) args.Add("--margin-bottom " + settings.MarginBottom);
            if (settings.Landscape) args.Add("--orientation Landscape" );
            args.Add(source);
            args.Add(desc);
            return args;
        }

    }



    public class PdfSettings
    {
        public int dpi = 0;
        public string MarginLeft = null;
        public string MarginRight = null;
        public string MarginTop = null;
        public string MarginBottom = null;
        public bool Landscape = false;
        public string PageSize = null; //Letter, Legal, A1, ...
        public int? PageHeight = null;
        public int? PageWidth = null;

        public string MarginAll { set {
            MarginLeft = MarginRight = MarginTop = MarginBottom = value;
        } }
    }

}
