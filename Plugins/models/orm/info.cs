using SQLite;

namespace Plugins.models.orm
{
    [Table("info")]
    public class Info
    {
        [Column("value")] public int Value { get; set; }
        [Column("name")] public string Name { get; set; }
        [Column("pval")] public int Pval { get; set; }
    }
}
