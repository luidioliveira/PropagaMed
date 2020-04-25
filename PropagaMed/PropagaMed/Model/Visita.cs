using System;
using SQLite;

namespace PropagaMed.Model
{
    public class Visita
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int id { get; set; }
        public int idMedicoVisita { get; set; }
        public string nomeMedicoVisita { get; set; }
        public DateTime diaVisita { get; set; }
        public TimeSpan horaVisita { get; set; }
        public string observacao { get; set; }
    }
}