using System;
using System.IO;
using Newtonsoft.Json;

namespace GlycReSoft
{
	public class Feature
	{
		public Double Initial;
		public Double numChargeStates;
		public Double ScanDensity;
		public Double numModiStates;
		public Double totalVolume;
		public Double ExpectedA;
		public Double CentroidScan;
		public Double numOfScan;
		public Double avgSigNoise;

		public static String REFERENCE_MODEL_FEATURE_PATH = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, 
			                                                    "Data", "DefaultFeatures.fea");

		public static Feature ReadFeature (String path)
		{
			Feature Ans = new Feature ();
			try {
				FileStream FS = new FileStream (path, FileMode.Open, FileAccess.Read);
				StreamReader read1 = new StreamReader (FS);
				String line = read1.ReadLine ();
				String[] Lines = line.Split (',');
				Ans.Initial = Convert.ToDouble (Lines [0]);
				Ans.numChargeStates = Convert.ToDouble (Lines [1]);
				Ans.ScanDensity = Convert.ToDouble (Lines [2]);
				Ans.numModiStates = Convert.ToDouble (Lines [3]);
				Ans.totalVolume = Convert.ToDouble (Lines [4]);
				Ans.ExpectedA = Convert.ToDouble (Lines [5]);
				Ans.CentroidScan = Convert.ToDouble (Lines [6]);
				Ans.numOfScan = Convert.ToDouble (Lines [7]);
				Ans.avgSigNoise = Convert.ToDouble (Lines [8]);
				read1.Close ();
				FS.Close ();
			} catch (Exception ex) {
				throw new FeatureFileException ("An error occurred while parsing feature file " + path, ex);
			}
			return Ans;
		}

		public override string ToString ()
		{
			return JsonConvert.SerializeObject (this, Formatting.Indented);
		}
	}

	class FeatureFileException : Exception
	{
		public FeatureFileException (string str = null)
		{

		}

		public FeatureFileException (string str, Exception inner)
		{

		}
	}
}

