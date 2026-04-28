namespace TuTicketAPI.Dtos.Usuario
{
    public class UsuarioSelectDto
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? UserName { get; set; }
    }
}
