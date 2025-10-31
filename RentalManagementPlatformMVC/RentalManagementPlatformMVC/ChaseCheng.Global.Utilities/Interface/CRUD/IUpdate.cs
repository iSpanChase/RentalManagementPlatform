using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Interface.CRUD
{
    /// <summary>
    /// 根據主鍵更新資料
    /// </summary>
    /// <typeparam name="Key">物件的主鍵</typeparam>
    /// <typeparam name="Values">物件的單體資料結構</typeparam>
    public interface IUpdate<Key, Values>
    {
        /// <summary>
        /// 根據主鍵更新資料，回傳是否成功
        /// </summary>
        /// <param name="entity">要更新的新資料實體。</param>
        /// <param name="id">要更新的資料主鍵。</param>
        /// <returns>是否更新成功。</returns>
        bool TryUpdate(Key id, Values entity);
    }
}
