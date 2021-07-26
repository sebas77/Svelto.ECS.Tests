using Svelto.ECS;

namespace Serialization
{
    public struct EntityReferenceSerializer
    {
        public EntityReferenceSerializer(EnginesRoot.LocatorMap entityLocator)
        {
            _entityLocator = entityLocator;
        }

        public EGID SerializeReference(EntityReference reference)
        {
            _entityLocator.TryGetEGID(reference, out var egid);
            return egid;
        }

        public EntityReference DeserializeReference(EGID egid)
        {
            return _entityLocator.GetOrCreateEntityReference(egid);
        }

        readonly EnginesRoot.LocatorMap _entityLocator;
    }
}