var nextCheckBoxID = 0;
VUI.RegisterControlType("checkBox", function() {
	var root = $(this);
	var labelText = root.attr("_labelText");

	var checkBox = $("<input type='checkbox' style='margin-left: 5;'/>");
	var id = root.attr("id");
	if (id == null)
		id = "checkBox_" + nextCheckBoxID;
	checkBox.attr("id", id);
	root.replaceWith(checkBox);
	if (labelText != null)
		var label = $("<label style='margin-top: 3;'><span/><div>" + labelText + "</div></label>").insertAfter(checkBox).attr("for", id);
	else
		var label = $("<label style='margin-top: 3;'><span/></label>").insertAfter(checkBox).attr("for", "checkBox_" + nextCheckBoxID);
	nextCheckBoxID++;

	root.data("checkBox", checkBox);
});

VUI.RegisterControlType("separator", function() {
	var root = $(this);
	var labelText = root.attr("_labelText");

	if (labelText)
		var label = $("<div style='display: inline-block; margin-bottom: -3; padding: 0 7px; border-radius: 10px 10px 0 0; background: rgba(255, 255, 255, .1);'>").appendTo(root).html(labelText).css("margin-top", root.parent().children().length > 1 ? 5 : 0);
	var separator = $("<div style='margin-bottom: 3; border: solid rgba(255, 255, 255, .2); border-width: 1px 0 0 0;'>").css("margin-top", labelText == null && root.parent().children().length > 1 ? 8 : 3).appendTo(root);
});
VUI.RegisterControlType("vSeparator", function() {
	var root = $(this);
	//var separator = $("<div style='margin: 0 3; border: solid rgba(255, 255, 255, .2); border-width: 0 1px 0 0;'>").appendTo(root);
	root.attr("style", "display: inline-block; margin: 0 3; border: solid rgba(255, 255, 255, .2); border-width: 0 1px 0 0;");
	if (root.css("height") == "0px")
		root.css("height", "100%");
});