using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace CrossEngine.Platform.Wasm
{
    [SupportedOSPlatform("browser")]
    public static partial class Interop
	{
		[JSImport("initialize", "main.js")]
		public static partial void Initialize();

        [JSImport("globalThis.alert")]
        public static partial void Alert([JSMarshalAs<JSType.Any>] object? message);

        public static partial class Console
		{
            [JSImport("globalThis.console.log")]
            public static partial void Log([JSMarshalAs<JSType.String>] string message);
            [JSImport("globalThis.console.log")]
            public static partial void Log([JSMarshalAs<JSType.String>] string message, [JSMarshalAs<JSType.String>] string arg);
            [JSImport("globalThis.console.debug")]
            public static partial void Debug([JSMarshalAs<JSType.String>] string message);
            [JSImport("globalThis.console.debug")]
            public static partial void Debug([JSMarshalAs<JSType.String>] string message, [JSMarshalAs<JSType.String>] string arg);
            [JSImport("globalThis.console.info")]
            public static partial void Info([JSMarshalAs<JSType.String>] string message);
            [JSImport("globalThis.console.info")]
            public static partial void Info([JSMarshalAs<JSType.String>] string message, [JSMarshalAs<JSType.String>] string arg);
            [JSImport("globalThis.console.warn")]
            public static partial void Warn([JSMarshalAs<JSType.String>] string message);
            [JSImport("globalThis.console.warn")]
            public static partial void Warn([JSMarshalAs<JSType.String>] string message, [JSMarshalAs<JSType.String>] string arg);
            [JSImport("globalThis.console.error")]
            public static partial void Error([JSMarshalAs<JSType.String>] string message);
            [JSImport("globalThis.console.error")]
            public static partial void Error([JSMarshalAs<JSType.String>] string message, [JSMarshalAs<JSType.String>] string arg);
        }

        [JSExport]
		public static void OnKeyDown(bool shift, bool ctrl, bool alt, bool repeat, string code)
		{
			KeyDown?.Invoke(shift, ctrl, alt, repeat, code);
        }

		[JSExport]
		public static void OnKeyUp(bool shift, bool ctrl, bool alt, string code)
		{
            KeyUp?.Invoke(shift, ctrl, alt, code);
        }

        [JSExport]
		public static void OnMouseMove(float x, float y)
		{
            MouseMove?.Invoke(x, y);
        }

        [JSExport]
		public static void OnMouseDown(bool shift, bool ctrl, bool alt, int button)
		{
            MouseDown?.Invoke(shift, ctrl, alt, button);
        }

        [JSExport]
		public static void OnMouseUp(bool shift, bool ctrl, bool alt, int button)
		{
            MouseUp?.Invoke(shift, ctrl, alt, button);
        }

        [JSExport]
		public static void OnCanvasResize(float width, float height)
		{
            CanvasResize?.Invoke(width, height);
            //Test.CanvasResized((int)width, (int)height);
        }

        [JSExport]
		public static void SetRootUri(string uri)
		{
			//Test.BaseAddress = new Uri(uri);
		}

		[JSExport]
		public static void AddLocale(string locale)
		{
		}

		public delegate void KeyDownCallback(bool shift, bool ctrl, bool alt, bool repeat, string code);
		public delegate void KeyUpCallback(bool shift, bool ctrl, bool alt, string code);
		public delegate void MouseMoveCallback(float x, float y);
		public delegate void MouseDownCallback(bool shift, bool ctrl, bool alt, int button);
		public delegate void MouseUpCallback(bool shift, bool ctrl, bool alt, int button);
		public delegate void CanvasResizeCallback(float width, float height);

        public static event KeyDownCallback KeyDown;
		public static event KeyUpCallback KeyUp;
        public static event MouseMoveCallback MouseMove;
        public static event MouseDownCallback MouseDown;
        public static event MouseUpCallback MouseUp;
        public static event CanvasResizeCallback CanvasResize;
    }
}