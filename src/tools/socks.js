(function() {
  var $, Page, children, el, empty_page, footer, header, heightWithin, index, old_body, over, overflow, page, pages, to_add, _i, _j, _k, _len, _len1, _len2,
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  $ = jQuery;

  empty_page = $("<div></div>");

  header = $("header").remove();

  footer = $("footer").remove();

  empty_page.append(header);

  empty_page.append(footer);

  heightWithin = function(container, elm) {
    var height;
    elm = $(elm).clone();
    $(container).append(elm);
    height = $(elm).height();
    elm.remove();
    return height || 0;
  };

  Page = (function() {

    function Page() {
      this.add = __bind(this.add, this);

      this.tableOverflow = __bind(this.tableOverflow, this);

      this.heightOfChildren = __bind(this.heightOfChildren, this);

      this.load_settings = __bind(this.load_settings, this);
      this.first_elm = true;
      this.el = $(empty_page).clone();
      this.el_outer = $("<div class='socks-page'></div>");
      this.el_outer.append(this.el);
      $("body").append(this.el_outer);
      this.load_settings();
      this.max = this.el.height();
      this.running_height = 0;
    }

    Page.prototype.load_settings = function() {
      var m;
      this.el_outer.css("padding-top", pdf_settings.MarginTop);
      this.el_outer.css("padding-left", pdf_settings.MarginLeft);
      this.el_outer.css("padding-right", pdf_settings.MarginRight);
      this.el_outer.css("padding-bottom", pdf_settings.MarginBottom);
      this.el.height("100%");
      this.el.width("100%");
      m = 1.50;
      if (pdf_settings.Landscape) {
        this.el_outer.css("height", pdf_settings.PageWidth * m + "in");
        this.el_outer.css("width", pdf_settings.PageHeight * m + "in");
      } else {
        this.el_outer.css("width", pdf_settings.PageWidth * m + "in");
        this.el_outer.css("height", pdf_settings.PageHeight * m + "in");
      }
      $('footer', this.el).css("left", 0);
      return $('footer', this.el).css("right", 0);
    };

    Page.prototype.heightOfChildren = function() {
      var child, height, _i, _len, _ref;
      height = 0;
      _ref = this.el.children();
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        child = _ref[_i];
        height += $(child).outerHeight(true);
      }
      return height;
    };

    Page.prototype.tableOverflow = function(table) {
      var new_table, tr, trs;
      new_table = $(table).clone();
      $('tbody', new_table).empty();
      $(table).replaceWith(new_table);
      trs = $('tbody tr', table).toArray();
      while (this.heightOfChildren() < this.max) {
        tr = trs.shift();
        $('tbody', new_table).append(tr);
      }
      tr = $('tbody tr', new_table).last();
      $('tbody', table).prepend(tr);
      return table;
    };

    Page.prototype.add = function(elm) {
      this.el.append(elm);
      if (this.heightOfChildren() > this.max && elm.tagName === "TABLE") {
        return [this.tableOverflow(elm)];
      }
      if (this.first_elm) {
        this.first_elm = false;
        return [];
      }
      if (this.heightOfChildren() > this.max) {
        $(elm).remove();
        return [elm];
      }
      return [];
    };

    return Page;

  })();

  to_add = [];

  old_body = $("body").remove();

  children = old_body.children();

  for (_i = 0, _len = children.length; _i < _len; _i++) {
    el = children[_i];
    if (el.tagName !== "SCRIPT") {
      to_add.push(el);
    }
  }

  to_add.reverse();

  $("html").append($('<body>'));

  page = new Page();

  pages = [page];

  while (to_add.length) {
    el = to_add.pop();
    overflow = page.add(el);
    if (overflow.length) {
      for (_j = 0, _len1 = overflow.length; _j < _len1; _j++) {
        over = overflow[_j];
        to_add.push(over);
      }
      page = new Page();
      pages.push(page);
    }
  }

  index = 1;

  for (_k = 0, _len2 = pages.length; _k < _len2; _k++) {
    page = pages[_k];
    $("footer [data-pdf=\"current_page\"]", page.el).text(index);
    $("footer [data-pdf=\"total_page\"]", page.el).text(pages.length);
    index++;
  }

}).call(this);
