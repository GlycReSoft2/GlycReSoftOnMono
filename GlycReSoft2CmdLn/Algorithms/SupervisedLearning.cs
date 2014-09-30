using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Statistics.Models.Regression.Fitting;


namespace GlycReSoft
{
	public static class SupervisedLearning
	{
		public static List<ResultsGroup>[] Run (String[] FileLinks, 
		                                        List<CompositionHypothesis.comphypo> comhyp, 
		                                        MS1Parameters paradata,
		                                        Feature userFeatureData = null)
		{
			//Initialize storage variables.
			List<ResultsGroup>[] AllFinalResults = new List<ResultsGroup>[Convert.ToInt32 (FileLinks.Count ())];
			Int32 Count = 0;

			//Each data file is treated separately, hence the for loop.
			foreach (String filename in FileLinks) {

				//Perform the First and second grouping, matching and getting data for the features by the Grouping function.
				Double Mas = new Double ();
				Mas = adductMass (comhyp);
				List<ResultsGroup> LRLR = new List<ResultsGroup> ();
				LRLR = Groupings (filename, paradata, Mas, comhyp);

				//Error prevention
				if (LRLR.Count == 1) {
					Utils.Warn ("There is no match between the hypothesis and the data. Unable to generate results from the file:" + filename);
					List<ResultsGroup> FinalResult = LRLR;
					AllFinalResults [Count] = FinalResult;
					Count++;
					continue;
				}

				//##############Logistic Regression####################
				Feature featureData = logisRegression (LRLR);

				//Generate scores.
				AllFinalResults [Count] = Scorings (LRLR, featureData, paradata, userFeatureData);
				Count++;
			}
			return AllFinalResults;
		}

		//This is used by the Features class to evaluate the features
		public static List<ResultsGroup>[] evaluate (String[] FileLinks, 
		                                             List<CompositionHypothesis.comphypo> comhyp, 
		                                             MS1Parameters paradata,
		                                             Feature dfeatureData,
		                                             Feature userFeatureData = null)
		{
			//Initialize storage variables.
			List<ResultsGroup>[] AllFinalResults = new List<ResultsGroup>[Convert.ToInt32 (FileLinks.Count ())];
			Int32 Count = 0;
			//Each data file is treated separately, hence the for loop.
			foreach (String filename in FileLinks) {

				//Perform the First and second grouping, matching and getting data for the features by the Grouping function.
				Double Mas = new Double ();
				Mas = adductMass (comhyp);
				List<ResultsGroup> LRLR = new List<ResultsGroup> ();
				LRLR = Groupings (filename, paradata, Mas, comhyp);

				//Error prevention
				if (LRLR.Count == 1) {
					Utils.Warn ("There is no match between the hypothesis and the data. Unable to generate results from the file:" + filename);
					List<ResultsGroup> FinalResult = LRLR;
					AllFinalResults [Count] = FinalResult;
					Count++;
					continue;
				}

				//##############Logistic Regression####################
				Feature featureData = logisRegression (LRLR);

				//Generate scores.
				AllFinalResults [Count] = Scorings (LRLR, featureData, paradata, userFeatureData);
				Count++;
			}
			return AllFinalResults;
		}

		//This is used by the Feature Class to generate Features
		public static Feature obtainFeatures (String[] FileLinks, 
		                                      List<CompositionHypothesis.comphypo> comhyp,
		                                      MS1Parameters paradata)
		{
			List<Double> Ini = new List<Double> ();
			List<Double> nCS = new List<Double> ();
			List<Double> SD = new List<Double> ();
			List<Double> nMS = new List<Double> ();
			List<Double> tV = new List<Double> ();
			List<Double> EA = new List<Double> ();
			List<Double> CS = new List<Double> ();
			List<Double> NS = new List<Double> ();
			List<Double> SN = new List<Double> ();

			//Each data file is treated separately, hence the for loop.
			foreach (String filename in FileLinks) {

				//Perform the First and second grouping, matching and getting data for the features by the Grouping function.
				Double Mas = new Double ();
				Mas = adductMass (comhyp);
				List<ResultsGroup> LRLR = new List<ResultsGroup> ();
				LRLR = Groupings (filename, paradata, Mas, comhyp);

				//Error prevention
				if (LRLR.Count == 1) {
					Utils.Warn ("There is no match between the hypothesis and the data. Unable to generate results from the file:" + filename);
					continue;
				}

				//##############Logistic Regression####################
				//Perform logistic regression to get the parameters
				Feature featureData = new Feature ();
				featureData = logisRegression (LRLR);
				Ini.Add (featureData.Initial);
				nCS.Add (featureData.numChargeStates);
				SD.Add (featureData.ScanDensity);
				nMS.Add (featureData.numModiStates);
				tV.Add (featureData.totalVolume);
				EA.Add (featureData.ExpectedA);
				CS.Add (featureData.CentroidScan);
				NS.Add (featureData.numOfScan);
				SN.Add (featureData.avgSigNoise);

			}
			// Get the average of all features.
			Feature finalans = new Feature ();
			finalans.Initial = Ini.Average ();
			finalans.numChargeStates = nCS.Average ();
			finalans.ScanDensity = SD.Average ();
			finalans.numModiStates = nMS.Average ();
			finalans.totalVolume = tV.Average ();
			finalans.ExpectedA = EA.Average ();
			finalans.CentroidScan = CS.Average ();
			finalans.numOfScan = NS.Average ();
			finalans.avgSigNoise = SN.Average ();
			return finalans;
		}

