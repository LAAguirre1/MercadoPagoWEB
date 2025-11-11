using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MercadoPagoWEB.Models
{
    // Puedes agregar datos de perfil personalizados para el usuario 
    // agregando más propiedades a tu clase ApplicationUser. 
    // Visita https://go.microsoft.com/fwlink/?LinkID=317594 para obtener más información.

    /// <summary>
    /// Esta es la clase de usuario que ASP.NET Identity gestionará (Login, Password Hashing, etc.)
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Esta es la clase que gestionará Login/Password.
        // La vincularemos a tu tabla 'Usuario' (que tiene DNI, Nombre, etc.) 
        // a través de la columna 'AspNetUserId' que agregamos en el Paso 5 de las instrucciones.

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Tenga en cuenta que el authenticationType debe coincidir con el definido en CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Agregar aquí notificaciones personalizadas de usuario
            return userIdentity;
        }
    }

    /// <summary>
    /// Este es el DbContext que gestionará las tablas:
    /// AspNetUsers, AspNetRoles, AspNetUserClaims, etc.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            // Le dice a Identity que use la connection string "DefaultConnection"
            // que definiste en Web.config (Paso 2).
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // NOTA: No agregues DbSets para 'Usuario', 'CuentaDigital', etc. aquí.
        // Esas tablas pertenecen a tu 'MercadoPagoDBEntities1' (EDMX).
        // Este DbContext es *solo* para las tablas de Identidad.
    }
}