// <copyright file="NameValidator.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Text.RegularExpressions;

namespace CarlosLab.Common.UI
{
    public static class NameValidator
    {
        public static bool ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (name.Length > 64)
                return false;

            return Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$");
        }
    }
}