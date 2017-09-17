
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Modifiers/Morph Ref")]
public class MegaMorphRef : MegaMorphBase
{
	public MegaMorph		source;

	public bool				UseLimit;
	public float			Max;
	public float			Min;
	//public List<MegaMorphChan>	chanBank;
	public int[]			mapping;
	public float			importScale = 1.0f;
	public bool				flipyz = false;
	public bool				negx = false;

	[HideInInspector]
	public float			tolerance = 0.0001f;

	public bool				showmapping = false;
	public float			mappingSize = 0.001f;
	public int				mapStart = 0;
	public int				mapEnd = 0;

	Vector3[]				dif;
	static Vector3[]		endpoint	= new Vector3[4];
	static Vector3[]		splinepoint	= new Vector3[4];
	static Vector3[]		temppoint	= new Vector3[2];
	Vector3[]	p1;
	Vector3[]	p2;
	Vector3[]	p3;
	Vector3[]	p4;

	public List<float>	pers = new List<float>(4);

	public override string ModName() { return "Morph Ref"; }
	public override string GetHelpURL() { return "?page_id=257"; }

	[HideInInspector]
	public int compressedmem = 0;
	[HideInInspector]
	public int compressedmem1 = 0;
	[HideInInspector]
	public int memuse = 0;


	public void SetSource(MegaMorph src)
	{
		source = src;

		if ( source )
		{
			if ( chanBank == null )
				chanBank = new List<MegaMorphChan>();

			chanBank.Clear();

			for ( int i = 0; i < source.chanBank.Count; i++ )
			{
				MegaMorphChan ch = new MegaMorphChan();

				ch.control = source.chanBank[i].control;
				ch.Percent = source.chanBank[i].Percent;
				ch.mName = source.chanBank[i].mName;
					 
				chanBank.Add(ch);
			}
		}
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		if ( source )
		{
			if ( animate )
			{
				animtime += Time.deltaTime * speed;

				switch ( repeatMode )
				{
					case MegaRepeatMode.Loop: animtime = Mathf.Repeat(animtime, looptime); break;
					case MegaRepeatMode.Clamp: animtime = Mathf.Clamp(animtime, 0.0f, looptime); break;
				}
				SetAnim(animtime);
			}

			if ( dif == null )
			{
				dif = new Vector3[mc.mod.verts.Length];
			}
		}
		return Prepare(mc);
	}

	public bool  animate = false;
	public float atime = 0.0f;
	public float animtime = 0.0f;
	public float looptime = 0.0f;
	public MegaRepeatMode	repeatMode = MegaRepeatMode.Loop;
	public float speed = 1.0f;

	public override bool Prepare(MegaModContext mc)
	{
		if ( source )
			return source.Prepare(mc);

		return false;
	}

	// Only need to call if a Percent value has changed on a Channel or a target, so flag for a change
	void SetVerts(int j, Vector3[] p)
	{
		switch ( j )
		{
			case 0: p1 = p; break;
			case 1: p2 = p; break;
			case 2: p3 = p; break;
			case 3: p4 = p; break;
		}
	}

	void SetVerts(MegaMorphChan chan, int j, Vector3[] p)
	{
		switch ( j )
		{
			case 0: chan.p1 = p; break;
			case 1: chan.p2 = p; break;
			case 2: chan.p3 = p; break;
			case 3: chan.p4 = p; break;
		}
	}

	static int framenum;

