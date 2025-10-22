<script setup>
import { useBookingStore } from '@/stores/bookingStore'
import { Modal } from 'bootstrap'
const bookingStore = useBookingStore()

// 格式化日期
const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

// 處理變更日期
const handleChangeDates = () => {
  console.log('變更日期')
  // 可以跳轉回房源頁面或開啟日期選擇器
}

// 處理變更客人
const handleChangeGuests = () => {
  console.log('變更客人')
  // 可以開啟客人選擇器
}

// 顯示價格明細
const showPriceDetails = () => {
  console.log('顯示價格明細')
  // 可以開啟 modal 顯示詳細資訊
}
</script>

<template>
  <div class="right-section">
    <div class="summary-card">
      <div class="property-info">
        <img :src="bookingStore.bookingDraft.propertyImage" alt="房源圖片">
        <div>
          <h4>{{ bookingStore.bookingDraft.propertyTitle }}</h4>
        </div>
      </div>

      <hr>

      <div class="cancellation">
        <strong>可免費取消</strong>
        <p>12月30日前取消可以全額退款。
          <button type="button" class="full-cancellation" data-bs-toggle="modal" data-bs-target="#cancellationModal">
            完整政策
          </button>
        </p>
      </div>

      <hr>

      <div class="info-row">
        <strong>日期</strong>
        <button type="button" class="btn-edit" data-bs-toggle="modal" data-bs-target="#dateChangeModal">變更</button>
      </div>
      <div class="date-info">
        {{ formatDate(bookingStore.bookingDraft.checkIn) }} 至
        {{ formatDate(bookingStore.bookingDraft.checkOut) }}
      </div>

      <hr>

      <div class="info-row">
        <strong>客人</strong>
        <button type="button" class="btn-edit" data-bs-toggle="modal" data-bs-target="#guestsChangeModal">變更</button>
      </div>
      <div>{{ bookingStore.bookingDraft.guests }} 名成人</div>

      <hr>

      <div class="price-section">
        <h4>價格詳情</h4>
        <div class="price-row">
          <span>{{ bookingStore.nights }} 晚 x ${{ bookingStore.bookingDraft.pricePerNight.toLocaleString() }} TWD</span>
          <span>${{ bookingStore.subtotal.toLocaleString() }} TWD</span>
        </div>
        <div class="price-row discount">
          <span>特別優惠</span>
          <span class="green">-${{ bookingStore.bookingDraft.discount.toLocaleString() }} TWD</span>
        </div>
        <hr>
        <div class="price-row total">
          <strong>總計 TWD</strong>
          <strong>${{ bookingStore.totalPrice.toLocaleString() }} TWD</strong>
        </div>
        <button class="btn-details" data-bs-toggle="modal" data-bs-target="#priceDetailsModal">價格明細</button>
      </div>

      <div class="special-offer">
        <span class="icon">⏰</span>
        <p>特別優惠：節省 ${{ bookingStore.bookingDraft.discount.toLocaleString() }} TWD。這位房東為新 3 筆預訂提供折扣。</p>
      </div>
    </div>
  </div>

  <!-- Modal -->
  <Teleport to="body">
    <!-- 取消政策 -->
    <div class="modal fade" id="cancellationModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">取消政策</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <p>12月30日前取消可以全額退款。</p>
            <p>如果在 12月30日 之後取消預訂，將無法獲得退款。</p>
          </div>
        </div>
      </div>
    </div>

    <!-- 日期變更 -->
    <div class="modal fade" id="dateChangeModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">變更日期</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <p>請選擇新的入住和退房日期</p>
            <!-- 這裡之後可以放日期選擇器 -->
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
            <button type="button" class="btn btn-primary">確認變更</button>
          </div>
        </div>
      </div>
    </div>

    <!-- 人數變更 -->
    <div class="modal fade" id="guestsChangeModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">變更客人人數</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <p>請選擇客人人數</p>
            <!-- 這裡之後可以放人數選擇器 -->
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
            <button type="button" class="btn btn-primary">確認變更</button>
          </div>
        </div>
      </div>
    </div>

    <!-- 價格明細 -->
    <div class="modal fade" id="priceDetailsModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">價格明細</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="price-row">
              <span>{{ bookingStore.nights }} 晚 x ${{ bookingStore.bookingDraft.pricePerNight.toLocaleString() }} TWD</span>
              <span>${{ bookingStore.subtotal.toLocaleString() }} TWD</span>
            </div>
            <div class="price-row">
              <span>特別優惠</span>
              <span class="text-success">-${{ bookingStore.bookingDraft.discount.toLocaleString() }} TWD</span>
            </div>
            <hr>
            <div class="price-row">
              <strong>總計 TWD</strong>
              <strong>${{ bookingStore.totalPrice.toLocaleString() }} TWD</strong>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" scoped>
.right-section {
  position: sticky;
  top: 80px;
  height: fit-content;
}

.summary-card {
  border: 1px solid #ddd;
  border-radius: 30px;
  padding: 24px;
}

.property-info {
  display: flex;
  gap: 16px;
  margin-bottom: 20px;

  img {
    width: 120px;
    height: 90px;
    border-radius: 8px;
    object-fit: cover;
  }
}

.cancellation {
  margin: 16px 0;

  strong {
    display: block;
    margin-bottom: 4px;
  }

  p {
    font-size: 14px;
    color: #717171;
  }

  .full-cancellation {
    background: none;
    border: none;
    text-decoration: underline;
    cursor: pointer;
    font-size: 14px;
  }
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;

  .btn-edit {
    background: none;
    border: none;
    text-decoration: underline;
    cursor: pointer;
    font-size: 14px;
  }
}

.date-info {
  color: #717171;
  font-size: 14px;
  margin-bottom: 16px;
}

.price-section {
  margin-top: 20px;
}

.price-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;

  .discount {
    color: #008489;

    .green {
      color: #008489;
    }
  }

  .total {
    font-size: 16px;
    padding-top: 12px;
    border-top: 1px solid #ddd;
  }
}

.btn-details {
  width: 100%;
  padding: 12px;
  background: white;
  border: 1px solid #222;
  border-radius: 8px;
  cursor: pointer;
  margin-top: 16px;
}

.special-offer {
  display: flex;
  gap: 12px;
  margin-top: 16px;
  padding: 12px;
  background: #f7f7f7;
  border-radius: 8px;
  font-size: 14px;
}

hr {
  border: none;
  border-top: 1px solid #ebebeb;
  margin: 16px 0;
}

@media (max-width: 768px) {
  .right-section {
    position: static;  /* 取消固定定位 */
  }
}
</style>
