using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace GlycReSoft
{
	class GroupingResultsParser
	{
		//This is used to read a ResultFile
		public List<ResultsGroup> readResultFile(String path)
		{
			//This code looks int, but its just repetitive code. Look for ext and you will understand.
			List<ResultsGroup> Ans = new List<ResultsGroup>();
			List<String> molnames = new List<String>();
			FileStream FS = new FileStream(path, FileMode.Open, FileAccess.Read);
			StreamReader read = new StreamReader(FS);
			String ext = Path.GetExtension(path).Replace(".", "");


			if (ext == "csv")
			{
				String header = read.ReadLine();
				String[] headers = header.Split(',');
				List<string> elementIDs = new List<string>();
				//This is another older form of data
				if (headers[5] != "Hypothesis MW")
				{
					Boolean moreCompounds = true;
					int i = 17;
					while (moreCompounds)
					{
						if (headers[i] != "Hypothesis MW")
						{
							elementIDs.Add(headers[i]);
							i++;
						}
						else
						{
							moreCompounds = false;
							i++;
						}
					}
					moreCompounds = true;
					while (moreCompounds)
					{
						if (headers[i] != "Adduct/Replacement")
						{
							molnames.Add(headers[i]);
							i++;
						}
						else
							moreCompounds = false;
					}
					bool firstRow = true;
					while (read.Peek() >= 0)
					{
						//Read data
						String Line = read.ReadLine();
						String[] Lines = Line.Split(',');
						//initialize new gR object
						ResultsGroup gR = new ResultsGroup();
						DeconRow dR = new DeconRow();
						CompositionHypothesis.comphypo cH = new CompositionHypothesis.comphypo();
						gR.DeconRow = dR;
						gR.comphypo = cH;

						//Input data
						if (!String.IsNullOrEmpty(Lines[0]))
						{
							if (firstRow)
							{
								gR.comphypo.elementIDs = elementIDs;
								gR.comphypo.MoleNames = molnames;
								firstRow = false;
							}
							gR.Score = Convert.ToDouble(Lines[0]);
							gR.DeconRow.monoisotopic_mw = Convert.ToDouble(Lines[1]);
							gR.comphypo.compoundCompo = Lines[2];
							if (String.IsNullOrEmpty(Lines[2]) || Lines[2] == "0")
								gR.Match = false;
							else
								gR.Match = true;
							gR.comphypo.PepSequence = Lines[3];
							gR.numModiStates = Convert.ToDouble(Lines[5]);
							gR.numChargeStates = Convert.ToInt32(Lines[6]);
							gR.numOfScan = Convert.ToDouble(Lines[7]);
							gR.ScanDensity = Convert.ToDouble(Lines[8]);
							gR.ExpectedA = Convert.ToDouble(Lines[9]);
							gR.avgAA2List = new List<double>();
							gR.avgAA2List.Add(Convert.ToDouble(Lines[10]));
							gR.totalVolume = Convert.ToDouble(Lines[11]);
							gR.avgSigNoise = Convert.ToDouble(Lines[12]);
							gR.CentroidScan = Convert.ToDouble(Lines[13]);
							gR.DeconRow.scan_num = Convert.ToInt32(Lines[14]);
							gR.maxScanNum = Convert.ToInt32(Lines[15]);
							gR.minScanNum = Convert.ToInt32(Lines[16]);
							gR.comphypo.eqCount = new Dictionary<string, int>();
							int sh = 17;
							for (int ele = 0; ele < elementIDs.Count(); ele++ )
							{
								gR.comphypo.elementAmount.Add(Convert.ToInt32(Lines[sh]));
								sh++;
							}
							gR.comphypo.MW = Convert.ToDouble(Lines[sh]);
							sh++;
							List<int> eqCoun = new List<int>();
							for (int j = 0; j < molnames.Count(); j++)
							{
								eqCoun.Add(Convert.ToInt32(Lines[sh + j]));
							}
							gR.comphypo.eqCounts = eqCoun;
							gR.comphypo.AddRep = Lines[sh + molnames.Count()];
							gR.comphypo.AdductNum = Convert.ToInt32(Lines[sh + molnames.Count() + 1]);
							gR.comphypo.PepModification = Lines[24 + molnames.Count() + 2];
							gR.comphypo.MissedCleavages = Convert.ToInt32(Lines[sh + molnames.Count() + 3]);
							gR.comphypo.numGly = Convert.ToInt32(Lines[sh + molnames.Count() + 4]);
							gR.comphypo.StartAA = Convert.ToInt32(Lines[sh + molnames.Count() + 5]);
							gR.comphypo.EndAA = Convert.ToInt32(Lines[sh + molnames.Count() + 6]);
							Ans.Add(gR);
						}
					}
				}
				//older data format.
				else if (headers[3] == "PeptideSequence")
				{
					Boolean moreCompounds = true;
					int i = 24;
					while (moreCompounds)
					{
						if (headers[i] != "Adduct/Replacement")
						{
							molnames.Add(headers[i]);
							i++;
						}
						else
							moreCompounds = false;
					}
					bool firstRow = true;
					while (read.Peek() >= 0)
					{
						//Read data
						String Line = read.ReadLine();
						String[] Lines = Line.Split(',');
						//initialize new gR object
						ResultsGroup gR = new ResultsGroup();
						DeconRow dR = new DeconRow();
						CompositionHypothesis.comphypo cH = new CompositionHypothesis.comphypo();
						gR.DeconRow = dR;
						gR.comphypo = cH;
						if (firstRow)
						{
							gR.comphypo.elementIDs.AddRange(new List<string> { "C", "H", "N", "O", "S", "P" });
							gR.comphypo.MoleNames = molnames;
							firstRow = false;
						}

						//Input data
						if (!String.IsNullOrEmpty(Lines[0]))
						{                            
							gR.Score = Convert.ToDouble(Lines[0]);
							gR.DeconRow.monoisotopic_mw = Convert.ToDouble(Lines[1]);
							gR.comphypo.compoundCompo = Lines[2];
							if (String.IsNullOrEmpty(Lines[2]) || Lines[2] == "0")
								gR.Match = false;
							else
								gR.Match = true;
							gR.comphypo.PepSequence = Lines[3];
							gR.comphypo.MW = Convert.ToDouble(Lines[5]);
							gR.numModiStates = Convert.ToDouble(Lines[6]);
							gR.numChargeStates = Convert.ToInt32(Lines[7]);
							gR.numOfScan = Convert.ToDouble(Lines[8]);
							gR.ScanDensity = Convert.ToDouble(Lines[9]);
							gR.ExpectedA = Convert.ToDouble(Lines[10]);
							gR.avgAA2List = new List<double>();
							gR.avgAA2List.Add(Convert.ToDouble(Lines[11]));
							gR.totalVolume = Convert.ToDouble(Lines[12]);
							gR.avgSigNoise = Convert.ToDouble(Lines[13]);
							gR.CentroidScan = Convert.ToDouble(Lines[14]);
							gR.DeconRow.scan_num = Convert.ToInt32(Lines[15]);
							gR.maxScanNum = Convert.ToInt32(Lines[16]);
							gR.minScanNum = Convert.ToInt32(Lines[17]);
							gR.comphypo.eqCount = new Dictionary<string, int>();
							for (int k = 18; k < 24; k++)
							{
								gR.comphypo.elementAmount.Add(Convert.ToInt32(Lines[k]));
							}
							List<int> eqCoun = new List<int>();
							for (int j = 0; j < molnames.Count(); j++)
							{
								eqCoun.Add(Convert.ToInt32(Lines[24 + j]));
							}
							gR.comphypo.eqCounts = eqCoun;
							gR.comphypo.AddRep = Lines[24 + molnames.Count()];
							gR.comphypo.AdductNum = Convert.ToInt32(Lines[24 + molnames.Count() + 1]);
							gR.comphypo.PepModification = Lines[24 + molnames.Count() + 2];
							gR.comphypo.MissedCleavages = Convert.ToInt32(Lines[24 + molnames.Count() + 3]);
							gR.comphypo.numGly = Convert.ToInt32(Lines[24 + molnames.Count() + 4]);
							Ans.Add(gR);
						}
					}
				}
				//This is supporting an older format of data. Today is Sept 2013, can be deleted after 1 year.
				else
				{
					Boolean moreCompounds = true;
					int i = 23;
					while (moreCompounds)
					{
						if (headers[i] != "Adduct/Replacement")
						{
							molnames.Add(headers[i]);
							i++;
						}
						else
							moreCompounds = false;
					}
					bool firstRow = true;
					while (read.Peek() >= 0)
					{
						//Read data
						String Line = read.ReadLine();
						String[] Lines = Line.Split(',');
						//initialize new gR object
						ResultsGroup gR = new ResultsGroup();
						if (firstRow)
						{
							gR.comphypo.elementIDs.AddRange(new List<string> { "C", "H", "N", "O", "S", "P" });
							gR.comphypo.MoleNames = molnames;
							firstRow = false;
						}
						DeconRow dR = new DeconRow();
						CompositionHypothesis.comphypo cH = new CompositionHypothesis.comphypo();
						gR.DeconRow = dR;
						gR.comphypo = cH;
						if (!String.IsNullOrEmpty(Lines[0]))
						{
							//Input data

							gR.Score = Convert.ToDouble(Lines[0]);
							gR.DeconRow.monoisotopic_mw = Convert.ToDouble(Lines[1]);
							gR.comphypo.compoundCompo = Lines[2].Replace(",", ";");
							if (String.IsNullOrEmpty(Lines[2]) || Lines[2] == "0")
								gR.Match = false;
							else
								gR.Match = true;
							gR.comphypo.MW = Convert.ToDouble(Lines[4]);
							gR.numModiStates = Convert.ToDouble(Lines[5]);
							gR.numChargeStates = Convert.ToInt32(Lines[6]);
							gR.numOfScan = Convert.ToDouble(Lines[7]);
							gR.ScanDensity = Convert.ToDouble(Lines[8]);
							gR.ExpectedA = Convert.ToDouble(Lines[9]);
							gR.avgAA2List = new List<double>();
							gR.avgAA2List.Add(Convert.ToDouble(Lines[10]));
							gR.totalVolume = Convert.ToDouble(Lines[11]);
							gR.avgSigNoise = Convert.ToDouble(Lines[12]);
							gR.CentroidScan = Convert.ToDouble(Lines[13]);
							gR.DeconRow.scan_num = Convert.ToInt32(Lines[14]);
							gR.maxScanNum = Convert.ToInt32(Lines[15]);
							gR.minScanNum = Convert.ToInt32(Lines[16]);
							gR.comphypo.eqCount = new Dictionary<string, int>();
							for (int k = 17; k < 23; k++)
							{
								gR.comphypo.elementAmount.Add(Convert.ToInt32(Lines[k]));
							}
							gR.comphypo.eqCount.Add("A", Convert.ToInt32(Lines[23]));
							gR.comphypo.eqCount.Add("B", Convert.ToInt32(Lines[24]));
							gR.comphypo.eqCount.Add("C", Convert.ToInt32(Lines[25]));
							gR.comphypo.eqCount.Add("D", Convert.ToInt32(Lines[26]));
							gR.comphypo.eqCount.Add("E", Convert.ToInt32(Lines[27]));
							gR.comphypo.eqCount.Add("F", Convert.ToInt32(Lines[28]));
							gR.comphypo.eqCount.Add("G", Convert.ToInt32(Lines[29]));
							gR.comphypo.eqCount.Add("H", Convert.ToInt32(Lines[30]));
							gR.comphypo.eqCount.Add("I", Convert.ToInt32(Lines[31]));
							gR.comphypo.eqCount.Add("J", Convert.ToInt32(Lines[32]));
							gR.comphypo.eqCount.Add("K", Convert.ToInt32(Lines[33]));
							gR.comphypo.eqCount.Add("L", Convert.ToInt32(Lines[34]));
							gR.comphypo.eqCount.Add("M", Convert.ToInt32(Lines[35]));
							gR.comphypo.eqCount.Add("N", Convert.ToInt32(Lines[36]));
							gR.comphypo.eqCount.Add("O", Convert.ToInt32(Lines[37]));
							gR.comphypo.eqCount.Add("P", Convert.ToInt32(Lines[38]));
							gR.comphypo.eqCount.Add("Q", Convert.ToInt32(Lines[39]));
							gR.comphypo.AddRep = Lines[40];
							gR.comphypo.AdductNum = Convert.ToInt32(Lines[41]);
							gR.comphypo.PepSequence = Lines[42];
							gR.comphypo.PepModification = Lines[43];
							gR.comphypo.MissedCleavages = Convert.ToInt32(Lines[44]);
							gR.comphypo.numGly = Convert.ToInt32(Lines[45]);
							Ans.Add(gR);
						}
					}
				}
			}
			//This is gly1 data.
			else
			{
				String header = read.ReadLine();
				String[] headers = header.Split('\t');

				while (read.Peek() >= 0)
				{
					//Read data
					String Line = read.ReadLine();
					String[] Lines = Line.Split('\t');
					//initialize new gR object
					ResultsGroup gR = new ResultsGroup();
					DeconRow dR = new DeconRow();
					CompositionHypothesis.comphypo cH = new CompositionHypothesis.comphypo();
					gR.DeconRow = dR;
					gR.comphypo = cH;
					if (!String.IsNullOrEmpty(Lines[0]))
					{
						//Input data
						gR.comphypo.MoleNames = molnames;
						gR.Score = Convert.ToDouble(Lines[0]);
						gR.DeconRow.monoisotopic_mw = Convert.ToDouble(Lines[1]);
						gR.comphypo.compoundCompo = Lines[2].Replace(",", ";");
						if (String.IsNullOrEmpty(Lines[2]) || Lines[2] == "0")
						{
							gR.Match = false;
							gR.comphypo.MW = 0;
						}
						else
						{
							gR.Match = true;
							gR.comphypo.MW = Convert.ToDouble(Lines[4]);
						}
						gR.numModiStates = Convert.ToDouble(Lines[5]);
						gR.numChargeStates = Convert.ToInt32(Lines[6]);
						gR.numOfScan = Convert.ToDouble(Lines[7]);
						gR.ScanDensity = Convert.ToDouble(Lines[8]);
						gR.ExpectedA = Convert.ToDouble(Lines[9]);
						gR.avgAA2List = new List<double>();
						gR.avgAA2List.Add(Convert.ToDouble(Lines[10]));
						gR.totalVolume = Convert.ToDouble(Lines[11]);
						gR.avgSigNoise = Convert.ToDouble(Lines[12]);
						gR.CentroidScan = Convert.ToDouble(Lines[13]);
						gR.DeconRow.scan_num = Convert.ToInt32(Convert.ToDouble(Lines[14]));
						Ans.Add(gR);
					}
				}

			}
			return Ans;
		}

		//This is used to combine several groupingResults.
		public List<ResultsGroup> combineResults(List<ResultsGroup>[] Results)
		{
			//Get molenames and elementIDs as usual, as first thing
			List<string> elementIDs = new List<string>();
			List<string> molename = new List<string>();
			for (int i = 0; i < Results[0].Count(); i++)
			{
				if (Results[0][i].comphypo.elementIDs.Count > 0)
				{
					for (int k = 0; k < Results[0][i].comphypo.elementIDs.Count(); k++)
					{
						elementIDs.Add(Results[0][i].comphypo.elementIDs[k]);
					}
					for (int k = 0; k < Results[0][i].comphypo.MoleNames.Count(); k++)
					{
						molename.Add(Results[0][i].comphypo.MoleNames[k]);
					}
					break;
				}
			}
			Double SumofTotalVolume = 0;
			List<ResultsGroup> storage = new List<ResultsGroup>();
			for (int i = 0; i < Results.Count(); i++)
			{
				for (int k = 0; k < Results[i].Count(); k++)
				{
					Double[] AllTotalVolumes = new Double[Results.Count()];
					Results[i][k].listOfOriginalTotalVolumes = AllTotalVolumes;
					Results[i][k].listOfOriginalTotalVolumes[i] = Results[i][k].totalVolume;
					storage.Add(Results[i][k]);
					SumofTotalVolume = SumofTotalVolume + Results[i][k].totalVolume;
				}
			}
			List<ResultsGroup> FinalAns = new List<ResultsGroup>();
			storage = storage.OrderByDescending(a => a.comphypo.MW).ThenBy(b => b.comphypo.compoundCompo).ToList();
			int j = 0;
			while (j < storage.Count())
			{
				if (storage[j].comphypo.MW == 0)
				{
					storage[j].relativeTotalVolumeSD = 0;
					storage[j].relativeTotalVolume = storage[j].totalVolume / SumofTotalVolume;
					FinalAns.Add(storage[j]);
					j++;
					continue;
				}
				if (j == storage.Count() - 1)
				{
					storage[j].relativeTotalVolumeSD = 0;
					storage[j].relativeTotalVolume = storage[j].totalVolume / SumofTotalVolume;
					FinalAns.Add(storage[j]);
					j++;
					continue;
				}
				List<ResultsGroup> combiningRows = new List<ResultsGroup>();
				if (storage[j].comphypo.compoundCompo.Equals(storage[j + 1].comphypo.compoundCompo) && (storage[j].comphypo.AdductNum == storage[j + 1].comphypo.AdductNum) && storage[j].comphypo.PepSequence.Equals(storage[j + 1].comphypo.PepSequence))
				{
					storage[j].relativeTotalVolumeSD = 0;
					storage[j].relativeTotalVolume = storage[j].totalVolume / SumofTotalVolume;
					combiningRows.Add(storage[j]);
					j++;
					storage[j].relativeTotalVolumeSD = 0;
					storage[j].relativeTotalVolume = storage[j].totalVolume / SumofTotalVolume;
					combiningRows.Add(storage[j]);
					bool moreRows = true;
					while (moreRows)
					{
						if (storage[j].comphypo.compoundCompo.Equals(storage[j + 1].comphypo.compoundCompo) && (storage[j].comphypo.AdductNum == storage[j + 1].comphypo.AdductNum) && storage[j].comphypo.PepSequence.Equals(storage[j + 1].comphypo.PepSequence))
						{
							j++;
							storage[j].relativeTotalVolumeSD = 0;
							storage[j].relativeTotalVolume = storage[j].totalVolume / SumofTotalVolume;
							combiningRows.Add(storage[j]);
						}
						else
						{
							moreRows = false;
						}
					}
					j++;
					FinalAns.Add(averagingGR(combiningRows));
					continue;
				}
				else
				{
					storage[j].relativeTotalVolumeSD = 0;
					storage[j].relativeTotalVolume = storage[j].totalVolume / SumofTotalVolume;
					FinalAns.Add(storage[j]);
					j++;
					continue;
				}
			}
			FinalAns[0].comphypo.elementIDs = elementIDs;
			FinalAns[0].comphypo.MoleNames = molename;
			return FinalAns;
		}

		private ResultsGroup averagingGR(List<ResultsGroup> combiningRows)
		{
			List<Int32> numChargeStates = new List<int>();
			List<Double> ScanDensity= new List<Double>() ;
			List<Double> numModiStates= new List<Double>() ;
			List<Double> totalVolume= new List<Double>() ;
			List<Double> ExpectedA= new List<Double>() ;
			List<Double> Score= new List<Double>() ;
			List<Double> CentroidScan= new List<Double>() ;
			List<Double> numOfScan= new List<Double>() ;
			List<Double> avgSigNoise= new List<Double>() ;
			//These are for calculating the features
			List<Int32> maxScanNum= new List<int>() ;
			List<Int32> minScanNum= new List<int>() ;
			List<Int32> scanNumList= new List<int>() ;
			List<Int32> ChargeStateList = new List<int>();
			List<Double> avgSigNoiseList= new List<Double>() ;
			List<Double> CentroidScanLR= new List<Double>() ;
			List<Double> avgAA2List= new List<Double>() ;
			List<Double> relativeTotalVolume = new List<double>();


			List<Int32> scan_num = new List<Int32>();
			List<Int32> charge = new List<Int32>();
			List<Int32> abundance = new List<Int32>();
			List<Double> mz = new List<Double>();
			List<Double> fit = new List<Double>();
			List<Double> average_mw = new List<Double>();
			List<Double> monoisotopic_mw = new List<Double>();
			List<Double> mostabundant_mw = new List<Double>();
			List<Double> fwhm = new List<Double>();
			List<Double> signal_noise = new List<Double>();
			List<Int32> mono_abundance = new List<Int32>();
			List<Int32> mono_plus2_abundance = new List<Int32>();
			List<Int32> flag = new List<Int32>();
			List<Double> interference_sore = new List<Double>();

			ResultsGroup FinalAns = new ResultsGroup();
			FinalAns.listOfOriginalTotalVolumes = combiningRows[0].listOfOriginalTotalVolumes;

			for (int i = 0; i < combiningRows.Count(); i++)
			{
				numChargeStates.Add(combiningRows[i].numChargeStates);
				ScanDensity.Add(combiningRows[i].ScanDensity);
				numModiStates.Add(combiningRows[i].numModiStates);
				totalVolume.Add(combiningRows[i].totalVolume);
				ExpectedA.Add(combiningRows[i].ExpectedA);
				Score.Add(combiningRows[i].Score);
				CentroidScan.Add(combiningRows[i].CentroidScan);
				numOfScan.Add(combiningRows[i].numOfScan);
				avgSigNoise.Add(combiningRows[i].avgSigNoise);
				//These are for calculating the features
				maxScanNum.Add(combiningRows[i].maxScanNum);
				minScanNum.Add(combiningRows[i].minScanNum);
				CentroidScanLR.Add(combiningRows[i].CentroidScanLR);
				avgAA2List.AddRange(combiningRows[i].avgAA2List);
				relativeTotalVolume.Add(combiningRows[i].relativeTotalVolume);

				charge.Add(combiningRows[i].DeconRow.charge);
				scan_num.Add(combiningRows[i].DeconRow.scan_num);
				abundance.Add(combiningRows[i].DeconRow.abundance);
				mz.Add(combiningRows[i].DeconRow.mz);
				fit.Add(combiningRows[i].DeconRow.fit);
				average_mw.Add(combiningRows[i].DeconRow.average_mw);
				monoisotopic_mw.Add(combiningRows[i].DeconRow.monoisotopic_mw);
				mostabundant_mw.Add(combiningRows[i].DeconRow.mostabundant_mw);
				fwhm.Add(combiningRows[i].DeconRow.fwhm);
				signal_noise.Add(combiningRows[i].DeconRow.signal_noise);
				mono_abundance.Add(combiningRows[i].DeconRow.mono_abundance);
				mono_plus2_abundance.Add(combiningRows[i].DeconRow.mono_plus2_abundance);
				flag.Add(combiningRows[i].DeconRow.flag);
				interference_sore.Add(combiningRows[i].DeconRow.interference_sore);
				for (int h = 0; h < combiningRows[i].listOfOriginalTotalVolumes.Count(); h++)
				{
					if (combiningRows[i].listOfOriginalTotalVolumes[h] > FinalAns.listOfOriginalTotalVolumes[h])
					{
						FinalAns.listOfOriginalTotalVolumes[h] = combiningRows[i].listOfOriginalTotalVolumes[h];
					}
				}

			}


			FinalAns.DeconRow = new DeconRow();
			FinalAns.comphypo = combiningRows[0].comphypo;
			FinalAns.numChargeStates = Convert.ToInt32(numChargeStates.Average());
			FinalAns.ScanDensity = ScanDensity.Average();
			FinalAns.numModiStates = numModiStates.Average();
			FinalAns.totalVolume = totalVolume.Average();
			FinalAns.ExpectedA = ExpectedA.Average();
			FinalAns.Score = Score.Average();
			FinalAns.CentroidScan = CentroidScan.Average();
			FinalAns.numOfScan = numOfScan.Average();
			FinalAns.avgSigNoise = avgSigNoise.Average();
			FinalAns.maxScanNum = Convert.ToInt32(maxScanNum.Average());
			FinalAns.minScanNum = Convert.ToInt32(minScanNum.Average());
			FinalAns.CentroidScanLR = CentroidScanLR.Average();
			FinalAns.totalVolumeSD = totalVolume.StdDev();
			FinalAns.relativeTotalVolumeSD = relativeTotalVolume.StdDev();
			FinalAns.avgAA2List = new List<double>();
			FinalAns.avgAA2List.Add(avgAA2List.Average());
			FinalAns.relativeTotalVolume = relativeTotalVolume.Average();

			FinalAns.DeconRow.scan_num = Convert.ToInt32(scan_num.Average());
			FinalAns.DeconRow.abundance = Convert.ToInt32(abundance.Average());
			FinalAns.DeconRow.mz = mz.Average();
			FinalAns.DeconRow.fit = fit.Average();
			FinalAns.DeconRow.average_mw = average_mw.Average();
			FinalAns.DeconRow.monoisotopic_mw = monoisotopic_mw.Average();
			FinalAns.DeconRow.mostabundant_mw = mostabundant_mw.Average();
			FinalAns.DeconRow.fwhm = fwhm.Average();
			FinalAns.DeconRow.signal_noise = signal_noise.Average();
			FinalAns.DeconRow.mono_abundance = Convert.ToInt32(mono_abundance.Average());
			FinalAns.DeconRow.mono_plus2_abundance = Convert.ToInt32(mono_plus2_abundance.Average());
			FinalAns.DeconRow.flag = Convert.ToInt32(flag.Average());
			FinalAns.DeconRow.interference_sore = interference_sore.Average();




			return FinalAns;
		}
	}

	//Accessor Methods:####################################################################
	public class ResultsGroup
	{
		public ResultsGroup()
		{
			comphypo = new CompositionHypothesis.comphypo();
		}
		public DeconRow DeconRow;
		public Boolean mostAbundant;
		public Boolean Match;
		public CompositionHypothesis.comphypo comphypo;
		//The features
		public Int32 numChargeStates;
		public Double ScanDensity;
		public Double numModiStates;
		public Double totalVolume;
		public Double ExpectedA;
		public Double Score;
		public Double CentroidScan;
		public Double numOfScan;
		public Double avgSigNoise;
		//These are for calculating the features
		public Int32 maxScanNum;
		public Int32 minScanNum;
		public List<Int32> scanNumList;
		public List<Int32> ChargeStateList;
		public List<Double> avgSigNoiseList;
		public Double CentroidScanLR;
		public List<Double> avgAA2List;
		//These are special values for combining groupingResults
		public Double totalVolumeSD;
		public Double relativeTotalVolume;
		public Double relativeTotalVolumeSD;
		public Double[] listOfOriginalTotalVolumes;
		//They are for calculating the average weighted MW after groupings.
		public List<Double> listMonoMW;
		public List<Double> listAbun;
	}

	#region CopyPaste from original Program.cs to support old shimmed methods
	//http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp This function of randomizing a list is good, so I am using it. It is written by grenade.
	//start of grenade's code.
	public static class ThreadSafeRandom
	{
		[ThreadStatic]
		private static Random Local;

		public static Random ThisThreadsRandom {
			get { return Local ?? (Local = new Random (unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
		}
	}

	static class MyExtensions
	{
		public static void Shuffle<T> (this IList<T> list)
		{
			int n = list.Count;
			while (n > 1) {
				n--;
				int k = ThreadSafeRandom.ThisThreadsRandom.Next (n + 1);
				T value = list [k];
				list [k] = list [n];
				list [n] = value;
			}
		}
		// End of grenade's code
		//This is a standard deviation function from ehre http://stackoverflow.com/questions/2253874/linq-equivalent-for-standard-deviation
		public static double StdDev (this IEnumerable<double> values)
		{
			double ret = 0;
			int count = values.Count ();
			if (count > 1) {
				//Compute the Average
				double avg = values.Average ();

				//Perform the Sum of (value-avg)^2
				double sum = values.Sum (d => (d - avg) * (d - avg));

				//Put it all together
				ret = Math.Sqrt (sum / count);
			}
			return ret;
		}

	}
	#endregion
}

