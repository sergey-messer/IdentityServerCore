using System;

namespace TzIdentityManager.Configuration
{
    public class IdentityManagerOptions
    {
        public IdentityManagerOptions()
        {
            SecurityConfiguration = new LocalhostSecurityConfiguration();
        }
        
        public SecurityConfiguration SecurityConfiguration { get; set; }
        public bool DisableUserInterface { get; set; }

        internal void Validate()
        {
           
            if (this.SecurityConfiguration == null)
            {
                throw new Exception("SecurityConfiguration is required.");
            }

            SecurityConfiguration.Validate();
        }
    }
}