		//this "Grouping" function performs the grouping.
		static private List<ResultsGroup> Groupings (String filename, 
		                                             MS1Parameters paradata, Double Mas, 
		                                             List<CompositionHypothesis.comphypo> comhyp)
		{
			List<string> elementIDs = new List<string> ();
			List<string> molename = new List<string> ();
			for (int i = 0; i < comhyp.Count (); i++) {
				if (comhyp [i].elementIDs.Count > 0) {
					for (int j = 0; j < comhyp [i].elementIDs.Count (); j++) {
						elementIDs.Add (comhyp [i].elementIDs [j]);
					}
					for (int j = 0; j < comhyp [i].MoleNames.Count (); j++) {
						molename.Add (comhyp [i].MoleNames [j]);
					}
					break;
				}
			}
			List<DeconRow> sortedDeconData = new List<DeconRow> ();
			sortedDeconData = DeconData.Parse (filename, paradata);
			//First, sort the list descendingly by its abundance.
			sortedDeconData = sortedDeconData.OrderByDescending (a => a.abundance).ToList ();
			//################Second, create a new list to store results from the first grouping.###############
			List<ResultsGroup> fgResults = new List<ResultsGroup> ();
			ResultsGroup GR2 = new ResultsGroup ();
			Int32 currentMaxBin = new Int32 ();
			currentMaxBin = 1;
			GR2.DeconRow = sortedDeconData [0];
			GR2.mostAbundant = true;
			GR2.numOfScan = 1;
			GR2.minScanNum = sortedDeconData [0].scan_num;
			GR2.maxScanNum = sortedDeconData [0].scan_num;
			GR2.ChargeStateList = new List<int> ();
			GR2.ChargeStateList.Add (sortedDeconData [0].charge);
			GR2.avgSigNoiseList = new List<Double> ();
			GR2.avgSigNoiseList.Add (sortedDeconData [0].signal_noise);
			GR2.avgAA2List = new List<double> ();
			GR2.avgAA2List.Add (sortedDeconData [0].mono_abundance / (sortedDeconData [0].mono_plus2_abundance + 1));
			GR2.scanNumList = new List<Int32> ();
			GR2.scanNumList.Add (sortedDeconData [0].scan_num);
			GR2.numModiStates = 1;
			GR2.totalVolume = sortedDeconData [0].abundance * sortedDeconData [0].fwhm;
			GR2.listAbun = new List<double> ();
			GR2.listAbun.Add (sortedDeconData [0].abundance);
			GR2.listMonoMW = new List<double> ();
			GR2.listMonoMW.Add (sortedDeconData [0].monoisotopic_mw);
			fgResults.Add (GR2);

			for (int j = 1; j < sortedDeconData.Count; j++) {
				for (int i = 0; i < fgResults.Count; i++) {
					//Obtain grouping error. Note: its in ppm, so it needs to be multiplied by 0.000001.
					Double GroupingError = fgResults [i].DeconRow.monoisotopic_mw * paradata.groupingErrorEG * 0.000001;
					if ((sortedDeconData [j].monoisotopic_mw < (fgResults [i].DeconRow.monoisotopic_mw + GroupingError) && (sortedDeconData [j].monoisotopic_mw > (fgResults [i].DeconRow.monoisotopic_mw - GroupingError)))) {
						if (fgResults [i].maxScanNum < sortedDeconData [j].scan_num) {
							fgResults [i].maxScanNum = sortedDeconData [j].scan_num;
						} else if (fgResults [i].minScanNum > sortedDeconData [j].scan_num) {
							fgResults [i].minScanNum = sortedDeconData [j].scan_num;
						}
						fgResults [i].numOfScan = fgResults [i].numOfScan + 1;
						fgResults [i].scanNumList.Add (sortedDeconData [j].scan_num);
						fgResults [i].totalVolume = fgResults [i].totalVolume + sortedDeconData [j].abundance * sortedDeconData [j].fwhm;
						fgResults [i].ChargeStateList.Add (sortedDeconData [j].charge);
						fgResults [i].avgSigNoiseList.Add (sortedDeconData [j].signal_noise);
						fgResults [i].avgAA2List.Add (sortedDeconData [j].mono_abundance / (sortedDeconData [j].mono_plus2_abundance + 1));
						fgResults [i].listAbun.Add (sortedDeconData [j].abundance);
						fgResults [i].listMonoMW.Add (sortedDeconData [j].monoisotopic_mw);
						break;
					}

					if (i == fgResults.Count - 1) {
						ResultsGroup GR = new ResultsGroup ();
						currentMaxBin = currentMaxBin + 1;
						GR.DeconRow = sortedDeconData [j];
						GR.mostAbundant = true;
						GR.numOfScan = 1;
						GR.minScanNum = sortedDeconData [j].scan_num;
						GR.maxScanNum = sortedDeconData [j].scan_num;
						GR.ChargeStateList = new List<int> ();
						GR.ChargeStateList.Add (sortedDeconData [j].charge);
						GR.avgSigNoiseList = new List<Double> ();
						GR.avgSigNoiseList.Add (sortedDeconData [j].signal_noise);
						GR.avgAA2List = new List<double> ();
						GR.avgAA2List.Add (sortedDeconData [j].mono_abundance / (sortedDeconData [j].mono_plus2_abundance + 1));
						GR.scanNumList = new List<int> ();
						GR.scanNumList.Add (sortedDeconData [j].scan_num);
						GR.numModiStates = 1;
						GR.totalVolume = sortedDeconData [j].abundance * sortedDeconData [j].fwhm;
						GR.listAbun = new List<double> ();
						GR.listAbun.Add (sortedDeconData [j].abundance);
						GR.listMonoMW = new List<double> ();
						GR.listMonoMW.Add (sortedDeconData [j].monoisotopic_mw);
						fgResults.Add (GR);
					}
				}
			}
			//Lastly calculate the Average Weighted Abundance
			for (int y = 0; y < fgResults.Count (); y++) {
				Double sumofTopPart = 0;
				for (int z = 0; z < fgResults [y].listMonoMW.Count (); z++) {
					sumofTopPart = sumofTopPart + fgResults [y].listMonoMW [z] * fgResults [y].listAbun [z];
				}
				fgResults [y].DeconRow.monoisotopic_mw = sumofTopPart / fgResults [y].listAbun.Sum ();
			}

			//######################## Here is the second grouping. ################################
			fgResults = fgResults.OrderBy (o => o.DeconRow.monoisotopic_mw).ToList ();
			if (Mas != 0) {
				for (int i = 0; i < fgResults.Count - 1; i++) {
					if (fgResults [i].mostAbundant == true) {
						int numModStates = 1;
						for (int j = i + 1; j < fgResults.Count; j++) {
							Double AdductTolerance = fgResults [i].DeconRow.monoisotopic_mw * paradata.adductToleranceEA * 0.000001;
							if ((fgResults [i].DeconRow.monoisotopic_mw >= (fgResults [j].DeconRow.monoisotopic_mw - Mas * numModStates - AdductTolerance)) && (fgResults [i].DeconRow.monoisotopic_mw <= (fgResults [j].DeconRow.monoisotopic_mw - Mas * numModStates + AdductTolerance))) {
								//obtain max and min scan number
								if (fgResults [i].maxScanNum < fgResults [j].maxScanNum) {
									fgResults [i].maxScanNum = fgResults [j].maxScanNum;
								} else {
									fgResults [i].maxScanNum = fgResults [i].maxScanNum;
								}

								if (fgResults [i].minScanNum > fgResults [j].minScanNum) {
									fgResults [i].minScanNum = fgResults [j].minScanNum;
								} else {
									fgResults [i].minScanNum = fgResults [i].minScanNum;
								}
								//numOfScan
								fgResults [i].numOfScan = fgResults [i].numOfScan + fgResults [j].numOfScan;
								fgResults [i].scanNumList.AddRange (fgResults [j].scanNumList);
								//ChargeStateList
								for (int h = 0; h < fgResults [j].ChargeStateList.Count; h++) {
									fgResults [i].ChargeStateList.Add (fgResults [j].ChargeStateList [h]);
								}
								//avgSigNoiseList
								for (int h = 0; h < fgResults [j].avgSigNoiseList.Count; h++) {
									fgResults [i].avgSigNoiseList.Add (fgResults [j].avgSigNoiseList [h]);
								}
								//avgAA2List
								for (int h = 0; h < fgResults [j].avgAA2List.Count; h++) {
									fgResults [i].avgAA2List.Add (fgResults [j].avgAA2List [h]);
								}
								//numModiStates
								numModStates++;
								fgResults [i].numModiStates = fgResults [i].numModiStates + 1;
								fgResults [j].mostAbundant = false;
								//TotalVolume
								fgResults [i].totalVolume = fgResults [i].totalVolume + fgResults [j].totalVolume;
								if (fgResults [i].DeconRow.abundance < fgResults [j].DeconRow.abundance) {
									fgResults [i].DeconRow = fgResults [j].DeconRow;
									numModStates = 1;
								}
							} else if (fgResults [i].DeconRow.monoisotopic_mw < (fgResults [j].DeconRow.monoisotopic_mw - (Mas + AdductTolerance * 2) * numModStates)) {
								//save running time. Since the list is sorted, any other mass below won't match as an adduct.
								break;
							}
						}
					}
				}
			} else {
				for (int i = 0; i < fgResults.Count; i++) {
					fgResults [i].numModiStates = 0;
				}
			}
			List<ResultsGroup> sgResults = new List<ResultsGroup> ();
			//Implement the scan number threshold
			fgResults = fgResults.OrderByDescending (a => a.numOfScan).ToList ();
			Int32 scanCutOff = fgResults.Count () + 1;
			for (int t = 0; t < fgResults.Count (); t++) {
				if (fgResults [t].numOfScan < paradata.minScanNumber) {
					scanCutOff = t;
					break;
				}
			}
			if (scanCutOff != fgResults.Count () + 1) {
				fgResults.RemoveRange (scanCutOff, fgResults.Count () - scanCutOff);
			}

			//############# This is the matching part. It matches the composition hypothesis with the grouped decon data.############
			String[] MolNames = new String[17];

			//These numOfMatches and lists are used to fit the linear regression model for Expect A: A+2. They are put here to decrease the already-int running time.
			Int32 numOfMatches = new Int32 ();
			List<Double> moleWeightforA = new List<Double> ();
			List<Double> AARatio = new List<Double> ();
			//Used to obtain all available bins for centroid scan error.
			//Read the other lines for compTable data.
			fgResults = fgResults.OrderByDescending (a => a.DeconRow.monoisotopic_mw).ToList ();
			comhyp = comhyp.OrderByDescending (b => b.MW).ToList ();
			bool hasMatch = false;
			int lastMatch = 0;
			for (int j = 0; j < fgResults.Count; j++) {
				if (fgResults [j].mostAbundant == true) {
					lastMatch = lastMatch - 4;
					if (lastMatch < 0)
						lastMatch = 0;
					for (int i = lastMatch; i < comhyp.Count; i++) {

						Double MatchingError = comhyp [i].MW * paradata.matchErrorEM * 0.000001;
						if ((fgResults [j].DeconRow.monoisotopic_mw <= (comhyp [i].MW + MatchingError)) && (fgResults [j].DeconRow.monoisotopic_mw >= (comhyp [i].MW - MatchingError))) {
							ResultsGroup GR = new ResultsGroup ();
							GR = matchPassbyValue (fgResults [j], comhyp [i]);
							sgResults.Add (GR);
							//Stuffs for feature
							numOfMatches++;
							moleWeightforA.Add (fgResults [j].DeconRow.monoisotopic_mw);
							AARatio.Add (fgResults [j].avgAA2List.Average ());
							lastMatch = i + 1;
							hasMatch = true;
							continue;
						}
						//Since the data is sorted, there are no more matches below that row, break it.
						if (fgResults [j].DeconRow.monoisotopic_mw > (comhyp [i].MW + MatchingError)) {
							if (hasMatch == false) {
								ResultsGroup GR = new ResultsGroup ();
								CompositionHypothesis.comphypo comhypi = new CompositionHypothesis.comphypo ();
								GR = fgResults [j];
								GR.Match = false;
								GR.comphypo = comhypi;
								sgResults.Add (GR);
								lastMatch = i;
								break;
							} else {
								hasMatch = false;
								break;
							}
						}
					}
				}
			}

			//##############Last part, this is to calculate the feature data needed for logistic regression###################
			//Expected A and Centroid Scan Error need linear regression. The models are built here separately.
			//In the this model. output is the Y axis and input is X.
			SimpleLinearRegression AA2regression = new SimpleLinearRegression ();
			List<double> aainput = new List<double> ();
			List<double> aaoutput = new List<double> ();
			//Centroid Scan Error
			List<double> ccinput = new List<double> ();
			List<double> ccoutput = new List<double> ();
			if (numOfMatches > 3) {
				for (int i = 0; i < sgResults.Count; i++) {
					if (sgResults [i].Match == true) {
						if (sgResults [i].avgAA2List.Average () != 0) {
							aainput.Add (sgResults [i].DeconRow.monoisotopic_mw);
							aaoutput.Add (sgResults [i].avgAA2List.Average ());
						}
						if (sgResults [i].DeconRow.abundance > 250) {
							ccoutput.Add (sgResults [i].DeconRow.scan_num);
							ccinput.Add (sgResults [i].DeconRow.monoisotopic_mw);
						}
					}
				}
			} else {
				for (int i = 0; i < sgResults.Count; i++) {
					if (sgResults [i].avgAA2List.Average () != 0) {
						aainput.Add (sgResults [i].DeconRow.monoisotopic_mw);
						aaoutput.Add (sgResults [i].avgAA2List.Average ());
					}
					if (sgResults [i].DeconRow.abundance > 250) {
						ccoutput.Add (sgResults [i].scanNumList.Average ());
						ccinput.Add (sgResults [i].DeconRow.monoisotopic_mw);
					}
				}
			}
			SimpleLinearRegression CSEregression = new SimpleLinearRegression ();
			CSEregression.Regress (ccinput.ToArray (), ccoutput.ToArray ());
			AA2regression.Regress (aainput.ToArray (), aaoutput.ToArray ());


			//The remaining features and input them into the grouping results
			for (int i = 0; i < sgResults.Count; i++) {
				//ScanDensiy is: Number of scan divided by (max scan number – min scan number)
				Double ScanDensity = new Double ();
				Int32 MaxScanNumber = sgResults [i].maxScanNum;
				Int32 MinScanNumber = sgResults [i].minScanNum;
				Double NumOfScan = sgResults [i].numOfScan;
				List<Int32> numChargeStatesList = sgResults [i].ChargeStateList.Distinct ().ToList ();
				Int32 numChargeStates = numChargeStatesList.Count;
				Double numModiStates = sgResults [i].numModiStates;
				if ((MaxScanNumber - MinScanNumber) != 0)
					ScanDensity = NumOfScan / (MaxScanNumber - MinScanNumber + 15);
				else
					ScanDensity = 0;
				//Use this scandensity for all molecules in this grouping.

				sgResults [i].numChargeStates = numChargeStates;
				sgResults [i].ScanDensity = ScanDensity;
				sgResults [i].numModiStates = numModiStates;
				sgResults [i].CentroidScanLR = CSEregression.Compute (sgResults [i].DeconRow.monoisotopic_mw);
				sgResults [i].CentroidScan = Math.Abs (sgResults [i].scanNumList.Average () - sgResults [i].CentroidScanLR);
				sgResults [i].ExpectedA = Math.Abs (sgResults [i].avgAA2List.Average () - AA2regression.Compute (sgResults [i].DeconRow.monoisotopic_mw));
				sgResults [i].avgSigNoise = sgResults [i].avgSigNoiseList.Average ();
			}
			for (int i = 0; i < sgResults.Count (); i++) {
				sgResults [i].comphypo.elementIDs.Clear ();
				sgResults [i].comphypo.MoleNames.Clear ();

				if (i == sgResults.Count () - 1) {
					sgResults [0].comphypo.elementIDs = elementIDs;
					sgResults [0].comphypo.MoleNames = molename;
				}
			}
			return sgResults;
		}