	// oPoints whould be verts
	public override void Modify(MegaModifiers mc)
	{
		if ( source == null )
			return;

		if ( source.nonMorphedVerts != null && source.nonMorphedVerts.Length > 1 )
		{
			ModifyCompressed(mc);
			return;
		}

		framenum++;
		mc.ChangeSourceVerts();

		float fChannelPercent;
		Vector3	delt;

		// cycle through channels, searching for ones to use
		bool firstchan = true;
		bool morphed = false;

		float min = 0.0f;
		float max = 100.0f;

		if ( UseLimit )
		{
			min = Min;
			max = Max;
		}

		for ( int i = 0; i < source.chanBank.Count; i++ )
		{
			MegaMorphChan chan = source.chanBank[i];

			// This needs to be local chan list
			chan.UpdatePercent();

			if ( UseLimit )
			{
				fChannelPercent = Mathf.Clamp(chan.Percent, min, max);	//chan.mSpinmin, chan.mSpinmax);
			}
			else
			{
				if ( chan.mUseLimit )
					fChannelPercent = Mathf.Clamp(chan.Percent, chan.mSpinmin, chan.mSpinmax);
				else
					fChannelPercent = Mathf.Clamp(chan.Percent, 0.0f, 100.0f);
			}

			if ( fChannelPercent != 0.0f || (fChannelPercent == 0.0f && chan.fChannelPercent != 0.0f) )
			{
				chan.fChannelPercent = fChannelPercent;

				if ( chan.mTargetCache != null && chan.mTargetCache.Count > 0 && chan.mActiveOverride )	//&& fChannelPercent != 0.0f )
				{
					morphed = true;

					if ( chan.mUseLimit )	//|| glUseLimit )
					{
					}

					if ( firstchan )
					{
						firstchan = false;
						for ( int pointnum = 0; pointnum < source.oPoints.Length; pointnum++ )
						{
							dif[pointnum] = source.oPoints[pointnum];
						}
					}

					if ( chan.mTargetCache.Count == 1 )
					{
						for ( int pointnum = 0; pointnum < source.oPoints.Length; pointnum++ )
						{
							delt = chan.mDeltas[pointnum];

							dif[pointnum].x += delt.x * fChannelPercent;
							dif[pointnum].y += delt.y * fChannelPercent;
							dif[pointnum].z += delt.z * fChannelPercent;
						}
					}
					else
					{
						int totaltargs = chan.mTargetCache.Count;	// + 1;	// + 1;

						float fProgression = fChannelPercent;	//Mathf.Clamp(fChannelPercent, 0.0f, 100.0f);
						int segment = 1;
						while ( segment <= totaltargs && fProgression >= chan.GetTargetPercent(segment - 2) )
							segment++;

						if ( segment > totaltargs )
							segment = totaltargs;

						p4 = source.oPoints;

						if ( segment == 1 )
						{
							p1 = source.oPoints;
							p2 = chan.mTargetCache[0].points;	// mpoints
							p3 = chan.mTargetCache[1].points;
						}
						else
						{
							if ( segment == totaltargs )
							{
								int targnum = totaltargs - 1;

								for ( int j = 2; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(j, source.oPoints);
									else
										SetVerts(j, chan.mTargetCache[targnum + 1].points);
								}
							}
							else
							{
								int targnum = segment;

								for ( int j = 3; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(j, source.oPoints);
									else
										SetVerts(j, chan.mTargetCache[targnum + 1].points);
								}
							}
						}

						float targetpercent1 = chan.GetTargetPercent(segment - 3);
						float targetpercent2 = chan.GetTargetPercent(segment - 2);

						float top = fProgression - targetpercent1;
						float bottom = targetpercent2 - targetpercent1;
						float u = top / bottom;

						{
							for ( int pointnum = 0; pointnum < source.oPoints.Length; pointnum++ )
							{
								Vector3 vert = source.oPoints[pointnum];

								float length;

								Vector3 progession;

								endpoint[0] = p1[pointnum];
								endpoint[1] = p2[pointnum];
								endpoint[2] = p3[pointnum];
								endpoint[3] = p4[pointnum];

								if ( segment == 1 )
								{
									splinepoint[0] = endpoint[0];
									splinepoint[3] = endpoint[1];
									temppoint[1] = endpoint[2] - endpoint[0];
									temppoint[0] = endpoint[1] - endpoint[0];
									length = temppoint[1].sqrMagnitude;

									if ( length == 0.0f )
									{
										splinepoint[1] = endpoint[0];
										splinepoint[2] = endpoint[1];
									}
									else
									{
										splinepoint[2] = endpoint[1] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
										splinepoint[1] = endpoint[0] + chan.mCurvature * (splinepoint[2] - endpoint[0]);
									}
								}
								else
								{
									if ( segment == totaltargs )
									{
										splinepoint[0] = endpoint[1];
										splinepoint[3] = endpoint[2];
										temppoint[1] = endpoint[2] - endpoint[0];
										temppoint[0] = endpoint[1] - endpoint[2];
										length = temppoint[1].sqrMagnitude;

										if ( length == 0.0f )
										{
											splinepoint[1] = endpoint[0];
											splinepoint[2] = endpoint[1];
										}
										else
										{
											splinepoint[1] = endpoint[1] - (Vector3.Dot(temppoint[1], temppoint[0]) * chan.mCurvature / length) * temppoint[1];
											splinepoint[2] = endpoint[2] + chan.mCurvature * (splinepoint[1] - endpoint[2]);
										}
									}
									else
									{
										temppoint[1] = endpoint[2] - endpoint[0];
										temppoint[0] = endpoint[1] - endpoint[0];
										length = temppoint[1].sqrMagnitude;
										splinepoint[0] = endpoint[1];
										splinepoint[3] = endpoint[2];

										if ( length == 0.0f )
											splinepoint[1] = endpoint[0];
										else
											splinepoint[1] = endpoint[1] + (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];

										temppoint[1] = endpoint[3] - endpoint[1];
										temppoint[0] = endpoint[2] - endpoint[1];
										length = temppoint[1].sqrMagnitude;

										if ( length == 0.0f )
											splinepoint[2] = endpoint[1];
										else
											splinepoint[2] = endpoint[2] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
									}
								}

								MegaUtils.Bez3D(out progession, ref splinepoint, u);

								dif[pointnum].x += progession.x - vert.x;	//delt;
								dif[pointnum].y += progession.y - vert.y;	//delt;
								dif[pointnum].z += progession.z - vert.z;	//delt;
							}
						}
					}
				}
			}
		}

		if ( morphed )
		{
			for ( int i = 0; i < source.mapping.Length; i++ )
				sverts[i] = dif[source.mapping[i]];
		}
		else
		{
			for ( int i = 0; i < verts.Length; i++ )
				sverts[i] = verts[i];
		}
	}

