using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OrdexIn.Models
{
    [Table("profiles")]
    public class ProfileModel : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid UserId { get; set; }

        [Column("email")]
        public string Email { get; set; }
        
        [Column("is_admin")]
        public bool IsAdmin { get; set; }
    }
}
