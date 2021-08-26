/* ----------------------------------------------------------
文件名称：Decode.cs

作者：秦建辉

MSN：splashcn@msn.com
QQ：36748897

博客：http://blog.csdn.net/jhqin

开发环境：
    Visual Studio V2010
    .NET Framework 4 Client Profile

版本历史：    
    V1.0	2011年08月19日
			PDU格式短信解码部分（不支持文本压缩短信）
------------------------------------------------------------ */
using System;
using System.Text;
using System.Collections.Generic;

namespace App.Util.SMS
{
    /// <summary>
    /// PDU格式短信解码部分
    /// 接口函数：PDUDecoding
    /// 注意：不支持文本压缩短信
    /// </summary>
    public partial class SMS
    {
        /// <summary>
        /// 信息元素结构体，包含信息元素标识和信息元素数据
        /// </summary>
        public struct PDUUDH
        {
            public Byte IEI;    // 信息元素标识（Information Element Identifier）
            public Byte[] IED;  // 信息元素数据（Information Element Data）
        }

        /// <summary>
        /// 短信结构体
        /// </summary>
        public struct SMSPARTS
        {
            public String SCA;          // 服务中心地址（Service Center Address）
            public String OA;           // 发送方地址（Originator Adress）            
            public DateTime SCTS;       // 服务中心的时间戳（Service Center Time Stamp）
            public PDUUDH[] UDH;        // 用户数据头（User Data Header）
            public Object UD;           // 用户数据（User Data）

            // PDU Type 协议数据单元类型
            public Boolean RP;          // 应答路径（Reply Path）
            public Boolean UDHI;        // 用户数据头标识（User Data Header Indicator）
            public Boolean SRI;         // 状态报告指示（Status Report Indication）
            public Boolean MMS;         // 更多信息发送（More Messages to Send）
            public Int32 MTI;           // 信息类型指示（Message Type Indicator）

            // PID协议标识
            public Byte PID;            // PID协议标识（Protocol Identifier）

            // DCS数据编码方案
            public EnumDCS DCS;  // 数据编码方案（Data Coding Scheme）
            public Boolean TC;  // 文本压缩指示 0-文本未压缩 1-文本用GSM标准压缩算法压缩
            public Int32 MC;    // 消息类型（Message Class）-1：无 0：立即显示 1：移动设备特定类型 2：SIM特定类型 3：终端设备特定类型
        }

        /// <summary>
        /// 短信解码
        /// </summary>
        /// <param name="data">数据报文</param>
        /// <returns>短信信息</returns>
        /// <remarks>
        /// 接收方PDU格式（SMS-DELIVER-PDU）
        /// SCA（Service Center Adress）：短信中心，长度1-12
        /// PDU-Type（Protocol Data Unit Type）：协议数据单元类型，长度1
        /// OA（Originator Adress）：发送方SME的地址
        /// PID（Protocol Identifier）：协议标识，长度1
        /// DCS（Data Coding Scheme）：编码方案，长度1
        /// SCTS（Service Center Time Stamp）：服务中心时间戳，长度7
        /// UDL（User Data Length）：用户数据段长度，长度1
        /// UD（User Data）：用户数据，长度0-140
        /// </remarks>
        public static SMSPARTS PDUDecoding(String data)
        {
            SMSPARTS Parts;
            Int32 EndIndex;

            // 短信中心
            Parts.SCA = SCADecoding(data, out EndIndex);

            // 协议数据单元类型
            Int32 PDUType = Convert.ToInt32(data.Substring(EndIndex, 2), 16);
            EndIndex += 2;
            Parts.RP = PDUType.BitTest(7);      // 应答路径
            Parts.UDHI = PDUType.BitTest(6);    // 用户数据头标识
            Parts.SRI = PDUType.BitTest(5);     // 状态报告指示
            Parts.MMS = !PDUType.BitTest(2);    // 更多信息发送  
            Parts.MTI = PDUType & 3;            // 信息类型指示

            // 发送方SME的地址
            Parts.OA = OADecoding(data, EndIndex, out EndIndex);

            // 协议标识
            Parts.PID = Convert.ToByte(data.Substring(EndIndex, 2), 16);
            EndIndex += 2;

            // 数据编码方案
            Int32 DCSType = Convert.ToInt32(data.Substring(EndIndex, 2), 16);
            EndIndex += 2;
            Parts.TC = DCSType.BitTest(5);  // 文本压缩指示
            Parts.DCS = (EnumDCS)((DCSType >> 2) & 3);  // 编码字符集
            if (DCSType.BitTest(4))
            {   // 信息类型信息 0：立即显示 1：移动设备特定类型 2：SIM特定类型 3：终端设备特定类型
                Parts.MC = DCSType & 3;
            }
            else
            {   // 不含信息类型信息
                Parts.MC = -1;
            }

            // 服务中心时间戳（BCD编码）
            Parts.SCTS = SCTSDecoding(data, EndIndex);
            EndIndex += 14;

            // 用户数据头
            if (Parts.UDHI)
            {
                Parts.UDH = UDHDecoding(data, EndIndex + 2);
            }
            else
            {
                Parts.UDH = null;
            }

            // 用户数据
            Parts.UD = UserDataDecoding(data, EndIndex, Parts.UDHI, Parts.DCS);

            return Parts;
        }

