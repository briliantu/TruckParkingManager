using System;

namespace TruckParkingManager
{
    public class Camion
    {
        public string NumarInmatriculare { get; set; }
        public string Tara { get; set; }
        public DateTime DataIntrare { get; set; }
        public DateTime? DataIesire { get; set; }
        public string DurataTotala { get; set; }
        public int NumarLoc { get; set; }

        public Camion(string numar, DateTime intrare, int numarLoc, string tara = "România")
        {
            NumarInmatriculare = numar;
            Tara = tara;
            DataIntrare = intrare;
            DataIesire = null;
            DurataTotala = "-";
            NumarLoc = numarLoc;
        }

        // Calculează automat durata exactă de staționare
        public void FinalizeazaStationare(DateTime iesire)
        {
            DataIesire = iesire;
            DurataTotala = CalculeazaDurata(DataIntrare, iesire);
        }

        // Calcul pe bază de calendar real (ani/luni de lungime variabilă),
        // în loc de aproximarea anterioară (zile / 30, zile / 365) care putea
        // introduce erori de câteva zile în rapoartele de audit.
        private static string CalculeazaDurata(DateTime start, DateTime end)
        {
            if (end < start)
            {
                return "0 ore, 0 min";
            }

            int ani = 0;
            int luni = 0;
            DateTime cursor = start;

            while (cursor.AddYears(1) <= end)
            {
                cursor = cursor.AddYears(1);
                ani++;
            }

            while (cursor.AddMonths(1) <= end)
            {
                cursor = cursor.AddMonths(1);
                luni++;
            }

            TimeSpan rest = end - cursor;
            int saptamani = rest.Days / 7;
            int zile = rest.Days % 7;
            int ore = rest.Hours;
            int minute = rest.Minutes;

            string rezultat = "";
            if (ani > 0) rezultat += $"{ani} ani, ";
            if (luni > 0) rezultat += $"{luni} luni, ";
            if (saptamani > 0) rezultat += $"{saptamani} săpt, ";
            if (zile > 0) rezultat += $"{zile} zile, ";
            rezultat += $"{ore} ore, {minute} min";

            return rezultat;
        }
    }
}