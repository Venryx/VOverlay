var images = {
    //menu: "url(/Packages/Images/Tiling/Menu/Menu.png)"
};

class TopBar extends BaseComponent {
	constructor(args) {
		super(args);
	}
	
	render() {
	    //var glowingTextDivStyle = this.props.styles.glowingTextDivStyle.Extended();
		var glowWhiteColor = "rgba(255,255,255,1)";
		var glowColor = "rgba(0,0,120,1)";
		var glowingTextDivStyle = {
		    fontSize: 25, color: "rgba(220,220,255,1)",
	        textShadow: `0 0 10px ${glowWhiteColor}, 0 0 10px ${glowColor}, 0 0 20px ${glowColor}, 0 0 30px ${glowColor}, 0 0 40px ${glowColor}`
		}

		return (
			<div style={{position: "absolute", top: 0, width: "100%", height: 64}}>
				{/*<div style={{display: "block", width: 290, height: 30, margin: "20px auto 0px", borderRadius: "30px",
					backgroundColor: "rgba(0,0,10,.3)", boxShadow: "rgba(0,0,10,.3) 0px 0px 10px 5px"}}>
					<div style={this.props.styles.glowingTextDivStyle.Extended({fontSize: 32, textAlign: "center", transform: "translateY(-10px)"})}>
					BiomeDefense.com
					</div>
				</div>*/}
				<div style={glowingTextDivStyle.Extended({fontSize: 32, textAlign: "center"})}>
				BiomeDefense.com
				</div>
			</div>
		);
	}
}

export default TopBar;