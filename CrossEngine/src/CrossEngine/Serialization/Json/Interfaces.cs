﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Serialization.Json
{
    internal interface IInitializedConverter
    {
        void Init();
        void Finish();
    }

    internal interface ITypeResolveConverter
    {
        TypeResolver Resolver { get; set; }
    }
}
