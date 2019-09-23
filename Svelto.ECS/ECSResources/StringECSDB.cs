using System;

namespace Svelto.ECS.Experimental
{
    [Serialization.DoNotSerialize]
    public struct ECSString:IEquatable<ECSString>
    {
        internal uint id;

        ECSString(uint toEcs)
        {
            id = toEcs;
        }

        public static implicit operator string(ECSString ecsString)
        {
            return ResourcesECSDB<string>.FromECS(ecsString.id);
        }
        
        public static implicit operator ECSString(string text)
        {
            return new ECSString(ResourcesECSDB<string>.ToECS(text));
        }

        public bool Equals(ECSString other)
        {
            return other.id == id;
        }

        public override string ToString()
        {
            return ResourcesECSDB<string>.FromECS(id);
        }
    }
}