// <copyright file="SetParam.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence.Lib
{
    public abstract class SetParam : AnimatorActionTask
    {
        public VariableReference<string> ParamName;
    }
}