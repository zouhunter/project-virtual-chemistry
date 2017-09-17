
using UnityEngine;

[ExecuteInEditMode]
public class MegaFFDAnimate : MonoBehaviour
{
	bool record = false;
	public bool		Enabled = true;
	MegaFFD ffd;

	public Vector3	p00;
	public Vector3	p01;
	public Vector3	p02;
	public Vector3	p03;
	public Vector3	p04;
	public Vector3	p05;
	public Vector3	p06;
	public Vector3	p07;
	public Vector3	p08;
	public Vector3	p09;
	public Vector3	p10;
	public Vector3	p11;
	public Vector3	p12;
	public Vector3	p13;
	public Vector3	p14;
	public Vector3	p15;
	public Vector3	p16;
	public Vector3	p17;
	public Vector3	p18;
	public Vector3	p19;
	public Vector3	p20;
	public Vector3	p21;
	public Vector3	p22;
	public Vector3	p23;
	public Vector3	p24;
	public Vector3	p25;
	public Vector3	p26;
	public Vector3	p27;
	public Vector3	p28;
	public Vector3	p29;
	public Vector3	p30;
	public Vector3	p31;
	public Vector3	p32;
	public Vector3	p33;
	public Vector3	p34;
	public Vector3	p35;
	public Vector3	p36;
	public Vector3	p37;
	public Vector3	p38;
	public Vector3	p39;
	public Vector3	p40;
	public Vector3	p41;
	public Vector3	p42;
	public Vector3	p43;
	public Vector3	p44;
	public Vector3	p45;
	public Vector3	p46;
	public Vector3	p47;
	public Vector3	p48;
	public Vector3	p49;
	public Vector3	p50;
	public Vector3	p51;
	public Vector3	p52;
	public Vector3	p53;
	public Vector3	p54;
	public Vector3	p55;
	public Vector3	p56;
	public Vector3	p57;
	public Vector3	p58;
	public Vector3	p59;
	public Vector3	p60;
	public Vector3	p61;
	public Vector3	p62;
	public Vector3	p63;


	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2270");
	}

	public MegaFFD GetFFD()
	{
		return ffd;
	}

	public bool GetRecord()
	{
		return record;
	}

	public void SetRecord(bool rec)
	{
		record = rec;
	}

	public void GetPoints()
	{
		if ( ffd )
		{
			p00 = ffd.pt[0];
			p01 = ffd.pt[1];
			p02 = ffd.pt[2];
			p03 = ffd.pt[3];
			p04 = ffd.pt[4];
			p05 = ffd.pt[5];
			p06 = ffd.pt[6];
			p07 = ffd.pt[7];

			if ( ffd.GridSize() > 2 )
			{
				p08 = ffd.pt[8];
				p09 = ffd.pt[9];
				p10 = ffd.pt[10];
				p11 = ffd.pt[11];
				p12 = ffd.pt[12];
				p13 = ffd.pt[13];
				p14 = ffd.pt[14];
				p15 = ffd.pt[15];
				p16 = ffd.pt[16];
				p17 = ffd.pt[17];
				p18 = ffd.pt[18];
				p19 = ffd.pt[19];
				p20 = ffd.pt[20];
				p21 = ffd.pt[21];
				p22 = ffd.pt[22];
				p23 = ffd.pt[23];
				p24 = ffd.pt[24];
				p25 = ffd.pt[25];
				p26 = ffd.pt[26];

				if ( ffd.GridSize() > 3 )
				{
					p27 = ffd.pt[27];
					p28 = ffd.pt[28];
					p29 = ffd.pt[29];
					p30 = ffd.pt[30];
					p31 = ffd.pt[31];
					p32 = ffd.pt[32];
					p33 = ffd.pt[33];
					p34 = ffd.pt[34];
					p35 = ffd.pt[35];
					p36 = ffd.pt[36];
					p37 = ffd.pt[37];
					p38 = ffd.pt[38];
					p39 = ffd.pt[39];
					p40 = ffd.pt[40];
					p41 = ffd.pt[41];
					p42 = ffd.pt[42];
					p43 = ffd.pt[43];
					p44 = ffd.pt[44];
					p45 = ffd.pt[45];
					p46 = ffd.pt[46];
					p47 = ffd.pt[47];
					p48 = ffd.pt[48];
					p49 = ffd.pt[49];
					p50 = ffd.pt[50];
					p51 = ffd.pt[51];
					p52 = ffd.pt[52];
					p53 = ffd.pt[53];
					p54 = ffd.pt[54];
					p55 = ffd.pt[55];
					p56 = ffd.pt[56];
					p57 = ffd.pt[57];
					p58 = ffd.pt[58];
					p59 = ffd.pt[59];
					p60 = ffd.pt[60];
					p61 = ffd.pt[61];
					p62 = ffd.pt[62];
					p63 = ffd.pt[63];
				}
			}
		}
	}

	public void SetPoints()
	{
		if ( ffd )
		{
			ffd.pt[0] = p00;
			ffd.pt[1] = p01;
			ffd.pt[2] = p02;
			ffd.pt[3] = p03;
			ffd.pt[4] = p04;
			ffd.pt[5] = p05;
			ffd.pt[6] = p06;
			ffd.pt[7] = p07;

			if ( ffd.GridSize() > 2 )
			{
				ffd.pt[8] = p08;
				ffd.pt[9] = p09;
				ffd.pt[10] = p10;
				ffd.pt[11] = p11;
				ffd.pt[12] = p12;
				ffd.pt[13] = p13;
				ffd.pt[14] = p14;
				ffd.pt[15] = p15;
				ffd.pt[16] = p16;
				ffd.pt[17] = p17;
				ffd.pt[18] = p18;
				ffd.pt[19] = p19;
				ffd.pt[20] = p20;
				ffd.pt[21] = p21;
				ffd.pt[22] = p22;
				ffd.pt[23] = p23;
				ffd.pt[24] = p24;
				ffd.pt[25] = p25;
				ffd.pt[26] = p26;

				if ( ffd.GridSize() > 3 )
				{
					ffd.pt[27] = p27;
					ffd.pt[28] = p28;
					ffd.pt[29] = p29;
					ffd.pt[30] = p30;
					ffd.pt[31] = p31;
					ffd.pt[32] = p32;
					ffd.pt[33] = p33;
					ffd.pt[34] = p34;
					ffd.pt[35] = p35;
					ffd.pt[36] = p36;
					ffd.pt[37] = p37;
					ffd.pt[38] = p38;
					ffd.pt[39] = p39;
					ffd.pt[40] = p40;
					ffd.pt[41] = p41;
					ffd.pt[42] = p42;
					ffd.pt[43] = p43;
					ffd.pt[44] = p44;
					ffd.pt[45] = p45;
					ffd.pt[46] = p46;
					ffd.pt[47] = p47;
					ffd.pt[48] = p48;
					ffd.pt[49] = p49;
					ffd.pt[50] = p50;
					ffd.pt[51] = p51;
					ffd.pt[52] = p52;
					ffd.pt[53] = p53;
					ffd.pt[54] = p54;
					ffd.pt[55] = p55;
					ffd.pt[56] = p56;
					ffd.pt[57] = p57;
					ffd.pt[58] = p58;
					ffd.pt[59] = p59;
					ffd.pt[60] = p60;
					ffd.pt[61] = p61;
					ffd.pt[62] = p62;
					ffd.pt[63] = p63;
				}
			}
		}
	}

	void Start()
	{
		ffd = GetComponent<MegaFFD>();

		GetPoints();
	}

	public void SetPoint(int index, Vector3 p)
	{
		switch ( index )
		{
			case 0: p00 = p; break;
			case 1: p01 = p; break;
			case 2: p02 = p; break;
			case 3: p03 = p; break;
			case 4: p04 = p; break;
			case 5: p05 = p; break;
			case 6: p06 = p; break;
			case 7: p07 = p; break;
			case 8: p08 = p; break;
			case 9: p09 = p; break;
			case 10: p10 = p; break;
			case 11: p11 = p; break;
			case 12: p12 = p; break;
			case 13: p13 = p; break;
			case 14: p14 = p; break;
			case 15: p15 = p; break;
			case 16: p16 = p; break;
			case 17: p17 = p; break;
			case 18: p18 = p; break;
			case 19: p19 = p; break;
			case 20: p20 = p; break;
			case 21: p21 = p; break;
			case 22: p22 = p; break;
			case 23: p23 = p; break;
			case 24: p24 = p; break;
			case 25: p25 = p; break;
			case 26: p26 = p; break;
			case 27: p27 = p; break;
			case 28: p28 = p; break;
			case 29: p29 = p; break;
			case 30: p30 = p; break;
			case 31: p31 = p; break;
			case 32: p32 = p; break;
			case 33: p33 = p; break;
			case 34: p34 = p; break;
			case 35: p35 = p; break;
			case 36: p36 = p; break;
			case 37: p37 = p; break;
			case 38: p38 = p; break;
			case 39: p39 = p; break;
			case 40: p40 = p; break;
			case 41: p41 = p; break;
			case 42: p42 = p; break;
			case 43: p43 = p; break;
			case 44: p44 = p; break;
			case 45: p45 = p; break;
			case 46: p46 = p; break;
			case 47: p47 = p; break;
			case 48: p48 = p; break;
			case 49: p49 = p; break;
			case 50: p50 = p; break;
			case 51: p51 = p; break;
			case 52: p52 = p; break;
			case 53: p53 = p; break;
			case 54: p54 = p; break;
			case 55: p55 = p; break;
			case 56: p56 = p; break;
			case 57: p57 = p; break;
			case 58: p58 = p; break;
			case 59: p59 = p; break;
			case 60: p60 = p; break;
			case 61: p61 = p; break;
			case 62: p62 = p; break;
			case 63: p63 = p; break;
		}
	}

	public Vector3 GetPoint(int index)
	{
		switch ( index )
		{
			case 0: return p00;
			case 1: return p01;
			case 2: return p02;
			case 3: return p03;
			case 4: return p04;
			case 5: return p05;
			case 6: return p06;
			case 7: return p07;
			case 8: return p08;
			case 9: return p09;
			case 10: return p10;
			case 11: return p11;
			case 12: return p12;
			case 13: return p13;
			case 14: return p14;
			case 15: return p15;
			case 16: return p16;
			case 17: return p17;
			case 18: return p18;
			case 19: return p19;
			case 20: return p20;
			case 21: return p21;
			case 22: return p22;
			case 23: return p23;
			case 24: return p24;
			case 25: return p25;
			case 26: return p26;
			case 27: return p27;
			case 28: return p28;
			case 29: return p29;
			case 30: return p30;
			case 31: return p31;
			case 32: return p32;
			case 33: return p33;
			case 34: return p34;
			case 35: return p35;
			case 36: return p36;
			case 37: return p37;
			case 38: return p38;
			case 39: return p39;
			case 40: return p40;
			case 41: return p41;
			case 42: return p42;
			case 43: return p43;
			case 44: return p44;
			case 45: return p45;
			case 46: return p46;
			case 47: return p47;
			case 48: return p48;
			case 49: return p49;
			case 50: return p50;
			case 51: return p51;
			case 52: return p52;
			case 53: return p53;
			case 54: return p54;
			case 55: return p55;
			case 56: return p56;
			case 57: return p57;
			case 58: return p58;
			case 59: return p59;
			case 60: return p60;
			case 61: return p61;
			case 62: return p62;
			case 63: return p63;
		}

		return Vector3.zero;
	}

	void Update()
	{
		if ( !Enabled )
			return;

		if ( !ffd )
			ffd = GetComponent<MegaFFD>();

		if ( GetRecord() )
			GetPoints();
		else
			SetPoints();
	}
}