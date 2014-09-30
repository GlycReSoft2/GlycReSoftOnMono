using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Accord.Statistics.Analysis;
using YAMP_MathParserTK_NET;

namespace GlycReSoft
{

	public class Tag1Result
	{
		//These are used to store the results, so that there is no need to rerun supervisedLearning repeatedly when you need data.
		private static List<GroupingResults>[] AllFinalResults;
		private static OpenFileDialog DeconData;
		private static List<GlycanCompositionParser.comphypo> ComHyLink;
		private static List<string> elementIDs = new List<string>();
		private static List<string> molenames = new List<string>();
		//PleaseWait msgForm = new PleaseWait();

		//First method, run supervised learning with composition hypothesis.
		public tag1Result(OpenFileDialog oFDDeconData, String ComHypoLink)
		{
			InitializeComponent();
			DeconData = oFDDeconData;
			composition ct = new composition();
			ComHyLink = ct.getCompHypo(ComHypoLink);

			//This part is used to show the "Please Wait" box while running the code.
			BackgroundWorker bw = new BackgroundWorker();
			bw.DoWork += new DoWorkEventHandler(bw_DoWork);
			bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

			//set lable and your waiting text in this form
			try
			{
				bw.RunWorkerAsync();//this will run the DoWork code at background thread
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			elementIDs.Clear();
			molenames.Clear();
			for (int i = 0; i < AllFinalResults[0].Count(); i++)
			{
				if (AllFinalResults[0][i].comphypo.elementIDs.Count() > 0)
				{
					for (int j = 0; j < AllFinalResults[0][i].comphypo.elementIDs.Count(); j++)
					{
						elementIDs.Add(AllFinalResults[0][i].comphypo.elementIDs[j]);
					}
					for (int j = 0; j < AllFinalResults[0][i].comphypo.MoleNames.Count(); j++)
					{
						molenames.Add(AllFinalResults[0][i].comphypo.MoleNames[j]);
					}
				}
			}

			comboBox1.DataSource = oFDDeconData.FileNames;
			comboBox1.SelectedIndex = 0;
			comboBox3.DataSource = oFDDeconData.FileNames;
			comboBox3.SelectedIndex = 0;
			dataGridView1.DataSource = toDataTable(AllFinalResults, 0);
			dataGridView1.ReadOnly = true;
		}
		void bw_DoWork(object sender, DoWorkEventArgs e)
		{
			stuffToDo();
		}
		private void stuffToDo()
		{
			supervisedLearning SL = new supervisedLearning();
			AllFinalResults = SL.run(DeconData, ComHyLink);
		}

		//Second method, run unsupervised learning without composition hypothesis.
		public tag1Result(OpenFileDialog oFDDeconData)
		{
			InitializeComponent();
			DeconData = oFDDeconData;

			//This part is used to show the "Please Wait" box while running the code.
			BackgroundWorker bw = new BackgroundWorker();
			bw.DoWork += new DoWorkEventHandler(bw_DoWork2);
			bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted2);

			//set lable and your waiting text in this form
			try
			{
				bw.RunWorkerAsync();//this will run the DoWork code at background thread
				msgForm.ShowDialog();//show the please wait box
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}


			comboBox1.DataSource = oFDDeconData.FileNames;
			comboBox1.SelectedIndex = 0;
			comboBox3.DataSource = oFDDeconData.FileNames;
			comboBox3.SelectedIndex = 0;
			dataGridView1.DataSource = toDataTable(AllFinalResults, 0);
			dataGridView1.ReadOnly = true;

		}
		void bw_DoWork2(object sender, DoWorkEventArgs e)
		{
			stuffToDo2();
		}
		void bw_RunWorkerCompleted2(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				MessageBox.Show(Convert.ToString(e.Error));
			}
			//all background work has completed and we are going to close the waiting message
			msgForm.Close();
		}
		private void stuffToDo2()
		{
			unsupervisedLearning UL = new unsupervisedLearning();
			AllFinalResults = UL.run(DeconData);
		}


