var JQueryOthers = new function() {
	var children_old = $.fn.children;
	$.fn.children = function() {
		if (arguments[0] instanceof Function) {
			var matchFunc = arguments[0];
			var result = children_old.apply(this, arguments);
			return $(result.toArray().Where(matchFunc));
		}
		return children_old.apply(this, arguments);
	};
    $.fn.first = function(/*o:*/ matchFunc, preferEmptyOverNull) { // equivalent to C# FirstOrDefault
	    preferEmptyOverNull = preferEmptyOverNull != null ? preferEmptyOverNull : false;

        var result = this.toArray().First(matchFunc || function() { return true; });
        return preferEmptyOverNull ? $(result) : (result ? $(result) : null);
    };
    $.fn.First = function(/*o:*/ matchFunc, preferEmptyOverNull) { // equivalent to C# FirstOrDefault
    	preferEmptyOverNull = preferEmptyOverNull != null ? preferEmptyOverNull : false;

    	var result = this.toArray().Select(function(a) { return $(a); }).First(matchFunc || function() { return true; });
    	return preferEmptyOverNull ? $(result) : (result ? $(result) : null);
    };
    $.fn.Where = function(/*o:*/ matchFunc) { // equivalent to C# Where, except it wraps each item in a jQuery object
    	//return this.toArray().Where(matchFunc || function() { return true; }).Select(function(a) { return $(a); });
    	return this.toArray().Select(function(a) { return $(a); }).Where(matchFunc || function() { return true; });
    };
    //$.fn.Contains = function(element) { return this.toArray().Contains(element); };
    $.fn.ToList = function()
    {
    	var result = [];
    	/*for (var i in this)
    		result.push($(this[i]));*/
    	//this.each(function(a, b) { result.push($(b)); });
		this.each(function() { result.push($(this)); });
    	return result;
    };

	$.fn.setClassPresent = function(className, present)
	{
		if (present)
			this.addClass(className);
		else
			this.removeClass(className);
	};

	var lastElementAutoID = -1;
	$.fn.SetID = function(/*o:*/ id)
	{
		id = id != null ? id : "AutoID_" + ++lastElementAutoID;
		this.attr("id", id);
		return this;
	};

	/*$.fn.offset_old = $.fn.offset;
	//$.fn.offset = function(ignorePadding_orArg1)
	$.fn.offset = function()
	{
		//if (window.InUnity && InUnity() && result && ignorePadding_orArg1 === true)
		if (window.InUnity && InUnity() && result)
			result.top -= 50;
		return result;
	};*/

	//$.fn.addParents = function() { return this.add(this.parents()); };
	$.fn.plusParents = function() { return $(this.toArray().concat(this.parents().toArray())); }; // (this way keeps child as first item)

	$.fn.positionFrom = function (/*o:*/ referenceControl, useCloneToCalculate)
	{
		referenceControl = referenceControl || VO.root;

		if (useCloneToCalculate) // 'this' must be descendent of 'referenceControl', for this code to work
		{
			$(this).attr("positionFrom_temp_controlB", true);
			//$(this).data("positionFrom_temp_controlB", true);
			if (!$(this).parents().toArray().Contains(referenceControl[0]))
				throw new Error("'this' must be descendent of 'referenceControl'.");
			var referenceControl_clone = referenceControl.clone(true).appendTo("#hiddenTempHolder");
			var this_clone = referenceControl_clone.find("[positionFrom_temp_controlB]");
			//var this_clone = referenceControl_clone.find(":data(positionFrom_temp_controlB)");
			var result = this_clone.positionFrom(referenceControl_clone);
			referenceControl_clone.remove();
			$(this).attr("positionFrom_temp_controlB", null);
			//$(this).data("positionFrom_temp_controlB", null);
			return result;
		}

		var offset = $(this).offset();
		var referenceControlOffset = referenceControl.offset();
		return {left: offset.left - referenceControlOffset.left, top: offset.top - referenceControlOffset.top};
	};
	$.fn.positionFrom_Vector2i = function(/*o:*/ referenceControl, useCloneToCalculate) // maybe make-so: this is merged with the above
	{
		var result = $(this).positionFrom(referenceControl, useCloneToCalculate);
		return new Vector2i(result.left, -result.top);
	};

	$.fn.withoutChildren = function(sel) { return this.clone().children(sel || "> *").remove().end(); };

    $.fn.on_doubleClick = function(descendentSelector, functionToCall)
    {
        $(this).on("click", descendentSelector, function(event)
        {
            this.clicks = (this.clicks ? this.clicks + 1 : 1); // count clicks
            if (this.clicks == 1)
            {
                var self = this;
                this.timer = setTimeout(function()
                {
                    self.clicks = 0; // second click delayed too long, reset
                }, 500);
            }
            else
            {
                clearTimeout(this.timer); // cancel delay timer
                this.clicks = 0; // reset
                functionToCall.call(this);
            }
        }).on("dblclick", function(e)
        {
            e.preventDefault(); // cancel system double-click event
        });
    };
    $.fn.mouseInBounds = function(mouseX, mouseY)
    {
        var bounds = $(this).offset();
        bounds.bottom = bounds.top + $(this).outerHeight();
        bounds.right = bounds.left + $(this).outerWidth();
        if ((mouseX >= bounds.left && mouseX <= bounds.right) && (mouseY >= bounds.top && mouseY <= bounds.bottom))
            return true;
        return false;
    };
    $.fn.insertInto = function(parent, index)
    {
	    if (index == -1) // -1 taken as 'normal append'
		    this.appendTo(parent);
        else if (index == 0)
            this.prependTo(parent);
        else
            this.insertAfter(parent.children().eq(index - 1));
        return this;
    };
	$.fn.insert = function(index, child) { child.insertInto(this, index); };

    /*var oldText = $.fn.text;
    $.fn.text = function() // fix for custom-textarea's requiring two-hits-of-the-enter-key to add the first line-break
    {
        if (this.is("div[text-area]") && typeof arguments[0] == "string") // if setting text, add carriage-return to end of text
            arguments[0] += "\r";
        var result = oldText.apply(this, arguments);
        if (this.is("div[text-area]") && !arguments.length) // if getting text, strip text of carriage-returns (as added by the above)
            result = result.replace(/\r/g, "");
        return result;
    };*/
    var oldText = $.fn.text;
    $.fn.text = function()
    {
        if (this.is("div[text-area]") && typeof arguments[0] == "string") // if setting text, add line-break to end of text (the last one is never displayed)
            arguments[0] += "\n";
        var result = oldText.apply(this, arguments);
        if (this.is("div[text-area]") && !arguments.length) // if getting text, and text ends with line-break, remove last line-break (the last one is never displayed)
            result = result.replace(/\n$/, "");

        if (this.is("div[text-area]") && typeof arguments[0] == "string") // if set text, trigger change event
            this.trigger("textSet");

        return result;
    };

	/*var oldVal = $.fn.val;
	$.fn.val = function() {
		if (this.is("input[type=number]") && arguments.length == 0)
			return parseFloat(oldVal.call(this));
        return oldVal.apply(this, arguments);
    };*/

    // run angular-compile command on new content (maybe: this is separate from the MutationObserver system, since it's meant to process element-tree, rather than individual elements)
    var oldPrepend = $.fn.prepend;
    $.fn.prepend = function()
    {
        var isFragment = arguments[0] && arguments[0][0] && arguments[0][0].parentNode && arguments[0][0].parentNode.nodeName == "#document-fragment";
        var result = oldPrepend.apply(this, arguments);
	    if (isFragment)
		    VUI.Process(arguments[0]); //AngularCompile(arguments[0]);
        return result;
    };
    var oldAppend = $.fn.append;
    $.fn.append = function()
    {
        var isFragment = arguments[0] && arguments[0][0] && arguments[0][0].parentNode && arguments[0][0].parentNode.nodeName == "#document-fragment";
        var result = oldAppend.apply(this, arguments);
        if (isFragment)
        	VUI.Process(arguments[0]); //AngularCompile(arguments[0]);
        return result;
    };

	$.fn.removeHtml = function(index, length)
	{
		var contents = $(this).contents().toArray();
		var processedHtmlLength = 0;
		for (var i in contents)
		{
			var content = $(contents[i]);
			//var contentHtml = content.html() || content.text();
			var contentHtml = content[0].outerHTML || content[0].textContent;
			var contentHtmlIndex = processedHtmlLength;
			var contentHtmlLength = contentHtml.length;
			//if (index >= contentHtmlIndex || index + length < contentHtmlIndex + contentHtmlLength)
			// (node-type check added to fix odd issue, probably caused by outerHTML being slightly different/longer than expected)
			if (content[0].nodeType == 3 && index >= contentHtmlIndex || index + length <= contentHtmlIndex + contentHtmlLength) // if html-to-be-removed starts in element, or ends in element
			{
				var index_local = index - contentHtmlIndex;
				var preHtml = contentHtml.substr(0, index_local);
				var postHtml = contentHtml.substr(index_local + length);

				//content.html(preHtml + postHtml);
				/*if (content[0].outerHTML)
					content[0].outerHTML = preHtml + postHtml;
				else*/
				content[0].textContent = preHtml + postHtml;
			}
			processedHtmlLength += contentHtmlLength;
		}
	};
	$.fn.insertHtml = function(htmlOrElement, index)
	{
		var result;

		var contents = $(this).contents().toArray();
		var processedHtmlLength = 0;
		for (var i in contents)
		{
			var content = $(contents[i]);
			//var contentHtml = content.html() || content.text();
			var contentHtml = content[0].outerHTML || content[0].textContent;
			var contentHtmlIndex = processedHtmlLength;
			var contentHtmlLength = contentHtml.length;
			//if (index >= contentHtmlIndex && index < contentHtmlIndex + contentHtmlLength)
			if (content[0].nodeType == 3 && index >= contentHtmlIndex && index <= contentHtmlIndex + contentHtmlLength)
			{
				var index_local = index - contentHtmlIndex;
				var preHtml = contentHtml.substr(0, index_local);
				var postHtml = contentHtml.substr(index_local + length);

				//content.html(preHtml);
				/*if (content[0].outerHTML)
					content[0].outerHTML = preHtml;
				else*/
				content[0].textContent = preHtml;
				result = htmlOrElement.insertAfter(content);
				if (postHtml.length)
					$(document.createTextNode(postHtml)).insertAfter(result);

				break;
			}
			processedHtmlLength += contentHtmlLength;
		}

		return result;
	};

	// vui
	// ==========

	$.fn.ApplyControlType = function(controlTypeName, options)
	{
		VUI.ApplyControlType(this, controlTypeName, options);
		return this;
	};

	// extra
	// ==========

	$.fn.GetScreenRect = function () { return new VRect(this.positionFrom().left, V.GetScreenHeight() - (this.positionFrom().top + this.height()), this.width(), this.height()); };
    $.fn.GetScreenRect_Normalized = function()
		{ return new VRect(this.positionFrom().left / V.GetScreenWidth(), (V.GetScreenHeight() - (this.positionFrom().top + this.height())) / V.GetScreenHeight(), this.width() / V.GetScreenWidth(), this.height() / V.GetScreenHeight()); };
};