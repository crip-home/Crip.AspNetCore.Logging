﻿// Fix for "IsExternalInit" error.
// https://www.mking.net/blog/error-cs0518-isexternalinit-not-defined

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET45 || NET451 || NET452 || NET6 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable SA1600
    internal static class IsExternalInit
    {
    }
#pragma warning restore SA1600
}

#endif