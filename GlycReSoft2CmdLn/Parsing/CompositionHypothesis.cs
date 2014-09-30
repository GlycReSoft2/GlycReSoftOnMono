using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using YAMP_MathParserTK_NET;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;

namespace GlycReSoft
{
	public static class CompositionHypothesis
	{
		//This function reads the generate composition page and save it to a variable of class "generator".
		public static generatorData GetGenerator (String compositionFilePath)
		{
			//compotable is used to store the data from the composition table before they are used.
			List<compTable> compotable = new List<CompositionHypothesis.compTable> ();
			//arTable is used to store the data from additional rules table before they're used.
			List<arTable> arTable = new List<CompositionHypothesis.arTable> ();
			//GD is used to store all the data that will be returned.
			generatorData GD = new generatorData ();

			String currentpath = compositionFilePath;
			try {
				FileStream reading = new FileStream (currentpath, FileMode.Open, FileAccess.Read);
				StreamReader readcompo = new StreamReader (reading);
				//Read the first line to skip the column names:
				String Line1 = readcompo.ReadLine ();
				String[] headers = Line1.Split (',');
				List<string> elementIDs = new List<string> ();
				bool moreElements = true;
				int sh = 2;
				while (moreElements) {
					if (headers [sh] != "Lower Bound") {
						elementIDs.Add (headers [sh]);
						sh++;
					} else
						moreElements = false;
				}

				bool firstrow = true;
				//Read the other lines for compTable data.
				while (readcompo.Peek () >= 0) {
					compTable compTable = new compTable ();
					if (firstrow) {
						compTable.elementIDs = elementIDs;
						firstrow = false;
					}
					String Line = readcompo.ReadLine ();
					String[] eachentry = Line.Split (',');
					//When next line is the modification line, it breaks.
					if (eachentry [0] == "Modification%^&!@#*()iop")
						break;
					compTable.Letter = eachentry [0];
					compTable.Molecule = eachentry [1];
					for (int i = 2; i < elementIDs.Count + 2; i++) {
						compTable.elementAmount.Add (Convert.ToInt32 (eachentry [i]));
					}
					List<String> bounds = new List<String> ();
					bounds.Add (eachentry [elementIDs.Count + 2]);
					bounds.Add (eachentry [elementIDs.Count + 3]);
					compTable.Bound = bounds;
					compotable.Add (compTable);
				}
				//Send compotable data to GD                
				GD.comTable = compotable;
				//Populate the Modification Table
				String modiLine = readcompo.ReadLine ();
				//Send Modification data to GD
				GD.Modification = modiLine.Split (',');

				//Populate the additional rules table
				while (readcompo.Peek () >= 0) {
					String Line = readcompo.ReadLine ();
					String[] eachentry = Line.Split (',');
					if (eachentry [0] != "" && eachentry [2] != "") {
						arTable areTable = new arTable ();
						areTable.Formula = eachentry [0];
						areTable.Relationship = eachentry [1];
						areTable.Constraint = Convert.ToString (eachentry [2]);
						arTable.Add (areTable);
					}
				}
				//Send arTable data to GD.
				GD.aTable = arTable;

				readcompo.Close ();
				reading.Close ();
			} catch (Exception compoex) {
				Console.WriteLine ("Error in loading Composition Table. Error:" + compoex);
				throw;
			}
			return GD;
		}

		//This function reads a composition hypothesis file, get its data and return a list of comphypo.
		public static List<comphypo> GetCompHypo (String currentPath)
		{
			//This is the list for storing the answer.
			List<comphypo> compotable = new List<comphypo> ();

			try {
				FileStream reading = new FileStream (currentPath, FileMode.Open, FileAccess.Read);
				StreamReader readcompo = new StreamReader (reading);
				//Read the first line to skip the column names:
				String head = readcompo.ReadLine ();
				String[] headers = head.Split (',');
				List<string> molename = new List<string> ();
				List<string> elementIDs = new List<string> ();
				int h = 1;
				while (headers [h] != "Compositions") {
					elementIDs.Add (headers [h]);
					h++;
				}
				h++;
				while (headers [h] != "Adduct/Replacement") {
					molename.Add (headers [h]);
					h++;
				}
				bool firstrow = true;
				//Read the other lines for compTable data.
				while (readcompo.Peek () >= 0) {
					String Line = readcompo.ReadLine ();
					String[] eachentry = Line.Split (',');
					if (eachentry.Count () < 2)
						break;
					if (string.IsNullOrEmpty (eachentry [0]))
						break;
					//comhyp is used to store the data that will be put into the list, compotable.
					comphypo comhyp = new comphypo ();
					comhyp.TrueOrFalse = true;
					if (firstrow) {
						comhyp.elementIDs = elementIDs;
						comhyp.MoleNames = molename;
						firstrow = false;
					}
					comhyp.MW = Convert.ToDouble (eachentry [0]);
					int i = 1;
					bool moreElements = true;
					while (moreElements) {
						if (headers [i] != "Compositions") {
							comhyp.elementAmount.Add (Convert.ToInt32 (eachentry [i]));
							i++;
						} else
							moreElements = false;
					}
					comhyp.compoundCompo = Convert.ToString (eachentry [i]);
					i++;
					bool moreCompounds = true;
					List<int> eqCoun = new List<int> ();
					while (moreCompounds) {
						if (headers [i] != "Adduct/Replacement") {
							if (!String.IsNullOrEmpty (eachentry [i]))
								eqCoun.Add (Convert.ToInt32 (eachentry [i]));
							else
								eqCoun.Add (0);
							i++;
						} else
							moreCompounds = false;
					}
					comhyp.eqCounts = eqCoun;
					comhyp.AddRep = Convert.ToString (eachentry [i]);
					comhyp.AdductNum = Convert.ToInt32 (eachentry [i + 1]);
					if (eachentry.Count () > (i + 2)) {
						comhyp.PepSequence = eachentry [i + 2];
						comhyp.PepModification = eachentry [i + 3];
						comhyp.MissedCleavages = Convert.ToInt32 (eachentry [i + 4]);
						comhyp.numGly = Convert.ToInt32 (eachentry [i + 5]);
						comhyp.StartAA = Convert.ToInt32 (eachentry [i + 6]);
						comhyp.EndAA = Convert.ToInt32 (eachentry [i + 7]);
					} else {
						comhyp.PepSequence = "";
						comhyp.PepModification = "";
						comhyp.MissedCleavages = 0;
						comhyp.numGly = 0;
						comhyp.StartAA = 0;
						comhyp.EndAA = 0;
					}
					compotable.Add (comhyp);
				}

				readcompo.Close ();
				reading.Close ();
			} catch (Exception compoex) {
				Console.WriteLine ("Error in loading Composition Hypothesis File. Error:" + compoex);
				throw(compoex);	
			}
			return compotable;
		}
			
		//This function reads tab delimited file.
		public static List<PPMSD> ReadTabDelimProteinProspector (String currentpath)
		{
			List<PPMSD> data = new List<PPMSD> ();
			FileStream FS = new FileStream (currentpath, FileMode.Open, FileAccess.Read);
			StreamReader read = new StreamReader (FS);
			//skip title line:
			read.ReadLine ();
			while (read.Peek () >= 0) {
				PPMSD pp = new PPMSD ();
				String line = read.ReadLine ();
				String[] Lines = line.Split ('\t');
				pp.number = Convert.ToInt32 (Lines [0]);
				pp.Mass = Convert.ToDouble (Lines [1]);
				pp.Charge = Convert.ToInt32 (Lines [3]);
				pp.Modifications = Convert.ToString (Lines [4]);
				pp.StartAA = Convert.ToInt32 (Lines [5]);
				pp.EndAA = Convert.ToInt32 (Lines [6]);
				pp.MissedCleavages = Convert.ToInt32 (Lines [7]);
				pp.PreviousAA = Convert.ToString (Lines [8]);
				pp.Sequence = Convert.ToString (Lines [9]);
				pp.NextAA = Convert.ToString (Lines [10]);
				data.Add (pp);
			}
			read.Close ();
			FS.Close ();

			return data;
		}
		//This function reads in a list of PPMSD and returns the full sequence
		public static String GetSequence (List<PPMSD> data)
		{
			int startAA = 1;
			String sequence = "";
			data = data.OrderBy (a => a.StartAA).ToList ();
			foreach (PPMSD pp in data) {
				if (pp.StartAA == startAA) {
					sequence = sequence + pp.Sequence;
					startAA = pp.EndAA + 1;
				}                
			}
			return sequence;
		}

