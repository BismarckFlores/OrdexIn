using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OrdexIn.Models
{
    [Table("roles")]
    public class UserRoleModel : BaseModel
    {
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("is_admin")]
        public bool IsAdmin { get; set; }
    }
}
