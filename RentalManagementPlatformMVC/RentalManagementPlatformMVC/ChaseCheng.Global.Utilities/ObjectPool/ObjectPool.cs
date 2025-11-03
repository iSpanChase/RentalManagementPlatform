using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.ObjectPool
{
    /// <summary>
    /// 提供重設狀態的方法，讓物件可被重複使用於物件池中。
    /// </summary>
    public interface IObjectPoolItem
    {
        /// <summary>
        /// 當物件從池中被取出時會被呼叫，以重置狀態。
        /// </summary>
        void Setup();
    }


    /// <summary>
    /// 提供重設狀態的方法，讓物件可被重複使用於物件池中。
    /// </summary>
    public interface IObjectPoolItem<TData>
    {
        /// <summary>
        /// 當物件從池中被取出時會被呼叫，以重置狀態。
        /// </summary>
        void Setup(TData createData);
    }


    /// <summary>
    /// 泛型物件池，只接受實作 IObjectPoolItem 的類型。
    /// </summary>
    /// <typeparam name="T">要池化的物件類型。</typeparam>
    public class ObjectPool<T> where T : class, IObjectPoolItem, new()
    {
        private readonly Stack<T> _pool;

        /// <summary>
        /// 初始化物件池，可選擇預先建立幾個實例。
        /// </summary>
        /// <param name="initialCapacity">預先建立的數量。</param>
        public ObjectPool(int initialCapacity = 0)
        {
            _pool = new Stack<T>(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
                _pool.Push(new T());
        }

        /// <summary>
        /// 從物件池中租借一個物件，若池中無可用物件則建立新的。
        /// </summary>
        /// <returns>可使用的物件。</returns>
        public T Rent()
        {
            var item = _pool.Count > 0 ? _pool.Pop() : new T();
            item.Setup();
            return item;
        }

        /// <summary>
        /// 將物件歸還到池中。
        /// </summary>
        /// <param name="item">欲歸還的物件。</param>
        public void Return(T item) => _pool.Push(item);

        /// <summary>
        /// 目前物件池中可用物件的數量。
        /// </summary>
        public int Count => _pool.Count;
    }

    /// <summary>
    /// 泛型物件池，只接受實作 IObjectPoolItem 的類型。
    /// </summary>
    /// <typeparam name="T">要池化的物件類型。</typeparam>
    /// <typeparam name="TData">池化物件的創見資料。</typeparam>
    public class ObjectPool<T, TData> where T : class, IObjectPoolItem<TData>, new()
    {
        private readonly Stack<T> _pool;

        /// <summary>
        /// 初始化物件池，可選擇預先建立幾個實例。
        /// </summary>
        /// <param name="initialCapacity">預先建立的數量。</param>
        public ObjectPool(int initialCapacity = 0)
        {
            _pool = new Stack<T>(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
                _pool.Push(new T());
        }

        /// <summary>
        /// 從物件池中租借一個物件，若池中無可用物件則建立新的。
        /// </summary>
        /// <returns>可使用的物件。</returns>
        public T Rent(TData data)
        {
            var item = _pool.Count > 0 ? _pool.Pop() : new T();
            item.Setup(data);
            return item;
        }

        /// <summary>
        /// 將物件歸還到池中。
        /// </summary>
        /// <param name="item">欲歸還的物件。</param>
        public void Return(T item) => _pool.Push(item);

        /// <summary>
        /// 目前物件池中可用物件的數量。
        /// </summary>
        public int Count => _pool.Count;
    }
}
