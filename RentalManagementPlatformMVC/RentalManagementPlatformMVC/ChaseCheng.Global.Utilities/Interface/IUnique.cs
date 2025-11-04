using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Interface
{
    public interface IUnique<TId, TValue>
    {
        static abstract TId GetNextID { get; }
        TId Id { get; }
        TValue Value { get; set; }
        static abstract IUnique<TId, TValue> Creat(TValue value);
    }
}
