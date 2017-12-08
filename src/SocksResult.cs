using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Socksnet
{

    public class SocksResult<T> : ActionResult
    {

        //private string _filename;
        private readonly PdfSettings _Settings;
        private T _model;

        public SocksResult(T model, PdfSettings Settings)
        {
            _Settings = Settings;
            _model = model;
        }


        public override Task ExecuteResultAsync(ActionContext context)
        {
            var eng = new SocksEngine(context);

            var stream_await = eng.Pdf<T>(_model, _Settings);
            stream_await.Wait();
            var stream = stream_await.Result;

            var content_disposition = "inline";
            if (_Settings.Action == PdfSettings.PdfAction.Download)
                content_disposition = "attachment";
            if (_Settings.Filename != null)
                content_disposition += "; filename=\"" + _Settings.Filename + "\"";
            context.HttpContext.Response.Headers.Add("Content-Disposition", content_disposition);


            if (_Settings.Action == PdfSettings.PdfAction.Html)
            {
                context.HttpContext.Response.ContentType = "text/html";
            }
            else
            {
                context.HttpContext.Response.ContentType = "application/pdf";
            }

            stream.CopyTo(context.HttpContext.Response.Body);
            return base.ExecuteResultAsync(context);
        }


        

    }

}
