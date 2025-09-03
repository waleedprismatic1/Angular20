namespace Webapi.Models
{
    public class Permission
    {
        public Guid Id { get; set; } 
        public string Name { get; set; }
        public string Description { get; set; }
        public string Module { get; set; } // e.g., "Sales", "Sales Transaction", "Sale Invoice"
        public int HierarchyLevel { get; set; } // 1, 2, 3 for the hierarchy

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}
