namespace TuTicketAPI.Dtos.Usuario
{
    public class AuthUsuarioDto
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
    }
}
