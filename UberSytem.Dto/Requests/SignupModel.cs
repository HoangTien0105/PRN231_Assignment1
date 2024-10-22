namespace UberSytem.Dto.Requests
{
    public class SignupModel
    {

        public required string Role { get; set; }

        public required string UserName { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}
