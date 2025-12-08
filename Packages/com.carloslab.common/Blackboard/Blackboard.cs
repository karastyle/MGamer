// <copyright file="Blackboard.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public class Blackboard : ItemValueContainer<Variable>, IRuntimeObject
    {
        public TVariable GetVariable<TVariable>(string name)
            where TVariable : Variable
        {
            return GetItem<TVariable>(name);
        }

        public bool TryGetVariable<TVariable>(string name, out TVariable variable)
            where TVariable : Variable
        {
            return TryGetItem(name, out variable);
        }
    }
}