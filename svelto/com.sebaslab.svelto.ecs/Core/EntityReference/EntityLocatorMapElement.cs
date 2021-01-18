using System;

namespace Svelto.ECS
{
    struct EntityLocatorMapElement
    {
        internal EGID egid;
        internal uint version;

        internal EntityLocatorMapElement(EGID egid)
        {
            this.egid = egid;
            version = 0;
        }
    }
}