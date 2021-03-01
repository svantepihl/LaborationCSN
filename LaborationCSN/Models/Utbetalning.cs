using System;

namespace LaborationCSN.Models
{
    public class Utbetalning
    {
        public int Arendenummer;
        public String Beskrivning;
        public string Datum;
        public string Status;
        public decimal Belopp;

        public Utbetalning(int arendenummer, string beskrivning, string datum, string status, decimal belopp)
        {
            Arendenummer = arendenummer;
            Beskrivning = beskrivning;
            Datum = datum;
            Status = status;
            Belopp = belopp;
        }

        public Utbetalning()
        {
        }
    }
}