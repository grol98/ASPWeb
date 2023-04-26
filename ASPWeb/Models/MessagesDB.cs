namespace ASPWeb.Models
{
    public class MessagesDB
    {
        public int Id { get; set; }
        public int sn { get; set; }
        public string operation { get; set; }
        public string? card { get; set; }
        public int? flag { get; set; }
        public int? tz { get; set; }
    }
}