		//Save All to File button:
		private void button1_Click(object sender, EventArgs e)
		{
			DialogResult result = fBDGResults1.ShowDialog();
			if (result == DialogResult.OK)
			{
				String Path = fBDGResults1.SelectedPath;
				String[] Filenames = DeconData.SafeFileNames;
				for (int i = 0; i < AllFinalResults.Count(); i++)
				{
					String currentPath = Path + "//ResultOf" + Filenames[i];
					FileStream FS = new FileStream(currentPath, FileMode.Create, FileAccess.Write);
					StreamWriter write = new StreamWriter(FS);
					//Write the header
					write.Write("Score,MassSpec MW,Compound Key,PeptideSequence,PPM Error,#ofAdduct,#ofCharges,#ofScans,ScanDensity,Avg A:A+2 Error,A:A+2 Ratio,Total Volume,Signal to Noise Ratio,Centroid Scan Error,Centroid Scan,MaxScanNumber,MinScanNumber");
					int elementnamecount = 0;
					for (int u = 0; u < AllFinalResults[i].Count; u++)
					{

						foreach (String name in elementIDs)
						{
							write.Write("," + name);
						}
						elementnamecount = elementIDs.Count();
						break;

					}
					write.Write(",Hypothesis MW");
					int molenamecount = 0;
					foreach (String name in molenames)
					{
						write.Write("," + name);
					}
					molenamecount = molenames.Count();

					write.WriteLine(",Adduct/Replacement,Adduct Amount,PeptideModification,PeptideMissedCleavage#,#ofGlycanAttachmentToPeptide,StarAA,EndAA");
					parameters pr = new parameters();
					parameters.para paradata = pr.getParameters();

					//Write the data
					for (int u = 0; u < AllFinalResults[i].Count; u++)
					{
						if (AllFinalResults[i][u].comphypo.MW != 0)
						{
							Double MatchingError = 0;
							if (AllFinalResults[i][u].comphypo.MW != 0)
							{
								MatchingError = ((AllFinalResults[i][u].DeconRow.monoisotopic_mw - AllFinalResults[i][u].comphypo.MW) / (AllFinalResults[i][u].DeconRow.monoisotopic_mw)) * 1000000;
							}
							write.Write(AllFinalResults[i][u].Score + "," + AllFinalResults[i][u].DeconRow.monoisotopic_mw + "," + AllFinalResults[i][u].comphypo.compoundCompo + "," + AllFinalResults[i][u].comphypo.PepSequence + "," + MatchingError + "," + AllFinalResults[i][u].numModiStates + "," + AllFinalResults[i][u].numChargeStates + "," + AllFinalResults[i][u].numOfScan + "," + AllFinalResults[i][u].ScanDensity + "," + AllFinalResults[i][u].ExpectedA + "," + (AllFinalResults[i][u].DeconRow.mono_abundance / (AllFinalResults[i][u].DeconRow.mono_plus2_abundance + 1)) + "," + AllFinalResults[i][u].totalVolume + "," + AllFinalResults[i][u].DeconRow.signal_noise + "," + AllFinalResults[i][u].CentroidScan + "," + AllFinalResults[i][u].DeconRow.scan_num + "," + AllFinalResults[i][u].maxScanNum + "," + AllFinalResults[i][u].minScanNum);
							for (int s = 0; s < elementnamecount; s++)
							{
								write.Write("," + AllFinalResults[i][u].comphypo.elementAmount[s]);
							}
							write.Write("," + AllFinalResults[i][u].comphypo.MW);
							for (int s = 0; s < molenamecount; s++)
							{
								write.Write("," + AllFinalResults[i][u].comphypo.eqCounts[s]);
							}
							write.WriteLine("," + AllFinalResults[i][u].comphypo.AddRep + "," + AllFinalResults[i][u].comphypo.AdductNum + "," + AllFinalResults[i][u].comphypo.PepModification + "," + AllFinalResults[i][u].comphypo.MissedCleavages + "," + AllFinalResults[i][u].comphypo.numGly + "," + AllFinalResults[i][u].comphypo.StartAA + "," + AllFinalResults[i][u].comphypo.EndAA);
						}
						else
						{                            
							write.Write(AllFinalResults[i][u].Score + "," + AllFinalResults[i][u].DeconRow.monoisotopic_mw + "," + 0 + "" + "," + "," + 0 + "," + AllFinalResults[i][u].numModiStates + "," + AllFinalResults[i][u].numChargeStates + "," + AllFinalResults[i][u].numOfScan + "," + AllFinalResults[i][u].ScanDensity + "," + AllFinalResults[i][u].ExpectedA + "," + (AllFinalResults[i][u].DeconRow.mono_abundance / (AllFinalResults[i][u].DeconRow.mono_plus2_abundance + 1)) + "," + AllFinalResults[i][u].totalVolume + "," + AllFinalResults[i][u].DeconRow.signal_noise + "," + AllFinalResults[i][u].CentroidScan + "," + AllFinalResults[i][u].DeconRow.scan_num + "," + AllFinalResults[i][u].maxScanNum + "," + AllFinalResults[i][u].minScanNum);
							for (int s = 0; s < elementnamecount; s++)
							{
								write.Write("," + 0);
							}
							write.Write("," + 0);
							for (int s = 0; s < molenamecount; s++)
							{
								write.Write("," + 0);
							}
							write.WriteLine("," + "N/A" + "," + 0 + "," + "" + "," + 0 + "," + 0 + "," + 0 + "," + 0);
						}
					}
					write.Flush();
					write.Close();
					FS.Close();
				}
			}
		}

