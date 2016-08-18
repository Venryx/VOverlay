//var React = require('react');
import React, { Component, PropTypes } from 'react';
var classNames = require('classnames');

class TabView extends BaseComponent {
	constructor(args) {
		super(args);
		this._bind('setActive')
		this.displayName = 'TabView';
		this.state = {activeTab: 0};
	}

	// actor funcs
	setActive(e, id) {
		//var id = parseInt(e.target.getAttribute('data-tab-id'));
		var onAfterChange = this.props.onAfterChange;
		var onBeforeChange = this.props.onBeforeChange;
		var $selectedPanel = this.refs['tab-panel-' + id];
		var $selectedTabMenu = this.refs['tab-menu-' + id];

		if (onBeforeChange)
			onBeforeChange(id, $selectedPanel, $selectedTabMenu);
		
		this.setState({activeTab: id}, function()  {
			if (onAfterChange)
				onAfterChange(id, $selectedPanel, $selectedTabMenu);
		});

		e.preventDefault();
	}

	getPropsChildren() {
	    var result = this.props.children instanceof Array ? this.props.children : [this.props.children];
	    return result.filter(a=>a != null);
	}

	// render funcs
	render() {
		var menuItems = this._getMenuItems();
		var panelsList = this._getPanels();

		return (
			React.DOM.div({className: "VTabView"}, 
				menuItems, 
				panelsList
			)
		);
	}
	_getMenuItems() {
		if (!this.props.children)
			throw new Error('TabViews must contain at least one TabView.Panel');

		// custom replaced
		//var $menuItems = this.props.children.map(function($panel, index)  {
		var $menuItems = this.getPropsChildren().map(function($panel, index) {
			var ref = 'tab-menu-${index}';
			var title = $panel.props.title;
			var classes = classNames({
				button: true,
				'tabs-menu-item': true,
				'active': this.state.activeTab == index
			});

			return (
				/*React.DOM.li({ref: ref, key: index, className: classes}, 
					React.DOM.a({href: "#", 'data-tab-id': index, onClick: this.setActive}, title)
				)*/
				<div className={classes} ref={ref} key={index} data-tab-id={index} onClick={e=>this.setActive(e, index)}>{title}</div>
			);
		}.bind(this));

		return (
			/*React.DOM.nav({className: "tabs-navigation"}, 
				React.DOM.ul({className: "tabs-menu"}, $menuItems)
			)*/
			<div className="menuDarker" style={{paddingTop: 3}}>
				{$menuItems}
			</div>
		);
	}
	_getPanels() {
		const childrenWithProps = React.Children.map(this.getPropsChildren(),
			child=>React.cloneElement(child, {activeTab: this.state.activeTab}));

	    return <div>{childrenWithProps}</div>;
	}
}
/*TabView.propTypes = {
	activeTab: React.PropTypes.number,
	onBeforeChange: React.PropTypes.func,
	onAfterChange: React.PropTypes.func,
	children: React.PropTypes.oneOfType([
		React.PropTypes.array,
		React.PropTypes.component
	]).isRequired
};*/
TabView.defaultProps = {activeTab: 0};

TabView.Panel = class TabView_Panel extends BaseComponent {
	constructor(args) {
		super(args);
		this.displayName = 'Panel';
	}
	render() {
		var index = this._reactInternalInstance._mountIndex;
		var classes = classNames({
			menu: true,
			'tabs-panel': true,
			'active': this.props.activeTab == index
		});

		return (
			<div ref={"tab-panel-" + index} key={index} className={classes} style={{height: "calc(100% - 24px)"}}>
				{this.props.children}
			</div>
		);
	}
}
/*TabView.Panel.propTypes = {
	title: React.PropTypes.string.isRequired,
	children: React.PropTypes.oneOfType([
		React.PropTypes.array,
		React.PropTypes.component
	]).isRequired
};*/

export default TabView;