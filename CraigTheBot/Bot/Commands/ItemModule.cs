using CraigTheBot.Bot.Database;
using CraigTheBot.Bot.Objects;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraigTheBot.Bot.Commands
{
    public class ItemModule : ModuleBase
    {
        public static List<Item> GetItemsFromServer(string ServerID)
        {
            var database = DBConnector.Instance;

            DataTable table = database.GetDBObjects($"SELECT * FROM Item WHERE ServerID = {ServerID}");

            var itemList = table.AsEnumerable().Select(row =>
                    new Item
                    {
                        ID = row.Field<string>("ItemID"),
                        Name = row.Field<string>("ItemName"),
                        Price = row.Field<long>("Price"),
                        Command = row.Field<string>("CommandOnUse"),
                        ServerID = row.Field<string>("ServerID"),
                        MinRank = row.Field<string>("RankRequired")
                    }).ToList();
            return itemList;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [Command("item create")]
        public async Task CreateItem(params string[] args)
        {
            var db = DBConnector.Instance;

            Item item = new Item();
            string tempStr = "";

            foreach (var str in args)
            {
                tempStr += str + " ";
            }

            var prop = tempStr.Split(new char[] { '-', '-' });
            var propName = prop[0].ToLower();
            var propValue = prop[1].ToLower();

            while (true)
            {
                string ID = RandomString(10);

                if (db.GetDBData($"SELECT ItemID FROM Item WHERE ItemID = '{ID}'").Count == 0)
                {
                    item.ID = ID;
                    break;
                }
            }

            if (item.Name == null)
                item.Name = "Undefined";
            if (item.ID == null)
                item.ID = "Undefined";
            if (item.Description == null)
                item.Name = "";
            if (item.Command == null)
                item.Command = "";
            if (item.MinRank == null)
                item.MinRank = "";

            db.ExecuteCommand($"INSERT INTO Item (ItemID, ItemName, Price, CommandOnUse, RankRequired, ServerID, Description) VALUES ({item.ID}, {item.Name}, {item.Price}, {item.Command}, {item.MinRank}, {Context.Guild.Id.ToString()}, {item.Description})");

            Craig.Instance.Say($"Item {item.Name} successfully added.", Context.Channel);
        }
    }
}
