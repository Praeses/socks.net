using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Socksnet
{

    public class PdfSettings
    {
        public enum PdfAction
        {
            Open,
            Download,
            SaveToDisk
        };

        public string MarginLeft = "0.75in";
        public string MarginRight = "0.75in";
        public string MarginTop = "0.75in";
        public string MarginBottom = "0.75in";
        public bool Landscape = false;

        //public string PageSize = null; //Letter, Legal, A1, ...
        public decimal PageWidth = 8.5m;
        public decimal PageHeight = 11m;

        public bool EnableSocksJsAndCss = true;

        public string MasterPage;

        private Func<string, string> _fixCssPath;
        public Func<string, string> FixCssPathMethod { set { _fixCssPath = value; }}
        internal string FixCssPath(string path)
        {
            if (_fixCssPath != null)
                return _fixCssPath(path);
            return path;
        }

        public string MarginAll
        {
            set
            {
                MarginLeft = MarginRight = MarginTop = MarginBottom = value;
            }
        }

        public PdfAction Action = PdfAction.Open;
        public string Filename = null;
    }

}
