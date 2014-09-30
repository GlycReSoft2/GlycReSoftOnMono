using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;
using ManyConsole;

namespace GlycReSoft
{
	public class GlycopeptideCompositionHypothesisDriver
	{
		String GlycanCompHypoFile;
		String ProteinProspectorFile;
		String GlycositeIndexFile;
		List<CompositionHypothesis.comphypo> GlycoPeptideHypothesis;
		List<CompositionHypothesis.PPMSD> ProteinProspectorData;
		int[] GlycoSites;
		CompositionHypothesis.generatorData Generator;
		bool verbose = false;
		bool debug = false;

		public GlycopeptideCompositionHypothesisDriver (String glycanHypoFile, String protProsFile, 
		                                                String glycositeIndexFile, bool verbose, bool debug)
		{
			GlycanCompHypoFile = glycanHypoFile;
			ProteinProspectorFile = protProsFile;
			GlycositeIndexFile = glycositeIndexFile;
			this.verbose = verbose;
			this.debug = debug;
			Generator = new CompositionHypothesis.generatorData ();
			GlycoSites = CompositionHypothesis.ParseGlycoSiteFile (GlycositeIndexFile);
			ProteinProspectorData = CompositionHypothesis.ReadTabDelimProteinProspector (ProteinProspectorFile);
			foreach (CompositionHypothesis.PPMSD pp in ProteinProspectorData) {
				pp.selected = true;
				foreach (int i in GlycoSites) {
					if (pp.StartAA <= i && pp.EndAA >= i) {
						pp.numGly++;
					}
				}
			}
			Utils.Debug ("# of PPMSD:", ProteinProspectorData.Count);
		}

		/* Converts the list of composition hypothesis objects, this.Hypothesis into a list of lists of strings.
		 * This list of lists is used to generate the resulting CSV
		*/
		public List<List<string>> ExtractTable (List<CompositionHypothesis.comphypo> hypothesis)
		{
			List<List<string>> hypo = new List<List<string>> ();
			List<string> elementIDs = new List<string> ();
			List<string> molname = new List<string> ();
			for (int j = 0; j < hypothesis.Count; j++) {
				if (hypothesis [j].elementIDs.Count > 0) {
					for (int i = 0; i < hypothesis [j].elementIDs.Count; i++) {
						elementIDs.Add (hypothesis [j].elementIDs [i]);
					}
					for (int i = 0; i < hypothesis [j].MoleNames.Count; i++) {
						molname.Add (hypothesis [j].MoleNames [i]);
					}
					break;
				}
			}
			List<string> header = new List<string> ();
			#region Generate the column headers
			header.Add ("Molecular Weight");
			for (int i = 0; i < elementIDs.Count; i++) {
				header.Add (elementIDs [i]);
			}

			header.Add ("Compositions");
			for (int i = 0; i < hypothesis [0].eqCounts.Count; i++) {
				header.Add (molname [i]);
			}

			header.Add ("Adduct/Replacement");
			header.Add ("Adduct Amount");

			header.Add ("Peptide Sequence");
			header.Add ("Peptide Modification");
			header.Add ("Peptide Missed Cleavage Number");
			header.Add ("Number of Glycan Attachment to Peptide");
			header.Add ("Start AA");
			header.Add ("End AA");

			hypo.Add (header);
			#endregion

			#region Generate each row of the CSV
			foreach (CompositionHypothesis.comphypo row in hypothesis) {
				List<string> results = new List<string> ();
				results.Add (Convert.ToString (row.MW));
				foreach (int elem in row.elementAmount) {
					results.Add (Convert.ToString (elem));
				}
				String composition = "";
				if (row.eqCount.Count > 1) {
					composition = "[";
					foreach (int mol in row.eqCounts) {
						composition += (Convert.ToString (mol)) + ";";
					}
					composition = composition.Remove (composition.Length - 1) + "]";
				} else {
					composition = "0";
				}
				results.Add (composition);
				foreach (int mol in row.eqCounts) {
					results.Add (Convert.ToString (mol));
				}
				results.Add (Generator.Modification [0] + "/" + Generator.Modification [1]);
				results.Add (Convert.ToString (row.AdductNum));
				results.Add (row.PepSequence);
				results.Add (row.PepModification);
				results.Add (Convert.ToString (row.MissedCleavages));

				//
				results.Add (Convert.ToString (row.numGly));
				results.Add (Convert.ToString (row.StartAA));
				results.Add (Convert.ToString (row.EndAA));

				hypo.Add (results);
			}
			#endregion
			return hypo;
		}


