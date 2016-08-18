var VTextureInspector = new function()
{
	this.BuildUI = function(propUIRoot, oldValue, sendCallback)
	{
		var uiStartToRoot = "../../../"; // root folder is actually represented by "../../../", for us, since we start in the "<root>/UI/Main/Views" folder
		var rootToResources = "Packs/" + Designer.livePack.name + "/Resources/";
		
		var rightBox = $("<div class='borderBox' style='padding: 5; background: url(Packages/Images/Tiling/Menu/Menu.png) transparent; border-radius: 7px 7px 0 0; float: right;'>").appendTo(propUIRoot);
		var textureBox = $("<div id='textureBox' style='position: relative; display: inline-block; text-align: center;'>").appendTo(rightBox);
		var textureNameLabel = $("<div id='textureName' style='margin-top: 5; margin-bottom: 3;'>").appendTo(textureBox).html(oldValue ? oldValue.substring(oldValue.lastIndexOf("/") + 1) : "(none)"); //.attr("title", oldValue.TexturePath);
		var texturePreview = $("<img id='texturePreview' width='100' height='100' style='border: 3px inset #333; border-radius: 3px;'/>").appendTo(textureBox).attr("src", oldValue ? uiStartToRoot + rootToResources + oldValue : "");
		var textureSelect = $("<div id='textureSelect' class='button' style='position: absolute; bottom: 3; right: 3; padding: 0 5; font-size: 12;'>Select</div>").appendTo(textureBox);
		textureSelect.click(function(event, ui)
		{
			var dialogRootPath = uiStartToRoot + rootToResources;
			CSBridge.CallCS("FolderNode.GetFolderNodeByPath", rootToResources, function(rootFolderNode)
			{
				if (!InUnity())
					rootFolderNode = FromVDF("{name:'Main' folders:[{name:'Resources' folders:[{name:'Other Stuff'}] files[{name:'Grass.png' name:'Dirt.png'}]}] files[{name:'File1'}]}");

				var liveFolderPathParts = oldValue ? oldValue.replace(/\/[^/]+?\/?$/, "").split("/") : [];
				var liveFolder = rootFolderNode;
				for (var i = 0; i < liveFolderPathParts.length; i++)
					liveFolder = liveFolder.folders.filter(function (item) { return item.name == liveFolderPathParts[i]; })[0];
				var textureName = oldValue ? oldValue.substring(oldValue.lastIndexOf("/") + 1) : null;
				var liveNode = oldValue && liveFolder && liveFolder.files.filter(function(item) { return item.name == textureName; })[0];

				var fileBrowser = new VFileBrowser(
				{
					title: "Select Texture",
					rootPath: dialogRootPath,
					rootFolderNode: rootFolderNode,
					liveFolder: liveFolder,
					liveNode: liveNode,
					acceptFolders: false,
					preClose: function(success)
					{
						var frame = fileBrowser.GetRoot().parent();
						SetPref("popupPosition_VFileBrowser", {left: +frame.offset().left + (+frame.width() / 2), top: +frame.offset().top + (+frame.height() / 2)});
						SetPref("popupSize_VFileBrowser", {width: frame.width(), height: frame.height()});

						if (success)
						{
							var path = fileBrowser.GetSelectedPath();
							if (!path)
								return false;

							//$("#textureBox").attr("path", path);
							sendCallback("VTexture>" + path); // rather than write a wrapper class, we'll just fake it
						}
					}
				});
				GetPref("popupSize_VFileBrowser", function(pValue)
				{
					if(!pValue)
						return;
					fileBrowser.GetRoot().dialog(
					{
						width: pValue.width,
						height: pValue.height
					});
				});
				GetPref("popupPosition_VFileBrowser", function(pValue)
				{
					if(!pValue)
						return;
					var popupFrame = fileBrowser.GetRoot().parent();
					popupFrame.offset({left: pValue.left - (popupFrame.width() / 2), top: pValue.top - (popupFrame.height() / 2)});
				});
				fileBrowser.Open();
			});
		});
	};
};