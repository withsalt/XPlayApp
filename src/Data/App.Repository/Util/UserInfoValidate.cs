using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace App.Repository.Util
{
    public class UserInfoValidate
    {
        /// <summary>
        /// 判断是否为合法的电子邮件
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsEmail(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                return false;
            }
            return Match(emailAddress, "^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
        }

        /// <summary>
        /// 判断输入的字符串是否是一个合法的手机号
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsMobilePhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return false;
            }
            return Match(phone, "^1[345789]\\d{9}$");
        }

        public static bool IsUserId(string uid, List<int> uidLengths)
        {
            if (string.IsNullOrEmpty(uid))
            {
                return false;
            }
            if (!uidLengths.Contains(uid.Length))
            {
                return false;
            }
            for (int i = 0; i < uid.Length; i++)
            {
                if (uid[i] > 57 || uid[i] < 48)
                {
                    return false;
                }
            }
            return true;
        }

        //public static bool UserNameValidate(string uname, List<BanUserRules> banUserRules)
        //{
        //    if (string.IsNullOrEmpty(uname))
        //    {
        //        return false;
        //    }
        //    bool result = Match(uname, @"^[a-zA-Z]\w{3,15}$");
        //    if (!result)
        //    {
        //        return false;
        //    }
        //    if(banUserRules == null || banUserRules.Count == 0)
        //    {
        //        return true;
        //    }
        //    if(banUserRules.Where(ur=>ur.Name.Equals(uname,StringComparison.OrdinalIgnoreCase)).Count() > 0)
        //    {
        //        return false;
        //    }
        //    return result;
        //}


        private static bool Match(string input, string match)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            Regex r = new Regex(match);
            return r.IsMatch(input);
        }
    }
}
