socks.net
=========

socks.net is a wrapper around the socks PDF service to make PDF generation simple in .net

Socks is a web service that takes html and generates Pdfs using the webkit rendering engine.

Installation
--------------

```sh
Install-Package socks.net
```



Usage
-----
In your controller change View() to this.Pdf()

Example Action
```sh
        public ActionResult Index()
        {
            return this.Pdf();
        }
```
    
    
Headers / Footers
-----
Headers and footers are generated from separate view files. The header and footer view will repeat on each page of the pdf. To use headers of views create a razor of the format ViewName.header.cshtml
This will tell socks.net to include the header in the pdf

In header and footer text the following variables will be substituted.
   * [page]       Replaced by the number of the pages currently being printed
   * [frompage]   Replaced by the number of the first page to be printed
   * [topage]     Replaced by the number of the last page to be printed
   * [webpage]    Replaced by the URL of the page being printed
   * [section]    Replaced by the name of the current section
   * [subsection] Replaced by the name of the current subsection
   * [date]       Replaced by the current date in system local format
   * [time]       Replaced by the current time in system local format
   * [title]      Replaced by the title of the of the current page object
   * [doctitle]   Replaced by the title of the output document
   * [sitepage]   Replaced by the number of the page in the current site being converted
   * [sitepages]  Replaced by the number of pages in the current site being converted

