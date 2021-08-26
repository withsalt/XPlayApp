using App.IServices.Interface;
using App.Data.Entity.User;
using App.Data.Model.Response.User;
using System.Threading.Tasks;

namespace App.IServices.User
{
    public interface IUserInfoService : IBaseServices<UserInfo>
    {
        Task<(int, UserInfoModel)> Login(string loginInfo, string passwd);

        Task<UserInfoModel> LoginByStateCookie(string uid, string Md5Passwd, string token);

        Task<UserInfoModel> LoginByUserCookie(string uid, string Md5Passwd);

        Task<bool> ValidateToken(string uid, string token);
    }
}
