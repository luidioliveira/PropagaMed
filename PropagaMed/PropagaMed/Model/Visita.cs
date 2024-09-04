using System;
using SQLite;

namespace PropagaMed.Model
{
    public class Visita
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        public int IdMedicoVisita { get; set; }
        public string NomeMedicoVisita { get; set; }
        public DateTime DiaVisita { get; set; }
        public TimeSpan HoraVisita { get; set; }
        public string TipoVisita { get; set; }
        public string Observacao { get; set; }
        public bool IsBirthday { get; set; }
    }
}