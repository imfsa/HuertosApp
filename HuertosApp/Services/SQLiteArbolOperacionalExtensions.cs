using SQLite;
using HuertosApp.Models; // Modelo ArbolOperacional
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace HuertosApp.Services
{
    /// <summary>
    /// Métodos de extensión para trabajar con la tabla ArbolOperacional
    /// usando SQLiteAsyncConnection (igual estilo que Fertirriego y RegistroComercial).
    /// </summary>
    public static class SQLiteArbolOperacionalExtensions
    {
        /// <summary>
        /// Obtiene todos los árboles operacionales almacenados.
        /// </summary>
        public static Task<List<ArbolOperacional>> GetAllArbolesOperacionalesAsync(
            this SQLiteAsyncConnection db)
        {
            return db.Table<ArbolOperacional>().ToListAsync();
        }

        /// <summary>
        /// Busca un árbol por TreeId (usando long).
        /// Ideal cuando ya convertiste el QR a número.
        /// </summary>
        public static Task<ArbolOperacional?> GetArbolByTreeIdAsync(
            this SQLiteAsyncConnection db,
            long treeId)
        {
            return db.Table<ArbolOperacional>()
                     .Where(a => a.TreeId == treeId.ToString())
                     .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Busca un árbol por TreeId recibido como string (texto del QR).
        /// Si no se puede convertir a long, devuelve null.
        /// </summary>
        public static Task<ArbolOperacional?> GetArbolByTreeIdAsync(
            this SQLiteAsyncConnection db,
            string treeIdTexto)
        {
            if (string.IsNullOrWhiteSpace(treeIdTexto))
                return Task.FromResult<ArbolOperacional?>(null);

            if (!long.TryParse(treeIdTexto, out var treeId))
                return Task.FromResult<ArbolOperacional?>(null);

            return db.GetArbolByTreeIdAsync(treeId);
        }

        /// <summary>
        /// Obtiene árboles filtrados por predio.
        /// </summary>
        public static Task<List<ArbolOperacional>> GetArbolesPorPredioAsync(
            this SQLiteAsyncConnection db,
            int predio)
        {
            return db.Table<ArbolOperacional>()
                     .Where(a => a.Predio == predio.ToString())
                     .ToListAsync();
        }

        /// <summary>
        /// Inserta un árbol individual.
        /// </summary>
        public static Task<int> InsertArbolOperacionalAsync(
            this SQLiteAsyncConnection db,
            ArbolOperacional arbol)
        {
            return db.InsertAsync(arbol);
        }

        /// <summary>
        /// Inserta una lista de árboles (por ejemplo, luego de descargar desde la API).
        /// </summary>
        public static Task<int> InsertArbolesOperacionalesAsync(
            this SQLiteAsyncConnection db,
            IEnumerable<ArbolOperacional> lista)
        {
            return db.InsertAllAsync(lista);
        }

        /// <summary>
        /// Actualiza un árbol.
        /// </summary>
        public static Task<int> UpdateArbolOperacionalAsync(
            this SQLiteAsyncConnection db,
            ArbolOperacional arbol)
        {
            return db.UpdateAsync(arbol);
        }

        /// <summary>
        /// Elimina un árbol puntual.
        /// </summary>
        public static Task<int> DeleteArbolOperacionalAsync(
            this SQLiteAsyncConnection db,
            ArbolOperacional arbol)
        {
            return db.DeleteAsync(arbol);
        }

        /// <summary>
        /// Elimina todos los árboles operacionales (útil para refrescar desde la API).
        /// </summary>
        public static Task<int> DeleteAllArbolesOperacionalesAsync(
            this SQLiteAsyncConnection db)
        {
            return db.DeleteAllAsync<ArbolOperacional>();
        }
    }
}
