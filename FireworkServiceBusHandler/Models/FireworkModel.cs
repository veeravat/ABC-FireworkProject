using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireworkServices.Models
{
    public record FireworkModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public int Id { get; init; }
        public string Name { get; init; }
        public DateTimeOffset FireTime { get; init; }
    }
}