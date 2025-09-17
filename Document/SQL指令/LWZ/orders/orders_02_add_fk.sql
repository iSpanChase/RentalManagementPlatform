-- orders_add_fk.sql
-- 建立 BOOKING_GUEST.booking_id -> BOOKING.booking_id 的外鍵
ALTER TABLE dbo.BOOKING_GUEST
ADD CONSTRAINT FK_BOOKING_GUEST_BOOKING
FOREIGN KEY (booking_id) REFERENCES dbo.BOOKING(booking_id);