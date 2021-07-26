using Serialization;

namespace Svelto.ECS.Serialization
{
    public interface IEntityReferenceSerializer
    {
        EntityReferenceSerializer referenceSerializer { set; }
    }
}