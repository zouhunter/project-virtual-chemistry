

using UnityEngine;

[AddComponentMenu("Modifiers/Morph Animate")]
[ExecuteInEditMode]
public class MegaMorphAnim : MonoBehaviour
{
	public string	SrcChannel = "None";
	public float	Percent = 0.0f;
	MegaMorphChan	channel;

	public string	SrcChannel1 = "None";
	public float	Percent1 = 0.0f;
	MegaMorphChan	channel1;

	public string	SrcChannel2 = "None";
	public float	Percent2 = 0.0f;
	MegaMorphChan	channel2;

	public string	SrcChannel3 = "None";
	public float	Percent3 = 0.0f;
	MegaMorphChan	channel3;

	public string	SrcChannel4 = "None";
	public float	Percent4 = 0.0f;
	MegaMorphChan	channel4;

	public string	SrcChannel5 = "None";
	public float	Percent5 = 0.0f;
	MegaMorphChan	channel5;

	public string	SrcChannel6 = "None";
	public float	Percent6 = 0.0f;
	MegaMorphChan	channel6;

	public string	SrcChannel7 = "None";
	public float	Percent7 = 0.0f;
	MegaMorphChan	channel7;

	public string	SrcChannel8 = "None";
	public float	Percent8 = 0.0f;
	MegaMorphChan	channel8;

	public string	SrcChannel9 = "None";
	public float	Percent9 = 0.0f;
	MegaMorphChan	channel9;

	public string	SrcChannel10 = "None";
	public float	Percent10 = 0.0f;
	MegaMorphChan	channel10;

	public string	SrcChannel11 = "None";
	public float	Percent11 = 0.0f;
	MegaMorphChan	channel11;

	public string	SrcChannel12 = "None";
	public float	Percent12 = 0.0f;
	MegaMorphChan	channel12;

	public string	SrcChannel13 = "None";
	public float	Percent13 = 0.0f;
	MegaMorphChan	channel13;

	public string	SrcChannel14 = "None";
	public float	Percent14 = 0.0f;
	MegaMorphChan	channel14;

	public string	SrcChannel15 = "None";
	public float	Percent15 = 0.0f;
	MegaMorphChan	channel15;

	public string	SrcChannel16 = "None";
	public float	Percent16 = 0.0f;
	MegaMorphChan	channel16;

	public string	SrcChannel17 = "None";
	public float	Percent17 = 0.0f;
	MegaMorphChan	channel17;

	public string	SrcChannel18 = "None";
	public float	Percent18 = 0.0f;
	MegaMorphChan	channel18;

	public string	SrcChannel19 = "None";
	public float	Percent19 = 0.0f;
	MegaMorphChan	channel19;

	public string	SrcChannel20 = "None";
	public float	Percent20 = 0.0f;
	MegaMorphChan	channel20;

	public string	SrcChannel21 = "None";
	public float	Percent21 = 0.0f;
	MegaMorphChan	channel21;

	public string	SrcChannel22 = "None";
	public float	Percent22 = 0.0f;
	MegaMorphChan	channel22;

	public string	SrcChannel23 = "None";
	public float	Percent23 = 0.0f;
	MegaMorphChan	channel23;

	public string	SrcChannel24 = "None";
	public float	Percent24 = 0.0f;
	MegaMorphChan	channel24;

	public string	SrcChannel25 = "None";
	public float	Percent25 = 0.0f;
	MegaMorphChan	channel25;

	public string	SrcChannel26 = "None";
	public float	Percent26 = 0.0f;
	MegaMorphChan	channel26;

	public string	SrcChannel27 = "None";
	public float	Percent27 = 0.0f;
	MegaMorphChan	channel27;

	public string	SrcChannel28 = "None";
	public float	Percent28 = 0.0f;
	MegaMorphChan	channel28;

	public string	SrcChannel29 = "None";
	public float	Percent29 = 0.0f;
	MegaMorphChan	channel29;

	public string	SrcChannel30 = "None";
	public float	Percent30 = 0.0f;
	MegaMorphChan	channel30;

	public string	SrcChannel31 = "None";
	public float	Percent31 = 0.0f;
	MegaMorphChan	channel31;

	public string	SrcChannel32 = "None";
	public float	Percent32 = 0.0f;
	MegaMorphChan	channel32;

	public string	SrcChannel33 = "None";
	public float	Percent33 = 0.0f;
	MegaMorphChan	channel33;

	public string	SrcChannel34 = "None";
	public float	Percent34 = 0.0f;
	MegaMorphChan	channel34;

	public string	SrcChannel35 = "None";
	public float	Percent35 = 0.0f;
	MegaMorphChan	channel35;

	public string	SrcChannel36 = "None";
	public float	Percent36 = 0.0f;
	MegaMorphChan	channel36;

