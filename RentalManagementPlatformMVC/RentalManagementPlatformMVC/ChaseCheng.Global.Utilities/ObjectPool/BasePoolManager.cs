using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.ObjectPool
{
    public abstract class BasePoolManager<TItem, TData> where TItem : class, IObjectPoolItem<TData>, new()
    {
        private readonly ObjectPool<TItem, TData> objectPool;
        private readonly HashSet<TItem> itemSet = new();

        /// <summary>
        /// 建構時將創建物件池
        /// </summary>
        /// <param name="initialCapacity">物件池初始大小</param>
        public BasePoolManager(int initialCapacity = 0)
        {
            objectPool = new ObjectPool<TItem, TData>(initialCapacity);
        }

        /// <summary>
        /// 取得並初始化物件，並註冊到集合中
        /// </summary>
        public virtual TItem GetPoolItem(TData data)
        {
            TItem item = objectPool.Rent(data);
            itemSet.Add(item);
            return item;
        }

        /// <summary>
        /// 回收物件
        /// </summary>
        public virtual void ReturnPoolItem(TItem item)
        {
            itemSet.Remove(item);
            objectPool.Return(item);
        }

        /// <summary>
        /// 確保能存取目前所有活躍的物件（例如做更新、檢查等）
        /// </summary>
        protected IEnumerable<TItem> ActiveItems => itemSet;
    }
}
