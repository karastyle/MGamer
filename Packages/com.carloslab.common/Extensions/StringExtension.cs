// <copyright file="StringExtension.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Text;
using System.Threading;

namespace CarlosLab.Common.Extensions
{
    public static class StringExtension
    {
        private static readonly ThreadLocal<StringBuilder> stringBuilder = new(() => new StringBuilder(256));

        public static string FastConcat(this string str1, string value)
        {
            var builder = stringBuilder.Value;

            builder.Clear();

            builder.Append(str1).Append(value);

            return builder.ToString();
        }

        public static string FastConcat(this string str1, int value)
        {
            var builder = stringBuilder.Value;

            builder.Clear();

            builder.Append(str1).Append(value);

            return builder.ToString();
        }

        public static string FastConcat(this string str1, uint value)
        {
            var builder = stringBuilder.Value;

            builder.Clear();

            builder.Append(str1).Append(value);

            return builder.ToString();
        }

        public static string FastConcat(this string str1, long value)
        {
            var builder = stringBuilder.Value;

            builder.Length = 0;

            builder.Append(str1).Append(value);

            return builder.ToString();
        }

        public static string FastConcat(this string str1, float value)
        {
            var builder = stringBuilder.Value;

            builder.Length = 0;

            builder.Append(str1).Append(value);

            return builder.ToString();
        }

        public static string FastConcat(this string str1, double value)
        {
            var builder = stringBuilder.Value;

            builder.Length = 0;

            builder.Append(str1).Append(value);

            return builder.ToString();
        }

        public static string FastConcat(this string str1, string str2, string str3)
        {
            var builder = stringBuilder.Value;

            builder.Length = 0;

            builder.Append(str1);
            builder.Append(str2);
            builder.Append(str3);

            return builder.ToString();
        }

        public static string FastConcat(this string str1, string str2, string str3, string str4)
        {
            var builder = stringBuilder.Value;

            builder.Length = 0;

            builder.Append(str1);
            builder.Append(str2);
            builder.Append(str3);
            builder.Append(str4);

            return builder.ToString();
        }

        public static string FastConcat(this string str1, string str2, string str3, string str4, string str5)
        {
            var builder = stringBuilder.Value;

            builder.Length = 0;

            builder.Append(str1);
            builder.Append(str2);
            builder.Append(str3);
            builder.Append(str4);
            builder.Append(str5);

            return builder.ToString();
        }
    }
}