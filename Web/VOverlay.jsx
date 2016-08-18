var React = require("react");
var ReactDOM = require("react-dom");

require("Packages/CSS/Globals.scss");

var TopBar = require("VOverlay/TopBar");
var BottomBar = require("VOverlay/BottomBar");

var urlVars = GetURLVars();

class VOverlay extends BaseComponent {
	constructor(props) {
		super(props);
	}

	getRoot() {
	    return $(ReactDOM.findDOMNode(this));
	}
	
	render() {
		// var glowColor = "#ff00de";
	    var glowWhiteColor = "rgba(255,255,255,1)";
		var glowColor = "rgba(0,0,120,1)";
	    var glowingTextDivStyle = {
	        fontSize: 23, color: "rgba(220,220,255,1)",
	        textShadow: `0 0 10px ${glowWhiteColor}, 0 0 10px ${glowColor}, 0 0 20px ${glowColor}, 0 0 30px ${glowColor}, 0 0 40px ${glowColor}`
	    };

		var styles = {glowingTextDivStyle};

		return (
			<div style={{height: "100%",
				//backgroundImage: "url(/Packages/Images/Backgrounds/Hawaii_1920x1080.jpg)"
				backgroundImage: urlVars.background ? "url(/Packages/Images/Tiling/Menu/Menu_B130.png)" : ""
			}}>
				<TopBar styles={styles}/>
				<BottomBar styles={styles}/>
			</div>
		);
	}

	/*componentDidMount() {
		WaitXThenRun(0, ()=>window.requestAnimationFrame(()=>this.PostRender(true)));
	}
	componentDidUpdate() {
		WaitXThenRun(0, ()=>window.requestAnimationFrame(()=>this.PostRender(false)));
	}
	PostRender(firstRender) {
	    if (firstRender) {
	        this.resize();
	        this.getRoot().OnVisible(()=>this.resize(), true);
	    } 
	}*/
}

//export default VOverlay;

$(()=> {
	ReactDOM.render(<VOverlay/>, $("#VOverlayRoot")[0]);
});