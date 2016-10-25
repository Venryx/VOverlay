class RightBar extends BaseComponent {
	constructor(args) {
		super(args);
	}
	
	render() {
		//var screenRect = {x: 0, y: 0, width: 1920, height: 1080};
	    var screenWidth = V.GetScreenWidth();
	    var screenHeight = V.GetScreenHeight();

		var webcamRect = {right: screenWidth - 7.5, bottom: screenHeight - 65, width: 400, height: 237.5};
	    webcamRect.x = webcamRect.right - webcamRect.width;
	    webcamRect.y = webcamRect.bottom - webcamRect.height;
		
		var webcamBorderDivStyle = {
			position: "absolute", backgroundColor: "rgba(0,0,20,1)",
			border: "solid rgba(30,30,70,1)", //borderRadius: "20px 20px 0 0",
			boxShadow: "rgba(30,30,70,1) 0px 0px 10px, rgba(0,0,10,.5) 0px 0px 30px 10px"};
	    var borderWidth = 15;
	    var r = 10; // border radius width

		var rightBarRect = {left: webcamRect.x - borderWidth/2, right: webcamRect.right + borderWidth/2 + 1,
			top: webcamRect.y - borderWidth/2, bottom: webcamRect.bottom + 1};
		rightBarRect.width = rightBarRect.right - rightBarRect.left;
	    rightBarRect.height = rightBarRect.bottom - rightBarRect.top;

		var glowWhiteColor = "rgba(255,255,255,1)";
		var glowColor = "rgba(0,0,120,1)";
		var glowingTextDivStyle = {
		    fontSize: 25, color: "rgba(220,220,255,1)",
	        textShadow: `0 0 10px ${glowWhiteColor}, 0 0 10px ${glowColor}, 0 0 20px ${glowColor}, 0 0 30px ${glowColor}, 0 0 40px ${glowColor}`
		}

	    return (
			//<div style={{position: "absolute", right: 0, top: 0, bottom: screenRect.height - webcamRect.bottom}}>
			<div style={{height: "100%"}}>
				<div style={{
						position: "absolute", top: 0, right: 0, width: 415, height: webcamRect.y,
						backgroundColor: "rgba(0,0,20,.7)",
						border: "solid rgba(30,30,70,1)", borderWidth: "1px 0 0 0", //borderRadius: "0 0 0 20px",
						boxShadow: "rgba(30,30,70,1) 0px 0px 10px, rgba(0,0,10,.5) 0px 0px 30px 10px"}}>
					<div style={glowingTextDivStyle.Extended({fontSize: 32, textAlign: "center"})}>
					BiomeDefense.com
					</div>
				</div>
				<div style={{position: "absolute", left: rightBarRect.left, right: screenWidth - rightBarRect.right,
					top: rightBarRect.top, bottom: screenHeight - rightBarRect.bottom,
					clip: `rect(-1000px 1000px ${rightBarRect.height} -1000px)`}}>
					<div ref="leftBar" style={webcamBorderDivStyle.Extended({left: 0, width: borderWidth, top: borderWidth, bottom: 0,
						boxSizing: "border-box", borderWidth: "0 1px 0 1px", zIndex: 1, clip: `rect(0 1000px 1000px -1000px)`})}/>
					<div ref="rightBar" style={webcamBorderDivStyle.Extended({right: 0, width: borderWidth, top: borderWidth, bottom: 0,
						boxSizing: "border-box", borderWidth: "0 1px 0 1px", zIndex: 1, clip: `rect(0 1000px 1000px -1000px)`})}/>

					{/*<div ref="topBar" style={webcamBorderDivStyle.Extended({left: borderWidth, right: borderWidth, top: 0, height: borderWidth,
						boxSizing: "border-box", borderWidth: "1px 0 1px 0", zIndex: 1,
						clip: `rect(-1000px ${rightBarRect.width - borderWidth*2} 1000px 0)`})}/>
					<div ref="topLeft" style={webcamBorderDivStyle.Extended({left: 0, width: borderWidth, top: 0, height: borderWidth,
						boxSizing: "border-box", borderRadius: `${r}px 0 0 0`, borderWidth: "1px 0 0 1px", zIndex: 2,
						clip: `rect(-1000px ${borderWidth} ${borderWidth} -1000px)`})}/>
					<div ref="topRight" style={webcamBorderDivStyle.Extended({right: 0, width: borderWidth, top: 0, height: borderWidth,
						boxSizing: "border-box", borderRadius: `0 ${r}px 0 0`, borderWidth: "1px 1px 0 0", zIndex: 2,
						clip: `rect(-1000px ${borderWidth} ${borderWidth} 0)`})}/>*/}
					
					<div ref="topBar_leftTopAndRight" style={webcamBorderDivStyle.Extended({left: 0, right: 0, top: 0, height: borderWidth,
						boxSizing: "border-box", borderWidth: "1px 1px 0 1px", borderRadius: `${r}px ${r}px 0 0`, zIndex: 1,
						clip: `rect(-1000px 1000px ${borderWidth} -1000px)`})}/>
					<div ref="topBar_bottom" style={webcamBorderDivStyle.Extended({left: 0, right: 0, top: 0, height: borderWidth,
						boxSizing: "border-box", borderWidth: "1px 1px 1px 1px", borderRadius: `${r}px ${r}px 0 0`, zIndex: 1,
						clip: `rect(${borderWidth - 1} ${rightBarRect.width - borderWidth} 1000px ${borderWidth})`})}/>
				</div>
			</div>
		);
	}
}

export default RightBar;