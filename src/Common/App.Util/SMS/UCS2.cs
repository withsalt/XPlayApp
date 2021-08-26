using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace App.Util.SMS
{
    public class UCS2
    {
        /// <summary>
        /// UCS2解码 
        /// </summary>
        /// <param name="src"> UCS2 源串 </param>
        /// <returns> 解码后的UTF-16BE字符串</returns>
        public static string Decode(string src)
        {
            if (string.IsNullOrEmpty(src))
                throw new ArgumentNullException(nameof(src));
            int indexLR = src.IndexOf("\r");
            if (indexLR > -1)
                src = src.Remove(indexLR);

            string pstr = "^[0-9a-fA-F]+$";
            if (!Regex.IsMatch(src, pstr))
            {
                return null;
            }
            if (src.Length % 4 != 0)
            {
                return null;
            }
            StringBuilder builer = new StringBuilder();
            for (int i = 0; i < src.Length; i += 4)
            {
                if (int.TryParse(src.Substring(i, 4), NumberStyles.HexNumber, null, out int unicode))
                {
                    builer.Append(string.Format("{0}", (char)unicode));
                }
                else
                {
                    builer.Clear();
                    return null;
                }
            }
            return builer.ToString();
        }

        public static bool TryDecode(string input, out string output)
        {
            output = null;
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    return false;
                }
                string value = Decode(input);
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }
                output = value;
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// UCS2编码
        /// </summary>
        /// <param name="src"> UTF-16BE编码的源串</param>
        /// <returns>编码后的UCS2串 </returns>
        public static string Encode(string src)
        {
            StringBuilder builer = new StringBuilder();
            builer.Append("000800");
            byte[] tmpSmsText = Encoding.Unicode.GetBytes(src);
            builer.Append(tmpSmsText.Length.ToString("X2"));
            for (int i = 0; i < tmpSmsText.Length; i += 2)
            {
                builer.Append(tmpSmsText[i + 1].ToString("X2"));
                builer.Append(tmpSmsText[i].ToString("X2"));
            }
            builer = builer.Remove(0, 8);

            return builer.ToString();
        }
    }
}
