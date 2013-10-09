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

        public static ActionResult Pdf(this System.Web.Mvc.Controller controller)
        {
            string caller = (new StackFrame(1)).GetMethod().Name;
            return Pdf(controller, caller, null, null); 
        }

        public static ActionResult Pdf(this System.Web.Mvc.Controller controller, string view, PdfSettings settings = null)
        {
            return Pdf(controller, view, null, settings: settings);
        }

        public static ActionResult Pdf(this System.Web.Mvc.Controller controller, Object model, PdfSettings settings = null)
        {
            string caller = (new StackFrame(1)).GetMethod().Name;
            return Pdf(controller, caller, model, settings: settings); 
        }

        public static ActionResult Pdf(this System.Web.Mvc.Controller controller, string view, Object model, PdfSettings settings = null)
        {
            var data = new Dictionary<string, string>();

            if (settings == null) settings = new PdfSettings();
            if (settings.Landscape) data.Add("orientation", "Landscape");

            string headerHtml = "";
            //will return "" if there is no header view
            headerHtml = controller.RenderViewToString(view + ".header", model);

            var html = controller.RenderViewToString(view, model);
            html = System.Web.HttpUtility.UrlEncode(html);
            headerHtml = System.Web.HttpUtility.UrlEncode(headerHtml);
            data.Add("data-html", html);
            if (headerHtml != "") { data.Add("data-header-html", headerHtml); }
            data.Add("dpi", settings.dpi.ToString() );
            data.Add("margin-left", settings.MarginLeft);
            data.Add("margin-right", settings.MarginRight);
            data.Add("margin-top", settings.MarginTop);
            data.Add("margin-bottom", settings.MarginBottom);
            if(settings.PageSize != null) data.Add("page-size", settings.PageSize);
            if(settings.PageWidth != null) data.Add("page-width", settings.PageWidth.ToString() );
            if(settings.PageHeight != null) data.Add("page-height", settings.PageHeight.ToString() );

            var options = String.Join("&", data.Select(x => x.Key + "=" + x.Value));
            var generator = @"http://socksjs.herokuapp.com/generate_report";
            var pdf = HtmlPost.Send(generator, options).ToStream();

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


        /// <summary>
        /// Reads the full content of a steam into a byte array
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] ReadFully(this Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);
                return ms.ToArray();
            }
        }

    }


    public class HtmlPost
    {
        public HtmlPost(string url, string data)
        { _results = _Send(url, data); }

        public static HtmlPost Send(string url, string data)
        { return new HtmlPost(url, data); }

        private HttpWebResponse _results;

        public Byte[] ToBytes()
        { return _results.GetResponseStream().ReadFully(); }

        public Stream ToStream()
        { return _results.GetResponseStream(); }

        public override string ToString()
        {
            using (StreamReader reader = new StreamReader(_results.GetResponseStream()))
                return reader.ReadToEnd();
        }

        private HttpWebResponse _Send(string url, string postData)
        {
            string webpageContent = string.Empty;

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = byteArray.Length;

                using (Stream webpageStream = webRequest.GetRequestStream())
                    webpageStream.Write(byteArray, 0, byteArray.Length);

                return (HttpWebResponse)webRequest.GetResponse();
            }
            catch { return null; }
        }
    }


    public class PdfSettings
    { 
        public int dpi = 150;
        public string MarginLeft = "2mm";
        public string MarginRight = "2mm";
        public string MarginTop = "2mm";
        public string MarginBottom = "2mm";
        public bool Landscape = false;
        public string PageSize = null; //Letter, Legal, A1, ...
        public int? PageHeight = null;
        public int? PageWidth = null;

        public string MarginAll { set {
            MarginLeft = MarginRight = MarginTop = MarginBottom = value;
        } }
    }

}
