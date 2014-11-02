using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Represents a SharePoint taxonomy field binding context.
    /// I.e. defines the term set or sub-term of a term set that
    /// will serve to limit in the choices of a taxonomy value picker
    /// dialog for a particular taxonomy field.
    /// </summary>
    public class TaxonomyContext
    {
        /// <summary>
        /// Default empty constructor for serialization purposes.
        /// </summary>
        public TaxonomyContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TaxonomyContext"/>
        /// using a term set metadata object. Users' choices will be limited
        /// to all terms in the term set in the taxonomy picker dialog.
        /// </summary>
        /// <param name="termSetInfo">The taxonomy field's limiting term set</param>
        public TaxonomyContext(TermSetInfo termSetInfo)
        {
            if (termSetInfo == null)
            {
                throw new InvalidOperationException("Failed to create TaxonomyContext: the TermSet property should never be null.");
            }

            this.TermSet = termSetInfo;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TaxonomyContext"/>
        /// using a taxonomy term metadata object. Users' choices will be limited
        /// to sub-terms of the specified term.
        /// </summary>
        /// <param name="limitingSubTermInfo">The term by which the choices of the users will be limited in the taxonomy pickers</param>
        public TaxonomyContext(TermInfo limitingSubTermInfo)
            : this(limitingSubTermInfo.TermSet)
        {
            this.TermSubset = limitingSubTermInfo;
        }

        /// <summary>
        /// Context's term group. Assume Default Farm Term Store if this property is null.
        /// </summary>
        public TermStoreInfo TermStore 
        { 
            get
            {
                if (this.Group != null)
                {
                    // We're not dealing with a term set belonging to the default
                    // site collection term group
                    if (this.Group.TermStore != null)
                    {
                        // We're not dealing with a term group that belongs to the default farm term store.
                        // Important assumption: we assume that default site collection term groups always
                        // belong to the default term store.
                        return this.Group.TermStore;
                    }
                }

                // Assume the binding context corresponds to the default farm term store
                return null;
            }            
        }

        /// <summary>
        /// Context's term group. Assume Default Site Collection Term Group if this property is null.
        /// </summary>
        public TermGroupInfo Group 
        { 
            get
            {
                if (this.TermSet == null)
                {
                    throw new InvalidOperationException("TaxonomyContext property TermSet should never be empty.");
                }

                if (this.TermSet.Group != null)
                {
                    // We're not dealing with a term set belonging to the default
                    // site collection term group
                    return this.TermSet.Group;
                }

                // We're dealing with a term set belonging to the default
                // site collection term group (and we assume this term group
                // belongs to the default farm term store).
                return null;
            }
        }

        /// <summary>
        /// Context's term set.
        /// </summary>
        public TermSetInfo TermSet { get; set; }

        /// <summary>
        /// Terms that limits the choices of term options further.
        /// Assume that field is bound to term set if this property is null.
        /// </summary>
        public TermInfo TermSubset { get; set; }
    }
}
