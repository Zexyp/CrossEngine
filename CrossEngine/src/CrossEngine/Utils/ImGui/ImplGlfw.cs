// dear imgui: Platform Backend for GLFW
// This needs to be used along with a Renderer (e.g. OpenGL3, Vulkan, WebGPU..)
// (Info: GLFW is a cross-platform general purpose library for handling windows, inputs, OpenGL/Vulkan graphics context creation, etc.)
// (Requires: GLFW 3.1+. Prefer GLFW 3.3+ or GLFW 3.4+ for full feature support.)

// Implemented features:
//  [X] Platform: Clipboard support.
//  [X] Platform: Mouse support. Can discriminate Mouse/TouchScreen/Pen (Windows only).
//  [X] Platform: Keyboard support. Since 1.87 we are using the io.AddKeyEvent() function. Pass ImGuiKey values to all key functions e.g. ImGui::IsKeyPressed(ImGuiKey_Space). [Legacy GLFW_KEY_* values will also be supported unless IMGUI_DISABLE_OBSOLETE_KEYIO is set]
//  [X] Platform: Gamepad support. Enable with 'io.ConfigFlags |= ImGuiConfigFlags_NavEnableGamepad'.
//  [X] Platform: Mouse cursor shape and visibility. Disable with 'io.ConfigFlags |= ImGuiConfigFlags_NoMouseCursorChange' (note: the resizing cursors requires GLFW 3.4+).

// You can use unmodified imgui_impl_* files in your project. See examples/ folder for examples of using this.
// Prefer including the entire imgui/ repository into your project (either as a copy or as a submodule), and only build the backends you need.
// If you are new to Dear ImGui, read documentation from the docs/ folder + read the top of imgui.cpp.
// Read online: https://github.com/ocornut/imgui/tree/master/docs

// CHANGELOG
// (minor and older changes stripped away, please see git history for details)
//  2023-04-04: Inputs: Added support for io.AddMouseSourceEvent() to discriminate ImGuiMouseSource_Mouse/ImGuiMouseSource_TouchScreen/ImGuiMouseSource_Pen on Windows ONLY, using a custom WndProc hook. (#2702)
//  2023-03-16: Inputs: Fixed key modifiers handling on secondary viewports (docking branch). Broken on 2023/01/04. (#6248, #6034)
//  2023-03-14: Emscripten: Avoid using glfwGetError() and glfwGetGamepadState() which are not correctly implemented in Emscripten emulation. (#6240)
//  2023-02-03: Emscripten: Registering custom low-level mouse wheel handler to get more accurate scrolling impulses on Emscripten. (#4019, #6096)
//  2023-01-04: Inputs: Fixed mods state on Linux when using Alt-GR text input (e.g. German keyboard layout), could lead to broken text input. Revert a 2022/01/17 change were we resumed using mods provided by GLFW, turns out they were faulty.
//  2022-11-22: Perform a dummy glfwGetError() read to cancel missing names with glfwGetKeyName(). (#5908)
//  2022-10-18: Perform a dummy glfwGetError() read to cancel missing mouse cursors errors. Using GLFW_VERSION_COMBINED directly. (#5785)
//  2022-10-11: Using 'nullptr' instead of 'NULL' as per our switch to C++11.
//  2022-09-26: Inputs: Renamed ImGuiKey_ModXXX introduced in 1.87 to ImGuiMod_XXX (old names still supported).
//  2022-09-01: Inputs: Honor GLFW_CURSOR_DISABLED by not setting mouse position.
//  2022-04-30: Inputs: Fixed ImGui_ImplGlfw_TranslateUntranslatedKey() for lower case letters on OSX.
//  2022-03-23: Inputs: Fixed a regression in 1.87 which resulted in keyboard modifiers events being reported incorrectly on Linux/X11.
//  2022-02-07: Added ImGui_ImplGlfw_InstallCallbacks()/ImGui_ImplGlfw_RestoreCallbacks() helpers to facilitate user installing callbacks after initializing backend.
//  2022-01-26: Inputs: replaced short-lived io.AddKeyModsEvent() (added two weeks ago) with io.AddKeyEvent() using ImGuiKey_ModXXX flags. Sorry for the confusion.
//  2021-01-20: Inputs: calling new io.AddKeyAnalogEvent() for gamepad support, instead of writing directly to io.NavInputs[].
//  2022-01-17: Inputs: calling new io.AddMousePosEvent(), io.AddMouseButtonEvent(), io.AddMouseWheelEvent() API (1.87+).
//  2022-01-17: Inputs: always update key mods next and before key event (not in NewFrame) to fix input queue with very low framerates.
//  2022-01-12: *BREAKING CHANGE*: Now using glfwSetCursorPosCallback(). If you called ImGui_ImplGlfw_InitXXX() with install_callbacks = false, you MUST install glfwSetCursorPosCallback() and forward it to the backend via ImGui_ImplGlfw_CursorPosCallback().
//  2022-01-10: Inputs: calling new io.AddKeyEvent(), io.AddKeyModsEvent() + io.SetKeyEventNativeData() API (1.87+). Support for full ImGuiKey range.
//  2022-01-05: Inputs: Converting GLFW untranslated keycodes back to translated keycodes (in the ImGui_ImplGlfw_KeyCallback() function) in order to match the behavior of every other backend, and facilitate the use of GLFW with lettered-shortcuts API.
//  2021-08-17: *BREAKING CHANGE*: Now using glfwSetWindowFocusCallback() to calling io.AddFocusEvent(). If you called ImGui_ImplGlfw_InitXXX() with install_callbacks = false, you MUST install glfwSetWindowFocusCallback() and forward it to the backend via ImGui_ImplGlfw_WindowFocusCallback().
//  2021-07-29: *BREAKING CHANGE*: Now using glfwSetCursorEnterCallback(). MousePos is correctly reported when the host platform window is hovered but not focused. If you called ImGui_ImplGlfw_InitXXX() with install_callbacks = false, you MUST install glfwSetWindowFocusCallback() callback and forward it to the backend via ImGui_ImplGlfw_CursorEnterCallback().
//  2021-06-29: Reorganized backend to pull data from a single structure to facilitate usage with multiple-contexts (all g_XXXX access changed to bd->XXXX).
//  2020-01-17: Inputs: Disable error callback while assigning mouse cursors because some X11 setup don't have them and it generates errors.
//  2019-12-05: Inputs: Added support for new mouse cursors added in GLFW 3.4+ (resizing cursors, not allowed cursor).
//  2019-10-18: Misc: Previously installed user callbacks are now restored on shutdown.
//  2019-07-21: Inputs: Added mapping for ImGuiKey_KeyPadEnter.
//  2019-05-11: Inputs: Don't filter value from character callback before calling AddInputCharacter().
//  2019-03-12: Misc: Preserve DisplayFramebufferScale when main window is minimized.
//  2018-11-30: Misc: Setting up io.BackendPlatformName so it can be displayed in the About Window.
//  2018-11-07: Inputs: When installing our GLFW callbacks, we save user's previously installed ones - if any - and chain call them.
//  2018-08-01: Inputs: Workaround for Emscripten which doesn't seem to handle focus related calls.
//  2018-06-29: Inputs: Added support for the ImGuiMouseCursor_Hand cursor.
//  2018-06-08: Misc: Extracted imgui_impl_glfw.cpp/.h away from the old combined GLFW+OpenGL/Vulkan examples.
//  2018-03-20: Misc: Setup io.BackendFlags ImGuiBackendFlags_HasMouseCursors flag + honor ImGuiConfigFlags_NoMouseCursorChange flag.
//  2018-02-20: Inputs: Added support for mouse cursors (ImGui::GetMouseCursor() value, passed to glfwSetCursor()).
//  2018-02-06: Misc: Removed call to ImGui::Shutdown() which is not available from 1.60 WIP, user needs to call CreateContext/DestroyContext themselves.
//  2018-02-06: Inputs: Added mapping for ImGuiKey_Space.
//  2018-01-25: Inputs: Added gamepad support if ImGuiConfigFlags_NavEnableGamepad is set.
//  2018-01-25: Inputs: Honoring the io.WantSetMousePos by repositioning the mouse (when using navigation and ImGuiConfigFlags_NavMoveMouse is set).
//  2018-01-20: Inputs: Added Horizontal Mouse Wheel support.
//  2018-01-18: Inputs: Added mapping for ImGuiKey_Insert.
//  2017-08-25: Inputs: MousePos set to -FLT_MAX,-FLT_MAX when mouse is unavailable/missing (instead of -1,-1).
//  2016-10-15: Misc: Added a void* user_data parameter to Clipboard function handlers.

