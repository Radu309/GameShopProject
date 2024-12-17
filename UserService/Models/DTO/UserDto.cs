namespace UserService.Models.DTO;

public class UserDto
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }

    public UserDto(string email, string firstName, string lastName, string phoneNumber, string password)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Password = password;
    }
}