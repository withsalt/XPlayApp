using App.IRepository.Interface;
using App.Data.Entity.User;
using App.Data.Model.Response.User;
using System.Threading.Tasks;

namespace App.IRepository.User
{
    public interface IUserInfoRepository : IBaseRepository<UserInfo>
    {
        Task<UserInfo> Login(string loginInfo, string passwd);

        Task<UserInfo> LoginByStateCookie(string uid, string Md5Passwd, string token);

        Task<UserInfo> LoginByUserCookie(string uid, string Md5Passwd);

        Task<bool> SetUserToken(string uid, string token);

        Task<bool> SetUserToken(UserTokenInfo tokenInfo);

        Task<UserTokenInfo> GetUserToken(string uid, string token);
    }
}
