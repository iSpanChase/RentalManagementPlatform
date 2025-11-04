using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Looping
{
    /// <summary>
    /// 定義一個支援統一控制的循環更新物件介面。
    /// 實作此介面的物件可被繼承自 <see cref="LoopingEntityController"/> 的子物件控管其更新流程，
    /// 並依序執行 OnStartLoop、OnBeforeUpdate、OnUpdate、OnAfterUpdate、OnEndLoop 等生命週期方法。
    /// </summary>
    public interface ILoopingEntity
	{
		/// <summary>
		/// 執行優先級，在一個Update內，系統將優先執行高優先級的ILoopingEntity的OnUpdate，OnBeforeUpdate、OnAfterUpdate也是如此
		/// </summary>
		int Priority { get; set; }

		/// <summary>
		/// 物件被加入循環時初次呼叫，適合做初始化。
		/// </summary>
		void OnStartLoop();
		/// <summary>
		/// 每次更新循環前呼叫，用於執行更新前邏輯。
		/// </summary>
		void OnBeforeUpdate();
		/// <summary>
		/// 每次更新循環時呼叫，為主要邏輯執行位置。
		/// </summary>
		void OnUpdate();
		/// <summary>
		/// 每次更新循環後呼叫，用於收尾或處理更新後邏輯。
		/// </summary>
		void OnAfterUpdate();
		/// <summary>
		/// 物件從循環移除時呼叫，適合做清理。
		/// </summary>
		void OnEndLoop();
	}
}
