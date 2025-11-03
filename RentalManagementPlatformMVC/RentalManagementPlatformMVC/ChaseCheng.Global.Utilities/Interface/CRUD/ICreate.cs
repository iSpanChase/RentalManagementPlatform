using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Interface.CRUD
{
    /// <summary>
    /// 建立一筆新資料
    /// </summary>
    /// <typeparam name="Values">物件的單體資料結構</typeparam>
    public interface ICreate<Values>
    {
        /// <summary>
        /// 嘗試建立一筆新資料，回傳是否成功
        /// </summary>
        /// <param name="entity">要新增的資料實體。</param>
        /// <returns>是否新增成功</returns>
        bool TryCreate(Values entity);
    }
}
