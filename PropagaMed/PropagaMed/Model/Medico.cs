using System;
using SQLite;

namespace PropagaMed.Model
{
    public class Medico
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id {get; set;}
        public string Nome {get; set;}
        public string Especialidade { get; set; }
        public string Localizacao { get; set; }
        public string Endereco { get; set; }
        public string CEP { get; set; }
        public DateTime Aniversario { get; set; }
        public string Secretaria { get; set; }
        public string Telefone { get; set; }
        public string Celular { get; set; }
        public string Email { get; set; }
        public string TiposVisita { get; set; }
        public string DiasVisita { get; set; }
        public string HorariosVisita { get; set; }
        public string CRM { get; set; }
    }
}