		public void Run (String outPath)
		{
			GlycoPeptideHypothesis = CompositionHypothesis.GenerateGlycopeptideHypothesis (GlycanCompHypoFile, ProteinProspectorData);
			Generator.Modification = CompositionHypothesis.GetCompHypo (GlycanCompHypoFile) [0].AddRep.Split ('/');

			// Make sure the output has somewhere to go.
			if (outPath == null) {
				outPath = Path.ChangeExtension (ProteinProspectorFile, ".glycopeptide_hypothesis.csv");
			}
			if (!Directory.Exists (Path.GetDirectoryName (outPath))) {
				Directory.CreateDirectory (Path.GetDirectoryName (outPath));
			}
			Utils.Warn ("Output Path set to: " + outPath);
			ConvertResultsToCSV (outPath);
			if (debug) {
				Utils.Debug ("Verifying output can be read");
				CompositionHypothesis.GetCompHypo (outPath);
				Utils.Debug ("output read.");
			}
		}

		public void ConvertResultsToCSV (string outPath)
		{
			var fileStream = new FileStream (outPath, FileMode.Create, FileAccess.Write);
			var writer = new StreamWriter (fileStream);

			List<List<string>> table = ExtractTable (GlycoPeptideHypothesis);
			foreach (List<string> row in table) {
				writer.Write (String.Join (",", row) + "\n");
			}

			writer.Close ();
			fileStream.Close ();
		}
	}

	public class GenerateGlycoPeptideCompositionHypothesis : ConsoleCommand
	{
		public bool RunVerbose = false;
		public bool RunDebug = false;
		public bool ShowHelp = false;
		public string GlycanHypothesisFile = null;
		public string ProteinProspectorFile = null;
		public string GlycoSiteFile = null;

		public string OutputFile = null;

		public GenerateGlycoPeptideCompositionHypothesis ()
		{
			IsCommand ("GenGlycoPeptideHypothesis", "Generate a GlycoPeptide Composition Hypothesis file for use" +
			" in classifying glycan data or for generating a Glycopeptide Composition Hypothesis\n");

			Options = new OptionSet ()
				.Add ("v|verbose", "Emit additional information during run", (string v) => RunVerbose |= v != null)
				.Add ("h|?|help", "Show help message", (string v) => ShowHelp = v != null)
				#region InputFiles
				.Add ("g|glycanComposition=", "Path to the Glycan composition file to build the hypothesis from", 
				v => GlycanHypothesisFile = v)
				.Add ("p|proteinProspector=", "Path to the Protein Prospector output file in TSV format", 
				v => ProteinProspectorFile = v)
				.Add ("s|glycoSite=", "Path to the GlycoSite file", 
				v => GlycoSiteFile = v)

				.Add ("o|outputFile", "Path to store results in", (string v) => OutputFile = v)
				#endregion
				.Add ("debug", "Debugging Flag", (string v) => RunDebug = true);
		}

		public override int Run (string[] remainingArguments)
		{
			if (ShowHelp || (GlycanHypothesisFile == null) || !File.Exists (GlycanHypothesisFile) ||
			    (ProteinProspectorFile == null) || !File.Exists (ProteinProspectorFile) ||
			    (GlycoSiteFile == null) || !File.Exists (GlycoSiteFile)) {
				Console.WriteLine ("GlycReSoft2 GenGlycoPeptideHypothesis Help:\n");
				Options.WriteOptionDescriptions (Console.Out);
				return -1;
			}

			try {
				var driver = new GlycopeptideCompositionHypothesisDriver (GlycanHypothesisFile,
					             ProteinProspectorFile, GlycoSiteFile, RunVerbose, RunDebug);
				driver.Run (OutputFile);
			} catch (Exception ex) {
				Utils.Warn (ex);
				return -1;
			}

			return 0;
		}

	}
}

