using Microsoft.SharePoint.Taxonomy;
using System;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Taxonomy
{
    internal class TaxonomySessionManager
    {
        private Dictionary<Guid, TaxonomySession> taxonomySessions = new Dictionary<Guid,TaxonomySession>();

        private int taxCreationCount = 0;
        private Guid requestIdentifier = Guid.NewGuid();
        private GSoft.Dynamite.Logging.ILogger log;

        public TaxonomySessionManager(GSoft.Dynamite.Logging.ILogger logger)
        {
            this.log = logger;
        }

        private void LogTaxCreation()
        {
            this.taxCreationCount++;
            this.log.Error("Eddy: Create Taxonomy Session! count: " + this.taxCreationCount + " Request: " + this.requestIdentifier);
        }

        internal TaxonomySession GetSession(SPSite site)
        {
            if (!this.taxonomySessions.ContainsKey(site.ID))
            {
                this.LogTaxCreation();
                this.taxonomySessions.Add(site.ID, new TaxonomySession(site, true));
            }

            this.log.Error("Eddy: Return Taxonomy Session. Request: " + this.requestIdentifier);
            return this.taxonomySessions[site.ID];
        }

        internal void RefreshTaxonomySerssionCache(SPSite site)
        {
            if (this.taxonomySessions.ContainsKey(site.ID))
            {
                this.taxonomySessions[site.ID] = new TaxonomySession(site, true);
            }
            else
            {
                var newSession = new TaxonomySession(site, true);
            }
        }
    }
}
