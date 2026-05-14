namespace TuTicketAPI.Dtos.Usuario
{
    public class AdminUsuarioDto
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    }
}
