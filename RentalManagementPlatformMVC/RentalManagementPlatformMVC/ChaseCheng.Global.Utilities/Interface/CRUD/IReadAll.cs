using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Interface.CRUD
{
    /// <summary>
    /// 查詢整張表
    /// </summary>
    /// <typeparam name="Values">物件的單體資料結構</typeparam>
    public interface IReadAll<Values>
    {
        /// <summary>
        /// 讀取所有資料實體。
        /// </summary>
        /// <returns>包含所有資料的集合。</returns>
        IEnumerable<Values> ReadAll();
    }
}