		//Controls for combobox1
		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			dataGridView1.DataSource = this.toDataTable(AllFinalResults, comboBox1.SelectedIndex);
		}




		//Tag2####################################################################################

		//Score radiobutton.
		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			button15.Enabled = false;
			button2.Enabled = true;
		}
		//Randomly generated False Data Set radio button.
		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			button15.Enabled = true;
			button17.Enabled = true;
			button2.Enabled = false;
		}

		//Add composition File button.
		private void button15_Click(object sender, EventArgs e)
		{
			oFDcposTest.Filter = "cpos files (*.cpos)|*.cpos";
			oFDcposTest.ShowDialog();
		}
		private void oFDcposTest_FileOk(object sender, CancelEventArgs e)
		{
			String fileInfo = "";
			Stream mystream = null;
			try
			{
				if ((mystream = oFDcposTest.OpenFile()) != null)
				{
					String file = oFDcposTest.FileName;
					fileInfo += String.Format("{0}\n", file);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
			}
			mystream.Close();
			richTextBox6.Font = new Font("Times New Roman", 12);
			richTextBox6.Text = fileInfo;
			button2.Enabled = true;
		}

		//Load protein prospector MS-digest button.
		private void button17_Click(object sender, EventArgs e)
		{
			oFDPPMSD.Filter = "txt files (*.txt)|*.txt";
			oFDPPMSD.ShowDialog();
		}
		private void oFDPPMSD_FileOk(object sender, CancelEventArgs e)
		{
			String fileInfo = "";
			Stream mystream = null;
			try
			{
				if ((mystream = oFDPPMSD.OpenFile()) != null)
				{
					String file = oFDPPMSD.FileName;
					fileInfo += String.Format("{0}\n", file);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: Could not read Protein Prospector MS-digest file from disk. Original error: " + ex.Message);
			}
			mystream.Close();
			richTextBox7.Font = new Font("Times New Roman", 12);
			richTextBox7.Text = fileInfo;
		}
		private List<composition.PPMSD> readtablim(String currentpath)
		{
			List<composition.PPMSD> data = new List<composition.PPMSD>();
			FileStream FS = new FileStream(currentpath, FileMode.Open, FileAccess.Read);
			StreamReader read = new StreamReader(FS);
			//skip title line:
			read.ReadLine();
			while (read.Peek() >= 0)
			{
				composition.PPMSD pp = new composition.PPMSD();
				String line = read.ReadLine();
				String[] Lines = line.Split('\t');
				pp.number = Convert.ToInt32(Lines[0]);
				pp.Mass = Convert.ToDouble(Lines[1]);
				pp.Charge = Convert.ToInt32(Lines[3]);
				pp.Modifications = Convert.ToString(Lines[4]);
				pp.StartAA = Convert.ToInt32(Lines[5]);
				pp.EndAA = Convert.ToInt32(Lines[6]);
				pp.MissedCleavages = Convert.ToInt32(Lines[7]);
				pp.PreviousAA = Convert.ToString(Lines[8]);
				pp.Sequence = Convert.ToString(Lines[9]);
				pp.NextAA = Convert.ToString(Lines[10]);
				data.Add(pp);
			}
			read.Close();
			FS.Close();

			return data;
		}




		//Evaluate Results Button
		private void button2_Click(object sender, EventArgs e)
		{
			comboBox2.Items.Clear();
			//This part is used to show the "Please Wait" box while running the code.
			BackgroundWorker bw = new BackgroundWorker();
			bw.DoWork += new DoWorkEventHandler(bw_DoWork3);
			bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted3);

			//set lable and your waiting text in this form
			try
			{
				bw.RunWorkerAsync();//this will run the DoWork code at background thread
				msgForm.ShowDialog();//show the please wait box
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		void bw_DoWork3(object sender, DoWorkEventArgs e)
		{
			stuffToDo3();
		}
		void bw_RunWorkerCompleted3(object sender, RunWorkerCompletedEventArgs e)
		{
			//all background work has completed and we are going to close the waiting message
			msgForm.Close();
			tabPage2.Show();
		}
		//List used to store data for the datagridview (data of TP rate and FP rate).
		List<DataTable> TF = new List<DataTable>();
		private void stuffToDo3()
		{

			TF.Clear();
			//Clean up the chart for new data.
			chart1.Invoke(new MethodInvoker(
				delegate
				{
					foreach (var series in chart1.Series)
					{
						series.Points.Clear();
					}
					chart1.Series.Clear();
				}));

			//This is the diagonal Black Line
			chart1.Invoke(new MethodInvoker(
				delegate
				{
					chart1.Series.Add("Diagonal");
					chart1.Series["Diagonal"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
					chart1.Series["Diagonal"].Color = Color.Black;
					chart1.Series["Diagonal"].Points.AddXY(0, 0);
					chart1.Series["Diagonal"].Points.AddXY(1, 1);

				}));
			if (radioButton1.Checked == true)
			{
				this.scoreBasedGraph();
			}
			if (radioButton2.Checked == true)
			{
				this.FDSBasedGraph();
			}

		}



		//This function draws the ROC curve by the TP and FP rates calculated from the score.
		private void scoreBasedGraph()
		{
			composition comp = new composition();

			Features FT = new Features();
			String currentpath = Application.StartupPath + "\\FeatureCurrent.fea";
			Feature Fea = FT.readFeature(currentpath);
			//Create the list of random composition hypotesis for testing FDR. 
			//ObtainTrue Position data.
			this.drawGraph(AllFinalResults, " ");


			//Checkbox1 is default features.####################################
			List<groupingResults>[] DefaultFeature = new List<groupingResults>[DeconData.FileNames.Count()];
			String path = Application.StartupPath + "\\FeatureDefault.fea";
			Feature DeFea = FT.readFeature(path);
			if (checkBox1.Checked == true)
			{
				supervisedLearning SL = new supervisedLearning();
				List<groupingResults>[] TrueDATADefault = SL.evaluate(DeconData, ComHyLink, DeFea);

				this.drawGraph(TrueDATADefault, " Default Features");
			}


			//################################################
			//Checkbox2 is unsupervised Learning. It is a bit different from supervised learning, so it is hard-coded here.
			unsupervisedLearning UL = new unsupervisedLearning();
			if (checkBox2.Checked == true)
			{
				List<groupingResults>[] USLTrueDATA = UL.evaluate(DeconData, Fea);
				//ROC curve needs match to perform, so we will use the match list from Supervised learning and apply them to USLDATA.
				for (int i = 0; i < DeconData.FileNames.Count(); i++)
				{
					AllFinalResults[i] = AllFinalResults[i].OrderByDescending(a => a.DeconRow.monoisotopic_mw).ToList();
					USLTrueDATA[i] = USLTrueDATA[i].OrderByDescending(b => b.DeconRow.monoisotopic_mw).ToList();
					int USllasttruematch = 0;
					for (int j = 0; j < AllFinalResults[i].Count; j++)
					{
						if (AllFinalResults[i][j].Match == true)
						{
							for (int k = USllasttruematch; k < USLTrueDATA[i].Count; k++)
							{
								if (USLTrueDATA[i][k].DeconRow.monoisotopic_mw < AllFinalResults[i][j].DeconRow.monoisotopic_mw)
								{
									USllasttruematch = k;
									break;
								}
								if (USLTrueDATA[i][k].DeconRow.monoisotopic_mw == AllFinalResults[i][j].DeconRow.monoisotopic_mw)
								{
									USLTrueDATA[i][k].Match = true;
									USLTrueDATA[i][k].comphypo = AllFinalResults[i][j].comphypo;
									USllasttruematch = k + 1;
									break;
								}
								if (USLTrueDATA[i][k].DeconRow.monoisotopic_mw > AllFinalResults[i][j].DeconRow.monoisotopic_mw)
								{
									USLTrueDATA[i][k].Match = false;
								}
							}
						}
					}
				}

				//Now that both of the data got their matchs, draw the graph
				this.drawGraph(USLTrueDATA, " Unsupervised Learning");
			}
			//#############################unsupervised learning part ends#################

			//Finally populate the Resulting datagridview and the combobox1

			comboBox2.Invoke(new MethodInvoker(delegate
				{
					for (int i = 0; i < TF.Count; i++)
					{
						comboBox2.Items.Add(TF[i].TableName);
					}
					comboBox2.SelectedIndex = 0;
				}));
			dataGridView2.Invoke(new MethodInvoker(delegate
				{
					dataGridView2.DataSource = TF[0];
				}));
		}
		private void drawGraph(List<groupingResults>[] TrueDATA, String status)
		{
			for (int i = 0; i < TrueDATA.Count(); i++)
			{
				chart1.Invoke(new MethodInvoker(
					delegate
					{
						chart1.Series.Add(DeconData.SafeFileNames[i] + status);
						chart1.Series[DeconData.SafeFileNames[i] + status].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
						chart1.Series[DeconData.SafeFileNames[i] + status].Points.AddXY(0, 0);
						chart1.ChartAreas[0].AxisX.Title = "False Positive Rate";
						chart1.ChartAreas[0].AxisY.Title = "True Positive Rate";
					}));

				DataTable store = new DataTable();
				store.Columns.Add("Cutoff Score", typeof(Double));
				store.Columns.Add("False Positive Rate", typeof(Double));
				store.Columns.Add("True Positive Rate", typeof(Double));
				store.TableName = DeconData.SafeFileNames[i] + status;
				store.Rows.Add(1.001, 0, 0);
				List<groupingResults> True = TrueDATA[i].OrderByDescending(a => a.Score).ToList();

				Double TrueTotal = 0.000000000001;
				Double FalseTotal = 0.000000000001;


				for (int j = 0; j < True.Count; j++)
				{
					if (True[j].Match == true)
					{
						TrueTotal = TrueTotal + True[j].Score;
						//FalseTotal = FalseTotal + 1 - True[j].Score;
					}
					else
					{
						FalseTotal = FalseTotal + True[j].Score;
						//TrueTotal = TrueTotal + 1 - True[j].Score;
					}
				}
				Double TruePositive = 0.000000000001;
				Double FalsePositive = 0.000000000001;
				Double Step = 0.001;
				int NextStartj = 0;
				Double cutoff = 1;
				Boolean end = false;
				while (cutoff >= 0)
				{
					for (int j = NextStartj; j < True.Count; j++)
					{
						if (Convert.ToDouble(True[j].Score) < (cutoff))
						{
							NextStartj = j;
							break;
						}
						if (True[j].Match == true)
						{
							TruePositive = TruePositive + Convert.ToDouble(True[j].Score);
							//FalsePositive = FalsePositive + 1 - Convert.ToDouble(True[j].Score);
						}
						else
						{
							//TruePositive = TruePositive + 1 - Convert.ToDouble(True[j].Score);
							FalsePositive = FalsePositive + Convert.ToDouble(True[j].Score);
						}
						if (j == True.Count - 1)
							end = true;
					}

					Double TrueRate = 1;
					Double FalseRate = 1;
					TrueRate = Math.Round((TruePositive) / (TrueTotal), 6);
					FalseRate = Math.Round((FalsePositive) / (FalseTotal), 6);

					chart1.Invoke(new MethodInvoker(
						delegate
						{
							chart1.Series[DeconData.SafeFileNames[i] + status].Points.AddXY(FalseRate, TrueRate);
						}));

					store.Rows.Add(cutoff, FalseRate, TrueRate);

					//for some reason, if we simplay do minus 0.001 to cutoff for 4-5 times, it will gain lots of decimal points with 9s in them. So, we're doing it this way:
					cutoff = Math.Round(cutoff - Step, 3);
					if (end)
						break;
				}
				TF.Add(store);
			}
		}

		//This function draws the ROC curve by the TP and FP rates calculated from the false data set
		private void FDSBasedGraph()
		{
			composition comp = new composition();
			supervisedLearning SL = new supervisedLearning();
			Features FT = new Features();
			String currentpath = Application.StartupPath + "\\FeatureCurrent.fea";
			Feature Fea = FT.readFeature(currentpath);
			//Create the list of random composition hypotesis for testing FDR. 
			//ObtainTrue Position data.

			falseDataset fD = new falseDataset();
			List<composition.comphypo> falseCH = fD.genFalse(oFDcposTest.FileName, ComHyLink, oFDPPMSD);
			List<groupingResults>[] FalseDATA = SL.evaluate(DeconData, falseCH, Fea);

			this.drawGraph(FalseDATA, " ", 0);


			//Checkbox1 is default features.####################################
			List<groupingResults>[] DefaultFeature = new List<groupingResults>[DeconData.FileNames.Count()];
			String path = Application.StartupPath + "\\FeatureDefault.fea";
			Feature DeFea = FT.readFeature(path);
			if (checkBox1.Checked == true)
			{
				List<groupingResults>[] FalseDATADefault = SL.evaluate(DeconData, falseCH, DeFea);
				this.drawGraph(FalseDATADefault, " Default Features", 0);
			}

			//################################################
			//Checkbox2 is unsupervised Learning. It is a bit different from supervised learning, so it is hard-coded here.
			unsupervisedLearning UL = new unsupervisedLearning();
			if (checkBox2.Checked == true)
			{
				List<groupingResults>[] USLFalseDATA = UL.evaluate(DeconData, Fea);
				//ROC curve needs match to perform, so we will use the match list from Supervised learning and apply them to USLDATA.
				for (int i = 0; i < DeconData.FileNames.Count(); i++)
				{
					FalseDATA[i] = FalseDATA[i].OrderByDescending(a => a.DeconRow.monoisotopic_mw).ToList();
					USLFalseDATA[i] = USLFalseDATA[i].OrderByDescending(b => b.DeconRow.monoisotopic_mw).ToList();
					int USllasttruematch = 0;
					for (int j = 0; j < FalseDATA[i].Count; j++)
					{
						if (FalseDATA[i][j].Match == true)
						{
							for (int k = USllasttruematch; k < USLFalseDATA[i].Count; k++)
							{
								if (USLFalseDATA[i][k].DeconRow.monoisotopic_mw < FalseDATA[i][j].DeconRow.monoisotopic_mw)
								{
									USllasttruematch = k;
									break;
								}
								if (USLFalseDATA[i][k].DeconRow.monoisotopic_mw == FalseDATA[i][j].DeconRow.monoisotopic_mw)
								{
									USLFalseDATA[i][k].Match = true;
									USLFalseDATA[i][k].comphypo = FalseDATA[i][j].comphypo;
									USllasttruematch = k + 1;
									break;
								}
								if (USLFalseDATA[i][k].DeconRow.monoisotopic_mw > FalseDATA[i][j].DeconRow.monoisotopic_mw)
								{
									USLFalseDATA[i][k].Match = false;
								}
							}
						}
					}
				}

				//Now that both of the data got their matchs, draw the graph
				this.drawGraph(USLFalseDATA, " Unsupervised Learning", 0);
			}
			//#############################unsupervised learning part ends#################

			//Finally populate the Resulting datagridview and the combobox1

			comboBox2.Invoke(new MethodInvoker(delegate
				{
					for (int i = 0; i < TF.Count; i++)
					{
						comboBox2.Items.Add(TF[i].TableName);
					}
					comboBox2.SelectedIndex = 0;
				}));
			dataGridView2.Invoke(new MethodInvoker(delegate
				{
					dataGridView2.DataSource = TF[0];
				}));
		}
		private void drawGraph(List<groupingResults>[] FalseDATA, String status, int isfalse)
		{
			for (int i = 0; i < FalseDATA.Count(); i++)
			{
				chart1.Invoke(new MethodInvoker(
					delegate
					{
						chart1.Series.Add(DeconData.SafeFileNames[i] + status);
						chart1.Series[DeconData.SafeFileNames[i] + status].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
						chart1.Series[DeconData.SafeFileNames[i] + status].Points.AddXY(0, 0);
						chart1.ChartAreas[0].AxisX.Title = "False Positive Rate";
						chart1.ChartAreas[0].AxisY.Title = "True Positive Rate";
					}));

				DataTable store = new DataTable();
				store.Columns.Add("Cutoff Score", typeof(Double));
				store.Columns.Add("False Positive Rate", typeof(Double));
				store.Columns.Add("True Positive Rate", typeof(Double));
				store.TableName = DeconData.SafeFileNames[i] + status;
				store.Rows.Add(1.001, 0, 0);

				List<groupingResults> False = FalseDATA[i].OrderByDescending(b => b.Score).ToList();

				Double TrueTotal = 0.000000000001;
				Double FalseTotal = 0.000000000001;


				for (int j = 0; j < False.Count; j++)
				{
					if (False[j].Match == true)
					{
						if (False[j].comphypo.TrueOrFalse == false)
							FalseTotal = FalseTotal + False[j].Score;
						if (False[j].comphypo.TrueOrFalse == true)
							TrueTotal = TrueTotal + False[j].Score;
					}
				}
				Double TruePositive = 0.000000000001;
				Double FalsePositive = 0.000000000001;
				Double Step = 0.001;
				int NextStartjf = 0;
				Double cutoff = 1;
				Boolean endTrue = false;
				Boolean endFalse = false;
				while (cutoff >= 0)
				{

					for (int j = NextStartjf; j < False.Count; j++)
					{
						if (endFalse)
							break;
						if (Convert.ToDouble(False[j].Score) < (cutoff))
						{
							NextStartjf = j;
							break;
						}
						if (False[j].Match == true)
						{
							if (False[j].comphypo.TrueOrFalse == false)
								FalsePositive = FalsePositive + False[j].Score;
							if (False[j].comphypo.TrueOrFalse == true)
								TruePositive = TruePositive + False[j].Score;
						}
						else

							if (j == False.Count - 1)
								endFalse = true;
					}
					Double TrueRate = 1;
					Double FalseRate = 1;
					TrueRate = Math.Round((TruePositive) / (TrueTotal), 6);
					FalseRate = Math.Round((FalsePositive) / (FalseTotal), 6);

					chart1.Invoke(new MethodInvoker(
						delegate
						{
							chart1.Series[DeconData.SafeFileNames[i] + status].Points.AddXY(FalseRate, TrueRate);
						}));

					store.Rows.Add(cutoff, FalseRate, TrueRate);

					//for some reason, if we simplay do minus 0.001 to cutoff for 4-5 times, it will gain lots of decimal points with 9s in them. So, we're doing it this way:
					cutoff = Math.Round(cutoff - Step, 3);
					if (endTrue && endFalse)
						break;
				}
				TF.Add(store);
			}

		}


		//Change the datagridview when the combobox selection is changed
		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			dataGridView2.DataSource = TF[comboBox2.SelectedIndex];
		}


		//Tag4################################################################
		private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
		{
			drawTVhistogram();
		}
		private void checkBox3_CheckedChanged(object sender, EventArgs e)
		{
			drawTVhistogram();
		}
		private void checkBox4_CheckedChanged(object sender, EventArgs e)
		{
			drawTVhistogram();
		}
		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			drawTVhistogram();
		}




		//Tag3##################################################################
		//Add Result Files Button
		public bool Multiselect { get; set; }
		private void button3_Click(object sender, EventArgs e)
		{
			oFDResults.Filter = "csv files (*.csv)|*.csv";
			oFDResults.Multiselect = true;
			oFDResults.ShowDialog();
		}
		private void oFDResults_FileOk(object sender, CancelEventArgs e)
		{
			String fileInfo = "";
			Stream mystream = null;
			try
			{
				if ((mystream = oFDResults.OpenFile()) != null)
				{
					foreach (String file in oFDResults.FileNames)
					{
						fileInfo += String.Format("{0}\n", file);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
			}
			mystream.Close();
			richTextBox2.Font = new Font("Times New Roman", 12);
			richTextBox2.Text = fileInfo;
			button9.Enabled = true;
		}

		//Remove all files button
		private void button8_Click(object sender, EventArgs e)
		{
			oFDResults.FileName = String.Empty;
			richTextBox2.Text = String.Empty;
			button9.Enabled = false;
		}

		//Generate Combined Result button#########################################
		private void button9_Click(object sender, EventArgs e)
		{
			GroupingResults GR = new GroupingResults();
			List<groupingResults>[] results = new List<groupingResults>[oFDResults.FileNames.Count()];
			for (int i = 0; i < oFDResults.FileNames.Count(); i++)
			{
				results[i] = GR.readResultFile(oFDResults.FileNames[i]);
			}
			List<groupingResults> Answer = new List<groupingResults>();
			Answer = GR.combineResults(results);
			List<groupingResults>[] Ans = { Answer };
			DataTable DT = toDataTable(Ans, 0);
			dataGridView3.DataSource = DT;
		}

		//Save to File button
		private void button4_Click(object sender, EventArgs e)
		{
			sFDCR.Filter = "csv files (*.csv)|*.csv";
			sFDCR.ShowDialog();
		}
		private void sFDCR_FileOk(object sender, CancelEventArgs e)
		{
			saveDGV3ToFile(sFDCR.FileName);
		}



		//Other Functions
		//Function that turns the AllFinalResult data into DataTable
		private DataTable toDataTable(List<groupingResults>[] AFR, Int32 index)
		{
			//write.Write("Score,MassSpec MW,Compound Key,PPM Error,Hypothesis MW,#ofModificationStates,#ofCharges,#ofScans,Scan Density,Avg A:A+2 Error,A:A+2 Ratio,Total Volume,Signal to Noise Ratio,Centroid Scan Error,Centroid Scan,MaxScanNumber,MinScanNumber,C,H,N,O,S,P");

			List<groupingResults> current = AFR[index];
			DataTable output = new DataTable();
			output.Columns.Add("Score", typeof(Double));
			output.Columns.Add("MassSpec MW", typeof(Double));
			output.Columns.Add("Compositions", typeof(String));
			output.Columns.Add("PeptideSequence", typeof(String));
			output.Columns.Add("PPM Error", typeof(Double));
			output.Columns.Add("#ofAdduct", typeof(Double));
			output.Columns.Add("#ofCharges", typeof(Double));
			output.Columns.Add("#ofScans", typeof(Double));
			output.Columns.Add("ScanDensity", typeof(Double));
			output.Columns.Add("Avg A:A+2 Error", typeof(Double));
			output.Columns.Add("A:A+2 Ratio", typeof(Double));
			output.Columns.Add("Total Volume", typeof(Double));
			output.Columns.Add("Signal to Noise Ratio", typeof(Double));
			output.Columns.Add("Centroid Scan Error", typeof(Double));
			output.Columns.Add("Centroid Scan", typeof(Double));
			output.Columns.Add("MaxScanNumber", typeof(Double));
			output.Columns.Add("MinScanNumber", typeof(Double));
			int ElementCount = 0;
			ElementCount = elementIDs.Count();

			foreach (String name in elementIDs)
			{
				output.Columns.Add(name, typeof(Int32));                        
			}


			output.Columns.Add("Hypothesis MW", typeof(Double));
			int moleculeCount = 0;
			moleculeCount = molenames.Count();
			foreach (String name in molenames)
			{
				output.Columns.Add(name, typeof(Int32));                
			}

			output.Columns.Add("Adduct/Replacement", typeof(String));
			output.Columns.Add("Adduct Amount", typeof(Int32));
			output.Columns.Add("PeptideModification", typeof(String));
			output.Columns.Add("PeptideMissedCleavage#", typeof(Int32));
			output.Columns.Add("#ofGlycanAttachmentToPeptide", typeof(Int32));
			output.Columns.Add("StartAA", typeof(Int32));
			output.Columns.Add("EndAA", typeof(Int32));
			foreach (groupingResults ch in current)
			{
				if (ch.comphypo.MW != 0)
				{
					DataRow ab = output.NewRow();
					Double MatchingError = 0;
					if (ch.comphypo.MW != 0)
					{
						MatchingError = ((ch.DeconRow.monoisotopic_mw - ch.comphypo.MW)/(ch.DeconRow.monoisotopic_mw)) * 1000000 ;
					}
					ab[0] = ch.Score;
					ab[1] = ch.DeconRow.monoisotopic_mw;
					ab[2] = ch.comphypo.compoundCompo;
					ab[3] = ch.comphypo.PepSequence;
					ab[4] = MatchingError;                    
					ab[5] = ch.numModiStates;
					ab[6] = ch.numChargeStates;
					ab[7] = ch.numOfScan;
					ab[8] = ch.ScanDensity;
					ab[9] = ch.ExpectedA;
					ab[10] = (ch.DeconRow.mono_abundance / (ch.DeconRow.mono_plus2_abundance + 1));
					ab[11] = ch.totalVolume;
					ab[12] = ch.DeconRow.signal_noise;
					ab[13] = ch.CentroidScan;
					ab[14] = ch.DeconRow.scan_num;
					ab[15] = ch.maxScanNum;
					ab[16] = ch.minScanNum;
					int sh = 17;
					for (int s = 0; s < ElementCount; s++)
					{
						ab[sh + s] = ch.comphypo.elementAmount[s];
					}
					ab[sh + ElementCount] = ch.comphypo.MW;
					for (int s = 0; s < molenames.Count(); s++)
					{
						ab[sh + ElementCount + 1 + s] = ch.comphypo.eqCounts[s];
					}
					ab[sh + ElementCount + 1 + moleculeCount] = ch.comphypo.AddRep;
					ab[sh + ElementCount + 1 + moleculeCount + 1] = ch.comphypo.AdductNum;
					ab[sh + ElementCount + 1 + moleculeCount + 2] = ch.comphypo.PepModification;
					ab[sh + ElementCount + 1 + moleculeCount + 3] = ch.comphypo.MissedCleavages;
					ab[sh + ElementCount + 1 + moleculeCount + 4] = ch.comphypo.numGly;
					ab[sh + ElementCount + 1 + moleculeCount + 5] = ch.comphypo.StartAA;
					ab[sh + ElementCount + 1 + moleculeCount + 6] = ch.comphypo.EndAA;
					output.Rows.Add(ab);
				}
				else
				{
					DataRow ab = output.NewRow();
					Double MatchingError = 0;
					if (ch.comphypo.MW != 0)
					{
						MatchingError = ch.comphypo.MW - ch.DeconRow.monoisotopic_mw;
					}
					ab[0] = ch.Score;
					ab[1] = ch.DeconRow.monoisotopic_mw;
					ab[2] = ch.comphypo.compoundCompo;
					ab[3] = ch.comphypo.PepSequence;
					ab[4] = MatchingError;
					ab[5] = ch.numModiStates;
					ab[6] = ch.numChargeStates;
					ab[7] = ch.numOfScan;
					ab[8] = ch.ScanDensity;
					ab[9] = ch.ExpectedA;
					ab[10] = (ch.DeconRow.mono_abundance / (ch.DeconRow.mono_plus2_abundance + 1));
					ab[11] = ch.totalVolume;
					ab[12] = ch.DeconRow.signal_noise;
					ab[13] = ch.CentroidScan;
					ab[14] = ch.DeconRow.scan_num;
					ab[15] = ch.maxScanNum;
					ab[16] = ch.minScanNum;
					int sh = 17;
					for (int s = 0; s < ElementCount; s++)
					{
						ab[sh + s] = 0;
					}
					ab[sh + ElementCount] = ch.comphypo.MW;
					for (int s = 0; s < molenames.Count(); s++)
					{
						ab[sh + ElementCount + 1 + s] = 0;
					}
					ab[sh + ElementCount + 1 + moleculeCount] = "N/A";
					ab[sh + ElementCount + 1 + moleculeCount + 1] = 0;
					ab[sh + ElementCount + 1 + moleculeCount + 2] = "";
					ab[sh + ElementCount + 1 + moleculeCount + 3] = 0;
					ab[sh + ElementCount + 1 + moleculeCount + 4] = 0;
					ab[sh + ElementCount + 1 + moleculeCount + 5] = 0;
					ab[sh + ElementCount + 1 + moleculeCount + 6] = 0;
					output.Rows.Add(ab);
				}
			}
			return output;
		}

		//This function saves the datagridview3 (combine glycan data) to a csv file
		private void saveDGV3ToFile(String currentPath)
		{
			//First, prepare the writers.
			FileStream FS = new FileStream(currentPath, FileMode.Create, FileAccess.Write);
			StreamWriter write = new StreamWriter(FS);

			//Second,  loop from the datagridview and write to csv file.
			for (int j = 0; j < this.dataGridView3.ColumnCount; ++j)
			{
				String cell = this.dataGridView3.Columns[j].HeaderText;
				if (j == (this.dataGridView3.ColumnCount - 1))
				{
					write.WriteLine(cell);
				}
				else
				{
					write.Write(cell + ",");
				}
			}
			int iii = 0;
			int jjj = 0;
			try
			{
				for (int i = 0; i < this.dataGridView3.RowCount; ++i)
				{
					iii = 1;
					for (int j = 0; j < this.dataGridView3.ColumnCount; ++j)
					{
						jjj = j;
						var cell = this.dataGridView3.Rows[i].Cells[j].Value;
						if (j == (this.dataGridView3.ColumnCount - 1))
						{
							write.WriteLine(cell);
						}
						else
						{
							write.Write(cell + ",");
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("i:" + Convert.ToString(iii) + " j:" + Convert.ToString(jjj) + " error " + ex);
			}

			write.Flush();
			write.Close();
			FS.Close();
		}

		//This function draws the percent total volume histogram, depending on the checkboxes.
		private void drawTVhistogram()
		{
			//This is matched compounds only.
			if (checkBox4.Checked == true)
			{
				foreach (var series in chart2.Series)
				{
					series.Points.Clear();
				}
				chart2.Series.Clear();
				List<groupingResults> dataset = new List<groupingResults>();
				dataset = AllFinalResults[comboBox3.SelectedIndex];
				dataset = dataset.OrderByDescending(a => a.comphypo.MW).ToList();
				//Getting total of total volumes
				Double TotalTV = 0;
				for (int i = 0; i < dataset.Count(); i++)
				{
					TotalTV = TotalTV + dataset[i].totalVolume;
				}
				List<Double> percentages = new List<Double>();
				List<string> rownames = new List<string>();
				//Limit number of data to n if checkBox 3 is checked. Else, no limit.
				if (checkBox3.Checked == true && dataset.Count() > numericUpDown1.Value)
				{
					dataset = dataset.OrderByDescending(a => a.Score).ToList();
					List<groupingResults> newdataset = new List<groupingResults>();
					int count = 0;
					int i7 = 0;
					while (count < numericUpDown1.Value)
					{
						if (dataset[i7].Match == true)
						{
							newdataset.Add(dataset[i7]);
							i7++;
							count++;
							continue;
						}
						i7++;
						if (i7 >= dataset.Count())
							break;
					}
					//output to chart
					newdataset = newdataset.OrderByDescending(a => a.comphypo.MW).ToList();
					for (int i = 0; i < newdataset.Count(); i++)
					{
						if (newdataset[i].comphypo.MW == 0)
							break;
						percentages.Add((newdataset[i].totalVolume / TotalTV) * 100);
						rownames.Add(Convert.ToString(newdataset[i].comphypo.compoundCompo));

					}
				}
				else
				{
					for (int i = 0; i < dataset.Count(); i++)
					{
						if (dataset[i].comphypo.MW == 0)
							break;

						percentages.Add((dataset[i].totalVolume / TotalTV) * 100);
						rownames.Add(Convert.ToString(dataset[i].comphypo.compoundCompo));

					}
				}
				if (dataset.Count() != 0)
				{
					String titleName = "";
					dataset = dataset.OrderByDescending(a => a.comphypo.compoundCompo).ToList();
					if (molenames.Count() == 0)
					{
						titleName = "Composition: N/A";
					}
					else
					{
						titleName = "Composition: ";
						titleName = titleName + String.Join(", ", molenames);
					}
					chart2.Series.Add(DeconData.FileNames[comboBox3.SelectedIndex]);
					chart2.ChartAreas[0].AxisX.Title = titleName;
					for (int i = 0; i < percentages.Count(); i++)
					{
						chart2.Series[DeconData.FileNames[comboBox3.SelectedIndex]].Points.AddXY(rownames[i], percentages[i]);
					}
				}
			}
			//This includes unmatched compounds:
			else
			{
				foreach (var series in chart2.Series)
				{
					series.Points.Clear();
				}
				chart2.Series.Clear();
				List<groupingResults> dataset = new List<groupingResults>();
				dataset = AllFinalResults[comboBox3.SelectedIndex];
				//Getting total of total volumes
				Double TotalTV = 0;
				for (int i = 0; i < dataset.Count(); i++)
				{
					TotalTV = TotalTV + dataset[i].totalVolume;
				}
				List<Double> percentages = new List<Double>();
				List<string> rownames = new List<string>();
				//Limit number of data to n if checkBox 3 is checked. Else, no limit.
				if (checkBox3.Checked == true && dataset.Count() > numericUpDown1.Value)
				{
					dataset = dataset.OrderBy(a => a.Score).ToList();
					List<groupingResults> newdataset = new List<groupingResults>();
					int count = 0;
					int i7 = 0;
					while (count < numericUpDown1.Value)
					{
						newdataset.Add(dataset[i7]);
						i7++;
						count++;
						continue;
					}
					//output to chart
					newdataset = newdataset.OrderBy(a => a.DeconRow.scan_num).ToList();
					for (int i = 0; i < newdataset.Count(); i++)
					{
						percentages.Add((newdataset[i].totalVolume / TotalTV) * 100);
						rownames.Add(Convert.ToString(newdataset[i].DeconRow.scan_num));

					}
				}
				else
				{
					dataset = dataset.OrderBy(a => a.DeconRow.scan_num).ToList();
					for (int i = 0; i < dataset.Count(); i++)
					{
						percentages.Add((dataset[i].totalVolume / TotalTV) * 100);
						rownames.Add(Convert.ToString(dataset[i].DeconRow.scan_num));
					}
				}
				String titleName = "Scan Number";
				chart2.Series.Add(DeconData.FileNames[comboBox3.SelectedIndex]);
				chart2.ChartAreas[0].AxisX.Title = titleName;
				for (int i = 0; i < percentages.Count(); i++)
				{
					chart2.Series[DeconData.FileNames[comboBox3.SelectedIndex]].Points.AddXY(rownames[i], percentages[i]);
				}
			}
		}




	}


}

