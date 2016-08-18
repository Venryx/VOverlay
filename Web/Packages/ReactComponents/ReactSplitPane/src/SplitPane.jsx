import React, { Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import Pane from "./Pane";
import Splitter from "./Splitter";

require("Packages/ReactComponents/ReactSplitPane/css/Custom.scss");

// flags
const maxPanelCount = 3;

class SplitPane extends BaseComponent {
    constructor(...args) {
        super(...args);
		this._bind("onMouseDown", "onMouseMove", "onMouseUp");
		this.state = {
            dragSplitterIndex: -1
        };
    }

    componentDidMount() {
		var state = {};
		const defaultPanelSize = 1 / this.props.children.length;
		for (var i of Range(0, this.props.children.length - 1)) {
			var splitterStr = "splitter" + i;
			state[splitterStr + "_pos"] = (this.props[splitterStr + "_pos"] || ((i + 1) * defaultPanelSize))
				.KeepBetween(this.props[splitterStr + "_minPos"], this.props[splitterStr + "_maxPos"]);
		}
		//Log("SettingState1)" + ToJSON(state));
		this.setState(state);
		
        document.addEventListener("mousemove", this.onMouseMove);
        document.addEventListener("mouseup", this.onMouseUp);
    }

	/*componentWillUpdate(nextProps, nextState) {
		Log("WillUpdate;New:" + ToJSON(nextProps, "children") + ";" + ToJSON(nextState, "children"));
	}
	componentDidUpdate(prevProps, prevState) {
		Log("DidUpdate;Old:" + ToJSON(prevProps, "children") + ";" + ToJSON(prevState, "children"));
	}
    componentWillReceiveProps(props) {
		Log("WillReceive)" + ToJSON(props));
        this.setSplitterPos(props, this.state);
    }*/

    componentWillUnmount() {
        document.removeEventListener("mousemove", this.onMouseMove);
        document.removeEventListener("mouseup", this.onMouseUp);
    }

	getRootSizeAbs() { return ReactDOM.findDOMNode(this.refs.root).getBoundingClientRect()[this.props.direction == "horizontal" ? "width" : "height"]; }
	getSplitter(index) { return this.refs["splitter" + index]; }
	getSplitterPos(index) {
	    if (index == -1) 
	        return 0;
		return this.state["splitter" + index + "_pos"];
	}
	getSplitterPosAbs(index) { return this.state["splitter" + index + "_pos"] * this.getRootSizeAbs(); }
	getPanelSize(index) {
		var leftSplitterPos = this.state["splitter" + (index - 1) + "_pos"] || 0;
		var rightSplitterPos = this.state["splitter" + index + "_pos"] || 1;
		return rightSplitterPos - leftSplitterPos;
	}

    onMouseDown(index, event) {
        if (!this.props.allowResize) return;

		this.unFocus();
		const posAbs = event[this.props.direction == "horizontal" ? "clientX" : "clientY"];
        this.mouseToPosDif = this.getSplitterPosAbs(index) - posAbs;
		if (this.props.onDragStart) this.props.onDragStart(index);
		this.setState({
			dragSplitterIndex: index
		});
    }

    onMouseMove(event) {
		if (this.state.dragSplitterIndex == -1) return;

		this.unFocus();
		var index = this.state.dragSplitterIndex;

		//const splitter = this.getSplitter(index);
		//const splitterDiv = ReactDOM.findDOMNode(splitter);
        const posAbs = event[this.props.direction == "horizontal" ? "clientX" : "clientY"] + this.mouseToPosDif;
		
		var splitterStr = "splitter" + index;
		const pos = (posAbs / this.getRootSizeAbs()).KeepBetween(this.props[splitterStr + "_minPos"], this.props[splitterStr + "_maxPos"]);
		this.setState({
			[splitterStr + "_pos"]: pos
		});
		if (this.props.onDragMove) this.props.onDragMove(index, pos);
    }

    onMouseUp() {
		if (this.state.dragSplitterIndex == -1) return;
		
		if (this.props.onDragEnd) this.props.onDragEnd();
		this.setState({
			dragSplitterIndex: -1
		});
    }

    unFocus() {
        if (document.selection)
            document.selection.empty();
        else
            window.getSelection().removeAllRanges();
    }

    render() {
        const {direction, allowResize} = this.props;

        const style = {}.Extended(this.props.style).Extended({
            display: "flex",
            flex: 1,
            position: "relative",
            outline: "none",
            overflow: "hidden",
            MozUserSelect: "text",
            WebkitUserSelect: "text",
            msUserSelect: "text",
            userSelect: "text",
        });

        if (direction == "horizontal")
            style.Extended({
                flexDirection: "row",
                height: "100%",
                position: "absolute",
                left: 0,
                right: 0
            });
        else
            style.Extended({
                flexDirection: "column",
                height: "100%",
                minHeight: "100%",
                position: "absolute",
                top: 0,
                bottom: 0,
                width: "100%"
            });

		const disablednessClass = allowResize ? "" : "disabled";
        const classes = ["SplitPane", this.props.className, direction, disablednessClass];
		var newChildren = [];
		this.props.children.forEach((child, i)=>{
			var paneStyle = {}.Extended(this.props.paneStyle).Extended(child.style);
			/*newChildren.push(<Pane ref={"pane" + i} id={child.id} key={"pane" + i} className={child.className}
				pos={this.getSplitterPos(i - 1)} size={this.getPanelSize(i)} direction={direction} style={paneStyle}>{child}</Pane>);*/
			var newChild = React.cloneElement(child, {key: "pane" + i, pos: this.getSplitterPos(i - 1), size: this.getPanelSize(i),
				direction: direction, style: paneStyle});
		    newChildren.push(newChild);

			if (i < this.props.children.length - 1) { // if not last, add splitter
				var splitterStr = "splitter" + i;
				newChildren.push(
					<Splitter ref={splitterStr} key={splitterStr} className={disablednessClass}
						onMouseDown={e=>this.onMouseDown(i, e)} direction={direction} style={this.props.resizerStyle}
						pos={this.getSplitterPos(i)}/>
				);
			}
		});

        return (
            <div className={classes.join(" ")} style={style} ref="root">
                {newChildren}
            </div>
        );
    }
}
/*SplitPane.propTypes = {
    primary: PropTypes.oneOf(["first", "second"]),
    minSize: PropTypes.oneOfType([
        React.PropTypes.string,
        React.PropTypes.number,
    ]),
    maxSize: PropTypes.oneOfType([
        React.PropTypes.string,
        React.PropTypes.number,
    ]),
    defaultSize: PropTypes.oneOfType([
        React.PropTypes.string,
        React.PropTypes.number,
    ]),
    size: PropTypes.oneOfType([
        React.PropTypes.string,
        React.PropTypes.number,
    ]),
    allowResize: PropTypes.bool,
    direction: PropTypes.oneOf(["vertical", "horizontal"]),
    onDragStarted: PropTypes.func,
    onDragFinished: PropTypes.func,
    onChange: PropTypes.func,
    className: PropTypes.string,
    children: PropTypes.arrayOf(PropTypes.node).isRequired,
};*/
SplitPane.defaultProps = {
    direction: "horizontal",
    allowResize: true
};
for (var i of Range(0, maxPanelCount)) {
	SplitPane.defaultProps["splitter" + i + "_minPos"] = 0;
	SplitPane.defaultProps["splitter" + i + "_maxPos"] = 1;
}

export default SplitPane;