using CraigTheBot.Bot.Database;
using CraigTheBot.Bot.Objects;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace CraigTheBot.Bot.Commands
{
    public class ShopModule : ModuleBase
    {
        [Command("shop")]
        public async Task shop(params string[] args)
        {
            if (args.Count() > 0)
            {
                if (args[0].ToLower() == "list")
                {
                    var database = DBConnector.Instance;

                    var itemList = ItemModule.GetItemsFromShop(Context.Guild.Id.ToString());

                    string tempString = "";
                    int i = 0;
                    foreach (var item in itemList)
                    {
                        tempString += $"{++i}. {item.Name}\n";
                    }

                    Craig.Instance.Say($"```These are the current items:\n{tempString}```", Context.Channel);
                }
            }
        }

       /* [Command("shop add item")]
        public async Task AddItem(string id)
        {

        }*/

        [Command("Buy")]
        public async Task Buy(string Item)
        {

        }
    }
}
