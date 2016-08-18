//var template = V.Multiline(function()
//{/*
//<div class="scrollView_rootChild scrollbar-chrome" ng-transclude>
//</div>
//*/}).trim();

//var module = window.module || angular.module("app", []);
//module.directive("scrollView", function()
//{
//    return {
//        link: function(scope, element)
//        {
//            var root = element;
//            if (!root.prop("style").width.length)
//                root.css("width", "100%");
//            if (!root.prop("style").height.length)
//                root.css("height", "100%");
//            var options = {wrapper: root};
//            /*if (element.attr("onScrollbarShow"))
//                options.onScrollbarShow = new Function(element.attr("onScrollbarShow"));
//            if (element.attr("onScrollbarHide"))
//                options.onScrollbarHide = new Function(element.attr("onScrollbarHide"));*/
//            root.children(".scrollbar-chrome").scrollbar(options);
//        },
//        transclude: true,
//        template: template
//    };
//});

VUI.RegisterControlType("scroll-view", function(options) {
	var outer = $(this);

	options.useBackgroundDrag = outer.attr("useBackgroundDrag") != "false";

	var inner = $("<div class='scrollView_rootChild scrollbar-chrome'>");
	//outer.insertInto(inner.parent(), inner.index());
	//inner.appendTo(outer);

	outer.children().appendTo(inner);
	inner.appendTo(outer);

	if (!inner.prop("style").width.length)
		inner.css("width", "100%");
	if (!inner.prop("style").height.length)
		inner.css("height", "100%");
	var jOptions = {wrapper: outer, onScroll: options.onScroll};
	/*if (inner.attr("onScrollbarShow"))
	    jOptions.onScrollbarShow = new Function(inner.attr("onScrollbarShow"));
	if (inner.attr("onScrollbarHide"))
	    jOptions.onScrollbarHide = new Function(inner.attr("onScrollbarHide"));*/
	inner.scrollbar(jOptions); //inner.children(".scrollbar-chrome").scrollbar(jOptions);

	if (options.useBackgroundDrag)
	{
		var move_lastPos;
		outer.mousedown(function(e)
		{
			if (e.button != 1)
				return;
			//e.preventDefault();
			move_lastPos = new Vector2i(e.pageX, e.pageY);
			//inner.children().css("cursor", "move");
			return false;
		});
		//outer.mousemove(function(e)
		$(document).mousemove(function(e)
		{
			if (move_lastPos == null)
				return;
			var mousePos = new Vector2i(e.pageX, e.pageY);
			var offsetSinceLast = mousePos.Minus(move_lastPos);

			inner.scrollLeft(inner.scrollLeft() - offsetSinceLast.x).scrollTop(inner.scrollTop() - offsetSinceLast.y);

			move_lastPos = mousePos;
		});
		//outer.on("mouseup mouseleave", function(e)
		$(document).mouseup(function(e)
		{
			if (move_lastPos == null)
				return;
			move_lastPos = null;
			//inner.children().css("cursor", "");
			if (options.onScroll)
				options.onScroll();
		});
	}
});