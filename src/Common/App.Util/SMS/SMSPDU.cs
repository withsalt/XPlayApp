/* ----------------------------------------------------------
文件名称：SMSPDU.cs

作者：秦建辉

MSN：splashcn@msn.com
QQ：36748897

博客：http://blog.csdn.net/jhqin

开发环境：
    Visual Studio V2010
    .NET Framework 4 Client Profile

版本历史：    
    V1.0	2011年08月19日
			短信编码参数设置部分
------------------------------------------------------------ */
using System;
using System.Text;

namespace App.Util.SMS
{
    /// <summary>
    /// 短信编码参数设置部分
    ///     mCSMIEI：长短信信息元素参考号（类型：enum）
    ///     mSCA：服务中心地址（类型：String）
    ///     mSRR：请求状态报告（类型：Boolean）
    ///     mRD：拒绝复本（类型：Boolean）
    ///     mVP：短信有效期（类型：Object，接受TimeSpan或者DateTime类型）
    /// </summary>
    public partial class SMS
    {
        /// <summary>
        /// 长短信信息元素参考号枚举类型
        ///     BIT8：8-Bit参考号
        ///     BIT16：16-Bit参考号
        /// </summary>
        public enum EnumCSMIEI 
        { 
            BIT8 = 0, 
            BIT16 = 8 
        };

        /// <summary>
        /// 长短信信息元素参考号，默认为8-Bit编码
        /// </summary>
        public EnumCSMIEI mCSMIEI { get; set; }  = EnumCSMIEI.BIT8;

        /// <summary>
        /// 服务中心地址（Service Center Address）
        /// </summary>
        private String _mSCA = null;

        public String mSCA
        {
            get
            {
                return _mSCA;
            }
            set
            {   // 国际号码、国内号码、固定电话、小灵通
                _mSCA = value;
            }
        }

        /// <summary>
        /// 请求状态报告（Status Report Request）
        /// </summary>
        public Boolean mSRR { get; set; } =  false;

        /// <summary>
        /// 拒绝复本（Reject Duplicate）
        /// </summary>
        public Boolean mRD { get; set; } = false;

        /// <summary>
        /// 短信有效期
        /// </summary>
        private String _mVP = String.Empty;   // 默认不提供VP段

        public Object mVP
        {
            get
            {
                if (_mVP.Length == 2)
                {   // 相对有效期
                    Int32 n = Convert.ToInt32(_mVP, 16);
                    if (n <= 0x8F)
                    {   // 00～8F (VP+1)*5分钟 从5分钟间隔到12个小时
                        return new TimeSpan(0, (n + 1) * 5, 0); // 时 分 秒
                    }
                    else if (n <= 0xA7)
                    {   // 90～A7 12小时+(VP-143)*30分钟
                        return new TimeSpan(12, (n - 143) * 30, 0); // 时 分 秒
                    }
                    else if (n <= 0xC4)
                    {   // A8～C4 (VP-166)*1天
                        return new TimeSpan(n - 166, 0, 0, 0);  // 天 时 分 秒
                    }
                    else
                    {   // C5～FF (VP-192)*1周
                        return new TimeSpan((n - 192) * 7, 0, 0, 0);    // 天 时 分 秒
                    }
                }
                else if (_mVP.Length == 14)
                {   // 绝对有效期
                    return SCTSDecoding(_mVP, 0);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {   
                    // 不提供VP段
                    _mVP = String.Empty;
                }
                else if (value is TimeSpan)
                {   
                    // 相对有效期
                    Int32 Days = ((TimeSpan)value).Days;
                    if (Days >= 2)
                    {
                        if (Days >= 35)
                        {   // C5～FF 5周～63周
                            _mVP = Math.Min(Days / 7 + 192, 255).ToString("X2");
                        }
                        else
                        {   // A8～C4 2天～30天
                            _mVP = Math.Min(Days + 166, 196).ToString("X2");
                        }
                    }
                    else
                    {
                        Int32 TotalMinutes = Math.Max(5, (Int32)((TimeSpan)value).TotalMinutes);
                        if (TotalMinutes >= 750)
                        {   // 90～A7 12小时30分钟～24小时
                            _mVP = Math.Min((TotalMinutes - 720) / 30 + 143, 167).ToString("X2");
                        }
                        else
                        {   // 00～8F 5分钟～12小时
                            _mVP = Math.Min(TotalMinutes / 5 - 1, 143).ToString("X2");
                        }
                    }
                }
                else if (value is DateTime)
                {   
                    // 绝对有效期                    
                    // 调整为本地时间
                    DateTime dt;
                    if (((DateTime)value).Kind == DateTimeKind.Utc)
                    {
                        dt = ((DateTime)value).ToLocalTime();
                    }
                    else if (((DateTime)value).Kind == DateTimeKind.Unspecified)
                    {
                        dt = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Local);
                    }
                    else
                    {
                        dt = (DateTime)value;
                    }

                    StringBuilder sb = new StringBuilder(14);
                    sb.Append(BCDEncoding(dt.Year % 100));  // 年
                    sb.Append(BCDEncoding(dt.Month));       // 月
                    sb.Append(BCDEncoding(dt.Day));         // 日
                    sb.Append(BCDEncoding(dt.Hour));        // 时
                    sb.Append(BCDEncoding(dt.Minute));      // 分
                    sb.Append(BCDEncoding(dt.Second));      // 秒

                    // 时区（-14小时～+14小时），度量范围为-56～+56
                    sb.Append(BCDEncoding((Int32)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes / 15));

                    _mVP = sb.ToString();
                }
                else
                {
                    _mVP = String.Empty;
                }
            }
        }

        /// <summary>
        /// 长短信信息元素消息参考号
        /// </summary>
        private static Int32 _mCSMMR = 0;
    }
}
