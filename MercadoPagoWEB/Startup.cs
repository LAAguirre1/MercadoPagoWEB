using MercadoPagoWEB.Models; // Namespace de tus IdentityModels
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

// Atributo que le dice a IIS que esta clase 'Startup' 
// debe ejecutarse al iniciar la aplicación.
[assembly: OwinStartup(typeof(MercadoPagoWEB.Startup))]

namespace MercadoPagoWEB
{
    /// <summary>
    /// Este archivo reemplaza partes del Global.asax para 
    /// configurar la autenticación de Identity (OWIN).
    /// </summary>
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        // Para obtener más información sobre cómo configurar la autenticación, visite https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configurar el contexto de base de datos, el administrador de usuarios y el administrador de inicio de sesión
            // para usar una única instancia por solicitud
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Habilitar la aplicación para que use una cookie para almacenar información
            // para el usuario que inició sesión
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"), // La URL a la que redirigirá si el usuario no está logueado
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: System.TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
        }
    }
}