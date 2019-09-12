using System;

namespace Svelto.ServiceLayer.Experimental
{
    public interface IServiceEventContainer : IDisposable
    {
        void ListenTo<TListener, TDelegate>(TDelegate callBack)
            where TListener : class, IServiceEventListener<TDelegate> where TDelegate : Delegate;
    }
}
