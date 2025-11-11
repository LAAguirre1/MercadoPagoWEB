using MercadoPagoWEB.Excepcion;   // O la ruta a tu Excepción
using MercadoPagoWEB.Models; // Namespace de tu DbContext y Modelos
using System;
using System.Data.Entity; // Entity Framework 6
using System.Data.SqlClient; // Para SqlParameter
using System.Linq;
using System.Threading.Tasks;

namespace MercadoPagoWEB.Services
{
    /// <summary>
    /// Implementación del servicio de transferencias para .NET Framework (EF6).
    /// </summary>
    public class TransferenciaService : ITransferenciaService
    {
        // Instancia el DbContext.
        // Asume que el DbContext se llama 'MercadoPagoDBEntities1'
        private readonly MercadoPagoDBEntities1 _context = new MercadoPagoDBEntities1();

        public async Task RealizarTransferenciaAsync(TransferenciaViewModel model, int idUsuarioOrigen)
        {
            // 1. Obtener la cuenta de origen (el usuario logueado)
            var cuentaOrigen = await _context.CuentaDigital
                .FirstOrDefaultAsync(c => c.id_usuario == idUsuarioOrigen);

            if (cuentaOrigen == null)
            {
                throw new TransferenciaException("No se encontró tu cuenta digital. Contacta a soporte.");
            }

            // 2. Obtener la cuenta de destino por CVU o Alias
            var cuentaDestino = await _context.CuentaDigital
                .FirstOrDefaultAsync(c => c.cvu == model.CvuOAliasDestino || c.alias == model.CvuOAliasDestino);

            if (cuentaDestino == null)
            {
                throw new TransferenciaException("La cuenta de destino (CVU o Alias) no existe o es incorrecta.");
            }

            // 3. Validaciones de negocio
            if (cuentaOrigen.id_cuenta == cuentaDestino.id_cuenta)
            {
                throw new TransferenciaException("No puedes transferirte dinero a ti mismo.");
            }

            if (cuentaOrigen.saldo_actual < model.Monto)
            {
                throw new TransferenciaException("No tienes saldo suficiente para realizar esta operación.");
            }

            // 4. Preparar los parámetros para el Stored Procedure
            var pIdOrigen = new SqlParameter("@id_origen", cuentaOrigen.id_cuenta);
            var pIdDestino = new SqlParameter("@id_destino", cuentaDestino.id_cuenta);
            var pMonto = new SqlParameter("@monto", model.Monto);
            // Manejo de nulos para .NET Framework
            var pDescripcion = new SqlParameter("@descripcion", (object)model.Descripcion ?? DBNull.Value);

            // 5. Ejecutar el Stored Procedure
            try
            {
                // Usamos ExecuteSqlCommandAsync para EF6 (en lugar de ExecuteSqlRawAsync de EF Core)
                await _context.Database.ExecuteSqlCommandAsync(
                    "EXEC SP_RealizarTransferencia @id_origen, @id_destino, @monto, @descripcion",
                    pIdOrigen, pIdDestino, pMonto, pDescripcion
                );
            }
            catch (SqlException ex)
            {
                // Capturamos la excepción de SQL Server.
                if (ex.Number >= 50000)
                {
                    // Error de negocio personalizado desde el SP (THROW)
                    throw new TransferenciaException(ex.Message);
                }
                else
                {
                    // Otro error de SQL
                    throw new TransferenciaException($"Error de base de datos: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // Captura de cualquier otro error
                throw new TransferenciaException($"Ocurrió un error inesperado: {ex.Message}");
            }
        }
    }
}