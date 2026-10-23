using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrayAnimalAdoption.Models; // 如果你的專案名稱不同，請修改這裡

namespace StrayAnimalAdoption.Controllers
{
    public class MembersController : Controller
    {
        private readonly Pet2Context _context;

        public MembersController(Pet2Context context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Member member)
        {
            if (ModelState.IsValid)
            {
                var emailExists = await _context.Members.AnyAsync(m => m.Email == member.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "這個 Email 已經被註冊過囉！");
                    return View(member);
                }

                member.RegistrationDate = DateOnly.FromDateTime(DateTime.Now);

                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "註冊成功！請直接登入。";
                return RedirectToAction("CustomerLogin", "Pet");
            }
            return View(member);
        }
    }
}