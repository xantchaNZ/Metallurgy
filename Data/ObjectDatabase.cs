using System;
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
		private const string UnitsPath = @"units.txt";
		private const string FormationsPath = @"formations.txt";
		private const string WorkPath = @"E:\Nico\Metallurgy\";
		private const string HomePath = @"C:\Work\Development\Metallurgy\Metallurgy\";

		private string GetPath(string path)
		{
			return string.Concat(Environment.MachineName.ToUpper() == "NICO-PC" ? HomePath : WorkPath, path);
		}

		public List<Unit> Units { get; set; }
		public List<Formation> Formations { get; set; }

		private Dictionary<Guid, Unit> unitsById;
		private Dictionary<string, Unit> unitsByName;

		private Dictionary<Guid, Formation> formationsByID;
		private Dictionary<string, Formation> formationsByName;

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
			using (var fs = new FileStream(GetPath(UnitsPath), FileMode.Create))
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
				using (var sr = new StreamReader(GetPath(UnitsPath)))
				{
					text = sr.ReadToEnd();
				}

				var serialiser = new JavaScriptSerializer();
				Units = serialiser.Deserialize<List<Unit>>(text);

				unitsById = new Dictionary<Guid, Unit>();
				unitsByName = new Dictionary<string, Unit>();
				foreach (var unit in Units)
				{
					unitsById.Add(unit.ID, unit);
					unitsByName.Add(unit.Name, unit);
				}
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
			using (var fs = new FileStream(GetPath(FormationsPath), FileMode.Create))
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
				using (var sr = new StreamReader(GetPath(FormationsPath)))
				{
					text = sr.ReadToEnd();
				}

				var serialiser = new JavaScriptSerializer();
				Formations = serialiser.Deserialize<List<Formation>>(text);

				formationsByID = new Dictionary<Guid, Formation>();
				formationsByName = new Dictionary<string, Formation>();
				foreach (var formation in Formations)
				{
					formationsByID.Add(formation.ID, formation);
					formationsByName.Add(formation.Name, formation);
				}
			}
			catch
			{
				// Empty formations
			}
		}

		public Unit GetUnitByName(string name)
		{
			return unitsByName.ContainsKey(name)
			       	? unitsByName[name]
			       	: null;
		}

		public Unit GetUnitByID(Guid id)
		{
			return unitsById.ContainsKey(id)
					? unitsById[id]
			       	: null;
		}

		public Formation GetFormationByName(string name)
		{
			return formationsByName.ContainsKey(name)
					? formationsByName[name]
			       	: null;
		}

		public Formation GetFormationByID(Guid id)
		{
			return formationsByID.ContainsKey(id)
					? formationsByID[id]
			       	: null;
		}
	}
}