		//			//This function reads datagridview4 and obtain its data.
		//			private List<PPMSD> obtainPP()
		//			{
		//				List<PPMSD> PP = new List<PPMSD>();
		//				for (int i = 0; i < dataGridView4.Rows.Count; i++)
		//				{
		//					//if (Convert.ToInt32(dataGridView4.Rows[i].Cells[0].Value) == 0)
		//					// continue;
		//					PPMSD pp = new PPMSD();
		//					pp.selected = Convert.ToBoolean((dataGridView4.Rows[i].Cells[0].Value));
		//					pp.Mass = Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
		//					pp.Modifications = Convert.ToString(dataGridView4.Rows[i].Cells[2].Value);
		//					pp.StartAA = Convert.ToInt32(dataGridView4.Rows[i].Cells[3].Value);
		//					pp.EndAA = Convert.ToInt32(dataGridView4.Rows[i].Cells[4].Value);
		//					pp.MissedCleavages = Convert.ToInt32(dataGridView4.Rows[i].Cells[5].Value);
		//					pp.Sequence = Convert.ToString(dataGridView4.Rows[i].Cells[6].Value);
		//					pp.numGly = Convert.ToInt32(dataGridView4.Rows[i].Cells[7].Value);
		//					PP.Add(pp);
		//				}
		//				return PP;
		//			}
		//
		//			//This function is used by the glycopeptide generator to generate hypothesis and put it into datagridview.
		//			private static String comhypopathStore;
		//			private void generateGPCompHypo(String comhypopath)
		//			{
		//				comhypopathStore = comhypopath;
		//				BackgroundWorker bw = new BackgroundWorker();
		//				bw.DoWork += new DoWorkEventHandler(bw_DoWork2);
		//				bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted2);
		//
		//				//set lable and your waiting text in this form
		//				try
		//				{
		//					bw.RunWorkerAsync();//this will run the DoWork code at background thread
		//					msgForm.ShowDialog();//show the please wait box
		//				}
		//				catch (Exception ex)
		//				{
		//					MessageBox.Show(ex.ToString());
		//				}
		//
		//				dataGridView2.DataSource = theComhypoOnTab2;
		//				button6.Enabled = true;
		//			}
		//			void bw_DoWork2(object sender, DoWorkEventArgs e)
		//			{
		//				stuffToDo2(comhypopathStore);
		//			}
		//			void bw_RunWorkerCompleted2(object sender, RunWorkerCompletedEventArgs e)
		//			{
		//				if (e.Error != null)
		//				{
		//					MessageBox.Show(Convert.ToString(e.Error));
		//				}
		//				//all background work has completed and we are going to close the waiting message
		//				msgForm.Close();
		//			}
		//			private void stuffToDo2(string comhypopath)
		//			{
		//				List<comphypo> CHy = getCompHypo(comhypopath);
		//				List<string> elementIDs = new List<string>();
		//				List<string> molename = new List<string>();
		//				for (int j = 0; j < CHy.Count(); j++)
		//				{
		//					if (CHy[j].elementIDs.Count > 0)
		//					{
		//						for (int i = 0; i < CHy[j].elementIDs.Count(); i++)
		//						{
		//							elementIDs.Add(CHy[j].elementIDs[i]);
		//						}
		//						for (int i = 0; i < CHy[j].MoleNames.Count(); i++)
		//						{
		//							molename.Add(CHy[j].MoleNames[i]);
		//						}
		//						break;
		//					}
		//				}
		//
		//				String AddRep = CHy[0].AddRep;
		//
		//				int indexH = 0;
		//				int indexO = 0;
		//				int indexWater = 0;
		//				try
		//				{
		//					indexH = elementIDs.IndexOf("H");
		//					indexO = elementIDs.IndexOf("O");
		//					indexWater = molename.IndexOf("Water");
		//				}
		//				catch
		//				{
		//					MessageBox.Show("Your composition hypothesis contains a compound without Water. Job terminated.");
		//					return;
		//				}
		//				List<PPMSD> PP = obtainPP();
		//				List<comphypo> Ans = new List<comphypo>();
		//				//Ans.AddRange(CHy);
		//				PeriodicTable PT = new PeriodicTable();
		//				for (int i = 0; i < PP.Count; i++)
		//				{
		//					if (PP[i].selected)
		//					{
		//						Int32 Count = Convert.ToInt32(PP[i].numGly);
		//						List<comphypo> Temp = new List<comphypo>();
		//						comphypo temp = new comphypo();
		//						//First line:
		//						temp.compoundCompo = "";
		//						temp.AdductNum = 0;
		//						temp.AddRep = "";
		//						temp.MW = PP[i].Mass;
		//						for (int s = 0; s < CHy[0].eqCounts.Count; s++)
		//						{
		//							temp.eqCounts.Add(0);
		//						}
		//						for (int s = 0; s < CHy[0].elementAmount.Count; s++)
		//						{
		//							temp.elementAmount.Add(0);
		//						}
		//						//columns for glycopeptides
		//						temp.PepModification = PP[i].Modifications;
		//						temp.PepSequence = PP[i].Sequence;
		//						temp.MissedCleavages = PP[i].MissedCleavages;
		//						temp.StartAA = PP[i].StartAA;
		//						temp.EndAA = PP[i].EndAA;
		//						temp.numGly = 0;
		//						Temp.Add(temp);
		//						for (int j = 0; j < Count; j++)
		//						{
		//							List<comphypo> Temp2 = new List<comphypo>();
		//							for (int k = 0; k < Temp.Count(); k++)
		//							{
		//								//need to reread the file and get new reference, because c# keeps passing by reference which creates a problem.
		//								List<comphypo> CH = getCompHypo(comhypopath);
		//								for (int l = 0; l < CH.Count(); l++)
		//								{
		//									comphypo temp2 = new comphypo();
		//									temp2 = CH[l];
		//									temp2.numGly = Temp[k].numGly + 1;
		//									temp2.PepModification = Temp[k].PepModification;
		//									temp2.PepSequence = Temp[k].PepSequence;
		//									temp2.MissedCleavages = Temp[k].MissedCleavages;
		//									temp2.StartAA = Temp[k].StartAA;
		//									temp2.EndAA = Temp[k].EndAA;
		//									List<string> forsorting = new List<string>();
		//									forsorting.Add(Temp[k].compoundCompo);
		//									forsorting.Add(temp2.compoundCompo);
		//									forsorting = forsorting.OrderBy(a => a).ToList();
		//									temp2.compoundCompo = forsorting[0] + forsorting[1];
		//									temp2.AdductNum = temp2.AdductNum + Temp[k].AdductNum;
		//									for (int s = 0; s < temp2.eqCounts.Count; s++)
		//									{
		//										temp2.eqCounts[s] = temp2.eqCounts[s] + Temp[k].eqCounts[s];
		//									}
		//									for (int s = 0; s < temp2.elementAmount.Count; s++)
		//									{
		//										temp2.elementAmount[s] = temp2.elementAmount[s] + Temp[k].elementAmount[s];
		//									}
		//									for (int ui = 0; ui < molename.Count(); ui++)
		//									{
		//										if (molename[ui] == "Water")
		//										{
		//											if (temp2.eqCounts[ui] > 0)
		//											{
		//												temp2.eqCounts[ui] = temp2.eqCounts[ui] - 1;
		//											}
		//											break;
		//										}
		//									}
		//									temp2.elementAmount[indexH] = temp2.elementAmount[indexH] - 2;
		//									temp2.elementAmount[indexO] = temp2.elementAmount[indexO] - 1;
		//									if (temp2.elementAmount[indexO] < 0)
		//										temp2.elementAmount[indexO] = 0;
		//									if (temp2.elementAmount[indexH] < 0)
		//										temp2.elementAmount[indexH] = 0;
		//									temp2.MW = temp2.MW + Temp[k].MW - PT.getMass("H") * 2 - PT.getMass("O");
		//									Temp2.Add(temp2);
		//								}
		//							}
		//							Temp.AddRange(Temp2);
		//						}
		//						Ans.AddRange(Temp);
		//					}
		//				}
		//				//Remove Duplicates from CHy
		//				Ans = Ans.OrderBy(a => a.MW).ToList();
		//				CHy.Clear();
		//				for (int i = 0; i < Ans.Count() - 1; i++)
		//				{
		//					bool thesame = false;
		//					bool equal = (Ans[i].eqCounts.Count == Ans[i + 1].eqCounts.Count) && new HashSet<int>(Ans[i].eqCounts).SetEquals(Ans[i + 1].eqCounts);
		//					if (Ans[i].PepSequence == Ans[i + 1].PepSequence && equal)
		//					{
		//						if (Ans[i].AdductNum == Ans[i + 1].AdductNum && Ans[i].PepModification == Ans[i + 1].PepModification)
		//						{
		//							thesame = true;
		//						}
		//					}
		//					if (!thesame)
		//						CHy.Add(Ans[i]);
		//				}
		//				//Enter elementID and MoleNames into each rows
		//				CHy.Add(Ans[Ans.Count() - 1]);
		//				for (int i = 0; i < CHy.Count(); i++)
		//				{
		//					CHy[i].elementIDs.Clear();
		//					CHy[i].MoleNames.Clear();
		//
		//					if (i == CHy.Count() - 1)
		//					{
		//						CHy[0].elementIDs = elementIDs;
		//						CHy[0].MoleNames = molename;
		//					}
		//				}
		//
		//				//Obtain the Name of the adduct molecules:
		//				generatorData GD = new generatorData();
		//				GD.Modification = AddRep.Split('/');
		//
		//				//Send to generate DataTable
		//				theComhypoOnTab2 = genDT(CHy, GD);
		//
		//			}
		//
		//			//override, if a composition hyposthesis file isn't loaded
		//			private void generateGPCompHypo()
		//			{
		//				List<PPMSD> PP = obtainPP();
		//				List<comphypo> Ans = new List<comphypo>();
		//				for (int i = 0; i < PP.Count; i++)
		//				{
		//					if (PP[i].selected)
		//					{
		//						List<comphypo> Temp = new List<comphypo>();
		//						comphypo temp = new comphypo();
		//						//First line:
		//						temp.compoundCompo = "";
		//						temp.AdductNum = 0;
		//						temp.AddRep = "";
		//						temp.MW = PP[i].Mass;
		//						//columns for glycopeptides
		//						temp.PepModification = PP[i].Modifications;
		//						temp.PepSequence = PP[i].Sequence;
		//						temp.MissedCleavages = PP[i].MissedCleavages;
		//						temp.StartAA = PP[i].StartAA;
		//						temp.EndAA = PP[i].EndAA;
		//						temp.numGly = 0;
		//						Ans.Add(temp);
		//					}
		//				}
		//				String composition = "0";
		//				foreach (comphypo ch in Ans)
		//				{
		//					ch.compoundCompo = composition;
		//				}
		//
		//
		//				DataTable DT = genDT(Ans);
		//
		//				dataGridView2.DataSource = DT;
		//				button6.Enabled = true;
		//			}
		//
		//
		//			//#################################################generateHypo class is big and it deserves a block######################################
		//			//This function generates the Composition Hypothesis and print it on dataGridView2.
		//			PleaseWait msgForm = new PleaseWait();
		//			private void generateHypo()
		//			{
		//				BackgroundWorker bw = new BackgroundWorker();
		//				bw.DoWork += new DoWorkEventHandler(bw_DoWork);
		//				bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
		//
		//				//set lable and your waiting text in this form
		//				try
		//				{
		//					bw.RunWorkerAsync();//this will run the DoWork code at background thread
		//					msgForm.ShowDialog();//show the please wait box
		//				}
		//				catch (Exception ex)
		//				{
		//					MessageBox.Show(ex.ToString());
		//				}
		//				//Print the hypothesis to the datagridview.
		//				dataGridView2.DataSource = theComhypoOnTab2;
		//				button6.Enabled = true;
		//			}
		//			void bw_DoWork(object sender, DoWorkEventArgs e)
		//			{
		//				stuffToDo();
		//			}
		//			void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		//			{
		//				if (e.Error != null)
		//				{
		//					MessageBox.Show(Convert.ToString(e.Error));
		//				}
		//				//all background work has completed and we are going to close the waiting message
		//				msgForm.Close();
		//			}
		//			private void stuffToDo()
		//			{
		//				//Obtain Table
		//				generatorData GD = getGenerator();
		//				List<comphypo> UltFinalAns = genHypo(GD);
		//				theComhypoOnTab2 = genDT(UltFinalAns, GD);
		//
		//
		//			}
		public static List<comphypo> genHypo (generatorData GD, bool isGlycan)
		{
			//If there are letters in the lower bounds and upper bounds section, add those rules into the additional table in the GD variable.
			GD = obtainNewGD (GD);

			//Convert all letters in Bounds to numbers, so that we can calcuate them
			List<compTable> Table = ConvertLetters (GD);

			//Now, all bounds in Table only consists of numbers, but they're still not calculated. (e.g. 2+3+4) So, calculate them.
			List<compTable> CoTa0 = Calnumbers (Table);
			//Now, in those bounds, replace them with numbers from all ranges. (ex. if a bound has numbers (2,6,8), replace it with (2,3,4,5,6,7,8) for computations). Afterall, they are bounds.
			CoTa0 = expendBound (CoTa0);
			//Now that all of the bounds are numbers, we can create the composition hypothesis with them, by combining their combinations.
			List<comphypo> Ans = calComHypo (CoTa0);
			//Then limit them with the rules from the additional rules table.
			Ans = checkAddRules (Ans, GD);
			//Check if there is any unwanted rows, which have negative values.
			Ans = checkNegative (Ans);
			//Add the adducts into each row data.
			Ans = addAdducts (Ans, GD);
			//Final Check, if any of the values are negative (ex. a negative amount of S or C), remove that row.
			Ans = checkNegative (Ans);
			//Lastly transfer data from eqCount to molnames and eqCounts, to decrease number of columns needed
			Ans = cleanColumns (Ans, GD);
			//Really, this is the last step. Lastly, remove columns of all zero element counts.
			Ans = removeZeroElements (Ans);

			//If "Is is a Glycan?" box is checked, add one water to add compounds automatically.
			if (isGlycan)
				Ans = addWater (Ans);
			return Ans;
		}