//#include "imgui.h"
//#include "imgui_impl_glfw.h"
//
//// Clang warnings with -Weverything
//#if defined(__clang__)
//#pragma clang diagnostic push
//#pragma clang diagnostic ignored "-Wold-style-cast"     // warning: use of old-style cast
//#pragma clang diagnostic ignored "-Wsign-conversion"    // warning: implicit conversion changes signedness
//#endif
//
//// GLFW
//#include <GLFW/glfw3.h>
//
//#ifdef _WIN32
//#undef APIENTRY
//#define GLFW_EXPOSE_NATIVE_WIN32
//#include <GLFW/glfw3native.h>   // for glfwGetWin32Window()
//#endif
//#ifdef __APPLE__
//#define GLFW_EXPOSE_NATIVE_COCOA
//#include <GLFW/glfw3native.h>   // for glfwGetCocoaWindow()
//#endif
//
//#ifdef __EMSCRIPTEN__
//#include <emscripten.h>
//#include <emscripten/html5.h>
//#endif
//
//// We gather version tests as define in order to easily see which features are version-dependent.
//#define GLFW_VERSION_COMBINED           (GLFW_VERSION_MAJOR * 1000 + GLFW_VERSION_MINOR * 100 + GLFW_VERSION_REVISION)
//#ifdef GLFW_RESIZE_NESW_CURSOR          // Let's be nice to people who pulled GLFW between 2019-04-16 (3.4 define) and 2019-11-29 (cursors defines) // FIXME: Remove when GLFW 3.4 is released?
//#define GLFW_HAS_NEW_CURSORS            (GLFW_VERSION_COMBINED >= 3400) // 3.4+ GLFW_RESIZE_ALL_CURSOR, GLFW_RESIZE_NESW_CURSOR, GLFW_RESIZE_NWSE_CURSOR, GLFW_NOT_ALLOWED_CURSOR
//#else
//#define GLFW_HAS_NEW_CURSORS            (0)
//#endif
//#define GLFW_HAS_GAMEPAD_API            (GLFW_VERSION_COMBINED >= 3300) // 3.3+ glfwGetGamepadState() new api
//#define GLFW_HAS_GETKEYNAME             (GLFW_VERSION_COMBINED >= 3200) // 3.2+ glfwGetKeyName()
//#define GLFW_HAS_GETERROR               (GLFW_VERSION_COMBINED >= 3300) // 3.3+ glfwGetError()

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Evergine.Bindings.Imgui;
using static Evergine.Bindings.Imgui.ImguiNative;
using GLFW;

namespace CrossEngine.Utils.ImGui
{
    static class ImplGlfw
    {
        // GLFW data
        enum GlfwClientApi
        {
            Unknown,
            OpenGL,
            Vulkan
        };

        unsafe struct ImGui_ImplGlfw_Data
        {
            public Window              Window;
            public GlfwClientApi        ClientApi;
            public double               Time;
            public Window              MouseWindow;
            public Cursor               MouseCursors;
            private Cursor              _mouseCursors1;
            private Cursor              _mouseCursors2;
            private Cursor              _mouseCursors3;
            private Cursor              _mouseCursors4;
            private Cursor              _mouseCursors5;
            private Cursor              _mouseCursors6;
            private Cursor              _mouseCursors7;
            private Cursor              _mouseCursors8;
            public ImVec2               LastValidMousePos;
            public bool                 InstalledCallbacks;
            public bool                 CallbacksChainForAllWindows;

            // Chain GLFW callbacks: our callbacks will call the user's previously installed callbacks, if any.
            public IntPtr               PrevUserCallbackWindowFocus;
            public IntPtr               PrevUserCallbackCursorPos;
            public IntPtr               PrevUserCallbackCursorEnter;
            public IntPtr               PrevUserCallbackMousebutton;
            public IntPtr               PrevUserCallbackScroll;
            public IntPtr               PrevUserCallbackKey;
            public IntPtr               PrevUserCallbackChar;
            public IntPtr               PrevUserCallbackMonitor;
#if _WIN32
            public WNDPROC              GlfwWndProc;
#endif
        };

        // Backend data stored in io.BackendPlatformUserData to allow support for multiple Dear ImGui contexts
        // It is STRONGLY preferred that you use docking branch with multi-viewports (== single Dear ImGui context + multiple windows) instead of multiple Dear ImGui contexts.
        // FIXME: multi-context support is not well tested and probably dysfunctional in this backend.
        // - Because glfwPollEvents() process all windows and some events may be called outside of it, you will need to register your own callbacks
        //   (passing install_callbacks=false in ImGui_ImplGlfw_InitXXX functions), set the current dear imgui context and then call our callbacks.
        // - Otherwise we may need to store a GLFWWindow* -> ImGuiContext* map and handle this in the backend, adding a little bit of extra complexity to it.
        // FIXME: some shared resources (mouse cursor shape, gamepad) are mishandled when using multi-context.
        static unsafe ImGui_ImplGlfw_Data* ImGui_ImplGlfw_GetBackendData()
        {
            return igGetCurrentContext() != IntPtr.Zero ? (ImGui_ImplGlfw_Data*)igGetIO()->BackendPlatformUserData : null;
        }

        // Functions
        static unsafe char* ImGui_ImplGlfw_GetClipboardText(void* user_data)
        {
            return (char*)Marshal.StringToHGlobalAuto(Glfw.GetClipboardString(*(Window*)user_data));
        }

        static unsafe void ImGui_ImplGlfw_SetClipboardText(void* user_data, char* text)
        {
            Glfw.SetClipboardString(*(Window*)user_data, new string(text));
        }

