// <copyright file="Profiler.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public static class Profiler
    {
        public static ProfilerSampler Sample(string name)
        {
            return new ProfilerSampler(name);
        }
    }
}

