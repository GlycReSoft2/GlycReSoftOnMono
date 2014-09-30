using System;
using System.IO;
using System.Collections.Generic;
using ManyConsole;
using NDesk.Options;

namespace GlycReSoft
{
	///<summary>
	///An object-oriented driver for running supervised learning and handling its results
	///</summary>
	public class MS1SupervisedLearningDriver
	{
		MS1Parameters MS1Params;
		String[] deconData;
		List<CompositionHypothesis.comphypo> glycCompHypo;
		bool verbose;
		bool debug;
		List<ResultsGroup>[] results;

		public MS1SupervisedLearningDriver (MS1Parameters ms1, String[] deconToolsFiles, 
		                                    String glycanCompHypoFile, bool verbose = false, 
		                                    bool debug = false)
		{
			this.MS1Params = ms1;
			this.deconData = deconToolsFiles;
			this.glycCompHypo = CompositionHypothesis.GetCompHypo (glycanCompHypoFile);
			this.verbose = verbose;
			this.debug = debug;

			if (this.debug) {
				Utils.Debug ("Debug Instantiation of MS1SupervisedLearningDriver");
				Utils.Debug ("MS1 Parameters: ", MS1Params);
				foreach (CompositionHypothesis.comphypo comphypo in glycCompHypo) {
					//Utils.Debug (comphypo);
				}
			}
		}

		public void Run (Feature userFeatureData = null, String outputPath = null)
		{
			if (verbose)
				Console.WriteLine ("Running Supervised Learning Algorithm");
			results = SupervisedLearning.Run (this.deconData, this.glycCompHypo, this.MS1Params, userFeatureData);
			if (verbose)
				Console.WriteLine ("Run Complete. Saving Results.");
			if (null == outputPath) {
				outputPath = Path.GetDirectoryName (this.deconData [0]);
				Utils.Warn ("Output Path set to: " + outputPath);
			}
			ConvertResultsToCSV (outputPath);
		}


