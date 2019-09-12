using System;

namespace Svelto.ECS
{
    public class ECSException : Exception
    {
        public ECSException(string message):base("<color=orange>".FastConcat(message, "</color>"))
        {}
        
        public ECSException(string message, Exception innerE):base("<color=orange>".FastConcat(message, "</color>"), innerE)
        {}
    }
}