	public string	SrcChannel37 = "None";
	public float	Percent37 = 0.0f;
	MegaMorphChan	channel37;

	public string	SrcChannel38 = "None";
	public float	Percent38 = 0.0f;
	MegaMorphChan	channel38;

	public string	SrcChannel39 = "None";
	public float	Percent39 = 0.0f;
	MegaMorphChan	channel39;

	public string	SrcChannel40 = "None";
	public float	Percent40 = 0.0f;
	MegaMorphChan	channel40;

	public string	SrcChannel41 = "None";
	public float	Percent41 = 0.0f;
	MegaMorphChan	channel41;

	public string	SrcChannel42 = "None";
	public float	Percent42 = 0.0f;
	MegaMorphChan	channel42;

	public string	SrcChannel43 = "None";
	public float	Percent43 = 0.0f;
	MegaMorphChan	channel43;

	public string	SrcChannel44 = "None";
	public float	Percent44 = 0.0f;
	MegaMorphChan	channel44;

	public string	SrcChannel45 = "None";
	public float	Percent45 = 0.0f;
	MegaMorphChan	channel45;

	public string	SrcChannel46 = "None";
	public float	Percent46 = 0.0f;
	MegaMorphChan	channel46;

	public string	SrcChannel47 = "None";
	public float	Percent47 = 0.0f;
	MegaMorphChan	channel47;

	public string	SrcChannel48 = "None";
	public float	Percent48 = 0.0f;
	MegaMorphChan	channel48;

	public string	SrcChannel49 = "None";
	public float	Percent49 = 0.0f;
	MegaMorphChan	channel49;

	public string	SrcChannel50 = "None";
	public float	Percent50 = 0.0f;
	MegaMorphChan	channel50;

	public void GetMinMax(MegaMorph mr, int index, ref float min, ref float max)
	{
		MegaMorphChan chan = null;

		switch ( index )
		{
			case 0: chan = channel;	break;
			case 1: chan = channel1; break;
			case 2: chan = channel2; break;
			case 3: chan = channel3; break;
			case 4: chan = channel4; break;
			case 5: chan = channel5; break;
			case 6: chan = channel6; break;
			case 7: chan = channel7; break;
			case 8: chan = channel8; break;
			case 9: chan = channel9; break;
			case 10: chan = channel10; break;
			case 11: chan = channel11; break;
			case 12: chan = channel12; break;
			case 13: chan = channel13; break;
			case 14: chan = channel14; break;
			case 15: chan = channel15; break;
			case 16: chan = channel16; break;
			case 17: chan = channel17; break;
			case 18: chan = channel18; break;
			case 19: chan = channel19; break;
			case 20: chan = channel20; break;
			case 21: chan = channel21; break;
			case 22: chan = channel22; break;
			case 23: chan = channel23; break;
			case 24: chan = channel24; break;
			case 25: chan = channel25; break;
			case 26: chan = channel26; break;
			case 27: chan = channel27; break;
			case 28: chan = channel28; break;
			case 29: chan = channel29; break;
			case 30: chan = channel30; break;
			case 31: chan = channel31; break;
			case 32: chan = channel32; break;
			case 33: chan = channel33; break;
			case 34: chan = channel34; break;
			case 35: chan = channel35; break;
			case 36: chan = channel36; break;
			case 37: chan = channel37; break;
			case 38: chan = channel38; break;
			case 39: chan = channel39; break;
			case 40: chan = channel40; break;
			case 41: chan = channel41; break;
			case 42: chan = channel42; break;
			case 43: chan = channel43; break;
			case 44: chan = channel44; break;
			case 45: chan = channel45; break;
			case 46: chan = channel46; break;
			case 47: chan = channel47; break;
			case 48: chan = channel48; break;
			case 49: chan = channel49; break;
			case 50: chan = channel50; break;
		}

		if ( chan != null )
		{
			min = chan.mSpinmin;
			max = chan.mSpinmax;
		}
		else
		{
			min = 0.0f;
			max = 100.0f;
		}
	}

