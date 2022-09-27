using System.ComponentModel.DataAnnotations;

namespace StartupProject_Asp.NetCore_PostGRE.ViewModels.Api
{
	public class RegisterViewModel
	{
		[MinLength(3, ErrorMessage = "First Name does not meet minimum length requirement"), MaxLength(32767)]
		[Display(Name = "First Name", Prompt = "Please Provide First Name")]
		[Required(ErrorMessage = "First Name is required")]
		[RegularExpression("^[a-zA-Z]*$", ErrorMessage = "Only Alphabets are allowed for First Name")]
		public string FirstName { get; set; }

		[Display(Name = "Last Name")]
		[Required(ErrorMessage = "Last Name is required")]
		[RegularExpression("^[a-zA-Z]*$", ErrorMessage = "Only Alphabets are allowed for Last Name")]
		public string LastName { get; set; }

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required(ErrorMessage = "You must provide your phone number")]
		[Display(Name = "Phone Number")]
		[DataType(DataType.PhoneNumber)]
		[StringLength(14, MinimumLength = 10)]
		[RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Not a valid USA mobile phone number")]
		public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "A paassword must be provided")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

		[Required]
		[RegularExpression("Doctor|Patient", ErrorMessage = "User role can pnly be 'Doctor' or 'Patient'")]
		public string Role { get; set; }
	}
}
