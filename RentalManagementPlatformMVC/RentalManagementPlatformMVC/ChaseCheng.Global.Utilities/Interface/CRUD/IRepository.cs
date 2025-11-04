using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Interface.CRUD
{
    /// <summary>
    /// 定義基本的 CRUD 操作介面，用於資料的新增、查詢、更新與刪除。+
    /// 強迫實作ICreate, IQuery, IUpdate, IDelete
    /// </summary>
    public interface IRepository<Key,Values> : ICreate<Values>, IQuery<Key,Values>, IReadAll<Values>, IUpdate<Key,Values>, IDelete<Key>
    {

    }
}