		//Used by matching part to prevent pass by reference.
		private static ResultsGroup matchPassbyValue (ResultsGroup input1, CompositionHypothesis.comphypo comhypo)
		{
			ResultsGroup storage = new ResultsGroup ();
			//Pass by value, I only way I know we can do this is to pass them one by one. Yes, it is troublesome.
			storage.DeconRow = input1.DeconRow;
			storage.mostAbundant = input1.mostAbundant;
			storage.numChargeStates = input1.numChargeStates;
			storage.ScanDensity = input1.ScanDensity;
			storage.numModiStates = input1.numModiStates;
			storage.totalVolume = input1.totalVolume;
			storage.ExpectedA = input1.ExpectedA;
			storage.CentroidScan = input1.CentroidScan;
			storage.numOfScan = input1.numOfScan;
			storage.avgSigNoise = input1.avgSigNoise;
			storage.maxScanNum = input1.maxScanNum;
			storage.minScanNum = input1.minScanNum;
			storage.scanNumList = input1.scanNumList;
			storage.ChargeStateList = input1.ChargeStateList;
			storage.avgSigNoiseList = input1.avgSigNoiseList;
			storage.CentroidScanLR = input1.CentroidScanLR;
			storage.avgAA2List = input1.avgAA2List;

			storage.comphypo = comhypo;
			storage.Match = true;
			return storage;
		}

