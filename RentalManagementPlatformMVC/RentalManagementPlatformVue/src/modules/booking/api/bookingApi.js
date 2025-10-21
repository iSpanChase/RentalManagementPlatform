// 獲取訂單列表
export async function getBookingList() {
  const response = await fetch('https://localhost:7230/api/Booking');
  const data = await response.json();
  return data;
}
