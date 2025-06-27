using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class Usuario
	{
		public int Id { get; set; }
		public string Correo { get; set; } = string.Empty;
        [NotMapped]
        public string Password { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; }
		public byte[] PasswordSalt { get; set; }
		public string Token { get; set; } = string.Empty;
		public string NombreUsuario { get; set; } = string.Empty;

        public int PerfilId { get; set; }
        public Perfil Perfil { get; set; }

        public int? PersonaId { get; set; }
        public Persona? Persona { get; set; }
    }
}
