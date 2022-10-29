using ImGuiNET;
using System;

using System.Numerics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;

using CrossEngine.Utils;
using CrossEngineEditor.Utils.Gui;

namespace CrossEngineEditor.Utils.Gui
{
    class ImGradient
    {
        private enum MarkerDirection
        {
            Unknown,
            ToLower,
            ToUpper,
        }

        private static Vector4 GetVector4(object val)
        {
            if (val is Vector4)
                return (Vector4)val;
            if (val is Vector3)
            {
                var cval = (Vector3)val;
                return new Vector4(cval, 1);
            }
            if (val is Vector2)
            {
                var cval = (Vector2)val;
                return new Vector4(cval, 0, 1);
            }
            if (val is float)
            {
                var cval = (float)val;
                return new Vector4(cval, cval, cval, 1);
            }

            Debug.Assert(false);
            return default;
        }

        private static void EditColor(IGradient gradient, int index)
        {
            if (gradient is Gradient<Vector4>)
            {
                var markerCol = ((Gradient<Vector4>)gradient).Elements[index].value;
                if (ImGui.ColorEdit4("value", ref markerCol))
                    gradient.SetElementValue(index, markerCol);
            }
            if (gradient is Gradient<Vector3>)
            {
                var markerCol = ((Gradient<Vector3>)gradient).Elements[index].value;
                if (ImGui.ColorEdit3("value", ref markerCol))
                    gradient.SetElementValue(index, markerCol);
            }
            if (gradient is Gradient<Vector2>)
            {
                var markerCol = ((Gradient<Vector2>)gradient).Elements[index].value;
                if (ImGui.DragFloat2("value", ref markerCol, 0.01f))
                    gradient.SetElementValue(index, markerCol);
            }
            if (gradient is Gradient<float>)
            {
                var markerCol = ((Gradient<float>)gradient).Elements[index].value;
                if (ImGui.DragFloat("value", ref markerCol, 0.01f))
                    gradient.SetElementValue(index, markerCol);
            }
        }

        private static unsafe void DrawMarker(Vector2 pmin, Vector2 pmax, uint color, bool isSelected)
        {
            var drawList = ImGui.GetWindowDrawList();
            var w = pmax.X - pmin.X;
            var h = pmax.Y - pmin.Y;
            var sign = Math.Sign(h);

            var margin = 2;
            var marginh = margin * sign;
            var selectedCol = *ImGui.GetStyleColorVec4(ImGuiCol.TextSelectedBg);
            selectedCol.W = 1;
            var outlineColor = isSelected ?
                ImGui.ColorConvertFloat4ToU32(selectedCol) :
                0xff323232;

            drawList.AddTriangleFilled(
                new Vector2(pmin.X + w / 2, pmin.Y),
                new Vector2(pmin.X + 0, pmin.Y + h / 2),
                new Vector2(pmin.X + w, pmin.Y + h / 2),
                outlineColor);

            drawList.AddRectFilled(
                new Vector2(pmin.X + 0, pmin.Y + h / 2),
                new Vector2(pmin.X + w, pmin.Y + h),
                outlineColor);

            drawList.AddTriangleFilled(
                new Vector2(pmin.X + w / 2, pmin.Y + marginh),
                new Vector2(pmin.X + 0 + margin, pmin.Y + h / 2),
                new Vector2(pmin.X + w - margin, pmin.Y + h / 2),
                color);

            drawList.AddRectFilled(
                new Vector2(pmin.X + 0 + margin, pmin.Y + h / 2 - sign),
                new Vector2(pmin.X + w - margin, pmin.Y + h - marginh),
                color);
        }

