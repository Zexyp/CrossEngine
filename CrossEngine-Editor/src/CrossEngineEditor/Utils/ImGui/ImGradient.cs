using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CrossEngineEditor.Utils.Gui
{
    class ImGradient
    {
        public static void DrawMarker(Vector2 pmin, Vector2 pmax, uint color, bool isSelected)
        {
            var drawList = ImGui.GetWindowDrawList();
            var w = pmax.X - pmin.X;
            var h = pmax.Y - pmin.Y;
            var sign = Math.Sign(h);

            var margin = 2;
            var marginh = margin * sign;
            var outlineColor = isSelected ?
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 1.0f, 1.0f)) :
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));

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
    }
}
        /*
        enum MarkerDirection
        {
            ToUpper,
            ToLower,
        }

        bool ImGradient(int gradientID, ref ImGradientHDRState state, ref ImGradientHDRTemporaryState temporaryState)
        {
            bool changed = false;

            ImGui.PushID(gradientID);

            var originPos = ImGui.GetCursorScreenPos();

            var drawList = ImGui.GetWindowDrawList();

            var margin = 5;

            var width = ImGui.GetContentRegionAvail().x - margin * 2;
            var barHeight = 20;
            var markerWidth = 10;
            var markerHeight = 15;

            changed |= UpdateMarker(state.Alphas, state.AlphaCount, temporaryState, ImGradientHDRMarkerType.Alpha, "a", originPos, width, markerWidth, markerHeight, MarkerDirection.ToLower);

            if (temporaryState.draggingMarkerType == ImGradientHDRMarkerType::Alpha)
            {
                SortMarkers(state.Alphas, state.AlphaCount, temporaryState.selectedIndex, temporaryState.draggingIndex);
            }

            ImGui.SetCursorScreenPos(originPos);

            ImGui.InvisibleButton("AlphaArea", {width, static_cast<float>(markerHeight)});

            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(0))
            {
                float x = (ImGui.GetIO().MousePos.X - originPos.X) / width;
                var alpha = state.GetAlpha(x);
                changed |= state.AddAlphaMarker(x, alpha);
            }

            originPos = ImGui.GetCursorScreenPos();

            ImGui.InvisibleButton("BarArea", new (width, barHeight));

            int gridSize = 10;

            drawList.AddRectFilled(new Vector2(originPos.X - 2, originPos.Y - 2),
                                   new Vector2(originPos.X + width + 2, originPos.Y + barHeight + 2),
                                   0x646464ff));

            for (int y = 0; y * gridSize < barHeight; y += 1)
            {
                for (int x = 0; x * gridSize < width; x += 1)
                {
                    int wgrid = Math.Min(gridSize, (int)(width) - x * gridSize);
                    int hgrid = Math.Min(gridSize, barHeight - y * gridSize);
                    uint color = 0x646464ff;

                    if ((x + y) % 2 == 0)
                    {
                        color = 0x646464ff;
                    }

                    drawList.AddRectFilled(new Vector2(originPos.X + x * gridSize, originPos.Y + y * gridSize),
                                           new Vector2(originPos.X + x * gridSize + wgrid, originPos.Y + y * gridSize + hgrid),
                                           color);
                }
            }

            {
                List<float> xkeys = new List<float>(16);

                for (int i = 0; i < state.ColorCount; i++)
                {
                    xkeys.Add(state.Colors[i].Position);
                }

                for (int i = 0; i < state.AlphaCount; i++)
                {
                    xkeys.Add(state.Alphas[i].Position);
                }

                xkeys.Add(0.0f);
                xkeys.Add(1.0f);

                auto result = std::unique(xkeys.begin(), xkeys.end());
                xkeys.erase(result, xkeys.end());

                xkeys.Sort();

                for (int i = 0; i < xkeys.Count - 1; i++)
                {
                    var c1 = state.GetCombinedColor(xkeys[i]);
                    var c2 = state.GetCombinedColor(xkeys[i + 1]);
                    
                    var colorAU32 = ImGui.ColorConvertFloat4ToU32(new Vector4(c1[0], c1[1], c1[2], c1[3]));
                    var colorBU32 = ImGui.ColorConvertFloat4ToU32(new Vector4(c2[0], c2[1], c2[2], c2[3]));

                    drawList.AddRectFilledMultiColor(new Vector2(originPos.X + xkeys[i] * width, originPos.y),
                                                     new Vector2(originPos.X + xkeys[i + 1] * width, originPos.y + barHeight),
                                                     colorAU32,
                                                     colorBU32,
                                                     colorBU32,
                                                     colorAU32);
                }
            }

            originPos = ImGui.GetCursorScreenPos();

            changed |= UpdateMarker(state.Colors, state.ColorCount, temporaryState, ImGradientHDRMarkerType.Color, "c", originPos, width, markerWidth, markerHeight, MarkerDirection.ToUpper);

            if (temporaryState.draggingMarkerType == ImGradientHDRMarkerType.Color)
            {
                SortMarkers(state.Colors, state.ColorCount, temporaryState.selectedIndex, temporaryState.draggingIndex);
            }

            ImGui.SetCursorScreenPos(originPos);

            ImGui.InvisibleButton("ColorArea", new (width, markerHeight));

            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(0))
            {
                float x = (ImGui.GetIO().MousePos.x - originPos.x) / width;
                var c = state.GetColorAndIntensity(x);
                changed |= state.AddColorMarker(x, {c[0], c[1], c[2]}, c[3]);
            }

            ImGui.PopID();

            return changed;
        }

std::array<float, 4> ImGradientHDRState::GetColorAndIntensity(float x) const
{
    if (ColorCount == 0)
    {
        return std::array<float, 4>{1.0f, 1.0f, 1.0f, 1.0f};
    }

    if (x < Colors[0].Position)
    {
        const auto c = Colors[0].Color;
        return {c[0], c[1], c[2], Colors[0].Intensity};
    }

    if (Colors[ColorCount - 1].Position <= x)
    {
        const auto c = Colors[ColorCount - 1].Color;
        return {c[0], c[1], c[2], Colors[ColorCount - 1].Intensity};
    }

    auto key = ColorMarker();
    key.Position = x;

    auto it = std::lower_bound(Colors.begin(), Colors.begin() + ColorCount, key, [](const ColorMarker& a, const ColorMarker& b)
                               { return a.Position < b.Position; });
    auto ind = static_cast<int32_t>(std::distance(Colors.begin(), it));

    {
        if (Colors[ind].Position != x)
        {
            ind--;
        }

        if (Colors[ind].Position <= x && x <= Colors[ind + 1].Position)
        {
            const auto area = Colors[ind + 1].Position - Colors[ind].Position;
            if (area == 0)
            {
                return std::array<float, 4>{Colors[ind].Color[0], Colors[ind].Color[1], Colors[ind].Color[2], Colors[ind].Intensity};
            }

            const auto alpha = (x - Colors[ind].Position) / area;
            const auto r = Colors[ind + 1].Color[0] * alpha + Colors[ind].Color[0] * (1.0f - alpha);
            const auto g = Colors[ind + 1].Color[1] * alpha + Colors[ind].Color[1] * (1.0f - alpha);
            const auto b = Colors[ind + 1].Color[2] * alpha + Colors[ind].Color[2] * (1.0f - alpha);
            const auto intensity = Colors[ind + 1].Intensity * alpha + Colors[ind].Intensity * (1.0f - alpha);
            return std::array<float, 4>{r, g, b, intensity};
        }
        else
        {
            assert(0);
        }
    }

    return std::array<float, 4>{1.0f, 1.0f, 1.0f, 1.0f};
}

float ImGradientHDRState::GetAlpha(float x) const
{
    if (AlphaCount == 0)
    {
        return 1.0f;
    }

    if (x < Alphas[0].Position)
    {
        return Alphas[0].Alpha;
    }

    if (Alphas[AlphaCount - 1].Position <= x)
    {
        return Alphas[AlphaCount - 1].Alpha;
    }

    auto key = AlphaMarker();
    key.Position = x;

    auto it = std::lower_bound(Alphas.begin(), Alphas.begin() + AlphaCount, key, [](const AlphaMarker& a, const AlphaMarker& b)
                               { return a.Position < b.Position; });
    auto ind = static_cast<int32_t>(std::distance(Alphas.begin(), it));

    {
        if (Alphas[ind].Position != x)
        {
            ind--;
        }

        if (Alphas[ind].Position <= x && x <= Alphas[ind + 1].Position)
        {
            const auto area = Alphas[ind + 1].Position - Alphas[ind].Position;
            if (area == 0)
            {
                return Alphas[ind].Alpha;
            }

            const auto alpha = (x - Alphas[ind].Position) / area;
            return Alphas[ind + 1].Alpha * alpha + Alphas[ind].Alpha * (1.0f - alpha);
        }
        else
        {
            assert(0);
        }
    }

    return 1.0f;
}


}

struct ImGradientHDRState
    {
        struct ColorMarker
        {
            float Position;
            Vector3 Color;
            float Intensity;
        };

        struct AlphaMarker
        {
            float Position;
            float Alpha;
        };

        int ColorCount;
        int AlphaCount;
        List<ColorMarker> Colors;
        List<AlphaMarker> Alphas;

        ref ColorMarker GetColorMarker(int index);

        ref AlphaMarker GetAlphaMarker(int index);

        bool AddColorMarker(float x, Vector3 color, float intensity);

        bool AddAlphaMarker(float x, float alpha);

        bool RemoveColorMarker(int32_t index);

        bool RemoveAlphaMarker(int32_t index);

        std::array<float, 4> GetCombinedColor(float x) const;

        std::array<float, 4> GetColorAndIntensity(float x) const;

        float GetAlpha(float x) const;
    };

    enum ImGradientHDRMarkerType
    {
        Unknown = default,
        Color,
        Alpha,
    };

    struct ImGradientHDRTemporaryState
    {
        ImGradientHDRMarkerType selectedMarkerType;
        int selectedIndex = -1;

        ImGradientHDRMarkerType draggingMarkerType;
        int draggingIndex = -1;
    };

    class ImGradient
    {
        

        public static bool Draw(int gradientID, ref ImGradientHDRState state, ref ImGradientHDRTemporaryState temporaryState)
        {
            bool changed = false;

            ImGui.PushID(gradientID);

            var originPos = ImGui.GetCursorScreenPos();

            var drawList = ImGui.GetWindowDrawList();

            var margin = 5;

            var width = ImGui.GetContentRegionAvail().X - margin * 2;
            var barHeight = 20;
            var markerWidth = 10;
            var markerHeight = 15;

            changed |= UpdateMarker(state.Alphas, state.AlphaCount, temporaryState, ImGradiengMarkerType.Alpha, "a", originPos, width, markerWidth, markerHeight, MarkerDirection::ToLower);

            if (temporaryState.draggingMarkerType == ImGradientHDRMarkerType::Alpha)
            {
                SortMarkers(state.Alphas, state.AlphaCount, temporaryState.selectedIndex, temporaryState.draggingIndex);
            }

            ImGui::SetCursorScreenPos(originPos);

            ImGui::InvisibleButton("AlphaArea", { width, static_cast<float>(markerHeight)});

            if (ImGui::IsItemHovered() && ImGui::IsMouseClicked(0))
            {
                float x = (ImGui::GetIO().MousePos.x - originPos.x) / width;
                const auto alpha = state.GetAlpha(x);
                changed |= state.AddAlphaMarker(x, alpha);
            }

            originPos = ImGui::GetCursorScreenPos();

            ImGui::InvisibleButton("BarArea", { width, static_cast<float>(barHeight)});

            const int32_t gridSize = 10;

            drawList->AddRectFilled(ImVec2(originPos.x - 2, originPos.y - 2),
                                    ImVec2(originPos.x + width + 2, originPos.y + barHeight + 2),
                                    IM_COL32(100, 100, 100, 255));

            for (int y = 0; y * gridSize < barHeight; y += 1)
            {
                for (int x = 0; x * gridSize < width; x += 1)
                {
                    int wgrid = std::min(gridSize, static_cast<int>(width) - x * gridSize);
                    int hgrid = std::min(gridSize, barHeight - y * gridSize);
                    ImU32 color = IM_COL32(100, 100, 100, 255);

                    if ((x + y) % 2 == 0)
                    {
                        color = IM_COL32(50, 50, 50, 255);
                    }

                    drawList->AddRectFilled(ImVec2(originPos.x + x * gridSize, originPos.y + y * gridSize),
                                            ImVec2(originPos.x + x * gridSize + wgrid, originPos.y + y * gridSize + hgrid),
                                            color);
                }
            }

            {
                std::vector<float> xkeys;
                xkeys.reserve(16);

                for (int32_t i = 0; i < state.ColorCount; i++)
                {
                    xkeys.emplace_back(state.Colors[i].Position);
                }

                for (int32_t i = 0; i < state.AlphaCount; i++)
                {
                    xkeys.emplace_back(state.Alphas[i].Position);
                }

                xkeys.emplace_back(0.0f);
                xkeys.emplace_back(1.0f);

                auto result = std::unique(xkeys.begin(), xkeys.end());
                xkeys.erase(result, xkeys.end());

                std::sort(xkeys.begin(), xkeys.end());

                for (size_t i = 0; i < xkeys.size() - 1; i++)
                {
                    const auto c1 = state.GetCombinedColor(xkeys[i]);
                    const auto c2 = state.GetCombinedColor(xkeys[i + 1]);

                    const auto colorAU32 = ImGui::ColorConvertFloat4ToU32({ c1[0], c1[1], c1[2], c1[3]});
                const auto colorBU32 = ImGui::ColorConvertFloat4ToU32({ c2[0], c2[1], c2[2], c2[3]});

                drawList->AddRectFilledMultiColor(ImVec2(originPos.x + xkeys[i] * width, originPos.y),
                                                  ImVec2(originPos.x + xkeys[i + 1] * width, originPos.y + barHeight),
                                                  colorAU32,
                                                  colorBU32,
                                                  colorBU32,
                                                  colorAU32);
            }
        }
    }
*/