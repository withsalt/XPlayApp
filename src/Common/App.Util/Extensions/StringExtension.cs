using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace App.Util.Extensions
{
    public static class StringExtension
    {
        /// <summary>
        /// Windows 路径转换为Unix路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ConvertWindowsPathToUnixPath(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// Unix路径转换为Windows路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ConvertUnixPathToWindowsPath(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return path.Replace('/', '\\');
        }

        /// <summary>
        /// 更具当前系统自动转换路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AutoPathConvert(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return path.Replace('\\', '/');
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return path.Replace('/', '\\');
            }
            else
            {
                throw new Exception("Convert path split failed. Unknow system.");
            }
        }

        public static string JoinToString(this string[] stringArray)
        {
            if (stringArray.Length == 0)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var item in stringArray)
            {
                sb.Append(item);
                sb.Append(" ");
            }
            string result = sb.ToString();
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            else
            {
                return result.TrimEnd();
            }
        }

        public static int ZhLength(this string value, int oneCharLength = 2)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            if(oneCharLength < 1)
            {
                throw new ArgumentException("Char length can not less than one.");
            }
            int length = 0;
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            foreach (byte item in bytes)
            {
                if (item == 63)
                {
                    length += oneCharLength;
                }
                else
                {
                    length += 1;
                }
            }
            return length;
        }
    }
}
