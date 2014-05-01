#logs = []
#console = {}
#console.log = () =>
#  logs.push arg for arg in arguments
#window.onError = (error) ->
#  $("body").text error


$ = jQuery

empty_page = $("<div></div>")
header = $("header").remove()
footer = $("footer").remove()
empty_page.append header
empty_page.append footer


heightWithin = (container, elm) ->
  elm = $(elm).clone()
  $(container).append( elm )
  height = $(elm).height()
  elm.remove()
  height || 0


class Page
  constructor: ->
    @el = $(empty_page).clone()
    @el_outer = $("<div class='socks-page'></page>")
    @el_outer.append @el
    $("body").append @el_outer
    @load_settings()
    @max = @el.height()
    @max -= $('footer',@el).outerHeight()
    @running_height = 0

  load_settings: =>
    @el_outer.css "padding-top", pdf_settings.MarginTop
    @el_outer.css "padding-left", pdf_settings.MarginLeft
    @el_outer.css "padding-right", pdf_settings.MarginRight
    @el_outer.css "padding-bottom", pdf_settings.MarginBottom
    @el.height("100%")
    @el.width("100%")
    m = 1.50
    if pdf_settings.Landscape
      @el_outer.css "height", pdf_settings.PageWidth * m + "in"
      @el_outer.css "width", pdf_settings.PageHeight * m + "in"
    else
      @el_outer.css "width", pdf_settings.PageWidth * m + "in"
      @el_outer.css "height", pdf_settings.PageHeight * m + "in"
    $('footer',@el).css "left", 0
    $('footer',@el).css "right", 0
    #@el.css "margin-bottom", footer.height()

  heightOfChildren: () =>
    height = 0
    height += $(child).outerHeight() for child in @el.children()
    height

  add: (elm) =>
    @el.append( elm )
    if @heightOfChildren() > @max && @el.children().length > 1
      $(elm).remove()
      return [elm]
    []




to_add = []
old_body = $("body").remove()
children = old_body.children()
for el in children
  to_add.push(el) unless el.tagName is "SCRIPT"
to_add.reverse()
$("html").append $('<body>')
page = new Page()
pages =[page]


#add content to pages
while( to_add.length )
  el = to_add.pop()
  overflow = page.add el
  if overflow.length
    to_add.push over for over in overflow
    page = new Page()
    pages.push page








##header and footer Data
index = 1
for page in pages
  $("footer [data-pdf=\"current_page\"]", page.el).text index
  $("footer [data-pdf=\"total_page\"]", page.el).text pages.length
  index++





#log_el = $("<div>")
#log_el.append("<div>#{log}</div>") for log in logs
#log_el.css("position","fixed")
#log_el.css("background-color","red")
#log_el.css("font-size","3em")
#log_el.css("z-index","9999")
#$('body').prepend(log_el)
