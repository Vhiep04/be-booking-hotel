using System;
using System.Collections.Generic;

namespace be_booking_hotel.Models;

public partial class Facility
{
    public int FacilityId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
