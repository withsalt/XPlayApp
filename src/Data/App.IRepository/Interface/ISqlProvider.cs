using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace App.IRepository.Interface
{
    public interface ISqlProvider
    {
        Task<int> ExecuteNonQuery(string cmdText, params DbParameter[] commandParameters);

        Task<int> ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);

        Task<int> ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);

        Task<int> ExecuteNonQuery(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);

        Task<DbDataReader> ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);

        DataSet GetDataSet(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);

        Task<object> ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);

        Task<object> ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);

        void CacheParameters(string cacheKey, params DbParameter[] commandParameters);

        DbParameter[] GetCachedParameters(string cacheKey);
    }
}