        static ImGuiKey ImGui_ImplGlfw_KeyToImGuiKey(int key)
        {
            switch ((Keys)key)
            {
                case Keys.Tab: return ImGuiKey.Tab;
                case Keys.Left: return ImGuiKey.LeftArrow;
                case Keys.Right: return ImGuiKey.RightArrow;
                case Keys.Up: return ImGuiKey.UpArrow;
                case Keys.Down: return ImGuiKey.DownArrow;
                case Keys.PageUp: return ImGuiKey.PageUp;
                case Keys.PageDown: return ImGuiKey.PageDown;
                case Keys.Home: return ImGuiKey.Home;
                case Keys.End: return ImGuiKey.End;
                case Keys.Insert: return ImGuiKey.Insert;
                case Keys.Delete: return ImGuiKey.Delete;
                case Keys.Backspace: return ImGuiKey.Backspace;
                case Keys.Space: return ImGuiKey.Space;
                case Keys.Enter: return ImGuiKey.Enter;
                case Keys.Escape: return ImGuiKey.Escape;
                case Keys.Apostrophe: return ImGuiKey.Apostrophe;
                case Keys.Comma: return ImGuiKey.Comma;
                case Keys.Minus: return ImGuiKey.Minus;
                case Keys.Period: return ImGuiKey.Period;
                case Keys.Slash: return ImGuiKey.Slash;
                case Keys.SemiColon: return ImGuiKey.Semicolon;
                case Keys.Equal: return ImGuiKey.Equal;
                case Keys.LeftBracket: return ImGuiKey.LeftBracket;
                case Keys.Backslash: return ImGuiKey.Backslash;
                case Keys.RightBracket: return ImGuiKey.RightBracket;
                case Keys.GraveAccent: return ImGuiKey.GraveAccent;
                case Keys.CapsLock: return ImGuiKey.CapsLock;
                case Keys.ScrollLock: return ImGuiKey.ScrollLock;
                case Keys.NumLock: return ImGuiKey.NumLock;
                case Keys.PrintScreen: return ImGuiKey.PrintScreen;
                case Keys.Pause: return ImGuiKey.Pause;
                case Keys.Numpad0: return ImGuiKey.Keypad0;
                case Keys.Numpad1: return ImGuiKey.Keypad1;
                case Keys.Numpad2: return ImGuiKey.Keypad2;
                case Keys.Numpad3: return ImGuiKey.Keypad3;
                case Keys.Numpad4: return ImGuiKey.Keypad4;
                case Keys.Numpad5: return ImGuiKey.Keypad5;
                case Keys.Numpad6: return ImGuiKey.Keypad6;
                case Keys.Numpad7: return ImGuiKey.Keypad7;
                case Keys.Numpad8: return ImGuiKey.Keypad8;
                case Keys.Numpad9: return ImGuiKey.Keypad9;
                case Keys.NumpadDecimal: return ImGuiKey.KeypadDecimal;
                case Keys.NumpadDivide: return ImGuiKey.KeypadDivide;
                case Keys.NumpadMultiply: return ImGuiKey.KeypadMultiply;
                case Keys.NumpadSubtract: return ImGuiKey.KeypadSubtract;
                case Keys.NumpadAdd: return ImGuiKey.KeypadAdd;
                case Keys.NumpadEnter: return ImGuiKey.KeypadEnter;
                case Keys.NumpadEqual: return ImGuiKey.KeypadEqual;
                case Keys.LeftShift: return ImGuiKey.LeftShift;
                case Keys.LeftControl: return ImGuiKey.LeftCtrl;
                case Keys.LeftAlt: return ImGuiKey.LeftAlt;
                case Keys.LeftSuper: return ImGuiKey.LeftSuper;
                case Keys.RightShift: return ImGuiKey.RightShift;
                case Keys.RightControl: return ImGuiKey.RightCtrl;
                case Keys.RightAlt: return ImGuiKey.RightAlt;
                case Keys.RightSuper: return ImGuiKey.RightSuper;
                case Keys.Menu: return ImGuiKey.Menu;
                case Keys.Alpha0: return ImGuiKey._0;
                case Keys.Alpha1: return ImGuiKey._1;
                case Keys.Alpha2: return ImGuiKey._2;
                case Keys.Alpha3: return ImGuiKey._3;
                case Keys.Alpha4: return ImGuiKey._4;
                case Keys.Alpha5: return ImGuiKey._5;
                case Keys.Alpha6: return ImGuiKey._6;
                case Keys.Alpha7: return ImGuiKey._7;
                case Keys.Alpha8: return ImGuiKey._8;
                case Keys.Alpha9: return ImGuiKey._9;
                case Keys.A: return ImGuiKey.A;
                case Keys.B: return ImGuiKey.B;
                case Keys.C: return ImGuiKey.C;
                case Keys.D: return ImGuiKey.D;
                case Keys.E: return ImGuiKey.E;
                case Keys.F: return ImGuiKey.F;
                case Keys.G: return ImGuiKey.G;
                case Keys.H: return ImGuiKey.H;
                case Keys.I: return ImGuiKey.I;
                case Keys.J: return ImGuiKey.J;
                case Keys.K: return ImGuiKey.K;
                case Keys.L: return ImGuiKey.L;
                case Keys.M: return ImGuiKey.M;
                case Keys.N: return ImGuiKey.N;
                case Keys.O: return ImGuiKey.O;
                case Keys.P: return ImGuiKey.P;
                case Keys.Q: return ImGuiKey.Q;
                case Keys.R: return ImGuiKey.R;
                case Keys.S: return ImGuiKey.S;
                case Keys.T: return ImGuiKey.T;
                case Keys.U: return ImGuiKey.U;
                case Keys.V: return ImGuiKey.V;
                case Keys.W: return ImGuiKey.W;
                case Keys.X: return ImGuiKey.X;
                case Keys.Y: return ImGuiKey.Y;
                case Keys.Z: return ImGuiKey.Z;
                case Keys.F1: return ImGuiKey.F1;
                case Keys.F2: return ImGuiKey.F2;
                case Keys.F3: return ImGuiKey.F3;
                case Keys.F4: return ImGuiKey.F4;
                case Keys.F5: return ImGuiKey.F5;
                case Keys.F6: return ImGuiKey.F6;
                case Keys.F7: return ImGuiKey.F7;
                case Keys.F8: return ImGuiKey.F8;
                case Keys.F9: return ImGuiKey.F9;
                case Keys.F10: return ImGuiKey.F10;
                case Keys.F11: return ImGuiKey.F11;
                case Keys.F12: return ImGuiKey.F12;
                default: return ImGuiKey.None;
            }
        }

        // X11 does not include current pressed/released modifier key in 'mods' flags submitted by GLFW
        // See https://github.com/ocornut/imgui/issues/6034 and https://github.com/glfw/glfw/issues/1630
        static unsafe void ImGui_ImplGlfw_UpdateKeyModifiers(Window window)
        {
            ImGuiIO* io = igGetIO();
            io->AddKeyEvent((ImGuiKey)ImGuiModFlags.Ctrl,  (Glfw.GetKey(window, Keys.LeftControl) == InputState.Press) || (Glfw.GetKey(window, Keys.RightControl) == InputState.Press));
            io->AddKeyEvent((ImGuiKey)ImGuiModFlags.Shift, (Glfw.GetKey(window, Keys.LeftShift)   == InputState.Press) || (Glfw.GetKey(window, Keys.RightShift)   == InputState.Press));
            io->AddKeyEvent((ImGuiKey)ImGuiModFlags.Alt,   (Glfw.GetKey(window, Keys.LeftAlt)     == InputState.Press) || (Glfw.GetKey(window, Keys.RightAlt)     == InputState.Press));
            io->AddKeyEvent((ImGuiKey)ImGuiModFlags.Super, (Glfw.GetKey(window, Keys.LeftSuper)   == InputState.Press) || (Glfw.GetKey(window, Keys.RightSuper)   == InputState.Press));
        }