		//This class performs logistic regression from data obtained from the Groupings Function
		private static Feature logisRegression (List<ResultsGroup> LRLR)
		{
			int numofMatches = 0;
			//now, put LRLR into a table of arrays so that the regression function can read it.
			Double[][] inputs = new Double[LRLR.Count][];
			Double[] output = new Double[LRLR.Count];
			for (int i = 0; i < LRLR.Count; i++) {
				inputs [i] = new Double[] {
					Convert.ToDouble (LRLR [i].numChargeStates),
					Convert.ToDouble (LRLR [i].ScanDensity),
					Convert.ToDouble (LRLR [i].numModiStates),
					Convert.ToDouble (LRLR [i].totalVolume),
					Convert.ToDouble (LRLR [i].ExpectedA),
					Convert.ToDouble (LRLR [i].CentroidScan),
					Convert.ToDouble (LRLR [i].numOfScan),
					Convert.ToDouble (LRLR [i].avgSigNoise)
				};
				output [i] = Convert.ToDouble (LRLR [i].Match);
				if (LRLR [i].Match == true)
					numofMatches++;
			}

			if (numofMatches < 10) {
				Utils.Warn ("Warning: there are less than 10 matches.");
			}

			//Perform logistic regression to get the parameters
			LogisticRegression regression = new LogisticRegression (inputs: 8);
			var results = new IterativeReweightedLeastSquares (regression);
			double delta = 0;
			do {
				// Perform an iteration
				delta = results.Run (inputs, output);

			} while (delta > 0.001);

			Feature answer = new Feature ();
			//Here are the beta values in logistic regression.
			answer.Initial = regression.Coefficients [0];
			answer.numChargeStates = regression.Coefficients [1];
			answer.ScanDensity = regression.Coefficients [2];
			answer.numModiStates = regression.Coefficients [3];
			answer.totalVolume = regression.Coefficients [4];
			answer.ExpectedA = regression.Coefficients [5];
			answer.CentroidScan = regression.Coefficients [6];
			answer.numOfScan = regression.Coefficients [7];
			answer.avgSigNoise = regression.Coefficients [8];
			return answer;
		}

