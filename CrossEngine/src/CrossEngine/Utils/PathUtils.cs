﻿using System;
using System.IO;

namespace CrossEngine.Utils
{
    public static class PathUtils
    {
        // ! this is broken
        // TODO: please fix
        public static string GetRelativePath(string relativTo, string path)
        {
            if (Path.IsPathRooted(path)) return Path.GetRelativePath(relativTo, path);
            else return path;
        }
    }
}
