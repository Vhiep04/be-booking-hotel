namespace be_booking_hotel.DTOs
{

    public class UploadImageRequest
    {
        public IFormFile File { get; set; }
        public string Folder { get; set; } = "general";
    }
    public class UploadImagesRequest
    {
        public List<IFormFile> Files { get; set; }
        public string Folder { get; set; } = "general";
    }
}
