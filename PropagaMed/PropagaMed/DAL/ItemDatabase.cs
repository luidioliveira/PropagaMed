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

        public Task<List<Visita>> GetItemsVisitaByParameterAsync(int typeParameter)
        {
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
            var lastDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).Date;
            var firstDayOfLastMonth = firstDayOfMonth.AddMonths(-1).Date;
            var LastDayOfLastMonth = new DateTime(firstDayOfLastMonth.Year, firstDayOfLastMonth.Month, DateTime.DaysInMonth(firstDayOfLastMonth.Year, firstDayOfLastMonth.Month)).Date;
            var SixMonthsSameDay = DateTime.Now.AddMonths(-6).Date;

            switch (typeParameter)
            { 
                case (int)ExportEnum.byDay:
                    return Database.Table<Visita>().Where(i => i.DiaVisita == DateTime.Now.Date).ToListAsync();
                case (int)ExportEnum.byMonth:
                    return Database.Table<Visita>().Where(i => i.DiaVisita >= firstDayOfMonth && i.DiaVisita <= lastDayOfMonth).ToListAsync();
                case (int)ExportEnum.lastMonth:
                    return Database.Table<Visita>().Where(i => i.DiaVisita >= firstDayOfLastMonth && i.DiaVisita <= LastDayOfLastMonth).ToListAsync();
                case (int)ExportEnum.lastSixMonths:
                    return Database.Table<Visita>().Where(i => i.DiaVisita >= SixMonthsSameDay && i.DiaVisita <= DateTime.Now.Date).ToListAsync();
                default:
                    return Database.Table<Visita>().Where(i => i.DiaVisita == DateTime.Now.Date).ToListAsync();
            }
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