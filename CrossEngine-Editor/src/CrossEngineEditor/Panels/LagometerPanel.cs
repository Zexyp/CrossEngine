using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine;

namespace CrossEngineEditor.Panels
{
    class LagometerPanel : EditorPanel
    {
        public LagometerPanel() : base("Lagometer")
        {
        }

        float queue = 1;
        int lastFrames = 0;
        int frames = 0;
        float updateFramesEvery = 1f;

        const int FrameTimePlotLength = 256;
        float[] frameTimePlot = new float[FrameTimePlotLength];
        int frameTimePlotOffset = 0;
        bool updateFrameTimePlot = true;

        float memoryLastUpdated = 0.0f;
        float memoryUpdateEvery = 5f;
        float memoryMegabytes = 0;

        protected override void DrawWindowContent()
        {
            {
                frames++;
                queue += Time.DeltaTimeF;
                if (queue >= updateFramesEvery)
                {
                    queue = 0;
                    lastFrames = (int)((float)frames / updateFramesEvery);
                    frames = 0;
                }
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, Math.Min((float)lastFrames / 30, 1), (float)lastFrames / 60, 1));
                ImGui.Text($"FPS: {lastFrames:F1}");
                ImGui.PopStyleColor();
            }

            {
                if (updateFrameTimePlot)
                {
                    frameTimePlot[frameTimePlotOffset] = Time.DeltaTimeF * 1000;
                    frameTimePlotOffset = (frameTimePlotOffset + 1) % frameTimePlot.Length;
                }

                float average = 0.0f;
                float max = 0.0f;
                // so lazy, replace dis to remember max value index
                for (int n = 0; n < frameTimePlot.Length; n++)
                {
                    max = (max < frameTimePlot[n]) ? frameTimePlot[n] : max;
                    average += frameTimePlot[n];
                }
                average /= (float)frameTimePlot.Length;

                ImGui.PlotHistogram("Frame times", ref frameTimePlot[0], frameTimePlot.Length, frameTimePlotOffset, $"avg {average:F2} ms", 0, max, new Vector2(0, 80.0f));
                updateFrameTimePlot = !(ImGui.IsItemHovered() && ImGui.IsMouseDown(ImGuiMouseButton.Left));
            }

            {
                if (Time.TotalElapsedSecondsF - memoryLastUpdated >= memoryUpdateEvery)
                {
                    memoryLastUpdated = Time.TotalElapsedSecondsF;
                    using (System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess())
                        memoryMegabytes = (proc.PrivateMemorySize64 / (1024 * 1024));
                }
                ImGui.Text($"Memory usage: {memoryMegabytes} MB");
            }
        }
    }
}
