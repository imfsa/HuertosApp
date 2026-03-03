using System.Globalization;
using SQLite;
using HuertosApp.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Linq;

namespace HuertosApp.Services
{
    public static class Database
    {
        private static SQLiteAsyncConnection? _database;
        private static string _databasePath = string.Empty;

        // *************** INICIALIZACIÓN BASE DE DATOS ********************

        public static async Task InitializeDatabase()
        {
            try
            {
                if (_database != null)
                {
                    Console.WriteLine("Base de datos ya inicializada.");
                    return;
                }

#if ANDROID
                var context = Android.App.Application.Context;
                _databasePath = Path.Combine(
                    context.GetExternalFilesDir(null)?.AbsolutePath ?? string.Empty,
                    "HuertosApp.db3");
#else
                _databasePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "HuertosApp.db3");
#endif

                Console.WriteLine($"Ruta de la base de datos: {_databasePath}");

                _database = new SQLiteAsyncConnection(_databasePath);

                // Crear tablas necesarias
                await _database.CreateTableAsync<Usuario>();
                await _database.CreateTableAsync<Fertirriego>();
                await _database.CreateTableAsync<Foto>();
                await _database.CreateTableAsync<RegistroComercial>();
                await _database.CreateTableAsync<ArbolOperacional>();
                await _database.CreateTableAsync<RegistroCosecha>();

                // 🔹 Migración silenciosa de esquema (no borra datos)
                await EnsureSchemaAsync();

                Console.WriteLine("Base de datos inicializada correctamente.");

                // Agregar usuario de prueba
                await AddTestUserAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
                throw;
            }
        }

