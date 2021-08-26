using App.IRepository.Interface;
using App.Data.Entity.User;
using System.Threading.Tasks;
using App.Data.Model.Request.Password;

namespace App.IRepository.User
{
    public interface IUserPasswdRepository : IBaseRepository<UserPasswd>
    {
        Task<bool> ResetPassword(ResetPasswordParams param);
        Task<bool> CheckOldPassword(string uid, string passwd);
    }
}
