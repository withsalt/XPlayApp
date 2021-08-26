using App.IServices.Interface;
using App.Data.Entity.User;
using System.Threading.Tasks;
using App.Data.Model.Request.Password;

namespace App.IServices.User
{
    public interface IUserPasswdService : IBaseServices<UserPasswd>
    {
        Task<(bool, int)> ResetPassword(ResetPasswordParams param);
    }
}
