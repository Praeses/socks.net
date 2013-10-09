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

