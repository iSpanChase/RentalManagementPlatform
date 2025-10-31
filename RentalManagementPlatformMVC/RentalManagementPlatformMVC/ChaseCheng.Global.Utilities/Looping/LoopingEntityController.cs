using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Looping
{
	/// <summary>
	/// 負責統一協調所有 <see cref="ILoopingEntity"/> 物件的更新流程。
	/// 所有受管理的物件將在每次循環中，依優先級順序執行 OnBeforeUpdate、OnUpdate、OnAfterUpdate。
	/// 所有新增與移除操作將延遲至循環的安全點進行，以避免多執行緒衝突與迴圈期間修改集合造成例外。
	/// </summary>
	public abstract class LoopingEntityController : BaseLoopingLogic
    {

		/// <summary>
		/// 用於保護同步註冊／取消註冊 的狀態
		/// 鎖定全域控制權的操作，防止多控制器競爭同一實體
		/// </summary>
		private static readonly object _entityControlLock = new();
		/// <summary>
		/// 用於保護同步存取 activeLoopObjects
		/// </summary>
		private readonly object _activeLock = new();

		/// <summary>
		/// 紀錄目前已被某個控制器正式管理的實體。
		/// 不可對外暴露此集合，僅允許透過快照存取，以避免資料競爭。
		/// </summary>
		private static readonly Dictionary<ILoopingEntity, LoopingEntityController> _controlledEntities = new();
		/// <summary>
		/// 紀錄已由某個控制器登記、等待進入控制狀態的實體（尚未正式加入）。
		/// 不可對外暴露此集合，僅允許透過快照存取，以避免資料競爭。
		/// </summary>
		private static readonly Dictionary<ILoopingEntity, LoopingEntityController> _pendingEntities = new();

		/// <summary>
		/// 記錄每個實體最後一次被分類到的優先級，用於偵測 Priority 是否變動。
		/// </summary>
		private readonly Dictionary<ILoopingEntity, int> entityPriorityMap = new();

		/// <summary>
		/// 目前已參與循環的實體，依照優先級分類儲存。
		/// 不可對外暴露此集合，僅允許透過快照存取，以避免資料競爭。
		/// </summary>
		private readonly SortedDictionary<int, List<ILoopingEntity>> activeEntitiesByPriority = new();
        /// <summary>
        /// 供迭代用的快照，確保一次更新期間集合內容不被改動。
        /// </summary>
        List<ILoopingEntity> activeEntitiesSnapshot = new();

		/// <summary>
		/// 等待加入循環的實體佇列。真正加入會在下一次迴圈開頭處理。
		/// </summary>
		public ConcurrentQueue<ILoopingEntity> pendingAddQueue { get; } = new();
        /// <summary>
        /// 等待從循環移除的實體佇列。真正移除會在本次迴圈結束時處理。
        /// </summary>
        public ConcurrentQueue<ILoopingEntity> pendingRemoveQueue { get; } = new();


		/// <summary>
		/// 登記一個物件加入循環控制，實際會在下一個循環開始前完成加入。
		/// 若該物件已被其他控制器正式管理或正在等待加入，則登記失敗。
		/// </summary>
		public virtual bool TryAdd(ILoopingEntity obj)
        {
			lock (_entityControlLock)
			{
				if (_controlledEntities.ContainsKey(obj) || _pendingEntities.ContainsKey(obj))
					return false;

				_pendingEntities[obj] = this;
				pendingAddQueue.Enqueue(obj);
				return true;
			}
		}
		/// <summary>
		/// 登記一個物件從循環中移除，實際會在本次循環結束時進行移除。
		/// 若該物件尚未正式加入但處於候補狀態，則直接取消登記。
		/// </summary>
		public virtual bool TryRemove(ILoopingEntity obj)
        {
            lock (_entityControlLock)
            {
                if (_controlledEntities.TryGetValue(obj, out var controller) && controller == this)
                {
                    pendingRemoveQueue.Enqueue(obj);
                    return true;
                }
                else if (_pendingEntities.TryGetValue(obj, out var pendingController) && pendingController == this)
                {
					_pendingEntities.Remove(obj);
					return true;
				}

                return false;
            }
        }

        /// <summary>
        /// 將所有物品從循環中清除
        /// </summary>
        void RemoveAll()
        {
            lock (_entityControlLock)
            {
                lock (_activeLock)
                {
                    foreach (var v in activeEntitiesByPriority)
                    {
                        foreach (var entity in v.Value)
                        {
                            _controlledEntities.Remove(entity);
                            entityPriorityMap.Remove(entity);
                        }
                    }
                    activeEntitiesByPriority.Clear();
                }
            }
        }
        /// <summary>
        /// 登記一個物件從循環中移除，實際會在本次循環結束時進行移除。
        /// 若該物件尚未正式加入但處於候補狀態，不移除該狀態。
        /// </summary>
        public virtual bool TryRemoveDontRemovePendingEntity(ILoopingEntity obj)
		{
			lock (_entityControlLock)
			{
				if (_controlledEntities.TryGetValue(obj, out var controller) && controller == this)
				{
					pendingRemoveQueue.Enqueue(obj);
					return true;
				}
				else if (_pendingEntities.TryGetValue(obj, out var pendingController) && pendingController == this)
				{
					_pendingEntities.Remove(obj);
					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// 將所有等待加入的物件實際加入更新清單，並呼叫其 OnStartLoop。
		/// </summary>
		protected virtual void ApplyPendingAdditions()
        {
            while (pendingAddQueue.TryDequeue(out var obj))
            {
                bool shouldAdd = false;

                lock (_entityControlLock)
                {
					// 若已換人或被搶先正式加入，就跳過
					if (_pendingEntities.TryGetValue(obj, out var controller) && controller == this)
					{
						_pendingEntities.Remove(obj);
						_controlledEntities[obj] = this;
						shouldAdd = true;
					}
				}

                if (shouldAdd)
                {
                    lock (_activeLock)
					{
						if (!activeEntitiesByPriority.TryGetValue(obj.Priority, out var list))
						{
							list = new List<ILoopingEntity>();
							activeEntitiesByPriority[obj.Priority] = list;
						}
						list.Add(obj);

						entityPriorityMap[obj] = obj.Priority; // 初始化記錄
					}

					obj.OnStartLoop();                      // 不持鎖呼叫，避免死鎖
                }
            }
        }
        /// <summary>
        /// 將所有等待移除的物件實際移出更新清單，並呼叫其 OnEndLoop。
        /// </summary>
        protected virtual void ApplyPendingRemovals()
        {
            while (pendingRemoveQueue.TryDequeue(out var obj))
            {
                bool shouldRemove = false;

                lock (_entityControlLock)
                {
                    if (_controlledEntities.TryGetValue(obj, out var controller) && controller == this)
                    {
						_controlledEntities.Remove(obj);
						shouldRemove = true;
					}
                }

                if (shouldRemove)
                {
					lock (_activeLock)
					{
						if (activeEntitiesByPriority.TryGetValue(obj.Priority, out var list))
						{
							list.Remove(obj);
							if (list.Count == 0)
								activeEntitiesByPriority.Remove(obj.Priority);
							entityPriorityMap.Remove(obj);
						}
					}

					obj.OnEndLoop();                        // 不持鎖呼叫
                }
            }
        }

		/// <summary>
		/// 統一處理所有 Priority 發生變動的實體，重新分類至對應優先級集合。
		/// 應於 AfterUpdate 之後執行。
		/// </summary>
		protected virtual void ReorganizePriorityChangedEntities()
		{
			lock (_activeLock)
			{
				foreach (var kvp in activeEntitiesByPriority.ToList())
				{
					var priority = kvp.Key;
					var list = kvp.Value.ToList(); // 快照，避免中途修改

					foreach (var entity in list)
					{
						int currentPriority = entity.Priority;
						if (!entityPriorityMap.TryGetValue(entity, out int recordedPriority))
							continue; // 理論上不應該發生

						if (currentPriority != recordedPriority)
						{
							// 移除舊的
							activeEntitiesByPriority[recordedPriority].Remove(entity);
							if (activeEntitiesByPriority[recordedPriority].Count == 0)
								activeEntitiesByPriority.Remove(recordedPriority);

							// 加入新的
							if (!activeEntitiesByPriority.TryGetValue(currentPriority, out var newList))
							{
								newList = new List<ILoopingEntity>();
								activeEntitiesByPriority[currentPriority] = newList;
							}
							newList.Add(entity);

							// 更新記錄
							entityPriorityMap[entity] = currentPriority;
						}
					}
				}
			}
		}

		/// <summary>
		/// 查詢指定的 <see cref="ILoopingEntity"/> 物件目前被誰控制
		/// </summary>
		/// <param name="obj">要被查詢的物件</param>
		/// <returns>正在控制該物件的 <see cref="LoopingEntityController"/> </returns>
		public static LoopingEntityController? GetCurrentController(ILoopingEntity obj)
		{
			lock (_entityControlLock)
			{
				_controlledEntities.TryGetValue(obj, out var controller);
				return controller;
			}
		}

		/// <summary>
		/// 覆寫基底的循環邏輯，增加了物件新增與移除的延遲處理機制。
		/// </summary>
		protected override void LoopLogic()
        {
            Task.Run(async () =>
            {
                StartLoop();
                while (IsLooping)
                {
                    ApplyPendingAdditions();
                    try
                    {
						activeEntitiesSnapshot = GetActiveLoopObjectsSnapshot();
                        BeforeUpdate();
                        Update();
                        AfterUpdate();
						ReorganizePriorityChangedEntities();
					}
                    catch (Exception ex)
                    {
                        HandleUpdateException(ex);
                    }
                    ApplyPendingRemovals();
                    await Task.Delay(UpdateIntervalMs);
                }
                EndLoop();
            });
        }

		/// <summary>
		/// 取得目前所有參與循環更新實體的快照集合，依照優先級從高至低排序。
		/// 此為一次性快照，僅反映當下狀態，不會追蹤後續變化。
		/// </summary>
		public List<ILoopingEntity> GetActiveLoopObjectsSnapshot()
        {
			lock (_activeLock)
			{
				return activeEntitiesByPriority
					.OrderByDescending(pair => pair.Key) // 高優先級優先
					.SelectMany(pair => pair.Value)
					.ToList();
			}
		}

        /// <summary>
        /// 執行所有物件的 OnBeforeUpdate。
        /// </summary>
        protected sealed override void BeforeUpdate()
        {
            foreach (var obj in activeEntitiesSnapshot)
                obj.OnBeforeUpdate();
        }
        /// <summary>
        /// 執行所有物件的 OnUpdate。
        /// </summary>
        protected sealed override void Update()
        {
            foreach (var obj in activeEntitiesSnapshot)
                obj.OnUpdate();
        }
        /// <summary>
        /// 執行所有物件的 OnAfterUpdate。
        /// </summary>
        protected sealed override void AfterUpdate()
        {
            foreach (var obj in activeEntitiesSnapshot)
                obj.OnAfterUpdate();
        }

		/// <summary>
		/// 結束循環邏輯，將當前所有物件移除
		/// </summary>
        protected override void EndLoop()
        {
            RemoveAll();
        }
    }
}
