/* ========== 1) 建議唯一索引（若已存在可略過） ========== */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_USER_Email' AND object_id = OBJECT_ID('dbo.[USER]'))
BEGIN
    CREATE UNIQUE INDEX UX_USER_Email ON dbo.[USER]([email]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_USER_Phone_NN' AND object_id = OBJECT_ID('dbo.[USER]'))
BEGIN
    CREATE UNIQUE INDEX UX_USER_Phone_NN ON dbo.[USER]([phone]) WHERE [phone] IS NOT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_USER_Username' AND object_id = OBJECT_ID('dbo.[USER]'))
BEGIN
    CREATE UNIQUE INDEX UX_USER_Username ON dbo.[USER]([username]);
END
GO

/* ========== 2) 參數（批次識別與時間範圍） ========== */
DECLARE @RunTag CHAR(8) = SUBSTRING(CONVERT(varchar(36), NEWID()), 1, 8);
DECLARE @Start  DATETIME2(7) = '2025-08-21T00:00:00';
DECLARE @End    DATETIME2(7) = '2025-08-25T23:59:59';

;WITH
Nums AS (
    SELECT TOP (200) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects
),

/* 字元集合：0-9A-Za-z（供 username 使用） */
Chars AS (
    SELECT ch FROM (VALUES
        ('0'),('1'),('2'),('3'),('4'),('5'),('6'),('7'),('8'),('9'),
        ('A'),('B'),('C'),('D'),('E'),('F'),('G'),('H'),('I'),('J'),
        ('K'),('L'),('M'),('N'),('O'),('P'),('Q'),('R'),('S'),('T'),
        ('U'),('V'),('W'),('X'),('Y'),('Z'),
        ('a'),('b'),('c'),('d'),('e'),('f'),('g'),('h'),('i'),('j'),
        ('k'),('l'),('m'),('n'),('o'),('p'),('q'),('r'),('s'),('t'),
        ('u'),('v'),('w'),('x'),('y'),('z')
    ) t(ch)
),

/* 台灣縣市⇄行政區（擴充） */
CityDistrict AS (
    SELECT v.city, v.district
    FROM (VALUES
        (N'臺北市', N'中正區'), (N'臺北市', N'大同區'), (N'臺北市', N'中山區'), (N'臺北市', N'松山區'),
        (N'臺北市', N'大安區'), (N'臺北市', N'萬華區'), (N'臺北市', N'信義區'), (N'臺北市', N'士林區'),
        (N'臺北市', N'北投區'), (N'臺北市', N'內湖區'), (N'臺北市', N'南港區'), (N'臺北市', N'文山區'),

        (N'新北市', N'板橋區'), (N'新北市', N'三重區'), (N'新北市', N'中和區'), (N'新北市', N'永和區'),
        (N'新北市', N'新莊區'), (N'新北市', N'新店區'), (N'新北市', N'土城區'), (N'新北市', N'蘆洲區'),
        (N'新北市', N'汐止區'), (N'新北市', N'淡水區'), (N'新北市', N'林口區'), (N'新北市', N'鶯歌區'),
        (N'新北市', N'樹林區'), (N'新北市', N'泰山區'), (N'新北市', N'五股區'), (N'新北市', N'三峽區'),
        (N'新北市', N'八里區'), (N'新北市', N'深坑區'), (N'新北市', N'石門區'), (N'新北市', N'金山區'),
        (N'新北市', N'雙溪區'), (N'新北市', N'貢寮區'), (N'新北市', N'坪林區'), (N'新北市', N'烏來區'),
        (N'新北市', N'平溪區'), (N'新北市', N'石碇區'), (N'新北市', N'瑞芳區'), (N'新北市', N'三芝區'),
        (N'新北市', N'萬里區'),

        (N'桃園市', N'桃園區'), (N'桃園市', N'中壢區'), (N'桃園市', N'平鎮區'), (N'桃園市', N'八德區'),
        (N'桃園市', N'龜山區'), (N'桃園市', N'蘆竹區'), (N'桃園市', N'楊梅區'), (N'桃園市', N'大溪區'),
        (N'桃園市', N'觀音區'), (N'桃園市', N'新屋區'), (N'桃園市', N'大園區'), (N'桃園市', N'龍潭區'),
        (N'桃園市', N'復興區'),

        (N'臺中市', N'中區'), (N'臺中市', N'東區'), (N'臺中市', N'南區'), (N'臺中市', N'西區'),
        (N'臺中市', N'北區'), (N'臺中市', N'北屯區'), (N'臺中市', N'西屯區'), (N'臺中市', N'南屯區'),
        (N'臺中市', N'太平區'), (N'臺中市', N'大里區'), (N'臺中市', N'霧峰區'), (N'臺中市', N'烏日區'),
        (N'臺中市', N'豐原區'), (N'臺中市', N'后里區'), (N'臺中市', N'東勢區'), (N'臺中市', N'石岡區'),
        (N'臺中市', N'新社區'), (N'臺中市', N'潭子區'), (N'臺中市', N'大雅區'), (N'臺中市', N'神岡區'),
        (N'臺中市', N'大肚區'), (N'臺中市', N'沙鹿區'), (N'臺中市', N'龍井區'), (N'臺中市', N'梧棲區'),
        (N'臺中市', N'清水區'), (N'臺中市', N'大甲區'), (N'臺中市', N'外埔區'), (N'臺中市', N'大安區'),

        (N'臺南市', N'中西區'), (N'臺南市', N'東區'), (N'臺南市', N'南區'), (N'臺南市', N'北區'),
        (N'臺南市', N'安平區'), (N'臺南市', N'安南區'), (N'臺南市', N'永康區'), (N'臺南市', N'仁德區'),
        (N'臺南市', N'新營區'), (N'臺南市', N'佳里區'), (N'臺南市', N'麻豆區'),

        (N'高雄市', N'楠梓區'), (N'高雄市', N'左營區'), (N'高雄市', N'鼓山區'), (N'高雄市', N'三民區'),
        (N'高雄市', N'苓雅區'), (N'高雄市', N'前鎮區'), (N'高雄市', N'鳳山區'), (N'高雄市', N'小港區'),
        (N'高雄市', N'岡山區'), (N'高雄市', N'路竹區'), (N'高雄市', N'阿蓮區'),

        (N'基隆市', N'仁愛區'), (N'基隆市', N'中正區'), (N'基隆市', N'信義區'), (N'基隆市', N'安樂區'),
        (N'基隆市', N'七堵區'),

        (N'新竹市', N'東區'), (N'新竹市', N'北區'), (N'新竹市', N'香山區'),

        (N'嘉義市', N'東區'), (N'嘉義市', N'西區'),

        (N'新竹縣', N'竹北市'), (N'新竹縣', N'竹東鎮'), (N'新竹縣', N'湖口鄉'), (N'新竹縣', N'新豐鄉'), (N'新竹縣', N'新埔鎮'),

        (N'苗栗縣', N'苗栗市'), (N'苗栗縣', N'頭份市'), (N'苗栗縣', N'竹南鎮'), (N'苗栗縣', N'後龍鎮'), (N'苗栗縣', N'通霄鎮'),

        (N'彰化縣', N'彰化市'), (N'彰化縣', N'員林市'), (N'彰化縣', N'和美鎮'), (N'彰化縣', N'鹿港鎮'), (N'彰化縣', N'溪湖鎮'),

        (N'南投縣', N'南投市'), (N'南投縣', N'草屯鎮'), (N'南投縣', N'竹山鎮'), (N'南投縣', N'名間鄉'), (N'南投縣', N'埔里鎮'),

        (N'雲林縣', N'斗六市'), (N'雲林縣', N'虎尾鎮'), (N'雲林縣', N'西螺鎮'), (N'雲林縣', N'斗南鎮'), (N'雲林縣', N'土庫鎮'),

        (N'嘉義縣', N'太保市'), (N'嘉義縣', N'朴子市'), (N'嘉義縣', N'民雄鄉'), (N'嘉義縣', N'新港鄉'), (N'嘉義縣', N'水上鄉'),

        (N'屏東縣', N'屏東市'), (N'屏東縣', N'潮州鎮'), (N'屏東縣', N'東港鎮'), (N'屏東縣', N'恆春鎮'), (N'屏東縣', N'萬丹鄉'),

        (N'宜蘭縣', N'宜蘭市'), (N'宜蘭縣', N'羅東鎮'), (N'宜蘭縣', N'蘇澳鎮'), (N'宜蘭縣', N'頭城鎮'), (N'宜蘭縣', N'礁溪鄉'),

        (N'花蓮縣', N'花蓮市'), (N'花蓮縣', N'吉安鄉'), (N'花蓮縣', N'新城鄉'), (N'花蓮縣', N'鳳林鎮'), (N'花蓮縣', N'壽豐鄉'),

        (N'臺東縣', N'臺東市'), (N'臺東縣', N'成功鎮'), (N'臺東縣', N'關山鎮'), (N'臺東縣', N'池上鄉'), (N'臺東縣', N'太麻里鄉'),

        (N'澎湖縣', N'馬公市'), (N'澎湖縣', N'湖西鄉'), (N'澎湖縣', N'白沙鄉'),

        (N'金門縣', N'金城鎮'), (N'金門縣', N'金沙鎮'), (N'金門縣', N'金湖鎮'),

        (N'連江縣', N'南竿鄉'), (N'連江縣', N'北竿鄉'), (N'連江縣', N'莒光鄉'), (N'連江縣', N'東引鄉')
    ) AS v(city, district)
),

/* 路名片語 */
RoadWords AS (
    SELECT v FROM (VALUES
        (N'中山路'),(N'中正路'),(N'民族路'),(N'民生路'),(N'復興南路'),(N'忠孝東路'),(N'南京東路'),
        (N'建國路'),(N'成功路'),(N'文化路'),(N'文心路'),(N'博愛路'),(N'光復南路'),(N'民權東路'),
        (N'和平東路'),(N'青年路'),(N'自由路'),(N'博學路')
    ) t(v)
),

/* 姓名用字（姓 + 名1 + 名2） */
Surnames AS (
    SELECT v FROM (VALUES
        (N'陳'),(N'林'),(N'黃'),(N'張'),(N'李'),(N'王'),(N'吳'),(N'劉'),(N'蔡'),(N'楊'),
        (N'許'),(N'鄭'),(N'謝'),(N'郭'),(N'洪'),(N'邱'),(N'曾'),(N'廖'),(N'賴'),(N'徐'),
        (N'周'),(N'葉'),(N'蘇'),(N'莊'),(N'呂'),(N'江'),(N'何'),(N'羅'),(N'高'),(N'蕭')
    ) t(v)
),
Given1 AS (
    SELECT v FROM (VALUES
        (N'怡'),(N'文'),(N'俊'),(N'家'),(N'明'),(N'雅'),(N'冠'),(N'懿'),(N'宥'),(N'品'),
        (N'佩'),(N'紹'),(N'詠'),(N'庭'),(N'偉'),(N'昱'),(N'采'),(N'語'),(N'欣'),(N'宸'),
        (N'哲'),(N'承'),(N'育'),(N'凱'),(N'子'),(N'昀'),(N'郡'),(N'筱'),(N'郁'),(N'惠')
    ) t(v)
),
Given2 AS (
    SELECT v FROM (VALUES
        (N'婷'),(N'豪'),(N'傑'),(N'妍'),(N'翔'),(N'志'),(N'誼'),(N'哲'),(N'涵'),(N'筠'),
        (N'琳'),(N'琦'),(N'瑜'),(N'辰'),(N'潔'),(N'萱'),(N'彤'),(N'均'),(N'筑'),(N'綺'),
        (N'芸'),(N'雯'),(N'峰'),(N'鈞'),(N'翰'),(N'全'),(N'薇'),(N'慈'),(N'萱'),(N'澐')
    ) t(v)
),

/* ========== 3) 造候選池（保證唯一 & 可重複執行） ========== */

/* 3a) username 候選：隨機大小寫英數 10–12 碼 */
UserNameCandidates AS (
    SELECT TOP (6000)
        (
            SELECT STRING_AGG(c2.ch, '')
            FROM (
                SELECT TOP (10 + ABS(CHECKSUM(NEWID())) % 3) ch
                FROM Chars
                ORDER BY NEWID()
            ) c2
        ) AS username
    FROM sys.all_objects
),
UserNamePool AS (
    SELECT username,
           ROW_NUMBER() OVER (ORDER BY NEWID()) AS rn
    FROM (SELECT DISTINCT username FROM UserNameCandidates) u
    WHERE NOT EXISTS (SELECT 1 FROM dbo.[USER] x WHERE x.[username] = u.username)
),

/* 3b) email = username + 網域（gmail / yahoo） */
EmailPool AS (
    SELECT
        up.rn,
        up.username,
        LOWER(CONCAT(
            up.username, '@',
            CASE WHEN (ABS(CHECKSUM(up.username)) % 2) = 0 THEN 'gmail.com' ELSE 'yahoo.com.tw' END
        )) AS email
    FROM UserNamePool up
    WHERE up.rn <= 200
    AND NOT EXISTS (
        SELECT 1 FROM dbo.[USER] e WHERE e.[email] = LOWER(CONCAT(
            up.username, '@',
            CASE WHEN (ABS(CHECKSUM(up.username)) % 2) = 0 THEN 'gmail.com' ELSE 'yahoo.com.tw' END
        ))
    )
),

/* 3c) phone 候選（09 + 8 碼），去重 + 排除既有 */
PhoneCandidates AS (
    SELECT TOP (6000)
        CONCAT('09',
               RIGHT(CONCAT('00000000',
                    ABS(CHECKSUM(NEWID())) % 100000000
               ), 8)
        ) AS phone
    FROM sys.all_objects
),
PhonePool AS (
    SELECT p.phone,
           ROW_NUMBER() OVER (ORDER BY NEWID()) AS rn
    FROM (SELECT DISTINCT phone FROM PhoneCandidates) p
    WHERE NOT EXISTS (SELECT 1 FROM dbo.[USER] u WHERE u.[phone] = p.phone)
),

/* 3d) 姓名候選（去重後取 200，確保每筆都不同） */
NameCandidates AS (
    SELECT TOP (6000)
        (SELECT TOP 1 s.v FROM Surnames s ORDER BY NEWID())
        + (SELECT TOP 1 g1.v FROM Given1  g1 ORDER BY NEWID())
        + (SELECT TOP 1 g2.v FROM Given2  g2 ORDER BY NEWID()) AS FullName
    FROM sys.all_objects
),
NamePool AS (
    SELECT FullName,
           ROW_NUMBER() OVER (ORDER BY NEWID()) AS rn
    FROM (SELECT DISTINCT FullName FROM NameCandidates) t
),

/* 3e) 地址候選（合法 city→district 配對 + 路名 + 段/巷/弄/號），去重後取 200 */
AddressCandidates AS (
    SELECT TOP (12000)
        CONCAT(
            cd.city, cd.district, r.v,
            (ABS(CHECKSUM(NEWID())) % 6 + 1), N'段',
            CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN CONCAT((ABS(CHECKSUM(NEWID())) % 50 + 1), N'巷') ELSE N'' END,
            CASE WHEN ABS(CHECKSUM(NEWID())) % 3 = 0 THEN CONCAT((ABS(CHECKSUM(NEWID())) % 30 + 1), N'弄') ELSE N'' END,
            (ABS(CHECKSUM(NEWID())) % 300 + 1), N'號'
        ) AS FullAddress
    FROM (SELECT DISTINCT city, district FROM CityDistrict) cd
    CROSS JOIN RoadWords r
),
AddressPool AS (
    SELECT FullAddress,
           ROW_NUMBER() OVER (ORDER BY NEWID()) AS rn
    FROM (SELECT DISTINCT FullAddress FROM AddressCandidates) t
),

/* 3f) 其他基本欄位（生日/點數/驗證/自動訂閱/密碼/建立時間/性別） */
BaseData AS (
    SELECT
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn,
        CASE WHEN (ABS(CHECKSUM(NEWID())) % 2) = 0 THEN N'M' ELSE N'F' END AS Gender,
        DATEADD(DAY,
            ABS(CHECKSUM(NEWID())) % DATEDIFF(DAY, '1950-01-01', '2015-12-31'),
            CAST('1950-01-01' AS date)
        ) AS BirthDate,
        ABS(CHECKSUM(NEWID())) % 51 AS PointVal,
        ABS(CHECKSUM(NEWID())) % 2 AS IsVerifiedVal,
        /* auto_subscribe 只產生 0/1 */
        ABS(CHECKSUM(NEWID())) % 2 AS AutoSubscribeVal,
        CONVERT(varchar(64), HASHBYTES('SHA2_256', CONVERT(varchar(36), NEWID())), 2) AS PasswordHashVal,
        DATEADD(SECOND,
            ABS(CHECKSUM(NEWID())) % DATEDIFF(SECOND, @Start, @End),
            @Start
        ) AS CreatedAtVal
),

/* ========== 4) 依 rn 對齊各池，確保每一筆都不同 ========== */
Pack AS (
    SELECT
        u.rn,
        u.username,
        e.email,
        ph.phone,
        nm.FullName,
        ad.FullAddress,
        bd.Gender,
        bd.BirthDate,
        bd.PointVal,
        bd.IsVerifiedVal,
        bd.AutoSubscribeVal,
        bd.PasswordHashVal,
        bd.CreatedAtVal
    FROM (SELECT rn, username FROM UserNamePool WHERE rn <= 200)            AS u
    JOIN (SELECT rn, email      FROM EmailPool    WHERE rn <= 200)            AS e  ON e.rn  = u.rn
    JOIN (SELECT rn, phone      FROM PhonePool    WHERE rn <= 200)            AS ph ON ph.rn = u.rn
    JOIN (SELECT rn, FullName   FROM NamePool     WHERE rn <= 200)            AS nm ON nm.rn = u.rn
    JOIN (SELECT rn, FullAddress FROM AddressPool WHERE rn <= 200)            AS ad ON ad.rn = u.rn
    JOIN (SELECT TOP (200) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) rn,
                 Gender, BirthDate, PointVal, IsVerifiedVal,
                 AutoSubscribeVal, PasswordHashVal, CreatedAtVal
          FROM BaseData
          ORDER BY rn)                                                        AS bd ON bd.rn = u.rn
)

/* ========== 5) 寫入資料表 ========== */
INSERT INTO dbo.[USER] (
    [username],
    [email],
    [name],
    [auto_subscribe],
    [password_hash],
    [gender],
    [birth_date],
    [phone],
    [address],
    [profile_imageurl],
    [point],
    [isverified],
    [created_at],
    [updated_at]
)
SELECT
    p.username,
    p.email,
    p.FullName,
    p.AutoSubscribeVal,               -- 0 或 1
    p.PasswordHashVal,
    p.Gender,
    p.BirthDate,
    p.phone,
    p.FullAddress,
    NULL,                             -- profile_imageurl
    p.PointVal,
    p.IsVerifiedVal,
    p.CreatedAtVal,
    DATEADD(SECOND, ABS(CHECKSUM(NEWID())) % (3*24*60*60), p.CreatedAtVal)
FROM Pack AS p;
GO
