using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InvetarioCrud.Models;
using System.Linq;

namespace InvetarioCrud.Services
{
    public static class DbConstants
    {
        public const string DatabaseFilename = "InventoryApp.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }

    public class DatabaseService
    {
        SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            
        }

        async Task Init()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(DbConstants.DatabasePath, DbConstants.Flags);
            await _database.CreateTableAsync<Producto>();
            await _database.CreateTableAsync<Almacen>();
            await _database.CreateTableAsync<AlmacenProducto>();
            await _database.CreateTableAsync<Movimiento>();
        }

        #region CRUD Productos
        public async Task<List<Producto>> GetProductosAsync()
        {
            await Init();
            return await _database.Table<Producto>().ToListAsync();
        }

        public async Task<Producto> GetProductoAsync(int id)
        {
            await Init();
            return await _database.Table<Producto>().Where(p => p.ID == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveProductoAsync(Producto producto)
        {
            await Init();
            if (producto.ID != 0)
            {
                return await _database.UpdateAsync(producto);
            }
            else
            {
                return await _database.InsertAsync(producto);
            }
        }

        public async Task<int> DeleteProductoAsync(Producto producto)
        {
            await Init();
            return await _database.DeleteAsync(producto);
        }
        #endregion
        #region CRUD Almacenes
        public async Task<List<Almacen>> GetAlmacenesAsync()
        {
            await Init();
            return await _database.Table<Almacen>().ToListAsync();
        }

        public async Task<Almacen> GetAlmacenAsync(int id)
        {
            await Init();
            return await _database.Table<Almacen>().Where(a => a.ID == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveAlmacenAsync(Almacen almacen)
        {
            await Init();
            if (almacen.ID != 0)
            {
                return await _database.UpdateAsync(almacen);
            }
            else
            {
                return await _database.InsertAsync(almacen);
            }
        }

        public async Task<bool> AlmacenTieneProductosAsync(int almacenId)
        {
            await Init();
            var count = await _database.Table<AlmacenProducto>().Where(ap => ap.IDAlmacen == almacenId).CountAsync();
            return count > 0;
        }

        public async Task<int> DeleteAlmacenAsync(Almacen almacen)
        {
            await Init();

            
            bool tieneProductos = await AlmacenTieneProductosAsync(almacen.ID);
            if (tieneProductos)
            {
                throw new InvalidOperationException("No se puede eliminar el almacén porque contiene productos. Transfiera o elimine los productos primero.");
            }
            return await _database.DeleteAsync(almacen);
        }

        #endregion
        #region CRUD AlmacenProducto
        public async Task<List<AlmacenProductoInfo>> GetProductosEnAlmacenAsync(int almacenId)
        {
            await Init();
            var query = @"
                SELECT ap.ID, ap.IDProducto, ap.IDAlmacen, ap.Cantidad, ap.FechaVencimiento, p.NombreProducto
                FROM AlmacenProducto ap
                INNER JOIN Producto p ON ap.IDProducto = p.ID
                WHERE ap.IDAlmacen = ?";
            var result = await _database.QueryAsync<AlmacenProductoInfo>(query, almacenId);
            return result;
        }

        public async Task<AlmacenProducto> GetProductoEspecificoEnAlmacenAsync(int productoId, int almacenId)
        {
            await Init();
            return await _database.Table<AlmacenProducto>()
                                  .Where(ap => ap.IDProducto == productoId && ap.IDAlmacen == almacenId)
                                  .FirstOrDefaultAsync();
        }

        public async Task<AlmacenProducto> GetAlmacenProductoByIdAsync(int almacenProductoId)
        {
            await Init();
            return await _database.Table<AlmacenProducto>().Where(ap => ap.ID == almacenProductoId).FirstOrDefaultAsync();
        }

        public async Task<int> AddOrUpdateProductoEnAlmacenAsync(AlmacenProducto item)
        {
            await Init();
            if (item.ID != 0)
            {
                return await _database.UpdateAsync(item);
            }
            else 
            {
                
                var existente = await GetProductoEspecificoEnAlmacenAsync(item.IDProducto, item.IDAlmacen);
                if (existente != null)
                {
                   
                    existente.Cantidad = item.Cantidad; 
                    existente.FechaVencimiento = item.FechaVencimiento; 
                    return await _database.UpdateAsync(existente);
                }
                else
                {
                    return await _database.InsertAsync(item);
                }
            }
        }

        public async Task<int> DeleteProductoDeAlmacenAsync(int almacenProductoId)
        {
            await Init();
            return await _database.DeleteAsync<AlmacenProducto>(almacenProductoId);
        }
        #endregion
        #region CRUD Movimientos
        public async Task IngresarProductoAsync(int productoId, int almacenDestinoId, int cantidad, DateTime fechaVencimiento)
        {
            await Init();
            // Usar transacción para asegurar atomicidad
            await _database.RunInTransactionAsync(async (SQLiteConnection tran) => // tran es síncrono, usamos métodos síncronos dentro
            {
                // 1. Actualizar inventario en AlmacenProducto
                // Usamos la conexión de la transacción (tran)
                var itemInventario = tran.Table<AlmacenProducto>()
                                         .Where(ap => ap.IDProducto == productoId && ap.IDAlmacen == almacenDestinoId)
                                         .FirstOrDefault(); // Síncrono

                if (itemInventario == null)
                {
                    itemInventario = new AlmacenProducto
                    {
                        IDProducto = productoId,
                        IDAlmacen = almacenDestinoId,
                        Cantidad = cantidad,
                        FechaVencimiento = fechaVencimiento
                    };
                    tran.Insert(itemInventario); // Síncrono
                }
                else
                {
                    itemInventario.Cantidad += cantidad;
                    // Lógica de Fecha de Vencimiento para ingresos:
                    // Si se ingresa un lote nuevo del mismo producto, puede que se quiera mantener la fecha más próxima
                    // o manejar lotes separados. Simplificación: el nuevo ingreso establece/actualiza la fecha.
                    // Si se quiere mantener la fecha más próxima, se necesitaría:
                    // if (fechaVencimiento < itemInventario.FechaVencimiento) itemInventario.FechaVencimiento = fechaVencimiento;
                    itemInventario.FechaVencimiento = fechaVencimiento; // Simplificado: la nueva fecha reemplaza.
                    tran.Update(itemInventario); // Síncrono
                }

                // 2. Registrar el movimiento
                var movimiento = new Movimiento
                {
                    IDProducto = productoId,
                    IDAlmacenDestino = almacenDestinoId,
                    IDAlmacenOrigen = null, // Es un ingreso
                    Cantidad = cantidad,
                    Tipo = TipoMovimiento.Ingreso,
                    FechaMovimiento = DateTime.Now
                };
                tran.Insert(movimiento); // Síncrono
            });
        }

        public async Task<bool> MoverProductoAsync(int productoId, int almacenOrigenId, int almacenDestinoId, int cantidad)
        {
            await Init();
            try
            {
                await _database.RunInTransactionAsync(async (SQLiteConnection tran) => // tran es síncrono
                {
                    // 1. Verificar stock en origen
                    var itemOrigen = tran.Table<AlmacenProducto>()
                                          .Where(ap => ap.IDProducto == productoId && ap.IDAlmacen == almacenOrigenId)
                                          .FirstOrDefault();

                    if (itemOrigen == null || itemOrigen.Cantidad < cantidad)
                    {
                        throw new Exception("Stock insuficiente en almacén de origen o producto no encontrado.");
                    }

                    // 2. Restar de almacén origen
                    itemOrigen.Cantidad -= cantidad;
                    if (itemOrigen.Cantidad == 0)
                    {
                        tran.Delete(itemOrigen);
                    }
                    else
                    {
                        tran.Update(itemOrigen);
                    }

                    // 3. Sumar a almacén destino
                    var itemDestino = tran.Table<AlmacenProducto>()
                                           .Where(ap => ap.IDProducto == productoId && ap.IDAlmacen == almacenDestinoId)
                                           .FirstOrDefault();
                    if (itemDestino == null)
                    {
                        itemDestino = new AlmacenProducto
                        {
                            IDProducto = productoId,
                            IDAlmacen = almacenDestinoId,
                            Cantidad = cantidad,
                            FechaVencimiento = itemOrigen.FechaVencimiento // Asumimos que se mueve el mismo lote/fecha
                        };
                        tran.Insert(itemDestino);
                    }
                    else
                    {
                        itemDestino.Cantidad += cantidad;
                        // Lógica de fecha de vencimiento para traslados:
                        // Si se mueven lotes diferentes, esto se complica.
                        // Simplificación: se asume que la fecha de vencimiento del destino se actualiza si es un lote nuevo,
                        // o se mantiene la del lote existente. Aquí, usamos la del origen.
                        // itemDestino.FechaVencimiento = itemOrigen.FechaVencimiento; // O promediar, o mantener la más próxima, etc.
                        tran.Update(itemDestino);
                    }

                    // 4. Registrar el movimiento
                    var movimiento = new Movimiento
                    {
                        IDProducto = productoId,
                        IDAlmacenOrigen = almacenOrigenId,
                        IDAlmacenDestino = almacenDestinoId,
                        Cantidad = cantidad,
                        Tipo = TipoMovimiento.Traslado,
                        FechaMovimiento = DateTime.Now
                    };
                    tran.Insert(movimiento);
                });
                return true; // Si no hubo excepciones, la transacción fue exitosa
            }
            catch (Exception) // Captura la excepción de la transacción (ej. stock insuficiente)
            {
                // La excepción será relanzada y manejada por el ViewModel
                throw;
            }
        }

        public async Task<List<Movimiento>> GetMovimientosAsync()
        {
            await Init();
            var movimientos = await _database.Table<Movimiento>().OrderByDescending(m => m.FechaMovimiento).ToListAsync();

            // Enriquecer con nombres. Podría ser más eficiente con un JOIN complejo en SQL crudo si hay muchos movimientos.
            var productosIds = movimientos.Select(m => m.IDProducto).Distinct().ToList();
            var almacenesIds = movimientos.Where(m => m.IDAlmacenOrigen.HasValue).Select(m => m.IDAlmacenOrigen.Value).Distinct()
                                .Union(movimientos.Select(m => m.IDAlmacenDestino).Distinct()).ToList();

            var productos = await _database.Table<Producto>().Where(p => productosIds.Contains(p.ID)).ToListAsync();
            var almacenes = await _database.Table<Almacen>().Where(a => almacenesIds.Contains(a.ID)).ToListAsync();

            foreach (var mov in movimientos)
            {
                mov.NombreProducto = productos.FirstOrDefault(p => p.ID == mov.IDProducto)?.NombreProducto;
                if (mov.IDAlmacenOrigen.HasValue)
                {
                    mov.NombreAlmacenOrigen = almacenes.FirstOrDefault(a => a.ID == mov.IDAlmacenOrigen.Value)?.NombreAlmacen;
                }
                mov.NombreAlmacenDestino = almacenes.FirstOrDefault(a => a.ID == mov.IDAlmacenDestino)?.NombreAlmacen;
            }
            return movimientos;
        }
        #endregion
        #region Reportes
        public async Task<List<AlmacenProductoInfo>> GetStockTotalConsolidadoAsync()
        {
            await Init();
            var query = @"
                SELECT 
                    ap.IDProducto,
                    p.NombreProducto,
                    SUM(ap.Cantidad) AS Cantidad,
                    MIN(ap.FechaVencimiento) AS FechaVencimiento /* Muestra la fecha de vencimiento más próxima globalmente */
                FROM AlmacenProducto ap
                INNER JOIN Producto p ON ap.IDProducto = p.ID
                GROUP BY ap.IDProducto, p.NombreProducto
                ORDER BY p.NombreProducto";
            return await _database.QueryAsync<AlmacenProductoInfo>(query);
            // Nota: AlmacenProductoInfo no tiene NombreAlmacen, lo cual es correcto para este reporte.
            // ID y IDAlmacen en AlmacenProductoInfo no tendrán sentido aquí, se llenarán con el primer IDProducto y NULL respectivamente.
        }

        public async Task<List<AlmacenProductoInfo>> GetStockPorAlmacenAsync(int almacenId)
        {
            await Init();
            var query = @"
                SELECT 
                    ap.ID, /* ID de AlmacenProducto */
                    ap.IDProducto, 
                    p.NombreProducto,
                    ap.IDAlmacen,
                    alm.NombreAlmacen, /* Añadido para el reporte */
                    ap.Cantidad, 
                    ap.FechaVencimiento
                FROM AlmacenProducto ap
                INNER JOIN Producto p ON ap.IDProducto = p.ID
                INNER JOIN Almacen alm ON ap.IDAlmacen = alm.ID
                WHERE ap.IDAlmacen = ?
                ORDER BY p.NombreProducto";
            return await _database.QueryAsync<AlmacenProductoInfo>(query, almacenId);
        }


        public async Task<List<Movimiento>> GetHistorialMovimientosProductoAsync(int productoId)
        {
            await Init();
            var movimientos = await _database.Table<Movimiento>()
                                       .Where(m => m.IDProducto == productoId)
                                       .OrderByDescending(m => m.FechaMovimiento)
                                       .ToListAsync();
            // Enriquecer con nombres (similar a GetMovimientosAsync)
            var almacenesIds = movimientos.Where(m => m.IDAlmacenOrigen.HasValue).Select(m => m.IDAlmacenOrigen.Value).Distinct()
                                .Union(movimientos.Select(m => m.IDAlmacenDestino).Distinct()).ToList();

            var producto = await GetProductoAsync(productoId); // Solo necesitamos un producto
            var almacenes = await _database.Table<Almacen>().Where(a => almacenesIds.Contains(a.ID)).ToListAsync();

            foreach (var mov in movimientos)
            {
                mov.NombreProducto = producto?.NombreProducto;
                if (mov.IDAlmacenOrigen.HasValue)
                {
                    mov.NombreAlmacenOrigen = almacenes.FirstOrDefault(a => a.ID == mov.IDAlmacenOrigen.Value)?.NombreAlmacen;
                }
                mov.NombreAlmacenDestino = almacenes.FirstOrDefault(a => a.ID == mov.IDAlmacenDestino)?.NombreAlmacen;
            }
            return movimientos;
        }
        #endregion
    }
}