using App.Util.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace App.Util.Api.Msg
{
    public class SendMsg
    {
        public string MessageApi { get; set; }

        public string MessageCert { get; set; }

        public SendMsg(string msgApi, string msgCert)
        {
            this.MessageApi = msgApi;
            this.MessageCert = msgCert;
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="phone">电话</param>
        /// <param name="msg">要发送的消息</param>
        /// <param name="err">out err</param>
        /// <returns>
        /// 发送成功 返回0 ，并返回ok
        /// 发送失败返回错误码，并返回错误消息
        /// </returns>
        public async Task<(bool, string)> Send(string msg, string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(msg))
                {
                    return (false, "Send message is null");
                }
                if (string.IsNullOrEmpty(phone))
                {
                    return (false, "Receive user phone is null");
                }
                HttpUtil httpHelper = new HttpUtil();
                HttpItem requestItem = new HttpItem()
                {
                    Method = Method.GET,
                    URL = CreateRequestApi(MessageCert, phone, CreateMsgContent(msg))
                };

                HttpResult result = await httpHelper.Request(requestItem);
                if (result == null || result.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(result.Html))
                {
                    return (false, "Msg send result is null");
                }
                else
                {
                    try
                    {
                        JObject jo = JObject.Parse(result.Html);
                        if (jo != null)
                        {
                            string resultCode = jo["status"].ToString();
                            string resultMsg = jo["msg"].ToString();
                            if (resultCode == "0")
                            {
                                return (true, "success");
                            }
                            else
                            {
                                return (false, $"Result code is {resultCode},result msg is {resultMsg}");
                            }
                        }
                        else
                        {
                            return (false, "Can not parse result msg");
                        }
                    }
                    catch (Exception ex)
                    {
                        return (false, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public bool Send(string msg, string[] phone, out string err)
        {
            err = string.Empty;
            if (phone.Length == 0)
            {
                err = "Phones count is zero";
                return false;
            }
            if (string.IsNullOrEmpty(msg))
            {
                err = "Send msg is null";
                return false;
            }
            foreach (var item in phone)
            {
                Send(msg, phone, out err);
            }
            return true;
        }

        private string CreateRequestApi(string cert, string phone, string content)
        {
            try
            {
                if (string.IsNullOrEmpty(cert) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(content))
                {
                    return null;
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(MessageApi);
                sb.AppendFormat("?mobile={0}&content={1}&appkey={2}", phone, HttpUtil.UrlEncode(content), cert);
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string CreateMsgContent(string msg)
        {
            try
            {
                if (string.IsNullOrEmpty(msg))
                {
                    return null;
                }
                return msg;
            }
            catch
            {
                return null;
            }
        }
    }
}
