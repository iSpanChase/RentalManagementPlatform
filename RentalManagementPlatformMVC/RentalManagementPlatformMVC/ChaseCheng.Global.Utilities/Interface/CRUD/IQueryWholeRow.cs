using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Interface.CRUD
{
    /// <summary>
    /// 根據主鍵查詢資料
    /// </summary>
    /// <typeparam name="Key">查詢物件的主鍵</typeparam>
    /// <typeparam name="Value">物件的單體資料結構</typeparam>
    public interface IQueryWholeRow<Key, Value>
    {
        /// <summary>
        /// 用主鍵查詢資料的各個欄位值
        /// </summary>
        /// <param name="id">查詢物件的主鍵</param>
        /// <param name="value">回傳物件的單體資料結構</param>
        /// <returns>回傳是否成功</returns>
        bool  TryQueryWholeRow(Key id, out Value value);
    }
}
