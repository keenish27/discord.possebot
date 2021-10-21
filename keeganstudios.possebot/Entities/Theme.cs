using System.ComponentModel.DataAnnotations.Schema;

namespace keeganstudios.possebot.Entities
{
    public class Theme
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string AudioPath { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public int Start { get; set; }
        public int Duration { get; set; }
        public bool Enabled { get; set; }
    }
}
