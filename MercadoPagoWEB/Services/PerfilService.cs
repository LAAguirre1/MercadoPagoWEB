using MercadoPagoWEB.Models;
using System;
using System.Threading.Tasks;

namespace MercadoPagoWEB.Services
{
    /// <summary>
    /// Servicio para manejar la creación de perfiles de negocio (Usuario y CuentaDigital)
    /// después de que el registro de Identity (login) es exitoso.
    /// </summary>
    public class PerfilService
    {
        // Conexión a tu base de datos principal (la de EF6 EDMX)
        private MercadoPagoDBEntities1 db = new MercadoPagoDBEntities1();

        /// <summary>
        /// Crea un nuevo perfil de Usuario y una CuentaDigital asociada,
        /// vinculándolos al ID de usuario de ASP.NET Identity.
        /// Esta operación es transaccional.
        /// </summary>
        /// <param name="idAspNetUser">El ID (string/GUID) del usuario recién creado en la tabla AspNetUsers.</param>
        /// <param name="model">El ViewModel de registro que contiene Nombre, Apellido y DNI.</param>
        public async Task CrearPerfilYCuentaAsync(string idAspNetUser, RegisterViewModel model)
        {
            // Usamos una transacción de base de datos porque vamos a insertar en dos tablas (Usuario y CuentaDigital).
            // O ambas se crean, o ninguna lo hace.
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // --- 1. Crear el Usuario (tu tabla de negocio) ---
                    var nuevoUsuario = new Usuario
                    {
                        nombre = model.Nombre,
                        apellido = model.Apellido,
                        dni = model.DNI,
                        email = model.Email,
                        AspNetUserId = idAspNetUser, // ¡El vínculo clave con Identity!
                        password_hash = " (gestionado por ASP.NET Identity) ", // Marcador
                        fecha_registro = DateTime.Now,
                        estado_kyc = "Pendiente"
                    };

                    db.Usuario.Add(nuevoUsuario);
                    await db.SaveChangesAsync(); // Guardamos para obtener el nuevoUsuario.id_usuario (que es IDENTITY)

                    // --- 2. Crear la CuentaDigital ---
                    var nuevaCuenta = new CuentaDigital
                    {
                        id_usuario = nuevoUsuario.id_usuario, // Vinculamos a la PK del usuario recién creado
                        cvu = GenerarCVUUnico(),
                        alias = GenerarAliasUnico(model.Nombre, model.Apellido),
                        saldo_actual = 0.00m,
                        moneda = "ARS"
                    };

                    db.CuentaDigital.Add(nuevaCuenta);
                    await db.SaveChangesAsync();

                    // --- 3. Si todo salió bien, confirmamos la transacción ---
                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    // Si algo falla (ej: el CVU no es único), revertimos todos los cambios.
                    dbContextTransaction.Rollback();
                    // Relanzamos la excepción para que el AccountController se entere del error.
                    throw;
                }
            }
        }

        // --- Métodos Helpers (Simples) ---

        /// <summary>
        /// Genera un CVU numérico de 22 dígitos (simulación).
        /// </summary>
        private string GenerarCVUUnico()
        {
            // Simulación simple. En un caso real, esto debe verificar colisiones.
            Random rand = new Random();
            string p1 = rand.Next(10000000, 99999999).ToString();
            string p2 = rand.Next(10000000, 99999999).ToString();
            string p3 = rand.Next(100000, 999999).ToString();
            return $"{p1}{p2}{p3}";
        }

        /// <summary>
        /// Genera un Alias (simulación).
        /// </summary>
        private string GenerarAliasUnico(string nombre, string apellido)
        {
            // Simulación simple. En un caso real, esto debe verificar colisiones.
            Random rand = new Random();
            string aliasBase = $"{nombre.ToLower().Split(' ')[0]}.{apellido.ToLower().Split(' ')[0]}";
            return $"{aliasBase}.{rand.Next(100, 999)}.mp";
        }

        /// <summary>
        /// Libera la conexión a la base de datos cuando el servicio es destruido.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }
    }
}