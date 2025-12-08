// <copyright file="NavMeshAgentVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.AI;

namespace CarlosLab.Common
{
    public class NavMeshAgentVariable : Variable<NavMeshAgent>
    {
        public static implicit operator NavMeshAgentVariable(NavMeshAgent value)
        {
            return new NavMeshAgentVariable { Value = value };
        }
    }
}