	bool Changed(int v, int c)
	{
		for ( int t = 0; t < source.chanBank[c].mTargetCache.Count; t++ )
		{
			if ( !source.oPoints[v].Equals(source.chanBank[c].mTargetCache[t].points[v]) )
				return true;
		}

		return false;
	}

	public void ModifyCompressed(MegaModifiers mc)
	{
		framenum++;
		mc.ChangeSourceVerts();

		float fChannelPercent;
		Vector3	delt;

		// cycle through channels, searching for ones to use
		bool firstchan = true;
		bool morphed = false;

		for ( int i = 0; i < source.chanBank.Count; i++ )
		{
			MegaMorphChan chan = source.chanBank[i];
			MegaMorphChan lchan = chanBank[i];	// copy of source banks with out the vert data

			chan.UpdatePercent();

			if ( chan.mUseLimit )
				fChannelPercent = Mathf.Clamp(lchan.Percent, chan.mSpinmin, chan.mSpinmax);
			else
				fChannelPercent = Mathf.Clamp(lchan.Percent, 0.0f, 100.0f);

			if ( fChannelPercent != 0.0f || (fChannelPercent == 0.0f && chan.fChannelPercent != 0.0f) )
			{
				chan.fChannelPercent = fChannelPercent;

				if ( chan.mTargetCache != null && chan.mTargetCache.Count > 0 && chan.mActiveOverride )	//&& fChannelPercent != 0.0f )
				{
					morphed = true;

					if ( chan.mUseLimit )	//|| glUseLimit )
					{
					}

					// New bit
					if ( firstchan )
					{
						firstchan = false;
						// Save a int array of morphedpoints and use that, then only dealing with changed info
						for ( int pointnum = 0; pointnum < source.morphedVerts.Length; pointnum++ )
						{
							// this will change when we remove points
							int p = source.morphedVerts[pointnum];
							dif[p] = source.oPoints[p];	//morphedVerts[pointnum]];
						}
					}
					// end new

					if ( chan.mTargetCache.Count == 1 )
					{
						// Save a int array of morphedpoints and use that, then only dealing with changed info
						for ( int pointnum = 0; pointnum < source.morphedVerts.Length; pointnum++ )
						{
							int p = source.morphedVerts[pointnum];
							delt = chan.mDeltas[p];	//morphedVerts[pointnum]];
							//delt = chan.mDeltas[pointnum];	//morphedVerts[pointnum]];

							dif[p].x += delt.x * fChannelPercent;
							dif[p].y += delt.y * fChannelPercent;
							dif[p].z += delt.z * fChannelPercent;
						}
					}
					else
					{
						int totaltargs = chan.mTargetCache.Count;	// + 1;	// + 1;

						float fProgression = fChannelPercent;	//Mathf.Clamp(fChannelPercent, 0.0f, 100.0f);
						int segment = 1;
						while ( segment <= totaltargs && fProgression >= chan.GetTargetPercent(segment - 2) )
							segment++;

						if ( segment > totaltargs )
							segment = totaltargs;

						p4 = source.oPoints;

						if ( segment == 1 )
						{
							p1 = source.oPoints;
							p2 = chan.mTargetCache[0].points;	// mpoints
							p3 = chan.mTargetCache[1].points;
						}
						else
						{
							if ( segment == totaltargs )
							{
								int targnum = totaltargs - 1;

								for ( int j = 2; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(j, source.oPoints);
									else
										SetVerts(j, chan.mTargetCache[targnum + 1].points);
								}
							}
							else
							{
								int targnum = segment;

								for ( int j = 3; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(j, source.oPoints);
									else
										SetVerts(j, chan.mTargetCache[targnum + 1].points);
								}
							}
						}

						float targetpercent1 = chan.GetTargetPercent(segment - 3);
						float targetpercent2 = chan.GetTargetPercent(segment - 2);

						float top = fProgression - targetpercent1;
						float bottom = targetpercent2 - targetpercent1;
						float u = top / bottom;

						for ( int pointnum = 0; pointnum < source.morphedVerts.Length; pointnum++ )
						{
							int p = source.morphedVerts[pointnum];
							Vector3 vert = source.oPoints[p];	//pointnum];

							float length;

							Vector3 progession;

							endpoint[0] = p1[p];
							endpoint[1] = p2[p];
							endpoint[2] = p3[p];
							endpoint[3] = p4[p];

							if ( segment == 1 )
							{
								splinepoint[0] = endpoint[0];
								splinepoint[3] = endpoint[1];
								temppoint[1] = endpoint[2] - endpoint[0];
								temppoint[0] = endpoint[1] - endpoint[0];
								length = temppoint[1].sqrMagnitude;

								if ( length == 0.0f )
								{
									splinepoint[1] = endpoint[0];
									splinepoint[2] = endpoint[1];
								}
								else
								{
									splinepoint[2] = endpoint[1] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
									splinepoint[1] = endpoint[0] + chan.mCurvature * (splinepoint[2] - endpoint[0]);
								}
							}
							else
							{
								if ( segment == totaltargs )
								{
									splinepoint[0] = endpoint[1];
									splinepoint[3] = endpoint[2];
									temppoint[1] = endpoint[2] - endpoint[0];
									temppoint[0] = endpoint[1] - endpoint[2];
									length = temppoint[1].sqrMagnitude;

									if ( length == 0.0f )
									{
										splinepoint[1] = endpoint[0];
										splinepoint[2] = endpoint[1];
									}
									else
									{
										splinepoint[1] = endpoint[1] - (Vector3.Dot(temppoint[1], temppoint[0]) * chan.mCurvature / length) * temppoint[1];
										splinepoint[2] = endpoint[2] + chan.mCurvature * (splinepoint[1] - endpoint[2]);
									}
								}
								else
								{
									temppoint[1] = endpoint[2] - endpoint[0];
									temppoint[0] = endpoint[1] - endpoint[0];
									length = temppoint[1].sqrMagnitude;
									splinepoint[0] = endpoint[1];
									splinepoint[3] = endpoint[2];

									if ( length == 0.0f )
										splinepoint[1] = endpoint[0];
									else
										splinepoint[1] = endpoint[1] + (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];

									temppoint[1] = endpoint[3] - endpoint[1];
									temppoint[0] = endpoint[2] - endpoint[1];
									length = temppoint[1].sqrMagnitude;

									if ( length == 0.0f )
										splinepoint[2] = endpoint[1];
									else
										splinepoint[2] = endpoint[2] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
								}
							}

							MegaUtils.Bez3D(out progession, ref splinepoint, u);

							dif[p].x += progession.x - vert.x;
							dif[p].y += progession.y - vert.y;
							dif[p].z += progession.z - vert.z;
						}
					}
				}
			}
		}

		if ( morphed )
		{
			for ( int i = 0; i < source.morphMappingFrom.Length; i++ )
				sverts[source.morphMappingTo[i]] = dif[source.morphMappingFrom[i]];

			for ( int i = 0; i < source.nonMorphMappingFrom.Length; i++ )
				sverts[source.nonMorphMappingTo[i]] = source.oPoints[source.nonMorphMappingFrom[i]];
		}
		else
		{
			for ( int i = 0; i < verts.Length; i++ )
				sverts[i] = verts[i];
		}
	}