	public void SetChannel(MegaMorph mr, int index)
	{
		switch ( index )
		{
			case 0: channel = mr.GetChannel(SrcChannel);	break;
			case 1: channel1 = mr.GetChannel(SrcChannel1); break;
			case 2: channel2 = mr.GetChannel(SrcChannel2); break;
			case 3: channel3 = mr.GetChannel(SrcChannel3); break;
			case 4: channel4 = mr.GetChannel(SrcChannel4); break;
			case 5: channel5 = mr.GetChannel(SrcChannel5); break;
			case 6: channel6 = mr.GetChannel(SrcChannel6); break;
			case 7: channel7 = mr.GetChannel(SrcChannel7); break;
			case 8: channel8 = mr.GetChannel(SrcChannel8); break;
			case 9: channel9 = mr.GetChannel(SrcChannel9); break;
			case 10: channel10 = mr.GetChannel(SrcChannel10); break;
			case 11: channel11 = mr.GetChannel(SrcChannel11); break;
			case 12: channel12 = mr.GetChannel(SrcChannel12); break;
			case 13: channel13 = mr.GetChannel(SrcChannel13); break;
			case 14: channel14 = mr.GetChannel(SrcChannel14); break;
			case 15: channel15 = mr.GetChannel(SrcChannel15); break;
			case 16: channel16 = mr.GetChannel(SrcChannel16); break;
			case 17: channel17 = mr.GetChannel(SrcChannel17); break;
			case 18: channel18 = mr.GetChannel(SrcChannel18); break;
			case 19: channel19 = mr.GetChannel(SrcChannel19); break;
			case 20: channel20 = mr.GetChannel(SrcChannel20); break;
			case 21: channel21 = mr.GetChannel(SrcChannel21); break;
			case 22: channel22 = mr.GetChannel(SrcChannel22); break;
			case 23: channel23 = mr.GetChannel(SrcChannel23); break;
			case 24: channel24 = mr.GetChannel(SrcChannel24); break;
			case 25: channel25 = mr.GetChannel(SrcChannel25); break;
			case 26: channel26 = mr.GetChannel(SrcChannel26); break;
			case 27: channel27 = mr.GetChannel(SrcChannel27); break;
			case 28: channel28 = mr.GetChannel(SrcChannel28); break;
			case 29: channel29 = mr.GetChannel(SrcChannel29); break;
			case 30: channel30 = mr.GetChannel(SrcChannel30); break;
			case 31: channel31 = mr.GetChannel(SrcChannel31); break;
			case 32: channel32 = mr.GetChannel(SrcChannel32); break;
			case 33: channel33 = mr.GetChannel(SrcChannel33); break;
			case 34: channel34 = mr.GetChannel(SrcChannel34); break;
			case 35: channel35 = mr.GetChannel(SrcChannel35); break;
			case 36: channel36 = mr.GetChannel(SrcChannel36); break;
			case 37: channel37 = mr.GetChannel(SrcChannel37); break;
			case 38: channel38 = mr.GetChannel(SrcChannel38); break;
			case 39: channel39 = mr.GetChannel(SrcChannel39); break;
			case 40: channel40 = mr.GetChannel(SrcChannel40); break;
			case 41: channel41 = mr.GetChannel(SrcChannel41); break;
			case 42: channel42 = mr.GetChannel(SrcChannel42); break;
			case 43: channel43 = mr.GetChannel(SrcChannel43); break;
			case 44: channel44 = mr.GetChannel(SrcChannel44); break;
			case 45: channel45 = mr.GetChannel(SrcChannel45); break;
			case 46: channel46 = mr.GetChannel(SrcChannel46); break;
			case 47: channel47 = mr.GetChannel(SrcChannel47); break;
			case 48: channel48 = mr.GetChannel(SrcChannel48); break;
			case 49: channel49 = mr.GetChannel(SrcChannel49); break;
			case 50: channel50 = mr.GetChannel(SrcChannel50); break;
		}
	}

	void Start()
	{
		MegaMorph mr = GetComponent<MegaMorph>();

		if ( mr != null )
		{
			channel = mr.GetChannel(SrcChannel);
			channel1 = mr.GetChannel(SrcChannel1);
			channel2 = mr.GetChannel(SrcChannel2);
			channel3 = mr.GetChannel(SrcChannel3);
			channel4 = mr.GetChannel(SrcChannel4);
			channel5 = mr.GetChannel(SrcChannel5);
			channel6 = mr.GetChannel(SrcChannel6);
			channel7 = mr.GetChannel(SrcChannel7);
			channel8 = mr.GetChannel(SrcChannel8);
			channel9 = mr.GetChannel(SrcChannel9);
			channel10 = mr.GetChannel(SrcChannel10);
			channel11 = mr.GetChannel(SrcChannel11);
			channel12 = mr.GetChannel(SrcChannel12);
			channel13 = mr.GetChannel(SrcChannel13);
			channel14 = mr.GetChannel(SrcChannel14);
			channel15 = mr.GetChannel(SrcChannel15);
			channel16 = mr.GetChannel(SrcChannel16);
			channel17 = mr.GetChannel(SrcChannel17);
			channel18 = mr.GetChannel(SrcChannel18);
			channel19 = mr.GetChannel(SrcChannel19);
			channel20 = mr.GetChannel(SrcChannel20);
			channel21 = mr.GetChannel(SrcChannel21);
			channel22 = mr.GetChannel(SrcChannel22);
			channel23 = mr.GetChannel(SrcChannel23);
			channel24 = mr.GetChannel(SrcChannel24);
			channel25 = mr.GetChannel(SrcChannel25);
			channel26 = mr.GetChannel(SrcChannel26);
			channel27 = mr.GetChannel(SrcChannel27);
			channel28 = mr.GetChannel(SrcChannel28);
			channel29 = mr.GetChannel(SrcChannel29);
			channel30 = mr.GetChannel(SrcChannel30);
			channel31 = mr.GetChannel(SrcChannel31);
			channel32 = mr.GetChannel(SrcChannel32);
			channel33 = mr.GetChannel(SrcChannel33);
			channel34 = mr.GetChannel(SrcChannel34);
			channel35 = mr.GetChannel(SrcChannel35);
			channel36 = mr.GetChannel(SrcChannel36);
			channel37 = mr.GetChannel(SrcChannel37);
			channel38 = mr.GetChannel(SrcChannel38);
			channel39 = mr.GetChannel(SrcChannel39);
			channel40 = mr.GetChannel(SrcChannel40);
			channel41 = mr.GetChannel(SrcChannel41);
			channel42 = mr.GetChannel(SrcChannel42);
			channel43 = mr.GetChannel(SrcChannel43);
			channel44 = mr.GetChannel(SrcChannel44);
			channel45 = mr.GetChannel(SrcChannel45);
			channel46 = mr.GetChannel(SrcChannel46);
			channel47 = mr.GetChannel(SrcChannel47);
			channel48 = mr.GetChannel(SrcChannel48);
			channel49 = mr.GetChannel(SrcChannel49);
			channel50 = mr.GetChannel(SrcChannel50);
		}
	}

