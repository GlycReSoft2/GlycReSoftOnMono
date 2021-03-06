﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlycReSoft
{
	class PeriodicTable
	{
		private static Dictionary<String,Double> pTable = new Dictionary<String,Double>();        
		static PeriodicTable(){
			//The AME 2012 atomic mass evaluation, M. Wang, G. Augi, A. H. Wapstra, F. G. Kondev, M. MacCormick, X. Xu, and B. Pfeiffer. Chinese Phys. C 36, 1603 (2012).{AME 2012}
			pTable.Add("H", 1.0078250320);
			pTable.Add("H+", 1.00727646677);
			pTable.Add("D", 2.014101778);
			pTable.Add("He", 4.002603254);
			pTable.Add("He(3)", 3.016029319);
			pTable.Add("Li(6)", 6.015122795);
			pTable.Add("Li", 7.01600455);
			pTable.Add("Be", 9.0121822);
			pTable.Add("B(10)", 10.012937);
			pTable.Add("B",11.0093054);
			pTable.Add("C", 12.0000000000);
			pTable.Add("C(13)", 13.00335484);
			pTable.Add("N", 14.0030740000);
			pTable.Add("N(15)", 15.0001089);
			pTable.Add("O", 15.9949146200);
			pTable.Add("O(17)", 16.9991317);
			pTable.Add("O(18)", 17.999161);
			pTable.Add("F", 18.9984032200);
			pTable.Add("Ne", 19.99244018);
			pTable.Add("Ne(21)", 20.99384668);
			pTable.Add("Ne(22)", 21.99138511);
			pTable.Add("Na", 22.98976928);
			pTable.Add("Mg", 23.9850417);
			pTable.Add("Mg(25)", 24.98583692);
			pTable.Add("Mg(26)", 25.98259293);
			pTable.Add("Al", 26.98153863);
			pTable.Add("Si", 27.97692653);
			pTable.Add("Si(29)", 28.9764947);
			pTable.Add("Si(30)", 29.97377017);
			pTable.Add("P", 30.97376163);
			pTable.Add("S", 31.972071);
			pTable.Add("S(33)", 32.9714876);
			pTable.Add("S(34)", 33.9678669);
			pTable.Add("S(36)", 35.96708076);
			pTable.Add("Cl", 34.96885268);
			pTable.Add("Cl(37)", 36.96590259);
			pTable.Add("Ar(36)", 35.96754511);
			pTable.Add("Ar(38)", 37.9627324);
			pTable.Add("Ar", 39.96238312);
			pTable.Add("K", 38.96370668);
			pTable.Add("K(40)", 39.96399848);
			pTable.Add("K(41)", 40.96182576);
			pTable.Add("Ca", 39.96259098);
			pTable.Add("Ca(42)", 41.95861801);
			pTable.Add("Ca(43)", 42.9587666);
			pTable.Add("Ca(44)", 43.9554818);
			pTable.Add("Ca(46)", 45.9536926);
			pTable.Add("Ca(48)", 47.952534);
			pTable.Add("Sc", 44.9559119);
			pTable.Add("Ti(46)", 45.9526316);
			pTable.Add("Ti(47)", 46.9517631);
			pTable.Add("Ti", 47.9479463);
			pTable.Add("Ti(49)", 48.94797);
			pTable.Add("Ti(50)", 49.9447912);
			pTable.Add("V(50)", 49.9471585);
			pTable.Add("V", 50.9439595);
			pTable.Add("Cr(50)", 49.9460442);
			pTable.Add("Cr", 51.9405075);
			pTable.Add("Cr(53)", 52.9406494);
			pTable.Add("Cr(54)", 53.9388804);
			pTable.Add("Mn", 54.9380451);
			pTable.Add("Fe(54)", 53.9396105);
			pTable.Add("Fe", 55.9349375);
			pTable.Add("Fe(57)", 56.935394);
			pTable.Add("Fe(58)", 57.9332756);
			pTable.Add("Co", 58.933195);
			pTable.Add("Ni", 57.9353429);
			pTable.Add("Ni(60)", 59.9307864);
			pTable.Add("Ni(61)", 60.931056);
			pTable.Add("Ni(62)", 61.9283451);
			pTable.Add("Ni(64)", 63.927966);
			pTable.Add("Cu", 62.9295975);
			pTable.Add("Cu(65)", 64.9277895);
			pTable.Add("Zn", 63.9291422);
			pTable.Add("Zn(66)", 65.9260334);
			pTable.Add("Zn(67)", 66.9271273);
			pTable.Add("Zn(68)", 67.9248442);
			pTable.Add("Zn(70)", 69.9253193);
			pTable.Add("Ga", 68.9255736);
			pTable.Add("Ga(71)", 70.9247013);
			pTable.Add("Ge(70)", 69.9242474);
			pTable.Add("Ge(72)", 71.9220758);
			pTable.Add("Ge(73)", 72.9234589);
			pTable.Add("Ge", 73.9211778);
			pTable.Add("Ge(76)", 75.9214026);
			pTable.Add("As", 74.9215965);
			pTable.Add("Se(74)", 73.9224764);
			pTable.Add("Se(76)", 75.9192136);
			pTable.Add("Se(77)", 76.919914);
			pTable.Add("Se(78)", 77.9173091);
			pTable.Add("Se", 79.9165213);
			pTable.Add("Se(82)", 81.9166994);
			pTable.Add("Br", 78.9183371);
			pTable.Add("Br(81)", 80.9162906);
			pTable.Add("Kr(78)", 77.9203648);
			pTable.Add("Kr(80)", 79.916379);
			pTable.Add("Kr(82)", 81.9134836);
			pTable.Add("Kr(83)", 82.914136);
			pTable.Add("Kr", 83.911507);
			pTable.Add("Kr(86)", 85.91061073);
			pTable.Add("Rb", 84.91178974);
			pTable.Add("Rb(87)", 86.90918053);
			pTable.Add("Sr(84)", 83.913425);
			pTable.Add("Sr(86)", 85.9092602);
			pTable.Add("Sr(87)", 86.9088771);
			pTable.Add("Sr", 87.9056121);
			pTable.Add("Y", 88.9058483);
			pTable.Add("Zr", 89.9047044);
			pTable.Add("Zr(91)", 90.9056458);
			pTable.Add("Zr(92)", 91.9050408);
			pTable.Add("Zr(94)", 93.9063152);
			pTable.Add("Zr(96)", 95.9082734);
			pTable.Add("Nb", 92.9063781);
			pTable.Add("Mo(92)", 91.906811);
			pTable.Add("Mo(94)", 93.9050883);
			pTable.Add("Mo(95)", 94.9058421);
			pTable.Add("Mo(96)", 95.9046795);
			pTable.Add("Mo(97)", 96.9060215);
			pTable.Add("Mo", 97.9054082);
			pTable.Add("Mo(100)", 99.907477);
			pTable.Add("Tc", 98.9062);
			pTable.Add("Ru(96)", 95.907598);
			pTable.Add("Ru(98)", 97.905287);
			pTable.Add("Ru(99)", 98.9059393);
			pTable.Add("Ru(100)", 99.9042195);
			pTable.Add("Ru(101)", 100.9055821);
			pTable.Add("Ru", 101.9043493);
			pTable.Add("Ru(104)", 103.905433);
			pTable.Add("Rh", 102.905504);
			pTable.Add("Pd(102)", 101.905609);
			pTable.Add("Pd(104)", 103.904036);
			pTable.Add("Pd(105)", 104.905085);
			pTable.Add("Pd", 105.903486);
			pTable.Add("Pd(108)", 107.903892);
			pTable.Add("Pd(110)", 109.905153);
			pTable.Add("Ag", 106.905097);
			pTable.Add("Ag(109)", 108.904752);
			pTable.Add("Cd(106)", 105.906459);
			pTable.Add("Cd(108)", 107.904184);
			pTable.Add("Cd(110)", 109.9030021);
			pTable.Add("Cd(111)", 110.9041781);
			pTable.Add("Cd(112)", 111.9027578);
			pTable.Add("Cd(113)", 112.9044017);
			pTable.Add("Cd", 113.9033585);
			pTable.Add("Cd(116)", 115.904756);
			pTable.Add("In(113)", 112.904058);
			pTable.Add("In", 114.903878);
			pTable.Add("Sn(112)",111.904818);
			pTable.Add("Sn(114)",113.902779);
			pTable.Add("Sn(115)",114.903342);
			pTable.Add("Sn(116)",115.901741);
			pTable.Add("Sn(117)",116.902952);
			pTable.Add("Sn(118)",117.901603);
			pTable.Add("Sn(119)",118.903308);
			pTable.Add("Sn",119.9021947);
			pTable.Add("Sn(122)",121.903439);
			pTable.Add("Sn(124)",123.9052739);
			pTable.Add("Sb", 120.9038157);
			pTable.Add("Sb(123)", 122.904214);
			pTable.Add("Te(120)", 119.90402);
			pTable.Add("Te(122)", 121.9030439);
			pTable.Add("Te(123)", 122.90427);
			pTable.Add("Te(124)", 123.9028179);
			pTable.Add("Te(125)", 124.9044307);
			pTable.Add("Te(126)", 125.9033117);
			pTable.Add("Te(128)", 127.9044631);
			pTable.Add("Te", 129.9062244);
			pTable.Add("I", 126.9044730000);
			pTable.Add("Xe(124)", 123.905893);
			pTable.Add("Xe(126)", 125.904274);
			pTable.Add("Xe(128)", 127.9035313);
			pTable.Add("Xe(129)", 128.9047794);
			pTable.Add("Xe(130)", 129.903508);
			pTable.Add("Xe(131)", 130.9050824);
			pTable.Add("Xe", 131.9041535);
			pTable.Add("Xe(134)", 133.9053945);
			pTable.Add("Xe(136)", 135.907219);
			pTable.Add("Cs", 132.9054519);
			pTable.Add("Ba(130)", 129.9063208);
			pTable.Add("Ba(132)", 131.9050613);
			pTable.Add("Ba(134)", 133.9045084);
			pTable.Add("Ba(135)", 134.9056886);
			pTable.Add("Ba(136)", 135.9045759);
			pTable.Add("Ba(137)", 136.9058274);
			pTable.Add("Ba", 137.9052472);
			pTable.Add("La(138)", 137.907112);
			pTable.Add("La", 138.9063533);
			pTable.Add("Ce(136)", 135.907172);
			pTable.Add("Ce(138)", 137.905991);
			pTable.Add("Ce", 139.9054387);
			pTable.Add("Ce(142)", 141.909244);
			pTable.Add("Pr", 140.9076528);
			pTable.Add("Nd", 141.9077233);
			pTable.Add("Nd(143)", 142.9098143);
			pTable.Add("Nd(144)", 143.9100873);
			pTable.Add("Nd(145)", 144.9125736);
			pTable.Add("Nd(146)", 145.9131169);
			pTable.Add("Nd(148)", 147.916893);
			pTable.Add("Nd(150)", 149.920891);
			pTable.Add("Sm(144)", 143.911999);
			pTable.Add("Sm(147)", 146.9148979);
			pTable.Add("Sm(148)", 147.9148227);
			pTable.Add("Sm(149)", 148.9171847);
			pTable.Add("Sm(150)", 149.9172755);
			pTable.Add("Sm", 151.9197324);
			pTable.Add("Sm(154)", 153.9222093);
			pTable.Add("Eu(151)", 150.9198502);
			pTable.Add("Eu", 152.9212303);
			pTable.Add("Gd(152)", 151.919791);
			pTable.Add("Gd(154)", 153.9208656);
			pTable.Add("Gd(155)", 154.922622);
			pTable.Add("Gd(156)", 155.9221227);
			pTable.Add("Gd(157)", 156.9239601);
			pTable.Add("Gd", 157.9241039);
			pTable.Add("Gd(160)", 159.9270541);
			pTable.Add("Tb", 158.9253468);
			pTable.Add("Dy(156)", 155.924283);
			pTable.Add("Dy(158)", 157.924409);
			pTable.Add("Dy(160)", 159.9251975);
			pTable.Add("Dy(161)", 160.9269334);
			pTable.Add("Dy(162)", 161.9267984);
			pTable.Add("Dy(163)", 162.9287312);
			pTable.Add("Dy", 163.9291748);
			pTable.Add("Ho", 164.9303221);
			pTable.Add("Er(162)", 161.928778);
			pTable.Add("Er(164)", 163.9292);
			pTable.Add("Er", 165.9302931);
			pTable.Add("Er(167)", 166.9320482);
			pTable.Add("Er(168)", 167.9323702);
			pTable.Add("Er(170)", 169.9354643);
			pTable.Add("Tm", 168.9342133);
			pTable.Add("Yb(168)", 167.933897);
			pTable.Add("Yb(170)", 169.9347618);
			pTable.Add("Yb(171)", 170.9363258);
			pTable.Add("Yb(172)", 171.9363815);
			pTable.Add("Yb(173)", 172.9382108);
			pTable.Add("Yb", 173.9388621);
			pTable.Add("Yb(176)", 175.9425717);
			pTable.Add("Lu", 174.9407718);
			pTable.Add("Lu(176)", 175.9426863);
			pTable.Add("Hf(174)", 173.940046);
			pTable.Add("Hf(176)", 175.9414086);
			pTable.Add("Hf(177)", 176.9432207);
			pTable.Add("Hf(178)", 177.9436988);
			pTable.Add("Hf(179)", 178.9458161);
			pTable.Add("Hf", 179.94655);
			pTable.Add("Ta(180)", 179.9474648);
			pTable.Add("Ta", 180.9479958);
			pTable.Add("W(180)", 179.946704);
			pTable.Add("W(182)", 181.9482042);
			pTable.Add("W(183)", 182.950223);
			pTable.Add("W", 183.9509312);
			pTable.Add("W(186)", 185.9543641);
			pTable.Add("Re(185)", 184.952955);
			pTable.Add("Re", 186.9557531);
			pTable.Add("Os(184)", 183.9524891);
			pTable.Add("Os(186)", 185.9538382);
			pTable.Add("Os(187)", 186.9557505);
			pTable.Add("Os(188)", 187.9558382);
			pTable.Add("Os(189)", 188.9581475);
			pTable.Add("Os(190)", 189.958447);
			pTable.Add("Os", 191.9614807);
			pTable.Add("Ir(191)", 190.960594);
			pTable.Add("Ir", 192.9629264);
			pTable.Add("Pt(190)", 189.959932);
			pTable.Add("Pt(192)", 191.961038);
			pTable.Add("Pt(194)", 193.9626803);
			pTable.Add("Pt", 194.9647911);
			pTable.Add("Pt(196)", 195.9649515);
			pTable.Add("Pt(198)", 197.967893);
			pTable.Add("Au", 196.966560);
			pTable.Add("Hg(196)", 195.965833);
			pTable.Add("Hg(198)", 197.966769);
			pTable.Add("Hg(199)", 198.9682799);
			pTable.Add("Hg(200)", 199.968326);
			pTable.Add("Hg(201)", 200.9703023);
			pTable.Add("Hg", 201.970643);
			pTable.Add("Hg(204)", 203.9734939);
			pTable.Add("Tl(203)", 202.9723442);
			pTable.Add("Tl", 204.9744275);
			pTable.Add("Pb(204)", 203.9730436);
			pTable.Add("Pb(206)", 205.9744653);
			pTable.Add("Pb(207)", 206.9758969);
			pTable.Add("Pb", 207.9766521);
			pTable.Add("Bi", 208.9803987);
			pTable.Add("Th", 232.038054);
			pTable.Add("Pa", 231.035884);
			pTable.Add("U(234)", 234.0409521);
			pTable.Add("U(235)", 235.0439299);
			pTable.Add("U", 238.0507882);

		}

		public Double getMass(String ele) { return pTable[ele]; }
		public List<String> getElements() { return pTable.Keys.ToList(); }
	}
}
