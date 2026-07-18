using System;
using System.IO;
using System.Text.Json;

namespace TruckParkingManager
{
	// Configurare încărcată din appsettings.json la pornire.
	// Dacă fișierul lipsește sau e corupt, se folosesc valorile implicite de mai jos,
	// iar aplicația tot pornește (nu blochează operatorul de la poartă).
	public class ConfigurareApp
	{
		public string ApiUrl { get; set; } = "https://api.compania-ta.ro/parcare";
		public int CapacitateMaxima { get; set; } = 50;

		private const string CaleConfig = "appsettings.json";

		public static ConfigurareApp Incarca()
		{
			try
			{
				if (File.Exists(CaleConfig))
				{
					string json = File.ReadAllText(CaleConfig);
					var config = JsonSerializer.Deserialize<ConfigurareApp>(json);
					if (config != null)
					{
						return config;
					}
				}
			}
			catch
			{
				// fișier corupt/lipsă -> folosim valorile implicite, fără să blocăm pornirea
			}

			return new ConfigurareApp();
		}
	}
}