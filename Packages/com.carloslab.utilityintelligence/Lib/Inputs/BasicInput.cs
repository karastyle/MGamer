// <copyright file="BasicInput.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class BasicInput<T> : Input<T>
    {
        public VariableReference<T> InputValue;
        protected override T OnGetRawInput(in InputContext context)
        {
            return InputValue.Value;
        }
    }

    [Category("Basic")]
    public class BasicInputInt : BasicInput<int>
    {

    }

    [Category("Basic")]
    public class BasicInputBool : BasicInput<bool>
    {

    }

    [Category("Basic")]
    public class BasicInputFloat : BasicInput<float>
    {

    }

    [Category("Basic")]
    public class BasicInputDouble : BasicInput<double>
    {

    }

    [Category("Basic")]
    public class BasicInputLong : BasicInput<long>
    {

    }
}
