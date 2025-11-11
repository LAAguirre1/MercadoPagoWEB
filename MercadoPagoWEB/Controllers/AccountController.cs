using MercadoPagoWEB.Models;
using MercadoPagoWEB.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MercadoPagoWEB.Controllers
{
    [Authorize] // Requiere que el usuario esté logueado para la mayoría de las acciones
    public class AccountController : Controller
    {
        // --- Managers de ASP.NET Identity ---
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        // --- Nuestro Servicio de Perfil de Negocio ---
        private PerfilService _perfilService;

        // --- Constructores ---
        public AccountController()
        {
            // Inicializamos nuestro servicio de perfil
            _perfilService = new PerfilService();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _perfilService = new PerfilService();
        }

        // --- Propiedades para acceder a los Managers (vía OWIN) ---
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous] // Permite a usuarios no logueados ver esta página
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Esto no cuenta los errores de inicio de sesión para el bloqueo de cuentas
            // Para habilitar los errores de contraseña para desencadenar el bloqueo, cambie a SignInStatus.Failure
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl); // Redirige a donde el usuario quería ir
                case SignInStatus.LockedOut:
                    return View("Lockout");
                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Intento de inicio de sesión no válido. Verifique Email y Contraseña.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous] // Permite a usuarios no logueados ver esta página
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // --- 1. Crear el usuario de Identity (Login y Password) ---
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // --- 2. Si Identity se creó bien, creamos el Perfil de Negocio ---
                    try
                    {
                        // Llamamos a nuestro servicio transaccional
                        await _perfilService.CrearPerfilYCuentaAsync(user.Id, model);

                        // --- 3. Si todo salió bien, logueamos al usuario ---
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // Redirigimos a la billetera (Home)
                        return RedirectToAction("Index", "Billetera");
                    }
                    catch (Exception ex)
                    {
                        // --- ¡ROLLBACK MANUAL! ---
                        // Si el PerfilService falló (ej: DNI duplicado, error de BD),
                        // debemos borrar el usuario de Identity que acabamos de crear.
                        await UserManager.DeleteAsync(user);

                        // Agregamos el error específico de la creación del perfil
                        AddErrors(new IdentityResult(ex.Message));
                    }
                }
                else
                {
                    // Si falló la creación de Identity (ej: email ya existe, password débil)
                    AddErrors(result);
                }
            }

            // Si llegamos hasta aquí, algo falló, volvemos a mostrar el formulario
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account"); // Redirige al Login
        }


        // --- Métodos de limpieza y helpers (NO BORRAR) ---

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

                if (_perfilService != null)
                {
                    _perfilService.Dispose(); // Llama al Dispose de nuestro PerfilService
                    _perfilService = null;
                }
            }

            base.Dispose(disposing);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Billetera"); // Redirige a la Billetera por defecto
        }
    }
}