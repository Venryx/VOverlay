/*var React = require("react");
var ReactDOM = require("react-dom");*/

/*var images = {
    menu: "url(/Packages/Images/Tiling/Menu/Menu.png)"
};*/

class BottomBar extends BaseComponent {
	constructor(args) {
		super(args);
	}
	
	render() {
	    var boldStyle = {fontSize: this.props.styles.glowingTextDivStyle.fontSize, fontWeight: "bold"};
	    var bold = str=><span style={boldStyle}>{str}</span>;
		var firstDot = <span style={{fontSize: 18, display: "inline-block", verticalAlign: "top", margin: "5px 5px 0 0", color: "rgba(0,200,0,1)"}}>●</span>;
	    var dot = <span style={{fontSize: 18, display: "inline-block", verticalAlign: "top", margin: "5px 5px 0 15px", color: "rgba(0,200,0,1)"}}>●</span>;

	    return (
			<div style={{position: "absolute", bottom: 0, width: "100%", height: 64, paddingLeft: 10,
				//backgroundImage: images.menu,
				backgroundColor: "rgba(0,0,20,1)",
			    border: "solid rgba(30,30,70,1)", borderWidth: "1px 0 0 0", //borderRadius: "20px 20px 0 0",
				boxShadow: "rgba(30,30,70,1) 0px 0px 10px, rgba(0,0,10,.5) 0px 0px 30px 10px"}}>
				<div style={this.props.styles.glowingTextDivStyle.Extended({whiteSpace: "pre"})}>
				{firstDot}{bold("Schedule:")} 12am-5am pst (7-12 utc){dot}{bold("Discord server:")} discord.gg/xxJDCV9
				</div>
				<div style={this.props.styles.glowingTextDivStyle.Extended({marginTop: -2, whiteSpace: "pre"})}>
				{firstDot}YouTube/BiomeDefense{dot}Facebook/BiomeDefense{dot}Twitter/BiomeDefense{dot}Google/+BiomeDefense{dot}IndieDB/Games/Biome-Defense
				</div>
				{/*<div style={glowingTextDivStyle.Extended({})}>
				BiomeDefense.com
				</div>*/}
			</div>
		);
	}
}

export default BottomBar;