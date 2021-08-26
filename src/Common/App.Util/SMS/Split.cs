/* ----------------------------------------------------------
文件名称：Split.cs

作者：秦建辉

MSN：splashcn@msn.com
QQ：36748897

博客：http://blog.csdn.net/jhqin

开发环境：
    Visual Studio V2010
    .NET Framework 4 Client Profile

版本历史：    
    V1.0	2011年08月19日
			短信拆分部分
------------------------------------------------------------ */
using System;
using System.Collections.Generic;

namespace App.Util.SMS
{
    /// <summary>
    /// 短信拆分部分
    /// </summary>
    public partial class SMS
    {
        /// <summary>
        /// 编码方案对应的最大用户数据长度
        /// </summary>
        private enum EnumUDL
        {
            BIT7UDL = 160,  // 7Bit编码允许的最大字符数
            BIT8UDL = 140,  // 8Bit编码允许的最大字节数
            UCS2UDL = 70    // UCS2编码允许的最大字符数
        }

        /// <summary>
        /// 用户数据内容拆分
        /// </summary>
        /// <param name="UDC">用户数据内容</param>
        /// <param name="UDH">用户数据头</param>
        /// <param name="DCS">编码方案</param>
        /// <returns>拆分内容列表</returns>
        private List<String> UDCSplit(String UDC, PDUUDH[] UDH = null, EnumDCS DCS = EnumDCS.UCS2)
        {   // 统计用户数据头长度
            Int32 UDHL = GetUDHL(UDH);

            if (DCS == EnumDCS.BIT7)
            {   // 7-Bit编码
                // 计算剩余房间数
                Int32 Room = (Int32)EnumUDL.BIT7UDL - (UDHL * 8 + 6) / 7;
                if (Room < 1)
                {
                    if (String.IsNullOrEmpty(UDC))
                        return new List<String>() { UDC };
                    else
                        return null;    // 超出范围
                }

                if (SeptetsLength(UDC) <= Room)
                {
                    return new List<String>() { UDC };
                }
                else
                {   // 需要拆分成多条短信
                    if (UDHL == 0) UDHL++;
                    if (mCSMIEI == EnumCSMIEI.BIT8)
                        UDHL += 5;  // 1字节消息参考号
                    else
                        UDHL += 6;  // 2字节消息参考号

                    // 更新剩余房间数
                    Room = (Int32)EnumUDL.BIT7UDL - (UDHL * 8 + 6) / 7;
                    if (Room < 1) return null;   // 超出范围

                    List<String> CSM = new List<String>();                    
                    Int32 i = 0;
                    while (i < UDC.Length)
                    {
                        Int32 Step = SeptetsToChars(UDC, i, Room);
                        if (i + Step < UDC.Length)
                            CSM.Add(UDC.Substring(i, Step));                        
                        else
                            CSM.Add(UDC.Substring(i));

                        i += Step;
                    }

                    return CSM;
                }
            }
            else
            {   // UCS2编码
                // 计算剩余房间数
                Int32 Room = ((Int32)EnumUDL.BIT8UDL - UDHL) >> 1;
                if(Room < 1)
                {
                    if (String.IsNullOrEmpty(UDC))
                        return new List<String>() { UDC };
                    else
                        return null;    // 超出范围
                }

                if (UDC == null || UDC.Length <= Room)
                {
                    return new List<String>() { UDC };
                }
                else
                {   // 需要拆分成多条短信
                    if (UDHL == 0) UDHL++;
                    if (mCSMIEI == EnumCSMIEI.BIT8)
                        UDHL += 5;  // 1字节消息参考号
                    else
                        UDHL += 6;  // 2字节消息参考号

                    // 更新剩余房间数
                    Room = ((Int32)EnumUDL.BIT8UDL - UDHL) >> 1;
                    if (Room < 1) return null;  // 超出范围

                    List<String> CSM = new List<String>();
                    for (Int32 i = 0; i < UDC.Length; i += Room)
                    {
                        if (i + Room < UDC.Length)
                            CSM.Add(UDC.Substring(i, Room));
                        else
                            CSM.Add(UDC.Substring(i));
                    }

                    return CSM;
                }
            }
        }

        /// <summary>
        /// 用户数据内容拆分
        /// </summary>
        /// <param name="UDC">用户数据内容</param>
        /// <param name="UDH">用户数据头</param>
        /// <returns>拆分内容列表</returns>
        private List<Byte[]> UDCSplit(Byte[] UDC, PDUUDH[] UDH = null)
        {   // 统计用户数据头长度
            Int32 UDHL = GetUDHL(UDH);
            
            // 8-Bit编码
            if (UDC == null || UDC.Length <= (Int32)EnumUDL.BIT8UDL - UDHL)
            {   // 不需要拆分
                return new List<Byte[]>() { UDC };
            }
            else
            {   // 需要拆分成多条短信
                if (UDHL == 0) UDHL++;
                if (mCSMIEI == EnumCSMIEI.BIT8)
                    UDHL += 5;  // 1字节消息参考号
                else
                    UDHL += 6;  // 2字节消息参考号

                // 短信内容拆分
                List<Byte[]> CSM = new List<Byte[]>();
                Int32 Step = (Int32)EnumUDL.BIT8UDL - UDHL;
                for (Int32 i = 0; i < UDC.Length; i += Step)
                {
                    CSM.Add((Byte[])UDC.SubArray(i, Step));
                }

                return CSM;
            }
        }

