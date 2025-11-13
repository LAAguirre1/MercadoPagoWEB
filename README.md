## Proyecto Billetera Digital (App MercadoPago)
Este proyecto es una implementación de una billetera digital para la materia de Aplicaciones Distribuidas.

El objetivo de esta API es exponer la lógica de negocio de nuestra billetera (transferencias e ingresos) para que pueda ser consumida por otras aplicaciones y sistemas de forma segura.

## Autenticación
Todas las solicitudes a la API deben estar autenticadas mediante una API Key.

### 1. Obtener una API Key

Para interactuar con nuestra API, primero debes solicitar una API_KEY válida al equipo de MercadoPago App.

### 2. Enviar la API Key

La API Key debe ser enviada en el header de cada solicitud HTTP, utilizando el esquema Bearer.

__Formato del Header:__
~~~
Authorization: Bearer [TU_API_KEY_AQUI]
~~~

__Ejemplo:__
~~~
Authorization: Bearer key_grupo_c#_12345
~~~
Si la API Key es inválida, está ausente o tiene el formato incorrecto, la API devolverá un error `401 Unauthorized.`


## Endpoints de la API

Nuestra API expone dos endpoints principales para mover dinero.

### 1. Realizar un Pago (Transferencia Interna)

Este endpoint se utiliza para iniciar una transferencia de dinero entre dos cuentas que existen dentro de nuestro sistema.

- **Método:** `POST`

- **Endpoint:** `/api/v1/pagos`

- **Descripción:** Ordena a nuestro sistema que debite a un `IdentificadorOrigen` y acredite a un `IdentificadorDestino`. Es una transferencia interna (A -> B).

### Request Body (JSON)
~~~
{
  "Monto": 1500.50,
  "IdentificadorOrigen": "cvu.del.que.paga",
  "IdentificadorDestino": "alias.del.que.recibe",
  "Descripcion": "Pago por el proyecto de distribuidas"
}
~~~

### Campos del Request

- `Monto` (decimal, requerido): El monto a transferir. Debe ser positivo.

- `IdentificadorOrigen` (string, requerido): El CVU o Alias de la cuenta de origen (dentro de nuestro sistema).

- `IdentificadorDestino` (string, requerido): El CVU o Alias de la cuenta de destino (dentro de nuestro sistema).

- `Descripcion` (string, opcional): Una breve descripción de la transferencia.



### Respuesta Exitosa (HTTP 200 OK)
~~~
{
    "IdTransaccion": -1,
    "Estado": "Aprobada",
    "FechaHora": "2025-11-13T12:30:00.000",
    "MontoConfirmado": 1500.50,
    "Descripcion": "Pago por el proyecto de distribuidas",
    "Mensaje": "La transferencia fue procesada exitosamente."
}
~~~

### Respuestas de Error Comunes

- `400 Bad Request`: Faltan campos requeridos, el monto es inválido, o el saldo es insuficiente.

- `401 Unauthorized`: La API Key es incorrecta o no fue enviada.

- `404 Not Found`: No se encontró la cuenta de Origen o Destino.

- `403 Forbidden`: (Próximamente) La cuenta de origen no ha completado la verificación KYC.

### 2. Recibir un Ingreso (Depósito Externo)

Este endpoint se utiliza para que un sistema externo (ej. Ualá, Grupo B) nos notifique de un ingreso de dinero para acreditar en una de nuestras cuentas.

- **Método:** `POST`

- **Endpoint:** `/api/v1/ingresos`

- **Descripción:** Registra un ingreso (crédito) en una cuenta de nuestro sistema. Es un depósito externo (Externo -> B).

### Request Body (JSON)
~~~
{
  "Monto": 120.50,
  "IdentificadorDestino": "vino.truco.asado",
  "IdTransaccionExterna": "TXN_UAL-987654321",
  "Descripcion": "Recarga de saldo desde Ualá"
}
~~~

### Campos del Request

- `Monto` (decimal, requerido): El monto a acreditar.

- `IdentificadorDestino` (string, requerido): El CVU o Alias de la cuenta de destino (dentro de nuestro sistema) que recibirá el dinero.

- `IdTransaccionExterna` (string, requerido): El ID o código de referencia único de la transacción en el sistema externo (para rastreo).

- `Descripcion` (string, opcional): Una breve descripción del ingreso.

### Respuesta Exitosa (HTTP 200 OK)
~~~
{
    "IdTransaccionInterna": -1,
    "IdTransaccionExterna": "TXN_UAL-987654321",
    "Estado": "Aprobada",
    "FechaHora": "2025-11-13T12:35:00.000",
    "MontoConfirmado": 120.50,
    "Mensaje": "El ingreso fue acreditado exitosamente."
}
~~~

### Respuestas de Error Comunes

- `400 Bad Request`: Faltan campos requeridos o el monto es inválido.

- `401 Unauthorized`: La API Key es incorrecta o no fue enviada.

- `404 Not Found`: No se encontró la cuenta de Destino.
