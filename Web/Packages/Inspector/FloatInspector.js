var FloatInspector = new function()
{
	this.BuildUI = function (root, oldValue, sendCallback)
	{
		var textbox = $("<input type='text' style='width: 100%;'>").appendTo(root).val(oldValue);
		textbox.data("lastSetValue", oldValue);
		textbox.change(function(event, data)
		{
			textbox.data("lastSetValue", textbox.val());
			sendCallback(VDF.Serialize(parseFloat(textbox.val())));
		});
	};
};