		public static List<comphypo> GenerateGlycopeptideHypothesis (String glycanHypothesisPath, List<PPMSD> proteinDigest)
		{
			List<comphypo> hypothesis = GetCompHypo (glycanHypothesisPath);
			List<string> elementIDs = new List<string> ();
			List<string> molename = new List<string> ();
			for (int j = 0; j < hypothesis.Count (); j++) {
				if (hypothesis [j].elementIDs.Count > 0) {
					for (int i = 0; i < hypothesis [j].elementIDs.Count (); i++) {
						elementIDs.Add (hypothesis [j].elementIDs [i]);
					}
					for (int i = 0; i < hypothesis [j].MoleNames.Count (); i++) {
						molename.Add (hypothesis [j].MoleNames [i]);
					}
					break;
				}
			}

			String AddRep = hypothesis [0].AddRep;

			int indexH = 0;
			int indexO = 0;
			int indexWater = 0;
			try {
				indexH = elementIDs.IndexOf ("H");
				indexO = elementIDs.IndexOf ("O");
				indexWater = molename.IndexOf ("Water");
			} catch {
				throw new CompositionHypothesisException ("Your composition hypothesis contains a compound without Water. Job terminated.");
			}
			List<comphypo> Ans = new List<comphypo> ();
			//Ans.AddRange(CHy);
			PeriodicTable PT = new PeriodicTable ();
			for (int i = 0; i < proteinDigest.Count; i++) {
				if (proteinDigest [i].selected) {
					Int32 Count = Convert.ToInt32 (proteinDigest [i].numGly);
					List<comphypo> Temp = new List<comphypo> ();
					comphypo temp = new comphypo ();
					//First line:
					temp.compoundCompo = "";
					temp.AdductNum = 0;
					temp.AddRep = "";
					temp.MW = proteinDigest [i].Mass;
					for (int s = 0; s < hypothesis [0].eqCounts.Count; s++) {
						temp.eqCounts.Add (0);
					}
					for (int s = 0; s < hypothesis [0].elementAmount.Count; s++) {
						temp.elementAmount.Add (0);
					}
					//columns for glycopeptides
					temp.PepModification = proteinDigest [i].Modifications;
					temp.PepSequence = proteinDigest [i].Sequence;
					temp.MissedCleavages = proteinDigest [i].MissedCleavages;
					temp.StartAA = proteinDigest [i].StartAA;
					temp.EndAA = proteinDigest [i].EndAA;
					temp.numGly = 0;
					Temp.Add (temp);
					for (int j = 0; j < Count; j++) {
						List<comphypo> Temp2 = new List<comphypo> ();
						for (int k = 0; k < Temp.Count (); k++) {
							//need to reread the file and get new reference, because c# keeps passing by reference which creates a problem.
							List<comphypo> CH = GetCompHypo (glycanHypothesisPath);
							for (int fragIter = 0; fragIter < CH.Count (); fragIter++) {
								comphypo temp2 = new comphypo ();
								temp2 = CH [fragIter];
								temp2.numGly = Temp [k].numGly + 1;
								temp2.PepModification = Temp [k].PepModification;
								temp2.PepSequence = Temp [k].PepSequence;
								temp2.MissedCleavages = Temp [k].MissedCleavages;
								temp2.StartAA = Temp [k].StartAA;
								temp2.EndAA = Temp [k].EndAA;
								List<string> forsorting = new List<string> ();
								forsorting.Add (Temp [k].compoundCompo);
								forsorting.Add (temp2.compoundCompo);
								//Utils.Debug (forsorting.Count);
								forsorting = forsorting.OrderBy (a => a).ToList ();
								temp2.compoundCompo = forsorting [0] + forsorting [1];
								temp2.AdductNum = temp2.AdductNum + Temp [k].AdductNum;
								for (int s = 0; s < temp2.eqCounts.Count; s++) {
									temp2.eqCounts [s] = temp2.eqCounts [s] + Temp [k].eqCounts [s];
								}
								for (int s = 0; s < temp2.elementAmount.Count; s++) {
									temp2.elementAmount [s] = temp2.elementAmount [s] + Temp [k].elementAmount [s];
								}
								for (int ui = 0; ui < molename.Count (); ui++) {
									if (molename [ui] == "Water") {
										if (temp2.eqCounts [ui] > 0) {
											temp2.eqCounts [ui] = temp2.eqCounts [ui] - 1;
										}
										break;
									}
								}
								temp2.elementAmount [indexH] = temp2.elementAmount [indexH] - 2;
								temp2.elementAmount [indexO] = temp2.elementAmount [indexO] - 1;
								if (temp2.elementAmount [indexO] < 0)
									temp2.elementAmount [indexO] = 0;
								if (temp2.elementAmount [indexH] < 0)
									temp2.elementAmount [indexH] = 0;
								temp2.MW = temp2.MW + Temp [k].MW - PT.getMass ("H") * 2 - PT.getMass ("O");
								Temp2.Add (temp2);
							}
						}
						Temp.AddRange (Temp2);
					}
					Ans.AddRange (Temp);
				}
			}
			Utils.Debug ("Removing Duplicates");
			//Remove Duplicates from CHy
			Ans = Ans.OrderBy (a => a.MW).ToList ();
			hypothesis.Clear ();
			for (int i = 0; i < Ans.Count () - 1; i++) {
				bool thesame = false;
				bool equal = (Ans [i].eqCounts.Count == Ans [i + 1].eqCounts.Count) && new HashSet<int> (Ans [i].eqCounts).SetEquals (Ans [i + 1].eqCounts);
				if (Ans [i].PepSequence == Ans [i + 1].PepSequence && equal) {
					if (Ans [i].AdductNum == Ans [i + 1].AdductNum && Ans [i].PepModification == Ans [i + 1].PepModification) {
						thesame = true;
					}
				}
				if (!thesame)
					hypothesis.Add (Ans [i]);
			}
			//Enter elementID and MoleNames into each rows
			Utils.Debug ("Debugging Ans Out of Range Exception", Ans.Count (), Ans.Count);
			hypothesis.Add (Ans [Ans.Count () - 1]);
			for (int i = 0; i < hypothesis.Count (); i++) {
				hypothesis [i].elementIDs.Clear ();
				hypothesis [i].MoleNames.Clear ();

				if (i == hypothesis.Count () - 1) {
					hypothesis [0].elementIDs = elementIDs;
					hypothesis [0].MoleNames = molename;
				}
			}

			//Obtain the Name of the adduct molecules:           
//			generatorData GD = new generatorData ();
//			GD.Modification = AddRep.Split ('/');
			return hypothesis;
		}

