using App.Util.Date;
using App.Util.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace App.Util.User
{
    public class UserUtil
    {
        public static string PasswdAddSalt(string pwd, string salt)
        {
            if (string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(salt))
            {
                throw new Exception("Passwd or salt can not null.");
            }
            string newPwd = string.Format(salt, "I dont know what this", pwd);
            return Encrypt.MDString(newPwd);
        }

        /// <summary>
        /// 创建token
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string GenerateToken(string username)
        {
            string tokenKey = "yrzaiwt";
            //生成规则
            string tokenStr = username + "_" + TimeUtil.Timestamp() + "_" + tokenKey;
            return Encrypt.MDString(tokenStr);
        }
    }
}
