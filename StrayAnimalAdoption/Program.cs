using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
// 這裡的 StrayAnimalAdoption 請替換成你實際建立的專案名稱
using StrayAnimalAdoption.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// === 註冊 Pet2Context 資料庫環境 ===
builder.Services.AddDbContext<Pet2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// === 註冊 Cookie 驗證服務 (把沒寫完的地方補齊了) ===
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Pets/Login"; // 如果未登入就想看受保護的網頁，會被自動導向這個登入頁
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // 設定登入狀態保留 7 天
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// === 務必加上這行：啟用身分驗證 (這行必須夾在 UseRouting 和 UseAuthorization 之間) ===
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    // 這裡的 Home 改成 Pets (請確認你控制器的實際名稱有沒有 s)
    pattern: "{controller=Pets}/{action=Index}/{id?}");

app.Run();