var path = require("path");
var webpack = require("webpack");
var StringReplacePlugin = require("string-replace-webpack-plugin");
var WriteFilePlugin = require("write-file-webpack-plugin");

//const isDevServer = process.argv[1].indexOf('webpack-dev-server') != -1;
const devServer = process.argv.find(v=>v.indexOf('webpack-dev-server') != -1);
//var isDevServer = path.basename(require.main.filename) === 'webpack-dev-server.js';

var Path = path.resolve;

//var excludeNodeModules = /node_modules\/|node_modules_CustomExternal\//;
//var excludeNodeModules = /^(?!\.|globals!\.)|node_modules/;
//var excludeNodeModules = /node_modules|Libraries/;
//var excludeNodeModules = /node_modules/;
var excludeNodeModules = /node_modules\//;

module.exports = {
	/*entry: {
		app: ["./Start.js"]
	},*/
	//entry: ['webpack/hot/dev-server', './Start.js'],
	entry: {
		//html: "./BiomeDefense.html",
		javascript: ['babel-polyfill', "./Start.jsx"]
	},
	resolve: {
		root: [Path("./"), Path("./node_modules_CustomInternal"), Path("./node_modules_CustomExternal")],
		extensions: ['', '.js', '.jsx']
	},
	externals: [
        /closure-library/
    ],
	/*externals: {
        "nodejs": "../closure-library/closure/goog/bootstrap/nodejs"
	},*/
	module: {
		loaders: [
			{
				test: /\.scss$/,
				exclude: excludeNodeModules,
				loaders: ['style', 'css', 'sass']
			},
			/*{
				test: /\.scss$/,
				exclude: excludeNodeModules,
				loader: StringReplacePlugin.replace({
					replacements: [
						{
							pattern: /url\(\/Main\/Packages/,
							replacement: function(match, offset, string) {
								return devServer ? "url(/Packages" : match; //"url(/UI/Main/Packages";
							}
						}
					]
				})
			},*/
			{
				test: /\.css$/,
				exclude: excludeNodeModules,
				loaders: ['style', 'css']
			},
			{
				test: /\.js|\.jsx/,
				exclude: excludeNodeModules,
				loader: StringReplacePlugin.replace({
					replacements: [
						{
							pattern: /^("use script";?)?(.*)/,
							replacement: function (match, p1, p2, offset, string) {
								if (p1)
									return p1 + "var g = window;" + p2;
								return "var g = window;" + match;
							}
						}
					]
				})
            },
			{
				test: /\\Packages\\VDF\\|\\Globals\.jsx$/,
				exclude: excludeNodeModules,
				loader: StringReplacePlugin.replace({
					replacements: [
						{
							pattern: /^var ([0-9A-Z_a-z]+);/gm,
							replacement: function (match, p1, offset, string) {
								return "window." + p1 + " = undefined;";
							}
						},
						{
							pattern: /^var /gm,
							replacement: function (match, p1, offset, string) {
								return "window.";
							}
						},
						{
							pattern: /^function ([0-9A-Z_a-z]+)\(/gm,
							replacement: function (match, p1, offset, string) {
								return "window." + p1 + " = " + match;
							}
						}
					]
				})
            },
			{
				test: /\.jsx$/,
				exclude: excludeNodeModules,
				loaders: ["babel-loader"]
			},
			{
				test: /\.html$/,
				exclude: excludeNodeModules,
				loader: "file?name=[name].[ext]"
			}
		],
	},
	
	plugins: [
		new StringReplacePlugin(),
		new webpack.SourceMapDevToolPlugin({
			// for production, uncomment (eval mode is used otherwise, which is 'not safe for production')
			// "filename defines the output filename of the SourceMap. If no value is provided the SourceMap is inlined."
			//filename: '[file].map', // make-so: source-maps work when this is commented again! X(
			
			module: true, // "When false loaders do not generate SourceMaps and the transformed code is used as source instead."
			columns: false // "When false column mappings in SourceMaps are ignored and a faster SourceMap implementation is used."
		}),
		new WriteFilePlugin({
			//test: /\.css$/, // write only files matching the given regex
			//useHashIndex: true
		})
	],
	
	// for dev-server webpack
	devServer: {
		//outputPath: Path(__dirname, "Build"),
		outputPath: __dirname,
		//outputPath: Path(__dirname, "Main"),
		//inline:true,
		//host: "biomedefense.local",
		port: 5014,
		contentBase: Path("."),
		publicPath: "/"
	},
	watchOptions: {
		aggregateTimeout: 300, // Delay the rebuilt after the first change. Value is a time in ms.
		//poll: true // bool - enable / disable polling or number - polling delay
		poll: 1000
	},
	// for normal webpack
	output: {
		//path: Path(__dirname, "Build"),
		path: __dirname,
		//path: Path(__dirname, "Main"),
		//publicPath: "/Build/",
		publicPath: "/",
		filename: "Bundle.js"
	}
};