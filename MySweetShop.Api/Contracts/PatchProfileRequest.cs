using System.ComponentModel.DataAnnotations;

namespace MySweetShop.Api.Contracts
{
    public class PatchProfileRequest
    {
        [MinLength(2)]
        public string? FirstName { get; set; }
        [MinLength(2)]
        public string? LastName { get; set; }
    }
}