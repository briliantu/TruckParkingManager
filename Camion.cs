using System;

namespace ParcareCamioane
{
    public class Camion
    {
        public string NumarInmatriculare { get; set; }
        public DateTime DataIntrare { get; set; }
        public DateTime? DataIesire { get; set; }
        public string DurataTotala { get; set; }
        public int NumarLoc { get; set; }

        public Camion(string numar, DateTime intrare, int numarLoc)
        {
            NumarInmatriculare = numar;
            DataIntrare = intrare;
            DataIesire = null;
            DurataTotala = "-";
            NumarLoc = numarLoc;
        }

        // Calculează automat durata exactă de staționare
        public void FinalizeazaStationare(DateTime iesire)
        {
            DataIesire = iesire;
            TimeSpan ts = iesire - DataIntrare;

            int totalZile = ts.Days;
            int ore = ts.Hours;
            int minute = ts.Minutes;

            int ani = totalZile / 365;
            int restZile = totalZile % 365;
            int luni = restZile / 30;
            restZile = restZile % 30;
            int saptamani = restZile / 7;
            int zile = restZile % 7;

            string rezultat = "";
            if (ani > 0) rezultat += $"{ani} ani, ";
            if (luni > 0) rezultat += $"{luni} luni, ";
            if (saptamani > 0) rezultat += $"{saptamani} săpt, ";
            if (zile > 0) rezultat += $"{zile} zile, ";
            rezultat += $"{ore} ore, {minute} min";

            DurataTotala = rezultat;
        }
    }
}