		public static int[] ParseGlycoSiteFile (String filePath)
		{
			List<int> sitesList = new List<int> ();
			StreamReader fs = new StreamReader (new FileStream (filePath, FileMode.Open, FileAccess.Read));
			string line = null;
			while ((line = fs.ReadLine ()) != null) {
				sitesList.Add (Convert.ToInt32 (line));
			}

			return sitesList.ToArray ();
		}

		//genHypo runs these following functions one by one to do its task://///////////////////////////////////////////////////////
		private static generatorData obtainNewGD (generatorData GD)
		{
			List<compTable> Table = new List<compTable> ();
			Table.AddRange (GD.comTable);
			Boolean hasLetter = false;
			for (int i = 0; i < Table.Count (); i++) {
				String bound = Table [i].Bound [0];
				foreach (char j in bound) {
					if (char.IsUpper (j)) {
						hasLetter = true;
					}
				}
				if (hasLetter == true) {
					hasLetter = false;
					arTable AT = new arTable ();
					AT.Constraint = Table [i].Letter;
					AT.Relationship = "≤";
					AT.Formula = bound;
					GD.aTable.Add (AT);
				}
				String bound2 = Table [i].Bound [1];
				foreach (char j in bound2) {
					if (char.IsUpper (j)) {
						hasLetter = true;
					}
				}
				if (hasLetter == true) {
					hasLetter = false;
					arTable AT = new arTable ();
					AT.Constraint = Table [i].Letter;
					AT.Relationship = "≥";
					AT.Formula = bound2;
					GD.aTable.Add (AT);
				}

			}
			return GD;

		}

		public static List<compTable> ConvertLetters (generatorData tGD)
		{
			generatorData GD = new generatorData ();
			GD = tGD;
			List<compTable> Table = new List<compTable> ();
			Table.AddRange (GD.comTable);
			Boolean MoreLetters = true;
			//Use the cleanbound function to clean up all letters in the bounds.
			while (MoreLetters) {
				MoreLetters = false;
				for (int i = 0; i < Table.Count; i++) {
					Table [i].Bound = cleanBounds (Table [i].Bound, tGD);
				}
				for (int i = 0; i < Table.Count; i++) {
					foreach (String bound in Table[i].Bound) {
						foreach (char j in bound) {
							if (char.IsUpper (j)) {
								MoreLetters = true;
							}
						}
					}
				}
			}
			return Table;
		}

		public static List<compTable> Calnumbers (List<compTable> Table)
		{
			List<compTable> CoTa0 = new List<compTable> ();
			String oneBound = "";
			foreach (compTable j in Table) {
				List<String> CoTatwo = new List<String> ();
				foreach (String bound in j.Bound) {
					oneBound = "";
					MathParserTK Pa = new MathParserTK ();
					oneBound = Convert.ToString (Pa.Parse (bound, false));
					CoTatwo.Add (oneBound);
				}
				j.Bound.Clear ();
				//While we're at it, we'll remove the duplicate bounds also.
				j.Bound.AddRange (CoTatwo.Distinct ().ToList ());
			}
			CoTa0 = Table;
			return CoTa0;
		}

