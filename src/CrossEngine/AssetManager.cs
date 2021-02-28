using System;

using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

using CrossEngine.Rendering.Shading;
using CrossEngine.Rendering.Text;
using CrossEngine.Rendering.Texturing;
using CrossEngine.Utils;
using CrossEngine.Audio;

namespace CrossEngine
{
    public static class AssetManager
    {
        private static string ReadFileAsString(string path)
        {
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, (int)fileStream.Length);

                    string content = Encoding.Default.GetString(buffer);
                    return content;
                }
            }
            catch (FileNotFoundException)
            {
                Log.Error("file not found: " + path);
            }
            catch (DirectoryNotFoundException)
            {
                Log.Error("file not found: " + path);
            }

            return "";
        }

        public static class Shaders
        {
            static Dictionary<string, Shader> shaders = new Dictionary<string, Shader> { };

            public static Shader GetShader(string vertexPath, string fragmentPath)
            {
                if (shaders.ContainsKey(vertexPath + fragmentPath))
                {
                    return shaders[vertexPath + fragmentPath];
                }

                Shader shader = new Shader(ReadFileAsString(FileEnvironment.ResourceFolder + vertexPath), ReadFileAsString(FileEnvironment.ResourceFolder + fragmentPath));
                if (shader.id == 0)
                    Log.Error("invalid shader: " + vertexPath + " | " + fragmentPath);
                shaders.Add(vertexPath + fragmentPath, shader);
                return shader;
            }

            public static Shader GetShader(string path)
            {
                if (shaders.ContainsKey(path))
                {
                    return shaders[path];
                }

                string[] result = System.Text.RegularExpressions.Regex.Split(ReadFileAsString(FileEnvironment.ResourceFolder + path), @"#type+[ ]+([a-zA-Z]+)");

                if (result.Length < 4)
                {
                    Log.Error("unexpected shader format");
                    return null;
                }

                Shader shader;
                if (result[1] == "vertex" && result[3] == "fragment")
                {
                    shader = new Shader(result[2], result[4]);
                }
                else if (result[1] == "fragment" && result[3] == "vertex")
                {
                    shader = new Shader(result[4], result[2]);
                }
                else
                {
                    Log.Error("unexpected shader format");
                    return null;
                }

                if (shader.id == 0)
                    Log.Error("invalid shader: " + path);
                shaders.Add(path, shader);
                return shader;
            }
        }

        public static class Fonts
        {
            static Dictionary<string, Font> fonts = new Dictionary<string, Font> { };

            static PrivateFontCollection fontCollection = new PrivateFontCollection();
            //static Font defaultFont = new Font("Arial", 1); // defualt is null for now

            static List<FontAtlas> loadedFontAtlases = new List<FontAtlas> { }; // stores loaded fonts

            public static Font GetFont(string path)
            {
                // check if it's loaded
                if (fonts.ContainsKey(path))
                {
                    return fonts[path];
                }

                // adding to private font collection
                string fullpath = FileEnvironment.ResourceFolder + path; // the path is only for .ttf now
                if (File.Exists(fullpath))
                {
                    fontCollection.AddFontFile(fullpath);
                    fonts.Add(path, new Font(fontCollection.Families[0], FontAtlasRenderer.fixedFontSize));
                    return fonts[path];
                }
                else
                {
                    Log.Error("font not found!");
                    return null;
                }
            }

            public static Font GetSystemFont(string name)
            {
                return new Font(name, FontAtlasRenderer.fixedFontSize);
            }

            public static FontAtlas GetFontAtlas(Font font)
            {
                // check if it's loaded
                foreach (FontAtlas textFont in loadedFontAtlases)
                {
                    if (font.FontFamily.Name == textFont.font.FontFamily.Name)
                    {
                        return textFont;
                    }
                }

                FontAtlas fontAtlas = FontAtlasRenderer.CreateFontAtlas(font);
                loadedFontAtlases.Add(fontAtlas);
                return fontAtlas;
            }
        }

        // rip TextureManager class
        static public class Textures
        {
            static Dictionary<string, Texture> textures = new Dictionary<string, Texture> { };

            public static Texture GetTexture(string path)
            {
                if(textures.ContainsKey(path))
                {
                    return textures[path];
                }

                string fullpath = FileEnvironment.ResourceFolder + path;
                if (!File.Exists(fullpath))
                {
                    Log.Error("texture not found: " + path);
                    return null;
                }

                Bitmap image = new Bitmap(fullpath);

                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                Texture texture = new Texture(image);

                image.Dispose();

                textures.Add(path, texture);
                return texture;
            }

            public static Texture GetCubeMap(string path, bool secondary = false)
            {
                if (textures.ContainsKey(path))
                {
                    return textures[path];
                }

                string fullpath = FileEnvironment.ResourceFolder + path;
                if (!File.Exists(fullpath))
                {
                    Log.Error("texture not found: " + path);
                    return null;
                }

                Bitmap image = new Bitmap(fullpath);

                Size cut = new Size(image.Width / 4, image.Height / 3);

                Image[] sides = new Image[6];

                if (!secondary)
                {
                    // _ t _ _
                    // r f l b
                    // _ b _ _
                    sides[0] = image.Clone(new Rectangle(0, cut.Height, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // right	            <-- swap those two when doing horizontal flip
                    sides[1] = image.Clone(new Rectangle(cut.Width * 2, cut.Height, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // left    <--
                    sides[2] = image.Clone(new Rectangle(cut.Width, 0, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // top
                    sides[3] = image.Clone(new Rectangle(cut.Width, cut.Height * 2, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // bottom
                    sides[4] = image.Clone(new Rectangle(cut.Width, cut.Height, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // front
                    sides[5] = image.Clone(new Rectangle(cut.Width * 3, cut.Height, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // back
                }
                else
                {
                    // r - l t
                    // _ _ _ |
                    // f - b b
                    sides[0] = image.Clone(new Rectangle(0, 0, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // right	            <-- swap those two when doing horizontal flip
                    sides[1] = image.Clone(new Rectangle(cut.Width * 2, 0, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // left    <--
                    sides[2] = image.Clone(new Rectangle(cut.Width * 3, 0, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // top
                    sides[3] = image.Clone(new Rectangle(cut.Width * 3, cut.Height * 2, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // bottom
                    sides[4] = image.Clone(new Rectangle(0, cut.Height * 2, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // front
                    sides[5] = image.Clone(new Rectangle(cut.Width * 2, cut.Height * 2, cut.Width, cut.Height), PixelFormat.Format24bppRgb); // back
                }

                image.Dispose();

                for (int i = 0; i < sides.Length; i++)
                {
                    sides[i].RotateFlip(RotateFlipType.RotateNoneFlipX);
                }

                Texture texture = new Texture(sides);

                for (int i = 0; i < sides.Length; i++)
                {
                    sides[i].Dispose();
                }

                textures.Add(path, texture);
                return texture;
            }
        }

        public static class Audio
        {
            static Dictionary<string, AudioBuffer> buffers = new Dictionary<string, AudioBuffer> { };

            public static unsafe AudioBuffer GetAudioBuffer(string path, bool forceMono = false, int preferedChannel = 0)
            {
                if (buffers.ContainsKey(path))
                {
                    return buffers[path];
                }

                string fullpath = FileEnvironment.ResourceFolder + path;
                if (!File.Exists(fullpath))
                {
                    Log.Error("file not found: " + path);
                    return null;
                }

                using (BinaryReader binaryReader = new BinaryReader(File.Open(fullpath, FileMode.Open)))
                {
                    WAVReader.FileContent file = WAVReader.Read(binaryReader);
                    if (file == null)
                    {
                        Log.Error("file reading failed!");
                        return null;
                    }

                    if (file.riffChunk.formatSubChunk.numChannels == 1 || forceMono)
                    {
                        object e = file.channelsData[preferedChannel];
                        if (e.GetType() == typeof(sbyte[]))
                        {
                            var array = (byte[])e;
                            fixed (byte* ptr = &array[0])
                                return new AudioBuffer((IntPtr)ptr, array.Length * sizeof(sbyte), AudioFormat.Mono8bit, (int)file.riffChunk.formatSubChunk.sampleRate);
                        }
                        if (e.GetType() == typeof(short[]))
                        {
                            var array = (short[])e;
                            fixed (short* ptr = &array[0])
                                return new AudioBuffer((IntPtr)ptr, array.Length * sizeof(short), AudioFormat.Mono16bit, (int)file.riffChunk.formatSubChunk.sampleRate);
                        }
                    }
                    else if (file.riffChunk.formatSubChunk.numChannels == 2)
                    {
                        object e = file.channelsData[0];
                        if (e.GetType() == typeof(sbyte[]))
                        {
                            fixed (byte* ptr = &file.riffChunk.subChunks["data"].data[0])
                                return new AudioBuffer((IntPtr)ptr, file.riffChunk.subChunks["data"].data.Length, AudioFormat.Stereo8bit, (int)file.riffChunk.formatSubChunk.sampleRate);
                        }
                        if (e.GetType() == typeof(short[]))
                        {
                            fixed (byte* ptr = &file.riffChunk.subChunks["data"].data[0])
                                return new AudioBuffer((IntPtr)ptr, file.riffChunk.subChunks["data"].data.Length, AudioFormat.Stereo16bit, (int)file.riffChunk.formatSubChunk.sampleRate);
                        }
                    }

                    return null;
                }
            }

            class WAVReader
            {
                public static FileContent Read(BinaryReader binaryReader)
                {
                    RiffChunk riffChunk = new RiffChunk(binaryReader);

                    if (riffChunk.chunkId != "RIFF" || riffChunk.format != "WAVE")
                    {
                        Log.Error("wrong chunk id: " + riffChunk.chunkId + " or unexpected format: " + riffChunk.format);
                        return null;
                    }

                    uint expectedByteRate = riffChunk.formatSubChunk.sampleRate * riffChunk.formatSubChunk.numChannels * riffChunk.formatSubChunk.bitsPerSample / 8;
                    if (riffChunk.formatSubChunk.byteRate != expectedByteRate)
                    {
                        Log.Error("unexpected byte rate: " + riffChunk.formatSubChunk.byteRate + "; expected: " + expectedByteRate);
                        return null;
                    }

                    if (riffChunk.formatSubChunk.chunkId != "fmt ")
                    {
                        Log.Error("wrong chunk id: " + riffChunk.formatSubChunk.chunkId);
                        return null;
                    }
                    if (riffChunk.formatSubChunk.audioFormat != 1)
                    {
                        Log.Error("wrong audio format: " + riffChunk.formatSubChunk.audioFormat + " (compression not supported)");
                        return null;
                    }

                    SubChunk dataChunk = riffChunk.subChunks["data"];
                    binaryReader.BaseStream.Position = dataChunk.dataStartPos;

                    uint samples = (uint)(dataChunk.chunkSize / riffChunk.formatSubChunk.numChannels / (riffChunk.formatSubChunk.bitsPerSample / 8));
                    object[] audioDataArray = new object[riffChunk.formatSubChunk.numChannels];
                    if (riffChunk.formatSubChunk.bitsPerSample == 8)
                    {
                        for (uint i = 0; i < riffChunk.formatSubChunk.numChannels; i++)
                        {
                            audioDataArray[i] = new sbyte[samples];
                        }
                        for (uint sampleIndex = 0; sampleIndex < samples; sampleIndex++)
                        {
                            for (uint i = 0; i < riffChunk.formatSubChunk.numChannels; i++)
                            {
                                var sampleValue = binaryReader.ReadSByte();
                                ((sbyte[])audioDataArray[i])[sampleIndex] = sampleValue;
                            }
                        }

                    }
                    else if (riffChunk.formatSubChunk.bitsPerSample == 16)
                    {
                        for (uint i = 0; i < riffChunk.formatSubChunk.numChannels; i++)
                        {
                            audioDataArray[i] = new short[samples];
                        }
                        for (uint sampleIndex = 0; sampleIndex < samples; sampleIndex++)
                        {
                            for (uint i = 0; i < riffChunk.formatSubChunk.numChannels; i++)
                            {
                                var sampleValue = binaryReader.ReadInt16();
                                ((short[])audioDataArray[i])[sampleIndex] = sampleValue;
                            }
                        }

                    }
                    else if (riffChunk.formatSubChunk.bitsPerSample == 32)
                    {
                        for (uint i = 0; i < riffChunk.formatSubChunk.numChannels; i++)
                        {
                            audioDataArray[i] = new int[samples];
                        }
                        for (uint sampleIndex = 0; sampleIndex < samples; sampleIndex++)
                        {
                            for (uint i = 0; i < riffChunk.formatSubChunk.numChannels; i++)
                            {
                                var sampleValue = binaryReader.ReadInt32();
                                ((int[])audioDataArray[i])[sampleIndex] = sampleValue;
                            }
                        }

                    }
                    else
                    {
                        Log.Error("unsupported bits per sample: " + riffChunk.formatSubChunk.bitsPerSample);
                        return null;
                    }

                    //dataSubChunkData.data = binaryReader.ReadBytes((int)dataSubChunkData.subChunkSize);

                    //Console.WriteLine("----- file -----");
                    //Console.WriteLine("riffChunk.chunkId:                      " + riffChunk.chunkId);
                    //Console.WriteLine("riffChunk.chunkSize:                    " + riffChunk.chunkSize);
                    //Console.WriteLine("riffChunk.format:                       " + riffChunk.format);
                    //Console.WriteLine("riffChunk.formatSubChunk.chunkId:       " + riffChunk.formatSubChunk.chunkId);
                    //Console.WriteLine("riffChunk.formatSubChunk.chunkSize:     " + riffChunk.formatSubChunk.chunkSize);
                    //Console.WriteLine("riffChunk.formatSubChunk.audioFormat:   " + riffChunk.formatSubChunk.audioFormat);
                    //Console.WriteLine("riffChunk.formatSubChunk.numChannels:   " + riffChunk.formatSubChunk.numChannels);
                    //Console.WriteLine("riffChunk.formatSubChunk.sampleRate:    " + riffChunk.formatSubChunk.sampleRate);
                    //Console.WriteLine("riffChunk.formatSubChunk.byteRate:      " + riffChunk.formatSubChunk.byteRate);
                    //Console.WriteLine("riffChunk.formatSubChunk.blockAlign:    " + riffChunk.formatSubChunk.blockAlign);
                    //Console.WriteLine("riffChunk.formatSubChunk.bitsPerSample: " + riffChunk.formatSubChunk.bitsPerSample);

                    FileContent wavFileData = new FileContent();
                    wavFileData.riffChunk = riffChunk;
                    wavFileData.channelsData = audioDataArray;
                    return wavFileData;
                }

                //public struct RiffChunkData
                //{
                //    public string riffChunkId;
                //    public uint riffChunkSize;
                //    public string format;
                //}
                //
                //public struct FmtSubChunkData
                //{
                //    public string subChunkId;
                //    public uint subChunkSize;
                //    public ushort audioFormat;
                //    public ushort numChannels;
                //    public uint sampleRate;
                //    public uint byteRate;
                //    public ushort blockAlign;
                //    public ushort bitsPerSample;
                //}
                //
                //public struct DataSubChunkData
                //{
                //    public string subChunkId;
                //    public uint subChunkSize;
                //    public object[] data;
                //}

                //stuint byteRate = binaryatic int ToInt32BigEndian(byte[] buf, int i)
                //{ ushort blockAlign = bi
                //  ushort bitsPerSample =  return (buf[i] << 24) | (buf[i + 1] << 16) | (buf[i + 2] << 8) | buf[i + 3];
                //}

                public class FileContent
                {
                    public RiffChunk riffChunk;
                    public object[] channelsData;
                }

                public class Chunk
                {
                    public long dataStartPos = 0;

                    public string chunkId;
                    public uint chunkSize;

                    public Chunk(BinaryReader binaryReader)
                    {
                        chunkId = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                        chunkSize = binaryReader.ReadUInt32();

                        dataStartPos = binaryReader.BaseStream.Position;
                    }
                }

                public class SubChunk : Chunk
                {

                    public byte[] data;

                    public SubChunk(BinaryReader binaryReader) : base(binaryReader)
                    {
                        data = binaryReader.ReadBytes((int)chunkSize);
                        long endPos = binaryReader.BaseStream.Position;
                        binaryReader.BaseStream.Position = dataStartPos;
                        ReadData(binaryReader);
                        binaryReader.BaseStream.Position = endPos;
                    }

                    protected virtual void ReadData(BinaryReader binaryReader)
                    {

                    }
                }

                public class RiffChunk : Chunk
                {
                    public string format;
                    public FormatSubChunk formatSubChunk;
                    public Dictionary<string, SubChunk> subChunks = new Dictionary<string, SubChunk> { };

                    public RiffChunk(BinaryReader binaryReader) : base(binaryReader)
                    {
                        if (chunkSize != binaryReader.BaseStream.Length - 8)
                            Log.Error("unexpected file size: " + binaryReader.BaseStream.Length + "; expected: " + chunkSize);

                        format = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));

                        formatSubChunk = new FormatSubChunk(binaryReader);

                        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                        {
                            SubChunk subChunk = new SubChunk(binaryReader);
                            subChunks.Add(subChunk.chunkId, subChunk);
                        }
                    }
                }

                public class FormatSubChunk : SubChunk
                {
                    public ushort audioFormat;
                    public ushort numChannels;
                    public uint sampleRate;
                    public uint byteRate;
                    public ushort blockAlign;
                    public ushort bitsPerSample;

                    public FormatSubChunk(BinaryReader binaryReader) : base(binaryReader)
                    {

                    }

                    protected override void ReadData(BinaryReader binaryReader)
                    {
                        audioFormat = binaryReader.ReadUInt16();
                        numChannels = binaryReader.ReadUInt16();
                        sampleRate = binaryReader.ReadUInt32();
                        byteRate = binaryReader.ReadUInt32();
                        blockAlign = binaryReader.ReadUInt16();
                        bitsPerSample = binaryReader.ReadUInt16();
                    }
                }
            }
        }
    }
}
