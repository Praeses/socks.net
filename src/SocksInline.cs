using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Socksnet
{
    
    /// <summary>
    /// Tasks a given html render and inlines all the things
    /// This way wktohtml doesn't need to make any request to load access.
    /// </summary>
    internal class SocksInline
    {


        public string render(string html, PdfSettings settings) {
            html = InlineCss(html, settings);
            html = InlineJs(html);
            html = InlineImg(html);
            return html;
        }


        private string InlineCss(string html, PdfSettings settings)
        {
            Match match = null;
            var rx = new Regex(@"<link[^>]*href=[""']([^h][^t][^t][^p][^""']*.css)[^""']*[""'][^>]*((/>)|(>\s*</link>))", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            match = rx.Match(html);
            while (match.Success)
            {
                var source = match.Groups[1].ToString();
                source = settings.FixCssPath(source);
                var path = PathHelper.Instance.BuildPath(source);
                try
                {
                    var content = File.ReadAllText(path);
                    html = html.Replace(match.ToString(), string.Format(@"<style>{0}</style>", content));
                    match = rx.Match(html);
                }
                catch  /* bad asset path, dont kill the render */ 
                { return html; }
            }
            return html;
        }


        private string InlineJs(string html)
        {
            Match match = null;
            var rx = new Regex(@"<script[^>]*src=[""']((?!http)[^""']*.js)[^""']*[""'][^>]*((/>)|(>\s*</script>))", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            match = rx.Match(html);
            while (match.Success)
            {
                try
                {
                    var src = match.Groups[1].ToString();
                    var path = PathHelper.Instance.BuildPath(src);
                    var content = File.ReadAllText(path);
                    html = html.Replace(match.ToString(), string.Format(@"<script>{0}</script>", content));
                    match = rx.Match(html);
                }
                catch { /* bad asset path, dont kill the render */ }
            }
            return html;
        }


        private string InlineImg(string html)
        {
            Match match = null;
            var rx = new Regex(@"<img[^>]*src=""([^d][^a][^t][^a][^:][^""]*)""[^>]*>", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            match = rx.Match(html);
            while (match.Success)
            {
                try
                {
                    var path = PathHelper.Instance.BuildPath(match.Groups[1].ToString());
                    var content = Convert.ToBase64String(File.ReadAllBytes(path));
                    html = html.Replace(match.ToString(), string.Format(@"<img src=""data:image/gif;base64,{0}"" />", content));
                    match = rx.Match(html);
                }
                catch { return html; } /* bad asset path, dont kill the render */
            }
            return html;
        }

    }
}
