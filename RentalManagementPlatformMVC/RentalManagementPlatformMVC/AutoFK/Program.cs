using AutoFK.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AutoFK
{
    internal class Program
    {
        static Random random = new Random();

        static void Main(string[] args)
        {
            CorrectionComplyUSERPermissions();
            FkAutoPair();
            CorrectionTime();
            RandomRoomListStateRemove();
            SetBookingStauts();
            GenerateHostPayouts(new DateTime(2025, 8, 28, 0, 0, 0));

            //未實裝
            RemoveAllCouponFK();
        }

        /// <summary>
        /// 移除所有優惠券相關內容
        /// </summary>
        static void RemoveAllCouponFK()
        {
            Console.WriteLine("移除優惠券未實裝");
        }

        /// <summary>
        /// 隨機把幾筆沒人住過的房子狀態改為remove
        /// </summary>
        static void RandomRoomListStateRemove()
        {
            using (var context = new RentalManagementPlatformSqlContext())
            {
                var candidates = context.RoomLists
                    .Where(r => !context.Bookings.Any(b => b.RoomId == r.RoomId)
                                && r.UpdatedAt > r.CreatedAt)
                    .ToList();

                Random random = new Random();
                foreach (var room in candidates)
                {
                    if (random.Next(100) < 20)
                    {
                        room.Status = "remove";
                    }
                }

                context.SaveChanges();
            }
        }

        /// <summary>
        /// 權限限定的操作，隨機出擁有指定權限的使用者並賦予
        /// </summary>
        static void CorrectionComplyUSERPermissions()
        {
            using (var context = new RentalManagementPlatformSqlContext())
            {
                Console.WriteLine(0);
                var urs = context.Users
                    .Join(context.UserRoles, u => u.UserId, ur => ur.UserId, (u, ur) => new { u.UserId, ur.RoleId })
                    .AsEnumerable()// 這裡切換到記憶體 LINQ
                    .Select(x => new ur(x.UserId, x.RoleId));
                Console.WriteLine(1);
                //房客user清單
                var tenants = urs.Where(x => x.RoleId == 2).ToList();
                Console.WriteLine(2);
                //房東user清單
                var hosts = urs.Where(x => x.RoleId == 3).ToList();
                Console.WriteLine(3);
                //廠商user清單
                var suppliers = urs.Where(x => x.RoleId == 4).ToList();
                Console.WriteLine(4);
                //工作人員user清單
                var operators = urs.Where(x => x.RoleId == 5).ToList();

                Random random = new Random();
                int randomIndex;
                ur randomItem;

                Console.WriteLine(5);
                foreach (var x in context.Bookings.ToList())
                {
                    var temp = tenants;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.GuestId = randomItem.UserId;
                }
                context.SaveChanges();

                Console.WriteLine(6);
                foreach (var x in context.CouponGuests.ToList())
                {
                    var temp = tenants;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.GuestId = randomItem.UserId;
                }
                context.SaveChanges();

                Console.WriteLine(7);
                foreach (var x in context.Reviews.ToList())
                {
                    var temp = hosts;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.HostId = randomItem.UserId;

                    var temp2 = tenants;
                    randomIndex = random.Next(temp2.Count);
                    randomItem = temp2[randomIndex];
                    x.ReviewerId = randomItem.UserId;
                }
                context.SaveChanges();
                Console.WriteLine(8);
                foreach (var x in context.RoomLists.ToList())
                {
                    var temp = hosts;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.HostId = randomItem.UserId;
                }
                context.SaveChanges();
                /*角色分配表不用改
                 * foreach (var x in context.UserRoles.ToList())
                {
                    randomIndex = random.Next(tenants.Count);
                    randomItem = tenants[randomIndex];
                    x.UserId = randomItem._userId;
                }*/
                Console.WriteLine(9);
                context.SaveChanges();
                foreach (var x in context.HostPayouts.ToList())
                {
                    var temp = hosts;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.HostId = randomItem.UserId;
                }
                Console.WriteLine(10);
                context.SaveChanges();
                foreach (var x in context.HostSubscriptions.ToList())
                {
                    var temp = hosts;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.HostId = randomItem.UserId;
                }
                Console.WriteLine(11);
                context.SaveChanges();
                foreach (var x in context.PointLedgers.ToList())
                {
                    var temp = tenants;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.GuestId = randomItem.UserId;
                }
                Console.WriteLine(12);
                context.SaveChanges();
                foreach (var x in context.FaqArticles.ToList())
                {
                    var temp = operators;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.AuthorId = randomItem.UserId;
                }
                Console.WriteLine(13);
                context.SaveChanges();
                foreach (var x in context.SupportTickets.ToList())
                {
                    var temp = operators;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.AssignedStaffId = randomItem.UserId;
                }
                Console.WriteLine(14);
                context.SaveChanges();
                foreach (var x in context.Messages.ToList())
                {
                    var temp = operators;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.ReceiverId = randomItem.UserId;

                    var temp2 = tenants;
                    randomIndex = random.Next(temp2.Count);
                    randomItem = temp2[randomIndex];
                    x.SenderId = randomItem.UserId;
                }
                Console.WriteLine(15);
                context.SaveChanges();
                foreach (var x in context.Posts.ToList())
                {
                    var temp = suppliers;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.UserId = randomItem.UserId;
                }
                Console.WriteLine(16);
                context.SaveChanges();
                foreach (var x in context.UserFavoriteReports.ToList())
                {
                    var temp = operators;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.UserId = randomItem.UserId;
                }
                Console.WriteLine(17);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 自動讀取"建立所有FK.sql"內容，然後根據內容給定的FK，將FK為-1的自動對應至對應表
        /// </summary>
        private static void FkAutoPair()
        {
            string filePath = @"D:\Project\RentalManagementPlatform\Document\SQL指令\建立所有FK.sql"; // 請將路徑替換為你的檔案路徑
            try
            {
                // 讀取整個檔案內容為一個字串
                string fileContent = File.ReadAllText(filePath);
                Console.WriteLine("檔案內容：");
                //Console.WriteLine(fileContent);
                SetFK(fileContent);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"錯誤：檔案 '{filePath}' 未找到。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取檔案時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 將FK為-1的自動對應至對應表
        /// </summary>
        /// <param name="fkString"></param>
        /// <exception cref="InvalidOperationException"></exception>
        static void SetFK(string fkString)
        {
            string[] fkStrings = Regex.Split(fkString, "ALTER TABLE");
            int fkCount = 0;
            int unUniqefkCount = 0;
            foreach (var st in fkStrings)
            {
                #region 取得TABLE、COL名稱
                var matches = Regex.Matches(st, @"\[(.*?)\]");
                if (matches.Count < 4)
                    continue;
                fkCount++;
                string leftTable = matches[0].Groups[1].Value;
                string leftTableCol = matches[1].Groups[1].Value;
                string rightTable = matches[2].Groups[1].Value;
                string rightTableCol = matches[3].Groups[1].Value;

                Console.WriteLine($"左：{leftTable}.{leftTableCol}，右：{rightTable}.{rightTableCol}");
                #endregion

                using (var context = new RentalManagementPlatformSqlContext())
                {
                    string sql = string.Empty;
                    if (st.Contains("--not repeatable"))
                    {
                        //隨機對應FK_無重複值
                        sql = @$"
                        ;WITH L AS (
                            SELECT L.*, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
                            FROM [{leftTable}] L
                            WHERE L.[{leftTableCol}] = -1
                        ),
                        R AS (
                            SELECT RT.[{rightTableCol}],
                                   ROW_NUMBER() OVER (ORDER BY NEWID()) AS rn
                            FROM [{rightTable}] RT
                        )
                        UPDATE L
                        SET L.[{leftTableCol}] = R.[{rightTableCol}]
                        FROM L
                        JOIN R ON L.rn = R.rn;";

                        // 先檢查數量
                        int leftCount = context.Database
                            .ExecuteSqlRaw($"SELECT COUNT(*) FROM [{leftTable}] WHERE [{leftTableCol}] = -1");
                        int rightCount = context.Database
                            .ExecuteSqlRaw($"SELECT COUNT(*) FROM [{rightTable}]");
                        if (leftCount > rightCount)
                        {
                            Console.WriteLine($"右表 [{rightTable}] 的資料筆數 ({rightCount}) 不足以分配給左表需要更新的資料筆數 ({leftCount})。");
                            continue;
                            throw new InvalidOperationException(
                                $"右表 [{rightTable}] 的資料筆數 ({rightCount}) 不足以分配給左表需要更新的資料筆數 ({leftCount})。");
                        }
                        unUniqefkCount++;
                        Console.WriteLine("--not repeatable");
                    }
                    else
                    {
                        //隨機對應FK_可有重複值
                        sql = @$"
                        UPDATE L
                        SET L.[{leftTableCol}] = R.[{rightTableCol}]
                        FROM [{leftTable}] AS L
                        CROSS APPLY (SELECT NEWID() AS seed) AS S
                        CROSS APPLY (
                            SELECT TOP 1 RT.[{rightTableCol}]
                            FROM [{rightTable}] AS RT
                            -- 用種子 + 右表鍵做雜湊，為「每一列 L」產生不同的隨機順序
                            ORDER BY CHECKSUM(S.seed, RT.[{rightTableCol}])
                        ) AS R
                        WHERE L.[{leftTableCol}] = -1;";
                    }

                    int rows = context.Database.ExecuteSqlRaw(sql);
                    Console.WriteLine($"{rows} rows updated.");
                }
            }
            Console.WriteLine($"fkCount : {fkCount}");
            Console.WriteLine($"unUniqefkCount : {unUniqefkCount}");
        }

        /// <summary>
        /// 自動校正對應FK的時間
        /// </summary>
        static void CorrectionTime()
        {
            using (var context = new RentalManagementPlatformSqlContext())
            {
                Console.WriteLine("A");
                #region 基礎設定時間設定
                foreach (var x in context.Permissions)
                {
                    x.CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0);
                    x.UpdatedAt = x.CreatedAt;
                }
                context.SaveChanges();
                foreach (var x in context.Roles)
                {
                    x.CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0);
                    x.UpdatedAt = x.CreatedAt;
                }
                context.SaveChanges();
                foreach (var x in context.RolePermissions)
                {
                    x.CreatedAt = new DateTime(2024, 1, 1, 0, 1, 0);
                }
                context.SaveChanges();
                foreach (var x in context.PointRules)
                {
                    x.CreatedAt = new DateTime(2024, 1, 1, 0, 1, 0);
                }
                context.SaveChanges();
                foreach (var x in context.FaqCategories)
                {
                    x.CreatedAt = new DateTime(2024, 1, 1, 0, 1, 0);
                    x.UpdatedAt = x.CreatedAt;
                }
                context.SaveChanges();
                foreach (var x in context.SubscriptionPlans)
                {
                    x.CreatedAt = new DateTime(2024, 1, 1, 0, 1, 0);
                }
                context.SaveChanges();
                #endregion
                Console.WriteLine("B");
                #region 帳戶創建時間設定
                var u_ur = context.Users.Join(context.UserRoles, u => u.UserId, ur => ur.UserId, (u, ur) => new { u, ur });
                //房客user清單
                var tenants = u_ur.Where(x => x.ur.RoleId == 2);
                //房東user清單
                var hosts = u_ur.Where(x => x.ur.RoleId == 3);
                //廠商user清單
                var suppliers = u_ur.Where(x => x.ur.RoleId == 4);
                //工作人員user清單
                var operators = u_ur.Where(x => x.ur.RoleId == 5);
                //工作人員user清單
                var admin = u_ur.Where(x => x.ur.RoleId == 6);
                Console.WriteLine("B1");

                foreach (var x in admin)
                {
                    x.u.CreatedAt = new DateTime(2024, 1, 1, 0, 2, 0);
                    if (random.Next(100) < 30)
                        x.u.UpdatedAt = RandomTime(x.u.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.u.UpdatedAt = x.u.CreatedAt;
                }
                context.SaveChanges();
                Console.WriteLine("B2");
                foreach (var x in operators)
                {
                    Console.WriteLine("B21");
                    x.u.CreatedAt = new DateTime(2024, 1, 1, 0, 3, 0);
                    if (random.Next(100) < 30)
                        x.u.UpdatedAt = RandomTime(x.u.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.u.UpdatedAt = x.u.CreatedAt;
                    Console.WriteLine("B22");
                }
                Console.WriteLine("B3");
                context.SaveChanges();
                foreach (var x in suppliers)
                {
                    x.u.CreatedAt = RandomTime(new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2025, 8, 28, 0, 0, 0));
                    if (random.Next(100) < 30)
                        x.u.UpdatedAt = RandomTime(x.u.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.u.UpdatedAt = x.u.CreatedAt;
                }
                Console.WriteLine("B4");
                context.SaveChanges();
                foreach (var x in hosts)
                {
                    x.u.CreatedAt = RandomTime(new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2025, 8, 28, 0, 0, 0));
                    if (random.Next(100) < 30)
                        x.u.UpdatedAt = RandomTime(x.u.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.u.UpdatedAt = x.u.CreatedAt;
                }
                Console.WriteLine("B5");
                context.SaveChanges();
                foreach (var x in tenants)
                {
                    x.u.CreatedAt = RandomTime(new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2025, 8, 28, 0, 0, 0));
                    if (random.Next(100) < 30)
                        x.u.UpdatedAt = RandomTime(x.u.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.u.UpdatedAt = x.u.CreatedAt;
                }
                Console.WriteLine("B6");

                context.SaveChanges();
                foreach (var x in context.UserRoles.Join(context.Users, ur => ur.UserId, u => u.UserId, (ur, u) => new { ur, u }))
                {
                    x.ur.CreatedAt = x.u.CreatedAt.Value;
                }
                Console.WriteLine("B7");
                #endregion
                Console.WriteLine("C");
                #region 文章時間設定
                context.SaveChanges();

                var users = context.Users.ToList();
                foreach (var x in context.Posts)
                {
                    x.CreatedAt = RandomTime(users.Where(y => y.UserId == x.UserId).FirstOrDefault().CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    if (random.Next(100) < 30)
                        x.UpdatedAt = RandomTime(x.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.UpdatedAt = x.CreatedAt;
                    x.ExpireAt = RandomTime(x.UpdatedAt, new DateTime(2026, 12, 31, 0, 0, 0));
                    if (random.Next(100) < 30)
                    {
                        if (x.ExpireAt > new DateTime(2025, 8, 28, 0, 0, 0))
                            x.DeletedAt = RandomTime(x.UpdatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                        else
                            x.DeletedAt = RandomTime(x.UpdatedAt, x.ExpireAt);
                    }
                    else
                    {
                        if (x.ExpireAt > new DateTime(2025, 8, 28, 0, 0, 0))
                            x.DeletedAt = null;
                        else
                            x.DeletedAt = x.ExpireAt;
                    }
                    if (random.Next(100) < 30)
                        x.PublishAt = RandomTime(x.CreatedAt, x.ExpireAt);
                    else
                        x.PublishAt = x.CreatedAt;
                }

                #endregion
                Console.WriteLine("D");
                #region 客服文章時間設定
                context.SaveChanges();
                foreach (var x in context.FaqArticles)
                {
                    x.CreatedAt = RandomTime(users.Where(y => y.UserId == x.AuthorId).FirstOrDefault().CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    if (random.Next(100) < 30)
                        x.UpdatedAt = RandomTime(x.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.UpdatedAt = x.CreatedAt;
                    if (random.Next(100) < 10)
                        x.PublishedAt = RandomTime(x.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.PublishedAt = x.CreatedAt;
                }
                #endregion
                Console.WriteLine("E");
                #region 房源時間設定
                context.SaveChanges();
                foreach (var x in context.RoomLists)
                {
                    x.CreatedAt = RandomTime(users.Where(y => y.UserId == x.HostId).FirstOrDefault().CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    if (random.Next(100) < 30)
                        x.UpdatedAt = RandomTime(x.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.UpdatedAt = x.CreatedAt;
                }
                context.SaveChanges();
                foreach (var x in context.Addresses.Join(context.RoomLists, a => a.AddressId, r => r.AddressId, (a, r) => new { a, r }))
                {
                    x.a.CreatedAt = x.r.CreatedAt;
                    if (x.r.CreatedAt > x.r.UpdatedAt)
                        x.a.UpdatedAt = RandomTime(x.r.CreatedAt, x.r.UpdatedAt);
                }
                #endregion
                Console.WriteLine("F");

                #region 訂單時間設定
                context.SaveChanges();
                foreach (var x in context.Bookings)
                {
                    var guestCreateTime = context.Users.Find(x.GuestId).CreatedAt;
                    var roomListCreateTime = context.RoomLists.Find(x.RoomId).CreatedAt;
                    if (guestCreateTime > roomListCreateTime)
                        x.CreatedAt = RandomTime(guestCreateTime, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.CreatedAt = RandomTime(roomListCreateTime, new DateTime(2025, 8, 28, 0, 0, 0));
                    x.CheckIn = x.CreatedAt.Value.Date.AddDays(random.Next(1, 150));
                    x.CheckOut = x.CheckIn.Value.Date.AddDays(random.Next(1, 30));
                }
                context.SaveChanges();
                foreach (var x in context.Payments.Join(context.Bookings, p => p.BookingId, b => b.BookingId, (p, b) => new { p, b }))
                {
                    x.p.CreatedAt = x.b.CreatedAt;
                    x.p.PaidAt = RandomTime(x.p.CreatedAt, x.b.CheckIn.Value.AddHours(-2));
                }
                context.SaveChanges();
                foreach (var x in context.PaymentTransactions.Join(context.Payments, pt => pt.PaymentId, p => p.PaymentId, (pt, p) => new { pt, p }))
                {
                    x.pt.CreatedAt = x.p.PaidAt;
                }
                context.SaveChanges();
                foreach (var x in context.PointLedgers
                    .Join(context.Payments, pl => pl.BookingId, p => p.BookingId, (pl, p) => new { pl, p }))
                {
                    x.pl.OccurredAt = x.p.PaidAt;
                    x.pl.ExpiresAt = x.pl.OccurredAt.Value.AddYears(1);
                }
                #endregion
                Console.WriteLine("G");

                #region 房東訂閱時間設定
                List<int> hostIdList = new List<int>();
                context.SaveChanges();
                foreach (var Group in context.Users.GroupJoin(context.HostSubscriptions, u => u.UserId, hs => hs.HostId, (u, hs) => new { u, hs }))
                {
                    DateTime earliest = Group.u.CreatedAt.Value;
                    foreach (var x in Group.hs)
                    {
                        if (earliest > new DateTime(2025, 8, 28, 0, 0, 0))
                        {
                            hostIdList.Add(x.HostId.Value);
                            continue;
                        }
                        if (random.Next(100) < 80)
                            x.CreatedAt = earliest;
                        else
                            x.CreatedAt = earliest.AddDays(random.Next(200));
                        x.StartDate = x.CreatedAt.Value.Date;
                        x.NextBillingDate = x.StartDate.Value.AddDays(30);
                        earliest = x.NextBillingDate.Value;
                    }
                }
                context.SaveChanges();
            
                for (int j = 0; j < hostIdList.Count; j++)
                {
                    var entity = context.HostSubscriptions.Find(hostIdList[j]);
                    if (entity != null)
                    {
                        context.HostSubscriptions.Remove(entity);
                    }
                }
                int z = context.SaveChanges();
                Console.WriteLine("G2");

                int i = 1;
                foreach (var x in context.HostSubscriptions)
                {
                    SubscriptionBillingLog log = new SubscriptionBillingLog()
                    {
                        BillId = i++,  // 會依序 1, 2, 3, 4...
                        HostSubId = x.HostSubId,
                        Amount = 1000,
                        PaidStatus = "paid",
                        PaidAt = x.StartDate,
                        CreatedAt = x.StartDate,
                        Note = string.Empty
                    };
                    context.SubscriptionBillingLogs.Add(log);
                }
                context.SaveChanges();
                #endregion

                Console.WriteLine("H");

                #region 最後再處理
                #region 房源評論時間設定
                foreach (var x in context.Reviews.Join(context.Bookings, r => r.BookingId, b => b.BookingId, (r, b) => new { r, b }))
                {
                    x.r.CreatedAt = RandomTime(x.b.CheckOut, x.b.CheckOut.Value.AddDays(30));
                }
                #endregion
                Console.WriteLine("I");
                #region 客服工單時間設定
                context.SaveChanges();
                foreach (var x in context.SupportTickets.Join(context.FaqFeedbacks, st => st.RelatedFeedbackId, ff => ff.FaqFeedbackId, (st, ff) => new { st, ff }))
                {
                    x.st.CreatedAt = x.ff.CreatedAt.Value.AddSeconds(random.Next(600));
                    if (random.Next(100) < 90)
                    {
                        x.st.UpdatedAt = x.st.CreatedAt.Value.AddSeconds(24 * 60 * 60 * 3);
                    }
                    else
                    {
                        x.st.UpdatedAt = x.st.CreatedAt;
                    }
                }
                #endregion
                Console.WriteLine("J");
                #region 訊息時間設定
                context.SaveChanges();
                foreach (var x in context.Messages.Join(context.Users, m => m.SenderId, s => s.UserId, (m, s) => new { m, s }).Join(context.Users, ms => ms.m.ReceiverId, r => r.UserId, (ms, r) => new { ms.m, ms.s, r }))
                {
                    if (x.s.CreatedAt > x.r.CreatedAt)
                        x.m.CreatedAt = RandomTime(x.s.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.m.CreatedAt = RandomTime(x.r.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                }
                #endregion
                Console.WriteLine("K");
                #endregion
            }
        }

        static void SetBookingStauts()
        {
            using (var context = new RentalManagementPlatformSqlContext())
            {
                context.SaveChanges();
                foreach (var x in context.Bookings)
                {
                    if (random.Next(100) < 15)
                        x.Status = "cancelled";
                    else if (x.CheckIn.Value > new DateTime(2025, 8, 28, 0, 0, 0))
                        if (random.Next(100) < 35)
                            x.Status = "pending";
                        else
                            x.Status = "confirmed";
                    else if (x.CheckOut < new DateTime(2025, 8, 28, 0, 0, 0))
                        x.Status = "completed";
                    else
                        x.Status = "cancelled";
                }
            }
        }

        /// <summary>
        /// 根據訂單自動生成HOST_SUBSCRIPTION、SUBSCRIPTION_BILLING_LOG
        /// </summary>
        /// <param name="cutoffDate"></param>
        static void GenerateHostPayouts(DateTime cutoffDate)
        {
            using (var context = new RentalManagementPlatformSqlContext())
            {
                // Step 1: 篩選需要生成 PayoutItem 的訂單
                var completedBookings = context.Bookings
                    .Where(b => b.Status == "completed" && b.CheckOut < cutoffDate)
                    .Join(context.RoomLists,
                        b => b.RoomId,
                        rl => rl.RoomId,
                        (b, rl) => new { b, rl })
                    .ToList();

                // Step 2: 依房東 + 月份分組
                var groupedByHostMonth = completedBookings
                    .GroupBy(x => new
                    {
                        x.rl.HostId,
                        Year = x.b.CheckOut.Value.Year,
                        Month = x.b.CheckOut.Value.Month
                    });

                context.SaveChanges();
                foreach (var group in groupedByHostMonth)
                {
                    int hostId = group.Key.HostId ?? 0;
                    DateTime cycleStart = new DateTime(group.Key.Year, group.Key.Month, 1);
                    DateTime cycleEnd = cycleStart.AddMonths(1).AddDays(-1);

                    // Step 3: 判斷該房東該週期是否已有 HostPayout
                    var payout = context.HostPayouts
                        .FirstOrDefault(p =>
                            p.HostId == hostId &&
                            p.CycleStart == cycleStart &&
                            p.CycleEnd == cycleEnd);

                    if (payout == null)
                    {
                        payout = new HostPayout
                        {
                            HostId = hostId,
                            CycleStart = cycleStart,
                            CycleEnd = cycleEnd,
                            AmountGross = 0,
                            PlatformFee = 0,
                            AmountNet = 0,
                            Status = "pending",
                            CreatedAt = DateTime.Now
                        };
                        context.HostPayouts.Add(payout);
                    }

                    // Step 4: 建立 HostPayoutItem
                context.SaveChanges();
                    foreach (var item in group)
                    {
                        // 判斷 checkOut 時是否有訂閱
                        bool hasSubscription = context.HostSubscriptions.Any(sub =>
                            sub.HostId == hostId &&
                            sub.Status == "active" &&
                            sub.StartDate <= item.b.CheckOut &&
                            (sub.NextBillingDate == null || sub.NextBillingDate > item.b.CheckOut));

                        decimal commissionPct = hasSubscription ? 0.1m : 0.2m; // 有訂閱抽10%，沒訂閱抽20%
                        decimal gross = item.b.TotalPrice ?? 0; // 假設 Booking 有 TotalAmount 欄位
                        decimal platformFee = gross * commissionPct;
                        decimal net = gross - platformFee;

                        var payoutItem = new HostPayoutItem
                        {
                            PayoutId = payout.PayoutId,  // EF 會自動填充（因為 payout 已經 Add）
                            BookingId = item.b.BookingId,
                            OrderNumberSnapshot = item.b.OrderNumber,
                            AmountGross = gross,
                            CommissionPct = commissionPct,
                            PlatformFee = platformFee,
                            AmountNet = net
                        };
                        context.HostPayoutItems.Add(payoutItem);

                        // 更新 Payout 總額
                        payout.AmountGross += gross;
                        payout.PlatformFee += platformFee;
                        payout.AmountNet += net;
                    }
                }

                context.SaveChanges();
            }
        }

        static DateTime RandomTime(DateTime? start, DateTime? end)
        {
            if (start == null)
            {
                Console.WriteLine("start不能是null");
                throw new Exception("start不能是null");
            }
            if (end == null)
            {
                Console.WriteLine("end不能是null");
                throw new Exception("end不能是null");
            }
            DateTime s = start.Value;
            DateTime e = end.Value;
            if (s > e)
            {
                Console.WriteLine($"s>e: {s} > {e}");
            }
            int range = (e - s).Days;
            return s.AddSeconds(random.Next(range * 24 * 60 * 60));
        }
    }

    public class ur
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public ur(int userId, int roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}
