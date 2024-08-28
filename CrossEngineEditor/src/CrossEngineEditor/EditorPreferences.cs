using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrossEngineEditor
{
    public class EditorPreferences
    {
        [EditorValue]
        public bool TouchpadControls = false;

        public static EditorPreferences Read(string filepath)
        {
            Debug.Assert(File.Exists(filepath));

            using (Stream stream = File.OpenRead(filepath))
            {
                return JsonSerializer.Deserialize<EditorPreferences>(stream);
            }
        }

        public static void Write(EditorPreferences preferences, string filepath)
        {
            using (Stream stream = File.OpenWrite(filepath))
            {
                JsonSerializer.Serialize(stream, preferences);
            }
        }
    }
}
