/*Array.SetAsBaseClassFor = function Multi(itemType)
{
	var s = this.CallBaseConstructor();
	//s.innerType = innerType;
	Object.defineProperty(s, "itemType", {enumerable: false, value: itemType});
};*/

Node.SetAsBaseClassFor = function Script() {
	var s = this.CallBaseConstructor();

	// heirarchy
	// ==========

	s._AddGetter_Inline = function ParentScript() { return s.Parent instanceof Script ? s.Parent : null; };
	s._AddGetter_Inline = function Module() { return s.Parent instanceof Script ? s.Parent.Module : s.Parent; };
	//s._AddGetter_Inline = function ScriptContext() { return s.Module.scriptContext; };

	s.p("children", "List(Script)", new ByPath()).set = [];

	// general
	// ==========

	s.name = null;

	s.blocksXML = "<xml xmlns='http://www.w3.org/1999/xhtml'></xml>";

	s.Delete = function() {
		var module = s.Module;
		/*var scriptContext = s.ScriptContext;
		var pageScript = s.PageScript;*/

		for (var i in module.scripts)
			if (module.scripts[i].children.Contains(s))
				module.scripts[i].a("children").remove = s;

		module.a("scripts").remove = s;
		module.a("selectedScript").set = null;
	};
};