        /// <summary>
        /// 用户数据解码
        /// </summary>
        /// <param name="data">编码字符串</param>
        /// <param name="Index">起始索引号</param>
        /// <param name="UDHI">用户数据头标识</param>
        /// <param name="DCS">编码方案</param>
        /// <returns>
        /// String类型：文本内容
        /// Byte[]类型：二进制内容
        /// </returns>
        private static Object UserDataDecoding(String data, Int32 Index, Boolean UDHI = false, EnumDCS DCS = EnumDCS.UCS2)
        {
            // 用户数据区长度
            Int32 UDL = Convert.ToInt32(data.Substring(Index, 2), 16);
            Index += 2;

            // 跳过用户数据头
            Int32 UDHL = 0;
            if (UDHI)
            {   // 用户数据头长度
                UDHL = Convert.ToInt32(data.Substring(Index, 2), 16);
                UDHL++;
                Index += UDHL << 1;                
            }

            // 获取用户数据
            if (DCS == EnumDCS.UCS2)
            {   // 获取字符个数
                Int32 CharNumber = (UDL - UDHL) >> 1;
                StringBuilder sb = new StringBuilder(CharNumber);                
                for (Int32 i = 0; i < CharNumber; i++)
                {
                    sb.Append(Convert.ToChar(Convert.ToInt32(data.Substring((i << 2) + Index, 4), 16)));
                }

                return sb;  
            }
            else if (DCS == EnumDCS.BIT7)
            {
                Int32 Septets = UDL - (UDHL * 8 + 6) / 7;   // 7-Bit编码字符数
                Int32 FillBits = (UDHL * 8 + 6) / 7 * 7 - UDHL * 8; // 填充位数
                return BIT7Decoding(BIT7Unpack(data, Index, Septets, FillBits));
            }
            else
            {   // 8Bit编码
                // 获取数据长度
                UDL -= UDHL;
                Byte[] Binary = new Byte[UDL];
                for (Int32 i = 0; i < UDL; i++)
                {
                    Binary[i] = Convert.ToByte(data.Substring((i << 1) + Index, 2), 16);
                }

                return Binary;
            }
        }

