using System;
using static OpenGL.GL;
using static Evergine.Bindings.Imgui.ImguiNative;
using Evergine.Bindings.Imgui;

using CrossEngine.Display;
using CrossEngine.Platform.Windows;
using CrossEngine.Utils.ImGui;

namespace CrossEngine
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            GlfwWindow window = new GlfwWindow();

            window.CreateWindow();

            Import(GLFW.Glfw.GetProcAddress);

            // Setup Dear ImGui context
            igCreateContext(null);
            var io = igGetIO();
            io->ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;   // Enable Keyboard Controls
            //io->ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;    // Enable Gamepad Controls

            // Setup Dear ImGui style
            igStyleColorsDark(igGetStyle());
            //ImGui::StyleColorsClassic();

            // Setup Platform/Renderer backends
            ImplGlfw.ImGui_ImplGlfw_InitForOpenGL(window.NativeHandle, true);
            ImplOpenGL.ImGui_ImplOpenGL3_Init("#version 330 core");

            // set up vertex data (and buffer(s)) and configure vertex attributes
            // ------------------------------------------------------------------
            float[] vertices = {
                -0.5f, -0.5f, 0.0f, // left  
                 0.5f, -0.5f, 0.0f, // right 
                 0.0f,  0.5f, 0.0f  // top   
            }; 

            uint VBO, VAO;
            glGenVertexArrays(1, &VAO);
            glGenBuffers(1, &VBO);
            // bind the Vertex Array Object first, then bind and set vertex buffer(s), and then configure vertex attributes(s).
            glBindVertexArray(VAO);

            glBindBuffer(GL_ARRAY_BUFFER, VBO);
            fixed (void* p = &vertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, p, GL_STATIC_DRAW);

            glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), (void*)0);
            glEnableVertexAttribArray(0);

            // note that this is allowed, the call to glVertexAttribPointer registered VBO as the vertex attribute's bound vertex buffer object so afterwards we can safely unbind
            glBindBuffer(GL_ARRAY_BUFFER, 0); 

            // You can unbind the VAO afterwards so other VAO calls won't accidentally modify this VAO, but this rarely happens. Modifying other
            // VAOs requires a call to glBindVertexArray anyways so we generally don't unbind VAOs (nor VBOs) when it's not directly necessary.
            glBindVertexArray(0);

            while (!window.ShouldClose)
            {
                window.PollWindowEvents();

                ImplOpenGL.ImGui_ImplOpenGL3_NewFrame();
                ImplGlfw.ImGui_ImplGlfw_NewFrame();
                igNewFrame();

                // render
                // ------

                glClearColor(MathF.Sin((float)window.Time), 0.3f, 0.3f, 1.0f);
                glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

                glBindVertexArray(VAO); // seeing as we only have a single VAO there's no need to bind it every time, but we'll do so to keep things a bit more organized
                glDrawArrays(GL_TRIANGLES, 0, 3);
                glBindVertexArray(0); // no need to unbind it every time 

                byte o = 1;
                ImguiNative.igShowDemoWindow(&o);
                igBegin("sus", &o, ImGuiWindowFlags.None);
                igEnd();

                igRender();
                ImplOpenGL.ImGui_ImplOpenGL3_RenderDrawData(igGetDrawData());
                //igUpdatePlatformWindows();
                //igRenderPlatformWindowsDefault(null, null);

                window.UpdateWindow();
            }
        }
    }
}
