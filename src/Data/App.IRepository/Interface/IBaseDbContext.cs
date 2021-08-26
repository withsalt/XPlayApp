using App.Data.Entity.Interface;
using SqlSugar;

namespace App.IRepository.Interface
{
    public interface IBaseDbContext
    {
        SqlSugarClient Context { get;  }

        IBaseRepository<T> GetRepository<T>() where T : class, IEntity, new();
    }
}
