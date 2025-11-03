using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Interface.CRUD
{
	/// <summary>
	/// 提供自動產生唯一整數 ID 的功能，避免重複使用已產生過的 ID。
	/// 使用者可指定起始數字，並可手動加入或釋放已使用的 ID。
	/// </summary>
	public class UniqueIdGenerator
	{
		public UniqueIdGenerator() {  }
		public UniqueIdGenerator(int startNum) { _currentId = startNum; }
		private int _currentId = int.MinValue;
		/// <summary>
		/// 已使用的idSet清單
		/// </summary>
		public HashSet<int> IdSet { get; set; } = new HashSet<int>();
		/// <summary>
		/// 取得下一個未使用的唯一 ID。
		/// </summary>
		public int GetNextId()
		{
			while (IdSet.Contains(_currentId))
			{
				if (_currentId == int.MaxValue)
				{
					throw new InvalidOperationException("已無可用的唯一 ID。");
				}
				_currentId++;
			}

			int result = _currentId;
			IdSet.Add(result);
			_currentId++;
			return result;
		}
	}
}
