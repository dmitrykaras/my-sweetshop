using System.ComponentModel.DataAnnotations;

namespace MyCandyShop.Api.Contracts
{
    public class PatchProfileRequest
    {
        [MinLength(2)]
        public string? FirstName { get; set; }
        [MinLength(2)]
        public string? LastName { get; set; }
    }
}