using System.ComponentModel.DataAnnotations;

namespace FriendsBlazorApp.Models
{
    public class ProfileModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username must be between 3 and 50 characters.", MinimumLength = 3)]
        public string Username { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name must be between 1 and 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name must be between 1 and 50 characters.")]
        public string LastName { get; set; }

        [StringLength(200, ErrorMessage = "Bio cannot be more than 200 characters.")]
        public string Bio { get; set; }

        [StringLength(100, ErrorMessage = "HomeTown cannot be more than 100 characters.")]
        public string HomeTown { get; set; }

        [StringLength(100, ErrorMessage = "Occupation cannot be more than 100 characters.")]
        public string Occupation { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string ExternalLink { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime DateOfBirth { get; set; }
    }
}
