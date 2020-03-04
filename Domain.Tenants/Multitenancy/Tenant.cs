using System.ComponentModel.DataAnnotations;

namespace Domain.Tenants.Multitenancy
{
    /// <summary>
    /// Tenant information
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// The tenant Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        public string Guid { get; set; }

        /// <summary>
        /// The tenant identifier
        /// </summary>
        public string Name { get; set; }

        public string ConnectionString { get; set; }
    }
}
