var VRefInspector = new function()
{
	this.BuildUI = function(root, oldValue, sendCallback)
	{
	    var label = $("<div style='background: rgba(100,100,100,.3); padding: 3 0;'>").appendTo(root).html(oldValue);

		label.droppable(
		{
			//accept: ".fancytree-drag-helper",
			activeClass: "vRefDrop_active",
			hoverClass: "vRefDrop_hover",
			drop: function(event, ui)
			{
			    if (ui.draggable.is("#typesTreeView") || ui.draggable.is("#instancesTreeView"))
			    {
			        alert(ui.draggable.data("lastDragStart_vObject").id);
			        sendCallback(ui.draggable.data("lastDragStart_vObject").id);
			    }
			}
		});

	    //$("<div>").appendTo(root).html("Hi there").draggable({});
	    //sendCallback(VDF.Serialize(label.val()));
	};
};