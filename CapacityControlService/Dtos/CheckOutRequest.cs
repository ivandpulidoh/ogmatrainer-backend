using System.ComponentModel.DataAnnotations;
namespace CapacityControlService.Dtos;
 public class CheckOutRequest
{
    [Required]
    public int UserId { get; set; }
    [Required]
    public int GymId { get; set; } // Use GymId to find the latest open check-in
}