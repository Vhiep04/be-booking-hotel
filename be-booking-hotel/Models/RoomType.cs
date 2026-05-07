namespace be_booking_hotel.Models;

public partial class RoomType
{
    public int RoomTypeId { get; set; }
    public int HotelId { get; set; }
    public string TypeName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal PricePerNight { get; set; }
    public int Capacity { get; set; }
    public string? ImgUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Hotel Hotel { get; set; } = null!;
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    public virtual ICollection<Facility> Facilities { get; set; } = new List<Facility>();
    // RoomType.cs - thêm navigation Reservations nếu chưa có
    //public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}