        static unsafe bool ImGui_ImplGlfw_ShouldChainCallback(Window window)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            return bd->CallbacksChainForAllWindows ? true : (window == bd->Window);
        }

        static unsafe void ImGui_ImplGlfw_MouseButtonCallback(Window window, int button, int action, int mods)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            if (bd->PrevUserCallbackMousebutton != IntPtr.Zero && ImGui_ImplGlfw_ShouldChainCallback(window))
                Marshal.GetDelegateForFunctionPointer<MouseButtonCallback>(bd->PrevUserCallbackMousebutton)(window, (MouseButton)button, (InputState)action, (ModifierKeys)mods);

            ImGui_ImplGlfw_UpdateKeyModifiers(window);

            ImGuiIO* io = igGetIO();
            if (button >= 0 && button < (int)ImGuiMouseButton.COUNT)
                io->AddMouseButtonEvent(button, action == (int)InputState.Press);
        }

        static unsafe void ImGui_ImplGlfw_ScrollCallback(Window window, double xoffset, double yoffset)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            if (bd->PrevUserCallbackScroll != IntPtr.Zero && ImGui_ImplGlfw_ShouldChainCallback(window))
                Marshal.GetDelegateForFunctionPointer<MouseCallback>(bd->PrevUserCallbackScroll)(window, xoffset, yoffset);

#if __EMSCRIPTEN__
            // Ignore GLFW events: will be processed in ImGui_ImplEmscripten_WheelCallback().
            return;
#endif

            ImGuiIO* io = igGetIO();
            io->AddMouseWheelEvent((float)xoffset, (float)yoffset);
        }

        static int ImGui_ImplGlfw_TranslateUntranslatedKey(int key, int scancode)
        {
#if GLFW_HAS_GETKEYNAME && !__EMSCRIPTEN__
            // GLFW 3.1+ attempts to "untranslate" keys, which goes the opposite of what every other framework does, making using lettered shortcuts difficult.
            // (It had reasons to do so: namely GLFW is/was more likely to be used for WASD-type game controls rather than lettered shortcuts, but IHMO the 3.1 change could have been done differently)
            // See https://github.com/glfw/glfw/issues/1502 for details.
            // Adding a workaround to undo this (so our keys are translated->untranslated->translated, likely a lossy process).
            // This won't cover edge cases but this is at least going to cover common cases.
            if (key >= GLFW_KEY_KP_0 && key <= GLFW_KEY_KP_EQUAL)
                return key;
            GLFWerrorfun prev_error_callback = glfwSetErrorCallback(nullptr);
            const char* key_name = glfwGetKeyName(key, scancode);
            glfwSetErrorCallback(prev_error_callback);
#if GLFW_HAS_GETERROR && !__EMSCRIPTEN__ // Eat errors (see #5908)
            (void)glfwGetError(nullptr);
#endif
            if (key_name && key_name[0] != 0 && key_name[1] == 0)
            {
                const char char_names[] = "`-=[]\\,;\'./";
                const int char_keys[] = { GLFW_KEY_GRAVE_ACCENT, GLFW_KEY_MINUS, GLFW_KEY_EQUAL, GLFW_KEY_LEFT_BRACKET, GLFW_KEY_RIGHT_BRACKET, GLFW_KEY_BACKSLASH, GLFW_KEY_COMMA, GLFW_KEY_SEMICOLON, GLFW_KEY_APOSTROPHE, GLFW_KEY_PERIOD, GLFW_KEY_SLASH, 0 };
                IM_ASSERT(IM_ARRAYSIZE(char_names) == IM_ARRAYSIZE(char_keys));
                if (key_name[0] >= '0' && key_name[0] <= '9')               { key = GLFW_KEY_0 + (key_name[0] - '0'); }
                else if (key_name[0] >= 'A' && key_name[0] <= 'Z')          { key = GLFW_KEY_A + (key_name[0] - 'A'); }
                else if (key_name[0] >= 'a' && key_name[0] <= 'z')          { key = GLFW_KEY_A + (key_name[0] - 'a'); }
                else if (const char* p = strchr(char_names, key_name[0]))   { key = char_keys[p - char_names]; }
            }
            // if (action == GLFW_PRESS) printf("key %d scancode %d name '%s'\n", key, scancode, key_name);
#else
            //IM_UNUSED(scancode);
#endif
            return key;
        }

        static unsafe void ImGui_ImplGlfw_KeyCallback(Window window, int keycode, int scancode, int action, int mods)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            if (bd->PrevUserCallbackKey != IntPtr.Zero && ImGui_ImplGlfw_ShouldChainCallback(window))
                Marshal.GetDelegateForFunctionPointer<KeyCallback>(bd->PrevUserCallbackKey)(window, (Keys)keycode, scancode, (InputState)action, (ModifierKeys)mods);

            if (action != (int)InputState.Press && action != (int)InputState.Release)
                return;

            ImGui_ImplGlfw_UpdateKeyModifiers(window);

            keycode = ImGui_ImplGlfw_TranslateUntranslatedKey(keycode, scancode);

            ImGuiIO* io = igGetIO();
            ImGuiKey imgui_key = ImGui_ImplGlfw_KeyToImGuiKey(keycode);
            io->AddKeyEvent(imgui_key, (action == (int)InputState.Press));
            io->SetKeyEventNativeData(imgui_key, keycode, scancode); // To support legacy indexing (<1.87 user code)
        }

        static unsafe void ImGui_ImplGlfw_WindowFocusCallback(Window window, int focused)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            if (bd->PrevUserCallbackWindowFocus != IntPtr.Zero && ImGui_ImplGlfw_ShouldChainCallback(window))
                Marshal.GetDelegateForFunctionPointer<FocusCallback>(bd->PrevUserCallbackWindowFocus)(window, focused != 0);

            ImGuiIO* io = igGetIO();
            io->AddFocusEvent(focused != 0);
        }

        static unsafe void ImGui_ImplGlfw_CursorPosCallback(Window window, double x, double y)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            if (bd->PrevUserCallbackCursorPos != IntPtr.Zero && ImGui_ImplGlfw_ShouldChainCallback(window))
                Marshal.GetDelegateForFunctionPointer<MouseCallback>(bd->PrevUserCallbackCursorPos)(window, x, y);
            if (Glfw.GetInputMode(window, InputMode.Cursor) == (int)CursorMode.Disabled)
                return;

            ImGuiIO* io = igGetIO();
            io->AddMousePosEvent((float)x, (float)y);
            bd->LastValidMousePos = new() { x = (float)x, y = (float)y };
        }

        // Workaround: X11 seems to send spurious Leave/Enter events which would make us lose our position,
        // so we back it up and restore on Leave/Enter (see https://github.com/ocornut/imgui/issues/4984)
        static unsafe void ImGui_ImplGlfw_CursorEnterCallback(Window window, int entered)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            if (bd->PrevUserCallbackCursorEnter != IntPtr.Zero && ImGui_ImplGlfw_ShouldChainCallback(window))
                Marshal.GetDelegateForFunctionPointer<MouseEnterCallback>(bd->PrevUserCallbackCursorEnter)(window, entered != 0);
            if (Glfw.GetInputMode(window, InputMode.Cursor) == (int)CursorMode.Disabled)
                return;

            ImGuiIO* io = igGetIO();
            if (entered != 0)
            {
                bd->MouseWindow = window;
                io->AddMousePosEvent(bd->LastValidMousePos.x, bd->LastValidMousePos.y);
            }
            else if (entered == 0 && bd->MouseWindow == window)
            {
                bd->LastValidMousePos = *(ImVec2*)&io->MousePos;
                bd->MouseWindow = Window.None;
                io->AddMousePosEvent(-float.MaxValue, -float.MaxValue);
            }
        }

        static unsafe void ImGui_ImplGlfw_CharCallback(Window window, uint c)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            if (bd->PrevUserCallbackChar != IntPtr.Zero && ImGui_ImplGlfw_ShouldChainCallback(window))
                Marshal.GetDelegateForFunctionPointer<CharCallback>(bd->PrevUserCallbackChar)(window, c);

            ImGuiIO* io = igGetIO();
            io->AddInputCharacter(c);
        }

        static unsafe void ImGui_ImplGlfw_MonitorCallback(Monitor monitor, int status)
        {
	        // Unused in 'master' branch but 'docking' branch will use this, so we declare it ahead of it so if you have to install callbacks you can install this one too.
        }

