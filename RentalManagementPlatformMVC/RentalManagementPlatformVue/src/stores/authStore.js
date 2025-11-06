import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

/**
 * 模擬的身份驗證 Store。
 * 當真實的會員模組完成後，這個檔案將會被替換。
 */
export const useAuthStore = defineStore('auth', () => {
  // 模擬一個已登入的使用者物件
  // 未來這個資訊會從 API 登入後取得
  const user = ref({
    id: 1, // 測試用的房客 (Guest) User ID
    name: '測試房客',
    email: 'guest@example.com',
    // 假設角色或權限資訊
    roles: ['guest', 'host'],
    // 假設該使用者同時也是房東，其 Host ID 為 47
    hostId: 47, 
  });

  // 計算屬性 (Getters)

  /**
   * 檢查使用者是否已登入
   * 目前總是返回 true 以供測試
   */
  const isAuthenticated = computed(() => user.value !== null);

  /**
   * 獲取當前使用者資訊
   */
  const currentUser = computed(() => user.value);

  /**
   * 獲取當前使用者的房東 ID (如果有的話)
   */
  const currentHostId = computed(() => {
    if (user.value && user.value.roles.includes('host')) {
      return user.value.hostId;
    }
    return null;
  });

  // 操作 (Actions)

  /**
   * 模擬登入
   * @param {object} loginData - 登入資訊
   */
  function login(loginData) {
    // 在真實應用中，這裡會呼叫 API
    console.log('模擬登入:', loginData);
    // 為了測試，這裡不做任何事，繼續使用預設的 user ref
  }

  /**
   * 模擬登出
   */
  function logout() {
    // 在真實應用中，這裡會呼叫 API 並清除 token
    console.log('模擬登出');
    user.value = null;
  }

  return {
    // State
    user,
    // Getters
    isAuthenticated,
    currentUser,
    currentHostId,
    // Actions
    login,
    logout,
  };
});
