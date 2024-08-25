using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlX.XDevAPI.Common;
using TShockAPI;
using TShockAPI.DB;

namespace MKLP
{
    public class KLPAccount
    {
        public string Name;
        public string Title;
        public ulong? Discord;
        public int Age;
        public string Gender;
        public int PlayTime;
        public int Balance;
        public int Expirience;
        public int Level;
        public int Rank;

        public UserAccount InGameAccount;

        public string SettingDB;
        private string InventoryDB;

        private IDbConnection _db = MKLP.MAINKLP_DB;


        #region [ Constructor ]
        public KLPAccount(string AccountName)
        {
            using var reader = _db.QueryReader("SELECT * FROM Accounts WHERE Name = @0", AccountName);

            while (reader.Read())
            {
                //set
                Name = reader.Get<string>("Name");
                Title = reader.Get<string>("Title");
                if (reader.Get<string>("Discord") == "0")
                {
                    Discord = null;
                }
                else
                {
                    Discord = ulong.Parse(reader.Get<string>("Discord"));
                }
                Age = reader.Get<int>("Age");
                Gender = reader.Get<string>("Gender");
                PlayTime = reader.Get<int>("PlayTime");
                Balance = reader.Get<int>("Balance");
                Expirience = reader.Get<int>("Expirience");
                Level = reader.Get<int>("Level");
                Rank = reader.Get<int>("Rank");

                //get db
                SettingDB = reader.Get<string>("Settings");
                InventoryDB = reader.Get<string>("Inventory");

                //set remaining variables
                InGameAccount = TShock.UserAccounts.GetUserAccountByName(Name);
                return;
            }

            throw new NullReferenceException();
        }

        public KLPAccount(ulong UserId)
        {
            using var reader = _db.QueryReader("SELECT * FROM Accounts WHERE Discord = @0", UserId);

            while (reader.Read())
            {
                //set
                Name = reader.Get<string>("Name");
                Title = reader.Get<string>("Title");
                Discord = ulong.Parse(reader.Get<string>("Discord"));
                Age = reader.Get<int>("Age");
                Gender = reader.Get<string>("Gender");
                PlayTime = reader.Get<int>("PlayTime");
                Balance = reader.Get<int>("Balance");
                Expirience = reader.Get<int>("Expirience");
                Level = reader.Get<int>("Level");
                Rank = reader.Get<int>("Rank");

                //get db
                SettingDB = reader.Get<string>("Settings");
                InventoryDB = reader.Get<string>("Inventory");

                //set remaining variables
                InGameAccount = TShock.UserAccounts.GetUserAccountByName(Name);
                return;
            }

            throw new NullReferenceException();
        }
        #endregion
    }
}