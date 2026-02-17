# Manual técnico: integración de inicio de sesión SGD (Matrícula CMP ↔ INTRANET)

Documento técnico para desarrolladores que integren el sistema **INTRANET** (u otro cliente) con la aplicación **Matrícula CMP**, permitiendo que usuarios inicien sesión en Matrícula usando documento de identidad y base de datos SGD.

**Base URL de la API (producción):** `https://matricula.cmp.org.pe`

---

## 1. Forma principal de acceso desde INTRANET: LoginSgdRedirect (GET)

El punto de entrada recomendado para que un usuario de INTRANET pase a Matrícula CMP es la URL **LoginSgdRedirect**, que recibe tipo de documento, número y perfil por query string.

### 1.1 Especificación del endpoint

| Propiedad   | Valor |
|------------|--------|
| **Método** | `GET` |
| **URL**    | `https://matricula.cmp.org.pe/api/usuario/LoginSgdRedirect` |

### 1.2 Parámetros de consulta (query string)

| Parámetro | Tipo   | Obligatorio | Descripción |
|-----------|--------|-------------|-------------|
| `tipo`    | string | Sí | Tipo de documento en **Base64**. Valores permitidos: `RE5J` (DNI) o `Q0U=` (CE). |
| `numero`  | string | Sí | Número del documento en **Base64**. DNI: 8 dígitos. CE: 8 o 9 dígitos. |
| `perfil`  | int    | No | Id del perfil con el que se inicia sesión. Por defecto: **2**. Ver listado de perfiles más abajo. |

### 1.3 Ejemplos de URL completas

**DNI 76777223, perfil 2 (por defecto):**
```
https://matricula.cmp.org.pe/api/usuario/LoginSgdRedirect?tipo=RE5J&numero=NzY3NzcyMjM=&perfil=2
```

**Carnet de extranjería (CE) número 123456789, perfil 1:**
```
https://matricula.cmp.org.pe/api/usuario/LoginSgdRedirect?tipo=Q0U=&numero=MTIzNDU2Nzg5&perfil=1
```

**Solo tipo y número (perfil por defecto 2):**
```
https://matricula.cmp.org.pe/api/usuario/LoginSgdRedirect?tipo=RE5J&numero=NzY3NzcyMjM=
```

### 1.4 Tipos de documento aceptados

Solo se aceptan dos tipos:

| Tipo  | Texto en Base64 | Base64   | Longitud del número |
|-------|------------------|----------|----------------------|
| **DNI** | DNI             | `RE5J`   | 8 dígitos           |
| **CE**  | CE (Carnet de Extranjería) | `Q0U=` | 8 o 9 dígitos       |

Cualquier otro tipo devolverá **400 Bad Request**.

---

## 2. Listado de perfiles

El perfil determina el rol con el que el usuario entra a Matrícula (permisos, menús, etc.). El listado actual se obtiene desde la API; no está fijo en este manual.

### 2.1 Endpoint de perfiles

| Propiedad | Valor |
|-----------|--------|
| **Método** | `GET` |
| **URL**    | `https://matricula.cmp.org.pe/api/perfiles` |

**Respuesta 200:** array JSON de objetos con `Id` y `Nombre`.

**Ejemplo de respuesta:**
```json
[
  { "id": 1, "nombre": "Admin" },
  { "id": 2, "nombre": "Médico" }
]
```

Desde INTRANET se debe llamar a este endpoint (por ejemplo al cargar la aplicación o al configurar la integración) y usar los `id` devueltos como valor del parámetro `perfil` en LoginSgdRedirect. Si no se envía `perfil`, el backend usa **2** (Médico).

---

## 3. Comportamiento cuando el usuario no está registrado en Matrícula

- **Si el documento (DNI o CE) existe en la base de datos SGD** pero **no existe aún un usuario en la base de datos local de Matrícula CMP**, el sistema **crea automáticamente** un nuevo usuario en Matrícula a partir de los datos de SGD (persona y usuario), asigna el perfil indicado en el request (o 2 por defecto) y devuelve **200 OK** con el `token`. No es necesario registrar previamente al usuario en Matrícula; el alta se hace en el primer login vía SGD.
- **Si el documento no existe en SGD**, el backend responde **404 Not Found** (usuario no encontrado en el sistema de gestión de documentos). En ese caso no se crea ningún usuario.

Resumen: **no es necesario que el usuario esté previamente registrado en Matrícula**; si está en SGD, se crea el usuario local en el primer acceso y se retorna el token.

---

## 4. Respuestas del endpoint LoginSgdRedirect (y LoginSgd)

| Código | Significado |
|--------|-------------|
| **200 OK** | Login correcto. Cuerpo: `{ "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9..." }`. El cliente debe guardar el token (p. ej. `localStorage`, cookie o pasarlo en la URL de redirección a Matrícula) y usarlo en cabecera `Authorization: Bearer {token}` en las siguientes peticiones a la API. |
| **400 Bad Request** | Tipo de documento no permitido (solo DNI/CE), número con formato inválido (longitud o no numérico), o `perfil` no existe. Cuerpo JSON con `message`. |
| **403 Forbidden** | El DNI corresponde a un **médico colegiado del CMP**. Este acceso no está permitido; el usuario debe usar “Iniciar con usuario y contraseña”. Cuerpo: `{ "message": "...", "esMedico": true }`. *(Solo aplica cuando el tipo es DNI; CE no se valida como médico.)* |
| **404 Not Found** | Usuario no encontrado en SGD, o tipo de documento no existe en catálogo SGD. No se crea usuario. |
| **500** | Error interno del servidor. |

