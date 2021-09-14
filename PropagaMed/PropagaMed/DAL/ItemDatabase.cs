using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PropagaMed;
using PropagaMed.Model;
using SQLite;

namespace PropagaMed.Dal
{
    public class ItemDatabase
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags, false);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        public ItemDatabase()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(Medico).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(Medico)).ConfigureAwait(false);
                    initialized = true;
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(Visita).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(Visita)).ConfigureAwait(false);
                    initialized = true;
                }
            }
        }

        public Task<List<Medico>> GetItemsMedicoAsync()
        {
            return Database.Table<Medico>().ToListAsync();
        }

        public Task<List<Visita>> GetItemsVisitaAsync()
        {
            return Database.Table<Visita>().ToListAsync();
        }

        public Task<List<Visita>> GetItemsVisitaDiaAsync()
        {
            return Database.Table<Visita>().Where(i => i.DiaVisita == DateTime.Now.Date).ToListAsync();
        }

        public Task<List<Medico>> GetItemsNotDoneAsync()
        {
            return Database.QueryAsync<Medico>("SELECT * FROM [Medico]");
        }

        public Task<Medico> GetItemAsync(int id)
        {
            return Database.Table<Medico>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveItemAsync(Object item)
        {
            return Database.InsertAsync(item);
        }

        public Task<int> UpdateItemAsync(Object item)
        {
            return Database.UpdateAsync(item);
        }

        public Task<int> DeleteItemAsync(object item)
        {
            return Database.DeleteAsync(item);
        }
    }
}