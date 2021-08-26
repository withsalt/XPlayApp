using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace App.Config
{
    public class ConnectionStringsNode
    {
        private string _sqliteDbConnectionString = null;

        /// <summary>
        /// PostgreSql
        /// </summary>
        public string SqliteDbConnectionString
        {
            get
            {
                return _sqliteDbConnectionString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("SqliteDbConnectionString can not null");
                }
                string connStr = value;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    connStr = connStr.Replace("/", "\\");
                }
                else
                {
                    connStr = connStr.Replace("\\", "/");
                }

                if (connStr.Contains("{root}", StringComparison.OrdinalIgnoreCase))
                {
                    _sqliteDbConnectionString = connStr.Replace("{root}", AppContext.BaseDirectory);
                }
                else
                {
                    _sqliteDbConnectionString = connStr;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string MysqlDbConnectionString { get; set; }

        /// <summary>
        /// PostgreSql
        /// </summary>
        public string NpgsqlDbConnectionString { get; set; }
    }
}
