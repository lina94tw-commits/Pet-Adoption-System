using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StrayAnimalAdoption.Models;

public partial class Pet
{
    [Key] // 聲明這是主鍵
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 關鍵：聲明這是資料庫自動遞增欄位！
    public int Pid { get; set; }

    public string? Photo {  get; set; }

    public int? Age { get; set; }

    public string Pname { get; set; } = null!;

    public DateOnly? IntakeDate { get; set; }

    public string? Species { get; set; }

    public string? Breed { get; set; }

    public string? Adoption { get; set; }

    public string? Status { get; set; }

    public string? Gender { get; set; }

    public int? Aid { get; set; }

    public int? Mid { get; set; }

    public string? VaccineStatus { get; set; }

    public virtual AdoptionApplication? AidNavigation { get; set; }

    public virtual ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();

    public virtual Member? MidNavigation { get; set; }
}
