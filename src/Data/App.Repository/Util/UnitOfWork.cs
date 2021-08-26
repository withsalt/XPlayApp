using App.IRepository.Interface;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Repository.Util
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly SqlSugarClient _db;

        public UnitOfWork(SqlSugarClient db)
        {
            _db = db;
        }

        public SqlSugarClient GetDbClient()
        {
            return _db;
        }

        public void BeginTran()
        {
            _db.BeginTran();
        }

        public void CommitTran()
        {
            try
            {
                _db.CommitTran(); //
            }
            catch (Exception)
            {
                _db.RollbackTran();
                throw;
            }
        }

        public void RollbackTran()
        {
            _db.RollbackTran();
        }
    }
}
