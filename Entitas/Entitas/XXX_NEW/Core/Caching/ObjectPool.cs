using System;
using System.Collections.Generic;

namespace Entitas {

    public class ObjectPool<T> {

        readonly Func<T> _factoryMethod;
        readonly Action<T> _resetMethod;
        readonly Stack<T> _pool;

        public ObjectPool(Func<T> factoryMethod, Action<T> resetMethod = null) {
            _factoryMethod = factoryMethod;
            _resetMethod = resetMethod;
            _pool = new Stack<T>();
        }

        public T Get() {
            return _pool.Count == 0
                ? _factoryMethod()
                : _pool.Pop();
        }

        public void Push(T obj) {
            if(_resetMethod != null) {
                _resetMethod(obj);
            }
            _pool.Push(obj);
        }
    }
}