#if __EMSCRIPTEN__
        static EM_BOOL ImGui_ImplEmscripten_WheelCallback(int, const EmscriptenWheelEvent* ev, void*)
        {
            // Mimic Emscripten_HandleWheel() in SDL.
            // Corresponding equivalent in GLFW JS emulation layer has incorrect quantizing preventing small values. See #6096
            float multiplier = 0.0f;
            if (ev->deltaMode == DOM_DELTA_PIXEL)       { multiplier = 1.0f / 100.0f; } // 100 pixels make up a step.
            else if (ev->deltaMode == DOM_DELTA_LINE)   { multiplier = 1.0f / 3.0f; }   // 3 lines make up a step.
            else if (ev->deltaMode == DOM_DELTA_PAGE)   { multiplier = 80.0f; }         // A page makes up 80 steps.
            float wheel_x = ev->deltaX * -multiplier;
            float wheel_y = ev->deltaY * -multiplier;
            ImGuiIO& io = ImGui::GetIO();
            io.AddMouseWheelEvent(wheel_x, wheel_y);
            //IMGUI_DEBUG_LOG("[Emsc] mode %d dx: %.2f, dy: %.2f, dz: %.2f --> feed %.2f %.2f\n", (int)ev->deltaMode, ev->deltaX, ev->deltaY, ev->deltaZ, wheel_x, wheel_y);
            return EM_TRUE;
        }
#endif

#if _WIN32
        // GLFW doesn't allow to distinguish Mouse vs TouchScreen vs Pen.
        // Add support for Win32 (based on imgui_impl_win32), because we rely on _TouchScreen info to trickle inputs differently.
        static ImGuiMouseSource GetMouseSourceFromMessageExtraInfo()
        {
            LPARAM extra_info = ::GetMessageExtraInfo();
            if ((extra_info & 0xFFFFFF80) == 0xFF515700)
                return ImGuiMouseSource_Pen;
            if ((extra_info & 0xFFFFFF80) == 0xFF515780)
                return ImGuiMouseSource_TouchScreen;
            return ImGuiMouseSource_Mouse;
        }
        static LRESULT CALLBACK ImGui_ImplGlfw_WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            switch (msg)
            {
            case WM_MOUSEMOVE: case WM_NCMOUSEMOVE:
            case WM_LBUTTONDOWN: case WM_LBUTTONDBLCLK: case WM_LBUTTONUP:
            case WM_RBUTTONDOWN: case WM_RBUTTONDBLCLK: case WM_RBUTTONUP:
            case WM_MBUTTONDOWN: case WM_MBUTTONDBLCLK: case WM_MBUTTONUP:
            case WM_XBUTTONDOWN: case WM_XBUTTONDBLCLK: case WM_XBUTTONUP:
                ImGui::GetIO().AddMouseSourceEvent(GetMouseSourceFromMessageExtraInfo());
                break;
            }
            return ::CallWindowProc(bd->GlfwWndProc, hWnd, msg, wParam, lParam);
        }
