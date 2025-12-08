// <copyright file="IRootObjectMember.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IRootObjectMember<TRootObject> : IRootObjectComponent
        where TRootObject : class, IRootObject
    {
        TRootObject RootObject { get; set; }
    }
}