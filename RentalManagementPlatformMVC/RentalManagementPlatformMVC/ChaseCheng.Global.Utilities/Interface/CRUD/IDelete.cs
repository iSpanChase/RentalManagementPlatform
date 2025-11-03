using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Interface.CRUD
{
    /// <summary>
    /// 根據主鍵刪除一筆資料
    /// </summary>
    /// <typeparam name="Value">要刪除的資料</typeparam>
    public interface IDelete<Value>
    {
        /// <summary>
        /// 根據主鍵刪除資料，回傳是否成功
        /// </summary>
        /// <param name="value">要刪除的資料</param>
        /// <returns>是否刪除成功。</returns>
        bool TryDelete(Value value);
    }
}