#endif

        static unsafe void ImGui_ImplGlfw_InstallCallbacks(Window window)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            Debug.Assert(bd->InstalledCallbacks == false, "Callbacks already installed!");
            Debug.Assert(bd->Window == window);

            var prevUserCallbackWindowFocus = Glfw.SetWindowFocusCallback(window, (wndp, focusing) => ImGui_ImplGlfw_WindowFocusCallback(*(Window*)&wndp, focusing ? 1 : 0));
            var prevUserCallbackCursorEnter = Glfw.SetCursorEnterCallback(window, (wndp, entering) => ImGui_ImplGlfw_CursorEnterCallback(*(Window*)&wndp, entering ? 1 : 0));
            var prevUserCallbackCursorPos = Glfw.SetCursorPositionCallback(window, (wndp, x, y) => ImGui_ImplGlfw_CursorPosCallback(*(Window*)&wndp, x, y));
            var prevUserCallbackMousebutton = Glfw.SetMouseButtonCallback(window, (wndp, button, state, modifiers) => ImGui_ImplGlfw_MouseButtonCallback(*(Window*)&wndp, (int)button, (int)state, (int)modifiers));
            var prevUserCallbackScroll = Glfw.SetScrollCallback(window, (wndp, x, y) => ImGui_ImplGlfw_ScrollCallback(*(Window*)&wndp, x, y));
            var prevUserCallbackKey = Glfw.SetKeyCallback(window, (wndp, key, scanCode, state, mods) => ImGui_ImplGlfw_KeyCallback(*(Window*)&wndp, (int)key, scanCode, (int)state, (int)mods));
            var prevUserCallbackChar = Glfw.SetCharCallback(window, (wndp, codePoint) => ImGui_ImplGlfw_CharCallback(*(Window*)&wndp, codePoint));
            var prevUserCallbackMonitor = Glfw.SetMonitorCallback((monitor, status) => ImGui_ImplGlfw_MonitorCallback(monitor, (int)status));
            // :puke:
            bd->PrevUserCallbackWindowFocus = prevUserCallbackWindowFocus == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(prevUserCallbackWindowFocus);
            bd->PrevUserCallbackCursorEnter = prevUserCallbackCursorEnter == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(prevUserCallbackCursorEnter);
            bd->PrevUserCallbackCursorPos = prevUserCallbackCursorPos == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(prevUserCallbackCursorPos);
            bd->PrevUserCallbackMousebutton = prevUserCallbackMousebutton == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(prevUserCallbackMousebutton);
            bd->PrevUserCallbackScroll = prevUserCallbackScroll == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(prevUserCallbackScroll);
            bd->PrevUserCallbackKey = prevUserCallbackKey == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(prevUserCallbackKey);
            bd->PrevUserCallbackChar = prevUserCallbackChar == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(prevUserCallbackChar);
            bd->PrevUserCallbackMonitor = prevUserCallbackMonitor == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(prevUserCallbackMonitor);
            bd->InstalledCallbacks = true;
        }

        static unsafe void ImGui_ImplGlfw_RestoreCallbacks(Window window)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            Debug.Assert(bd->InstalledCallbacks == true, "Callbacks not installed!");
            Debug.Assert(bd->Window == window);

            Glfw.SetWindowFocusCallback(window, Marshal.GetDelegateForFunctionPointer<FocusCallback>(bd->PrevUserCallbackWindowFocus));
            Glfw.SetCursorEnterCallback(window, Marshal.GetDelegateForFunctionPointer<MouseEnterCallback>(bd->PrevUserCallbackCursorEnter));
            Glfw.SetCursorPositionCallback(window, Marshal.GetDelegateForFunctionPointer<MouseCallback>(bd->PrevUserCallbackCursorPos));
            Glfw.SetMouseButtonCallback(window, Marshal.GetDelegateForFunctionPointer<MouseButtonCallback>(bd->PrevUserCallbackMousebutton));
            Glfw.SetScrollCallback(window, Marshal.GetDelegateForFunctionPointer<MouseCallback>(bd->PrevUserCallbackScroll));
            Glfw.SetKeyCallback(window, Marshal.GetDelegateForFunctionPointer<KeyCallback>(bd->PrevUserCallbackKey));
            Glfw.SetCharCallback(window, Marshal.GetDelegateForFunctionPointer<CharCallback>(bd->PrevUserCallbackChar));
            Glfw.SetMonitorCallback(Marshal.GetDelegateForFunctionPointer<MonitorCallback>(bd->PrevUserCallbackMonitor));
            bd->InstalledCallbacks = false;
            bd->PrevUserCallbackWindowFocus = IntPtr.Zero;
            bd->PrevUserCallbackCursorEnter = IntPtr.Zero;
            bd->PrevUserCallbackCursorPos = IntPtr.Zero;
            bd->PrevUserCallbackMousebutton = IntPtr.Zero;
            bd->PrevUserCallbackScroll = IntPtr.Zero;
            bd->PrevUserCallbackKey = IntPtr.Zero;
            bd->PrevUserCallbackChar = IntPtr.Zero;
            bd->PrevUserCallbackMonitor = IntPtr.Zero;
        }

        // Set to 'true' to enable chaining installed callbacks for all windows (including secondary viewports created by backends or by user.
        // This is 'false' by default meaning we only chain callbacks for the main viewport.
        // We cannot set this to 'true' by default because user callbacks code may be not testing the 'window' parameter of their callback.
        // If you set this to 'true' your user callback code will need to make sure you are testing the 'window' parameter.
        static unsafe void ImGui_ImplGlfw_SetCallbacksChainForAllWindows(bool chain_for_all_windows)
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            bd->CallbacksChainForAllWindows = chain_for_all_windows;
        }

        static unsafe bool ImGui_ImplGlfw_Init(Window window, bool install_callbacks, GlfwClientApi client_api)
        {
            ImGuiIO* io = igGetIO();
            Debug.Assert(io->BackendPlatformUserData == null, "Already initialized a platform backend!");
            //printf("GLFW_VERSION: %d.%d.%d (%d)", GLFW_VERSION_MAJOR, GLFW_VERSION_MINOR, GLFW_VERSION_REVISION, GLFW_VERSION_COMBINED);

            // Setup backend capabilities flags
            ImGui_ImplGlfw_Data* bd = (ImGui_ImplGlfw_Data*)Impl.New<ImGui_ImplGlfw_Data>();
            io->BackendPlatformUserData = (void*)bd;
            io->BackendPlatformName = (byte*)Marshal.StringToHGlobalAuto("imgui_impl_glfw");
            io->BackendFlags |= ImGuiBackendFlags.HasMouseCursors;         // We can honor GetMouseCursor() values (optional)
            io->BackendFlags |= ImGuiBackendFlags.HasSetMousePos;          // We can honor io.WantSetMousePos requests (optional, rarely used)

            bd->Window = window;
            bd->Time = 0.0;

            // jesus crist, since when why how is this valid c# syntax - yeye since c# 9... :flushed:
            io->SetClipboardTextFn = new IntPtr((delegate*<void*, char*, void>)&ImGui_ImplGlfw_SetClipboardText);
            io->GetClipboardTextFn = new IntPtr((delegate*<void*, char*>)&ImGui_ImplGlfw_GetClipboardText);
            io->ClipboardUserData = (void*)(IntPtr)bd->Window;

            // Create mouse cursors
            // (By design, on X11 cursors are user configurable and some cursors may be missing. When a cursor doesn't exist,
            // GLFW will emit an error which will often be printed by the app, so we temporarily disable error reporting.
            // Missing cursors will return nullptr and our _UpdateMouseCursor() function will use the Arrow cursor instead.)
            ErrorCallback prev_error_callback = Glfw.SetErrorCallback(null);
            Cursor* cursors = &bd->MouseCursors;
            cursors[(int)ImGuiMouseCursor.Arrow] = Glfw.CreateStandardCursor(CursorType.Arrow);
            cursors[(int)ImGuiMouseCursor.TextInput] = Glfw.CreateStandardCursor(CursorType.Beam);
            cursors[(int)ImGuiMouseCursor.ResizeNS] = Glfw.CreateStandardCursor(CursorType.ResizeVertical);
            cursors[(int)ImGuiMouseCursor.ResizeEW] = Glfw.CreateStandardCursor(CursorType.ResizeHorizontal);
            cursors[(int)ImGuiMouseCursor.Hand] = Glfw.CreateStandardCursor(CursorType.Hand);
#if GLFW_HAS_NEW_CURSORS
            bd->MouseCursors[(int)ImGuiMouseCursor_ResizeAll] = glfwCreateStandardCursor(GLFW_RESIZE_ALL_CURSOR);
            bd->MouseCursors[(int)ImGuiMouseCursor_ResizeNESW] = glfwCreateStandardCursor(GLFW_RESIZE_NESW_CURSOR);
            bd->MouseCursors[(int)ImGuiMouseCursor_ResizeNWSE] = glfwCreateStandardCursor(GLFW_RESIZE_NWSE_CURSOR);
            bd->MouseCursors[(int)ImGuiMouseCursor_NotAllowed] = glfwCreateStandardCursor(GLFW_NOT_ALLOWED_CURSOR);
#else
            cursors[(int)ImGuiMouseCursor.ResizeAll] = Glfw.CreateStandardCursor(CursorType.Arrow);
            cursors[(int)ImGuiMouseCursor.ResizeNESW] = Glfw.CreateStandardCursor(CursorType.Arrow);
            cursors[(int)ImGuiMouseCursor.ResizeNWSE] = Glfw.CreateStandardCursor(CursorType.Arrow);
            cursors[(int)ImGuiMouseCursor.NotAllowed] = Glfw.CreateStandardCursor(CursorType.Arrow);
#endif
            Glfw.SetErrorCallback(prev_error_callback);
#if GLFW_HAS_GETERROR && !__EMSCRIPTEN__ // Eat errors (see #5908)
            (void)glfwGetError(nullptr);
#endif

            // Chain GLFW callbacks: our callbacks will call the user's previously installed callbacks, if any.
            if (install_callbacks)
                ImGui_ImplGlfw_InstallCallbacks(window);
            // Register Emscripten Wheel callback to workaround issue in Emscripten GLFW Emulation (#6096)
            // We intentionally do not check 'if (install_callbacks)' here, as some users may set it to false and call GLFW callback themselves.
            // FIXME: May break chaining in case user registered their own Emscripten callback?
#if __EMSCRIPTEN__
            emscripten_set_wheel_callback(EMSCRIPTEN_EVENT_TARGET_DOCUMENT, nullptr, false, ImGui_ImplEmscripten_WheelCallback);
#endif

            // Set platform dependent data in viewport
            ImGuiViewport* main_viewport = igGetMainViewport();
#if _WIN32
            main_viewport->PlatformHandleRaw = glfwGetWin32Window(bd->Window);
#elif __APPLE__
            main_viewport->PlatformHandleRaw = (void*)glfwGetCocoaWindow(bd->Window);
#else
            //IM_UNUSED(main_viewport);
#endif

            // Windows: register a WndProc hook so we can intercept some messages.
#if _WIN32
            bd->GlfwWndProc = (WNDPROC)::GetWindowLongPtr((HWND)main_viewport->PlatformHandleRaw, GWLP_WNDPROC);
            IM_ASSERT(bd->GlfwWndProc != nullptr);
            ::SetWindowLongPtr((HWND)main_viewport->PlatformHandleRaw, GWLP_WNDPROC, (LONG_PTR)ImGui_ImplGlfw_WndProc);
#endif

            bd->ClientApi = client_api;
            return true;
        }

        public static unsafe bool ImGui_ImplGlfw_InitForOpenGL(Window window, bool install_callbacks)
        {
            return ImGui_ImplGlfw_Init(window, install_callbacks, GlfwClientApi.OpenGL);
        }

        static unsafe bool ImGui_ImplGlfw_InitForVulkan(Window window, bool install_callbacks)
        {
            return ImGui_ImplGlfw_Init(window, install_callbacks, GlfwClientApi.Vulkan);
        }

        static unsafe bool ImGui_ImplGlfw_InitForOther(Window window, bool install_callbacks)
        {
            return ImGui_ImplGlfw_Init(window, install_callbacks, GlfwClientApi.Unknown);
        }

        static unsafe void ImGui_ImplGlfw_Shutdown()
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            Debug.Assert(bd != null, "No platform backend to shutdown, or already shutdown?");
            ImGuiIO* io = igGetIO();

            if (bd->InstalledCallbacks)
                ImGui_ImplGlfw_RestoreCallbacks(bd->Window);

            Cursor* cursors = &bd->MouseCursors;
            for (ImGuiMouseCursor cursor_n = 0; cursor_n < ImGuiMouseCursor.COUNT; cursor_n++)
                Glfw.DestroyCursor(cursors[(int)cursor_n]);

            // Windows: register a WndProc hook so we can intercept some messages.
