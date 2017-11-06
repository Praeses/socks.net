Socks.Core
=========

Socks.Core is a tool to use the razor engine to make your PDFs.

This is a port of Socks.Net to run on .net Core 2

It renders your razor views passes it to wkhtmltopdf and sends the pdf file to the web user

Installation
--------------

```sh
Install-Package socks.core
```



Usage
-----
In your controller change View() to this.Pdf()

Example Action - Single View
```Csharp
  public ActionResult Index()
  {
      return this.Pdf("Index");
  }
```

Example Action - Stitch Multiple views 
```Csharp
  public ActionResult About()
  {
      var report1 = new PdfView {View = "About"};
      var report2 = new PdfView {View = "About"};

      return this.Pdf(report1, report2);
  }
```
Limitations: 
* When using multiple views paging tokens do not work as expected.  Paging tokens will work within each rendered view, but not in the final pdf as a whole.
* Because you are stitching multiple view pdfs together, html mode does not work. 



Headers / Footers
-----
Use the header and footer tag
```html
<header>
  <h1>Header for every page !!!</h1>
</header>
```

Paging and Page Numbers
-----
Socks splits up the contents of your body tag onto multiple pages.
If you want an element to page, put it in the body.
If not wrap it in a div

Tables behave slightly differently than all other elements. Tables under the body tag will automatically be paged.

For page numbers the following magic strings are replaced with the corresponding values
  * {{page}}            Replaced with the current page
  * {{pages}}           Replaced with the number of pages




