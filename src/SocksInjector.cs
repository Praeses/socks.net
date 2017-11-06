using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Socksnet
{

    /// <summary>
    /// this class tasks a html render and mods it to include socks.
    /// </summary>
    internal class SocksInjector
    {



        public string render(string html, PdfSettings settings)
        {            
            html = IncludeSockStyles(html, settings);
            html = IncludeSockJavascript(html, settings);
            html = html.Replace("{{page}}", @"<span data-pdf='current_page'></span>");
            html = html.Replace("{{pages}}", @"<span data-pdf='total_page'></span>");
            return html;
        }








        private string IncludeSockScreenStyles(string html)
        {
            Match match = null;
            var rx = new Regex(@"< *head *>", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            match = rx.Match(html);
            if (match.Success)
            {
                var path = PathHelper.Instance.tools_path() + "screen.css";
                var content = "<style>" + File.ReadAllText(path) + "</style>";
                html = html.Replace(match.ToString(), match.ToString() + content);
            }
            return html;
        }


        private string IncludeSockStyles(string html, PdfSettings settings)
        {
            if (!settings.EnableSocksJsAndCss)
                return html;

            Match match = null;
            var rx = new Regex(@"< *head *>", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            match = rx.Match(html);
            if (match.Success)
            {
                var path = PathHelper.Instance.tools_path() + "socks.css";
                var content = "<style>" + File.ReadAllText(path) + "</style>";
                html = html.Replace(match.ToString(), match.ToString() + content);
            }
            return html;
        }



        private static string IncludeSockJavascript(string html, PdfSettings settings)
        {
            string pdf_settings = JsonConvert.SerializeObject(settings);

            var path = PathHelper.Instance.tools_path() + "jquery-1.12.4.min.js";
            var content = "<script type='text/javascript'>";
            content += File.ReadAllText(path) + "</script>";
            html += content;

            if (!settings.EnableSocksJsAndCss)
                return html;

            path = PathHelper.Instance.tools_path() + "socks.js";
            content = "<script type='text/javascript'>";
            content += "var pdf_settings=" + pdf_settings + ";";
            content += File.ReadAllText(path) + "</script>";

            return html + content;
        }






    }

}
