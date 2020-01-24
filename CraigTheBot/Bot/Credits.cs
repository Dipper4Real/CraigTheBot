using CraigTheBot.Bot.Database;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraigTheBot.Bot
{
    class Credits
    {
        public static int AddMoney(IUser user, int amount, IGuild guild, bool capped = false)
        {
            int money = Convert.ToInt32(DBConnector.Instance.GetDBData
                ($"SELECT Money FROM Users WHERE UserID = {user.Id} AND ServerID = {guild.Id}")[0]);

            if (capped)
            {
                int maxAmount = Convert.ToInt32(DBConnector.Instance.GetDBData
                ($"SELECT Money FROM Users WHERE UserID = {user.Id} AND ServerID = {guild.Id}")[0]);

                if (money + amount > maxAmount)
                {
                    return -1;
                }
            }

            DBConnector.Instance.ExecuteCommand($"UPDATE Users SET Money=Money+{amount} WHERE UserID = {user.Id} AND ServerID = {guild.Id}");
            return 0;
        }

        public static void RemoveMoney(IUser user, int amount, IGuild guild, bool capped = false)
        {
            int money = Convert.ToInt32(DBConnector.Instance.GetDBData
                ($"SELECT Money FROM Users WHERE UserID = {user.Id} AND ServerID = {guild.Id}")[0]);

            if (money - amount < 0)
            {
                amount = money;
            }

            DBConnector.Instance.ExecuteCommand($"UPDATE Users SET Money=Money-{amount} WHERE UserID = {user.Id} AND ServerID = {guild.Id}");
        }
    }
}