---

## 5. Regla de negocio: médico colegiado (solo DNI)

Antes de consultar la base de datos, el backend comprueba si el **DNI** (no aplica para CE) corresponde a un médico colegiado en el CMP (servicio externo configurado en Matrícula). Si es médico colegiado, se responde **403** y no se permite el login por SGD. Para CE no se realiza esta verificación.

---

## 6. Login por POST (alternativa a GET)

Si en INTRANET se prefiere llamar por POST en lugar de redirigir por GET:

| Propiedad     | Valor |
|---------------|--------|
| **Método**    | `POST` |
| **URL**       | `https://matricula.cmp.org.pe/api/usuario/LoginSgd` |
| **Content-Type** | `application/json` |

**Cuerpo (JSON):**

| Campo                     | Tipo   | Obligatorio | Descripción |
|---------------------------|--------|-------------|-------------|
| `tipoDocumentoEncrypted` | string | Sí         | Tipo en Base64: `RE5J` (DNI) o `Q0U=` (CE). |
| `numeroDocumentoEncrypted` | string | Sí       | Número del documento en Base64. |
| `perfilId`                | int?   | No         | Id del perfil. Por defecto 2. |

**Ejemplo:**
```json
{
  "tipoDocumentoEncrypted": "RE5J",
  "numeroDocumentoEncrypted": "NzY3NzcyMjM=",
  "perfilId": 2
}
```

Las respuestas y reglas (creación de usuario si no existe en Matrícula, 403 para médico, etc.) son las mismas que para LoginSgdRedirect.

---

## 7. Consulta opcional: si un DNI es médico (solo DNI)

Para evitar enviar al usuario al login si ya se sabe que es médico:

| Propiedad | Valor |
|-----------|--------|
| **Método** | `GET` |
| **URL**    | `https://matricula.cmp.org.pe/api/reniec/consulta-es-medico/{dni}` |

`{dni}` = 8 dígitos en claro (no Base64). Solo aplica a DNI.

**Respuesta 200 (ejemplo):**  
`{ "esMedico": true, "message": "..." }` o `{ "esMedico": false, "message": "..." }`.  
Si `esMedico === true`, no llamar a LoginSgd/LoginSgdRedirect y mostrar mensaje para que use usuario y contraseña.

---

## 8. Codificación Base64 de referencia

- **JavaScript:**  
  - `btoa("DNI")` → `"RE5J"`  
  - `btoa("CE")` → `"Q0U="`  
  - `btoa("76777223")` → `"NzY3NzcyMjM="`  
  - `btoa("123456789")` → `"MTIzNDU2Nzg5"` (CE 9 dígitos)

- **C#:**  
  `Convert.ToBase64String(Encoding.UTF8.GetBytes("DNI"))` → `"RE5J"`

Al construir la URL de LoginSgdRedirect, los valores Base64 deben ir en **query string**; si el cliente lo requiere, usar codificación URL para los caracteres especiales (p. ej. `=` en Base64).

---

## 9. Flujo técnico recomendado desde INTRANET

1. Usuario ingresa en INTRANET tipo de documento (DNI o CE) y número.
2. *(Opcional, solo DNI)* Llamar a `GET /api/reniec/consulta-es-medico/{dni}`. Si `esMedico === true`, mostrar mensaje y no continuar.
3. Codificar en Base64 el tipo (`"DNI"` o `"CE"`) y el número (8 dígitos para DNI; 8 o 9 para CE).
4. O bien **redirigir** al usuario a:  
   `https://matricula.cmp.org.pe/api/usuario/LoginSgdRedirect?tipo={base64Tipo}&numero={base64Numero}&perfil={idPerfil}`  
   o bien llamar por **POST** a `/api/usuario/LoginSgd` con el JSON indicado.
5. Si la respuesta es **200**, guardar el `token` e iniciar sesión en Matrícula (redirigir a la app con el token en cookie, header o query, según diseño acordado).
6. Si es **403**, indicar que debe usar “Iniciar con usuario y contraseña”.
7. Si es **404**, indicar que el usuario no está registrado en SGD.
8. Si es **400**, mostrar el `message` devuelto (formato de documento o perfil inválido).

---

## 10. Resumen para el desarrollador

- **URL principal desde INTRANET:**  
  `https://matricula.cmp.org.pe/api/usuario/LoginSgdRedirect?tipo=RE5J&numero=NzY3NzcyMjM=&perfil=2`
- **Tipos de documento:** solo **DNI** y **CE** (Base64: `RE5J` y `Q0U=`).
- **Perfiles:** obtener listado con `GET https://matricula.cmp.org.pe/api/perfiles`; enviar el `id` deseado en el parámetro `perfil` (por defecto 2).
- **Usuario no registrado en Matrícula:** si está en SGD, se crea automáticamente y se devuelve el token; si no está en SGD, 404.
