using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExampleApp.Domain;

[Table(nameof(Location), Schema = "app")]
public class Location
{
    [Key]
    public int MyId { get; set; }

    [MaxLength(255)]
    public required string Name { get; set; }
}