var React = require("react");
var ReactDOM = require("react-dom");

require("./TextArea.scss");

class TextArea extends BaseComponent {
    constructor(args) {
        super(args);
    }
	render() {
	    return <textarea ref="root" readOnly={!this.props.editable} className="TextArea simpleText" value={this.props.defaultValue} onChange={this.props.onChange}></textarea>;
	}
}
TextArea.defaultProps = {
    editable: true
};

/*var TextAreaChange = V.CreateClass(ChangeManager.Command, { //ChangeManager.Command.extend(
    constructor: function (control, oldText, oldSelection, newText, newSelection) {
        this.control = control;
        this.oldText = oldText;
        this.oldSelection = oldSelection;
        this.newText = newText;
        this.newSelection = newSelection;
    },
    execute: function() {},
    undo: function() {
        this.control[0].internallySettingText = true;
        this.control.text(this.oldText);
        this.control[0].internallySettingText = false;
        SetSelection(this.control[0].firstChild, this.oldSelection.startOffset, this.oldSelection.endOffset);
    },
    redo: function() {
        this.control[0].internallySettingText = true;
        this.control.text(this.newText);
        this.control[0].internallySettingText = false;
        SetSelection(this.control[0].firstChild, this.newSelection.startOffset, this.newSelection.endOffset);
    }
});*/

/*class TextArea extends BaseComponent {
    constructor(args) {
        super(args);
    }
	render() {
	    //return <div ref="root" contentEditable className="TextArea simpleText" onChange={this.props.onChange}>{this.props.defaultValue}</div>;
		return <div ref="root" contentEditable className="TextArea simpleText">{this.props.defaultValue}</div>;
	}

	componentDidMount() {
		WaitXThenRun(0, ()=>window.requestAnimationFrame(()=>this.PostRender()));
	}
	componentDidUpdate(prevProps, prevState) {
	    WaitXThenRun(0, ()=>window.requestAnimationFrame(()=>this.PostRender()));
	}
	PostRender() {
	    var rootDiv = ReactDOM.findDOMNode(this.refs.root);
	    var root = $(rootDiv);

		var stack = new ChangeManager.Stack();
		//stack.execute(new TextAreaChange(root, root.text(), GetSelection(), root.text(), GetSelection()));

		var currentChange = null;
		var currentChangeAdding = false;
		var endCurrentChange = ()=>{
			currentChange = null;
			currentChangeAdding = null;

		    var newText = root.text();
		    if (newText == this.lastSentText)
		        return;
		    this.lastSentText = newText;
		    this.props.onChange(newText);
		};
		var clearStack = ()=>{
			if(currentChange)
				endCurrentChange();
			stack.clear();
		};

	    root.blur(endCurrentChange);

		var preInputText;
		var preInputSelection;
		var preInputKey;
		root.keydown(function(event) { //keypress(function(event)
			if (event.ctrlKey && !event.shiftKey && event.which == 90) { // ctrl+z
				if (currentChange)
					endCurrentChange();

				if (stack.canUndo())
					stack.undo();
				event.preventDefault();
				return false;
			}
			else if ((event.ctrlKey && event.shiftKey && event.which == 90) || (event.ctrlKey && event.which == 89)) { // ctrl+shift+z or ctrl+y
				if (currentChange)
					endCurrentChange();

				if (stack.canRedo())
					stack.redo();
				event.preventDefault();
				return false;
			}

			preInputText = root.text();
			preInputSelection = GetSelection();
			preInputKey = event.which;
		});
		root.on("input", function(event) { // forces fake text-areas to always only have text content after input/typing
			//event.preventDefault();
			var oldText = preInputText; //root.text();
			var oldSelection = preInputSelection; //GetSelection();
			WaitXThenRun(0, function() {
				root[0].internallySettingText = true;
				root.text(root.text()); // combine all child text-nodes into one
				/*Log((preInputKey == 13) + ";" + root.text().endsWith("\n\n") + ";" + (root.text().length == oldText.length + 2));
				if (preInputKey == 13 && root.text().endsWith("\n\n") && root.text().length == oldText.length + 2)
					root.text(root.text().substr(0, root.text().length - 1));*#/ // todo; break point
				root[0].internallySettingText = false;
				//SetSelection(root[0].firstChild, oldSelection.startOffset, oldSelection.endOffset + (root.text().length - oldText.length));
				if (preInputKey == 46 && oldSelection.startOffset == oldSelection.endOffset) // if delete key was pressed, and nothing was selected
					SetSelection(root[0].firstChild, oldSelection.endOffset + (root.text().length - oldText.length) + 1, oldSelection.endOffset + (root.text().length - oldText.length) + 1);
				else
					SetSelection(root[0].firstChild, oldSelection.endOffset + (root.text().length - oldText.length), oldSelection.endOffset + (root.text().length - oldText.length));

				var newText = root.text();
				var newSelection = GetSelection();

				var overwroteBlockOfText = oldSelection.endOffset > oldSelection.startOffset;
				var adding = newText.length > oldText.length || overwroteBlockOfText; // if text is longer, or we overwrote a block of text
				var startOfNewChange = !currentChange || currentChangeAdding != adding || overwroteBlockOfText || oldSelection.startOffset != currentChange.newSelection.startOffset || oldSelection.endOffset != currentChange.newSelection.endOffset;
				if (startOfNewChange) {
					if (currentChange)
						endCurrentChange();
					currentChange = new TextAreaChange(root, oldText, oldSelection, newText, newSelection);
					stack.execute(currentChange);
				}
				else {
					currentChange.newText = newText;
					currentChange.newSelection = newSelection;
				}
				currentChangeAdding = adding;

				//preInputText = null;
				//preInputSelection = null;
				preInputKey = null;
			});
		});
		root.on("paste", function(event) {
			if (currentChange)
				endCurrentChange();
		});
		root.on("textSet", function(event) {
			if (root[0].internallySettingText)
				return;

			clearStack();
		});
	}
}*/

export default TextArea;