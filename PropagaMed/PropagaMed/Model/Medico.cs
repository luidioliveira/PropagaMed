using System;
using SQLite;

namespace PropagaMed.Model
{
    public class Medico
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int id {get; set;}
        public string nome {get; set;}
        public string especialidade { get; set; }
        public string localizacao { get; set; }
        public string endereco { get; set; }
        public string CEP { get; set; }
        public DateTime aniversario { get; set; }
        public string telefone { get; set; }
        public string celular { get; set; }
        public string email { get; set; }
        public string diasVisita { get; set; }
        public string horariosVisita { get; set; }
        public string CRM { get; set; }
    }
}