        /// <summary>
        /// 服务中心地址解码
        /// </summary>
        /// <param name="data">编码字符串</param>
        /// <param name="EndIndex">输出：结束索引位置</param>
        /// <returns>服务中心地址</returns>
        private static String SCADecoding(String data, out Int32 EndIndex)
        {
            // 获取地址长度
            Int32 Len = Convert.ToInt32(data.Substring(0, 2), 16);
            if (Len == 0)
            {
                EndIndex = 2;
                return String.Empty;
            }
            
            StringBuilder sb = new StringBuilder(Len << 1);

            // 服务中心地址类型
            if (data.Substring(2, 2) == "91")
            {   // 国际号码
                sb.Append("+");
            }

            // 服务中心地址
            EndIndex = (Len + 1) << 1;
            for (Int32 i = 4; i < EndIndex; i += 2)
            {
                sb.Append(data[i + 1]);
                sb.Append(data[i]);
            }

            // 去掉填充字符
            if (sb[sb.Length - 1] == 'F')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 发送方地址解码
        /// </summary>
        /// <param name="data">编码字符串</param>
        /// <param name="Index">起始索引位置</param>
        /// <param name="EndIndex">输出：结束索引位置</param>
        /// <returns>发送方地址</returns>
        private static String OADecoding(String data, Int32 Index, out Int32 EndIndex)
        {
            // 获取号码长度
            Int32 Len = Convert.ToInt32(data.Substring(Index, 2), 16);
            if (Len == 0)
            {
                EndIndex = Index + 2;
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder(Len + 1);
            if (data.Substring(Index + 2, 2) == "91")
            {   // 国际号码
                sb.Append("+");
            }
            
            // 电话号码
            for (Int32 i = 0; i < Len; i += 2)
            {
                sb.Append(data[Index + i + 5]);
                sb.Append(data[Index + i + 4]);
            }

            EndIndex = Index + 4 + Len;
            if (Len % 2 != 0)
            {   // 去掉填充字符
                sb.Remove(sb.Length - 1, 1);
                EndIndex++;
            }

            return sb.ToString();
        }
        
        /// <summary>
        /// 7-Bit编码解压缩
        /// </summary>
        /// <param name="data">短信数据</param>
        /// <param name="Index">起始索引号</param>
        /// <param name="Septets">7-Bit编码字符数</param>
        /// <param name="FillBits">填充Bit位数</param>
        /// <returns>7-Bit字节序列</returns>
        private static Byte[] BIT7Unpack(String data, Int32 Index, Int32 Septets, Int32 FillBits)
        {
            Byte[] Bit7Array = new Byte[Septets];

            // 每8个7-Bit编码字符存放到7个字节
            Int32 PackLen = (Septets * 7 + FillBits + 7) / 8;
            Int32 n = 0;
            Int32 Remainder = 0;            
            for (Int32 i = 0; i < PackLen; i++)
            {
                Int32 Order = (i + (7 - FillBits)) % 7;
                Int32 Value = Convert.ToInt32(data.Substring((i << 1) + Index, 2), 16);
                if ((i != 0) || (FillBits == 0))
                {
                    Bit7Array[n++] = (Byte)(((Value << Order) + Remainder) & 0x7F);
                }
                
                Remainder = Value >> (7 - Order);
                if (Order == 6)
                {
                    if (n == Septets) break;    // 避免写入填充数据
                    Bit7Array[n++] = (Byte)Remainder;                    
                    Remainder = 0;
                }
            }

            return Bit7Array;
        }

        /// <summary>
        /// 转换GSM字符编码到Unicode编码
        /// </summary>
        /// <param name="Bit7Array">7-Bit编码字节序列</param>
        /// <returns>Unicode字符串</returns>
        private static String BIT7Decoding(Byte[] Bit7Array)
        {
            StringBuilder sb = new StringBuilder(Bit7Array.Length);
            for (Int32 i = 0; i < Bit7Array.Length; i++)
            {
                UInt16 Key = Bit7Array[i];
                if (isBIT7Same(Key))
                {
                    sb.Append(Char.ConvertFromUtf32(Key));
                }
                else if (BIT7ToUCS2.ContainsKey(Key))
                {
                    if (Key == 0x1B)    // 转义字符
                    {   
                        if (i < Bit7Array.Length - 1 && BIT7EToUCS2.ContainsKey(Bit7Array[i + 1]))
                        {
                            sb.Append(Char.ConvertFromUtf32(BIT7EToUCS2[Bit7Array[i + 1]]));
                            i++;
                        }
                        else
                        {
                            sb.Append(Char.ConvertFromUtf32(BIT7ToUCS2[Key]));
                        }
                    }
                    else
                    {
                        sb.Append(Char.ConvertFromUtf32(BIT7ToUCS2[Key]));
                    }                        
                }
                else
                {   // 异常数据
                    sb.Append('?');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// BCD解码
        /// </summary>
        /// <param name="data">数据字符串</param>
        /// <param name="Index">起始索引号</param>
        /// <param name="isEnableMSB">最高位是否为符号位</param>
        /// <returns>转化后的十进制数</returns>
        private static Int32 BCDDecoding(String data, Int32 Index, Boolean isEnableMSB = false)
        {
            Int32 n = Convert.ToInt32(data.Substring(Index, 1));    // 个位
            Int32 m = Convert.ToInt32(data.Substring(Index + 1, 1), 16);    // 十位
            if (isEnableMSB)
            {   // 最高位为符号位，值的范围为-79～+79
                if (m >= 8)
                    return -((m - 8) * 10 + n); // 负值
                else
                    return m * 10 + n; 
            }
            else
            {   // 值的范围为0～99
                return m * 10 + n;
            }
        }
        
        /// <summary>
        /// 服务中心时间戳解码
        /// </summary>
        /// <param name="data">数据报文</param>
        /// <param name="Index">起始索引号</param>
        /// <returns>服务中心时间戳对应的本地时间</returns>
        private static DateTime SCTSDecoding(String data, Int32 Index)
        {   // 时区信息，其值为15分钟的倍数
            return new DateTimeOffset(
                (DateTime.Today.Year / 100 * 100) + BCDDecoding(data, Index),   // 年
                BCDDecoding(data, Index + 2),   // 月
                BCDDecoding(data, Index + 4),   // 日
                BCDDecoding(data, Index + 6),   // 时
                BCDDecoding(data, Index + 8),   // 分
                BCDDecoding(data, Index + 10),  // 秒
                new TimeSpan(0, BCDDecoding(data, Index + 12, true) * 15, 0)).LocalDateTime;
        }   
        
        /// <summary>
        /// 用户数据头解码
        /// </summary>
        /// <param name="data">数据报文</param>
        /// <param name="Index">起始索引号</param>
        /// <returns>解码后的用户数据头</returns>  
        /// <remarks>
        /// 信息元素标识
        /// 00  Concatenated short messages, 8-bit reference number
        /// 01  Special SMS Message Indication
        /// 02  Reserved
        /// 03  Value not used to avoid misinterpretation as &ltLF&gt character
        /// 04  Application port addressing scheme, 8 bit address
        /// 05  Application port addressing scheme, 16 bit address
        /// 06  SMSC Control Parameters
        /// 07  UDH Source Indicator
        /// 08  Concatenated short message, 16-bit reference number
        /// 09  Wireless Control Message Protocol
        /// 0A-6F   Reserved for future use
        /// 70-7F   SIM Toolkit Security Headers
        /// 80-9F   SME to SME specific use
        /// A0-BF   Reserved for future use
        /// C0-DF   SC specific use
        /// E0-FF   Reserved for future use
        /// </remarks>
        private static PDUUDH[] UDHDecoding(String data, Int32 Index)
        {   
            List<PDUUDH> UDH = new List<PDUUDH>();

            // 用户数据头长度
            Int32 UDHL = Convert.ToInt32(data.Substring(Index, 2), 16);
            Index += 2;

            Int32 i = 0;
            while (i < UDHL)
            {   // 信息元素标识（Information Element Identifier）
                Byte IEI = Convert.ToByte(data.Substring(Index, 2), 16);
                Index += 2;

                // 信息元素数据长度（Length of Information Element）
                Int32 IEDL = Convert.ToInt32(data.Substring(Index, 2), 16);
                Index += 2;

                // 信息元素数据（Information Element Data）
                Byte[] IED = new Byte[IEDL];
                for (Int32 j = 0; j < IEDL; j++)
                {
                    IED[j] = Convert.ToByte(data.Substring(Index, 2), 16);
                    Index += 2;
                }

                UDH.Add(new PDUUDH { IEI = IEI, IED = IED });
                i += IEDL + 2;
            }

            return UDH.ToArray();
        }        
    }
}
