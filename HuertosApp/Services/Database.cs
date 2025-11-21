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

        // Inicialización de la base de datos
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

        // *************** CRUD Fertirriego ********************

        // Crear Fertirriego
        public static async Task<int> InsertFertirriegoAsync(Fertirriego registro)
        {
            return await _database!.InsertAsync(registro);
        }

        // Leer todos los registros de Fertirriego
        public static async Task<List<Fertirriego>> GetAllFertirriegoAsync()
        {
            return await _database!.Table<Fertirriego>().ToListAsync();
        }

        // Leer registros de Fertirriego por fecha
        public static async Task<List<Fertirriego>> GetFertirriegoByFechaAsync(string fecha)
        {
            return await _database!.Table<Fertirriego>()
                                   .Where(r => r.FechaRiego == fecha)
                                   .ToListAsync();
        }

        // Actualizar Fertirriego
        public static async Task<int> UpdateFertirriegoAsync(Fertirriego registro)
        {
            return await _database!.UpdateAsync(registro);
        }

        // Eliminar Fertirriego
        public static async Task<int> DeleteFertirriegoAsync(Fertirriego registro)
        {
            return await _database!.DeleteAsync(registro);
        }

        // *************** CRUD RegistroComercial ********************

        // Crear Registro Comercial
        public static async Task<int> InsertRegistroComercialAsync(RegistroComercial registro)
        {
            return await _database!.InsertAsync(registro);
        }

        // Leer todos los registros comerciales
        public static async Task<List<RegistroComercial>> GetAllRegistroComercialAsync()
        {
            return await _database!.Table<RegistroComercial>().ToListAsync();
        }

        // Leer registros comerciales por fecha
        public static async Task<List<RegistroComercial>> GetRegistroComercialByFechaAsync(string fecha)
        {
            return await _database!.Table<RegistroComercial>()
                                   .Where(r => r.Fecha == fecha)
                                   .ToListAsync();
        }

        // Actualizar Registro Comercial
        public static async Task<int> UpdateRegistroComercialAsync(RegistroComercial registro)
        {
            return await _database!.UpdateAsync(registro);
        }

        // Eliminar Registro Comercial
        public static async Task<int> DeleteRegistroComercialAsync(RegistroComercial registro)
        {
            return await _database!.DeleteAsync(registro);
        }

        // *************** Funciones Adicionales ********************

        // Limpiar registros antiguos de Fertirriego
        public static async Task ClearOldRegistrosAsync()
        {
            var threeDaysAgo = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            await _database!.ExecuteAsync("DELETE FROM Fertirriego WHERE FechaRiego < ?", threeDaysAgo);
        }

        // Obtener fotos no enviadas
        public static Task<List<Foto>> GetFotosNoEnviadasAsync()
        {
            return _database!.Table<Foto>().Where(f => !f.Enviado).ToListAsync();
        }

        // Actualizar estado de una foto
        public static Task<int> UpdatePhotoAsync(Foto foto)
        {
            return _database!.UpdateAsync(foto);
        }

        // Insertar nueva foto
        public static Task<int> InsertPhotoAsync(Foto foto)
        {
            return _database!.InsertAsync(foto);
        }

        // Actualizar base de datos desde un JSON (usuarios)
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

        // Inserta lista completa, reemplazando todo lo anterior
        public static async Task InsertArbolesOperacionalesAsync(List<ArbolOperacional> lista)
        {
            await _database!.DeleteAllAsync<ArbolOperacional>();
            await _database.InsertAllAsync(lista);
        }

        // Obtener todos los árboles operacionales
        public static async Task<List<ArbolOperacional>> GetArbolesAsync()
        {
            return await _database!.Table<ArbolOperacional>().ToListAsync();
        }

        // Buscar un árbol por TreeId (para escaneo QR)
        public static async Task<ArbolOperacional?> GetArbolByIdAsync(string treeId)
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            return await _database.Table<ArbolOperacional>()
                                  .Where(a => a.TreeId == treeId)
                                  .FirstOrDefaultAsync();
        }


        // Descargar árboles operacionales desde API y guardarlos en SQLite
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

                // JSON → modelo ArbolOperacional (todo como string)
                var lista = root.data.Select(x => new ArbolOperacional
                {
                    TreeId = x.tree_id,       // string
                    Temporada = x.temporada,     // string
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

        public static Task<int> InsertRegistroCosechaAsync(RegistroCosecha registro)
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            return _database.InsertAsync(registro);
        }

        public static Task<List<RegistroCosecha>> GetRegistrosCosechaByFechaAsync(string fecha)
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            return _database.Table<RegistroCosecha>()
                            .Where(r => r.FechaCosecha == fecha)
                            .ToListAsync();
        }

        public static Task<List<RegistroCosecha>> GetRegistrosCosechaPendientesDespachoAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Base de datos no inicializada.");

            return _database.Table<RegistroCosecha>()
                            .Where(r => !r.Despachado)
                            .ToListAsync();
        }






        // Clase auxiliar para deserialización de JSON de usuarios
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
