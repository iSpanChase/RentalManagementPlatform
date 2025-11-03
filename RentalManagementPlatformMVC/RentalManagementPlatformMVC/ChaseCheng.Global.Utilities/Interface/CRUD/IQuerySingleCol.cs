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
    /// <typeparam name="Values">物件的單體資料結構</typeparam>
    public interface IQuerySingleCol<Key, Values>
    {
        /// <summary>
        /// 用主鍵查詢一筆資料的單個欄位值
        /// </summary>
        /// <typeparam name="Value">須查詢物件在該欄位的值</typeparam>
        /// <param name="Id">查詢物件的主鍵</param>
        /// <param name="ColName">須查詢的欄位名稱</param>
        /// <param name="value">回傳須查詢物件在該欄位的值</param>
        /// <returns>回傳是否成功</returns>
        bool TryQuerySingleCol<Value>(Key Id, string ColName, out Value value);
    }
}