	void Update()
	{
		if ( channel != null )	channel.Percent = Percent;
		if ( channel1 != null ) channel1.Percent = Percent1;
		if ( channel2 != null ) channel2.Percent = Percent2;
		if ( channel3 != null ) channel3.Percent = Percent3;
		if ( channel4 != null ) channel4.Percent = Percent4;
		if ( channel5 != null ) channel5.Percent = Percent5;
		if ( channel6 != null ) channel6.Percent = Percent6;
		if ( channel7 != null ) channel7.Percent = Percent7;
		if ( channel8 != null ) channel8.Percent = Percent8;
		if ( channel9 != null ) channel9.Percent = Percent9;
		if ( channel10 != null ) channel10.Percent = Percent10;
		if ( channel11 != null ) channel11.Percent = Percent11;
		if ( channel12 != null ) channel12.Percent = Percent12;
		if ( channel13 != null ) channel13.Percent = Percent13;
		if ( channel14 != null ) channel14.Percent = Percent14;
		if ( channel15 != null ) channel15.Percent = Percent15;
		if ( channel16 != null ) channel16.Percent = Percent16;
		if ( channel17 != null ) channel17.Percent = Percent17;
		if ( channel18 != null ) channel18.Percent = Percent18;
		if ( channel19 != null ) channel19.Percent = Percent19;
		if ( channel20 != null ) channel20.Percent = Percent20;
		if ( channel21 != null ) channel21.Percent = Percent21;
		if ( channel22 != null ) channel22.Percent = Percent22;
		if ( channel23 != null ) channel23.Percent = Percent23;
		if ( channel24 != null ) channel24.Percent = Percent24;
		if ( channel25 != null ) channel25.Percent = Percent25;
		if ( channel26 != null ) channel26.Percent = Percent26;
		if ( channel27 != null ) channel27.Percent = Percent27;
		if ( channel28 != null ) channel28.Percent = Percent28;
		if ( channel29 != null ) channel29.Percent = Percent29;
		if ( channel30 != null ) channel30.Percent = Percent30;
		if ( channel31 != null ) channel31.Percent = Percent31;
		if ( channel32 != null ) channel32.Percent = Percent32;
		if ( channel33 != null ) channel33.Percent = Percent33;
		if ( channel34 != null ) channel34.Percent = Percent34;
		if ( channel35 != null ) channel35.Percent = Percent35;
		if ( channel36 != null ) channel36.Percent = Percent36;
		if ( channel37 != null ) channel37.Percent = Percent37;
		if ( channel38 != null ) channel38.Percent = Percent38;
		if ( channel39 != null ) channel39.Percent = Percent39;
		if ( channel40 != null ) channel40.Percent = Percent40;
		if ( channel41 != null ) channel41.Percent = Percent41;
		if ( channel42 != null ) channel42.Percent = Percent42;
		if ( channel43 != null ) channel43.Percent = Percent43;
		if ( channel44 != null ) channel44.Percent = Percent44;
		if ( channel45 != null ) channel45.Percent = Percent45;
		if ( channel46 != null ) channel46.Percent = Percent46;
		if ( channel47 != null ) channel47.Percent = Percent47;
		if ( channel48 != null ) channel48.Percent = Percent48;
		if ( channel49 != null ) channel49.Percent = Percent49;
		if ( channel50 != null ) channel50.Percent = Percent50;
	}
}
