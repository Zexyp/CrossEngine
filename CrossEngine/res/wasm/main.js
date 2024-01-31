import { dotnet } from './dotnet.js'

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
	.withDiagnosticTracing(false)
	.withApplicationArgumentsFromQuery()
	.create();

const config = getConfig();
const INTEROP_EXPORT_ASSEMBLY = "CrossEngine.dll";
const exports = await getAssemblyExports(INTEROP_EXPORT_ASSEMBLY); // config.mainAssemblyName
const interop = exports.CrossEngine.Platform.Wasm.Interop;

var canvas = globalThis.document.getElementById("canvas");
dotnet.instance.Module["canvas"] = canvas;

globalThis.window.addEventListener("orientationchange", function (event) {
	globalThis.document.documentElement.requestFullscreen();
});

setModuleImports("main.js", {
	initialize: () => {

		var checkCanvasResize = (dispatch) => {
			var devicePixelRatio = window.devicePixelRatio || 1.0;
			var displayWidth = canvas.clientWidth * devicePixelRatio;
			var displayHeight = canvas.clientHeight * devicePixelRatio;

			if (canvas.width != displayWidth || canvas.height != displayHeight) {
				canvas.width = displayWidth;
				canvas.height = displayHeight;
				dispatch = true;
			}

			if (dispatch)
				interop.OnCanvasResize(displayWidth, displayHeight);
		}

		function checkCanvasResizeFrame() {
			checkCanvasResize(false);
			requestAnimationFrame(checkCanvasResizeFrame);
		}

		var keyDown = (e) => {
			e.stopPropagation();
			var shift = e.shiftKey;
			var ctrl = e.ctrlKey;
			var alt = e.altKey;
			var repeat = e.repeat;
			var code = e.code;

			interop.OnKeyDown(shift, ctrl, alt, repeat, code);
		}

		var keyUp = (e) => {
			e.stopPropagation();
			var shift = e.shiftKey;
			var ctrl = e.ctrlKey;
			var alt = e.altKey;
			var code = e.code;

			interop.OnKeyUp(shift, ctrl, alt, code);
		}

		var mouseMove = (e) => {
			var x = e.offsetX;
			var y = e.offsetY;
			interop.OnMouseMove(x, y);
		}

		var mouseDown = (e) => {
			var shift = e.shiftKey;
			var ctrl = e.ctrlKey;
			var alt = e.altKey;
			var button = e.button;

			interop.OnMouseDown(shift, ctrl, alt, button);

			if (navigator.userAgent.match(/Android/i))
				document.documentElement.requestFullscreen();
		}

		var mouseUp = (e) => {
			var shift = e.shiftKey;
			var ctrl = e.ctrlKey;
			var alt = e.altKey;
			var button = e.button;

			interop.OnMouseUp(shift, ctrl, alt, button);
		}

		var shouldIgnore = (e) => {
			e.preventDefault();
			return e.touches.length > 1 || e.type == "touchend" && e.touches.length > 0;
		}

		var touchStart = (e) => {
			if (shouldIgnore(e))
				return;

			var devicePixelRatio = window.devicePixelRatio || 1.0;
			var shift = e.shiftKey;
			var ctrl = e.ctrlKey;
			var alt = e.altKey;
			var button = 0;
			var touch = e.changedTouches[0];
			var bcr = e.target.getBoundingClientRect();
			var x = (touch.clientX - bcr.x) * devicePixelRatio;
			var y = (touch.clientY - bcr.y) * devicePixelRatio;

			interop.OnMouseMove(x, y);
			interop.OnMouseDown(shift, ctrl, alt, button);
		}

		var touchMove = (e) => {
			if (shouldIgnore(e))
				return;

			var devicePixelRatio = window.devicePixelRatio || 1.0;
			var touch = e.changedTouches[0];
			var bcr = e.target.getBoundingClientRect();
			var x = (touch.clientX - bcr.x) * devicePixelRatio;
			var y = (touch.clientY - bcr.y) * devicePixelRatio;

			interop.OnMouseMove(x, y);
		}

		var touchEnd = (e) => {
			if (shouldIgnore(e))
				return;

			var devicePixelRatio = window.devicePixelRatio || 1.0;
			var shift = e.shiftKey;
			var ctrl = e.ctrlKey;
			var alt = e.altKey;
			var button = 0;
			var touch = e.changedTouches[0];
			var bcr = e.target.getBoundingClientRect();
			var x = (touch.clientX - bcr.x) * devicePixelRatio;
			var y = (touch.clientY - bcr.y) * devicePixelRatio;

			interop.OnMouseMove(x, y);
			interop.OnMouseUp(shift, ctrl, alt, button);
		}

		//canvas.addEventListener("contextmenu", (e) => e.preventDefault(), false);
		canvas.addEventListener("keydown", keyDown, false);
		canvas.addEventListener("keyup", keyUp, false);
		canvas.addEventListener("mousemove", mouseMove, false);
		canvas.addEventListener("mousedown", mouseDown, false);
		canvas.addEventListener("mouseup", mouseUp, false);
		canvas.addEventListener("mousewheel", function (e) {
			if (e.ctrlKey) {
				e.preventDefault();
			}
		}, true);
		canvas.addEventListener("touchstart", touchStart, false);
		canvas.addEventListener("touchmove", touchMove, false);
		canvas.addEventListener("touchend", touchEnd, false);
		checkCanvasResize(true);
		checkCanvasResizeFrame();

		//canvas.tabIndex = 1000;
		canvas.focus();

		interop.SetRootUri(window.location.toString());

		var langs = navigator.languages || [];
		for (var i = 0; i < langs.length; i++)
			interop.AddLocale(langs[i]);
		interop.AddLocale(navigator.language);
		interop.AddLocale(navigator.userLanguage);
	}
});

await dotnet.run();
