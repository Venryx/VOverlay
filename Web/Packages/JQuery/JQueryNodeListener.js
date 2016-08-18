(function($) {
	$.fn.OnVisible = function(callback, onlyRunOnce, triggerIfAlreadyVisible) {
		var $this = $(this);

		var options = {
			keyframes: "\n\
@keyframes nodeInserted {from {clip: rect(1px, auto, auto, auto); } to {clip: rect(0px, auto, auto, auto); } }\n\
@-moz-keyframes nodeInserted {from {clip: rect(1px, auto, auto, auto); } to {clip: rect(0px, auto, auto, auto); } }\n\
@-webkit-keyframes nodeInserted {from {clip: rect(1px, auto, auto, auto); } to {clip: rect(0px, auto, auto, auto); } }\n\
@-ms-keyframes nodeInserted {from {clip: rect(1px, auto, auto, auto); } to {clip: rect(0px, auto, auto, auto); } }\n\
@-o-keyframes nodeInserted {from {clip: rect(1px, auto, auto, auto); } to {clip: rect(0px, auto, auto, auto); } }, ",
			selector: $this.selector,
			//stylesClass: $this.selector.replace(".", ""),
			//styles: $this.selector + " { animation-name: nodeInserted; -webkit-animation-name: nodeInserted; animation-duration: 0.001s; -webkit-animation-duration: 0.001s; }"
		}

		// if the keyframes aren't present, add them in a style element
		if (!$("style.domnodeappear-keyframes").length)
			$("head").append("<style class='domnodeappear-keyframes'>" + options.keyframes + "</style>");

		// add animation to selected element
		//$("head").append("<style class=\"" + options.stylesClass + "-animation\">" + options.styles + "</style>")

		if (triggerIfAlreadyVisible && $this.is(":visible")) {
			callback();
			if (onlyRunOnce) // if we were only supposed to run once anyway, we're done already
				return;
		}

		$this.css({animationName: "nodeInserted", "-webkit-animation-name": "nodeInserted", animationDuration: "0.001s", "-webkit-animation-duration": "0.001s"});

		// on animation start, execute the callback
		var handler = function(e) {
			var target = $(e.target);
			//if (e.originalEvent.animationName == "nodeInserted" && target.is(options.selector))
			//Log(e.target);
			if (e.originalEvent.animationName == "nodeInserted" && $this.get().Contains(e.target)) {
				callback.call(target);
				if (onlyRunOnce) {
					$this.css({animationName: "", "-webkit-animation-name": "", animationDuration: "", "-webkit-animation-duration": ""});
					$(document).off("animationstart webkitAnimationStart oanimationstart MSAnimationStart", handler);
				}
			}
		};
		$(document).on("animationstart webkitAnimationStart oanimationstart MSAnimationStart", handler);
	};
	//jQuery.fn.onAppear = jQuery.fn.DOMNodeAppear;
	$.fn.OnVisible_WithDelay = function(delay, callback, onlyRunOnce, triggerIfAlreadyVisible) {
		return this.OnVisible(function() { WaitXThenRun(delay, callback); }, onlyRunOnce, triggerIfAlreadyVisible);
	};
})(jQuery);

// another plugin like the above
// ==========

/*var insertionQ = (function()
{
	var sequence = 100,
		useTags,
		isAnimationSupported = false,
		animationstring = 'animationName',
		keyframeprefix = '',
		domPrefixes = 'Webkit Moz O ms Khtml'.split(' '),
		pfx = '',
		elm = document.createElement('div');

	if (elm.style.animationName)
		isAnimationSupported = true;

	if (isAnimationSupported === false)
	{
		for (var i = 0; i < domPrefixes.length; i++)
		{
			if (elm.style[domPrefixes[i] + 'AnimationName'] !== undefined)
			{
				pfx = domPrefixes[i];
				animationstring = pfx + 'AnimationName';
				keyframeprefix = '-' + pfx.toLowerCase() + '-';
				isAnimationSupported = true;
				break;
			}
		}
	}


	function listen(selector, callback)
	{
		var styleAnimation, animationName = 'insQ_' + (sequence++);

		var eventHandler = function(event)
		{
			if (event.animationName === animationName || event[animationstring] === animationName)
				if (!isTagged(event.target))
					callback(event.target);
		};

		styleAnimation = document.createElement('style');
		styleAnimation.innerHTML = '@keyframes ' + animationName + ' {  from {  clip: rect(1px, auto, auto, auto);  } to {  clip: rect(0px, auto, auto, auto); }  }' +
			"\n" + '@' + keyframeprefix + 'keyframes ' + animationName + ' {  from {  clip: rect(1px, auto, auto, auto);  } to {  clip: rect(0px, auto, auto, auto); }  }' +
			"\n" + selector + ' { animation-duration: 0.001s; animation-name: ' + animationName + '; ' +
			keyframeprefix + 'animation-duration: 0.001s; ' + keyframeprefix + 'animation-name: ' + animationName + '; ' +
			' } ';

		document.head.appendChild(styleAnimation);

		var bindAnimationLater = setTimeout(function()
		{
			document.addEventListener('animationstart', eventHandler, false);
			document.addEventListener('MSAnimationStart', eventHandler, false);
			document.addEventListener('webkitAnimationStart', eventHandler, false);
			//event support is not consistent with DOM prefixes
		}, 20); //starts listening later to skip elements found on startup. this might need tweaking

		return {
			destroy: function()
			{
				clearTimeout(bindAnimationLater);
				if (styleAnimation)
				{
					document.head.removeChild(styleAnimation);
					styleAnimation = null;
				}
				document.removeEventListener('animationstart', eventHandler);
				document.removeEventListener('MSAnimationStart', eventHandler);
				document.removeEventListener('webkitAnimationStart', eventHandler);
			}
		};
	}
	
	function tag(el) { el['-+-'] = true; }
	function isTagged(el) { return (useTags && (el['-+-'] === true)); }
	function topmostUntaggedParent(el)
	{
		if (isTagged(el.parentNode))
			return el;
		else
			return topmostUntaggedParent(el.parentNode);
	}
	function tagAll(e)
	{
		tag(e);
		e = e.firstChild;
		for (; e; e = e.nextSibling)
			if (e !== undefined && e.nodeType === 1)
				tagAll(e);
	}

	//aggregates multiple insertion events into a common parent
	function catchInsertions(selector, callback)
	{
		var insertions = [];
		//throttle summary
		var sumUp = (function()
		{
			var to;
			return function()
			{
				clearTimeout(to);
				to = setTimeout(function()
				{
					insertions.forEach(tagAll);
					callback(insertions);
					insertions = [];
				}, 10);
			};
		})();

		return listen(selector, function(el)
		{
			if (isTagged(el))
				return;
			tag(el);
			var myparent = topmostUntaggedParent(el);
			if (insertions.indexOf(myparent) < 0)
				insertions.push(myparent);
			sumUp();
		});
	}

	return function(selector, notag)
	{
		if (isAnimationSupported && selector.match(/[^{}]/))
		{
			useTags = (notag) ? false : true;
			if (useTags)
				tagAll(document.body); //prevents from catching things on show
			return {
				every: function(callback) { return listen(selector, callback); },
				summary: function(callback) { return catchInsertions(selector, callback); }
			};
		}
		else
			return false;
	}
})();*/