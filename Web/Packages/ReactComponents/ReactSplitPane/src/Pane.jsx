import React, {Component, PropTypes} from 'react';

class Pane extends BaseComponent {
    constructor(...args) {
        super(...args);
        this.state = {};
    }

    render() {
        const direction = this.props.direction;
        const classes = ['Pane', direction, this.props.className];

        const style = {}.Extended({
            //flex: this.props.size,
			//position: 'relative',
			position: "absolute",
			[this.props.direction == "horizontal" ? "left" : "top"]: (this.props.pos * 100) + "%",
			[this.props.direction == "horizontal" ? "width" : "height"]: (this.props.size * 100) + "%",
            outline: 'none',
        }).Extended(this.props.style);

        return (
            <div className={classes.join(' ')} style={style}>{this.props.children}</div>
        );
    }
}
/*Pane.propTypes = {
    split: PropTypes.oneOf(['vertical', 'horizontal']),
    className: PropTypes.string.isRequired,
    children: PropTypes.object.isRequired,
};*/

export default Pane;