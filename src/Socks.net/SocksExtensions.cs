using Socksnet;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace System.Web.Mvc
{

    public static partial class SocksExtensions
    {


        public static ActionResult Pdf(this Controller controller, string view, PdfSettings settings = null)
        {
            return Pdf(controller, view, null, settings: settings);
        }


        public static ActionResult Pdf(this Controller controller, string view, Object model, PdfSettings settings = null)
        {
            var data = new Dictionary<string, string>();

            if (settings == null) settings = new PdfSettings();
            if (settings.Landscape) data.Add("orientation", "Landscape");

            var html = Socks.RenderViewToString(controller, view, model, settings);
            //var html = controller.RenderViewToString(view, model, settings);

            if (settings.Action == PdfSettings.PdfAction.Html)
                return Socks.RenderPdfAsHtml(controller, html);

            Stream pdf = Socks.toPdf(html, settings);

            if (settings.Action == PdfSettings.PdfAction.Download) {
                var filename = settings.Filename ?? (Guid.NewGuid().ToString("N") + ".pdf");
                controller.Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename);
            }

            //use reflection to call protected method :(
            var obj = controller.GetType().InvokeMember("File"
                , (System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                , Type.DefaultBinder
                , controller
                , new object[] { pdf, @"application/pdf" });
            return (ActionResult)obj;
            
        }



        public static Stream PdfStream(this Controller controller, string view, Object model, PdfSettings settings = null)
        {
            var data = new Dictionary<string, string>();

            if (settings == null) settings = new PdfSettings();
            if (settings.Landscape) data.Add("orientation", "Landscape");

            var html = Socks.RenderViewToString(controller, view, model, settings);

            if (settings.Action == PdfSettings.PdfAction.Html)
                throw new Exception("Html mode is not supported in stream mode");

            return Socks.toPdf(html, settings);
        }


    }



}
