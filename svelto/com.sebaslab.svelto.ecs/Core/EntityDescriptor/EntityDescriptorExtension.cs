namespace Svelto.ECS
{
    public static class EntityDescriptorExtension
    {
        public static bool IsUnmanaged(this IEntityDescriptor descriptor)
        {
            foreach (var component in descriptor.componentsToBuild)
                if (component is EntityInfoComponent == false && component.isUnmanaged == false)
                    return false;
            
            return true;
        }
    }
}