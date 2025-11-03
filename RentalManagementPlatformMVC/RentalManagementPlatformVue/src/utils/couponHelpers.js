//篩選、排序、狀態更新等純邏輯工具

// 篩選與排序優惠券清單
export function filterCoupons(coupons, { searchText, statusFilter, sortOption }) {
  let list = [...coupons];

  // 搜尋
  if (searchText?.trim()) {
    const keyword = searchText.toLowerCase();
    list = list.filter(c => c.couponName.toLowerCase().includes(keyword));
  }

  // 狀態篩選
  if (statusFilter && statusFilter !== 'all') {
    list = list.filter(c => c.status === statusFilter);
  }

  // 排序
  switch (sortOption) {
    case 'newest':
      list.sort((a, b) => new Date(b.startDate) - new Date(a.startDate));
      break;
    case 'endingSoon':
      list.sort((a, b) => new Date(a.endDate) - new Date(b.endDate));
      break;
    case 'largestDiscount':
      list.sort((a, b) => b.discountQuota - a.discountQuota);
      break;
  }

  return list;
}

// 自動更新狀態 (例如判斷是否過期)
export function autoUpdateCouponStatus(coupons) {
  const now = new Date();
  return coupons.map(coupon => {
    const end = new Date(coupon.endDate);
    if (end < now) coupon.status = 'expired';
    return coupon;
  });
}

// 分頁邏輯
export function paginateCoupons(coupons, currentPage, itemsPerPage) {
  const start = (currentPage - 1) * itemsPerPage;
  return coupons.slice(start, start + itemsPerPage);
}
