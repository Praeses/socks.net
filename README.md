socks.net
=========

Socks.net is a tool to use the razor engine to make your PDFs.

It renders your razor views passes it to wkhtmltopdf and sends the pdf file to the web user

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
            return this.Pdf("Index");
        }
```


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

For page numbers the following magic strings are replaced with the corresponding values
  * {{page}}            Replaced with the current page
  * {{pages}}           Replaced with the number of pages




