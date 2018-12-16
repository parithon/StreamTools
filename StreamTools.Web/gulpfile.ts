import { series, parallel, src, dest, watch } from 'gulp';
import * as ts from 'gulp-typescript';
import * as sass from 'gulp-sass';
import * as del from 'del';
import * as autoprefixer from 'gulp-autoprefixer';
import * as csso from 'gulp-csso';
import * as rename from 'gulp-rename';
import * as sourcemaps from 'gulp-sourcemaps';
import * as uglify from 'gulp-uglify';

const tsProject = ts.createProject('tsconfig.json');

const buildCSS = series(transpileSASS, minifyCSS);
const buildJS = series(transpileTS, minifyJS);

const clean = parallel(cleanCSS, cleanJS);

function cleanCSS(cb: Function) {
	return del(['./wwwroot/css/**'], cb);
}

function cleanJS(cb: Function) {
	return del(['./wwwroot/js/**'], cb);
}

function minifyCSS(cb: Function) {
	return src(['./wwwroot/css/**/*.css'])
		.pipe(csso())
		.pipe(rename({ extname: '.min.css' }))
		.pipe(dest('./wwwroot/css'));
}

function minifyJS(cb: Function) {
	return src(['./wwwroot/js/**/*.js'])
		.pipe(uglify())
		.pipe(rename({ extname: '.min.js' }))
		.pipe(dest('./wwwroot/js'));
}

function transpileSASS(cb: Function) {
	return src(['./Styles/**/*.scss', '!./Styles/lib/**/*'])
		.pipe(sourcemaps.init())
		.pipe(sass())
		.pipe(autoprefixer({
			browsers: ['last 2 versions'],
			cascade: false
		}))
		.pipe(sourcemaps.mapSources((sourcePath, file) => {
			return `../../Styles/${sourcePath}`;
		}))
		.pipe(sourcemaps.write('.'))
		.pipe(dest("./wwwroot/css/"));
}

function transpileTS(cb: Function) {
	return tsProject.src()
		.pipe(sourcemaps.init())
		.pipe(tsProject())
		.pipe(sourcemaps.write('.'))
		.pipe(dest(tsProject.config.compilerOptions.outDir));
}

function watchCSS(cb: Function) {
	return watch(['./Styles/**/*.scss'], series(cleanCSS, buildCSS));
}

function watchSASS(cb: Function) {
	return watch(['./Scripts/**/*.ts'], series(cleanJS, buildJS));
}

export const build = parallel(buildCSS, buildJS);
export const watcher = parallel(watchCSS, watchSASS);
export default series(clean, build);