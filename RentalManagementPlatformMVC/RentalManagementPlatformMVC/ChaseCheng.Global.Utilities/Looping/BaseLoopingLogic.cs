namespace ChaseCheng.Global.Utilities.Looping
{
	/// <summary>
	/// 提供一個可重複執行的非同步循環架構，類似於 Unity 的 Update 機制。
	/// 使用者可透過繼承並覆寫各階段方法，實作客製化邏輯。
	/// </summary>
	public abstract class BaseLoopingLogic
	{
        /// <summary>
        /// 用於保護 IsLooping 的狀態切換
        /// </summary>
        private readonly object _isLoopingLock = new();
        /// <summary>
        /// 表示目前是否正在執行循環，true 表示背景迴圈仍在執行
        /// </summary>
        protected bool IsLooping { get; set; } = false;
		/// <summary>
		/// 每次更新之間的延遲時間（毫秒），預設為 500 毫秒。可由子類別覆寫。
		/// </summary>
		public virtual int UpdateIntervalMs { get; set; } = 500;
		/// <summary>
		/// 啟動循環邏輯。如果已在執行中則不會重複啟動。
		/// </summary>
		public virtual void StartLoopLogic()
		{
			lock (_isLoopingLock)
			{
                if (!IsLooping)
                {
                    IsLooping = true;
                    LoopLogic();
                }
            }
		}
		/// <summary>
		/// 停止循環邏輯。結束後會執行 EndLoop。
		/// </summary>
		public virtual void StopLoopLogic()
		{
			IsLooping = false;
		}

		/// <summary>
		/// 主循環邏輯，使用 Task 非同步執行，並依序呼叫 StartLoop、Loop的{BeforeUpdate、Update、AfterUpdate}、EndLoop。
		/// 子類別可覆寫，但通常不需修改。
		/// </summary>
		protected virtual void LoopLogic()
		{
            Task.Run(async () =>
			{
                StartLoop();
				while (IsLooping)
				{
					try
					{
						BeforeUpdate();
						Update();
						AfterUpdate();
					}
					catch (Exception ex)
					{
						HandleUpdateException(ex);
					}
					await Task.Delay(UpdateIntervalMs);
				}
				EndLoop();
			});
		}
		/// <summary>
		/// 當 Update 過程中發生例外時呼叫此方法。預設不處理，可由子類覆寫以記錄錯誤或進行處理。
		/// </summary>
		/// <param name="ex">例外物件</param>
		protected virtual void HandleUpdateException(Exception ex) { }
		#region Loop內容實際邏輯，使用者繼承後自行覆蓋設計
		/// <summary>
		/// 循環開始前執行一次，通常用於初始化狀態。
		/// </summary>
		protected virtual void StartLoop() { }
		/// <summary>
		/// 每次 Update 之前執行，通常用於邏輯準備或檢查。
		/// </summary>
		protected virtual void BeforeUpdate() { }
		/// <summary>
		/// 每次循環的主要邏輯處理區塊。
		/// </summary>
		protected virtual void Update() { }
		/// <summary>
		/// 每次 Update 之後執行，通常用於收尾或後處理。
		/// </summary>
		protected virtual void AfterUpdate() { }
		/// <summary>
		/// 循環結束後執行一次，通常用於釋放資源或記錄狀態。
		/// </summary>
		protected virtual void EndLoop() { }
		#endregion
	}
}
