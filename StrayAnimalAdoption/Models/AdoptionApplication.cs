using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StrayAnimalAdoption.Models;

public partial class AdoptionApplication
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Aid { get; set; }

    public string? Astatus { get; set; }

    public DateOnly? Adate { get; set; }

    public int? Mid { get; set; }

    public int? Pid { get; set; } // 確保有這一行來對應資料庫的新格子

    [ForeignKey("Mid")]
    public virtual Member MidNavigation { get; set; } 

    [ForeignKey("Pid")]
    public virtual Pet PidNavigation { get; set; } 

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

    public virtual ICollection<Employee> Eids { get; set; } = new List<Employee>();
}
