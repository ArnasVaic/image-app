namespace image_app.Models;

public class Image
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public byte[]? Data { get; set; }
    public DateTime? CreatedAt { get; set; }
}
