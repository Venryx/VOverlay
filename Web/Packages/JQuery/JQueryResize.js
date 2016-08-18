// jQuery resize event - v1.1 - 3/14/2010 (http://benalman.com/projects/jquery-resize-plugin)
// Copyright (c) 2010 "Cowboy" Ben Alman. Dual licensed under the MIT and GPL licenses. (http://benalman.com/about/license)

(function($, window, undefined)
{
    // a jQuery object containing all non-window elements to which the resize event is bound.
    var elems = $([]);

    // extend $.resize if it already exists, otherwise create it.
    var jq_resize = $.resize = $.extend($.resize, {});
    var timeout_id;

    // Property: jQuery.resize.delay. The numeric interval (in milliseconds) at which the resize event polling loop executes. Defaults to 250.
    jq_resize["delay"] = 250;

    // Property: jQuery.resize.throttleWindow. Throttle the native window object resize event to fire no more than once every <jQuery.resize.delay> milliseconds. Defaults to true.
    // Because the window object has its own resize event, it doesn't need to be provided by this plugin, and its execution can be left entirely up to the browser.
    // However, since certain browsers fire the resize event continuously while others do not, enabling this will throttle the window resize event, making event behavior consistent across all elements in all browsers.
    // While setting this property to false will disable window object resize event throttling, please note that this property must be changed before any window object resize event callbacks are bound.
    jq_resize["throttleWindow"] = true;

    // Event: resize event. Fired when an element's width or height changes.
    // Because browsers only provide this event for the window element, for other elements a polling loop is initialized, running every <jQuery.resize.delay> milliseconds to see if elements' dimensions have changed.
    // You may bind with either .resize(fn) or .bind("resize", fn), and unbind with .unbind("resize").
    // 
    // Usage:
    // 
    // > jQuery('selector').bind( 'resize', function(e)
    // > {
    // >     // element's width or height has changed!
    // >     [...]
    // > });
    // 
    // Additional Notes:
    // 
    // * The polling loop is not created until at least one callback is actually
    //   bound to the 'resize' event, and this single polling loop is shared
    //   across all elements.
    // 
    // Double firing issue in jQuery 1.3.2:
    // 
    // While this plugin works in jQuery 1.3.2, if an element's event callbacks
    // are manually triggered via .trigger( 'resize' ) or .resize() those
    // callbacks may double-fire, due to limitations in the jQuery 1.3.2 special
    // events system. This is not an issue when using jQuery 1.4+.
    // 
    // > // while this works in jQuery 1.4+
    // > $(elem).css({ width: new_w, height: new_h }).resize();
    // > 
    // > // in jQuery 1.3.2, you need to do this:
    // > var elem = $(elem);
    // > elem.css({ width: new_w, height: new_h });
    // > elem.data( 'resize-special-event', { width: elem.width(), height: elem.height() } );
    // > elem.resize();
    $.event.special["resize"] =
    {
        // called only when the first 'resize' event callback is bound per element
        setup: function()
        {
            if (this["setTimeout"] && !jq_resize["throttleWindow"]) // if element is window (i.e. has .setTimeout method), and override-native-resize-event is false, return false to bind the native event
                return false;

            var elem = $(this);
            elems = elems.add(elem); // add this element to the list of internal elements to monitor
            $.data(this, "resize-special-event", { w: elem.width(), h: elem.height() }); // initialize data store on the element
            if (elems.length === 1) // if this is the first element added, start the polling loop
                nextCheck();
        },

        // called only when the last 'resize' event callback is unbound per element
        teardown: function()
        {
            if (!jq_resize["throttleWindow"] && this["setTimeout"]) // if element is window (i.e. has .setTimeout method), and override-native-resize-event is false, return false to unbind the native event
                return false;

            var elem = $(this);
            elems = elems.not(elem);// remove this element from the list of internal elements to monitor
            elem.removeData("resize-special-event"); // remove any data stored on the element
            if (!elems.length) // if this is the last element removed, stop the polling loop
                clearTimeout(timeout_id);
        },

        // called every time a 'resize' event callback is bound per element (new in jQuery 1.4)
        add: function(handleObj)
        {
            if (!jq_resize["throttleWindow"] && this["setTimeout"])// if element is window (i.e. has .setTimeout method), and override-native-resize-event is false, return false so that the native event is not modified
                return false;

            var old_handler;

            // runs every time the event is triggered; updates the internal element data store with the width and height when the event is triggered manually, to avoid double-firing of the event callback (see the "Double firing issue in jQuery 1.3.2" comments above for more info)
            function new_handler(e, w, h)
            {
                var elem = $(this),
                    data = $.data(this, "resize-special-event");

                // if called from the polling loop, w and h will be passed in as arguments; if called manually, via .trigger( 'resize' ) or .resize(), those values will need to be computed
                data.w = w !== undefined ? w : elem.width();
                data.h = h !== undefined ? h : elem.height();

                old_handler.apply(this, arguments);
            };

            // this may seem a little complicated, but it normalizes the special event .add method between jQuery 1.4/1.4.1 and 1.4.2+
            if ($.isFunction(handleObj)) // 1.4, 1.4.1
            {
                old_handler = handleObj;
                return new_handler;
            }
            else // 1.4.2+
            {
                old_handler = handleObj.handler;
                handleObj.handler = new_handler;
            }
        }
    };

    function nextCheck()
    {
        // start the polling loop, asynchronously
        timeout_id = window["setTimeout"](function ()
        {
            // iterate over all elements to which the 'resize' event is bound
            elems.each(function()
            {
                var elem = $(this);
                var width = elem.width();
                var height = elem.height();
                var data = $.data(this, "resize-special-event");

                // if element size has changed since the last time, update the element data store and trigger the 'resize' event
                if (width !== data.w || height !== data.h)
                    elem.trigger("resize", [data.w = width, data.h = height]);
            });

            nextCheck(); // loop
        }, jq_resize["delay"]);
    };
})(jQuery, this);