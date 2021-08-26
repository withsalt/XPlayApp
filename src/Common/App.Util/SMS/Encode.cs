/* ----------------------------------------------------------
文件名称：Encode.cs

作者：秦建辉

MSN：splashcn@msn.com
QQ：36748897

博客：http://blog.csdn.net/jhqin

开发环境：
    Visual Studio V2010
    .NET Framework 4 Client Profile

版本历史：    
    V1.0	2011年08月19日
			PDU格式短信编码部分（不支持文本压缩短信）
------------------------------------------------------------ */
using System;
using System.Text;
using System.Collections.Generic;

namespace App.Util.SMS
{

    public class EncodeResult
    {
        public string Data { get; set; }

        public int Length { get; set; }
    }

    /// <summary>
    /// PDU格式短信编码部分
    /// 接口函数：PDUEncoding
    /// 注意：不支持文本压缩短信
    /// </summary>
    public partial class SMS
    {
        /// <summary>
        /// 数据编码方案（Data Coding Scheme）
        /// </summary>        
        public enum EnumDCS
        {
            BIT7 = 0,   // 采用GSM字符集
            BIT8 = 1,   // 采用ASCII字符集
            UCS2 = 2    // 采用Unicode字符集
        }

        /// <summary>
        /// 长短信编码
        /// </summary>
        /// <param name="DA">接收方地址</param>
        /// <param name="UDC">用户数据内容</param>
        /// <param name="UDH">用户数据头</param>
        /// <returns>长短信编码序列</returns>
        /// <remarks>
        ///     长短信自动拆分
        ///     自动确定最佳编码
        /// </remarks>
        public List<EncodeResult> PDUEncoding(String DA, Object UDC, String SCA = null, PDUUDH[] UDH = null)
        {
            // 确定编码方案
            EnumDCS DCS;
            if (UDC is String)
            {
                if (IsGSMString(UDC as String))
                    DCS = EnumDCS.BIT7;
                else
                    DCS = EnumDCS.UCS2;
            }
            else
            {
                DCS = EnumDCS.BIT8;
            }
            return PDUEncoding(DA, UDC, SCA, DCS, UDH);
        }