	// Build compressed data
	//public int[]	nonMorphedVerts;
	//public int[]	morphedVerts;
	//public int[]	morphMappingFrom;
	//public int[]	morphMappingTo;
	//public int[]	nonMorphMappingFrom;
	//public int[]	nonMorphMappingTo;

	// Threaded version
	Vector3[] _verts;
	Vector3[] _sverts;

	public override void PrepareMT(MegaModifiers mc, int cores)
	{
		PrepareForMT(mc, cores);
	}

	public override void DoWork(MegaModifiers mc, int index, int start, int end, int cores)
	{
		ModifyCompressedMT(mc, index, cores);
	}

	public void PrepareForMT(MegaModifiers mc, int cores)
	{
		if ( setStart == null )
			BuildMorphVertInfo(cores);

		// cycle through channels, searching for ones to use
		mtmorphed = false;

		for ( int i = 0; i < source.chanBank.Count; i++ )
		{
			MegaMorphChan chan = source.chanBank[i];
			chan.UpdatePercent();

			//float fChannelPercent = Mathf.Clamp(chan.Percent, 0.0f, 100.0f);
			float fChannelPercent;

			if ( chan.mUseLimit )
				fChannelPercent = Mathf.Clamp(chan.Percent, chan.mSpinmin, chan.mSpinmax);
			else
				fChannelPercent = Mathf.Clamp(chan.Percent, 0.0f, 100.0f);

			//if ( chan.fChannelPercent > 0.0f )
			if ( fChannelPercent != 0.0f || (fChannelPercent == 0.0f && chan.fChannelPercent != 0.0f) )
			{
				chan.fChannelPercent = fChannelPercent;

				if ( chan.mTargetCache != null && chan.mTargetCache.Count > 0 && chan.mActiveOverride )
				{
					mtmorphed = true;

					//if ( chan.mUseLimit )
					//	chan.fChannelPercent = Mathf.Lerp(chan.mSpinmin, chan.mSpinmax, chan.fChannelPercent * 0.01f);

					if ( chan.mTargetCache.Count > 1 )
					{
						int totaltargs = chan.mTargetCache.Count;	// + 1;	// + 1;

						chan.fProgression = chan.fChannelPercent;	//Mathf.Clamp(fChannelPercent, 0.0f, 100.0f);
						chan.segment = 1;
						while ( chan.segment <= totaltargs && chan.fProgression >= chan.GetTargetPercent(chan.segment - 2) )
							chan.segment++;

						if ( chan.segment > totaltargs )
							chan.segment = totaltargs;

						chan.p4 = source.oPoints;

						if ( chan.segment == 1 )
						{
							chan.p1 = source.oPoints;
							chan.p2 = chan.mTargetCache[0].points;	// mpoints
							chan.p3 = chan.mTargetCache[1].points;
						}
						else
						{
							if ( chan.segment == totaltargs )
							{
								int targnum = totaltargs - 1;

								for ( int j = 2; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(chan, j, source.oPoints);
									else
										SetVerts(chan, j, chan.mTargetCache[targnum + 1].points);
								}
							}
							else
							{
								int targnum = chan.segment;

								for ( int j = 3; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(chan, j, source.oPoints);
									else
										SetVerts(chan, j, chan.mTargetCache[targnum + 1].points);
								}
							}
						}
					}
				}
			}
		}

		if ( !mtmorphed )
		{
			for ( int i = 0; i < verts.Length; i++ )
				sverts[i] = verts[i];
		}
	}

