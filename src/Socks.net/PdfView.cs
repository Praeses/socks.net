using Socksnet;

namespace System.Web.Mvc
{
    public class PdfView
    {
        public string View { get; set; }
        public Object Model { get; set; }
        public PdfSettings PdfSettings { get; set; }
    }
}