        /// <summary>
        /// 长短信编码
        /// </summary>
        /// <param name="SCA">服务中心地址</param>
        /// <param name="DA">接收方地址</param>
        /// <param name="UDC">用户数据内容</param>
        /// <param name="UDH">用户数据头</param>
        /// <param name="DCS">编码方案</param>
        /// <returns>长短信编码序列</returns>
        /// <remarks>长短信自动拆分</remarks>
        public List<EncodeResult> PDUEncoding(String DA, Object UDC, String SCA, EnumDCS DCS, PDUUDH[] UDH = null)
        {
            // 短信拆分
            if (UDC is String)
            {
                List<String> CSMUDC = UDCSplit(UDC as String, UDH, DCS);
                if (CSMUDC == null) return null;

                if (CSMUDC.Count > 1)
                {   // 长短信
                    int CSMMR = _mCSMMR;
                    if (++_mCSMMR > 0xFFFF) _mCSMMR = 0;

                    // 生成短信编码序列
                    List<EncodeResult> CSMSeries = new List<EncodeResult>(CSMUDC.Count);

                    for (int i = 0; i < CSMUDC.Count; i++)
                    {   // 更新用户数据头
                        PDUUDH[] CSMUDH = UpdateUDH(UDH, CSMMR, CSMUDC.Count, i);
                        var result = SoloPDUEncoding(SCA, DA, CSMUDC[i], CSMUDH, DCS);
                        CSMSeries.Add(new EncodeResult()
                        {
                            Data = result.Item1,
                            Length = result.Item2
                        });
                    }
                    return CSMSeries;
                }
                else
                {
                    // 单条短信
                    var result = SoloPDUEncoding(SCA, DA, UDC, UDH, DCS);
                    return new List<EncodeResult>()
                    {
                        new EncodeResult()
                        {
                            Data = result.Item1,
                            Length = result.Item2
                        }
                    };
                }
            }
            else if (UDC is Byte[])
            {
                List<Byte[]> CSMUDC = UDCSplit(UDC as Byte[], UDH);
                if (CSMUDC == null) return null;

                if (CSMUDC.Count > 1)
                {   // 长短信
                    int CSMMR = _mCSMMR;
                    if (++_mCSMMR > 0xFFFF) _mCSMMR = 0;

                    // 生成短信编码序列
                    List<EncodeResult> CSMSeries = new List<EncodeResult>(CSMUDC.Count);
                    for (int i = 0; i < CSMUDC.Count; i++)
                    {   // 更新用户数据头
                        PDUUDH[] CSMUDH = UpdateUDH(UDH, CSMMR, CSMUDC.Count, i);
                        var result = SoloPDUEncoding(SCA, DA, CSMUDC[i], CSMUDH, DCS);
                        CSMSeries.Add(new EncodeResult()
                        {
                            Data = result.Item1,
                            Length = result.Item2
                        });
                    }

                    return CSMSeries;
                }
                else
                {   // 单条短信
                    var result = SoloPDUEncoding(SCA, DA, UDC, UDH, DCS);
                    return new List<EncodeResult>()
                    {
                        new EncodeResult()
                        {
                            Data = result.Item1,
                            Length = result.Item2
                        }
                    };
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 用户数据编码
        /// </summary>
        /// <param name="UDC">短信内容</param>
        /// <param name="UDH">用户数据头</param>
        /// <param name="DCS">编码方案</param>
        /// <returns>编码后的字符串</returns>
        /// <remarks>
        /// L：用户数据长度，长度1
        /// M：用户数据，长度0～140
        /// </remarks>
        private static string UDEncoding(Object UDC, PDUUDH[] UDH = null, EnumDCS DCS = EnumDCS.UCS2)
        {
            // 用户数据头编码
            int UDHL;
            String Header = UDHEncoding(UDH, out UDHL);

            // 用户数据内容编码
            int UDCL;
            String Body;
            if (UDC is String)
            {   // 7-Bit编码或UCS2编码
                Body = UDCEncoding(UDC as String, out UDCL, UDHL, DCS);
            }
            else
            {   // 8-Bit编码
                Body = UDCEncoding(UDC as Byte[], out UDCL);
            }

            // 用户数据区长度
            int UDL;
            if (DCS == EnumDCS.BIT7)
            {   // 7-Bit编码
                UDL = (UDHL * 8 + 6) / 7 + UDCL;    // 字符数
            }
            else
            {   // UCS2编码或者8-Bit编码
                UDL = UDHL + UDCL;                  // 字节数
            }
            return UDL.ToString("X2") + Header + Body;
        }

        /// <summary>
        /// 用户数据头编码
        /// </summary>
        /// <param name="UDH">用户数据头</param>
        /// <param name="UDHL">输出：用户数据头字节数</param>
        /// <returns>用户数据头编码字符串</returns>
        private static String UDHEncoding(PDUUDH[] UDH, out int UDHL)
        {
            UDHL = 0;
            if (UDH == null || UDH.Length == 0) return String.Empty;

            foreach (PDUUDH IE in UDH)
            {
                UDHL += IE.IED.Length + 2;  // 信息元素标识+信息元素长度+信息元素数据
            }

            StringBuilder sb = new StringBuilder((UDHL + 1) << 1);
            sb.Append(UDHL.ToString("X2"));
            foreach (PDUUDH IE in UDH)
            {
                sb.Append(IE.IEI.ToString("X2"));           // 信息元素标识1字节
                sb.Append(IE.IED.Length.ToString("X2"));    // 信息元素长度1字节
                foreach (Byte b in IE.IED)
                {
                    sb.Append(b.ToString("X2"));            // 信息元素数据
                }
            }

            UDHL++; // 加上1字节的用户数据头长度
            return sb.ToString();
        }

        /// <summary>
        /// 用户数据内容编码
        /// </summary>
        /// <param name="UDC">用户数据内容</param>
        /// <param name="UDCL">输出：UCS2编码字节数或7-Bit编码字符数</param>
        /// <param name="UDHL">用户数据头长度，7-Bit编码时需要参考</param>
        /// <param name="DCS">编码方案</param>
        /// <returns>编码字符串</returns>
        private static String UDCEncoding(String UDC, out int UDCL, int UDHL = 0, EnumDCS DCS = EnumDCS.UCS2)
        {
            if (String.IsNullOrEmpty(UDC))
            {
                UDCL = 0;
                return String.Empty;
            }

            if (DCS == EnumDCS.BIT7)
            {   // 7-Bit编码，需要参考用户数据头长度，已保证7-Bit边界对齐
                return BIT7Pack(BIT7Encoding(UDC, out UDCL), UDHL);
            }
            else
            {   // UCS2编码
                UDCL = UDC.Length << 1;     // 字节数
                StringBuilder sb = new StringBuilder(UDCL << 1);
                foreach (Char Letter in UDC)
                {
                    sb.Append(Convert.ToInt32(Letter).ToString("X4"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// 用户数据内容编码
        /// </summary>
        /// <param name="UDC">用户数据内容</param>
        /// <param name="UDCL">输出：编码字节数</param>
        /// <returns>编码字符串</returns>
        private static String UDCEncoding(Byte[] UDC, out int UDCL)
        {   // 8-Bit编码
            if (UDC == null || UDC.Length == 0)
            {
                UDCL = 0;
                return String.Empty;
            }

            UDCL = UDC.Length;
            StringBuilder sb = new StringBuilder(UDCL << 1);
            foreach (Byte b in UDC)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 7-Bit序列和Unicode编码是否相同
        /// </summary>
        /// <param name="UCS2">要检测的Unicode编码</param>
        /// <returns>
        /// 返回值：
        ///     true：编码一致
        ///     false：编码不一致
        /// </returns>
        private static Boolean isBIT7Same(UInt16 UCS2)
        {
            if (UCS2 >= 0x61 && UCS2 <= 0x7A ||
                UCS2 >= 0x41 && UCS2 <= 0x5A ||
                UCS2 >= 0x25 && UCS2 <= 0x3F ||
                UCS2 >= 0x20 && UCS2 <= 0x23 ||
                UCS2 == 0x0A ||
                UCS2 == 0x0D)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 将UCS2编码字符串转换成7-Bit编码字节 序列
        /// </summary>
        /// <param name="UDC">用户数据内容</param>
        /// <param name="Septets">7-Bit编码字符数</param>
        /// <returns>7-Bit编码字节序列</returns>
        private static Byte[] BIT7Encoding(String UDC, out int Septets)
        {
            Byte[] Bit7Array = new Byte[UDC.Length << 1];

            Septets = 0;
            foreach (Char Letter in UDC)
            {
                UInt16 Code = Convert.ToUInt16(Letter);
                if (isBIT7Same(Code))
                {   // 编码不变
                    Bit7Array[Septets++] = Convert.ToByte(Code);
                }
                else
                {
                    if (UCS2ToBIT7.ContainsKey(Code))
                    {
                        UInt16 Transcode = UCS2ToBIT7[Code];    // 转换码
                        if (Transcode > 0xFF)
                        {   // 转义序列
                            Bit7Array[Septets++] = Convert.ToByte(Transcode >> 8);
                            Bit7Array[Septets++] = Convert.ToByte(Transcode & 0xFF);
                        }
                        else
                        {
                            Bit7Array[Septets++] = Convert.ToByte(Transcode);
                        }
                    }
                    else
                    {   // 未知字符
                        Bit7Array[Septets++] = Convert.ToByte('?');
                    }
                }
            }

            // 重新调整大小
            Array.Resize(ref Bit7Array, Septets);
            return Bit7Array;
        }

        /// <summary>
        /// 7-Bit编码压缩
        /// </summary>
        /// <param name="Bit7Array">7-Bit编码字节序列</param>
        /// <param name="UDHL">用户数据头字节数</param>
        /// <returns>编码后的字符串</returns>
        private static String BIT7Pack(Byte[] Bit7Array, int UDHL)
        {
            // 7Bit对齐需要的填充位
            int FillBits = (UDHL * 8 + 6) / 7 * 7 - (UDHL * 8);

            // 压缩字节数
            int Len = Bit7Array.Length;
            int PackLen = (Len * 7 + FillBits + 7) / 8;
            StringBuilder sb = new StringBuilder(PackLen << 1);

            int Remainder = 0;
            for (int i = 0; i < Len; i++)
            {   // 每8个字节压缩成7个字节
                int CharValue = Bit7Array[i];
                int Index = (i + 8 - FillBits) % 8;
                if (Index == 0)
                {
                    Remainder = CharValue;
                }
                else
                {
                    int n = ((CharValue << (8 - Index)) | Remainder) & 0xFF;
                    sb.Append(n.ToString("X2"));
                    Remainder = CharValue >> Index;
                }
            }

            if (((Len * 7 + FillBits) % 8) != 0)
            {   // 写入剩余数据
                sb.Append(Remainder.ToString("X2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 服务中心地址编码（SCA = Service Center Adress）
        /// </summary>
        /// <param name="SCA">服务中心地址</param>
        /// <returns>编码后的字符串</returns>
        /// <remarks>
        /// SCA组成：1～12个八位位组
        /// A：服务中心地址长度，长度1，其值为B+C字节数
        /// B：服务中心地址类型，长度0～1
        /// C：服务中心地址，长度0～10。
        /// </remarks>
        private static String SCAEncoding(String SCA, out int SCAL)
        {
            SCAL = 0;
            if (String.IsNullOrEmpty(SCA))
            {
                // 表示使用SIM卡内部的设置值，该值通过AT+CSCA指令设置
                return "00";
            }

            StringBuilder sb = new StringBuilder(SCA.Length + 5);
            int Index = 0;
            if (SCA.StartsWith("+"))
            {
                // 国际号码
                sb.Append((SCA.Length / 2 + 1).ToString("X2"));         // SCA长度编码
                sb.Append("91");    // SCA类型编码
                Index = 1;
            }
            else
            {
                // 国内号码
                sb.Append(((SCA.Length + 1) / 2 + 1).ToString("X2"));   // SCA长度编码
                sb.Append("81");    // SCA类型编码
            }

            // SCA地址编码
            for (; Index < SCA.Length; Index += 2)
            {   // 号码部分奇偶位对调
                if (Index == SCA.Length - 1)
                {
                    sb.Append("F");     // 补“F”凑成偶数个
                    sb.Append(SCA[Index]);
                }
                else
                {
                    sb.Append(SCA[Index + 1]);
                    sb.Append(SCA[Index]);
                }
            }
            string result = sb.ToString();
            if (string.IsNullOrEmpty(result) || result.Length < 5)
            {
                return "00";
            }
            SCAL = Convert.ToInt32(result.Substring(0, 2), 16);
            return result;
        }

        /// <summary>
        /// 接收方地址编码
        /// </summary>
        /// <param name="DA">接收方地址</param>
        /// <returns>编码后的字符串</returns>
        /// <remarks>
        /// DA组成：2～12个八位位组
        /// F：地址长度，长度1。注意：其值是接收方地址长度，而非字节数
        /// G：地址类型，长度1，取值同B。
        /// H：地址，长度0～10。
        /// </remarks>
        private static string DAEncoding(string DA)
        {
            if (String.IsNullOrEmpty(DA))
            {
                // 地址长度0，地址类型未知
                return "0080";
            }

            StringBuilder sb = new StringBuilder(DA.Length + 5);
            int Index = 0;
            if (DA.StartsWith("+"))
            {
                // 国际号码
                sb.Append((DA.Length - 1).ToString("X2"));  // 地址长度编码
                sb.Append("91");    // 地址类型
                Index = 1;
            }
            else
            {
                // 国内号码
                sb.Append(DA.Length.ToString("X2"));        // 地址长度编码
                sb.Append("81");    // 地址类型
            }

            for (; Index < DA.Length; Index += 2)
            {
                // 号码部分奇偶位对调
                if (Index == DA.Length - 1)
                {
                    sb.Append("F");     // 补“F”凑成偶数个
                    sb.Append(DA[Index]);
                }
                else
                {
                    sb.Append(DA[Index + 1]);
                    sb.Append(DA[Index]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 协议数据单元类型编码
        /// </summary>
        /// <param name="UDHI">用户数据头标识</param>
        /// <returns>编码字符串</returns>
        private String PDUTypeEncoding(Boolean UDHI)
        {   // 信息类型指示（Message Type Indicator）
            int PDUType = 0x01;   // 01 SMS-SUBMIT（MS -> SMSC）

            // 用户数据头标识（User Data Header Indicator）
            if (UDHI)
            {
                PDUType |= 0x40;
            }

            // 请求状态报告（Status Report Request）
            if (mSRR)
            {
                PDUType |= 0x20;    // 请求状态报告
            }

            // 拒绝复本（Reject Duplicate）
            if (mRD)
            {
                PDUType |= 0x04;    // 拒绝复本
            }

            return PDUType.ToString("X2");
        }

        /// <summary>
        /// 消息参考编码（Message Reference）
        /// </summary>
        /// <returns>编码字符串</returns>
        private static String MREncoding()
        {   // 由手机设置
            return "00";
        }

        /// <summary>
        /// 协议标识（Protocol Identifier）
        /// </summary>
        /// <returns>编码字符串</returns>
        private static String PIDEncoding()
        {
            return "00";
        }

        /// <summary>
        /// 数据编码方案
        /// </summary>
        /// <param name="UDC">用户数据</param>
        /// <param name="DCS">编码字符集</param>
        /// <returns>编码字符串</returns>
        private static String DCSEncoding(Object UDC, EnumDCS DCS = EnumDCS.UCS2)
        {
            if (UDC is String)
            {
                if (DCS == EnumDCS.BIT7)
                {   // 7-Bit编码
                    return "00";
                }
                else
                {   // UCS2编码
                    return "08";
                }
            }
            else
            {   // 8-Bit编码
                return "04";
            }
        }

        /// <summary>
        /// 交换的BCD编码
        /// </summary>
        /// <param name="n">取值范围为-79～+79（MSB）或者0～99</param>
        /// <returns>编码后的字符串</returns>
        private static String BCDEncoding(int n)
        {   // n的取值范围为-79～+79（MSB）或者0～99
            if (n < 0) n = Math.Abs(n) + 80;
            return (n % 10).ToString("X") + (n / 10).ToString("X");
        }

        /// <summary>
        /// 单条短信编码
        /// </summary>
        /// <param name="SCA">服务中心地址，如果为null，则表示使用SIM卡设置</param>
        /// <param name="DA">接收方地址</param>
        /// <param name="UDC">用户数据内容</param>
        /// <param name="UDH">用户数据头</param>
        /// <param name="DCS">编码方案</param>
        /// <returns>编码后的字符串</returns>
        /// <remarks>
        /// 发送方PDU格式（SMS-SUBMIT-PDU）
        /// SCA（Service Center Adress）：短信中心，长度1-12
        /// PDU-Type（Protocol Data Unit Type）：协议数据单元类型，长度1
        /// MR（Message Reference）：消息参考值，为0～255。长度1
        /// DA（Destination Adress）：接收方SME的地址，长度2-12
        /// PID（Protocol Identifier）：协议标识，长度1
        /// DCS（Data Coding Scheme）：编码方案，长度1
        /// UDL（User Data Length）：用户数据段长度，长度1
        /// UD（User Data）：用户数据，长度0-140
        /// </remarks>
        private (string, int) SoloPDUEncoding(String SCA, String DA, Object UDC, PDUUDH[] UDH = null, EnumDCS DCS = EnumDCS.UCS2)
        {
            StringBuilder sb = new StringBuilder();
            // 短信中心
            sb.Append(SCAEncoding(SCA, out int SCAL));
            // 协议数据单元类型
            if (UDH == null || UDH.Length == 0)
                sb.Append(PDUTypeEncoding(false));
            else
                sb.Append(PDUTypeEncoding(true));
            // 消息参考值
            sb.Append(MREncoding());
            // 接收方SME地址
            sb.Append(DAEncoding(DA));
            // 协议标识
            sb.Append(PIDEncoding());
            // 有效期
            sb.Append(mVP);
            // 编码方案
            sb.Append(DCSEncoding(UDC, DCS));
            // 用户数据长度及内容
            sb.Append(UDEncoding(UDC as String, UDH, DCS));

            return (sb.ToString(), (sb.Length / 2 - SCAL - 1));
        }
    }
}