        private static bool UpdateMarkers<T>(
            Gradient<T> markerArray,
            string keyStr,
            Vector2 originPos,
            float width,
            float markerWidth,
            float markerHeight,
            MarkerDirection markerDir,
            ref int selectedIndex,
            ref int draggingIndex,
            out bool markerInteracted) where T : struct
        {
            markerInteracted = false;

            bool changed = false;

	        for (int i = 0; i < markerArray.Elements.Count ; i++)
	        {
		        var x = (int)(markerArray.Elements[i].position * width);
		        ImGui.SetCursorScreenPos(new(originPos.X + x - 5, originPos.Y));

                var markerCol = GetVector4(markerArray.Elements[i].value);
                markerCol.W = 1;
                if (markerDir == MarkerDirection.ToLower)
                {
                    DrawMarker(
                        new(originPos.X + x - 5, originPos.Y + markerHeight),
                        new(originPos.X + x + 5, originPos.Y + 0),
                        ImGui.ColorConvertFloat4ToU32(markerCol),
                        selectedIndex == i);
                }
                else if (markerDir == MarkerDirection.ToUpper)
                {
                    DrawMarker(
                        new(originPos.X + x - 5, originPos.Y + 0),
                        new(originPos.X + x + 5, originPos.Y + markerHeight),
                        ImGui.ColorConvertFloat4ToU32(markerCol),
                        selectedIndex == i);
                }
                else Debug.Assert(false);

		        ImGui.InvisibleButton(keyStr + i.ToString(), new(markerWidth, markerHeight));

		        if (draggingIndex == -1 && ImGui.IsItemHovered() && ImGui.IsMouseDown(0))
		        {
                    selectedIndex = i;
                    draggingIndex = i;
		        }

		        if (!ImGui.IsMouseDown(0))
		        {
                    draggingIndex = -1;
		        }

                if (draggingIndex == i && ImGui.IsMouseDragging(0))
                {
                    var diff = ImGui.GetIO().MouseDelta.X / width;
                    if (diff != 0.0f)
                    {
                        int ind = markerArray.SetElementPosition(i, Math.Max(Math.Min(markerArray.Elements[i].position + diff, 1.0f), 0.0f));

                        selectedIndex = ind;
                        draggingIndex = ind;

                        changed |= true;
                    }
                    markerInteracted = true; ;
		        }
	        }

            return changed;
        }

