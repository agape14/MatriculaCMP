using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	internal class RequiredIfAttribute: ValidationAttribute
	{
		private string _dependencia;
		private object _valor;

		public RequiredIfAttribute(string dependencia, object valor)
		{
			_dependencia = dependencia;
			_valor = valor;
		}

		protected override ValidationResult IsValid(object value, ValidationContext context)
		{
			var propiedad = context.ObjectType.GetProperty(_dependencia);
			var valorDependencia = propiedad?.GetValue(context.ObjectInstance);

			if (valorDependencia?.ToString() == _valor.ToString() && value == null)
			{
				return new ValidationResult(ErrorMessage);
			}
			return ValidationResult.Success;
		}
	}
}
