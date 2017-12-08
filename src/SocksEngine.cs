using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Socksnet
{
    public class SocksEngine
    {

        private readonly ICompositeViewEngine _compositeViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private ActionContext _actionContext;


        public SocksEngine(ActionContext actionContext) {
            _actionContext = actionContext;
            var provider = actionContext.HttpContext.RequestServices;
            _compositeViewEngine = provider.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            _tempDataProvider = provider.GetService(typeof(ITempDataProvider)) as ITempDataProvider;
            PathHelper.Instance.HostingEnvironment = 
                provider.GetService(typeof(IHostingEnvironment)) as IHostingEnvironment;
        }


        public async Task<Stream> Pdf<TModel>(TModel model, PdfSettings settings = null)
        {
            if (settings == null) settings = new PdfSettings();
            var html = await this.RenderViewToString(model, settings);
            html = InjectSocks(html, settings);

            if (settings.Action == PdfSettings.PdfAction.Html)
            {
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(html);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }

            Stream pdf = this.toPdf(html, settings);
            return pdf;
        }




        private async Task<string> RenderViewToString<TModel>(TModel model, PdfSettings settings)
        {

            using (var string_writer = new StringWriter())
            {
                var viewName = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)_actionContext.ActionDescriptor).ActionName;

                ViewEngineResult viewResult = _compositeViewEngine.FindView(_actionContext, viewName, false);

                var ViewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary());
                ViewData.Model = model;

                var tempData = new TempDataDictionary(_actionContext.HttpContext, _tempDataProvider);

                ViewContext viewContext = new ViewContext(_actionContext, viewResult.View, ViewData, tempData, string_writer, new HtmlHelperOptions());

                var view = _compositeViewEngine;
                await viewResult.View.RenderAsync(viewContext);

                return string_writer.ToString();
            }
        }



        private string InjectSocks(string html, PdfSettings settings) {
            html = new SocksInline().render(html, settings);
            html = new SocksInjector().render(html, settings);

            return html;
        }






        //calls the lib to do the convert
        private Stream toPdf(string html, PdfSettings settings)
        {
            var temp = Path.GetTempFileName();
            var source = temp + ".html";
            var desc = temp + ".pdf";
            try
            {
                File.WriteAllText(source, html);
                var args = BuildArgs(source, desc, settings);
                var wkexe_path = PathHelper.Instance.tools_path() + "wkhtmltopdf.exe";

                ProcessStartInfo psi = new ProcessStartInfo(wkexe_path, string.Join(" ", args))
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
            finally
            {
                if (File.Exists(source)) File.Delete(source);
                if (File.Exists(desc)) File.Delete(desc);
            }
        }



        private List<string> BuildArgs(string source, string desc, PdfSettings settings)
        {
            var args = new List<string>();

            args.Add("--page-height " + settings.PageHeight + "in");
            args.Add("--page-width " + settings.PageWidth + "in");
            args.Add("--window-status socks_complete");

            if (settings.EnableLongRunningJavascript)
                args.Add("--no-stop-slow-scripts");

            if (settings.EnableSocksJsAndCss)
            {
                args.Add("--margin-left 0");
                args.Add("--margin-right 0");
                args.Add("--margin-top 0");
                args.Add("--margin-bottom 0");
            }
            else
            {
                args.Add("--margin-left " + settings.MarginLeft);
                args.Add("--margin-right " + settings.MarginRight);
                args.Add("--margin-top " + settings.MarginTop);
                args.Add("--margin-bottom " + settings.MarginBottom);
            }
            if (settings.Landscape) args.Add("--orientation Landscape");
            args.Add(source);
            args.Add(desc);
            return args;
        }


    }
}
