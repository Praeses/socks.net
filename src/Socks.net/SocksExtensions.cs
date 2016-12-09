using Socksnet;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace System.Web.Mvc
{
    public static partial class SocksExtensions
    {
        public static ActionResult Pdf(this Controller controller, params PdfView[] pdfViews)
        {
            var pdfStreams = new List<Stream>();
            // Generate all the pdfs as MemoryStreams
            foreach (var pdfView in pdfViews)
            {
                pdfView.PdfSettings = pdfView.PdfSettings ?? new PdfSettings();
                var html = Socks.RenderViewToString(controller, pdfView.View, pdfView.Model, pdfView.PdfSettings);
                Stream pdf = Socks.toPdf(html, pdfView.PdfSettings);
                pdfStreams.Add(pdf);
            }

            // Stitch the files together using pdfSharp (http://www.pdfsharp.net/wiki/CombineDocuments-sample.ashx)
            var outputDocument = new PdfDocument();
            foreach(var pdfStream in pdfStreams)
            {
                var pages = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import).Pages;
                for (int pageIndex = 0; pageIndex < pages.Count; pageIndex++)
                {
                    outputDocument.AddPage(pages[pageIndex]);
                }
            }

            var finalPdf = new MemoryStream();
            outputDocument.Save(finalPdf, false);

            return controller.File(finalPdf);
        }

        public static ActionResult Pdf(this Controller controller, string view, PdfSettings settings = null)
        {
            return Pdf(controller, view, null, settings: settings);
        }

        public static ActionResult Pdf(this Controller controller, string view, Object model, PdfSettings settings = null)
        {
            if (settings == null) settings = new PdfSettings();

            var html = Socks.RenderViewToString(controller, view, model, settings);
            //var html = controller.RenderViewToString(view, model, settings);

            if (settings.Action == PdfSettings.PdfAction.Html)
                return Socks.RenderPdfAsHtml(controller, html);

            Stream pdf = Socks.toPdf(html, settings);

            if (settings.Action == PdfSettings.PdfAction.Download) {
                var filename = settings.Filename ?? (Guid.NewGuid().ToString("N") + ".pdf");
                controller.Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename);
            }
            
            return controller.File(pdf);
        }

        public static Stream PdfStream(this Controller controller, string view, Object model, PdfSettings settings = null)
        {
            if (settings == null) settings = new PdfSettings();

            var html = Socks.RenderViewToString(controller, view, model, settings);

            if (settings.Action == PdfSettings.PdfAction.Html)
                throw new Exception("Html mode is not supported in stream mode");

            return Socks.toPdf(html, settings);
        }

        internal static ActionResult File(this Controller controller, Stream stream)
        {
            //use reflection to call protected method :(
            var obj = controller.GetType().InvokeMember("File"
                , (Reflection.BindingFlags.InvokeMethod | Reflection.BindingFlags.NonPublic | Reflection.BindingFlags.Instance)
                , Type.DefaultBinder
                , controller
                , new object[] { stream, @"application/pdf" });
            return (ActionResult)obj;
        }
    }
}