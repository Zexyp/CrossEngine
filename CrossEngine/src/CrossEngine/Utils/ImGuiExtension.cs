using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Utils
{
    public static class ImGuiExtension
    {
        // fixes broken feature
        // issue is how the wrapper is generated
        public static unsafe bool BeginNullableOpen(string name, ref bool? p_open, ImGuiWindowFlags flags)
        {
            byte* native_name;
            int name_byteCount = 0;
            if (name != null)
            {
                name_byteCount = Encoding.UTF8.GetByteCount(name);
                if (name_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_name = Util.Allocate(name_byteCount + 1);
                }
                else
                {
                    byte* native_name_stackBytes = stackalloc byte[name_byteCount + 1];
                    native_name = native_name_stackBytes;
                }
                int native_name_offset = Util.GetUtf8(name, native_name, name_byteCount);
                native_name[native_name_offset] = 0;
            }
            else { native_name = null; }

            byte native_p_open_val;
            byte* native_p_open;
            if (p_open != null)
            {
                native_p_open_val = (bool)p_open ? (byte)1 : (byte)0;
                native_p_open = &native_p_open_val;
            }
            else
            {
                native_p_open_val = 0;
                native_p_open = null;
            }

            byte ret = ImGuiNative.igBegin(native_name, native_p_open, flags);
            if (name_byteCount > Util.StackAllocationSizeLimit)
            {
                Util.Free(native_name);
            }

            if (p_open != null)
                p_open = native_p_open_val != 0;

            return ret != 0;
        }

        public static unsafe bool BeginPopupModalNullableOpen(string name, ref bool? p_open, ImGuiWindowFlags flags)
        {
            byte* native_name;
            int name_byteCount = 0;
            if (name != null)
            {
                name_byteCount = Encoding.UTF8.GetByteCount(name);
                if (name_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_name = Util.Allocate(name_byteCount + 1);
                }
                else
                {
                    byte* native_name_stackBytes = stackalloc byte[name_byteCount + 1];
                    native_name = native_name_stackBytes;
                }
                int native_name_offset = Util.GetUtf8(name, native_name, name_byteCount);
                native_name[native_name_offset] = 0;
            }
            else { native_name = null; }

            byte native_p_open_val;
            byte* native_p_open;
            if (p_open != null)
            {
                native_p_open_val = (bool)p_open ? (byte)1 : (byte)0;
                native_p_open = &native_p_open_val;
            }
            else
            {
                native_p_open_val = 0;
                native_p_open = null;
            }

            byte ret = ImGuiNative.igBeginPopupModal(native_name, native_p_open, flags);
            if (name_byteCount > Util.StackAllocationSizeLimit)
            {
                Util.Free(native_name);
            }

            if (p_open != null)
                p_open = native_p_open_val != 0;

            return ret != 0;
        }
        public static unsafe bool BeginPopupModalNullableOpen(string name, bool* p_open, ImGuiWindowFlags flags)
        {
            byte* native_name;
            int name_byteCount = 0;
            if (name != null)
            {
                name_byteCount = Encoding.UTF8.GetByteCount(name);
                if (name_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_name = Util.Allocate(name_byteCount + 1);
                }
                else
                {
                    byte* native_name_stackBytes = stackalloc byte[name_byteCount + 1];
                    native_name = native_name_stackBytes;
                }
                int native_name_offset = Util.GetUtf8(name, native_name, name_byteCount);
                native_name[native_name_offset] = 0;
            }
            else { native_name = null; }

            byte ret = ImGuiNative.igBeginPopupModal(native_name, (byte*)p_open, flags);
            if (name_byteCount > Util.StackAllocationSizeLimit)
            {
                Util.Free(native_name);
            }

            return ret != 0;
        }

        public static unsafe bool BeginTabItemNullableOpen(string label, ref bool? p_open, ImGuiTabItemFlags flags)
        {
            byte* native_label;
            int label_byteCount = 0;
            if (label != null)
            {
                label_byteCount = Encoding.UTF8.GetByteCount(label);
                if (label_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_label = Util.Allocate(label_byteCount + 1);
                }
                else
                {
                    byte* native_label_stackBytes = stackalloc byte[label_byteCount + 1];
                    native_label = native_label_stackBytes;
                }
                int native_label_offset = Util.GetUtf8(label, native_label, label_byteCount);
                native_label[native_label_offset] = 0;
            }
            else { native_label = null; }

            byte native_p_open_val;
            byte* native_p_open;
            if (p_open != null)
            {
                native_p_open_val = (bool)p_open ? (byte)1 : (byte)0;
                native_p_open = &native_p_open_val;
            }
            else
            {
                native_p_open_val = 0;
                native_p_open = null;
            }

            byte ret = ImGuiNative.igBeginTabItem(native_label, native_p_open, flags);
            if (label_byteCount > Util.StackAllocationSizeLimit)
            {
                Util.Free(native_label);
            }

            if (p_open != null)
                p_open = native_p_open_val != 0;
            
            return ret != 0;
        }
        public static unsafe bool BeginTabItemNullableOpen(string label, bool* p_open, ImGuiTabItemFlags flags)
        {
            byte* native_label;
            int label_byteCount = 0;
            if (label != null)
            {
                label_byteCount = Encoding.UTF8.GetByteCount(label);
                if (label_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_label = Util.Allocate(label_byteCount + 1);
                }
                else
                {
                    byte* native_label_stackBytes = stackalloc byte[label_byteCount + 1];
                    native_label = native_label_stackBytes;
                }
                int native_label_offset = Util.GetUtf8(label, native_label, label_byteCount);
                native_label[native_label_offset] = 0;
            }
            else { native_label = null; }

            byte ret = ImGuiNative.igBeginTabItem(native_label, (byte*)p_open, flags);
            if (label_byteCount > Util.StackAllocationSizeLimit)
            {
                Util.Free(native_label);
            }

            return ret != 0;
        }
    }
}
