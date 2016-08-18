$.prototype.slider_old = $.prototype.slider;
$.prototype.slider = function()
{
    var result = $.prototype.slider_old.apply(this, arguments);
    this.find(".ui-slider-handle").unbind("keydown"); // disable keyboard actions
    return result;
};
/*$.prototype.slider_noEvent = function() // remove change listener, set value, then add change listener back
{
	var oldChangeFunc = this.slider("option", "change");
	this.slider("option", "change", null);
	var result = $.prototype.slider.apply(this, arguments);
	this.slider("option", "change", oldChangeFunc);
	return result;
};*/

$.prototype.spinner_old = $.prototype.spinner;
$.prototype.spinner = function()
{
    if (arguments.length) // add in validation
    {
        var options = arguments[0];
        var oldChangeFunc = options.change;
        var self = this;
        options.change = function()
        {
            var clampedValue = Math.min(options.max, Math.max(options.min, (options.step || 1).toString().contains(".") ? parseFloat(self.val()) : parseInt(self.val())));
            self.val(clampedValue.toString() != "NaN" ? clampedValue : options.min);
            if (oldChangeFunc)
                oldChangeFunc.apply(this, arguments);
        };
    }
    return $.prototype.spinner_old.apply(this, arguments);
};
$.prototype.spinner_noEvent = function() // remove change listener, set value, then add change listener back
{
	var oldChangeFunc = this.spinner("option", "change");
	this.spinner("option", "change", null);
	var result = $.prototype.spinner.apply(this, arguments);
	this.spinner("option", "change", oldChangeFunc);
	return result;
};

// disable tab view "arrow keys to switch tab" feature (by default, anyway)
$.widget("ui.tabs", $.ui.tabs,
{
	options: {keyboard: true},
	_tabKeydown: function()
	{
		if(this.options.keyboard)
			this._super('_tabKeydown');
		else
			return false;
	}
});

// disables the JQueryDialog [clicking the default dialog button when Enter is pressed-down or held] functionality (custom code will trigger it on key-up)
$.prototype.dialog_old = $.prototype.dialog;
$.prototype.dialog = function()
{
    var result = $.prototype.dialog_old.apply(this, arguments);
    this.keypress(function(event)
    {
        if (event.keyCode == $.ui.keyCode.ENTER)
            return false;
    });
    return result;
};

// note: context-menu event will not trigger for an element, if right-click occurs over one of its child text-elements (must instead be a span, div, etc.)
$.prototype.contextmenu_old = $.prototype.contextmenu;
$.prototype.contextmenu = function()
{
	var s = this;
	var s_args = arguments;
	//if (arguments.length) // todo: have this make sure this is the initialization call
	if (!this.data("contextmenu_buildPrepared") && !(s_args[0] && s_args[0].buildNow)) // if delayed-build is not prepared, and okay to have a delayed-build, prepare a delayed-build
	{
		//this.one("mouseenter", function() { $.prototype.contextmenu_old.apply(s, s_args); });
		this.one("contextmenu", function(event, data)
		{
			$.prototype.contextmenu_old.apply(s, s_args); // initialize context-menu using stored args
			s.trigger(event); // pass current right-click/contextmenu event onto now-initialized context-menu
			return false;
		});
		this.data("contextmenu_buildPrepared", true);
	}
	else
		return $.prototype.contextmenu_old.apply(this, arguments);
};

$.prototype.val_orig = $.prototype.val;
$.prototype.val = function()
{
	if (this.is("input[type=number]") && arguments.length == 0)
	{
		var min = this.attr("min") != null ? parseInt(this.attr("min")) : Number.MIN_SAFE_INTEGER;
		var max = this.attr("max") != null ? parseInt(this.attr("max")) : Number.MAX_SAFE_INTEGER;
		var rawValue = parseFloat(this.val_orig());
		var checkedValue = Math.max(min, Math.min(max, rawValue));
		if (this.attr("step"))
			checkedValue = parseInt(checkedValue / this.attr("step")) * this.attr("step"); // same as: checkedValue = checkedValue.FloorToMultipleOf(this.attr("step"))
		if (checkedValue != rawValue)
			this.val_orig(checkedValue); // apply checked-value to the ui
		return checkedValue;
	}
	else
		return this.val_orig.apply(this, arguments);
};