#if _WIN32
            ImGuiViewport* main_viewport = ImGui::GetMainViewport();
            ::SetWindowLongPtr((HWND)main_viewport->PlatformHandleRaw, GWLP_WNDPROC, (LONG_PTR)bd->GlfwWndProc);
            bd->GlfwWndProc = nullptr;
#endif

            io->BackendPlatformName = null;
            io->BackendPlatformUserData = null;
            io->BackendFlags &= ~(ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos | ImGuiBackendFlags.HasGamepad);
            Impl.Delete<ImGui_ImplGlfw_Data>(bd);
        }

        static unsafe void ImGui_ImplGlfw_UpdateMouseData()
        {
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            ImGuiIO* io = igGetIO();

            if (Glfw.GetInputMode(bd->Window, InputMode.Cursor) == (int)CursorMode.Disabled)
            {
                io->AddMousePosEvent(-float.MaxValue, -float.MaxValue);
                return;
            }

            // (those braces are here to reduce diff with multi-viewports support in 'docking' branch)
            {
                Window window = bd->Window;

#if __EMSCRIPTEN__
                const bool is_window_focused = true;
#else
                bool is_window_focused = Glfw.GetWindowAttribute(window, WindowAttribute.Focused);
#endif
                if (is_window_focused)
                {
                    // (Optional) Set OS mouse position from Dear ImGui if requested (rarely used, only when ImGuiConfigFlags_NavEnableSetMousePos is enabled by user)
                    if (io->WantSetMousePos != 0)
                        Glfw.SetCursorPosition(window, (double)io->MousePos.X, (double)io->MousePos.Y);

                    // (Optional) Fallback to provide mouse position when focused (ImGui_ImplGlfw_CursorPosCallback already provides this when hovered or captured)
                    if (bd->MouseWindow == null)
                    {
                        double mouse_x, mouse_y;
                        Glfw.GetCursorPosition(window, out mouse_x, out mouse_y);
                        bd->LastValidMousePos = new() { x = (float)mouse_x, y = (float)mouse_y };
                        io->AddMousePosEvent((float)mouse_x, (float)mouse_y);
                    }
                }
            }
        }

        static unsafe void ImGui_ImplGlfw_UpdateMouseCursor()
        {
            ImGuiIO* io = igGetIO();
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            if ((io->ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0 || Glfw.GetInputMode(bd->Window, InputMode.Cursor) == (int)CursorMode.Disabled)
                return;

            ImGuiMouseCursor imgui_cursor = igGetMouseCursor();
            // (those braces are here to reduce diff with multi-viewports support in 'docking' branch)
            {
                Window window = bd->Window;
                if (imgui_cursor == ImGuiMouseCursor.None || io->MouseDrawCursor != 0)
                {
                    // Hide OS mouse cursor if imgui is drawing it or if it wants no cursor
                    Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Hidden);
                }
                else
                {
                    // Show OS mouse cursor
                    // FIXME-PLATFORM: Unfocused windows seems to fail changing the mouse cursor with GLFW 3.2, but 3.3 works here.
                    Cursor* cursors = &bd->MouseCursors;
                    Glfw.SetCursor(window, (cursors[(int)imgui_cursor] != Cursor.None) ? cursors[(int)imgui_cursor] : cursors[(int)ImGuiMouseCursor.Arrow]);
                    Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Normal);
                }
            }
        }

        // Update gamepad inputs
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Saturate(float v) { return v < 0.0f ? 0.0f : v  > 1.0f ? 1.0f : v; }
        static unsafe void ImGui_ImplGlfw_UpdateGamepads()
        {
            ImGuiIO* io = igGetIO();
            if ((io->ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) == 0) // FIXME: Technically feeding gamepad shouldn't depend on this now that they are regular inputs.
                return;

            throw new NotImplementedException(); // laziness

            /*
            io->BackendFlags &= ~ImGuiBackendFlags.HasGamepad;
#if GLFW_HAS_GAMEPAD_API && !__EMSCRIPTEN__
            GLFWgamepadstate gamepad;
            if (!glfwGetGamepadState(GLFW_JOYSTICK_1, &gamepad))
                return;
            #define MAP_BUTTON(KEY_NO, BUTTON_NO, _UNUSED)          do { io.AddKeyEvent(KEY_NO, gamepad.buttons[BUTTON_NO] != 0); } while (0)
            #define MAP_ANALOG(KEY_NO, AXIS_NO, _UNUSED, V0, V1)    do { float v = gamepad.axes[AXIS_NO]; v = (v - V0) / (V1 - V0); io.AddKeyAnalogEvent(KEY_NO, v > 0.10f, Saturate(v)); } while (0)
#else
            int axes_count = 0, buttons_count = 0;
            const float* axes = glfwGetJoystickAxes(GLFW_JOYSTICK_1, &axes_count);
            const unsigned char* buttons = glfwGetJoystickButtons(GLFW_JOYSTICK_1, &buttons_count);
            if (axes_count == 0 || buttons_count == 0)
                return;
            #define MAP_BUTTON(KEY_NO, _UNUSED, BUTTON_NO)          do { io.AddKeyEvent(KEY_NO, (buttons_count > BUTTON_NO && buttons[BUTTON_NO] == GLFW_PRESS)); } while (0)
            #define MAP_ANALOG(KEY_NO, _UNUSED, AXIS_NO, V0, V1)    do { float v = (axes_count > AXIS_NO) ? axes[AXIS_NO] : V0; v = (v - V0) / (V1 - V0); io.AddKeyAnalogEvent(KEY_NO, v > 0.10f, Saturate(v)); } while (0)
#endif
            io.BackendFlags |= ImGuiBackendFlags_HasGamepad;
            MAP_BUTTON(ImGuiKey_GamepadStart,       GLFW_GAMEPAD_BUTTON_START,          7);
            MAP_BUTTON(ImGuiKey_GamepadBack,        GLFW_GAMEPAD_BUTTON_BACK,           6);
            MAP_BUTTON(ImGuiKey_GamepadFaceLeft,    GLFW_GAMEPAD_BUTTON_X,              2);     // Xbox X, PS Square
            MAP_BUTTON(ImGuiKey_GamepadFaceRight,   GLFW_GAMEPAD_BUTTON_B,              1);     // Xbox B, PS Circle
            MAP_BUTTON(ImGuiKey_GamepadFaceUp,      GLFW_GAMEPAD_BUTTON_Y,              3);     // Xbox Y, PS Triangle
            MAP_BUTTON(ImGuiKey_GamepadFaceDown,    GLFW_GAMEPAD_BUTTON_A,              0);     // Xbox A, PS Cross
            MAP_BUTTON(ImGuiKey_GamepadDpadLeft,    GLFW_GAMEPAD_BUTTON_DPAD_LEFT,      13);
            MAP_BUTTON(ImGuiKey_GamepadDpadRight,   GLFW_GAMEPAD_BUTTON_DPAD_RIGHT,     11);
            MAP_BUTTON(ImGuiKey_GamepadDpadUp,      GLFW_GAMEPAD_BUTTON_DPAD_UP,        10);
            MAP_BUTTON(ImGuiKey_GamepadDpadDown,    GLFW_GAMEPAD_BUTTON_DPAD_DOWN,      12);
            MAP_BUTTON(ImGuiKey_GamepadL1,          GLFW_GAMEPAD_BUTTON_LEFT_BUMPER,    4);
            MAP_BUTTON(ImGuiKey_GamepadR1,          GLFW_GAMEPAD_BUTTON_RIGHT_BUMPER,   5);
            MAP_ANALOG(ImGuiKey_GamepadL2,          GLFW_GAMEPAD_AXIS_LEFT_TRIGGER,     4,      -0.75f,  +1.0f);
            MAP_ANALOG(ImGuiKey_GamepadR2,          GLFW_GAMEPAD_AXIS_RIGHT_TRIGGER,    5,      -0.75f,  +1.0f);
            MAP_BUTTON(ImGuiKey_GamepadL3,          GLFW_GAMEPAD_BUTTON_LEFT_THUMB,     8);
            MAP_BUTTON(ImGuiKey_GamepadR3,          GLFW_GAMEPAD_BUTTON_RIGHT_THUMB,    9);
            MAP_ANALOG(ImGuiKey_GamepadLStickLeft,  GLFW_GAMEPAD_AXIS_LEFT_X,           0,      -0.25f,  -1.0f);
            MAP_ANALOG(ImGuiKey_GamepadLStickRight, GLFW_GAMEPAD_AXIS_LEFT_X,           0,      +0.25f,  +1.0f);
            MAP_ANALOG(ImGuiKey_GamepadLStickUp,    GLFW_GAMEPAD_AXIS_LEFT_Y,           1,      -0.25f,  -1.0f);
            MAP_ANALOG(ImGuiKey_GamepadLStickDown,  GLFW_GAMEPAD_AXIS_LEFT_Y,           1,      +0.25f,  +1.0f);
            MAP_ANALOG(ImGuiKey_GamepadRStickLeft,  GLFW_GAMEPAD_AXIS_RIGHT_X,          2,      -0.25f,  -1.0f);
            MAP_ANALOG(ImGuiKey_GamepadRStickRight, GLFW_GAMEPAD_AXIS_RIGHT_X,          2,      +0.25f,  +1.0f);
            MAP_ANALOG(ImGuiKey_GamepadRStickUp,    GLFW_GAMEPAD_AXIS_RIGHT_Y,          3,      -0.25f,  -1.0f);
            MAP_ANALOG(ImGuiKey_GamepadRStickDown,  GLFW_GAMEPAD_AXIS_RIGHT_Y,          3,      +0.25f,  +1.0f);
            #undef MAP_BUTTON
            #undef MAP_ANALOG
            */
        }

        public static unsafe void ImGui_ImplGlfw_NewFrame()
        {
            ImGuiIO* io = igGetIO();
            ImGui_ImplGlfw_Data* bd = ImGui_ImplGlfw_GetBackendData();
            Debug.Assert(bd != null, "Did you call ImGui_ImplGlfw_InitForXXX()?");

            // Setup display size (every frame to accommodate for window resizing)
            int w, h;
            int display_w, display_h;
            Glfw.GetWindowSize(bd->Window, out w, out h);
            Glfw.GetFramebufferSize(bd->Window, out display_w, out display_h);
            io->DisplaySize = new Evergine.Mathematics.Vector2((float)w, (float)h);
            if (w > 0 && h > 0)
                io->DisplayFramebufferScale = new Evergine.Mathematics.Vector2((float)display_w / (float)w, (float)display_h / (float)h);

            // Setup time step
            double current_time = Glfw.Time;
            io->DeltaTime = bd->Time > 0.0 ? (float)(current_time - bd->Time) : (float)(1.0f / 60.0f);
            bd->Time = current_time;

            ImGui_ImplGlfw_UpdateMouseData();
            ImGui_ImplGlfw_UpdateMouseCursor();

            // Update game controllers (if enabled and available)
            ImGui_ImplGlfw_UpdateGamepads();
        }
    }
}

//#if defined(__clang__)
//#pragma clang diagnostic pop
//#endif