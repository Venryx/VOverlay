import React, {Component, PropTypes} from 'react';

class Splitter extends BaseComponent {
    constructor(...args) {
        super(...args);
		this._bind('onMouseDown');
    }

    onMouseDown(event) {
        this.props.onMouseDown(event);
    }

    render() {
        const {direction, className} = this.props;
        const classes = ['Splitter', direction == "horizontal" ? "vertical" : "horizontal", className];
		var style = {}.Extended(this.props.style).Extended({
			[this.props.direction == "horizontal" ? "left" : "top"]: (this.props.pos * 100) + "%"
		});
        return (
            <span className={classes.join(' ')} style={style} onMouseDown={this.onMouseDown} />
        );
    }
}
/*Splitter.propTypes = {
    onMouseDown: PropTypes.func.isRequired,
    split: PropTypes.oneOf(['vertical', 'horizontal']),
    className: PropTypes.string.isRequired,
};*/

export default Splitter;