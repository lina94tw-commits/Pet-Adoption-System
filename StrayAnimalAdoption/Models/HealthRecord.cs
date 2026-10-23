using System;
using System.Collections.Generic;

namespace StrayAnimalAdoption.Models;

public partial class HealthRecord
{
    public int Rid { get; set; }

    public int Pid { get; set; }

    public string? Ttype { get; set; }

    public DateOnly? Tdate { get; set; }

    public string? Description { get; set; }

    public virtual Pet PidNavigation { get; set; } = null!;
}
