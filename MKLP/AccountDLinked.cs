using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;

namespace MKLP
{
    public class AccountDLinked
    {
        private Config Config = Config.Read();
        private IDbConnection _db;

        private string TableName;
        private string Get_AccountName_DB;
        private string Get_AccountID_DB;
        private string Get_UserID_DB;

        private string Custom_Get_AccountName_From_UserID;
        private string Custom_Get_UserID_From_AccountName;
        private string Custom_Get_UserID_From_AccountID;

        private bool UsingCustom;

        public AccountDLinked()
        {
            if (!(bool)Config.DataBaseDLink.UsingDB) return;

            TableName = Config.DataBaseDLink.TableName;
            Get_AccountName_DB = Config.DataBaseDLink.Get_AccountName_DB;
            Get_AccountID_DB = Config.DataBaseDLink.Get_AccountID_DB;
            Get_UserID_DB = Config.DataBaseDLink.Get_UserID_DB;

            Custom_Get_AccountName_From_UserID = Config.DataBaseDLink.Custom_Get_AccountName_From_UserID;
            Custom_Get_UserID_From_AccountName = Config.DataBaseDLink.Custom_Get_UserID_From_AccountName;
            Custom_Get_UserID_From_AccountID = Config.DataBaseDLink.Custom_Get_UserID_From_AccountID;

            UsingCustom = (bool)Config.DataBaseDLink.UsingCustom;

            if (Config.DataBaseDLink.StorageType == "sqlite")
            {
                string sql = Path.Combine(TShock.SavePath, Config.DataBaseDLink.SqliteDBPath);
                Directory.CreateDirectory(Path.GetDirectoryName(sql));
                _db = new Microsoft.Data.Sqlite.SqliteConnection(string.Format("Data Source={0}", sql));
            }
            else if (Config.DataBaseDLink.StorageType == "mysql")
            {
                try
                {
                    var hostport = Config.DataBaseDLink.MySqlHost.Split(':');
                    MySqlConnection DB = new MySqlConnection();
                    DB.ConnectionString =
                        String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                            hostport[0],
                            hostport.Length > 1 ? hostport[1] : "3306",
                            Config.DataBaseDLink.MySqlDbName,
                            Config.DataBaseDLink.MySqlUsername,
                            Config.DataBaseDLink.MySqlPassword
                            );
                    _db = DB;
                }
                catch (MySqlException ex)
                {
                    throw new Exception("MySql not setup correctly");
                }
            }
            else
            {
                throw new Exception("Invalid storage type");
            }
        }

        public void ReloadConfig()
        {
            Config = Config.Read();
        }

        public string GetAccountNameByUserID(ulong UserID)
        {
            if (!(bool)Config.DataBaseDLink.UsingDB) throw new NullReferenceException();

            if (UsingCustom)
            {
                using var reader = _db.QueryReader("SELECT * " + Custom_Get_AccountName_From_UserID, UserID);

                while (reader.Read())
                {
                    return reader.Get<string>(Get_AccountName_DB);
                }
                throw new NullReferenceException();
            } else
            {
                using var reader = _db.QueryReader($"SELECT * FROM {TableName} WHERE {Get_UserID_DB} = @0", UserID);

                while (reader.Read())
                {
                    return reader.Get<string>(Get_AccountName_DB);
                }
                throw new NullReferenceException();
            }

            throw new NullReferenceException();
        }
        public int GetAccountIDByUserID(ulong UserID)
        {
            if (!(bool)Config.DataBaseDLink.UsingDB) throw new NullReferenceException();

            if (UsingCustom)
            {
                using var reader = _db.QueryReader("SELECT * " + Custom_Get_AccountName_From_UserID, UserID);

                while (reader.Read())
                {
                    return reader.Get<int>(Get_AccountID_DB);
                }
                throw new NullReferenceException();
            } else
            {
                using var reader = _db.QueryReader($"SELECT * FROM {TableName} WHERE {Get_UserID_DB} = @0", UserID);

                while (reader.Read())
                {
                    return reader.Get<int>(Get_AccountID_DB);
                }
                throw new NullReferenceException();
            }

            throw new NullReferenceException();
        }

        public ulong GetUserIDByAccountName(string AccountName)
        {
            if (!(bool)Config.DataBaseDLink.UsingDB) throw new NullReferenceException();

            if (UsingCustom)
            {
                using var reader = _db.QueryReader("SELECT * " + Custom_Get_UserID_From_AccountName, AccountName);
                
                while (reader.Read())
                {
                    if (reader.Get<string>(Get_UserID_DB) == "0" || reader.Get<string>(Get_UserID_DB) == "")
                    {
                        throw new NullReferenceException();
                    }
                    else
                    {
                        return ulong.Parse(reader.Get<string>(Get_UserID_DB));
                    }
                }
                throw new NullReferenceException();
            }
            else
            {
                using var reader = _db.QueryReader($"SELECT * FROM {TableName} WHERE {Get_AccountName_DB} = @0", AccountName);

                while (reader.Read())
                {
                    if (reader.Get<string>(Get_UserID_DB) == "0" || reader.Get<string>(Get_UserID_DB) == "")
                    {
                        throw new NullReferenceException();
                    }
                    else
                    {
                        return ulong.Parse(reader.Get<string>(Get_UserID_DB));
                    }
                }
                throw new NullReferenceException();
            }

            throw new NullReferenceException();
        }
        public ulong GetUserIDByAccountID(int AccountID)
        {
            if (!(bool)Config.DataBaseDLink.UsingDB) throw new NullReferenceException();

            if (UsingCustom)
            {
                using var reader = _db.QueryReader("SELECT * " + Custom_Get_UserID_From_AccountID, AccountID);
                
                while (reader.Read())
                {
                    if (reader.Get<string>(Get_UserID_DB) == "0" || reader.Get<string>(Get_UserID_DB) == "")
                    {
                        throw new NullReferenceException();
                    }
                    else
                    {
                        return ulong.Parse(reader.Get<string>(Get_UserID_DB));
                    }
                }
                throw new NullReferenceException();
            }
            else
            {
                using var reader = _db.QueryReader($"SELECT * FROM {TableName} WHERE {Get_AccountID_DB} = @0", AccountID);

                while (reader.Read())
                {
                    if (reader.Get<string>(Get_UserID_DB) == "0" || reader.Get<string>(Get_UserID_DB) == "")
                    {
                        throw new NullReferenceException();
                    }
                    else
                    {
                        return ulong.Parse(reader.Get<string>(Get_UserID_DB));
                    }
                }
                throw new NullReferenceException();
            }

            throw new NullReferenceException();
        }
    }
}
