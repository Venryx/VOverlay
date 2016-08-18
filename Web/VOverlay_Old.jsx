//VOverlay.SetBaseClass(Node); //Node.SetAsBaseClassFor = //SetBaseClassTo(Node).For = 
Node.SetAsBaseClassFor = function VOverlay() {
	var s = this.CallBaseConstructor(null); // s for self
	//var p = s.__proto__; //arguments.callee.prototype; //VOverlay.prototype; // p for prototype

	window.VO = s; // maybe temp; lets VOverlay be referenced using VO variable even before it's done initializing

	// static
	// ==========

	s.SubmitChange = function(change, group) {
		group = group || ContextGroup.Local_CSAndUI;

		//if (change.gameTime == -1) // if there aren't any conflicts possible (i.e. not a multiplayer-game change)
		if (change.PreApply)
			change.PreApply();
		if (change.sourceContext == "ui" && (group == ContextGroup.Local_CS || group == ContextGroup.Local_CSAndUI)) // if change originated in this context, and CS is part of target
			CSBridge.CallCS("VO.main.SubmitChange", change, ContextGroup.Local_CS); // send change submission to C# v-tree
		if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI)
			change.Apply();
	};

	s.GetNodeByNodePath = function(path, /*o:*/ vdfPath, options) { // o stands for optional
		var currentNode = null; // (can also be a list or map)
		var currentNode_indexInVDFPath = -1;
		for (var i in path.nodes) {
			var pathNode = path.nodes[i];
			if (pathNode.voRoot)
				currentNode = VO;
			else if (pathNode.vdfRoot)
				currentNode = vdfPath.rootNode.obj;
			else if (pathNode.nodeRoot)
				//currentNode = options.messages.First(function() { return this instanceof DisconnectedRootReference; }).node;
				currentNode = vdfPath.nodes.First(function() { return this.obj instanceof Node; }).obj;
			else if (pathNode.currentParent) {
				currentNode_indexInVDFPath = vdfPath.nodes.Count - 2; // skip the last node (the last node is the prop/reference, so can't also be the reference value)
				currentNode = vdfPath.nodes[currentNode_indexInVDFPath].obj;
			}
			else if (pathNode.moveUp) {
				if (currentNode_indexInVDFPath == -1) // if no current-parent path-node before this, pretend there was one
					currentNode_indexInVDFPath = vdfPath.nodes.Count - 2; // skip the last node (the last node is the prop/reference, so can't also be the reference value)
					//currentNode = vdfPath.nodes[currentNode_indexInVDFPath].obj;

				currentNode_indexInVDFPath--;
				currentNode = vdfPath.nodes[currentNode_indexInVDFPath].obj;
			}
			else
				if (pathNode.listIndex != -1)
					currentNode = currentNode[pathNode.listIndex];
				else if (pathNode.mapKeyIndex != -1)
					currentNode = currentNode.keys[pathNode.mapKeyIndex];
				else if (pathNode.mapKey_str != null)
					if (pathNode.mapKey_str.contains(">")) // if has type metadata, from NodePathNode constructor
						currentNode = currentNode.Get(FromVDF(pathNode.mapKey_str));
					else if (pathNode.mapKey_str.contains("[embedded path]")) { // if embedded-path, from NodePathNode constructor
						var nodePath = NodePath.Deserialize(new VDFNode(pathNode.mapKey_str.replace(/\[embedded path\]/g, "").replace(/\[fs\]/g, "/")));
						var key = VO.GetNodeByNodePath(nodePath, vdfPath);
						currentNode = currentNode.Get(key);
					}
					else {
						// make-so: this workaround isn't needed (i.e. Dictionarys make use of a GetHashCode and/or Equals method, for Vector3i objects)
						if (currentNode.keyType == "Vector3i") {
							debugger;
							var key_inDictionary = currentNode.keys.First(function(a) { return a.toString() == pathNode.mapKey_str; });
							currentNode = currentNode.Get(key_inDictionary);
						}
						else
							currentNode = currentNode.Get(pathNode.mapKey_str);
					}
				else
					currentNode = currentNode[pathNode.prop_str];

			var currentNodes = (currentNodes || []).CAdd(currentNode);
			if (currentNode == null) // for debugging
				debugger;
		}
		return currentNode;
	};
	s.Node_BroadcastMessage = function(path, group, methodName, args___) {
		var args = V.Slice(arguments, 3);
		s.GetNodeByNodePath(path).BroadcastMessage.apply(s.GetNodeByNodePath(path), [group, methodName].concat(args));
	};
	s.Node_SendMessage = function(path, group, methodName, args___) {
		var args = V.Slice(arguments, 3);
		return s.GetNodeByNodePath(path).SendMessage.apply(s.GetNodeByNodePath(path), [group, methodName].concat(args));
	};

	// instance
	// ==========

	//s.initializedInJS = false;

	s.p("mainMenu", "MainMenu").set = null;
	s.p("console", "Console").set = null;
	s.p("settings", "Settings").set = null;
	s.p("maps", "Maps").set = null;
	s.p("biomes", "Biomes").set = null;
	s.p("objects", "Objects").set = null;
	s.p("modules", "Modules").set = null;
	s.p("matches", "Matches").set = null;
	s.p("live", "Live").set = null;

	s.rootFolderPath = null;
	//s.prefs = null;

	s.viewFrame = 0;
	// maybe temp
	if (!inUnity) {
		s.viewFramesPerSecond = 10;
		s.viewFrameTimer = new Timer(1 / s.viewFramesPerSecond, function() {
			s.a("viewFrame").increase = 1;

			var match = VO.live.liveMatch;
			if (match)
				match.a("dataFrame").increase = 1;
		});
		//s.viewFrameTimer.Start(); // now triggered by f2
	}

	s.LoadLazy_Map = function(attachPoint, name) {
		if (name == "[live match map]")
			return VO.live.liveMatch.map;

		if (s.maps != null && s.maps.maps != null && s.maps.maps.Any(function() { return this.name == name; }))
			return s.maps.maps.First(function() { return this.name == name; });
		s.GetSafeChild("maps").extraMethod = function maps_PostAdd(map) { if (map.name == name) AttachLazyLoadValue(attachPoint, map); };
		return null;
	};
	s.LoadLazy_Soil = function(attachPoint, name) {
		if (s.maps != null && s.maps.soils != null && s.maps.soils.Any(function() { return this.name == name; }))
			return s.maps.soils.First(function() { return this.name == name; });
		s.GetSafeChild("maps").extraMethod = function soils_PostAdd(soil) { if (soil.name == name) AttachLazyLoadValue(attachPoint, soil); };
		return null;
	};
	s.LoadLazy_Biome = function(attachPoint, name) {
		if (s.biomes != null && s.biomes.biomes != null && s.biomes.biomes.Any(function() { return this.name == name; }))
			return s.biomes.biomes.First(function() { return this.name == name; });
		s.GetSafeChild("biomes").extraMethod = function biomes_PostAdd(biome) { if (biome.name == name) AttachLazyLoadValue(attachPoint, biome); };
		return null;
	};
	s.LoadLazy_VObject_Type = function(attachPoint, name) {
		if (s.objects != null && s.objects.objects != null && s.objects.objects.Any(function() { return this.name == name; }))
			return s.objects.objects.First(function() { return this.name == name; });
		s.GetSafeChild("objects").extraMethod = function objects_PostAdd(obj2) { if (obj2.name == name) AttachLazyLoadValue(attachPoint, obj2); };
		return null;
	};
	s.LoadLazy_Module = function(attachPoint, name) {
		if (s.modules != null && s.modules.modules != null && s.modules.modules.Any(function() { return this.name == name; }))
			return s.modules.modules.First(function() { return this.name == name; });
		s.GetSafeChild("modules").extraMethod = function modules_PostAdd(module) { if (module.name == name) AttachLazyLoadValue(attachPoint, module); };
		return null;
	};

	var loadLazy_contextGroup = ContextGroup.Local_UI; // JS side never lazy-loads stuff independently, so no need for to-cs changes [ok, exact opposite of C# side's comment... which is correct?]
	function AttachLazyLoadValue(attachPoint, value) {
		if (attachPoint.map_key != null)
			s.SubmitChange(new Change_Add_Dictionary(attachPoint.parent, attachPoint.prop, attachPoint.map_key, value, true).AddMessages("from lazy load"), loadLazy_contextGroup);
		else if (attachPoint.list_index != -1)
			s.SubmitChange(new Change_SetItem_List(attachPoint.parent, attachPoint.prop, attachPoint.list_index, value).AddMessages("from lazy load"), loadLazy_contextGroup);
		else
			s.SubmitChange(new Change_Set(attachPoint.parent, attachPoint.prop, value).AddMessages("from lazy load"), loadLazy_contextGroup);
	}

	// early ui setup
	// ==========

	var R = s.R = function(selector) { return s.root.find(selector); };
	s.root = $(V.Multiline(function() {/*
	<div class="VOverlay">
		<!-- have overflow hidden if in Unity (i.e. if in a release--possibly, anyway) -->
		<!-- <style>
		.VOverlay.inUnity > #layer1, .VOverlay.inUnity > #layer2, .VOverlay.inUnity > #layer3 { overflow: hidden; }
		</style> -->
		<style>
		@body.inUnity { padding-top: 50; padding-bottom: 100; }
		@.VOverlay.inUnity { position: relative; height: calc(100% - 150px); }

		@.VOverlay.inUnity { position: fixed; left: 0; right: 0; top: 50; bottom: 100; }
		@.VOverlay:not(.inUnity) { position: relative; height: 100%; }

		.VOverlay.inUnity { position: fixed; left: 0; right: 0; top: 0; bottom: 0; }
		.VOverlay:not(.inUnity) { position: relative; height: 100%; }
		.VOverlay:not(.inUnity) .fake3DObjects { display: block !important; }

		.VOverlay.inUnity .hideInUnity { display: none; }
		</style>

		<!-- <div id="screen" style="position: absolute; width: 100%; height: 100%;"></div> -->
		<div id="screenCenter" style="position: absolute; left: 50%; top: 50%; width: 0; height: 0;"></div>
		<div id="hiddenPersistentHolder" style="position: absolute; left: -1000; top: -1000; width: 1000; height: 1000; overflow: hidden;"></div>
        <div id="hiddenTempHolder" style="position: absolute; left: -1000; top: -1000; width: 1000; height: 1000; overflow: hidden;"></div>
		<div id="vuiTempHolder" style="position: absolute; left: -1000; top: -1000; width: 1000; height: 1000; overflow: hidden;"></div>
		<style id="opacityApplier"></style>
		<div id="layer0" class="clickThrough" style="position: absolute; width: 100%; height: 100%; z-index: 0;">
			<div id="backdrop" style="position: relative; width: 100%; height: 100%;"></div>
		</div>
		<div id="layer1" class="clickThrough" style="position: absolute; width: 100%; height: 100%; z-index: 1;">
			<style>
			#exit:hover { background: rgba(0, 0, 0, 0.7) !important; }

            .topMenuL2 {
                display: inline-block;
                border-radius: 19px;
            }
            .topMenuL2:not(:first-child) { margin-left: 7; }
            .topMenuL2Button {
                display: inline-block;
                margin-left: -3;
			    padding: 9 15;
                border: solid rgb(50, 50, 50);
                border-width: 0 0 0 1px;
			    background: rgba(0, 0, 0, 0.15);
			    color: #AAA;
			    font-size: 14px;
                font-weight: bold;
			    cursor: pointer;
		    }
            .topMenuL2Button:first-child {
                margin-left: 0;
                padding-left: 18;
                border: none !important;
                border-radius: 19px 0 0 19px;
                background: transparent !important;
            }
            .topMenuL2Button:first-child:hover { background: rgba(0, 0, 0, 0.7) !important; }
            .topMenuL2Button.lastVisibleChild {
                border-radius: 0 19px 19px 0;
                padding-right: 18;
            }
		    .topMenuL2Button:hover { background: rgba(0, 0, 0, 0.7); }
		    .topMenuL2Button > div {
			    position: absolute;
			    margin-left: -10;
			    margin-top: -2;
			    width: 3;
			    height: 3;
			    border-radius: 3px;
			    background: green;
			    display: none;
		    }
		    .topMenuL2Button.active > div { display: block !important; }
			</style>
			<div id="topMenuController" class="button" style="position: absolute; top: 0; right: 0; width: 24; height: 24; padding: 3 11 4; box-sizing: border-box; z-index: 2; border-radius: 0 0 0 32px; background-image: url(Packages/Images/Arrows/TopRightArrow_16_White.png); background-repeat: no-repeat; background-position: 8px 5px; background-size: 12px;"></div>
			<div id="topMenu" class="centerDownShadow20 menu" style="position: relative; z-index: 1; width: 100%;">
				<div id="exit" class="topMenuL0Button" style="position: absolute; float: left; display: inline-block; z-index: 1; padding: 9 15; padding-right: 18; background: rgba(0, 0, 0, 0.2); color: #AAA; font-size: 14px; font-weight: bold; cursor: pointer; border-radius: 0 0 19px 0;">Exit</div>
				<div style="text-align: center;">
                    <div class="menuDark topMenuL2">
                        <div id="Settings" class="topMenuL2Button"><div></div>Settings</div>
                        <div id="Console" class="topMenuL2Button"><div></div>Console</div>
                        <div id="Maps" class="topMenuL2Button"><div></div>Maps</div>
						<div id="Biomes" class="topMenuL2Button"><div></div>Biomes</div>
						<div id="Objects" class="topMenuL2Button"><div></div>Objects</div>
						<div id="Modules" class="topMenuL2Button"><div></div>Modules</div>
				        <div id="Matches" class="topMenuL2Button lastVisibleChild"><div></div>Matches</div>
                        <div id="Live" class="topMenuL2Button" style="display: none;"><div></div>Live</div>
                    </div>
				</div>
			</div>
			<div id="page" class="clickThrough" style="position: relative; width: 100%; height: 100%;"></div>
            <div id="bottomPanel" class="borderBox centerUpShadow20" style="display: none; position: absolute; width: 100%; height: 300; border: solid #000; border-width: 3px 0 0 0;"></div>
		</div>
		<div id="layer2" class="clickThrough" style="position: absolute; width: 100%; height: 100%; z-index: 2;">
			<div id="overlay" class="clickThrough" style="position: relative; width: 100%; height: 100%;"></div>
			<!-- <div id="terrainHoverInfo" class="clickThrough" style="position: absolute; left: 10; bottom: 25;"></div> -->
            <div id="fps" class="clickThrough" style="position: absolute; left: 7; bottom: 7;"></div>
		</div>
	</div>
	*/}).trim());

	// methods: Unity-linked
	// ==========

    s.Tab_OnDown = function(shiftDown) { $(document).trigger("keydown", {keyCode: $.ui.keyCode.TAB, shiftDown: shiftDown}); }; //9
	//s.Enter_OnDown = function() { $(document).trigger("keydown.contextMenu", {keyCode: $.ui.keyCode.ENTER}); }; //13
	s.Enter_OnDown = function() { $(document).trigger("keydown", {keyCode: $.ui.keyCode.ENTER}); }; //13
	/*s.SelectAll = function() // simulate Ctrl+A's select-all; used when in editor
	{
        if ($(":focus").length)
            $(":focus").select();
        if ($(GetSelectionStartControl()).parents().filter(function() { return $(this).is("[textArea]"); }).length)
        {
            var textArea = $(getSelection().anchorNode).parent();
            //SetSelection(textArea[0].firstChild, 0, textArea.text().length);
            getSelection().selectAllChildren(textArea[0]);
        }
	};*/
	s.PreViewClose = function()
	{
		//for (var i = 1; i <= 3; i++) // notify pages at level-1 and down that they're about to close ('page 0' is essentially the window)
	    //	TryCall(window["Page" + i + "Root"]["PreClose"]);
	    if (s.openPage)
	        s.ClosePage(s.openPage, null);
	};

	// methods
	// ==========

	s.SetFPSText = function(text) { R("#fps").text(text); };

	s.GetSubButton = function(sub) { return R("#" + sub); };
	s.IsSubButtonActive = function(sub) { return s.GetSubButton(sub).hasClass("active"); };
	s.SetSubButtonActive = function(sub, active) {
		if (active)
			s.GetSubButton(sub).addClass("active");
		else
			s.GetSubButton(sub).removeClass("active");
	};

	s.IsPageOpen = function(page) { return s.openPage == page; };
	s.OpenPage = function(page, /*optional:*/ callback) {
		if (s.openPage)
			s.ClosePage(s.openPage, page);

		s[page].SendMessage(ContextGroup.Local_CSAndUI, "PreOpen");
		//s[page].Attach(R("#page"));
		s[page].a("visible").set = true;
		s.openPage = page;
		$("title").html("VO - " + page); //document.title = "VO - " + page;

		//self.SetSubButtonActive(page, true);
		s[page].SendMessage(ContextGroup.Local_CSAndUI, "PostOpen");
	};
	s.ClosePage = function(page, /*optional:*/ newPage) {
		if (!s.IsPageOpen(page))
			return; // nothing to close
		s[page].SendMessage(ContextGroup.Local_CSAndUI, "PreClosePage", newPage);
		s[page].a("visible").set = false; //s[page].Hide();
		s.openPage = null;
		$("title").html("VO"); //document.title = "VO";

		//self.SetSubButtonActive(page, false);
		s[page].SendMessage(ContextGroup.Local_CSAndUI, "PostClosePage", newPage);
	};
	s.TogglePageOpen = function(page) {
		if (s.IsPageOpen(page)) {
	        s.ClosePage(page);
	        s.OpenPage("mainMenu");
	    }
	    else
	        s.OpenPage(page);
	};

	// todo: make three blocking levels: none, ui, full-screen (and have 'ui' be used for map loading screens and the like)
	s.ShowLoadingScreen = function(message, captureMouseFocus) {
		if (!R("#loadingScreenMessage").length) {
			var root = $("<div style='background: rgba(0, 0, 0, .5); height: 100%;'>").appendTo(R("#overlay"));
			if (!captureMouseFocus)
				root.addClass("clickThroughChain");
			$("<div id='loadingScreenMessage' style='position: absolute; left: 50%; top: 50%; transform: translate(-50%, -50%); -webkit-transform: translate(-50%, -50%); font-size: 23; color: white;'>").appendTo(root).html(message);
	    }
	    else
            R("#loadingScreenMessage").html(message);
	};
	s.HideLoadingScreen = function() {
		R("#overlay").html("");
	};

	s.ResizePage = function() {
		var pageAndPanelsHeight = s.root.height() - (R("#topMenu").css("display") == "none" ? 0 : R("#topMenu").height());
		R("#page").css("height", pageAndPanelsHeight - ($("#bottomPanel").css("display") != "none" && $("#bottomPanel").outerHeight()));
		//R("#page").css("height", pageAndPanelsHeight);
	};
	s.ShowMenu = function() {
		R("#topMenuController").css("background-image", "url(Packages/Images/Arrows/TopRightArrow_16_White.png)").css("background-position", "8px 5px");
		R("#topMenu").css("display", "inherit");
		s.ResizePage();
	};
	s.HideMenu = function() {
		R("#topMenuController").css("background-image", "url(Packages/Images/Arrows/BottomLeftArrow_16_White.png)").css("background-position", "10px 3px");
		R("#topMenu").css("display", "none");
		s.ResizePage();
	};

	s.frame = 0;
	s.Update = function(frame) // number sent is actually the frame-count, but we'll just use the frame-count everywhere as if it were the frame-index
	{
		s.frame = frame;

		var viewport = R(".mainCameraViewport:visible");
		if (viewport.length) {
			var rect = viewport.GetScreenRect_Normalized();
			if (rect.y >= 0) // maybe temp; fixes that rect is sometimes invalid when switching from/to the Objects page
				if (rect.width > 0 && rect.height > 0 && !rect.Equals(s.lastAppliedRect)) {
					s.SendMessage(ContextGroup.Local_CS, "SetMainCameraRect", rect);
					VO.live.SendMessage(ContextGroup.Local_CS, "SetMinimapCameraRect", rect); // maybe temp
					s.lastAppliedRect = rect;
				}
		}

		if (VO.live)
			VO.live.CallMethod("Update", frame);
	};

	// old methods
	// ==========

	s.Attach = function(holder) {
		s.root.appendTo(holder);

		var vars = GetUrlVars(CurrentUrl());

		// special css marker
		// ----------

		if (InUnity() || vars.pretendInUnity)
			//$("body").addClass("inUnity");
			s.root.addClass("inUnity");

		// early ui activation
		// ----------

		$(window).resize(function() {
			WaitXThenRun(0, function() {
				//self.Frame2.AlignMenu();
				//self.VOverlay.AlignMenu();
				s.ResizePage();
			});
		});
		$(document).tooltip({
			track: true,
			hide: { duration: "fast" },
			//content: function(callback) { callback($(this).prop('title').replace(/\|/g, '<br>')); },
			content: function(callback) { callback($(this).prop('title').replace(/\n/g, '<br>')); },
			open: function(event, ui) { $(".ui-tooltip:not(:last)").remove(); }
		});
		$(document).on("mouseenter", "*", function(event, ui) {
			if ($(this).attr('title') == null && $(this).parents('[title]').length == 0) // if we (and ancestors) have no tooltip
				$(".ui-tooltip").remove();
		});
		$(document).on("mousemove", "*", function(event, ui) {
			if (event.handledGlobally)
				return;
			event.handledGlobally = true;

			if (InUnity())
				UpdateWebUIHasMouseFocus(!$(this).is("#backdrop"));
		});
		/*$(document).on("keypress", "[text-area]", function(event) { // forces fake textAreas to always only have text content after typing
            var textArea = $(this);
		    var oldTextLength = textArea.text().length;
		    var oldSelection = GetSelection();
		    WaitXThenRun(0, function() {
		        textArea.text(textArea.text()); // combine all child text-nodes into one
		        //SetSelection(textArea[0].firstChild, oldSelection.startOffset, oldSelection.endOffset + (textArea.text().length - oldTextLength));
		        SetSelection(textArea[0].firstChild, oldSelection.endOffset + (textArea.text().length - oldTextLength), oldSelection.endOffset + (textArea.text().length - oldTextLength));
		        event.preventDefault();
		    });

		    /*if (event.keyCode == 13) {
		        var textArea = $(this);
		        var selection = window.getSelection();
		        var selectionStart = selection.getRangeAt(0).startOffset; //textArea[0].selectionStart;
		        var selectionEnd = selection.getRangeAt(0).endOffset; //textArea[0].selectionEnd;
		            
		        var originalText = textArea.text();
		        var preText = originalText.substring(0, selectionStart);
		        var selectedText = originalText.substring(selectionStart, selectionEnd); //originalText.substr(selectionStart, selectionEnd - selectionStart);
		        var postText = originalText.substring(selectionEnd);

		        textArea.text(preText + "\n" + postText);
		        //textArea[0].selectionStart = selectionStart + 1;
		        //textArea[0].selectionEnd = selectionEnd + 1;
		        SetSelection(selection.anchorNode.firstChild, selectionStart + 1, selectionEnd - selectedText.length + 1);
		        event.preventDefault();
		    }*#/
		});
		$(document).on("paste", "[text-area]", function(event) { // forces fake textAreas to always only have text content after pasting
		    var textArea = $(this);
		    var oldTextLength = textArea.text().length;
		    var oldSelection = GetSelection();
		    WaitXThenRun(0, function() {
		        textArea.text(textArea.text()); // combine all child text-nodes into one
		        //SetSelection(textArea[0].firstChild, oldSelection.startOffset, oldSelection.endOffset + (textArea.text().length - oldTextLength));
		        SetSelection(textArea[0].firstChild, oldSelection.endOffset + (textArea.text().length - oldTextLength), oldSelection.endOffset + (textArea.text().length - oldTextLength));
		        event.preventDefault();
		    });
		});*/
		$(document).on("focus", "[text-area]", function () { this.textOnFocus = $(this).text(); });
	    $(document).on("blur", "[text-area]", function() {
	        if ($(this).text() != this.textOnFocus)
	            $(this).trigger("vChange");
	    });

	    /*document.oncopy = function() {
	        var selection = window.getSelection();
	        var oldSelectionRange = selection.getRangeAt(0);
	        if (!$(oldSelectionRange.startContainer).is("div.textarea")) // we only need to use a text-copy-buffer if selected text was in a custom textarea
	            return;

	        var tempDiv = $("<div class='selectable' style='position: absolute; left: -100000;'>").appendTo("body");
	        tempDiv.text(selection);
	        selection.selectAllChildren(tempDiv[0]);
	        WaitXThenRun(0, function()
	        {
	            tempDiv.remove();
	            selection.removeAllRanges();
	            selection.addRange(oldSelectionRange);
	        });
	    }*/
	    document.onpaste = function(event) { // intercept paste actions, to make sure we're only pasting text
	        event.preventDefault(); // cancel paste
	        var text = event.clipboardData.getData("text/plain"); // get text representation of clipboard
	        document.execCommand("insertText", false, text); // insert text manually
	    };

		// quick menu closer
		$(document).click(function(e) {
			if ($(e.target).hasClass("quickMenuToggler") || $(e.target).closest(".quickMenu").length > 0 || $(e.target).closest("html").length == 0)
				return;
			CloseQuickMenus();
		});

		// set up functionless droppability on backdrop (it's expected to have a droppability widget)
		R("#backdrop").droppable({});
		R("#backdrop")[0].layersOver = 0;

		// disables backdrop's droppability when dragging over anything in pageHolder, popupSourceHolder, or overlaysHolder (otherwise it always triggers)
	    /*var backdrop = R("#backdrop");
        $("#pageHolder, #popupSourceHolder, #overlaysHolder").droppable({
			over: function() {
				backdrop[0].layersOver++;
				backdrop.droppable("disable");
			},
			out: function() {
				backdrop[0].layersOver--;
				if (backdrop[0].layersOver <= 0) backdrop.droppable("enable");
			},
			drop: function() { backdrop[0].layersOver = 0; }
		});*/

		$(document).focusin(function(event, ui) { UpdateUIHasKeyboardFocus(); });
		$(document).focusout(function() { UpdateUIHasKeyboardFocus(); });

    	// note: this watcher gives you the call-stack of the element-adder-code
    	// options: DOMAttrModified, DOMAttributeNameChanged, DOMCharacterDataModified, DOMElementNameChanged, DOMNodeInserted, DOMNodeInsertedIntoDocument, DOMNodeRemoved, DOMNodeRemovedFromDocument, DOMSubtreeModified
		/*$(document).bind("DOMNodeInserted", function(event) {
			var target = $(event.target);
	    });*/

		//$("div").OnVisible(function() { var target = this;
	    //insertionQ("body *").every(function(element) { var target = $(element);
		var MutationObserver = window.MutationObserver || window.WebKitMutationObserver;
		var observer = new MutationObserver(function(mutations, observer) {
			mutations.forEach(function(mutation) {
				var addedNodes = V.CloneArray(mutation.addedNodes); // (for element trees/subtrees, only includes each tree's root, I believe)
				for (var i in addedNodes) {
					var target = $(addedNodes[i]);

					// remove focusability of all controls other than the input-absorbing ones
					var focusable = target.is("input[type=text]") || target.is("input[type=number]") || target.is("option"); //target.is("button")
					if (!focusable && !target.is("div"))
						target.attr("tabIndex", "-1");

					// fix for dialogs-being-dragged-outside-of-viewport issue
					if (target.is(".ui-dialog")) {
						target.draggable({containment: ".VOverlay"});
						if (target.hasClass("ui-resizable"))
							target.resizable({containment: ".VOverlay"});
					}

					// useful for, e.g.: <input id='[auto]' type='checkbox'><label for='[prev]'>
					target.find("[id=\"@auto\"]").each(function() { $(this).SetID(); });
					target.find("[for=\"@prev\"]").each(function() { $(this).attr("for", $(this).prev().attr("id")); });
					
					// fix for "transform" css element not working in Unity
					//if (InUnity() && target.attr("style") && target.attr("style").contains("transform:"))
					//	target.attr("style", target.attr("style").replace("transform:", "-webkit-transform"));
					if (InUnity()) {
						var descendants = V.GetElementDescendants(target, true);
						for (var i in descendants) {
							var descendant = descendants[i];
							if (descendant.attr("style") && descendant.attr("style").contains("transform:") && !descendant.attr("style").contains("-webkit-transform:"))
								descendant.attr("style", descendant.attr("style").replace("transform:", "-webkit-transform:"));
						}
					}

					//if (target.attr("style").startsWith("position: absolute; z-index: -1; top: 0px; left: 0px; right: 0px; height: 663px;"))
					//	debugger;
				}
			});
		});
		observer.observe(document, {subtree: true, childList: true}); // observe which element, and for what kind of mutations

		// fix for arrow-key-movement of draggables
		// for keycode values: http://keycode.info
		$(document).on("keydown", function(event, data) {
			//if (event.which == null)
			//	return;

			/*if (event.which == 112) // F1
			{
				if (!InUnity()) // for in-browser testing
					Frame.TogglePopupOpen("Testing");
			}*/
			if (event.which >= 37 && event.which <= 40) { // arrow keys
				if ($(".ui-draggable-dragging, .ui-sortable-helper").length > 0) // if there's an active draggable/sortable
					return false;
			}

			// text-area tab stuff
			if (event.which == $.ui.keyCode.TAB) {
				var textArea = $("div[text-area]:focus");
				if (textArea.length) {
					var selection = window.getSelection();
					var selectionStart = selection.getRangeAt(0).startOffset; //textArea[0].selectionStart;
					var selectionEnd = selection.getRangeAt(0).endOffset; //textArea[0].selectionEnd;

					var originalText = textArea.text();
					var preText = originalText.substring(0, selectionStart);
					var selectedText = originalText.substring(selectionStart, selectionEnd); //originalText.substr(selectionStart, selectionEnd - selectionStart);
					var postText = originalText.substring(selectionEnd);

					if (selectedText.length)
						if (shiftDown) {
							for (var i = selectionStart; i >= 0; i--)
								if ((i > 0 && preText[i - 1] == "\n") || i == 0) {
									if (preText[i] == "\t") {
										preText = preText.substring(0, i) + preText.substring(i + 1);
										selectionStart--;
										selectionEnd--;
									}
									break;
								}

							//if (!selectedText.match(/\n([^\t])/))
							var newSelectedText = selectedText.replace(/\n\t/g, "\n");
							if ((selectionStart == 0 || originalText[selectionStart - 1] == "\n") && newSelectedText.startsWith("\t"))
								newSelectedText = newSelectedText.substring(1);
							textArea.text(preText + newSelectedText + postText);
							//textArea[0].selectionStart = selectionStart;
							//textArea[0].selectionEnd = selectionEnd + (newSelectedText.length - selectedText.length);
							SetSelection(textArea[0].firstChild, selectionStart, selectionEnd + (newSelectedText.length - selectedText.length));
						}
						else {
							for (var i = selectionStart; i >= 0; i--) // go back till we are at first-char-of-line
								if ((i > 0 && preText[i - 1] == "\n") || i == 0) {
									preText = preText.substring(0, i) + "\t" + preText.substring(i);
									selectionStart++;
									selectionEnd++;
									break;
								}

							var newSelectedText = selectedText.replace(/\n/g, "\n\t");
							textArea.text(preText + newSelectedText + postText);
							//textArea[0].selectionStart = selectionStart;
							//textArea[0].selectionEnd = selectionEnd + (newSelectedText.length - selectedText.length);
							SetSelection(textArea[0].firstChild, selectionStart, selectionEnd + (newSelectedText.length - selectedText.length));
						}
					else
						if (shiftDown) {
							if (selectionStart > 0 && originalText[selectionStart - 1] == "\t") {
								textArea.text(originalText.substring(0, selectionStart - 1) + originalText.substring(selectionStart));
								//textArea[0].selectionStart = textArea[0].selectionEnd = selectionStart - 1;
								SetSelection(textArea[0].firstChild, selectionStart - 1, selectionEnd - 1);
							}
						}
						else {
							textArea.text(originalText.substring(0, selectionStart) + "\t" + originalText.substring(selectionStart));
							//textArea[0].selectionStart = textArea[0].selectionEnd = selectionStart + 1;
							SetSelection(textArea[0].firstChild, selectionStart + 1, selectionEnd + 1);
						}
				}
			}

			//if (event.which == $.ui.keyCode.ENTER) // apparently pressing Enter doesn't automatically/natively press the enter button (for text-input dialogs anyway; I suppose because the text-input has the focus in that case)
			if ((data != null && data.keyCode == $.ui.keyCode.ENTER) || (!InUnity() && event.which == $.ui.keyCode.ENTER)) // the native event is too slow, so listen for custom-triggered-event's data pack
				$(".ui-dialog").filter(function() { return ($(this).data("frameAtCreation") || -1) < VO.frame; }).find(".ui-dialog-buttonset button").eq(0).trigger("click");

			if (!inUnity) {
				/*if (event.which == 45) //$.ui.keyCode.INSERT)
				{
					self.Tab_OnDown(false);
					event.preventDefault();
					return false;
				}
				if (event.shiftKey && event.which == 49) //17 // ctrl+1
				{
					var selection = getSelection();
					var range = selection.rangeCount ? selection.getRangeAt(0) : null;
					Log(range ? selection.anchorNode + ";" + range.startOffset + ";" + range.endOffset : "[no-range]");
					SetSelection(selection.anchorNode, range.startOffset, range.endOffset);
				}*/

				if (event.which == 113) // f2
					if (VO.viewFrameTimer.timerID == -1) {
						Log("Starting view-frame timer");
						VO.viewFrameTimer.Start();
					}
					else {
						Log("Stopping view-frame timer");
						VO.viewFrameTimer.Stop();
					}
			}
		});
		// auto-click the first button of open dialog, when the enter key is pressed
		$(document).on("keyup", function(event, ui) {
			/*var tagName = event.target.tagName.toLowerCase();
			tagName = (tagName === "input" && event.target.type === "button") ? "button" : tagName;

			if (event.which === $.ui.keyCode.ENTER && tagName !== "textarea" && tagName !== "select" && tagName !== "button")
				$(this).find(".ui-dialog-buttonset button").eq(0).trigger("click");*/
		    //if (event.which === $.ui.keyCode.ENTER)
		    //    $(".ui-dialog-buttonset button").eq(0).trigger("click");
		});
		//if (!InUnity())
		//    $(document).on("keydown", function(event, data) { if (event.keyCode == 9) s.Tab_OnDown(event.shiftKey); });
		$(document).on("focus", "[tabIndex=-1]", function () { $(this).blur(); });

		// disable (middle-mouse) scrolling of body // old: probably todo: have this not also disable middle-mouse scrolling for descendents (e.g. scroll-views)
		/*$("body").mousedown(function(e)
		{
			if (e.button == 1)
				return false;
		});*/
		//window.onscroll = function() { document.body.scrollTop = 0; };
		//$("body").on("mousewheel", function() { return false; });

		// ui activation
		// ----------
		
		R("#exit").click(function(event, ui) { VO.SendMessage(ContextGroup.Local_CS, "Exit_OnClick"); });

		R("#Frame2").click(function() { s.ToggleTopMenuL2Open("Frame2"); });
		R("#Settings").click(function() { s.TogglePageOpen("settings"); });
		R("#Settings").contextmenu({
	    	show: false,
	    	hide: false,
	    	menu: [
			    {title: "Show Panel", action: function(event, ui) { s.settings.ShowAsPanel(); }},
			    {title: "Hide Panel", action: function(event, ui) { s.settings.HideAsPanel(); }}
		    ],
	    	beforeOpen: function(event, ui) {
	    		$(".contextMenu li:contains(Show Panel)").css("display", s.settings.IsOpenAsPanel() ? "none" : "");
		    	$(".contextMenu li:contains(Hide Panel)").css("display", s.settings.IsOpenAsPanel() ? "" : "none");
		    }
	    });
		R("#Console").click(function() { s.TogglePageOpen("console"); });
		R("#Console").contextmenu({
	    	show: false,
	    	hide: false,
		    menu: [
			    {title: "Show Panel", action: function(event, ui) { s.console.ShowAsPanel(); }},
			    {title: "Hide Panel", action: function(event, ui) { s.console.HideAsPanel(); }}
		    ],
		    beforeOpen: function(event, ui) {
		    	$(".contextMenu li:contains(Show Panel)").css("display", s.console.IsOpenAsPanel() ? "none" : "");
		    	$(".contextMenu li:contains(Hide Panel)").css("display", s.console.IsOpenAsPanel() ? "" : "none");
		    }
	    });

        R("#VOverlay").click(function() { s.ToggleTopMenuL2Open("VOverlay"); });
        R("#Maps").click(function() { s.TogglePageOpen("maps"); });
        R("#Biomes").click(function() { s.TogglePageOpen("biomes"); });
        R("#Objects").click(function() { s.TogglePageOpen("objects"); });
        R("#Modules").click(function() { s.TogglePageOpen("modules"); });
        R("#Matches").click(function() { s.TogglePageOpen("matches"); });
        R("#Live").click(function() { s.TogglePageOpen("live"); });

        R("#topMenuController").click(function() {
        	if ($("#topMenu").css("display") == "none")
        		s.ShowMenu();
        	else
        		s.HideMenu();
		});

		R("#topMenu").resize(function() { s.ResizePage(); });
		R("#page").VResizable({shareSpaceWith: R("#bottomPanel"), resizeDirection: "s"});
		R("#page").children(".ui-resizable-handle").css("margin-bottom", -5); // lazy hack; works fine for now
		s.PostPanelShowOrHide = function() {
			R("#page").children(".ui-resizable-handle").css("display", R("#bottomPanel").css("display"));
			VO.ResizePage();
		}.RunThenReturn();

        // attach pages
        // ----------

		/*s.mainMenu.Attach(R("#page"));
		s.settings.Attach(R("#page"));
		s.console.Attach(R("#page"));
		s.maps.Attach(R("#page"));
		//s.units.Attach(R("#page"));
		//s.items.Attach(R("#page"));
		s.objects.Attach(R("#page"));
		s.modules.Attach(R("#page"));
		s.matches.Attach(R("#page"));
		s.live.Attach(R("#page"));*/
		s.mainMenu_PostSet = function() { s.mainMenu.Attach("#page"); };
		s.console_PostSet = function() { s.console.Attach($("#page")); }; //"#page"); };
		s.settings_PostSet = function() { s.settings.Attach("#page"); };
		s.maps_PostSet = function() { s.maps.Attach("#page"); };
		s.biomes_PostSet = function() { s.biomes.Attach("#page"); };
		s.objects_PostSet = function() { s.objects.Attach("#page"); };
		//s.modules_PostSet = function() { s.modules.Attach("#page"); };
		s.matches_PostSet = function() { s.matches.Attach("#page"); };
		s.live_PostSet = function() { s.live.Attach("#page"); };

		// early startup
		// ----------

		VMessageBox.defaultCenterTo = $("#screenCenter");
		$.ui.fancytree.debugLevel = -1; // -2:none, -1:none-except-errors, 0:quiet, 1:info, 2:debug

		// startup
	    // ----------

		// browser url overrides
		var vars = GetUrlVars(CurrentUrl());
		if (vars.page)
			if (s[vars.page])
				s.OpenPage(vars.page);
			else
				s.AddExtraMethod(vars.page + "_PostSet", function() { s.OpenPage(vars.page); });
		else
			if (s.mainMenu)
				s.OpenPage("mainMenu");
			else
				s.extraMethod = function mainMenu_PostSet() { s.OpenPage("mainMenu"); };

	    s.ResizePage();
	};

	/*var html = function(){/*
	<div id="blocklyDivTest1" style="position: absolute; top: 0; height: 600px; width: 800px;"></div>
	<xml id="toolboxTest1" style="display: none">
		<category name="Logic">
			<category name="If">
				<block type="controls_if"></block>
				<block type="controls_if">
					<mutation else="1"></mutation>
				</block>
				<block type="controls_if">
					<mutation elseif="1" else="1"></mutation>
				</block>
			</category>
		</category>
	</xml>
*#/
	}.AsStr();
	$("body").append(html);
	var workspace = Blockly.inject('blocklyDivTest1', {
		media: 'Packages/Blockly/media/',
		toolbox: document.getElementById('toolboxTest1')
	});*/

	// from outside originally
	s.Attach($("body"));

	Random.PostUIInit();
	s.SendMessage(ContextGroup.Local_CSAndUI, "NotifyInitializedInJS"); //s.a("initializedInJS").set = true;
	//WaitXThenRun(0, function() { Log("js ready message sent"); s.SendMessage(ContextGroup.Local_CS, "NotifyInitializedInJS"); });
	
	var vars = GetUrlVars(CurrentUrl());
	if (vars.runJS) // run in-url script commands (for in-browser testing)
		CSBridge.RunJS(decodeURI(vars.runJS).replace(/`/g, "\"").replace(/\[h\]/g, "#").replace(/\[p\]/g, "%")); //replace(/"/g, "`").replace(/#/g, "[h]").replace(/%/g, "[p]")

	// old: maybe temp; needed for JQuery.ToList method
	//VO.initDone = true;

	/*if (!InUnity()) {
		//window.Map_custom = Map;
		window.Map = Map_orig;
		//WaitXThenRun(500, function() { window.Map = Map_custom; });
	}*/
};

var VO; // static root node (yes, exception to the non-class variable naming rule)

// startup
$(function() {
	VO = new VOverlay();
});