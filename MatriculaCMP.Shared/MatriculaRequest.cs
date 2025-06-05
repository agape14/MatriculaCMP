using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class MatriculaRequest
	{
		public Persona Persona { get; set; } = new Persona();
		public Educacion Educacion { get; set; } = new Educacion();
		public IFormFile Foto { get; set; }
		public IFormFile ResolucionFile { get; set; }
	}
}
