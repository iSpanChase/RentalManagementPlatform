using AutoFK.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
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
                Console.WriteLine(6);
                foreach (var x in context.CouponGuests.ToList())
                {
                    var temp = tenants;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.GuestId = randomItem.UserId;
                }
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
                Console.WriteLine(8);
                foreach (var x in context.RoomLists.ToList())
                {
                    var temp = hosts;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.HostId = randomItem.UserId;
                }
                /*角色分配表不用改
                 * foreach (var x in context.UserRoles.ToList())
                {
                    randomIndex = random.Next(tenants.Count);
                    randomItem = tenants[randomIndex];
                    x.UserId = randomItem._userId;
                }*/
                Console.WriteLine(9);
                foreach (var x in context.HostPayouts.ToList())
                {
                    var temp = hosts;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.HostId = randomItem.UserId;
                }
                Console.WriteLine(10);
                foreach (var x in context.HostSubscriptions.ToList())
                {
                    var temp = hosts;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.HostId = randomItem.UserId;
                }
                Console.WriteLine(11);
                foreach (var x in context.PointLedgers.ToList())
                {
                    var temp = tenants;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.GuestId = randomItem.UserId;
                }
                Console.WriteLine(12);
                foreach (var x in context.FaqArticles.ToList())
                {
                    var temp = operators;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.AuthorId = randomItem.UserId;
                }
                Console.WriteLine(13);
                foreach (var x in context.SupportTickets.ToList())
                {
                    var temp = operators;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.AssignedStaffId = randomItem.UserId;
                }
                Console.WriteLine(14);
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
                foreach (var x in context.Posts.ToList())
                {
                    var temp = suppliers;
                    randomIndex = random.Next(temp.Count);
                    randomItem = temp[randomIndex];
                    x.UserId = randomItem.UserId;
                }
                Console.WriteLine(16);
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

                foreach (var x in context.Permissions)
                {
                    x.CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0);
                    x.UpdatedAt = x.CreatedAt;
                }
                foreach (var x in context.Roles)
                {
                    x.CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0);
                    x.UpdatedAt = x.CreatedAt;
                }
                foreach (var x in context.RolePermissions)
                {
                    x.CreatedAt = new DateTime(2024, 1, 1, 0, 1, 0);
                }


                var u_ur = context.Users.Join(context.UserRoles, u => u.UserId, ur => ur.UserId, (u, ur) => new { u, ur });
                //房客user清單
                var tenants = u_ur.Where(x => x.ur.RoleId == 2);
                Console.WriteLine(2);
                //房東user清單
                var hosts = u_ur.Where(x => x.ur.RoleId == 3);
                Console.WriteLine(3);
                //廠商user清單
                var suppliers = u_ur.Where(x => x.ur.RoleId == 4);
                Console.WriteLine(4);
                //工作人員user清單
                var operators = u_ur.Where(x => x.ur.RoleId == 5);
                //工作人員user清單
                var admin = u_ur.Where(x => x.ur.RoleId == 6);

                foreach (var x in admin)
                {
                    x.u.CreatedAt = new DateTime(2024, 1, 1, 0, 2, 0);
                    if (random.Next(100) < 30)
                        x.u.UpdatedAt = RandomDay(x.u.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.u.UpdatedAt = x.u.CreatedAt;
                }
                foreach (var x in operators)
                {
                    x.u.CreatedAt = new DateTime(2024, 1, 1, 0, 3, 0);
                    if (random.Next(100) < 30)
                        x.u.UpdatedAt = RandomDay(x.u.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.u.UpdatedAt = x.u.CreatedAt;
                }
                foreach (var x in suppliers)
                {
                    x.u.CreatedAt = RandomDay(new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2025, 8, 28, 0, 0, 0));
                    if (random.Next(100) < 30)
                        x.u.UpdatedAt = RandomDay(x.u.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.u.UpdatedAt = x.u.CreatedAt;
                }
                foreach (var x in hosts)
                {
                    x.u.CreatedAt = RandomDay(new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2025, 8, 28, 0, 0, 0));
                    if (random.Next(100) < 30)
                        x.u.UpdatedAt = RandomDay(x.u.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.u.UpdatedAt = x.u.CreatedAt;
                }
                foreach (var x in tenants)
                {
                    x.u.CreatedAt = RandomDay(new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2025, 8, 28, 0, 0, 0));
                    if (random.Next(100) < 30)
                        x.u.UpdatedAt = RandomDay(x.u.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.u.UpdatedAt = x.u.CreatedAt;
                }

                foreach (var x in context.Posts)
                {
                    x.CreatedAt = RandomDay(context.Users.Where(y => y.UserId == x.UserId).FirstOrDefault().CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    if (random.Next(100) < 30)
                        x.UpdatedAt = RandomDay(x.CreatedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.UpdatedAt = x.CreatedAt;
                    if (random.Next(100) < 30)
                        x.DeletedAt = RandomDay(x.DeletedAt, new DateTime(2025, 8, 28, 0, 0, 0));
                    else
                        x.DeletedAt = null;
                }
            }

            DateTime RandomDay(DateTime? start, DateTime? end)
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
                int range = (s - e).Days;
                return s.AddDays(random.Next(range));
            }
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
