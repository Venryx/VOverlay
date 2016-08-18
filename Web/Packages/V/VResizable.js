function VResizable(control, options)
{
	var s = this;
	//s.control = control;

	var selfStartPercentage;
	var otherStartPercentage;

	var parentSize = function() { return control[["w", "e"].Contains(options.resizeDirection) ? "outerWidth" : "outerHeight"].apply(control.parent(), arguments); }
	var selfSizeOuter = function() { return control[["w", "e"].Contains(options.resizeDirection) ? "outerWidth" : "outerHeight"].apply(control, arguments); }
    var otherSizeOuter = function() { return options.shareSpaceWith[["w", "e"].Contains(options.resizeDirection) ? "outerWidth" : "outerHeight"].apply(options.shareSpaceWith, arguments); }
    control.resizable(
    {
        handles: options.resizeDirection,
        useOuterSize: true,
        start: function(event, ui)
        {
        	selfStartPercentage = (selfSizeOuter() / parentSize()) * 100;
        	otherStartPercentage = (otherSizeOuter() / parentSize()) * 100;
        },
        resize: function(event, ui)
        {
            var selfPercentage = (selfSizeOuter() / parentSize()) * 100;
            otherSizeOuter(((selfStartPercentage + otherStartPercentage) - selfPercentage) + "%");
        	//control.css("left", "0"); // fix for odd adding of "left: -[half of selfWidthOuter];" to css
        	//control.css("left", "initial !important");
        	//if (control.css("left") == "")
        	//if (control.css("left") == "auto")
            if (!control.attr("style").Contains("left:"))
            	control.addClass("forceLeftDisabled");

            if (options.resize)
            	options.resize();
        },
        stop: function(event, ui)
        {
            var selfPercentage = (selfSizeOuter() / parentSize()) * 100;
            selfSizeOuter(selfPercentage + "%");
            otherSizeOuter(((selfStartPercentage + otherStartPercentage) - selfPercentage) + "%");
            if (options.postResize)
                options.postResize();
        }
    });
    control.resizable("option").resize(null, {element: control});

	if (options.enabled === false)
		control.children(".ui-resizable-handle").addClass("disabled");

	if (options.addMarker)
		if (options.resizeDirection == "n")
			$("<div class='menuDarker' style='position: absolute; top: 0; height: 4; width: 100%;'></div>").appendTo(control);
		else if (options.resizeDirection == "e")
			$("<div class='menuDarker' style='position: absolute; right: 0; width: 4; height: 100%;'></div>").appendTo(control);
		else if (options.resizeDirection == "s")
			$("<div class='menuDarker' style='position: absolute; bottom: 0; height: 4; width: 100%;'></div>").appendTo(control);
		else //if (options.resizeDirection == "w")
			$("<div class='menuDarker' style='position: absolute; left: 0; width: 4; height: 100%;'></div>").appendTo(control);

	s.GetSize = function(/*o:*/ asPercentage)
	{
		if (asPercentage)
			return selfSizeOuter(((selfSizeOuter() / parentSize()) * 100) + "%");
		return selfSizeOuter();
	};
	s.SetSize = function(size) // can be number of pixels, or a percentage string
	{
		selfSizeOuter(size);
		var selfPercentage = (selfSizeOuter() / parentSize()) * 100;
        otherSizeOuter(((selfStartPercentage + otherStartPercentage) - selfPercentage) + "%");
	};
}

$.prototype.VResizable = function(options) // shareSpaceWith, resizeDirection
{
    var s = this;
	if (s[0].vResizable == null)
		s[0].vResizable = new VResizable(s, options);
	return s[0].vResizable;
};