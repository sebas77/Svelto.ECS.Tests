using System;

namespace Svelto.Common
{
    public static class ProcessorCount
    {
        static readonly int processorCount = Environment.ProcessorCount;

        public static int Batch(uint length)
        {
            var numBatches = length / processorCount;

            if (numBatches < 16)
                return 16;
            
            return (int) numBatches;
        }
    }
}