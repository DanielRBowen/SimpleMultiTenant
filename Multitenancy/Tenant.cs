using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Multitenancy
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
        public byte[] Id { get; set; }

        /// <summary>
        /// The tenant identifier
        /// </summary>
        public string Name { get; set; }

        public string ServicePlan { get; set; }
        public string RecoveryState { get; set; }
        public System.DateTime LastUpdated { get; set; }

        /// <summary>
        /// Tenant items
        /// </summary>
        //public Dictionary<string, object> Items { get; private set; } = new Dictionary<string, object>();
    }
}
