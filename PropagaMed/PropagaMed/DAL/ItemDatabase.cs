using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PropagaMed.Model;
using SQLite;

namespace PropagaMed.Dal
{
    public class ItemDatabase
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new(() =>
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
                try
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(Medico)).ConfigureAwait(false);
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(Visita)).ConfigureAwait(false);
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(UserData)).ConfigureAwait(false);

                    initialized = true;
                }
                catch { }
            }
        }

        public Task<List<Medico>> GetItemsMedicoAsync()
        {
            return Database.Table<Medico>().ToListAsync();
        }

        public Task<List<Visita>> GetItemsVisitaAsync(int? medicoId = null)
        {
            if (medicoId is null)
                return Database.Table<Visita>().ToListAsync();
            else
                return Database.Table<Visita>().Where(m => m.IdMedicoVisita.Equals(medicoId)).ToListAsync();
        }

        public Task<List<Visita>> GetItemsVisitaByParameterAsync(int typeParameter)
        {
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
            var lastDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).Date;
            var firstDayOfLastMonth = firstDayOfMonth.AddMonths(-1).Date;
            var LastDayOfLastMonth = new DateTime(firstDayOfLastMonth.Year, firstDayOfLastMonth.Month, DateTime.DaysInMonth(firstDayOfLastMonth.Year, firstDayOfLastMonth.Month)).Date;
            var SixMonthsSameDay = DateTime.Now.AddMonths(-6).Date;

            return typeParameter switch
            {
                (int)ExportEnum.byDay => Database.Table<Visita>().Where(i => i.DiaVisita == DateTime.Now.Date).ToListAsync(),
                (int)ExportEnum.byMonth => Database.Table<Visita>().Where(i => i.DiaVisita >= firstDayOfMonth && i.DiaVisita <= lastDayOfMonth).ToListAsync(),
                (int)ExportEnum.lastMonth => Database.Table<Visita>().Where(i => i.DiaVisita >= firstDayOfLastMonth && i.DiaVisita <= LastDayOfLastMonth).ToListAsync(),
                (int)ExportEnum.lastSixMonths => Database.Table<Visita>().Where(i => i.DiaVisita >= SixMonthsSameDay && i.DiaVisita <= DateTime.Now.Date).ToListAsync(),
                _ => Database.Table<Visita>().Where(i => i.DiaVisita == DateTime.Now.Date).ToListAsync(),
            };
        }

        public Task<List<Visita>> GetItemsVisitaByCustomParameterAsync(DateTime begin, DateTime end)
        {
            return Database.Table<Visita>().Where(v => v.DiaVisita >= begin && v.DiaVisita <= end).ToListAsync();
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

        public async Task DeleteAllVisitasAsync()
        {
            await Database.QueryAsync<Visita>("DELETE FROM [Visita]");
        }

        public Task<List<UserData>> GetItemsUserDataAsync()
        {
            return Database.Table<UserData>().ToListAsync();
        }

        public Task CloseConnections()
        {
            return Database.CloseAsync();
        }
    }
}