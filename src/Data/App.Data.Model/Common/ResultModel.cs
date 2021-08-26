using App.Data.Model.Common.JsonObject;
using App.Util.Date;

namespace App.Data.Model.Common
{
    public class ResultModel<T> : IRoot<T> where T : IChild
    {
        public ResultModel()
        {

        }

        public ResultModel(int code)
        {
            this.Code = code;
            if (code == 0)
            {
                this.Message = "Success";
            }
        }

        public ResultModel(int code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        public int Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public long Time
        {
            get
            {
                return TimeUtil.Timestamp();
            }
        }
    }
}
