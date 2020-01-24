namespace CraigTheBot.Bot.Objects
{
    public class Item
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public string Command { get; set; }
        public string ServerID { get; set; }
        public string Description { get; set; }
        public string MinRank { get; set; }
    }
}