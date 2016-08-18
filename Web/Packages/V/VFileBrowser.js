var VFileBrowser = function(options)
{
	var s = this;

	// setup
	// ==================

	var root = $("<div class='VFileBrowser' style='display: none;'>").appendTo($("body"));
	if (options.title)
		root.attr("title", options.title);

	var style = $("<style>.selectedNodeBox { background: rgba(255,255,255,.3) !important; } .selectedNodeBox .selectHighlight { opacity: .75 !important; }</style>").appendTo(root);

	var div = $("<div style='position: absolute; left: 0; right: 0; top: 0; bottom: 0;'>").appendTo(root);

	var topBar = $("<div class='clear'>").appendTo(div);
	var topBarScrollView = $("<div scroll-view class='borderBox' style='float: left; width: 100%;'>").appendTo(topBar).css("height", "");
    topBarScrollView.on("scrollbarShow", function() { bottomBar.css("top", 43); });
    topBarScrollView.on("scrollbarHide", function() { bottomBar.css("top", 31); });
    var topBarDiv = topBarScrollView.children(".scrollView_rootChild");
	var pathNodes = $("<div style='white-space: nowrap; padding-bottom: 2;'>").appendTo(topBarDiv);
	//var topBarDiv2 = $("<div style='position: absolute; top: 0; right: 0;'>").appendTo(topBar);
	//var search = $("<input type='text' style='margin-top: 2; margin-right: 2; width: 150; opacity: .75;'/>").appendTo(topBarDiv2);

	var bottomBar = $("<div style='position: absolute; top: 31; bottom: 0; left: 0; right: 0;'>").appendTo(div);
	var nodesScrollView = $("<div scroll-view style='height: 100%; background: rgba(0,0,0,.3);'>").appendTo(bottomBar);
    var nodes = nodesScrollView.children(".scrollView_rootChild");

	var open = false;
	root.dialog(
	{
		autoOpen: false,
		resizable: true,
		minWidth: 400,
		width: 600,
		minHeight: 300,
		height: 400,
		modal: true,
		buttons:
		{
			OK: function() { s.Close(true); },
			Cancel: function() { s.Close(false); }
		},
		beforeClose: function(event, ui)
		{
			if (open) // if called by close button
				s.Close(false);
		}
	});

	// ui setup
	// ==================

	nodes.mousedown(function(event, ui)
	{
		if(event.target != this)
			return;

		$(".selectedNodeBox").removeClass("selectedNodeBox");
	});
	nodes.on("mousedown", "> div", function(event, ui)
	{
		SelectNodeBox($(this));
	});
	nodes.on_doubleClick("> div", function(event, ui)
	{
		var node = this.node;
		if (this.isFolder)
			LoadFolder(node.path);
	});

	// variables: live
	// ==================

	var liveFolder;

	// methods: live
	// ==================

	function SelectNodeBox(box)
	{
		$(".selectedNodeBox").removeClass("selectedNodeBox");
		box.addClass("selectedNodeBox");
	}

	function CreatePathNodeBox(path, name)
	{
	    var div = $("<div class='button'>");
	    //div[0].path = path;

		/*var isRoot = !name;
		if (isRoot)
		    div.addClass(" diameter24").addClass("icon10").css({backgroundImage: "url(Packages/Images/Buttons/Home.png)"});
		else
			div.css("margin-left", 3);*/
	    if (path.contains("/"))
	        div.css("margin-left", 3);

		div.html(name);
		div.click(function(event, ui) { LoadFolder(path); });

		return div;
	}
	function CreateFolderBox(folder)
	{
		var box = $("<div style='position: relative; display: inline-block; width: 70; height: 70; margin: 5; background: rgba(255, 255, 255, .05); border-radius: 10px; border: 1px solid transparent;'>");
		box[0].node = folder;
		box[0].isFolder = true;

		var div = $("<div style='width: 100%; height: 100%;'/>").appendTo(box);

		box.attr("style", box.attr("style").replace("background: rgba(255, 255, 255, .05);", "background: none !important;"))
		var img = $("<img class='selectHighlight' style='position: absolute; left: -1; top: -1; width: 70; height: 80; border-radius: 10px; border: 1px solid transparent; opacity: .5;' src='Packages/Images/Icons/Folder2.png'/>").appendTo(div);

		var div2 = $("<div style='display: table-cell; text-align: center; vertical-align: bottom; padding: 2; width: 70; height: 70; cursor: default; position: relative; z-index: 1;'/>").appendTo(div);
		var div3 = $("<div style='width: 66; color: #000; white-space: pre-line; word-wrap: break-word; font-size: 12;'>").appendTo(div2);
		if (folder.name != "")
			div3.html(folder.name);

		return box;
	}
	function CreateFileBox(file)
	{
		var box = $("<div style='position: relative; display: inline-block; width: 70; height: 70; margin: 5; background: rgba(255, 255, 255, .05); border-radius: 10px; border: 1px solid transparent;'>");
		box[0].node = file;

		var div = $("<div style='width: 100%;'/>").appendTo(box);

		var img = $("<img style='position: absolute; left: -1; top: -1; width: 70; height: 70; border-radius: 10px; border: 1px solid transparent; opacity: .75; display: none;'/>").appendTo(div);
		/*var fileExtension = file.path.contains(".") && file.path.match(/\.([^.]+)$/)[1];
		if (["jpg"/*, "bmp"*#/, "png"].contains(fileExtension))
			img.css("display", "").attr("src", file.path.replace(/\//g, "\\")); //$.get(filePath).done(function() { img.css("display", "").attr("src", filePath);*/
		if (file.previewImageStr)
		    img.css("display", "").attr("src", "data:image/png;base64," + file.previewImageStr);
			
		var div2 = $("<div style='display: table-cell; text-align: center; vertical-align: bottom; padding: 2; width: 70; height: 70; cursor: default; position: relative; z-index: 1;'/>").appendTo(div);
		var div3 = $("<div style='width: 66; color: #000; white-space: pre-line; word-wrap: break-word; font-size: 12;'>").appendTo(div2);
		if (file.name != "")
			div3.html(file.name);

		return box;
	}

	function LoadFolder(folder)
	{
		CSBridge.CallCS("FolderNode.GetFolderNodeByPath", folder, function(folderNode)
		{
			if (!InUnity())
			{
				var rootFolderNode = FromVDF(V.Multiline(function() {/*
{path:'Root' name:'Root' folders:[^] files:[{path:'Root/File1' name:'File1'} {path:'Root/File2' name:'File2'}]}
	{path:'Root/2A' name:'2A' folders:[] files:[{path:'Root/2A/File1' name:'File1'} {path:'Root/2A/File2' name:'File2'}]}
	{path:'Root/2B' name:'2B' folders:[] files:[]}
*/}));
				if (folder == "" || folder.endsWith("Root"))
					folderNode = rootFolderNode;
				else if (folder.endsWith("2A"))
					folderNode = rootFolderNode.folders[0];
				else if (folder.endsWith("2B"))
					folderNode = rootFolderNode.folders[1];
			}

			if (folderNode) // if folder exists
	            LoadFolderNode(folderNode);
	        else // otherwise, load default folder
	            CSBridge.CallCS("FolderNode.GetFolderNodeByPath", "", function(folderNode2) { LoadFolderNode(folderNode2); });
	    });
	}
	function LoadFolderNode(folderNode)
	{
	    pathNodes.html("");
		//CreatePathNodeBox(options.rootFolder, "").appendTo(pathNodes);
	    var pathFolderNames = folderNode.path.split("/").Where(function() { return this.length; });
	    var nextPath = ""; //folderNode.name;
		for (var i in pathFolderNames)
		{
		    var name = pathFolderNames[i];
			nextPath += (nextPath.length > 0 ? "/" : "") + name;
			var folderNodeBox = CreatePathNodeBox(nextPath, name);
			pathNodes.append(folderNodeBox);
		}

		nodes.html("");
		for (var i in folderNode.folders)
		{
			var box = CreateFolderBox(folderNode.folders[i]);
			nodes.append(box);
		}
		for (var i in folderNode.files)
		{
		    var box = CreateFileBox(folderNode.files[i]);
			nodes.append(box);
		}  
	};

	function GetSelectedNode() { return GetSelectedNodeBox() ? GetSelectedNodeBox()[0].node : null; }
	function GetSelectedNodeBox() { return $(".selectedNodeBox").length ? $(".selectedNodeBox") : null; }

	// startup
	// ==================

	// if liveFolder wasn't specified, default liveFolder to root
	options.liveFolder = options.liveFolder || options.rootFolder;
	LoadFolder(options.liveFolder);
	nodes.children().each(function()
	{
		if (this.node == options.liveNode)
			SelectNodeBox($(this));
	});

	s.GetRoot = function()
	{
		return root;
	};
	s.Open = function()
	{
		open = true;
		root.css("display", "");
		root.dialog("open");
	};
	s.Close = function(success)
	{
		if (options.preClose && options.preClose(success) === false)
			return;
		open = false;
		root.dialog("close");
		root.remove();
	};
	s.GetSelectedPath = function()
	{
		var result = null;

		var selectedNodeBox = GetSelectedNodeBox();
		if (selectedNodeBox && (!selectedNodeBox[0].node.folders || options.acceptFolders !== false))
			result = GetSelectedNode().path;

		return result;
	};
};