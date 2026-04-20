using System;
using System.Linq;

namespace PropagaMed.Utils
{
    public static class DiasETurnosVisitaHelper
    {
        private static readonly string[] Turnos = { "Manhã", "Tarde", "Noite" };
        private static readonly string[] Dias = { "Segunda", "Terça", "Quarta", "Quinta", "Sexta" };

        public static string FormatarTurnos(string horariosVisita)
            => FormatarLista(horariosVisita, Turnos);

        public static string FormatarDias(string diasVisita)
            => FormatarLista(diasVisita, Dias);

        private static string FormatarLista(string fonte, string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(fonte))
                return string.Empty;

            var presentes = tokens
                .Where(t => fonte.Contains(t, StringComparison.OrdinalIgnoreCase))
                .Select(t => char.ToUpper(t[0]) + t.Substring(1).ToLower())
                .ToList();

            return presentes.Count switch
            {
                0 => string.Empty,
                1 => presentes[0],
                2 => $"{presentes[0]} e {presentes[1].ToLower()}",
                _ => string.Join(", ", presentes
                         .Take(presentes.Count - 1)
                         .Select((t, i) => i == 0 ? t : t.ToLower()))
                     + $" e {presentes.Last().ToLower()}"
            };
        }
    }
}