		private static List<compTable> expendBound (List<compTable> CoTa0)
		{
			foreach (compTable j in CoTa0) {
				Int32 LBound = Convert.ToInt32 (j.Bound [0]);
				Int32 UBound = Convert.ToInt32 (j.Bound [0]);
				List<String> CoTatwo = new List<String> ();
				foreach (String bound in j.Bound) {
					if (Convert.ToInt32 (bound) < LBound)
						LBound = Convert.ToInt32 (bound);
					if (Convert.ToInt32 (bound) > UBound)
						UBound = Convert.ToInt32 (bound);
				}
				for (Int32 i = LBound; i <= UBound; i++) {
					CoTatwo.Add (Convert.ToString (i));
				}
				j.Bound.Clear ();
				//While we're at it, we'll remove the duplicate bounds also.
				j.Bound.AddRange (CoTatwo.Distinct ().ToList ());
			}
			return CoTa0;
		}

		private static List<comphypo> calComHypo (List<compTable> CoTa)
		{
			List<comphypo> Ans = new List<comphypo> ();
			List<string> elementIDs = new List<string> ();
			for (int j = 0; j < CoTa.Count (); j++) {
				if (CoTa [j].elementIDs.Count > 0) {
					for (int i = 0; i < CoTa [j].elementIDs.Count (); i++) {
						elementIDs.Add (CoTa [j].elementIDs [i]);
					}
					break;
				}
			}


			Double MW = new Double ();
			foreach (compTable j in CoTa) {
				List<comphypo> tempAns = new List<comphypo> ();
				foreach (String k in j.Bound) {
					Int32 boundNumber = Convert.ToInt32 (k);
					//Append this molecule to the other compositions.
					if (Ans.Count != 0) {
						for (int l = 0; l < Ans.Count; l++) {
							comphypo comphypoAns = new comphypo ();
							foreach (var item in Ans[l].eqCount) {
								comphypoAns.eqCount.Add (item.Key, item.Value);
							}
							comphypoAns.eqCount [j.Letter] = boundNumber;                            
							for (int sh = 0; sh < Ans [l].elementAmount.Count (); sh++) {
								comphypoAns.elementAmount.Add (boundNumber * j.elementAmount [sh] + Ans [l].elementAmount [sh]);
							}
							MW = GetcompMass (j, elementIDs) * boundNumber;
							comphypoAns.MW = MW + Ans [l].MW;
							tempAns.Add (comphypoAns);
						}
					} else {
						comphypo anothercomphypoAns = new comphypo ();
						anothercomphypoAns.eqCount.Add ("A", 0);
						anothercomphypoAns.eqCount.Add ("B", 0);
						anothercomphypoAns.eqCount.Add ("C", 0);
						anothercomphypoAns.eqCount.Add ("D", 0);
						anothercomphypoAns.eqCount.Add ("E", 0);
						anothercomphypoAns.eqCount.Add ("F", 0);
						anothercomphypoAns.eqCount.Add ("G", 0);
						anothercomphypoAns.eqCount.Add ("H", 0);
						anothercomphypoAns.eqCount.Add ("I", 0);
						anothercomphypoAns.eqCount.Add ("J", 0);
						anothercomphypoAns.eqCount.Add ("K", 0);
						anothercomphypoAns.eqCount.Add ("L", 0);
						anothercomphypoAns.eqCount.Add ("M", 0);
						anothercomphypoAns.eqCount.Add ("N", 0);
						anothercomphypoAns.eqCount.Add ("O", 0);
						anothercomphypoAns.eqCount.Add ("P", 0);
						anothercomphypoAns.eqCount.Add ("Q", 0);
						//Add this molecule to the list
						anothercomphypoAns.eqCount [j.Letter] = boundNumber;
						for (int sh = 0; sh < j.elementAmount.Count (); sh++) {
							anothercomphypoAns.elementAmount.Add (boundNumber * j.elementAmount [sh]);
						}
						MW = boundNumber * GetcompMass (j, elementIDs);
						anothercomphypoAns.MW = MW;
						tempAns.Add (anothercomphypoAns);
					}

				}
				Ans.Clear ();
				Ans.AddRange (tempAns);
			}
			//Lastly, remove the repeated rows
			Ans = Ans.OrderByDescending (a => a.MW).ToList ();
			List<comphypo> Answer = new List<comphypo> ();
			int startrow = 1;
			int endrow = 3;
			if (Ans.Count () > endrow)
				endrow = Ans.Count ();
			Boolean OK = true;
			for (int i = 0; i < (Ans.Count ()) - 1; i++) {
				startrow = i + 1;

				endrow = startrow + 3;
				if (Ans.Count () < endrow)
					endrow = Ans.Count ();
				for (int j = startrow; j < endrow; j++) {
					if (Ans [i].eqCount.SequenceEqual (Ans [j].eqCount)) {
						OK = false;
						continue;
					}
				}
				if (OK)
					Answer.Add (Ans [i]);
				OK = true;
			}
			Answer.Add (Ans [Ans.Count () - 1]);
			for (int i = 0; i < Answer.Count (); i++) {
				Answer [i].elementIDs.Clear ();

				if (i == Answer.Count () - 1)
					Answer [0].elementIDs = elementIDs;
			}
			return Answer;
		}

		private static List<comphypo> checkAddRules (List<comphypo> Ans, generatorData GD)
		{
			List<comphypo> CHy = new List<comphypo> ();
			List<string> elementIDs = new List<string> ();
			for (int j = 0; j < Ans.Count (); j++) {
				if (Ans [j].elementIDs.Count > 0) {
					for (int i = 0; i < Ans [j].elementIDs.Count (); i++) {
						elementIDs.Add (Ans [j].elementIDs [i]);
					}
					break;
				}
			}

			for (int i = 0; i < CHy.Count (); i++) {
				CHy [i].elementIDs.Clear ();
				CHy [i].MoleNames.Clear ();

				if (i == CHy.Count () - 1) {
					CHy [0].elementIDs = elementIDs;
				}
			}
			Boolean RowGood = true;
			try {
				if (GD.aTable.Count != 0) {
					for (int j = 0; j < Ans.Count; j++) {
						for (int i = 0; i < GD.aTable.Count (); i++) {
							//MessageBox.Show(Convert.ToString(i));
							String equation = translet (Ans [j], GD.aTable [i]);
							String Relationship = GD.aTable [i].Relationship;
							String Constraint = GD.aTable [i].Constraint;
							Constraint = ConverConstraints (Ans [j], Constraint);
							//Five relationships
							//First one
							if (Relationship == "=") {
								MathParserTK parser = new MathParserTK ();
								Double solution = parser.Parse (equation, false);
								//only 3 situation exists
								//Equals to Even
								if (Constraint == "even") {
									if (solution % 2 != 0) {
										RowGood = false;
									}
									continue;
								}
								//Equals to Odd
								if (Constraint == "odd") {
									if (solution % 2 != 1) {
										RowGood = false;
									}
									continue;
								}
								//Equals to a number
								MathParserTK parser2 = new MathParserTK ();
								Double solution2 = parser2.Parse (Constraint, false);
								if (solution != solution2) {
									RowGood = false;
								}
								continue;
							}
								//Second one 
								else if (Relationship == ">") {
								MathParserTK parser = new MathParserTK ();
								Double solution = parser.Parse (equation, false);
								//only 1 situation exists
								//Equals to a number
								MathParserTK parser2 = new MathParserTK ();
								Double solution2 = parser2.Parse (Constraint, false);
								if (solution <= solution2) {
									RowGood = false;
								}
								continue;
							}
								//Third one
								else if (Relationship == "<") {
								MathParserTK parser = new MathParserTK ();
								Double solution = parser.Parse (equation, false);
								//only 1 situation exists
								//Equals to a number
								MathParserTK parser2 = new MathParserTK ();
								Double solution2 = parser2.Parse (Constraint, false);
								if (solution >= solution2) {
									RowGood = false;
								}
								continue;
							}
								//Forth one
								else if (Relationship == "≥") {
								MathParserTK parser = new MathParserTK ();
								Double solution = parser.Parse (equation, false);
								//only 1 situation exists
								//Equals to a number
								MathParserTK parser2 = new MathParserTK ();
								Double solution2 = parser2.Parse (Constraint, false);
								if (solution < solution2) {
									RowGood = false;
								}
								continue;
							}
								//Fifth one
								else if (Relationship == "≤") {
								MathParserTK parser = new MathParserTK ();
								Double solution = parser.Parse (equation, false);
								//only 1 situation exists
								//Equals to a number
								MathParserTK parser2 = new MathParserTK ();
								Double solution2 = parser2.Parse (Constraint, false);
								if (solution > solution2) {
									RowGood = false;
								}
								continue;
							}
								//Last one
								else if (Relationship == "≠") {
								MathParserTK parser = new MathParserTK ();
								Double solution = parser.Parse (equation, false);
								//only 1 situation exists
								//Equals to a number
								MathParserTK parser2 = new MathParserTK ();
								Double solution2 = parser2.Parse (Constraint, false);
								if (solution == solution2) {
									RowGood = false;
								}
								continue;
							}
						}
						if (RowGood)
							CHy.Add (Ans [j]);
						else
							RowGood = true;
					}
				} else {
					CHy.AddRange (Ans);
				}
			} catch (Exception ex) {
				Console.WriteLine ("Error in Additional Rule Table. Please refer to the help section and check your input. Error Code:" + ex);
				throw(ex);
			}

			for (int i = 0; i < CHy.Count (); i++) {
				CHy [i].elementIDs.Clear ();
				CHy [i].MoleNames.Clear ();

				if (i == CHy.Count () - 1) {
					CHy [0].elementIDs = elementIDs;
				}
			}
			return CHy;
		}

