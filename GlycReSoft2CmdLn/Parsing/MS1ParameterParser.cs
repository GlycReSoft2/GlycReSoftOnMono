using System;
using System.IO;
using Newtonsoft.Json;

namespace GlycReSoft{
	/* Replaces the "parameters" and "para" classes from the original implementation. 
	 * Does not have a GUI for modifying these parameters. They must be set from a 
	 * file or commandline.
	 * 
	 */
	public class MS1Parameters
	{
		public Double dataNoiseTheshold { get; set; }
		public Double minScoreThreshold { get; set; }
		public Double matchErrorEM { get; set; }
		public Int32  molecularWeightLowerBound { get; set; }
		public Int32  molecularWeightUpperBound { get; set; }
		public Double groupingErrorEG { get; set; }
		public Double adductToleranceEA { get; set; }
		public Int32  minScanNumber { get; set; }

		override public String ToString(){
			return JsonConvert.SerializeObject (this, Formatting.Indented);
		}

		public static string DEFAULT_PARAMETER_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "MS1Parameters.json");

		public static MS1Parameters getParamtersFromJSON(string filePath){
			MS1Parameters paramData = new MS1Parameters();
			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			StreamReader readPara = new StreamReader(fs);
			String data = readPara.ReadToEnd ();
			fs.Close();
			MS1Parameters tmp = JsonConvert.DeserializeObject<MS1Parameters> (data);
			return tmp;
		}
	}

}