        // Añadir un usuario de prueba
        private static async Task AddTestUserAsync()
        {
            if (_database == null)
            {
                throw new InvalidOperationException("Base de datos no inicializada.");
            }

            var testUser = new Usuario
            {
                Nombre = "Admin",
                Password = "123",
                Estado = "Activo"
            };

            var existingUser = await _database.Table<Usuario>()
                                              .Where(u => u.Nombre == testUser.Nombre)
                                              .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                await _database.InsertAsync(testUser);
                Console.WriteLine("Usuario de prueba agregado.");
            }
            else
            {
                Console.WriteLine("El usuario de prueba ya existe.");
            }
        }

        // Verificar si la base de datos existe
        public static bool DatabaseExists()
        {
            return File.Exists(_databasePath);
        }

        // Obtener conexión de la base de datos
        public static SQLiteAsyncConnection GetDatabase()
        {
            if (_database == null)
            {
                throw new InvalidOperationException("Base de datos no inicializada. Llama a InitializeDatabase primero.");
            }

            return _database;
        }

        // *************** UTILIDADES DE ESQUEMA / MIGRACIÓN ***************

        // Clase auxiliar para leer PRAGMA table_info
        private class TableInfoRaw
        {
            public int cid { get; set; }
            public string name { get; set; } = string.Empty;
            public string type { get; set; } = string.Empty;
            public int notnull { get; set; }
            public string? dflt_value { get; set; }
            public int pk { get; set; }
        }

        public class TableColumnInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public bool NotNull { get; set; }
            public string DefaultValue { get; set; } = string.Empty;
            public bool IsPrimaryKey { get; set; }
        }

        /// <summary>
        /// Devuelve la definición de columnas de una tabla usando PRAGMA table_info.
        /// </summary>
        private static async Task<List<TableColumnInfo>> GetTableInfoAsync(string tableName)
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            var result = await _database.QueryAsync<TableInfoRaw>($"PRAGMA table_info('{tableName}');");

            return result.Select(r => new TableColumnInfo
            {
                Name = r.name,
                Type = r.type,
                NotNull = r.notnull == 1,
                DefaultValue = r.dflt_value ?? string.Empty,
                IsPrimaryKey = r.pk == 1
            }).ToList();
        }

        /// <summary>
        /// Indica si una tabla tiene una columna específica.
        /// </summary>
        private static async Task<bool> HasColumnAsync(string tableName, string columnName)
        {
            var info = await GetTableInfoAsync(tableName);
            return info.Any(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Asegura que exista una columna en una tabla; si no existe, la agrega con ALTER TABLE.
        /// No borra datos.
        /// </summary>
        private static async Task EnsureColumnAsync(
            string tableName,
            string columnName,
            string sqlType,
            string defaultValueExpression = "NULL")
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            if (await HasColumnAsync(tableName, columnName))
                return;

            string sql = $"ALTER TABLE {tableName} " +
                         $"ADD COLUMN {columnName} {sqlType} " +
                         $"DEFAULT {defaultValueExpression};";

            await _database.ExecuteAsync(sql);
        }

        /// <summary>
        /// Migración silenciosa: corrige esquema sin borrar nada.
        /// </summary>
        private static async Task EnsureSchemaAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            // 🔹 Asegurar columnas nuevas de RegistroCosecha
            await EnsureColumnAsync("RegistroCosecha", "UsuarioId", "INTEGER", "NULL");
            await EnsureColumnAsync("RegistroCosecha", "Sincronizado", "INTEGER", "0");
        }

        // *************** CRUD Fertirriego ********************

        public static async Task<int> InsertFertirriegoAsync(Fertirriego registro)
        {
            return await _database!.InsertAsync(registro);
        }

        public static async Task<List<Fertirriego>> GetAllFertirriegoAsync()
        {
            return await _database!.Table<Fertirriego>().ToListAsync();
        }

        public static async Task<List<Fertirriego>> GetFertirriegoByFechaAsync(string fecha)
        {
            return await _database!.Table<Fertirriego>()
                                   .Where(r => r.FechaRiego == fecha)
                                   .ToListAsync();
        }

        public static async Task<int> UpdateFertirriegoAsync(Fertirriego registro)
        {
            return await _database!.UpdateAsync(registro);
        }

        public static async Task<int> DeleteFertirriegoAsync(Fertirriego registro)
        {
            return await _database!.DeleteAsync(registro);
        }

        // *************** CRUD RegistroComercial ********************

        public static async Task<int> InsertRegistroComercialAsync(RegistroComercial registro)
        {
            return await _database!.InsertAsync(registro);
        }

        public static async Task<List<RegistroComercial>> GetAllRegistroComercialAsync()
        {
            return await _database!.Table<RegistroComercial>().ToListAsync();
        }

        public static async Task<List<RegistroComercial>> GetRegistroComercialByFechaAsync(string fecha)
        {
            return await _database!.Table<RegistroComercial>()
                                   .Where(r => r.Fecha == fecha)
                                   .ToListAsync();
        }

        public static async Task<int> UpdateRegistroComercialAsync(RegistroComercial registro)
        {
            return await _database!.UpdateAsync(registro);
        }

        public static async Task<int> DeleteRegistroComercialAsync(RegistroComercial registro)
        {
            return await _database!.DeleteAsync(registro);
        }

        // *************** Funciones Adicionales ********************

        public static async Task ClearOldRegistrosAsync()
        {
            var threeDaysAgo = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            await _database!.ExecuteAsync("DELETE FROM Fertirriego WHERE FechaRiego < ?", threeDaysAgo);
        }

        public static Task<List<Foto>> GetFotosNoEnviadasAsync()
        {
            return _database!.Table<Foto>().Where(f => !f.Enviado).ToListAsync();
        }

        public static Task<int> UpdatePhotoAsync(Foto foto)
        {
            return _database!.UpdateAsync(foto);
        }

        public static Task<int> InsertPhotoAsync(Foto foto)
        {
            return _database!.InsertAsync(foto);
        }

        public static async Task UpdateDatabaseFromJsonAsync(string urlUsuarios)
        {
            using var httpClient = new HttpClient();
            var responseUsuarios = await httpClient.GetAsync(urlUsuarios);

            if (responseUsuarios.IsSuccessStatusCode)
            {
                var jsonDataUsuarios = await responseUsuarios.Content.ReadAsStringAsync();
                var dataUsuarios = JsonSerializer.Deserialize<RootObject>(jsonDataUsuarios);

                if (dataUsuarios != null && dataUsuarios.usuarios != null)
                {
                    await _database!.DeleteAllAsync<Usuario>();

                    foreach (var usuario in dataUsuarios.usuarios)
                    {
                        await _database.InsertAsync(usuario);
                    }
                }
            }
        }

        // *************** ARBOLES OPERACIONALES ************************

        public static async Task InsertArbolesOperacionalesAsync(List<ArbolOperacional> lista)
        {
            await _database!.DeleteAllAsync<ArbolOperacional>();
            await _database.InsertAllAsync(lista);
        }

        public static async Task<List<ArbolOperacional>> GetArbolesAsync()
        {
            return await _database!.Table<ArbolOperacional>().ToListAsync();
        }

        public static async Task<ArbolOperacional?> GetArbolByIdAsync(string treeId)
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            return await _database.Table<ArbolOperacional>()
                                  .Where(a => a.TreeId == treeId)
                                  .FirstOrDefaultAsync();
        }

        public static async Task<bool> DescargarArbolesOperacionalesAsync(string url)
        {
            try
            {
                if (_database == null)
                    throw new InvalidOperationException("Base de datos no inicializada.");

                using var http = new HttpClient();
                var json = await http.GetStringAsync(url);

                var root = JsonSerializer.Deserialize<RootArboles>(json);

                if (root == null || root.data == null)
                    return false;

                var lista = root.data.Select(x => new ArbolOperacional
                {
                    TreeId = x.tree_id,
                    Temporada = x.temporada,
                    Genotipo = x.genotipo,
                    Especie = x.especie,
                    Replica = x.replica,
                    Fila = x.fila,
                    Columna = x.columna,
                    Predio = x.predio,
                    CodHuerto = x.cod_huerto,
                    HuertoNombre = x.huerto_nombre
                }).ToList();

                await _database.DeleteAllAsync<ArbolOperacional>();
                await _database.InsertAllAsync(lista);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al descargar árboles operacionales: " + ex.Message);
                return false;
            }
        }

        // *************** REGISTRO DE COSECHA (LOCAL) ********************

        // Insertar un registro de cosecha
        public static Task<int> InsertRegistroCosechaAsync(RegistroCosecha registro)
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            return _database.InsertAsync(registro);
        }

        // Actualizar un registro de cosecha
        public static Task<int> UpdateRegistroCosechaAsync(RegistroCosecha registro)
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            return _database.UpdateAsync(registro);
        }

        // 🔹 Registros de cosecha por fecha (blindado: nunca bota la app)
        //public static async Task<List<RegistroCosecha>> GetRegistrosCosechaByFechaAsync(string fecha)
        //{
        //    if (_database == null)
        //        throw new InvalidOperationException("Base de datos no inicializada.");

        //    try
        //    {
        //        return await _database.Table<RegistroCosecha>()
        //                              .Where(r => r.FechaCosecha == fecha)
        //                              .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(
        //            $"Error al obtener registros de cosecha para fecha {fecha}: {ex.Message}");

        //        // Si algo sale mal (tabla rota, columna faltante, etc.), devolvemos lista vacía
        //        // para que la pantalla no se caiga.
        //        return new List<RegistroCosecha>();
        //    }
        //}

        // 🔹 Registros de cosecha por fecha (blindado: nunca bota la app)
        // 🔹 Registros de cosecha por fecha (blindado: nunca bota la app)
        //public static async Task<List<RegistroCosecha>> GetRegistrosCosechaByFechaAsync(string fechaIso)
        //{
        //    if (_database == null)
        //        throw new InvalidOperationException("Base de datos no inicializada.");

        //    try
        //    {
        //        // fechaIso viene como "yyyy-MM-dd" desde la página
        //        var targetDate = DateTime.ParseExact(fechaIso, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        //        // 1) Traemos TODOS los registros
        //        var todos = await _database.Table<RegistroCosecha>().ToListAsync();

        //        if (todos == null || todos.Count == 0)
        //            return new List<RegistroCosecha>();

        //        // 2) Intentamos interpretar FechaCosecha en varios formatos
        //        var formatos = new[]
        //        {
        //    "yyyy-MM-dd",
        //    "yyyy-MM-dd HH:mm",
        //    "yyyy-MM-dd HH:mm:ss",
        //    "dd/MM/yyyy",
        //    "dd/MM/yyyy HH:mm",
        //    "dd/MM/yyyy HH:mm:ss"
        //};

        //        var result = new List<RegistroCosecha>();

        //        foreach (var r in todos)
        //        {
        //            if (string.IsNullOrWhiteSpace(r.FechaCosecha))
        //                continue;

        //            if (!DateTime.TryParseExact(
        //                    r.FechaCosecha.Trim(),
        //                    formatos,
        //                    CultureInfo.InvariantCulture,
        //                    DateTimeStyles.None,
        //                    out var fechaReg))
        //            {
        //                // Si no se puede parsear, lo saltamos silenciosamente
        //                continue;
        //            }

        //            if (fechaReg.Date == targetDate.Date)
        //                result.Add(r);
        //        }

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(
        //            $"Error al obtener registros de cosecha para fecha {fechaIso}: {ex.Message}");

        //        // Si algo sale mal, devolvemos lista vacía para que la pantalla no se caiga.
        //        return new List<RegistroCosecha>();
        //    }
        //}
        public static async Task<List<RegistroCosecha>> GetRegistrosCosechaByFechaAsync(string fechaIso)
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            try
            {
                // fechaIso viene como "yyyy-MM-dd"
                return await _database.Table<RegistroCosecha>()
                                      .Where(r => r.FechaCosecha == fechaIso)
                                      .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Error al obtener registros de cosecha para fecha {fechaIso}: {ex.Message}");

                return new List<RegistroCosecha>();
            }
        }



        // Registros pendientes de despacho
        public static Task<List<RegistroCosecha>> GetRegistrosCosechaPendientesDespachoAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            return _database.Table<RegistroCosecha>()
                            .Where(r => !r.Despachado)
                            .ToListAsync();
        }

        // Registros pendientes de sincronizar
        public static Task<List<RegistroCosecha>> GetRegistrosCosechaPendientesSincronizarAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            return _database.Table<RegistroCosecha>()
                            .Where(r => !r.Sincronizado)
                            .ToListAsync();
        }

        // *************** JSON AUX ***************

        public class RootObject
        {
            public List<Usuario> usuarios { get; set; } = new List<Usuario>();
        }
    }

    // Clases auxiliares para deserializar el JSON de árboles operacionales
    public class RootArboles
    {
        public List<ArbolOperacionalJson> data { get; set; } = new List<ArbolOperacionalJson>();
    }

    public class ArbolOperacionalJson
    {
        public string? tree_id { get; set; }
        public string? temporada { get; set; }
        public string? genotipo { get; set; }
        public string? especie { get; set; }
        public string? replica { get; set; }
        public string? fila { get; set; }
        public string? columna { get; set; }
        public string? predio { get; set; }
        public string? cod_huerto { get; set; }
        public string? huerto_nombre { get; set; }
    }
}
