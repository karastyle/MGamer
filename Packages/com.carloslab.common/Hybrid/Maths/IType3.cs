// <copyright file="IType3.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IType3<T>
    {
        T X { get; set; }
        T Y { get; set; }
        T Z { get; set; }
    }
}