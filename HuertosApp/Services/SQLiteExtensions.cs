using SQLite;
using HuertosApp.Models; // Importa el modelo Fertirriego
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuertosApp.Services
{
    public static class SQLiteExtensions
    {
        // Método de extensión para obtener todos los registros de Fertirriego no enviados
        public static Task<List<Fertirriego>> GetAllFertirriegoAsync(this SQLiteAsyncConnection db)
        {
            return db.Table<Fertirriego>()
                     .Where(r => !r.Enviado) // Filtra registros no enviados
                     .ToListAsync();
        }

        // Método de extensión para actualizar un registro de Fertirriego
        public static Task<int> UpdateFertirriegoAsync(this SQLiteAsyncConnection db, Fertirriego registro)
        {
            return db.UpdateAsync(registro); // Actualiza el registro
        }

        // Método de extensión para eliminar un registro de Fertirriego
        public static Task<int> DeleteFertirriegoAsync(this SQLiteAsyncConnection db, Fertirriego registro)
        {
            return db.DeleteAsync(registro); // Elimina el registro
        }

        // Método de extensión para obtener registros filtrados por algún criterio 
        public static Task<List<Fertirriego>> GetFertirriegoAsync(this SQLiteAsyncConnection db)
        {
            return db.Table<Fertirriego>()
                     .Where(r => r.Enviado)
                     .ToListAsync(); // Filtra por enviados
        }

        // Método de extensión para obtener registros por fecha de riego
        public static async Task<List<Fertirriego>> GetFertirriegoByFechaAsync(this SQLiteAsyncConnection db, DateTime fecha)
        {
            string fechaFiltro = fecha.ToString("yyyy-MM-dd"); // Convertir fecha al formato de cadena

            var registros = await db.Table<Fertirriego>()
                                     .Where(r => r.FechaRiego == fechaFiltro) // Comparar como cadenas
                                     .ToListAsync();

            return registros;
        }
        // Métodos para Foto

        // Método de extensión para obtener fotos no enviadas
        public static Task<List<Foto>> GetFotosNoEnviadasAsync(this SQLiteAsyncConnection db)
        {
            return db.Table<Foto>()
                     .Where(f => !f.Enviado) // Filtra fotos no enviadas
                     .ToListAsync();
        }

        // Método de extensión para actualizar una foto
        public static Task<int> UpdatePhotoAsync(this SQLiteAsyncConnection db, Foto foto)
        {
            return db.UpdateAsync(foto); // Actualiza el registro de la foto
        }

        // Método de extensión para insertar una nueva foto
        public static Task<int> InsertPhotoAsync(this SQLiteAsyncConnection db, Foto foto)
        {
            return db.InsertAsync(foto); // Inserta una nueva foto
        }

        // Método de extensión para eliminar una foto
        public static Task<int> DeletePhotoAsync(this SQLiteAsyncConnection db, Foto foto)
        {
            return db.DeleteAsync(foto); // Elimina el registro de la foto
        }

    }
}
