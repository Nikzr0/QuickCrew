namespace QuickCrew.Models
{
    public sealed class RegisterInputModel
    {
        public required string Email { get; init; }

        public required string Name { get; init; }

        public required string Password { get; init; }
    }
}
