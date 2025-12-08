// <copyright file="Decision.Considerations.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Extensions;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public partial class Decision
    {
        private readonly Dictionary<string, Consideration> considerationDict = new();
        private readonly List<Consideration> considerations = new();
        public IReadOnlyList<Consideration> Considerations => considerations;

        public bool HasConsideration(string name)
        {
            return considerationDict.ContainsKey(name);
        }

        internal bool TryAddConsideration(string name, Consideration consideration)
        {
            if (TryAddConsiderationWithoutCompensation(name, consideration))
            {
                UpdateCompensationFactor();
                return true;
            }

            return false;
        }

        internal bool TryAddConsideration(int index, string name, Consideration consideration)
        {
            if (TryAddConsiderationWithoutCompensation(index, name, consideration))
            {
                UpdateCompensationFactor();
                return true;
            }

            return false;
        }

        private bool TryAddConsiderationWithoutCompensation(int index, string name, Consideration consideration)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (considerationDict.TryAdd(name, consideration))
            {
                considerations.Insert(index, consideration);
                return true;
            }

            return false;
        }

        internal bool TryAddConsiderationWithoutCompensation(string name, Consideration consideration)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (considerationDict.TryAdd(name, consideration))
            {
                considerations.Add(consideration);
                return true;
            }

            return false;
        }

        internal bool TryRemoveConsideration(string name)
        {
            if (TryRemoveConsiderationWithoutCompensation(name))
            {
                UpdateCompensationFactor();
                return true;
            }

            return false;
        }

        internal bool TryRemoveConsiderationWithoutCompensation(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (considerationDict.Remove(name, out Consideration consideration))
            {
                considerations.Remove(consideration);
                return true;
            }

            return false;
        }

        public bool TryGetConsideration(string name, out Consideration consideration)
        {
            consideration = null;
            if (string.IsNullOrEmpty(name))
                return false;

            if (considerationDict.TryGetValue(name, out consideration))
                return true;

            return false;
        }

        internal void MoveConsideration(int sourceIndex, int destIndex)
        {
            considerations.Move(sourceIndex, destIndex);
        }
    }
}