		private static List<ResultsGroup> balanceMatch (List<ResultsGroup> LRLR)
		{
			List<ResultsGroup> MatchList = new List<ResultsGroup> ();
			List<ResultsGroup> noMatchList = new List<ResultsGroup> ();
			for (int i = 0; i < LRLR.Count; i++) {
				if (LRLR [i].Match == true) {
					MatchList.Add (LRLR [i]);
				} else {
					noMatchList.Add (LRLR [i]);
				}
			}
			//Keep match: unmatch ratio to 1:ratio
			double ratio = 2;



			//for every 1 match, there can at most be 2 non-match, and vice versa
			Double MNMratio = Convert.ToDouble (MatchList.Count ()) / Convert.ToDouble (noMatchList.Count ());
			if (MNMratio <= ratio && MNMratio >= 1 / ratio)
				//the ratio is good, ending the function.
				return LRLR;

			//need less match in the data
			if (MNMratio > ratio) {
				//all nomatch will be in the new data, randomly pick match to fit in. (Reuse the nomatch list to save time and memory)
				//This is the amount of match we will add into LRLR.
				Int32 numneeded = Convert.ToInt32 (noMatchList.Count () * ratio);

				List<int> rangeone = Enumerable.Range (0, MatchList.Count).ToList ();
				rangeone.Shuffle ();

				for (int i = 0; i < numneeded; i++) {
					noMatchList.Add (MatchList [rangeone [i]]);
				}
				return noMatchList;
			}

			//need less no match in the data
			if (MNMratio < 1 / ratio) {
				//all omatch will be in the new data, randomly pick nomatch to fit in. (Reuse the match list to save time and memory)
				Int32 numneeded = Convert.ToInt32 (MatchList.Count () * ratio);
				List<int> rangeone = Enumerable.Range (0, noMatchList.Count).ToList ();
				rangeone.Shuffle ();

				for (int i = 0; i < numneeded; i++) {
					MatchList.Add (noMatchList [rangeone [i]]);
				}
				return MatchList;
			}

			//We should never arrive here, but when it happens....
			return LRLR;
		}
		//balanceMatch function uses this to randomly pick out items in list.
		static Random rnd = new Random ();

