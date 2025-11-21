using SQLite;
using HuertosApp.Models; // Importa los modelos
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HuertosApp.Services
{
    public static class SQLiteRegistroComercialExtensions
    {
        // ---------------- REGISTROS COMERCIALES ----------------

        /// <summary>
        /// Obtiene todos los registros de Cosecha Comercial.
        /// </summary>
        public static Task<List<RegistroComercial>> GetAllRegistroComercialAsync(this SQLiteAsyncConnection db)
        {
            return db.Table<RegistroComercial>().ToListAsync(); // Devuelve todos los registros.
        }

        /// <summary>
        /// Obtiene los registros NO enviados de Cosecha Comercial.
        /// </summary>
        public static Task<List<RegistroComercial>> GetAllRegistroComercialNoEnviadosAsync(this SQLiteAsyncConnection db)
        {
            return db.Table<RegistroComercial>()
                     .Where(r => !r.Enviado) // Filtra registros no enviados.
                     .ToListAsync();
        }

        /// <summary>
        /// Obtiene los registros ENVIADOS de Cosecha Comercial.
        /// </summary>
        public static Task<List<RegistroComercial>> GetAllRegistroComercialEnviadosAsync(this SQLiteAsyncConnection db)
        {
            return db.Table<RegistroComercial>()
                     .Where(r => r.Enviado) // Filtra registros enviados.
                     .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros comerciales filtrados por fecha.
        /// </summary>
        public static async Task<List<RegistroComercial>> GetRegistroComercialByFechaAsync(this SQLiteAsyncConnection db, DateTime fecha)
        {
            string fechaFiltro = fecha.ToString("yyyy-MM-dd"); // Formatea la fecha.

            var registros = await db.Table<RegistroComercial>()
                                    .Where(r => r.Fecha == fechaFiltro) // Filtra por fecha.
                                    .ToListAsync();

            return registros;
        }

        /// <summary>
        /// Inserta un nuevo registro comercial.
        /// </summary>
        public static Task<int> InsertRegistroComercialAsync(this SQLiteAsyncConnection db, RegistroComercial registro)
        {
            return db.InsertAsync(registro); // Inserta el registro.
        }

        /// <summary>
        /// Actualiza un registro comercial existente.
        /// </summary>
        public static Task<int> UpdateRegistroComercialAsync(this SQLiteAsyncConnection db, RegistroComercial registro)
        {
            return db.UpdateAsync(registro); // Actualiza el registro.
        }

        /// <summary>
        /// Elimina un registro comercial.
        /// </summary>
        public static Task<int> DeleteRegistroComercialAsync(this SQLiteAsyncConnection db, RegistroComercial registro)
        {
            return db.DeleteAsync(registro); // Elimina el registro.
        }

        // ---------------- FILTROS ADICIONALES ----------------

        ///// <summary>
        ///// Filtra registros comerciales por rango de kilos.
        ///// </summary>
        //public static async Task<List<RegistroComercial>> GetRegistrosPorKilosAsync(this SQLiteAsyncConnection db, decimal minimo, decimal maximo)
        //{
        //    var registros = await db.Table<RegistroComercial>()
        //                            .Where(r => r.Kilos >= minimo && r.Kilos <= maximo) // Filtra por rango de kilos.
        //                            .ToListAsync();

        //    return registros;
        //}

        /// <summary>
        /// Filtra registros comerciales por temporada.
        /// </summary>
        public static async Task<List<RegistroComercial>> GetRegistrosPorTemporadaAsync(this SQLiteAsyncConnection db, string temporada)
        {
            var registros = await db.Table<RegistroComercial>()
                                    .Where(r => r.Temporada == temporada) // Filtra por temporada.
                                    .ToListAsync();

            return registros;
        }

        /// <summary>
        /// Filtra registros comerciales por modalidad de cosecha.
        /// </summary>
        public static async Task<List<RegistroComercial>> GetRegistrosPorModalidadAsync(this SQLiteAsyncConnection db, string modalidad)
        {
            var registros = await db.Table<RegistroComercial>()
                                    .Where(r => r.Modalidad == modalidad) // Filtra por modalidad.
                                    .ToListAsync();

            return registros;
        }

        /// <summary>
        /// Filtra registros comerciales por cosechador.
        /// </summary>
        public static async Task<List<RegistroComercial>> GetRegistrosPorCosechadorAsync(this SQLiteAsyncConnection db, string cosechador)
        {
            var registros = await db.Table<RegistroComercial>()
                                    .Where(r => r.Cosechador.Contains(cosechador)) // Filtra por cosechador.
                                    .ToListAsync();

            return registros;
        }

        /// <summary>
        /// Filtra registros comerciales por predio.
        /// </summary>
        public static async Task<List<RegistroComercial>> GetRegistrosPorPredioAsync(this SQLiteAsyncConnection db, int predio)
        {
            var registros = await db.Table<RegistroComercial>()
                                    .Where(r => r.Predio == predio) // Filtra por predio.
                                    .ToListAsync();

            return registros;
        }

        /// <summary>
        /// Filtra registros comerciales por rodal.
        /// </summary>
        public static async Task<List<RegistroComercial>> GetRegistrosPorRodalAsync(this SQLiteAsyncConnection db, int rodal)
        {
            var registros = await db.Table<RegistroComercial>()
                                    .Where(r => r.Rodal == rodal) // Filtra por rodal.
                                    .ToListAsync();

            return registros;
        }
    }
}