		public void ConvertResultsToCSV (string outputPath)
		{
			if (!Directory.Exists (outputPath)) {
				Directory.CreateDirectory (outputPath);
				if (!Directory.Exists (outputPath))
					throw new IOException ("could not create output directory");
			}
			for (int i = 0; i < this.deconData.Length; i++) {
				string resPath = Path.Combine (outputPath, "ResultsOf" + Path.GetFileName (this.deconData [i]));
				if (verbose)
					Utils.Warn ("Output file: " + resPath);
				FileStream FS = new FileStream (resPath, FileMode.Create, FileAccess.Write);
				StreamWriter write = new StreamWriter (FS);
				//Write the header
				write.Write ("Score,MassSpec MW,Compound Key,PeptideSequence,PPM Error,#ofAdduct,#ofCharges,#ofScans,ScanDensity,Avg A:A+2 Error,A:A+2 Ratio,Total Volume,Signal to Noise Ratio,Centroid Scan Error,Centroid Scan,MaxScanNumber,MinScanNumber");
				int elementnamecount = 0;
				List<string> elementIDs = new List<string> ();
				List<string> moleculeNames = new List<string> ();
				for (int compIter = 0; i < glycCompHypo.Count; i++) {
					if (glycCompHypo [compIter].elementIDs.Count > 0) {
						for (int elemIter = 0; elemIter < glycCompHypo [compIter].elementIDs.Count; elemIter++) {
							elementIDs.Add (glycCompHypo [compIter].elementIDs [elemIter]);
						}
						for (int molIter = 0; molIter < glycCompHypo [compIter].MoleNames.Count; molIter++) {
							moleculeNames.Add (glycCompHypo [compIter].MoleNames [molIter]);
						}
						break;
					}
				}
				foreach (String name in elementIDs) {
					write.Write ("," + name);
				}
				elementnamecount = elementIDs.Count;
				write.Write (",Hypothesis MW");
				foreach (String name in moleculeNames) {
					write.Write ("," + name);
				}
				int molenamecount = moleculeNames.Count;
				write.WriteLine (",Adduct/Replacement,Adduct Amount,PeptideModification,PeptideMissedCleavage#,#ofGlycanAttachmentToPeptide,StarAA,EndAA");
				for (int u = 0; u < this.results [i].Count; u++) {
					if (this.results [i] [u].comphypo.MW != 0) {
						Double MatchingError = 0;
						if (this.results [i] [u].comphypo.MW != 0) {
							MatchingError = ((this.results [i] [u].DeconRow.monoisotopic_mw - this.results [i] [u].comphypo.MW) / (this.results [i] [u].DeconRow.monoisotopic_mw)) * 1000000;
						}
						write.Write (this.results [i] [u].Score + "," + this.results [i] [u].DeconRow.monoisotopic_mw + "," + this.results [i] [u].comphypo.compoundCompo + "," + this.results [i] [u].comphypo.PepSequence + "," + MatchingError + "," + this.results [i] [u].numModiStates + "," + this.results [i] [u].numChargeStates + "," + this.results [i] [u].numOfScan + "," + this.results [i] [u].ScanDensity + "," + this.results [i] [u].ExpectedA + "," + (this.results [i] [u].DeconRow.mono_abundance / (this.results [i] [u].DeconRow.mono_plus2_abundance + 1)) + "," + this.results [i] [u].totalVolume + "," + this.results [i] [u].DeconRow.signal_noise + "," + this.results [i] [u].CentroidScan + "," + this.results [i] [u].DeconRow.scan_num + "," + this.results [i] [u].maxScanNum + "," + this.results [i] [u].minScanNum);
						for (int s = 0; s < elementnamecount; s++) {
							write.Write ("," + this.results [i] [u].comphypo.elementAmount [s]);
						}
						write.Write ("," + this.results [i] [u].comphypo.MW);
						for (int s = 0; s < molenamecount; s++) {
							write.Write ("," + this.results [i] [u].comphypo.eqCounts [s]);
						}
						write.WriteLine ("," + this.results [i] [u].comphypo.AddRep + "," + this.results [i] [u].comphypo.AdductNum + "," + this.results [i] [u].comphypo.PepModification + "," + this.results [i] [u].comphypo.MissedCleavages + "," + this.results [i] [u].comphypo.numGly + "," + this.results [i] [u].comphypo.StartAA + "," + this.results [i] [u].comphypo.EndAA);
					} else {                            
						write.Write (this.results [i] [u].Score + "," + this.results [i] [u].DeconRow.monoisotopic_mw + "," + 0 + "" + "," + "," + 0 + "," + this.results [i] [u].numModiStates + "," + this.results [i] [u].numChargeStates + "," + this.results [i] [u].numOfScan + "," + this.results [i] [u].ScanDensity + "," + this.results [i] [u].ExpectedA + "," + (this.results [i] [u].DeconRow.mono_abundance / (this.results [i] [u].DeconRow.mono_plus2_abundance + 1)) + "," + this.results [i] [u].totalVolume + "," + this.results [i] [u].DeconRow.signal_noise + "," + this.results [i] [u].CentroidScan + "," + this.results [i] [u].DeconRow.scan_num + "," + this.results [i] [u].maxScanNum + "," + this.results [i] [u].minScanNum);
						for (int s = 0; s < elementnamecount; s++) {
							write.Write ("," + 0);
						}
						write.Write ("," + 0);
						for (int s = 0; s < molenamecount; s++) {
							write.Write ("," + 0);
						}
						write.WriteLine ("," + "N/A" + "," + 0 + "," + "" + "," + 0 + "," + 0 + "," + 0 + "," + 0);
					}
				}
				write.Flush ();
				write.Close ();
				FS.Close ();
			}
		}
	}

	public class RunSupervisedLearningCmd : ConsoleCommand
	{
		//Initialize Option variables
		public bool RunVerbose = false;
		public bool RunDebug = false;
		public bool ShowHelp = false;
		public List<String> DeconToolsFile = new List<String> ();
		public string CompositionHypothesisFile = null;
		public String OutputDir = null;
		public String UserModelPath = null;
		public Feature UserModel = null;
		//Read in default MS1 parameters
		public MS1Parameters MS1Params = MS1Parameters.getParamtersFromJSON (MS1Parameters.DEFAULT_PARAMETER_FILE_PATH);

