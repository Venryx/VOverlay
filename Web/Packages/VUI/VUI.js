VUI = new function() {
	var s = this;

	s.controlTypes = {};
	s.RegisterControlType = function(name, transformer) { s.controlTypes[name] = transformer; };
	s.ApplyControlType = function(control, controlType, options) {
		options = options || {};

		if (control.processedByVUI)
			return;

		/*if (control.attr(controlType) == null) // add control-type marker-attribute, if not existing
			control.attr(controlType, "");*/
		s.controlTypes[controlType].call(control, options);
		control.processedByVUI = true;
	};

	s.Process = function(rootNode) {
		/*var nodes = [rootNode[0]];
		$(rootNode).find("*").each(function() { nodes.push(this); });

		for (var i in nodes) {
			var node = nodes[i];
		}*/

		for (var name in s.controlTypes)
			$(rootNode).find("[" + name + "]").addBack("[" + name + "]").each(function() { s.ApplyControlType(this, name); });
	};
};

var tempHolder;
V$ = function(html) {
	var result = {};
	result.appendTo = function(parent) { // faster than (takes about a third the time of) JQuery's build-shard-then-add-later approach; so use this instead when it's convenient/performance-intensive
		var oldChildrenCount = parent.children().length;

		//parent.append(html);
		//parent[0].innerHTML += html;
		var element = document.createElement(html.substring(1, html.indexOfAny(" ", ">")));
		parent[0].appendChild(element);
		element.outerHTML = html;
		//$(element).replaceWith(html);

		//return $(element); // assume only one child was added
		return $(parent.children()[oldChildrenCount]); // needed, because setting outerHTML creates new element
	};
	result.insertBefore = function(oldElement) {
		var parent = oldElement.parent();
		var oldIndex = oldElement.index();

		var element = document.createElement(html.substring(1, html.indexOfAny(" ", ">")));
		parent[0].insertBefore(element, oldElement[0]);
		element.outerHTML = html;

		//return $(element); // assume only one child was added
		return $(parent.children()[oldIndex]); // needed, because setting outerHTML creates new element
	};
	result.appendToTempHolder = function() {
		if (tempHolder == null || tempHolder.length == 0)
			tempHolder = $("#vuiTempHolder");
		return result.appendTo(tempHolder);
	};
	return result;
};