        /// <summary>
        /// 用户数据头长度
        /// </summary>
        /// <param name="UDH">用户数据头</param>
        /// <returns>用户数据头编码字节数</returns>
        private static Int32 GetUDHL(PDUUDH[] UDH)
        {
            if (UDH == null || UDH.Length == 0) return 0;

            Int32 UDHL = 1;     // 加上1字节的用户数据头长度
            foreach (PDUUDH IE in UDH)
            {
                UDHL += IE.IED.Length + 2;  // 信息元素标识+信息元素长度+信息元素数据
            }

            return UDHL;
        }

        /// <summary>
        /// 计算字符串需要的7-Bit编码字节数
        /// </summary>
        /// <param name="source">字符串</param>
        /// <returns>7-Bit编码字节数</returns>
        private static Int32 SeptetsLength(String source)
        {
            if (String.IsNullOrEmpty(source)) return 0;
            
            Int32 Length = source.Length;
            foreach (Char Letter in source)
            {
                UInt16 Code = Convert.ToUInt16(Letter);
                if (UCS2ToBIT7.ContainsKey(Code))
                {
                    if (UCS2ToBIT7[Code] > 0xFF) Length++;
                }
            }

            return Length;
        }

        /// <summary>
        /// 判断字符串是否在GSM缺省字符集内
        /// </summary>
        /// <param name="source">要评估的字符串</param>
        /// <returns>
        ///     true：在GSM缺省字符集内，可以使用7-Bit编码
        ///     false：不在GSM缺省字符集内，只能使用UCS2编码
        /// </returns>
        public static Boolean IsGSMString(string source)
        {
            if (string.IsNullOrEmpty(source)) return true;

            foreach (Char Letter in source)
            {
                UInt16 Code = Convert.ToUInt16(Letter);
                if (!(isBIT7Same(Code) || UCS2ToBIT7.ContainsKey(Code)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 将7-Bit编码字节数换算成UCS2编码字符数
        /// </summary>
        /// <param name="source">字符串</param>
        /// <param name="index">起始索引号</param>
        /// <param name="septets">要换算的7-Bit编码字节数</param>
        /// <returns>UCS2编码字符数</returns>
        private static Int32 SeptetsToChars(string source, Int32 index, Int32 septets)
        {
            if (string.IsNullOrEmpty(source)) return 0;

            Int32 Count = 0;
            Int32 i = index;
            for (; i < source.Length; i++)
            {
                UInt16 Code = Convert.ToUInt16(source[i]);
                if (UCS2ToBIT7.ContainsKey(Code) && UCS2ToBIT7[Code] > 0xFF)
                {
                    Count++;
                }
                
                if (++Count >= septets)
                {
                    if (Count == septets) i++;
                    break;
                }
            }

            return i - index;
        }

        /// <summary>
        /// 在用户数据头中增加长短信信息元素
        /// </summary>
        /// <param name="UDH">原始用户数据头</param>
        /// <param name="CSMMR">消息参考号</param>
        /// <param name="Total">短消息总数</param>
        /// <param name="Index">短消息序号</param>
        /// <returns>更新后的用户数据头</returns>
        private PDUUDH[] UpdateUDH(PDUUDH[] UDH, Int32 CSMMR, Int32 Total, Int32 Index)
        {
            List<PDUUDH> CSMUDH;
            if (UDH == null || UDH.Length == 0)
                CSMUDH = new List<PDUUDH>(1);
            else
                CSMUDH = new List<PDUUDH>(UDH);
            
            if (mCSMIEI == EnumCSMIEI.BIT8)
            {
                Byte[] IED = new Byte[3]{(Byte)(CSMMR & 0xFF), (Byte)Total, (Byte)(Index + 1)};
                CSMUDH.Insert(0, new PDUUDH { IEI = 0, IED = IED });
            }
            else
            {
                Byte[] IED = new Byte[4] { (Byte)((CSMMR >> 8) & 0xFF), (Byte)(CSMMR & 0xFF), (Byte)Total, (Byte)(Index + 1) };
                CSMUDH.Insert(0, new PDUUDH { IEI = 8, IED = IED });
            }

            return CSMUDH.ToArray();
        }
    }
}
