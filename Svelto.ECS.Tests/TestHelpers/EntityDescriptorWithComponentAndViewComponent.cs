namespace Svelto.ECS.Tests
{
    class EntityDescriptorWithComponentAndViewComponent : GenericEntityDescriptor<TestEntityViewComponent,
        TestEntityComponent, TestEntityComponentWithProperties> { }
    
    class EntityDescriptorWithComponents : GenericEntityDescriptor<TestEntityComponent, TestEntityComponentWithProperties> { }
    
    class EntityDescriptorViewComponentWithString : GenericEntityDescriptor<TestEntityViewComponentString> { }
    
    class EntityDescriptorViewComponentWithCustomStruct : GenericEntityDescriptor<TestEntityViewComponentWithCustomStructIsInvalid> { }
    
    class EntityDescriptorWith4Components : GenericEntityDescriptor<TestEntityViewComponent,
        TestEntityComponent, TestEntityComponentWithProperties, TestEntityComponentInt> { }
}