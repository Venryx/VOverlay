var gulp         = require('gulp');
var babel        = require('gulp-babel');
var sourcemaps   = require('gulp-sourcemaps');

var onError = function(err) {
  notify.onError({
    title:    "Error",
    message:  "<%= error %>",
  })(err);
  this.emit('end');
};

var plumberOptions = {
  errorHandler: onError,
};

var jsFiles = {
  vendor: [

  ],
  source: [
    '**/*.jsx',
  ]
};

// Concatenate jsFiles.vendor and jsFiles.source into one JS file.
// Run copy-react and eslint before concatenating
gulp.task('concat', [], function() {
  return gulp.src(jsFiles.vendor.concat(jsFiles.source))
    .pipe(sourcemaps.init())
    .pipe(babel({
      compact: true,
	  blacklist: ["useStrict"]
    }))
    .pipe(sourcemaps.write('./'))
    .pipe(gulp.dest('CompiledJS'));
});

// Watch JSX
/*gulp.task('watch', function() {
	//gulp.watch('Packages/**#/*.jsx', ['concat']);
	//gulp.watch('BiomeDefense/**#/*.jsx', ['concat']);
  var watcher = gulp.watch('BiomeDefense/**#/*.jsx', ['concat']);
  watcher.on('change', function(event) {
    console.log('File ' + event.path + ' was ' + event.type + ', running tasks...');
  });
});*/
// this version only compiles the file that was changed
var watch = require('gulp-watch');
gulp.task('watch', function() {
	watch("**/*.jsx")
	.pipe(sourcemaps.init())
	.pipe(babel({
		compact: true,
		blacklist: ["useStrict"]
	}))
	.pipe(sourcemaps.write('./'))
	.pipe(gulp.dest('CompiledJS'));
});

//gulp.task('build', ['sass', 'copy-js-vendor', 'concat']);
gulp.task('build', ['concat']);
gulp.task('default', ['build', 'watch']);