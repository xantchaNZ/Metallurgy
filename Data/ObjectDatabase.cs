using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Web.Script.Serialization;
using Data.Types;

namespace Data
{
	public class ObjectDatabase
	{
		private const string UnitsPath = @"E:\Nico\Metallurgy\units.txt";
		private const string FormationsPath = @"E:\Nico\Metallurgy\formations.txt";

		public List<Unit> Units { get; set; }
		public List<Formation> Formations { get; set; }

		private static ObjectDatabase _instance;
		public static ObjectDatabase Database
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ObjectDatabase();
				}

				return _instance;
			}
		}

		private ObjectDatabase()
		{
			LoadUnits();
			LoadFormations();
		}

		public void Save()
		{
			SaveUnits();
			SaveFormations();
		}

		private void SaveUnits()
		{
			var serialiser = new JavaScriptSerializer();
			var output = serialiser.Serialize(Units);
			using (var fs = new FileStream(UnitsPath, FileMode.Create))
			{
				fs.Write(Encoding.ASCII.GetBytes(output), 0, output.Length);
			}
		}

		private void LoadUnits()
		{
			Units = new List<Unit>();

			try
			{
				var text = "";
				using (var sr = new StreamReader(UnitsPath))
				{
					text = sr.ReadToEnd();
				}

				var serialiser = new JavaScriptSerializer();
				Units = serialiser.Deserialize<List<Unit>>(text);
			}
			catch
			{
				// Empty units
			}
		}

		private void SaveFormations()
		{
			var serialiser = new JavaScriptSerializer();
			var output = serialiser.Serialize(Formations);
			using (var fs = new FileStream(FormationsPath, FileMode.Create))
			{
				fs.Write(Encoding.ASCII.GetBytes(output), 0, output.Length);
			}
		}

		private void LoadFormations()
		{
			Formations = new List<Formation>();

			try
			{
				var text = "";
				using (var sr = new StreamReader(FormationsPath))
				{
					text = sr.ReadToEnd();
				}

				var serialiser = new JavaScriptSerializer();
				Formations = serialiser.Deserialize<List<Formation>>(text);
			}
			catch
			{
				// Empty formations
			}
		}

		public Unit GetUnitByName(string name)
		{
			try
			{
				return Units.Single(x => x.Name == name);
			}
			catch
			{
				return null;
			}
		}

		public Formation GetFormationByName(string name)
		{
			try
			{
				return Formations.Single(x => x.Name == name);
			}
			catch
			{
				return null;
			}
		}
	}
}