        public static bool Manipulate<T>(Gradient<T> state) where T : struct
        {
            bool changed = false;

            ImGui.PushID(state.GetHashCode());

            ImGuiUtils.BeginGroupFrame();

            var originPos = ImGui.GetCursorScreenPos();

            var drawList = ImGui.GetWindowDrawList();

            var margin = 5;

            var width = ImGui.GetContentRegionAvail().X - margin * 2;
            var barHeight = 20;
            var markerWidth = 10;
            var markerHeight = 15;

            //changed |= UpdateMarker(state, "a", originPos, width, markerWidth, markerHeight, MarkerDirection.ToLower);
            //
            //ImGui.SetCursorScreenPos(originPos);
            //
            //ImGui.InvisibleButton("AlphaArea", new(width, markerHeight));
            //
            //if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(0))
            //{
            //    float x = (ImGui.GetIO().MousePos.X - originPos.X) / width;
            //    var alpha = state.Sample(x);
            //    /*changed |= */state.AddElement(x, alpha);
            //}

            originPos = ImGui.GetCursorScreenPos();

            ImGui.InvisibleButton("BarArea", new(width, barHeight));

            int gridSize = 10;

            drawList.AddRectFilled(new Vector2(originPos.X - 2, originPos.Y - 2),
                                   new Vector2(originPos.X + width + 2, originPos.Y + barHeight + 2),
                                   0xff646464);

            for (int y = 0; y * gridSize < barHeight; y += 1)
            {
                for (int x = 0; x * gridSize < width; x += 1)
                {
                    int wgrid = Math.Min(gridSize, ((int)width) - x * gridSize);
                    int hgrid = Math.Min(gridSize, barHeight - y * gridSize);
                    uint color = 0xff646464;

                    if ((x + y) % 2 == 0)
                    {
                        color = 0xff323232;
                    }

                    drawList.AddRectFilled(new Vector2(originPos.X + x * gridSize, originPos.Y + y * gridSize),
                                           new Vector2(originPos.X + x * gridSize + wgrid, originPos.Y + y * gridSize + hgrid),
                                           color);
                }
            }

            {
                List<float> xkeys = new List<float>(state.Elements.Select(el => el.position).Where(v => v >= 0 && v <= 1));

                for (int i = 0; i < state.Elements.Count; i++)
                {
                    xkeys.Add(state.Elements[i].position);
                }

                if (!xkeys.Contains(0.0f)) xkeys.Add(0.0f);
                if (!xkeys.Contains(1.0f)) xkeys.Add(1.0f);
                
                xkeys.Sort();

                for (int i = 0; i < xkeys.Count - 1; i++)
                {
                    var c1 = GetVector4(state.Sample(xkeys[i]));
                    var c2 = GetVector4(state.Sample(xkeys[i + 1]));

                    var colorAU32 = ImGui.ColorConvertFloat4ToU32(c1);
                    var colorBU32 = ImGui.ColorConvertFloat4ToU32(c2);

                    drawList.AddRectFilledMultiColor(new Vector2(originPos.X + xkeys[i] * width, originPos.Y),
                                                     new Vector2(originPos.X + xkeys[i + 1] * width, originPos.Y + barHeight),
                                                     colorAU32,
                                                     colorBU32,
                                                     colorBU32,
                                                     colorAU32);
                }
            }

            originPos = ImGui.GetCursorScreenPos();

            // get state
            var stateStorage = ImGui.GetStateStorage();
            int selectedIndex = stateStorage.GetInt(ImGui.GetID("gradientState.selectedIndex"));
            int draggingIndex = stateStorage.GetInt(ImGui.GetID("gradientState.draggingIndex"));

            changed |= UpdateMarkers(state,
                                     "marker",
                                     originPos,
                                     width,
                                     markerWidth,
                                     markerHeight,
                                     MarkerDirection.ToUpper,
                                     ref selectedIndex,
                                     ref draggingIndex,
                                     out var markerInteracted
                                     );

            ImGui.SetCursorScreenPos(originPos);

            ImGui.InvisibleButton("ColorArea", new(width, markerHeight));

            if (!markerInteracted && ImGui.IsItemHovered() && ImGui.IsMouseClicked(0))
            {
                float x = (ImGui.GetIO().MousePos.X - originPos.X) / width;
                var c = state.Sample(x);
                /*changed |= */state.AddElement(x, c);
            }

            var availWidth = ImGui.GetContentRegionAvail().X;
            ImGui.SetNextItemWidth(availWidth / 6);
            ImGui.DragInt("##selected_index", ref selectedIndex, 0.1f, 0, state.ElementCount - 1);
            ImGui.SameLine();
            ImGui.Text($"({state.ElementCount})");
            ImGui.SameLine();
            if (ImGui.ArrowButton("##add_index", ImGuiDir.Left)) selectedIndex--;
            ImGui.SameLine();
            if (ImGui.ArrowButton("##dec_index", ImGuiDir.Right)) selectedIndex++;

            // hot fix
            selectedIndex = Math.Clamp(selectedIndex, 0, Math.Max(state.ElementCount - 1, 0));

            originPos = ImGui.GetCursorPos();

            var style = ImGui.GetStyle();
            var font = ImGui.GetFont();
            ImGui.SetCursorPosX(availWidth - (style.FramePadding.X * 2 + font.FontSize * font.Scale));
            ImGui.SameLine();
            if (ImGuiUtils.SquareButton("+"))
            {
                // addition behaviour
                if (state.ElementCount == 0)
                {
                    selectedIndex = state.AddElement(0, default);
                }
                else if (state.ElementCount == 1)
                {
                    selectedIndex = state.AddElement(0.5f, state.Elements[0].value);
                }
                else if (selectedIndex == 0)
                {
                    float x = (state.Elements[selectedIndex].position + state.Elements[selectedIndex + 1].position) / 2;
                    var c = state.Sample(x);
                    selectedIndex = state.AddElement(x, c);
                }
                else
                {
                    float x = (state.Elements[selectedIndex].position + state.Elements[selectedIndex - 1].position) / 2;
                    var c = state.Sample(x);
                    selectedIndex = state.AddElement(x, c);
                }
            }
            ImGui.SameLine();
            if (ImGuiUtils.SquareButton("-") && state.ElementCount > 1)
            {
                state.RemoveElement(selectedIndex);
                if (state.ElementCount > 0)
                    selectedIndex = Math.Clamp(selectedIndex, 0, state.ElementCount - 1);
            }
            
            ImGui.SetCursorPos(originPos);
            
            if (state.ElementCount > 0)
            {
                float pos = state.Elements[selectedIndex].position;
                EditColor(state, selectedIndex);
                if (ImGui.DragFloat("pos", ref pos, 0.01f, 0, 1))
                    selectedIndex = state.SetElementPosition(selectedIndex, pos);
            }

            // set state
            stateStorage.SetInt(ImGui.GetID("gradientState.selectedIndex"), Math.Clamp(selectedIndex, 0, Math.Max(state.ElementCount - 1, 0)));
            stateStorage.SetInt(ImGui.GetID("gradientState.draggingIndex"), Math.Clamp(draggingIndex, -1, state.ElementCount - 1));

            ImGuiUtils.EndGroupFrame();

            ImGui.PopID();

            return changed;
        }
    }
}