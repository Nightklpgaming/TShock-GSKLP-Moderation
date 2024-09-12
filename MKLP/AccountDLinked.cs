using Microsoft.Data.Sqlite;
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
        private IDbConnection _db;

        private string TableName;
        private string Get_AccountName_DB;
        private string Get_UserID_DB;

        private string Custom_Get_AccountName_From_UserID;
        private string Custom_Get_UserID_From_AccountName;

        private bool UsingCustom;

        public AccountDLinked()
        {
            if (!(bool)MKLP.Config.DataBase.UsingDB) return;

            if ((bool)MKLP.Config.DataBase.UsingGSKLP)
            {
                _db = new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, "MAINKLP.sqlite")));
                Custom_Get_AccountName_From_UserID = "SELECT * FROM Accounts WHERE Discord = @0";
                Custom_Get_UserID_From_AccountName = "SELECT * FROM Accounts WHERE Name = @0";
                TableName = "Accounts";
                Get_AccountName_DB = "Name";
                Get_UserID_DB = "Discord";
                UsingCustom = true;
                return;
            } else
            {
                if (MKLP.Config.DataBase.UseTShockFilePath == null)
                {
                    _db = new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, MKLP.Config.DataBase.File)));
                } else if (!(bool)MKLP.Config.DataBase.UseTShockFilePath)
                {
                    _db = new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, MKLP.Config.DataBase.File)));
                } else
                {
                    _db = new SqliteConnection(("Data Source=" + Path.Combine(MKLP.Config.DataBase.Path, MKLP.Config.DataBase.File)));
                }

                TableName = MKLP.Config.DataBase.TableName;
                Get_AccountName_DB = MKLP.Config.DataBase.Get_AccountName_DB;
                Get_UserID_DB = MKLP.Config.DataBase.Get_UserID_DB;

                Custom_Get_AccountName_From_UserID = MKLP.Config.DataBase.Custom_Get_AccountName_From_UserID;
                Custom_Get_UserID_From_AccountName = MKLP.Config.DataBase.Custom_Get_UserID_From_AccountName;

                UsingCustom = (bool)MKLP.Config.DataBase.UsingCustom;
                return;
            }
        }

        public string GetAccountName(ulong UserID)
        {
            if (!(bool)MKLP.Config.DataBase.UsingDB) throw new NullReferenceException();

            if (UsingCustom)
            {
                using var reader = _db.QueryReader(Custom_Get_AccountName_From_UserID, UserID);

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

        public ulong GetUserID(string AccountName)
        {
            if (!(bool)MKLP.Config.DataBase.UsingDB) throw new NullReferenceException();

            if (UsingCustom)
            {
                using var reader = _db.QueryReader(Custom_Get_UserID_From_AccountName, AccountName);
                
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
    }
}
