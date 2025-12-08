// <copyright file="CharacterAnimationEvents.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        public event Action Attack;
        public void RaiseAttackEvent()
        {
            Attack?.Invoke();
        }
    }
}