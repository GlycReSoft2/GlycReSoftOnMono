using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlycReSoft
{
	class AminoAcidMass
	{
		private static Dictionary<String,Double> AAMass = new Dictionary<String,Double>();        
		static AminoAcidMass(){
			AAMass.Add("G", 57.021464);
			AAMass.Add("A", 71.037114);
			AAMass.Add("S", 87.032029);
			AAMass.Add("P", 97.052764);
			AAMass.Add("V", 99.068414);
			AAMass.Add("T", 101.04768);
			AAMass.Add("C", 103.00919);
			AAMass.Add("L", 113.08406);
			AAMass.Add("I", 113.08406);
			AAMass.Add("N", 114.04293);
			AAMass.Add("D", 115.02694);
			AAMass.Add("Q", 128.05858);
			AAMass.Add("K", 128.09496);
			AAMass.Add("E", 129.04259);
			AAMass.Add("M", 131.04048);
			AAMass.Add("H", 137.05891);
			AAMass.Add("F", 147.06841);
			AAMass.Add("R", 156.10111);
			AAMass.Add("Y", 163.06333);
			AAMass.Add("W", 186.07931);
		}

		public Double getMass(String AA) { return AAMass[AA]; }
	}
}

