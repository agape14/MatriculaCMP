using System.Security.Cryptography;
using System.Text;

namespace MatriculaCMP.Services
{
	/// <summary>
	/// Construye la URL de autorización ID Perú con el parámetro vd según especificación RENIEC.
	/// Código Fuente 10 y 11: vd = DNI encriptado (AES-CBC, PKCS5, Base64 + URL encoding).
	/// </summary>
	public class IdPeruUrlService
	{
		private readonly IConfiguration _config;

		public IdPeruUrlService(IConfiguration config)
		{
			_config = config;
		}

		/// <summary>
		/// Construye la URL completa de solicitud de autenticación ID Perú.
		/// Incluye todos los parámetros según documentación: redirect_uri, state, vd (si hay DNI), etc.
		/// </summary>
		/// <param name="redirectUri">URL de callback (debe estar registrada en RENIEC). Será URL-encoded.</param>
		/// <param name="state">State (Código Fuente 8-9).</param>
		/// <param name="nonce">Nonce para OIDC (opcional).</param>
		/// <param name="dni">DNI de 8 dígitos para el parámetro vd (opcional). Si se proporciona, se encripta e incluye.</param>
		public string BuildAuthorizationUrl(string redirectUri, string state, string? nonce = null, string? dni = null)
		{
			var authUri = _config["IDPeru:AuthUri"] ?? _config["IDPeru:Authority"] + "/service/auth";
			var clientId = _config["IDPeru:ClientId"] ?? "";
			var acr = _config["IDPeru:Acr"] ?? "";

			var query = new Dictionary<string, string>
			{
				["client_id"] = clientId,
				["response_type"] = "code",
				["redirect_uri"] = redirectUri,
				["scope"] = "openid profile",
				["state"] = state
			};

			if (!string.IsNullOrWhiteSpace(nonce))
				query["nonce"] = nonce;
			if (!string.IsNullOrWhiteSpace(acr))
				query["acr_values"] = acr;

			// Parámetro vd: DNI encriptado según Código Fuente 10 (AES-CBC, PKCS5, Base64 + URL encoded)
			if (!string.IsNullOrEmpty(dni) && dni.Length == 8 && dni.All(char.IsDigit) &&
			    !string.IsNullOrEmpty(clientId) && clientId.Length >= 16)
			{
				var vdValor = EncriptarDniParaVd(dni, clientId);
				query["vd"] = vdValor;
			}

			// Construir URL con percent-encoding (application/x-www-form-urlencoded)
			var sb = new StringBuilder();
			sb.Append(authUri);
			sb.Append("?");
			sb.Append(string.Join("&", query.Select(p => $"{p.Key}={UrlEncode(p.Value)}")));

			return sb.ToString();
		}

		/// <summary>
		/// Genera el parámetro state según Código Fuente 8: timestamp Unix → Base64 → URL encoding.
		/// </summary>
		public static string GenerarState()
		{
			var timestamp = ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
			var stateBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(timestamp));
			return UrlEncode(stateBase64);
		}

		/// <summary>
		/// Encripta el DNI para el parámetro vd según Código Fuente 10.
		/// AES-CBC, PKCS5/PKCS7, Base64. Si urlEncode=false (para SetParameter), el middleware aplica encoding.
		/// </summary>
		public static string EncriptarDniParaVd(string dni, string clientId, bool urlEncode = true)
		{
			var key = Encoding.UTF8.GetBytes(clientId.Substring(0, 16));
			var iv = Encoding.UTF8.GetBytes(clientId.Substring(0, 16));
			var inputBytes = Encoding.UTF8.GetBytes(dni);

			using var aes = Aes.Create();
			aes.Key = key;
			aes.IV = iv;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			using var encryptor = aes.CreateEncryptor();
			var encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
			var base64Encrypted = Convert.ToBase64String(encryptedBytes);
			return urlEncode ? UrlEncode(base64Encrypted) : base64Encrypted;
		}

		/// <summary>
		/// Percent-encoding (URL encoding) según application/x-www-form-urlencoded.
		/// : → %3A, / → %2F, = → %3D, etc.
		/// </summary>
		private static string UrlEncode(string value)
		{
			if (string.IsNullOrEmpty(value)) return "";
			return Uri.EscapeDataString(value);
		}
	}
}
