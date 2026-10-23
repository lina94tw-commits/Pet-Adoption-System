using System;
using System.Collections.Generic;

namespace StrayAnimalAdoption.Models;

public partial class Employee
{
    public int Eid { get; set; }

    public string Ename { get; set; } = null!;

    public virtual ICollection<AdoptionApplication> Aids { get; set; } = new List<AdoptionApplication>();
}
