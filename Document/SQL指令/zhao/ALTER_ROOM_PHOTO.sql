-- 1) 移除舊的 photo_url 欄位
ALTER TABLE ROOM_PHOTO
DROP COLUMN photo_url;

-- 2) 新增 bucket / object_key / content_type 欄位
ALTER TABLE ROOM_PHOTO
ADD bucket NVARCHAR(128) NOT NULL DEFAULT N'room-photos',
    object_key NVARCHAR(512) NOT NULL,
    content_type NVARCHAR(64) NOT NULL DEFAULT N'image/jpeg';
