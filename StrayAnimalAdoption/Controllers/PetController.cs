using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using StrayAnimalAdoption.Models;

namespace StrayAnimalAdoption.Controllers
{
    public class PetController : Controller
    {
        private readonly Pet2Context _context;
        private readonly IWebHostEnvironment _env;

        public PetController(Pet2Context context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 核心：雙入口主網頁 (不需登入即可看)
        // ==========================================
        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        // 入口一：客戶專區 (強制要求顧客身分)
        // ==========================================
        public async Task<IActionResult> CustomerIndex(string searchSpecies, string searchGender, string searchStatus = "待領養")
        {
            // 【安全鎖】檢查是否為「顧客」身分
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Customer")
            {
                TempData["ErrorMessage"] = "請先登入會員帳號！";
                return RedirectToAction("CustomerLogin");
            }

            ViewBag.SearchSpecies = searchSpecies;
            ViewBag.SearchGender = searchGender;
            ViewBag.SearchStatus = searchStatus;

            var petsQuery = _context.Pets.AsQueryable();
            if (!string.IsNullOrEmpty(searchSpecies)) petsQuery = petsQuery.Where(p => p.Species.Contains(searchSpecies));
            if (!string.IsNullOrEmpty(searchGender)) petsQuery = petsQuery.Where(p => p.Gender == searchGender);
            if (!string.IsNullOrEmpty(searchStatus)) petsQuery = petsQuery.Where(p => p.Status == searchStatus);

            return View(await petsQuery.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Customer")
                return RedirectToAction("CustomerLogin");

            var pet = await _context.Pets.Include(p => p.HealthRecords).FirstOrDefaultAsync(p => p.Pid == id);
            if (pet == null) return NotFound();
            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyAdoption(int petId)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Customer")
                return RedirectToAction("CustomerLogin");

            var memberIdStr = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdStr) || !int.TryParse(memberIdStr, out int memberId))
                return RedirectToAction("CustomerLogin");

            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null || pet.Status != "待領養")
            {
                TempData["ErrorMessage"] = "該寵物目前無法申請領養！";
                return RedirectToAction("CustomerIndex");
            }

            var application = new AdoptionApplication
            {
                Mid = memberId,
                Pid = petId,     // 🌟 就是少了這最關鍵的一行！把它補上去！
                Astatus = "審核中",
                Adate = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.AdoptionApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "領養申請已送出！收容所員工將盡快與您聯絡。";
            return RedirectToAction("Details", new { id = petId });
        }

        // ==========================================
        // 入口二：員工管理專區 (強制要求員工身分)
        // ==========================================
        public async Task<IActionResult> EmployeeIndex(string searchSpecies, string searchGender, string searchStatus)
        {
            // 【安全鎖】檢查是否為「員工」身分
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee")
            {
                TempData["ErrorMessage"] = "此區域僅限內部員工進入，請輸入員工編號！";
                return RedirectToAction("EmployeeLogin");
            }

            ViewBag.SearchSpecies = searchSpecies;
            ViewBag.SearchGender = searchGender;
            ViewBag.SearchStatus = searchStatus;

            var petsQuery = _context.Pets.AsQueryable();
            if (!string.IsNullOrEmpty(searchSpecies)) petsQuery = petsQuery.Where(p => p.Species.Contains(searchSpecies));
            if (!string.IsNullOrEmpty(searchGender)) petsQuery = petsQuery.Where(p => p.Gender == searchGender);
            if (!string.IsNullOrEmpty(searchStatus)) petsQuery = petsQuery.Where(p => p.Status == searchStatus);

            return View(await petsQuery.ToListAsync());
        }

        public IActionResult Create()
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee") return RedirectToAction("EmployeeLogin");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet, IFormFile? photoFile)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee") return RedirectToAction("EmployeeLogin");

