using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RentalManagementPlatformWebAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IRoleRepository _roles;
        private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<User> _hasher;
        private readonly IWebHostEnvironment _env;
        private readonly RentalManagementPlatformSqlContext _db;

        /// <summary>
        /// 透過 DI 注入使用者/角色 Repository、密碼雜湊器、執行環境與 DbContext
        /// </summary>
        public UserService(
            IUserRepository users,
            IRoleRepository roles,
            Microsoft.AspNetCore.Identity.IPasswordHasher<User> hasher,
            IWebHostEnvironment env,
            RentalManagementPlatformSqlContext db)
        {
            _users = users;
            _roles = roles;
            _hasher = hasher;
            _env = env;
            _db = db;
        }

        /// <summary>
        /// 從 ClaimsPrincipal 中解析出目前登入使用者的 UserId
        /// - 會先找 ClaimTypes.NameIdentifier，再找 JWT 的 sub
        /// - 若解析失敗，直接丟 UnauthorizedAccessException
        /// </summary>
        private static int GetUserIdFromClaims(ClaimsPrincipal principal)
        {
            var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(sub, out var userId)) throw new UnauthorizedAccessException();
            return userId;
        }

        /// <summary>
        /// 一般註冊流程：
        /// 1. 檢查 Email / Username 是否重複
        /// 2. 檢查密碼強度（至少 8 碼，含大小寫與數字）
        /// 3. 建立 User 實體並雜湊密碼
        /// 4. 依 RoleCode 決定角色：
        ///    - ADMIN：禁止註冊選用
        ///    - OPERATOR：標記 IsOperatorPending 等待審核，不直接指派角色
        ///    - 其他（Tenant/Host/Supplier）：若角色存在則直接指派
        /// </summary>
        public async Task<UserProfileDto> RegisterAsync(RegistrationRequestDto dto)
        {
            var exist = await _users.GetByEmailAsync(dto.Email);
            if (exist != null) throw new InvalidOperationException("Email 已被使用");

            var existUsername = await _users.Query().AnyAsync(u => u.Username == dto.Username);
            if (existUsername) throw new InvalidOperationException("此帳號已被使用");

            var pw = dto.PasswordHash ?? "";

            // ✅ 密碼強度檢查
            if (!System.Text.RegularExpressions.Regex.IsMatch(pw, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$"))
                throw new InvalidOperationException("密碼需至少 8 碼，且包含英文大小寫與數字");

            var user = new User
            {
                Email = dto.Email,
                // 若 Name 未填，預設使用 Email
                Name = string.IsNullOrWhiteSpace(dto.Name) ? dto.Email : dto.Name,
                // 若 Username 未填，預設使用 email 前半段
                Username = string.IsNullOrWhiteSpace(dto.Username) ? dto.Email.Split('@')[0] : dto.Username,
                Gender = dto.Gender,
                BirthDate = dto.BirthDate,
                Address = dto.Address,
                Phone = dto.Phone ?? "",
                ProfileImageurl = dto.ProfileImageUrl,   // 實體屬性是小寫 u
                Provider = "Local",
                Isverified = false,
                CreatedAt = DateTime.UtcNow
            };

            // 使用 Identity 的 PasswordHasher 產生密碼雜湊
            user.PasswordHash = _hasher.HashPassword(user, pw);

            await _users.AddAsync(user);

            // 若註冊時有帶角色代碼，依規則處理
            if (!string.IsNullOrWhiteSpace(dto.RoleCode))
            {
                var roleCode = dto.RoleCode.Trim().ToUpperInvariant();

                // A) Admin 禁止在註冊時選
                if (roleCode == "ADMIN")
                    throw new InvalidOperationException("此角色無法在註冊時選擇。");

                // B) Operator 需審核：先標記，不立即指派
                if (roleCode == "OPERATOR")
                {
                    user.IsOperatorPending = true;
                    await _users.SaveChangesAsync();  // 寫入旗標
                }
                else
                {
                    // C) Tenant/Host/Supplier 直接指派角色（若存在）
                    var role = await _roles.GetByCodeAsync(roleCode);
                    if (role == null)
                        throw new InvalidOperationException($"角色代碼不存在：{roleCode}");
                    await _roles.AssignUserAsync(role.RoleId, user.UserId);
                }
            }
            return Map(user);
        }

        /// <summary>
        /// 取得目前登入使用者的個人資料（從 Claims 取 UserId 再查 DB）
        /// </summary>
        public async Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal principal)
        {
            var userId = GetUserIdFromClaims(principal);
            var user = await _users.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();
            return Map(user);
        }

        /// <summary>
        /// 更新目前登入使用者的個人資料：
        /// - 只覆蓋前端有送進來的欄位（以 DTO 為主）
        /// - 更新 UpdatedAt
        /// - 回傳更新後的 Profile DTO
        /// </summary>
        public async Task<UserProfileDto> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileDto dto)
        {
            var userId = GetUserIdFromClaims(principal);

            // 以 repository 取回目前使用者
            var entity = await _users.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();

            // === 逐欄更新（未送的欄位保持原值；此處依照前端送入的 DTO 全覆蓋） ===
            entity.Name = dto.Name;
            entity.Gender = dto.Gender;
            entity.BirthDate = dto.BirthDate;           // 前端送 yyyy-MM-dd，binder 已轉 DateTime
            entity.Address = dto.Address;

            entity.Phone = dto.Phone;                   // 可 null
            entity.Point = dto.Point;                   // 可 null
            entity.ProfileImageurl = dto.ProfileImageUrl; // 實體屬性命名為 ProfileImageurl（小寫 u）

            entity.UpdatedAt = DateTime.UtcNow;

            await _users.SaveChangesAsync();

            // 統一回傳最新 Profile（建議 Controller 直接 Ok(...) 回這份）
            return Map(entity);
        }

        /// <summary>
        /// 上傳大頭照：
        /// 1. 從 Claims 取得 UserId
        /// 2. 建立 /wwwroot/avatars 資料夾（若不存在）
        /// 3. 檔名包含 userId + timestamp，避免覆蓋
        /// 4. 寫檔後更新 ProfileImageurl 並儲存
        /// </summary>
        public async Task<string> UploadAvatarAsync(ClaimsPrincipal principal, IFormFile file)
        {
            if (file == null || file.Length == 0) throw new InvalidOperationException("檔案為空");

            var userId = GetUserIdFromClaims(principal);
            var user = await _users.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();

            var ext = Path.GetExtension(file.FileName);
            var fname = $"avatar_{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{ext}";
            var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "avatars");
            Directory.CreateDirectory(folder);
            var full = Path.Combine(folder, fname);

            // 實際將上傳檔案寫入磁碟
            using (var fs = File.Create(full))
            {
                await file.CopyToAsync(fs);
            }

            // 嘗試判斷檔案 ContentType（雖然這裡實際只存路徑，仍是個防呆）
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(full, out var contentType))
                contentType = "application/octet-stream";

            user.ProfileImageurl = $"/avatars/{fname}";
            await _users.SaveChangesAsync();
            return user.ProfileImageurl ?? "";
        }

        /// <summary>
        /// 透過 Email 取得 UserId（常用於重設密碼、發驗證信等）
        /// </summary>
        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var user = await _users.GetByEmailAsync(email.Trim());
            return user?.UserId;
        }

        /// <summary>
        /// 第三方登入（Google / LINE 等）綁定/建立流程：
        /// 0. 先用 ExternalLogins (provider + providerUserId) 找既有綁定帳號
        /// 1. 若有 email → 試著以 email 合併既有帳號
        /// 2. 若沒找到 → 依外部資料建立新使用者：
        ///    - 若沒有 email（LINE 常見）會幫忙產生虛擬 email，避免違反 NOT NULL/UNIQUE
        ///    - 預設指派 TENANT 角色，或依公司網域改為 HOST 等
        /// 3. 建立 ExternalLogins 關聯，避免未來重覆建立帳號
        /// </summary>
        public async Task<UserProfileDto> FindOrCreateFromExternalAsync(ExternalProfileDto dto)
        {
            // 標準化 provider / subject（LINE 常見沒 email 的情況，靠這對鍵才能找到同一人）
            var provider = (dto.Provider ?? "").Trim();
            var subject = (dto.ProviderUserId ?? "").Trim();

            // 0) 先用 ExternalLogins 查（最穩）
            if (!string.IsNullOrEmpty(provider) && !string.IsNullOrEmpty(subject))
            {
                var link = await _db.ExternalLogins
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderUserId == subject);
                if (link?.User != null)
                    return Map(link.User); // 直接回既有帳號
            }

            // 1) 有 email：先嘗試以 email 合併既有帳號
            User? user = null;
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user = await _users.GetByEmailAsync(dto.Email.Trim());
            }

            // 2) 沒找到 → 建立新帳號（第三方註冊）
            if (user == null)
            {
                // 若沒 email（LINE 常見），合成一個 email 以通過 NOT NULL/UNIQUE 約束
                var emailSafe = string.IsNullOrWhiteSpace(dto.Email)
                    ? $"{(dto.Provider ?? "ext").ToLowerInvariant()}_{Guid.NewGuid():N}@externallogin.local"
                    : dto.Email!.Trim();

                // 以 email local-part 做 base username，確保唯一
                var baseUsername = (emailSafe.Split('@').FirstOrDefault() ?? "user").ToLowerInvariant();
                var username = baseUsername;
                int suffix = 0;
                while (await _users.Query().AnyAsync(u => u.Username == username))
                    username = $"{baseUsername}{++suffix}";

                user = new User
                {
                    Email = emailSafe,
                    Name = string.IsNullOrWhiteSpace(dto.DisplayName) ? emailSafe : dto.DisplayName!,
                    Username = username,

                    // ★ 這些欄位若你的 DB 是 NOT NULL，就不要寫 null
                    Gender = "",                 // 或 "U"
                    BirthDate = default,          // 若資料表允許 NULL 可改成 null
                    Address = "",
                    Phone = "",
                    ProfileImageurl = dto.PictureUrl ?? "",

                    Provider = dto.Provider ?? "External",
                    ProviderSubject = subject,
                    Isverified = !string.IsNullOrWhiteSpace(dto.Email),
                    CreatedAt = DateTime.UtcNow,

                    // ★ 若 PasswordHash 欄位是 NOT NULL，請改成固定字串（例如 "EXTERNAL_ONLY"）
                    PasswordHash = "EXTERNAL_ONLY"
                };

                await _users.AddAsync(user);

                // === 決定要指派的角色代碼（全部轉大寫以配合 DB 的 RoleCode） ===
                string targetRoleCode = "TENANT"; // 預設

                // 依 Email 網域決定角色：例如公司網域配給 HOST
                if (!string.IsNullOrWhiteSpace(dto.Email) &&
                    dto.Email.EndsWith("@mycorp.com", StringComparison.OrdinalIgnoreCase))
                {
                    targetRoleCode = "HOST";
                }

                // 若你有供應商白名單，可加上
                // if (supplierEmails.Contains(dto.Email?.ToLowerInvariant())) targetRoleCode = "SUPPLIER";

                // === 角色存在才指派 ===
                var role = await _roles.GetByCodeAsync(targetRoleCode);
                if (role != null)
                {
                    await _roles.AssignUserAsync(role.RoleId, user.UserId);
                }

                await _users.SaveChangesAsync();
            }
            else
            {
                // 若合併既有帳號，順便補 Provider/Subject（若你的 User 有這兩欄）
                if (!string.IsNullOrEmpty(subject))
                {
                    user.Provider = provider;
                    user.ProviderSubject = subject;
                    await _users.SaveChangesAsync();
                }
            }

            // 3) ★建立 ExternalLogins 關聯（關鍵：避免下次再重覆建）
            if (!string.IsNullOrEmpty(provider) && !string.IsNullOrEmpty(subject))
            {
                var exists = await _db.ExternalLogins
                    .AnyAsync(x => x.Provider == provider && x.ProviderUserId == subject);
                if (!exists)
                {
                    _db.ExternalLogins.Add(new ExternalLogin
                    {
                        UserId = user.UserId,
                        Provider = provider,
                        ProviderUserId = subject,
                        Email = dto.Email,
                        DisplayName = dto.DisplayName,
                        PictureUrl = dto.PictureUrl,
                        CreatedAt = DateTime.UtcNow,
                    });
                    await _db.SaveChangesAsync();
                }
            }

            return Map(user!);
        }

        /// <summary>
        /// 完成第三方登入帳號的資料補齊：
        /// - 只有在 User 原本沒有 Email 時，才允許填入新的 Email
        /// - 若新 Email 已被其他帳號使用，會拒絕
        /// - 可補 DisplayName / Phone 等欄位
        /// </summary>
        public async Task<UserProfileDto> CompleteExternalAsync(int userId, CompleteExternalDto dto)
        {
            var user = await _users.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();

            // 僅當前帳號原本沒有 Email 時才允許補
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                // 檢查新 email 是否被使用
                var exist = await _users.GetByEmailAsync(dto.Email.Trim());
                if (exist != null && exist.UserId != userId)
                    throw new InvalidOperationException("Email 已被其他帳號使用");

                user.Email = dto.Email.Trim();
                user.Isverified = true;                      // 你也可以改為：寄驗證信 → 驗證成功再改 true
            }

            // 以下欄位只在原本是空值時才更新，避免覆蓋使用者原先設定
            if (!string.IsNullOrWhiteSpace(dto.DisplayName) && string.IsNullOrWhiteSpace(user.Name))
                user.Name = dto.DisplayName;

            if (!string.IsNullOrWhiteSpace(dto.Phone) && string.IsNullOrWhiteSpace(user.Phone))
                user.Phone = dto.Phone;

            user.UpdatedAt = DateTime.UtcNow;
            await _users.SaveChangesAsync();
            return Map(user);
        }


        // FAQ
        /// <summary>
        /// 依 UserId 取得 Profile（主要給 FAQ 或後台查詢使用）
        /// 使用 IQueryable + Select 投影成 UserProfileDto，避免載入多餘欄位
        /// </summary>
        public async Task<UserProfileDto?> GetProfileByIdAsync(int userId)
        {
            // 1. 使用 _users.Query() 來存取 IQueryable
            return await _users.Query()
                .Where(u => u.UserId == userId) // 注意：這裡使用 C# 實體屬性 (UserId)
                .Select(u => new UserProfileDto
                {
                    // 這裡使用 C# 實體屬性 (PascalCase)
                    UserId = u.UserId,
                    Email = u.Email ?? "",
                    Name = u.Name ?? "",
                    Username = u.Username ?? "",
                    Phone = u.Phone ?? "",
                    Address = u.Address ?? "",
                    Point = u.Point,
                    ProfileImageUrl = u.ProfileImageurl,
                    IsVerified = u.Isverified,
                    Gender = u.Gender ?? "",   // 確保 DTO 欄位完整
                    BirthDate = u.BirthDate // 確保 DTO 欄位完整
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 依 Username 查詢 Profile（FAQ 版本），常用於公開顯示使用者資料
        /// </summary>
        public async Task<UserProfileDto?> FAQGetProfileByUsername(string username)
        {
            username = username.Trim();
            if (string.IsNullOrEmpty(username)) return null;

            return await _users.Query()
                .Where(u => u.Username == username)
                .Select(u => new UserProfileDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Name = u.Name,
                    Username = u.Username,
                    Phone = u.Phone,
                    Address = u.Address,
                    Point = u.Point,
                    ProfileImageUrl = u.ProfileImageurl,
                    IsVerified = u.Isverified
                })
                .FirstOrDefaultAsync();
        }

        // === 實體 → 前端用 DTO 的映射，欄位與前端完全對齊 ===
        /// <summary>
        /// 將 User 實體轉成 UserProfileDto 統一輸出格式
        /// </summary>
        private static UserProfileDto Map(User u) => new()
        {
            UserId = u.UserId,
            Username = u.Username ?? "",
            Email = u.Email ?? "",
            Name = u.Name ?? "",
            Gender = u.Gender ?? "",
            BirthDate = u.BirthDate,
            Phone = u.Phone ?? "",
            Address = u.Address ?? "",
            Point = u.Point,
            ProfileImageUrl = u.ProfileImageurl, // 注意命名差異
            IsVerified = u.Isverified
        };
    }
}