		//This runs the linear regression and generate score for the grouping results
		public static List<ResultsGroup> Scorings (List<ResultsGroup> LRLR, 
		                                           Feature candidateFeatureData,
		                                           MS1Parameters paradata,
			//If null, should replace with another copy of default model
		                                           Feature userFeatureData = null
		)
		{
			//Now, load current features from the software, if it doesn't exist, use default features.

			Feature defaultData = Feature.ReadFeature (Feature.REFERENCE_MODEL_FEATURE_PATH);

			if (userFeatureData == null) {
				userFeatureData = Feature.ReadFeature (Feature.REFERENCE_MODEL_FEATURE_PATH);
			}

			Double initial = candidateFeatureData.Initial * 0.9 + userFeatureData.Initial * 0.05 + defaultData.Initial * 0.05;
			Double bnumChargeStates = candidateFeatureData.numChargeStates * 0.9 + userFeatureData.numChargeStates * 0.05 + defaultData.numChargeStates * 0.05;
			Double bScanDensity = candidateFeatureData.ScanDensity * 0.9 + userFeatureData.ScanDensity * 0.05 + defaultData.ScanDensity * 0.05;
			Double bnumModiStates = candidateFeatureData.numModiStates * 0.9 + userFeatureData.numModiStates * 0.05 + defaultData.numModiStates * 0.05;
			Double btotalVolume = candidateFeatureData.totalVolume * 0.9 + userFeatureData.totalVolume * 0.05 + defaultData.totalVolume * 0.05;
			Double bExpectedA = candidateFeatureData.ExpectedA * 0.9 + userFeatureData.totalVolume * 0.05 + defaultData.totalVolume * 0.05;
			Double bCentroid = candidateFeatureData.CentroidScan * 0.9 + userFeatureData.CentroidScan * 0.05 + defaultData.CentroidScan * 0.05;
			Double bnumOfScan = candidateFeatureData.numOfScan * 0.9 + userFeatureData.numOfScan * 0.05 + defaultData.numOfScan * 0.05;
			Double bavgSigNoise = candidateFeatureData.avgSigNoise * 0.9 + userFeatureData.avgSigNoise * 0.05 + defaultData.avgSigNoise * 0.05;

			if (userFeatureData.Initial != defaultData.Initial) {
				//Here are the beta values in logistic regression. 0.75 is from default, 0.25 is from calculation.
				initial = candidateFeatureData.Initial * 0.7 + userFeatureData.Initial * 0.2 + defaultData.Initial * 0.1;
				bnumChargeStates = candidateFeatureData.numChargeStates * 0.7 + userFeatureData.numChargeStates * 0.2 + defaultData.numChargeStates * 0.1;
				bScanDensity = candidateFeatureData.ScanDensity * 0.7 + userFeatureData.ScanDensity * 0.2 + defaultData.ScanDensity * 0.1;
				bnumModiStates = candidateFeatureData.numModiStates * 0.7 + userFeatureData.numModiStates * 0.2 + defaultData.numModiStates * 0.1;
				btotalVolume = candidateFeatureData.totalVolume * 0.7 + userFeatureData.totalVolume * 0.2 + defaultData.totalVolume * 0.1;
				bExpectedA = candidateFeatureData.ExpectedA * 0.7 + userFeatureData.totalVolume * 0.2 + defaultData.totalVolume * 0.1;
				bCentroid = candidateFeatureData.CentroidScan * 0.7 + userFeatureData.CentroidScan * 0.2 + defaultData.CentroidScan * 0.1;
				bnumOfScan = candidateFeatureData.numOfScan * 0.7 + userFeatureData.numOfScan * 0.2 + defaultData.numOfScan * 0.1;
				bavgSigNoise = candidateFeatureData.avgSigNoise * 0.7 + userFeatureData.avgSigNoise * 0.2 + defaultData.avgSigNoise * 0.1;
			}


			Double e = Math.E;
			try {
				//Now calculate the scores for each of them.
				Double scoreInput = new Double ();
				Double Score = new Double ();
				for (int o = 0; o < LRLR.Count; o++) {                    
					scoreInput = (initial + bnumChargeStates * Convert.ToDouble (LRLR [o].numChargeStates) + bScanDensity * Convert.ToDouble (LRLR [o].ScanDensity) + bnumModiStates * Convert.ToDouble (LRLR [o].numModiStates) + btotalVolume * Convert.ToDouble (LRLR [o].totalVolume) + bExpectedA * Convert.ToDouble (LRLR [o].ExpectedA) + bCentroid * Convert.ToDouble (LRLR [o].CentroidScan) + bnumOfScan * Convert.ToDouble (LRLR [o].numOfScan) + bavgSigNoise * Convert.ToDouble (LRLR [o].avgSigNoise));
					Double store = Math.Pow (e, (-1 * scoreInput));
					Score = 1 / (1 + store);
					if (Score >= 0.5) {
						store = Math.Pow (e, (-0.6 * scoreInput));
						Score = (0.8512 / (1 + store)) + 0.1488;
					} else {
						store = Math.Pow (e, (-0.6 * scoreInput - 0.3));
						Score = 1 / (1 + store);
					}

					LRLR [o].Score = Score;
				}
				//Implement score threshold
				LRLR = LRLR.OrderByDescending (a => a.Score).ToList ();



				if (LRLR [0].Score + LRLR [1].Score + LRLR [2].Score > 2.94) {
					scoreInput = (initial + bnumChargeStates * Convert.ToDouble (LRLR [0].numChargeStates) + bScanDensity * Convert.ToDouble (LRLR [0].ScanDensity) + bnumModiStates * Convert.ToDouble (LRLR [0].numModiStates) + btotalVolume * Convert.ToDouble (LRLR [0].totalVolume) + bExpectedA * Convert.ToDouble (LRLR [0].ExpectedA) + bCentroid * Convert.ToDouble (LRLR [0].CentroidScan) + bnumOfScan * Convert.ToDouble (LRLR [0].numOfScan) + bavgSigNoise * Convert.ToDouble (LRLR [0].avgSigNoise));
					scoreInput = scoreInput + (initial + bnumChargeStates * Convert.ToDouble (LRLR [1].numChargeStates) + bScanDensity * Convert.ToDouble (LRLR [1].ScanDensity) + bnumModiStates * Convert.ToDouble (LRLR [1].numModiStates) + btotalVolume * Convert.ToDouble (LRLR [1].totalVolume) + bExpectedA * Convert.ToDouble (LRLR [1].ExpectedA) + bCentroid * Convert.ToDouble (LRLR [1].CentroidScan) + bnumOfScan * Convert.ToDouble (LRLR [1].numOfScan) + bavgSigNoise * Convert.ToDouble (LRLR [1].avgSigNoise));
					scoreInput = scoreInput + (initial + bnumChargeStates * Convert.ToDouble (LRLR [2].numChargeStates) + bScanDensity * Convert.ToDouble (LRLR [2].ScanDensity) + bnumModiStates * Convert.ToDouble (LRLR [2].numModiStates) + btotalVolume * Convert.ToDouble (LRLR [2].totalVolume) + bExpectedA * Convert.ToDouble (LRLR [2].ExpectedA) + bCentroid * Convert.ToDouble (LRLR [2].CentroidScan) + bnumOfScan * Convert.ToDouble (LRLR [2].numOfScan) + bavgSigNoise * Convert.ToDouble (LRLR [2].avgSigNoise));
					scoreInput = scoreInput / 3;
					Double n = -2.9444389791664404600090274318879 / scoreInput;
					for (int o = 0; o < LRLR.Count; o++) {
						if (LRLR [o].Score >= 0.57444251681) {
							scoreInput = (initial + bnumChargeStates * Convert.ToDouble (LRLR [o].numChargeStates) + bScanDensity * Convert.ToDouble (LRLR [o].ScanDensity) + bnumModiStates * Convert.ToDouble (LRLR [o].numModiStates) + btotalVolume * Convert.ToDouble (LRLR [o].totalVolume) + bExpectedA * Convert.ToDouble (LRLR [o].ExpectedA) + bCentroid * Convert.ToDouble (LRLR [o].CentroidScan) + bnumOfScan * Convert.ToDouble (LRLR [o].numOfScan) + bavgSigNoise * Convert.ToDouble (LRLR [o].avgSigNoise));
							Double store = Math.Pow (e, (n * scoreInput));
							Score = (0.8512 / (1 + store)) + 0.1488;
							LRLR [o].Score = Score;
						}
					}
				}



				Int32 scoreCutOff = LRLR.Count () + 1;
				for (int t = 0; t < LRLR.Count (); t++) {
					if (LRLR [t].Score < paradata.minScoreThreshold) {
						scoreCutOff = t;
						break;
					}
				}
				if (scoreCutOff != LRLR.Count () + 1) {
					LRLR.RemoveRange (scoreCutOff, LRLR.Count () - scoreCutOff);
				}
			} catch {
				for (int o = 0; o < LRLR.Count; o++) {
					LRLR [o].Score = 0;
				}
			}

			return LRLR;
		}