		public RunSupervisedLearningCmd ()
		{
			IsCommand ("SupervisedLearning", "Runs an iteratively reweighted logistic regression " +
			"to predict glycans and glycopeptides\n");

			//Configure Command Line Option Parser
			Options = new OptionSet ()
				.Add ("v|verbose", "Emit additional information during run", (string v) => RunVerbose |= v != null)
				.Add ("h|?|help", "Show help message", (string v) => ShowHelp = v != null)
				#region InputFiles
				.Add ("d|deconToolsFile=", "Path to DeconTools output file [required]", DeconToolsFile.Add)
				.Add ("g|glycanCompositionHypothesisFile=", "Path to glycan composition hypothesis [required]", 
				(string v) => CompositionHypothesisFile = v)
				.Add ("o|outputDir", "Path to store results in", (string v) => OutputDir = v)
				.Add ("f|featureModelFile", "Path to a feature model file to assist scoring", 
				(string v) => UserModelPath = v)
				#endregion
				#region MS1 Parameter Settings
				.Add ("MS1.minScoreThreshold=", "MS1 Minimum Score Threshold", 
				(string v) => MS1Params.minScoreThreshold = Convert.ToDouble (v))
				.Add ("MS1.matchErrorEM=", "MS1 Match Error EM", 
				(string v) => MS1Params.matchErrorEM = Convert.ToDouble (v))
				.Add ("MS1.molecularWeightLowerBound=", "MS1 Molecular Weight Lower Bound", 
				(string v) => MS1Params.molecularWeightLowerBound = Convert.ToInt32 (v))
				.Add ("MS1.molecularWeightUpperBound=", "MS1 Molecular Weight Upper Bound", 
				(string v) => MS1Params.molecularWeightUpperBound = Convert.ToInt32 (v))
				.Add ("MS1.groupingErrorEG=", "MS1 Grouping Error EG", 
				(string v) => MS1Params.groupingErrorEG = Convert.ToDouble (v))
				.Add ("MS1.adductToleranceEA=", "MS1 Adduct Tolerance EA", 
				(string v) => MS1Params.adductToleranceEA = Convert.ToDouble (v))
				.Add ("MS1.minScanNumber=", "MS1 Minimum Scan Number", 
				(string v) => MS1Params.minScanNumber = Convert.ToInt32 (v))
				#endregion
				.Add ("debug", "Debugging Flag", (string v) => RunDebug = true);

		}

		public override int Run (string[] remainingArguments)
		{
			if (ShowHelp || (DeconToolsFile == null) || (CompositionHypothesisFile == null)) {
				Console.WriteLine ("GlycReSoft2 SupervisedLearning Help:\n");
				Options.WriteOptionDescriptions (Console.Out);
				return -1;
			}

			if (RunVerbose && false) {
				Console.Write ("MS1 Parameters:");
				Console.WriteLine (MS1Params);
				Console.WriteLine ("DeconTools Files:");
				foreach (string f in DeconToolsFile) {
					Console.WriteLine ("\t" + f);
				}
				Console.WriteLine ("Output Directory: " + OutputDir);
			}

			try {
				UserModel = Feature.ReadFeature (UserModelPath);
			} catch (FeatureFileException ex) {
				Utils.Warn ("Could not load user feature model, using defaults");
				if (RunDebug)
					Utils.Debug (ex);
				UserModel = Feature.ReadFeature (Feature.REFERENCE_MODEL_FEATURE_PATH);
			}
			try {
				MS1SupervisedLearningDriver supervisedLearningStrategy = 
					new MS1SupervisedLearningDriver (MS1Params, DeconToolsFile.ToArray (), 
						CompositionHypothesisFile, 
						RunVerbose, 
						RunDebug);
				supervisedLearningStrategy.Run (userFeatureData: UserModel, outputPath: OutputDir);
			} catch (Exception ex) {
				Console.WriteLine ("An error occurred while running supervised learning algorithm");
				if (RunVerbose)
					Utils.Warn (ex);
				Console.ReadLine ();
				throw;
			}

			return 0;
		}
	}

}


