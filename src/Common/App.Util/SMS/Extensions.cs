/* ----------------------------------------------------------
文件名称：Extensions.cs

作者：秦建辉

MSN：splashcn@msn.com
QQ：36748897

博客：http://blog.csdn.net/jhqin

开发环境：
    Visual Studio V2010
    .NET Framework 4 Client Profile

版本历史：    
    V1.0	2011年08月09日
			扩展方法：
            1、Int32类型的Bit位测试和Bit位设置
            2、Array类型的子数组检索
------------------------------------------------------------ */
using System;

namespace App.Util.SMS
{
    /// <summary>
    /// 扩展方法：
    /// 1、Int32类型的Bit位测试和Bit位设置
    /// 2、Array类型的子数组检索
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Bit位测试
        /// </summary>
        /// <param name="n">要测试的整数</param>
        /// <param name="bit">要测试的Bit位序号</param>
        /// <returns>
        ///     true：该Bit位为1
        ///     false：该Bit为0
        /// </returns>
        public static Boolean BitTest(this Int32 n, Int32 bit)
        {
            if ((n & (1 << bit)) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Bit位设置
        /// </summary>
        /// <param name="n">要设置的整数</param>
        /// <param name="bit">要设置的Bit位序号</param>
        public static Int32 BitSet(this Int32 n, Int32 bit)
        {
            return n | (1 << bit);
        }

        /// <summary>
        /// 从此实例检索子数组
        /// </summary>
        /// <param name="source">要检索的数组</param>
        /// <param name="startIndex">起始索引号</param>
        /// <param name="length">检索最大长度</param>
        /// <returns>与此实例中在 startIndex 处开头、长度为 length 的子数组等效的一个数组</returns>
        public static Array SubArray(this Array source, Int32 startIndex, Int32 length)
        {
            if (startIndex < 0 || startIndex > source.Length || length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            Array Destination;
            if (startIndex + length <= source.Length)
            {
                Destination = Array.CreateInstance(source.GetType(), length);
                Array.Copy(source, startIndex, Destination, 0, length);
            }
            else
            {
                Destination = Array.CreateInstance(source.GetType(), source.Length - startIndex);
                Array.Copy(source, startIndex, Destination, 0, source.Length - startIndex);
            }

            return Destination;
        }        
    }
}
