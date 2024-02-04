﻿using CrossEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public static class AssetManager
    {
        [ThreadStatic]
        private static AssetPool _current;
        internal static AssetService service;
        public static AssetPool Current { get => _current; }

        public static Asset Get(Type typeOfAsset, Guid id)
        {
            return _current.Get(typeOfAsset, id);
        }

        public static void Bind(AssetPool pool)
        {
            _current = pool;
        }

        public static void Load()
        {
            service.Load(_current);
        }

        public static void Unload()
        {
            service.Unload(_current);
        }

        public static void Read(string filepath)
        {
            throw new NotImplementedException();
        }
    }
}