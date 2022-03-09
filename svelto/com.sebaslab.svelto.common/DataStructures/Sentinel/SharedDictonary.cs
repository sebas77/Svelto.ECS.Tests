using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.Common.DataStructures
{
    public class SharedDictonary
    {
        public static void Init()
        {
            test.Data.test = SharedDictionaryStruct.Create();
        }

        static readonly SharedStatic<SharedDictionaryStruct, SharedDictonary> test;
    }
}