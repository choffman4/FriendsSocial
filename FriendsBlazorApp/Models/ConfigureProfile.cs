using System.Text.Json.Serialization;

namespace FriendsBlazorApp.Models
{
    public class ConfigureProfile
    {
        public string Guid { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string HomeTown { get; set; }
        public string Occupation { get; set; }
        public string ExternalLink { get; set; }
        public string ProfilePictureUrl { get; set; } = "";
        public string CoverPictureUrl { get; set; } = "";
        public string DateOfBirth { get; set; }

        // Add any other necessary properties or methods here
    }
}
