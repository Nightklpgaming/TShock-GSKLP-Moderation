using Microsoft.Data.Sqlite;
using MKLP.Modules;
using MySql.Data.MySqlClient;
using NuGet.Protocol.Plugins;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Bcpg.Sig;
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
    public class MKLP_DB
    {
        private IDbConnection _db;

        public MKLP_DB(IDbConnection db)
        {
            _db = db;

            var sqlCreator = new SqlTableCreator(db, new SqliteQueryCreator());

            sqlCreator.EnsureTableStructure(new SqlTable("Reports",
                new SqlColumn("ID", MySqlDbType.Int32) { AutoIncrement = true, Primary = true },
                new SqlColumn("Reporter", MySqlDbType.VarChar),
                new SqlColumn("Target", MySqlDbType.VarChar),
                new SqlColumn("Message", MySqlDbType.Text),
                new SqlColumn("Since", MySqlDbType.DateTime),
                new SqlColumn("Location", MySqlDbType.VarChar),
                new SqlColumn("Players", MySqlDbType.Text)));

            sqlCreator.EnsureTableStructure(new SqlTable("Mute",
                new SqlColumn("ID", MySqlDbType.Int32) { AutoIncrement = true, Primary = true },
                new SqlColumn("Identifier", MySqlDbType.VarChar),
                new SqlColumn("Reason", MySqlDbType.Text),
                new SqlColumn("Expiration", MySqlDbType.DateTime)));

            sqlCreator.EnsureTableStructure(new SqlTable("AccountDLinking",
                new SqlColumn("Name", MySqlDbType.VarChar) { Primary = true, Unique = true },
                new SqlColumn("UserID", MySqlDbType.VarChar)));

        }

        #region [ AccountDLinked ]

        public Dictionary<string, string> AccountDLinkingList()
        {
            using var reader = _db.QueryReader("SELECT * FROM AccountDLinking");

            Dictionary<string, string> result = new();

            while (reader.Read())
            {
                result.Add(reader.Get<string>("Name"), reader.Get<string>("UserID"));
            }
            return result;
            throw new NullReferenceException();
        }

        public bool AddAccountDLinkingUserID(string AccountName, string UserID)
        {
            return _db.Query("INSERT INTO AccountDLinking (" +
                "Name, " +
                "UserID) " +
                "VALUES (@0, @1)",
                AccountName,
                UserID
                ) != 0;
        }

        public bool ChangeAccountDLinkingUserID(string AccountName, string UserID)
        {
            return _db.Query("UPDATE AccountDLinking SET UserID = @0 WHERE Name = @1", UserID, AccountName) != 0;
        }

        public bool DeleteAccountDLinkingUserID(string AccountName)
        {
            return _db.Query("DELETE FROM AccountDLinking WHERE Name = @0", AccountName) != 0;
        }

        #endregion

        #region [ Reports ]
        public List<MKLP_Report> GetReportList(int maxreport = 10, string from = "", string target = "")
        {
            using var reader = _db.QueryReader("SELECT * FROM Reports ORDER BY ID DESC");

            List<MKLP_Report> result = new();

            int index = 0;

            if (maxreport < 0) { maxreport = 1; }

            while (reader.Read())
            {
                if (index > maxreport) break;

                if (reader.Get<string>("Reporter") != from && from != "") continue;

                if (reader.Get<string>("Target") != target && target != "") continue;

                result.Add(new(
                        reader.Get<int>("ID"),
                        reader.Get<string>("Reporter"),
                        reader.Get<string>("Target"),
                        reader.Get<string>("Message"),
                        reader.Get<DateTime>("Since"),
                        reader.Get<string>("Location"),
                        reader.Get<string>("Players")
                        ));
                index++;
            }
            return result;
            throw new NullReferenceException();
        }

        public IEnumerable<MKLP_Report> GetReport(string from = "", string target = "")
        {
            if (target != "")
            {
                using var reader = _db.QueryReader("SELECT * FROM Reports WHERE Target = @0", target);
                while (reader.Read())
                {
                    yield return new(
                        reader.Get<int>("ID"),
                        reader.Get<string>("Reporter"),
                        reader.Get<string>("Target"),
                        reader.Get<string>("Message"),
                        reader.Get<DateTime>("Since"),
                        reader.Get<string>("Location"),
                        reader.Get<string>("Players")
                        );
                }
            } else if (from != "")
            {
                using var reader = _db.QueryReader("SELECT * FROM Reports WHERE From = @0", from);
                while (reader.Read())
                {
                    yield return new(
                        reader.Get<int>("ID"),
                        reader.Get<string>("Reporter"),
                        reader.Get<string>("Target"),
                        reader.Get<string>("Message"),
                        reader.Get<DateTime>("Since"),
                        reader.Get<string>("Location"),
                        reader.Get<string>("Players")
                        );
                }
            }
            
        }

        public MKLP_Report GetReportByID(int ID)
        {
            using var reader = _db.QueryReader("SELECT * FROM Reports WHERE ID = @0", ID);
            while (reader.Read())
            {
                return new(
                    reader.Get<int>("ID"),
                    reader.Get<string>("Reporter"),
                    reader.Get<string>("Target"),
                    reader.Get<string>("Message"),
                    reader.Get<DateTime>("Since"),
                    reader.Get<string>("Location"),
                    reader.Get<string>("Players")
                    );
            }

            throw new NullReferenceException();
        }

        public int AddReport(string reporter, string target, string message, DateTime Since, string location, string playerlist)
        {
            string query = "INSERT INTO Reports (" +
                "Reporter, " +
                "Target, " +
                "Message, " +
                "Since, " +
                "Location, " +
                "Players) " +
                "VALUES (@0, @1, @2, @3, @4, @5);";
            if (_db.GetSqlType() == SqlType.Mysql)
            {
                query += "SELECT LAST_INSERT_ID();";
            }
            else
            {
                query += "SELECT CAST(last_insert_rowid() as INT);";
            }

            int id = _db.QueryScalar<int>(query,
                reporter,
                target,
                message,
                Since,
                location,
                playerlist
                );

            if (id == 0)
            {
                throw new NullReferenceException();
            }

            return id;
        }

        public bool DeleteReport(int ID)
        {
            return _db.Query("DELETE FROM Reports WHERE ID = @0", ID) != 0;
        }

        #endregion

        #region [ Mutes ]

        public IEnumerable<Mute> GetMute(string Identifier)
        {
            using var reader = _db.QueryReader("SELECT * FROM Mute WHERE Identifier = @0", Identifier);
            while (reader.Read())
            {
                yield return new (
                    reader.Get<int>("ID"),
                    reader.Get<string>("Identifier"),
                    reader.Get<string>("Reason"),
                    reader.Get<DateTime>("Expiration")
                    );
            }
        }

        public IEnumerable<Mute> GetMute(int ID)
        {
            using var reader = _db.QueryReader("SELECT * FROM Mute WHERE ID = @0", ID);
            while (reader.Read())
            {
                yield return new(
                    reader.Get<int>("ID"),
                    reader.Get<string>("Identifier"),
                    reader.Get<string>("Reason"),
                    reader.Get<DateTime>("Expiration")
                    );
            }
        }

        public DateTime GetMuteExpiration(string Identifier)
        {
            using var reader = _db.QueryReader("SELECT * FROM Mute WHERE Identifier = @0", Identifier);
            while (reader.Read())
            {
                return reader.Get<DateTime>("Expiration");
            }
            throw new NullReferenceException();
        }

        public bool AddMute(string Identifier, DateTime Expiration, string Reason = "No Reason Provided")
        {
            return _db.Query("INSERT INTO Mute (" +
                "ID, " +
                "Identifier, " +
                "Reason, " +
                "Expiration) " +
                "VALUES (@0, @1, @2, @3)",
                null,
                Identifier,
                Reason,
                Expiration
                ) != 0;
        }

        public bool DeleteMute(string Identifier)
        {
            return _db.Query("DELETE FROM Mute WHERE Identifier = @0", Identifier) != 0;
        }

        
        public bool CheckPlayerMute(TSPlayer player, bool inform_unmuted = false)
        {
            bool IsExist = false;
            try
            {
                DateTime Name = GetMuteExpiration($"Name:{player.Name}");

                IsExist = true;
                if (DateTime.UtcNow > Name)
                {
                    player.mute = false;
                } else
                {
                    player.mute = true;
                }

            } catch (NullReferenceException) { }

            try
            {
                DateTime AccountName = GetMuteExpiration($"Account:{player.Account.Name}");

                IsExist = true;
                if (DateTime.UtcNow > AccountName)
                {
                    player.mute = false;
                }
                else
                {
                    player.mute = true;
                }

            }
            catch (NullReferenceException) { }

            try
            {
                DateTime IP = GetMuteExpiration($"IP:{player.IP}");

                IsExist = true;
                if (DateTime.UtcNow > IP)
                {
                    player.mute = false;
                }
                else
                {
                    player.mute = true;
                }

            }
            catch (NullReferenceException) { }

            try
            {
                DateTime UUID = GetMuteExpiration($"UUID:{player.UUID}");

                IsExist = true;
                if (DateTime.UtcNow > UUID)
                {
                    player.mute = false;
                }
                else
                {
                    player.mute = true;
                }

            }
            catch (NullReferenceException) { }

            /*
            if (IsExist)
            {
                if (!player.mute && inform_unmuted)
                {
                    player.SendSuccessMessage("You're no longer muted");
                } else if (inform_muted)
                {
                    player.SendErrorMessage("You're still muted!");
                }
                return player.mute;
            }
            */

            if (IsExist && !player.mute && inform_unmuted)
            {
                player.SendSuccessMessage("You're no longer muted");
            }

            return player.mute;

        }

        #endregion

    }

    public class Mute
    {
        public int ID;
        public string Identifier;
        public string Reason;
        public DateTime Expiration;

        public Mute(
            int ID,
            string Identifier,
            string Reason,
            DateTime Expiration
            )
        {
            this.ID = ID;
            this.Identifier = Identifier;
            this.Reason = Reason;
            this.Expiration = Expiration;
        }
    }

    public class MKLP_Report
    {
        public int ID;
        public string From;
        public string Target;
        public string Message;
        public DateTime Since;
        public string Location;
        public string Players;

        public MKLP_Report(
            int ID,
            string From,
            string Target,
            string Message,
            DateTime Since,
            string Location,
            string Players
            )
        {
            this.ID = ID;
            this.From = From;
            this.Target = Target;
            this.Message = Message;
            this.Since = Since;
            this.Location = Location;
            this.Players = Players;
        }
    }
}