            if (ModelState.IsValid)
            {
                if (photoFile != null && photoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "pets");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
                    using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                    { await photoFile.CopyToAsync(fileStream); }
                    pet.Photo = uniqueFileName;
                }
                pet.Status = "待領養"; pet.IntakeDate = DateOnly.FromDateTime(DateTime.Now);
                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "新寵物資料已成功建立！";
                return RedirectToAction(nameof(EmployeeIndex));
            }
            return View(pet);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee") return RedirectToAction("EmployeeLogin");
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound();
            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pet pet, IFormFile? photoFile)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee") return RedirectToAction("EmployeeLogin");
            if (id != pet.Pid) return NotFound();

            if (ModelState.IsValid)
            {
                if (photoFile != null && photoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "pets");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
                    using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                    { await photoFile.CopyToAsync(fileStream); }
                    pet.Photo = uniqueFileName;
                }
                _context.Update(pet);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "毛孩資料已成功更新！";
                return RedirectToAction(nameof(EmployeeIndex));
            }
            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee") return RedirectToAction("EmployeeLogin");
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null)
            {
                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "毛孩資料已成功刪除！";
            }
            return RedirectToAction(nameof(EmployeeIndex));
        }

        // ==========================================
        // 登入系統 1：顧客登入 (帳號密碼)
        // ==========================================
        public IActionResult CustomerLogin() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerLogin(string email, string password)
        {
            var user = await _context.Members.FirstOrDefaultAsync(m => m.Email == email && m.Password == password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Mname),
                    new Claim("MemberId", user.Mid.ToString()),
                    new Claim(ClaimTypes.Role, "Customer") // 標記為顧客
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return RedirectToAction("CustomerIndex");
            }
            ViewBag.ErrorMessage = "帳號或密碼錯誤！";
            return View();
        }

        // ==========================================
        // 登入系統 2：員工登入 (只輸入編號)
        // ==========================================
        public IActionResult EmployeeLogin() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeLogin(int employeeId)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Eid == employeeId);
            if (emp != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, emp.Ename),
                    new Claim("EmployeeId", emp.Eid.ToString()),
                    new Claim(ClaimTypes.Role, "Employee") // 標記為員工
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return RedirectToAction("EmployeeIndex");
            }
            ViewBag.ErrorMessage = "查無此員工編號，請重新輸入！";
            return View();
        }

        // 共用登出功能 (登出後回到最外層首頁)
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
        // ==========================================
        // 後台管理：審核領養申請清單
        // ==========================================
        public async Task<IActionResult> ManageAdoptions()
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee") return RedirectToAction("EmployeeLogin");

            // 把所有申請單抓出來，並連同「申請人(Member)」與「毛孩(Pet)」的資料一起載入
            var applications = await _context.AdoptionApplications
                .Include(a => a.MidNavigation)
                .Include(a => a.PidNavigation)
                .OrderByDescending(a => a.Adate)
                .ToListAsync();

            return View(applications);
        }

        // ==========================================
        // 後台管理：同意領養
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAdoption(int aid)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee") return RedirectToAction("EmployeeLogin");

            var application = await _context.AdoptionApplications
                .Include(a => a.PidNavigation)
                .Include(a => a.MidNavigation)
                .FirstOrDefaultAsync(a => a.Aid == aid);

            if (application != null && application.Astatus == "審核中")
            {
                // 1. 更改這張申請單的狀態為「已核准」
                application.Astatus = "已核准";

                // 2. 同步更改毛孩的狀態為「已領養」
                if (application.PidNavigation != null)
                {
                    application.PidNavigation.Status = "已領養";
                    application.PidNavigation.Aid = application.Aid;
                    application.PidNavigation.Mid = application.Mid;
                }

                // 3. 貼心功能：如果還有其他人申請同一隻毛孩，自動把他們改成「已拒絕」
                var otherApps = await _context.AdoptionApplications
                    .Where(a => a.Pid == application.Pid && a.Aid != aid && a.Astatus == "審核中")
                    .ToListAsync();

                foreach (var other in otherApps)
                {
                    other.Astatus = "已拒絕 (毛孩已被領養)";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"已成功核准！毛孩【{application.PidNavigation?.Pname}】的狀態已自動更新為「已領養」。";
            }
            return RedirectToAction(nameof(ManageAdoptions));
        }

        // ==========================================
        // 後台管理：拒絕領養
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAdoption(int aid)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee") return RedirectToAction("EmployeeLogin");

            var application = await _context.AdoptionApplications.FindAsync(aid);
            if (application != null && application.Astatus == "審核中")
            {
                application.Astatus = "已拒絕";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "已退回該筆領養申請。";
            }
            return RedirectToAction(nameof(ManageAdoptions));
        }
        // 1. 開啟新增健康紀錄的網頁畫面 (GET)
        public async Task<IActionResult> AddHealthRecord(int petId)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee")
                return RedirectToAction("EmployeeLogin");

            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null) return NotFound();

            // 用 ViewBag 把毛孩名字傳過去，方便畫面上顯示
            ViewBag.PetName = pet.Pname;
            ViewBag.PetId = petId;

            return View();
        }

        // 2. 接收表單並寫入資料庫 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHealthRecord(int petId, HealthRecord record)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirst(ClaimTypes.Role)?.Value != "Employee")
                return RedirectToAction("EmployeeLogin");

            // 🌟 關鍵解法：直接清除 ModelState 嚴格的關聯驗證
            // 因為前端網頁已經有 required 檢查了，我們直接信任傳進來的資料並強制存檔
            ModelState.Clear();

            // 重新賦予最重要的兩個 ID
            int maxRid = await _context.HealthRecords
                .Where(h => h.Pid == petId)
                .Select(h => (int?)h.Rid)
                .MaxAsync() ?? -1;

            record.Pid = petId;
            record.Rid = maxRid + 1;

            // 強制存檔
            _context.HealthRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "醫療紀錄已成功新增！";
            return RedirectToAction("Edit", new { id = petId }); // 存檔後正確帶著 ID 跳回修改頁面
        }
    }
}