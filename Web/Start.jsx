// js: early libraries
/*require("globals!./Packages/React/react.js");
require("globals!./Packages/React/react-dom.js");*/

//require("globals!./Packages/Babel/browser.min.js");
require("globals!./Packages/General/ClassExtensions.js");
require("globals!./Packages/JQuery/JQuery1.9.1.js");
require("imports?this=>window!globals!./Packages/JQuery/JQueryResize.js");
require("globals!./Packages/JQuery/JQueryNodeListener.js");
require("globals!./Packages/JQuery/JQueryOthers.js");
require("globals!./Packages/JQueryUI/JQueryUI1.11.0.js");
require("globals!./Packages/JQueryUI/JQueryUIMenuBar.js");
require("globals!./Packages/JQueryUI/JQueryUIContextMenu.js");
require("globals!./Packages/JQueryUI/JQueryUIOthers.js");
require("globals!./Packages/JQueryFancyTree/Core.js");
require("globals!./Packages/JQueryFancyTree/DND.js");
require("globals!./Packages/JQueryScrollbar/JQueryScrollbar.js");
require("globals!./Packages/Spectrum/Spectrum.js");
require("globals!./Packages/V/V.js");
require("globals!./Packages/V/VDebug.js");
require("globals!./Packages/V/VResizable.js");
require("globals!./Packages/V/VTabView.js");
window.VMessageBox = require("Packages/V/VMessageBox");
require("globals!./Packages/V/VFileBrowser.js");
require("globals!./Packages/V/VPersistentData.js");
require("globals!./Packages/VDF/VDF.js");
require("globals!./Packages/VDF/VDFTypeInfo.js");
require("globals!./Packages/VDF/VDFNode.js");
require("globals!./Packages/VDF/VDFLoader.js");
require("globals!./Packages/VDF/VDFTokenParser.js");
require("globals!./Packages/VDF/VDFSaver.js");
require("globals!./Packages/General/ChangeManager.js");
require("globals!./Packages/General/MutationEventsPolyfill.js");
require("globals!./Packages/General/LateClassExtensions.js");

// js: libraries
require("globals!./Packages/VTree/Node.js");
require("globals!./Packages/VTree/NodeExtras.js");
require("globals!./Packages/VTree/Change.js");
require("globals!./Packages/VTree/CSBridge.js");
require("globals!./Packages/General/Globals.jsx");

// node modules
require("globals!./node_modules/classnames/index.js");
// require("globals!./node_modules/react/lib/ReactFragment.js");

// react
//<link rel="stylesheet" type="text/css" href="Packages/ReactComponents/TabView.css"/>
require("globals!./Packages/ReactComponents/TabView.jsx");

// VUI
require("globals!./Packages/VUI/VUI.js");
require("globals!./Packages/VUI/General.js");
require("globals!./Packages/VUI/ScrollView.js");

// js: frame
require("globals!./Frame/VScript/Script.js");

require("globals!./VOverlay");