		//This class calculates delta adduct mass.
		private static Double adductMass (List<CompositionHypothesis.comphypo> comhyp)
		{
			String adduct = "";
			String replacement = "";
			try {
				String AddRep = comhyp [1].AddRep;
				String[] AddandRep = AddRep.Split ('/');
				adduct = AddandRep [0];
				replacement = AddandRep [1];
			} catch {
				adduct = "0";
				replacement = "0";
			}

			//Regex for the capital letters
			string regexpattern = @"[A-Z]{1}[a-z]?[a-z]?\d?";
			MatchCollection adducts = Regex.Matches (adduct, regexpattern);
			MatchCollection replacements = Regex.Matches (replacement, regexpattern);
			//Regex for the element;
			string elementregexpattern = "[A-Z]{1}[a-z]?[a-z]?";
			//Regex for the number of the element.
			string numberregexpattern = "[0-9]*$";
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
			foreach (String el in replacements) {
				String element = Convert.ToString (Regex.Match (el, elementregexpattern));
				String snumber = Convert.ToString (Regex.Match (el, numberregexpattern));
				Int32 number = 0;
				if (snumber == String.Empty) {
					number = 1;
				} else {
					number = Convert.ToInt32 (Regex.Match (el, numberregexpattern));
				}
				replacementmass = replacementmass + number * pTable.getMass (element);
			}

			//Finally, subtract them and obtain delta mass.
			Double dMass = adductmass - replacementmass;
			return dMass;
		}


	}

}