		public static String ConverConstraints (comphypo CH, String Constraint)
		{
			String newConstraint = "";
			//Use the cleanbound function to clean up all letters in the bounds.
			foreach (char i in Constraint) {
				if (char.IsUpper (i)) {
					newConstraint = newConstraint + "(" + CH.eqCount [Convert.ToString (i)] + ")";
				} else {
					newConstraint = newConstraint + Convert.ToString (i);
				}
			}
			return newConstraint;
		}

		private static List<comphypo> addAdducts (List<comphypo> CHy, generatorData GD)
		{
			List<string> elementIDs = new List<string> ();
			List<string> molname = new List<string> ();
			for (int j = 0; j < CHy.Count (); j++) {
				if (CHy [j].elementIDs.Count > 0) {
					for (int i = 0; i < CHy [j].elementIDs.Count (); i++) {
						elementIDs.Add (CHy [j].elementIDs [i]);
					}
					for (int i = 0; i < CHy [j].MoleNames.Count (); i++) {
						molname.Add (CHy [j].MoleNames [i]);
					}
					break;
				}
			}

			Double adductMas = adductMass (GD);
			Int32 adductLB = new Int32 ();
			Int32 adductUB = new Int32 ();
			try {
				adductLB = Convert.ToInt32 (GD.Modification [2]);
				adductUB = Convert.ToInt32 (GD.Modification [3]);
			} catch (Exception ex) {
				Console.WriteLine ("Lower bound and Upper bound in the modification list must be integers. Error:" + ex);
				throw(ex);
			}
			adductcomp adc = getAdductCompo (GD);

			//update elementID list
			for (int i = 0; i < adc.elementIDs.Count (); i++) {
				if (!(elementIDs.Any (a => a.Contains (adc.elementIDs [i])))) {
					elementIDs.Add (adc.elementIDs [i]);
					foreach (comphypo CH in CHy) {
						CH.elementAmount.Add (0);
					}
				}
			}

			List<comphypo> supFinalAns = new List<comphypo> ();
			for (int i = 0; i < CHy.Count (); i++) {
				if (adductLB != 0) {
					comphypo temp = new comphypo ();
					temp.elementAmount = CHy [i].elementAmount;
					temp.AdductNum = 0;
					temp.eqCount = CHy [i].eqCount;
					temp.MW = CHy [i].MW;
					supFinalAns.Add (temp);
				}

				for (int j = adductLB; j <= adductUB; j++) {
					comphypo temp = new comphypo ();
					for (int k = 0; k < CHy [i].elementAmount.Count (); k++) {
						temp.elementAmount.Add (CHy [i].elementAmount [k]);
					}
					for (int l = 0; l < adc.elementAmount.Count (); l++) {
						temp.elementAmount [elementIDs.IndexOf (adc.elementIDs [l])] = CHy [i].elementAmount [elementIDs.IndexOf (adc.elementIDs [l])] + j * adc.elementAmount [l];
					}
					temp.AdductNum = j;
					temp.eqCount = CHy [i].eqCount;
					temp.MW = CHy [i].MW + j * adductMas;
					supFinalAns.Add (temp);
				}
			}
			for (int i = 0; i < supFinalAns.Count (); i++) {
				supFinalAns [i].elementIDs.Clear ();
				supFinalAns [i].MoleNames.Clear ();

				if (i == supFinalAns.Count () - 1) {
					supFinalAns [0].elementIDs = elementIDs;
					supFinalAns [0].MoleNames = molname;
				}
			}

			return supFinalAns;
		}

		private static List<comphypo> checkNegative (List<comphypo> supFinalAns)
		{
			List<comphypo> UltFinalAns = new List<comphypo> ();
			for (int i = 0; i < supFinalAns.Count; i++) {
				Boolean hasnegative = false;
				int sumOfElements = 0;
				for (int j = 0; j < supFinalAns [i].elementAmount.Count; j++) {
					sumOfElements = sumOfElements + supFinalAns [i].elementAmount [j];
					if (supFinalAns [i].elementAmount [j] < 0) {
						hasnegative = true;
					}
				}
				//If all add up to zero, well, delete the line also.
				if (sumOfElements == 0)
					hasnegative = true;
				if (hasnegative == false) {
					//Finally, need to initialize the glycopeptide variables to prevent nullreference error;
					supFinalAns [i].numGly = 0;
					supFinalAns [i].PepSequence = "";
					supFinalAns [i].PepModification = "";
					supFinalAns [i].MissedCleavages = 0;
					supFinalAns [i].StartAA = 0;
					supFinalAns [i].EndAA = 0;
					UltFinalAns.Add (supFinalAns [i]);
				}
			}
			return UltFinalAns;
		}