	bool mtmorphed;

	public void ModifyCompressedMT(MegaModifiers mc, int tindex, int cores)
	{
		if ( !mtmorphed )
			return;

		int step = source.morphedVerts.Length / cores;
		int startvert = (tindex * step);
		int endvert = startvert + step;

		if ( tindex == cores - 1 )
			endvert = source.morphedVerts.Length;

		framenum++;
		Vector3	delt;

		// cycle through channels, searching for ones to use
		bool firstchan = true;

		Vector3[]	endpoint	= new Vector3[4];	// These in channel class
		Vector3[]	splinepoint	= new Vector3[4];
		Vector3[]	temppoint	= new Vector3[2];

		for ( int i = 0; i < chanBank.Count; i++ )
		{
			MegaMorphChan chan = chanBank[i];

			if ( chan.fChannelPercent != 0.0f )
			{
				if ( chan.mTargetCache != null && chan.mTargetCache.Count > 0 && chan.mActiveOverride )	//&& fChannelPercent != 0.0f )
				{
					if ( firstchan )
					{
						firstchan = false;
						for ( int pointnum = startvert; pointnum < endvert; pointnum++ )
						{
							int p = source.morphedVerts[pointnum];
							dif[p] = source.oPoints[p];
						}
					}

					if ( chan.mTargetCache.Count == 1 )
					{
						{
							for ( int pointnum = startvert; pointnum < endvert; pointnum++ )
							{
								int p = source.morphedVerts[pointnum];
								delt = chan.mDeltas[p];

								dif[p].x += delt.x * chan.fChannelPercent;
								dif[p].y += delt.y * chan.fChannelPercent;
								dif[p].z += delt.z * chan.fChannelPercent;
							}
						}
					}
					else
					{
						float targetpercent1 = chan.GetTargetPercent(chan.segment - 3);
						float targetpercent2 = chan.GetTargetPercent(chan.segment - 2);

						float top = chan.fProgression - targetpercent1;
						float bottom = targetpercent2 - targetpercent1;
						float u = top / bottom;

						for ( int pointnum = startvert; pointnum < endvert; pointnum++ )
						{
							int p = source.morphedVerts[pointnum];
							Vector3 vert = source.oPoints[p];	//pointnum];

							float length;

							Vector3 progession;

							endpoint[0] = chan.p1[p];
							endpoint[1] = chan.p2[p];
							endpoint[2] = chan.p3[p];
							endpoint[3] = chan.p4[p];

							if ( chan.segment == 1 )
							{
								splinepoint[0] = endpoint[0];
								splinepoint[3] = endpoint[1];
								temppoint[1] = endpoint[2] - endpoint[0];
								temppoint[0] = endpoint[1] - endpoint[0];
								length = temppoint[1].sqrMagnitude;

								if ( length == 0.0f )
								{
									splinepoint[1] = endpoint[0];
									splinepoint[2] = endpoint[1];
								}
								else
								{
									splinepoint[2] = endpoint[1] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
									splinepoint[1] = endpoint[0] + chan.mCurvature * (splinepoint[2] - endpoint[0]);
								}
							}
							else
							{
								if ( chan.segment == chan.mTargetCache.Count )	//chan.totaltargs )
								{
									splinepoint[0] = endpoint[1];
									splinepoint[3] = endpoint[2];
									temppoint[1] = endpoint[2] - endpoint[0];
									temppoint[0] = endpoint[1] - endpoint[2];
									length = temppoint[1].sqrMagnitude;

									if ( length == 0.0f )
									{
										splinepoint[1] = endpoint[0];
										splinepoint[2] = endpoint[1];
									}
									else
									{
										splinepoint[1] = endpoint[1] - (Vector3.Dot(temppoint[1], temppoint[0]) * chan.mCurvature / length) * temppoint[1];
										splinepoint[2] = endpoint[2] + chan.mCurvature * (splinepoint[1] - endpoint[2]);
									}
								}
								else
								{
									temppoint[1] = endpoint[2] - endpoint[0];
									temppoint[0] = endpoint[1] - endpoint[0];
									length = temppoint[1].sqrMagnitude;
									splinepoint[0] = endpoint[1];
									splinepoint[3] = endpoint[2];

									if ( length == 0.0f )
										splinepoint[1] = endpoint[0];
									else
										splinepoint[1] = endpoint[1] + (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];

									temppoint[1] = endpoint[3] - endpoint[1];
									temppoint[0] = endpoint[2] - endpoint[1];
									length = temppoint[1].sqrMagnitude;

									if ( length == 0.0f )
										splinepoint[2] = endpoint[1];
									else
										splinepoint[2] = endpoint[2] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
								}
							}

							MegaUtils.Bez3D(out progession, ref splinepoint, u);

							dif[p].x += progession.x - vert.x;
							dif[p].y += progession.y - vert.y;
							dif[p].z += progession.z - vert.z;
						}
					}
				}
			}
		}

		if ( mtmorphed )
		{
			for ( int i = setStart[tindex]; i < setEnd[tindex]; i++ )
				sverts[source.morphMappingTo[i]] = dif[source.morphMappingFrom[i]];

			for ( int i = copyStart[tindex]; i < copyEnd[tindex]; i++ )
				sverts[source.nonMorphMappingTo[i]] = source.oPoints[source.nonMorphMappingFrom[i]];
		}
	}

