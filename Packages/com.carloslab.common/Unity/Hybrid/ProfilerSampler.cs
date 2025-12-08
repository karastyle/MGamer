// <copyright file="ProfilerSampler.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common
{
    public struct ProfilerSampler : IDisposable
    {
#if CARLOSLAB_ENABLE_PROFILER
        Unity.Profiling.ProfilerMarker _marker;
        public ProfilerSampler(string name)
        {
            _marker = new(name);
            _marker.Begin();
        }
        public void Dispose()
        {
            _marker.End();
        }
#else
        public ProfilerSampler(string name)
        {
        }

        public void Dispose()
        {
        }
#endif
    }
}