		private static List<comphypo> cleanColumns (List<comphypo> Ans, generatorData GD)
		{
			List<string> molnames = new List<string> ();
			List<int> index = new List<int> ();
			if (Ans.FindIndex (item => item.eqCount ["A"] > 0) >= 0)
				index.Add (0);
			if (Ans.FindIndex (item => item.eqCount ["B"] > 0) >= 0)
				index.Add (1);
			if (Ans.FindIndex (item => item.eqCount ["C"] > 0) >= 0)
				index.Add (2);
			if (Ans.FindIndex (item => item.eqCount ["D"] > 0) >= 0)
				index.Add (3);
			if (Ans.FindIndex (item => item.eqCount ["E"] > 0) >= 0)
				index.Add (4);
			if (Ans.FindIndex (item => item.eqCount ["F"] > 0) >= 0)
				index.Add (5);
			if (Ans.FindIndex (item => item.eqCount ["G"] > 0) >= 0)
				index.Add (6);
			if (Ans.FindIndex (item => item.eqCount ["H"] > 0) >= 0)
				index.Add (7);
			if (Ans.FindIndex (item => item.eqCount ["I"] > 0) >= 0)
				index.Add (8);
			if (Ans.FindIndex (item => item.eqCount ["J"] > 0) >= 0)
				index.Add (9);
			if (Ans.FindIndex (item => item.eqCount ["K"] > 0) >= 0)
				index.Add (10);
			if (Ans.FindIndex (item => item.eqCount ["L"] > 0) >= 0)
				index.Add (11);
			if (Ans.FindIndex (item => item.eqCount ["M"] > 0) >= 0)
				index.Add (12);
			if (Ans.FindIndex (item => item.eqCount ["N"] > 0) >= 0)
				index.Add (13);
			if (Ans.FindIndex (item => item.eqCount ["O"] > 0) >= 0)
				index.Add (14);
			if (Ans.FindIndex (item => item.eqCount ["P"] > 0) >= 0)
				index.Add (15);
			if (Ans.FindIndex (item => item.eqCount ["Q"] > 0) >= 0)
				index.Add (16);
			Dictionary<Int32, String> toLetter = new Dictionary<Int32, String> ();
			toLetter.Add (0, "A");
			toLetter.Add (1, "B");
			toLetter.Add (2, "C");
			toLetter.Add (3, "D");
			toLetter.Add (4, "E");
			toLetter.Add (5, "F");
			toLetter.Add (6, "G");
			toLetter.Add (7, "H");
			toLetter.Add (8, "I");
			toLetter.Add (9, "J");
			toLetter.Add (10, "K");
			toLetter.Add (11, "L");
			toLetter.Add (12, "M");
			toLetter.Add (13, "N");
			toLetter.Add (14, "O");
			toLetter.Add (15, "P");
			toLetter.Add (16, "Q");

			for (int i = 0; i < index.Count (); i++) {
				molnames.Add (GD.comTable [index [i]].Molecule);
			}
			for (int i = 0; i < Ans.Count (); i++) {
				List<int> eqCountstemp = new List<int> ();
				for (int j = 0; j < index.Count (); j++) {
					int value = new int ();
					if (Ans [i].eqCount.TryGetValue (toLetter [index [j]], out value)) {
						eqCountstemp.Add (Convert.ToInt32 (value));
					} else
						eqCountstemp.Add (0);

				}
				Ans [i].eqCounts = eqCountstemp;
			}
			for (int i = 0; i < Ans.Count (); i++) {
				if (Ans [i].elementIDs.Count () > 0) {
					Ans [i].MoleNames = molnames;
					break;
				}
			}
			return Ans;
		}

		private static List<comphypo> addWater (List<comphypo> Ans)
		{
			PeriodicTable PT = new PeriodicTable ();
			List<string> elementIDs = new List<string> ();
			List<string> molname = new List<string> ();
			for (int j = 0; j < Ans.Count (); j++) {
				if (Ans [j].elementIDs.Count > 0) {
					for (int i = 0; i < Ans [j].elementIDs.Count (); i++) {
						elementIDs.Add (Ans [j].elementIDs [i]);
					}
					for (int i = 0; i < Ans [j].MoleNames.Count (); i++) {
						molname.Add (Ans [j].MoleNames [i]);
					}
					break;
				}
			}

			string H = "H";
			string O = "O";
			string Water = "Water";
			if (!elementIDs.Any (H.Contains)) {
				elementIDs.Add (H);
				foreach (comphypo CH in Ans) {
					CH.elementAmount.Add (0);
				}
			}
			if (!elementIDs.Any (O.Contains)) {
				elementIDs.Add (O);
				foreach (comphypo CH in Ans) {
					CH.elementAmount.Add (0);
				}
			}
			if (!molname.Any (Water.Contains)) {
				molname.Add (Water);
				foreach (comphypo CH in Ans) {
					CH.eqCounts.Add (0);
				}
			}
			foreach (comphypo CH in Ans) {

				CH.elementAmount [elementIDs.IndexOf ("H")] = CH.elementAmount [elementIDs.IndexOf ("H")] + 2;
				CH.elementAmount [elementIDs.IndexOf ("O")] = CH.elementAmount [elementIDs.IndexOf ("O")] + 1;

				CH.MW = CH.MW + PT.getMass ("H") * 2 + PT.getMass ("O");
				CH.eqCounts [molname.IndexOf ("Water")] = CH.eqCounts [molname.IndexOf ("Water")] + 1;
			}
			for (int i = 0; i < Ans.Count (); i++) {
				Ans [i].elementIDs.Clear ();
				Ans [i].MoleNames.Clear ();

				if (i == Ans.Count () - 1) {
					Ans [0].elementIDs = elementIDs;
					Ans [0].MoleNames = molname;
				}
			}

			return Ans;
		}