	int[]	setStart;
	int[]	setEnd;
	int[]	copyStart;
	int[]	copyEnd;

	int Find(int index)
	{
		int f = source.morphedVerts[index];

		for ( int i = 0; i < source.morphMappingFrom.Length; i++ )
		{
			if ( source.morphMappingFrom[i] > f )
				return i;
		}

		return source.morphMappingFrom.Length - 1;
	}

	void BuildMorphVertInfo(int cores)
	{
		int step = source.morphedVerts.Length / cores;

		setStart = new int[cores];
		setEnd = new int[cores];
		copyStart = new int[cores];
		copyEnd = new int[cores];

		int start = 0;
		int fv = 0;

		for ( int i = 0; i < cores; i++ )
		{
			setStart[i] = start;
			if ( i < cores - 1 )
			{
				setEnd[i] = Find(fv + step);
			}
			start = setEnd[i];
			fv += step;
		}

		setEnd[cores - 1] = source.morphMappingFrom.Length;

		// copys can be simple split as nothing happens to them
		start = 0;
		step = source.nonMorphMappingFrom.Length / cores;

		for ( int i = 0; i < cores; i++ )
		{
			copyStart[i] = start;
			copyEnd[i] = start + step;
			start += step;
		}

		copyEnd[cores - 1] = source.nonMorphMappingFrom.Length;
	}

	public void SetAnimTime(float t)
	{
		animtime = t;

		switch ( repeatMode )
		{
			case MegaRepeatMode.Loop: animtime = Mathf.Repeat(animtime, looptime); break;
			//case RepeatMode.PingPong: animtime = Mathf.PingPong(animtime, looptime); break;
			case MegaRepeatMode.Clamp: animtime = Mathf.Clamp(animtime, 0.0f, looptime); break;
		}
		SetAnim(animtime);
	}
}
// 1573
// 988