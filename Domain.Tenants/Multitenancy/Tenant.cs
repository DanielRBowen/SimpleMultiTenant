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

        /// <summary>
        /// A string which contains ip addresses which are comma delimited. Such as "97.198.174.206, 216.204.170.148"
        /// </summary>
        public string IpAddresses { get; set; }

        /// <summary>
        /// A string which contains domain names which are comma delimited. Such as "oiscus.com, slonds.com"
        /// </summary>
        public string DomainNames { get; set; }
    }
}