		private static List<comphypo> removeZeroElements (List<comphypo> Ans)
		{
			List<string> elementIDs = new List<string> ();
			List<string> molname = new List<string> ();
			for (int j = 0; j < Ans.Count (); j++) {
				if (Ans [j].elementIDs.Count > 0) {
					for (int i = 0; i < Ans [j].elementIDs.Count (); i++) {
						elementIDs.Add (Ans [j].elementIDs [i]);
					}
					for (int i = 0; i < Ans [j].MoleNames.Count (); i++) {
						molname.Add (Ans [j].MoleNames [i]);
					}
					break;
				}
			}


			List<bool> NonZeroRows = new List<bool> ();
			foreach (var i in Ans[0].elementAmount) {
				NonZeroRows.Add (false);
			}
			foreach (comphypo CH in Ans) {
				for (int i = 0; i < CH.elementAmount.Count (); i++) {
					if (CH.elementAmount [i] != 0) {
						NonZeroRows [i] = true;
					}
				}
			}
			List<int> NonZeroRowsIndex = new List<int> ();
			for (int i = 0; i < NonZeroRows.Count (); i++) {
				if (NonZeroRows [i])
					NonZeroRowsIndex.Add (i);
			}
			for (int j = 0; j < Ans.Count (); j++) {
				List<int> elementAmount = new List<int> ();
				for (int i = 0; i < NonZeroRowsIndex.Count (); i++) {
					elementAmount.Add (Ans [j].elementAmount [NonZeroRowsIndex [i]]);
				}
				Ans [j].elementAmount = elementAmount;                
			}
			List<string> elementIDs2 = new List<string> ();

			for (int i = 0; i < Ans.Count (); i++) {
				Ans [i].elementIDs.Clear ();
				Ans [i].MoleNames.Clear ();

				if (i == Ans.Count () - 1) {
					for (int j = 0; j < NonZeroRowsIndex.Count (); j++) {
						elementIDs2.Add (elementIDs [NonZeroRowsIndex [j]]);
					}
					Ans [0].elementIDs = elementIDs2;
					Ans [0].MoleNames = molname;
				}
			}


			return Ans;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		//This function is used by ConvertLetters to clean the letters in the lower and upper bound areas. It only clean it once, so if you want to clear the area of all letters, put this into a loop.
		private static List<String> cleanBounds (List<String> Bounds, generatorData GD)
		{

			List<String> FinalLS = new List<String> ();
			List<String> Ans = new List<String> ();
			foreach (String bound in Bounds) {
				foreach (char i in bound) {
					if (char.IsUpper (i)) {
						if (FinalLS.Count == 0) {
							foreach (String bou in getBound(i, GD.comTable)) {
								FinalLS.Add ("(" + bou + ")");
							}
						} else {
							List<String> someLS = new List<String> ();
							foreach (String bou in getBound(i, GD.comTable)) {
								for (int j = 0; j < FinalLS.Count; j++) {
									someLS.Add (FinalLS [j] + "(" + bou + ")");
								}
							}
							FinalLS.Clear ();
							FinalLS = someLS;
						}
					} else {
						if (FinalLS.Count == 0) {
							FinalLS.Add (Convert.ToString (i));
						} else {
							List<String> someLS = new List<String> ();
							for (int j = 0; j < FinalLS.Count; j++) {
								someLS.Add (FinalLS [j] + i);
							}
							FinalLS.Clear ();
							FinalLS = someLS;
						}
					}
				}
				Ans.AddRange (FinalLS);
				FinalLS.Clear ();
			}
			return Ans;
		}

		//This function is used by generateHypo to get the mass of a composition in the composition table.
		public static Double GetcompMass (compTable CT, List<string> elementIDs)
		{
			Double Mass = 0;
			PeriodicTable PT = new PeriodicTable ();
			for (int i = 0; i < CT.elementAmount.Count (); i++) {
				Mass = Mass + CT.elementAmount [i] * PT.getMass (elementIDs [i]);
			}
			return Mass;
		}

		//This class helps the generateHypo class by outputting bounds of a molecule by its letter name.
		private static List<String> getBound (char letter, List<compTable> CT)
		{
			List<String> Bounds = new List<String> ();
			foreach (compTable cT in CT) {
				if (Convert.ToChar (cT.Letter) == letter) {
					Bounds = cT.Bound;
					break;
				}
			}
			if (Bounds.Count == 0) {
				Console.WriteLine ("A character is not readable in the Lower Bounds or Upper Bounds. Please check your input.");
				return Bounds;
			}
			return Bounds;
		}

		//This class helps the generateHypo classin the Additional Rules section by translating letters in the artable into numbers.
		private static String translet (comphypo one, arTable ar)
		{
			String Ans = "";
			foreach (char i in ar.Formula) {
				if (char.IsUpper (i)) {
					try {
						Ans = Ans + Convert.ToString (one.eqCount [Convert.ToString (i)]);
					} catch {
						Console.WriteLine ("Invalid letter in the additional rules table.");
					}
				} else {
					Ans = Ans + Convert.ToString (i);
				}
			}
			return Ans;
		}

		//This class calculates delta adduct mass.
		private static Double adductMass (CompositionHypothesis.generatorData GD)
		{
			String adduct = GD.Modification [0];
			String replacement = GD.Modification [1];
			//Regex for the capital letters
			string regexpattern = @"[A-Z]{1}[a-z]?[a-z]?\d?";
			MatchCollection adducts = Regex.Matches (adduct, regexpattern);
			MatchCollection replacements = Regex.Matches (replacement, regexpattern);
			//Regex for the element;
			string elementregexpattern = @"[A-Z]{1}[a-z]?[a-z]?(\(([^)]*)\))?";
			//Regex for the number of the element.
			string numberregexpattern = @"\d+";
			Double adductmass = 0;
			Double replacementmass = 0;
			PeriodicTable pTable = new PeriodicTable ();
			//For each element in adducts, add up their masses.
			foreach (Match add in adducts) {
				String ad = Convert.ToString (add);
				String element = Convert.ToString (Regex.Match (ad, elementregexpattern));
				String snumber = Convert.ToString (Regex.Match (ad, numberregexpattern));
				Int32 number = 0;
				if (snumber == String.Empty) {
					number = 1;
				} else {
					number = Convert.ToInt32 (snumber);
				}
				adductmass = adductmass + number * pTable.getMass (element);
			}
			//For each element in replacements, add up their masses.
			foreach (Match el in replacements) {
				String element = Convert.ToString (Regex.Match (Convert.ToString (el), elementregexpattern));
				String snumber = Convert.ToString (Regex.Match (Convert.ToString (el), numberregexpattern));
				Int32 number = 0;
				if (snumber == String.Empty) {
					number = 1;
				} else {
					number = Convert.ToInt32 (snumber);
				}
				replacementmass = replacementmass + number * pTable.getMass (element);
			}

			//Finally, subtract them and obtain delta mass.
			Double dMass = adductmass - replacementmass;
			return dMass;
		}

		//This class outputs the elemental composition change from the adduct and replacement.
		private static adductcomp getAdductCompo (CompositionHypothesis.generatorData GD)
		{
			String adduct = GD.Modification [0];
			String replacement = GD.Modification [1];
			//Regex for the capital letters
			string regexpattern = @"([A-Z]{1}[a-z]?[a-z]?){1}(\({1}\d+\){1})?\d?";
			MatchCollection adducts = Regex.Matches (adduct, regexpattern);
			MatchCollection replacements = Regex.Matches (replacement, regexpattern);
			//Regex for the element;
			string elementregexpattern = @"([A-Z]{1}[a-z]?[a-z]?){1}(\({1}\d+\){1})?";
			//Regex for the number of the element.
			string numberregexpattern = @"\d+$";

			adductcomp adc = new adductcomp ();

			//For each element in adducts.
			foreach (Match add in adducts) {
				String ad = Convert.ToString (add);
				String element = Convert.ToString (Regex.Match (ad, elementregexpattern));
				String snumber = Convert.ToString (Regex.Match (ad, numberregexpattern));
				Int32 number = 0;
				if (snumber == String.Empty) {
					number = 1;
				} else {
					number = Convert.ToInt32 (snumber);
				}
				try {
					adc.elementAmount [adc.elementIDs.IndexOf (element)] = adc.elementAmount [adc.elementIDs.IndexOf (element)] + number;
				} catch {
					adc.elementIDs.Add (element);
					adc.elementAmount.Add (number);
				}

			}
			//For each element in replacements.
			foreach (Match ele in replacements) {
				String el = Convert.ToString (ele);
				String element = Convert.ToString (Regex.Match (el, elementregexpattern));
				String snumber = Convert.ToString (Regex.Match (el, numberregexpattern));
				Int32 number = 0;
				if (snumber == String.Empty) {
					number = 1;
				} else {
					number = Convert.ToInt32 (snumber);
				}

				try {
					adc.elementAmount [adc.elementIDs.IndexOf (element)] = adc.elementAmount [adc.elementIDs.IndexOf (element)] - number;
				} catch {
					adc.elementIDs.Add (element);
					adc.elementAmount.Add (number);
				}
			}
			//Finally, subtract them and obtain the answer.

			return adc;
		}

		//Accessor Methods:#####################################################################################
		//These are the classes that help store variables from the datagridviews in tag1.
		public class compTable
		{
			public compTable ()
			{
				elementIDs = new List<string> ();
				elementAmount = new List<int> ();
			}

			public String Letter;
			public String Molecule;
			//element ID and amount are used to record the element compositions.
			public List<string> elementIDs;
			public List<int> elementAmount;
			public List<String> Bound;
		}

		public class arTable
		{
			public String Formula;
			public String Relationship;
			public String Constraint;
		}

		public class generatorData
		{
			public List<compTable> comTable;
			public List<arTable> aTable;
			public String[] Modification;
		}

		//This class stores one row of data in the composition hypothesis result.
		public class comphypo
		{
			public comphypo ()
			{
				eqCounts = new List<int> ();
				elementIDs = new List<string> ();
				elementAmount = new List<int> ();
				eqCount = new Dictionary<string,int> ();
				MoleNames = new List<string> ();
				StartAA = 0;
				EndAA = 0;
			}
			//element ID and amount are used to record the element compositions.
			public List<string> elementIDs;
			public List<int> elementAmount;
			public String compoundCompo;
			public Int32 AdductNum;
			public String AddRep;
			//eqCount are the number of each element
			public Dictionary<String, Int32> eqCount;
			//eqCounts are the number of each molecule
			public List<int> eqCounts;
			public Double MW;
			public List<string> MoleNames;
			//columns for glycopeptides
			public String PepModification;
			public String PepSequence;
			public Int32 MissedCleavages;
			public Int32 numGly;
			public Int32 StartAA;
			public Int32 EndAA;
			//For falseDataset
			public Boolean TrueOrFalse;

			public override string ToString ()
			{
				return JsonConvert.SerializeObject (this, Formatting.Indented);
			}
		}

		//This class stores the elemental composition from the adduct.
		public class adductcomp
		{
			public adductcomp ()
			{
				elementIDs = new List<string> ();
				elementAmount = new List<int> ();
			}

			public List<string> elementIDs;
			public List<int> elementAmount;
		}

		public class PPMSD
		{
			public Boolean selected;
			public Int32 number;
			public Double Mass;
			public Int32 Charge;
			public String Modifications;
			public Int32 StartAA;
			public Int32 EndAA;
			public Int32 MissedCleavages;
			public String PreviousAA;
			public String Sequence;
			public String NextAA;
			public Int32 numGly;

			public override string ToString ()
			{
				return JsonConvert.SerializeObject (this, Formatting.Indented);
			}
		}


	}

	class CompositionHypothesisException : Exception
	{
		public CompositionHypothesisException (string str = null)
		{

		}

		public CompositionHypothesisException (string str, Exception inner)
		{

		}
	}
}


