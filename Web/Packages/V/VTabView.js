$.prototype.VTabView = function(/*optional:*/ options) // activate
{
	if ($("#VTabView_Style").length == 0)
	{
		var style = $(V.Multiline(function() {/*
		<style id="VTabView_Style">
		.VTabView > :first-child > div
		{
			display: inline-block;
			opacity: 1;
			background: url(Packages/Images/Tiling/Menu/Menu_B80.png) !important;
			margin: 0;
			padding: 1 5;
			border: none;
			border-radius: 3px 3px 0 0;
		}
		.VTabView > :first-child > div.active { background: url(Packages/Images/Tiling/Menu/Menu.png) !important; }
		.VTabView > :first-child > div:hover { color: #CCC; }
		.VTabView > :first-child > div.disabled { opacity: .5; }
		.VTabView > :nth-child(2) > div { height: 100%; display: none; }
		.VTabView > :nth-child(2) > div.active { display: block; }
		</style>
		*/}).trim());
		style.appendTo("body");
	}

    options = options || {};

    var s = this;
	s[0].vTabView = {};
	var data = s[0].vTabView;
    var buttonHolder = s.children().eq(0);
    var panelHolder = s.children().eq(1);

    if (!s.hasClass("VTabView")) // if not initialized
    {
        s.addClass("VTabView");
        buttonHolder.addClass("buttons");
        panelHolder.addClass("panels");

        var buttons = buttonHolder.find("div:not(.ignore)").filter(function () { return !$(this).parents(".buttons :not(.ignore)").length; });
        var panels = panelHolder.children();

        buttons.click(function() { s[0].SetTab(buttons.index($(this))); });
        s[0].GetTabIndex = function() { return buttons.index(buttons.filter(".active")); };
        s[0].SetTab = function(tabIndex)
        {
	        var oldTabIndex = s[0].GetTabIndex();
	        TryCall_OnX(s[0], options.preTabSet, tabIndex);

            buttons.removeClass("active");
            buttons.eq(tabIndex).addClass("active");
            panels.removeClass("active");
            panels.eq(tabIndex).addClass("active");

            TryCall_OnX(s[0], options.postTabSet, oldTabIndex);
        };

        s[0].SetTab(options.selectedTab != null ? options.selectedTab : 0);

	    data.options = options;
    }
    else
    	return data.options;
};