
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaMorphAnimClip
{
	public string			name;
	public float			start;
	public float			end;
	public MegaRepeatMode	loop;
	public float			speed = 1.0f;

	public MegaMorphAnimClip(string _name, float _start, float _end, MegaRepeatMode _loop)
	{
		name = _name;
		start = _start;
		end = _end;
		loop = _loop;
	}
}

public class MegaPlayingClip
{
	public float weight = 1.0f;
	public float t = 0.0f;
	public float at = 0.0f;
	public float targetblendtime = 1.0f;
	public float blendtime = 0.0f;	// if not 0 then count down to 0, lerp weight and when 0 remove from playing
	public int clipIndex = 0;

	public float[]	channelValues;
}

//public delegate bool MegaMorphAnimClipEvent(int clip, int id);

[AddComponentMenu("Modifiers/Morph Animator")]
[ExecuteInEditMode]
public class MegaMorphAnimator : MonoBehaviour
{
	public MegaMorphBase	morph;

	public MegaMorphBase[]	morphs;	

	public List<MegaMorphAnimClip>	clips = new List<MegaMorphAnimClip>();

	public List<MegaPlayingClip>	playing = new List<MegaPlayingClip>();

	public int current = 0;
	public float t = -1.0f;	// Current clip time
	public float at = 0.0f;
	//MegaMorphAnimClipEvent	listener;

	public int sourceFPS = 30;
	public bool	useFrames = true;

	Stack<MegaPlayingClip>	clippool;


	public void UpdatePlayingClips()
	{
		if ( playing.Count == 0 )
			return;
		// reset blends
		morph.ClearBlends();

		for ( int i = playing.Count - 1; i >= 0; i-- )
		{
			MegaPlayingClip pclip = playing[i];

			MegaMorphAnimClip clip = clips[pclip.clipIndex];

			if ( pclip.t >= 0.0f )
			{
				pclip.t += Time.deltaTime * clip.speed;
				float dt = clip.end - clip.start;

				switch ( clip.loop )
				{
					case MegaRepeatMode.Loop: pclip.at = Mathf.Repeat(pclip.t, dt); break;
					case MegaRepeatMode.PingPong: pclip.at = Mathf.PingPong(pclip.t, dt); break;
					case MegaRepeatMode.Clamp: pclip.at = Mathf.Clamp(pclip.t, 0.0f, dt); break;
				}

				pclip.at += clip.start;

				if ( pclip.targetblendtime != 0.0f )
				{
					if ( pclip.targetblendtime > 0.0f )
					{
						pclip.blendtime += Time.deltaTime;
						if ( pclip.blendtime >= pclip.targetblendtime )
						{
							pclip.targetblendtime = 0.0f;
							pclip.weight = 1.0f;
						}
						else
							pclip.weight = (pclip.targetblendtime - pclip.blendtime) / pclip.targetblendtime;
					}
					else
					{
						pclip.blendtime -= Time.deltaTime;
						if ( pclip.blendtime <= pclip.targetblendtime )
						{
							pclip.targetblendtime = 0.0f;
							pclip.weight = 0.0f;
						}
						else
							pclip.weight = (pclip.targetblendtime - pclip.blendtime) / pclip.targetblendtime;
					}
				}

				if ( pclip.weight != 0.0f )
				{
					if ( MultipleMorphs )
					{
						if ( morphs != null )
						{
							for ( int m = 0; m < morphs.Length; m++ )
							{
								morphs[m].SetAnimBlend(pclip.at, pclip.weight);
							}
						}
					}
					else
					{
						if ( morph )
							morph.SetAnimBlend(pclip.at, pclip.weight);
					}
				}
				else
				{
					PushClip(pclip);
					playing.RemoveAt(i);
				}
			}
		}
	}

	//public void SetListener(MegaMorphAnimClipEvent listen)
	//{
	//	listener = listen;
	//}

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=1108");
	}

	public bool IsPlaying()
	{
		if ( t >= 0.0f )
			return true;

		return false;
	}

	public void SetTime(float time)
	{
		t = time;
	}

	public float GetTime()
	{
		return at;
	}

	public void PlayClipEvent(int i)
	{
		PlayClip(i);
	}

	public void PlayClipNameEvent(string name)
	{
		PlayClip(name);
	}

	public void PlayClip(int i)
	{
		if ( i < clips.Count )
		{
			current = i;
			t = 0.0f;
		}
	}

	public void PlayClip(string name)
	{
		for ( int i = 0; i < clips.Count; i++ )
		{
			if ( clips[i].name == name )
			{
				current = i;
				t = 0.0f;
				return;
			}
		}
	}

	public void PlayClip(int i, float blend)
	{
		if ( i < clips.Count )
		{
			current = i;
			t = 0.0f;
			MegaPlayingClip pclip = PopClip();	//new MegaPlayingClip();	// TODO: have a pool for these
			pclip.t = 0.0f;
			pclip.targetblendtime = blend;

			MegaPlayingClip cclip = playing[playing.Count - 1];
			cclip.blendtime = -Mathf.Abs(cclip.blendtime);
			cclip.targetblendtime = -blend;

			playing.Add(pclip);
		}
	}

	public void PlayClip(string name, float blend)
	{
		for ( int i = 0; i < clips.Count; i++ )
		{
			if ( clips[i].name == name )
			{
				PlayClip(i, blend);
				//current = i;
				//t = 0.0f;
				return;
			}
		}
	}

	public void Stop()
	{
		t = -1.0f;
	}

	public int AddClip(string name, float start, float end, MegaRepeatMode loop)
	{
		MegaMorphAnimClip clip = new MegaMorphAnimClip(name, start, end, loop);
		clips.Add(clip);
		return clips.Count - 1;
	}

	public string[] GetClipNames()
	{
		string[] names = new string[clips.Count];

		for ( int i = 0; i < clips.Count; i++ )
		{
			names[i] = clips[i].name;
		}

		return names;
	}

	void Start()
	{
		if ( clippool == null )
		{
			clippool = new Stack<MegaPlayingClip>(8);
		}
		if ( PlayOnStart )
			t = 0.0f;
		else
			t = -1.0f;
	}

	MegaPlayingClip PopClip()
	{
		return clippool.Pop();
	}

	void PushClip(MegaPlayingClip clip)
	{
		clippool.Push(clip);
	}

	void Update()
	{
		if ( MultipleMorphs )
		{
			if ( morphs == null )
				morphs = GetComponentsInChildren<MegaMorphBase>();
		}
		else
		{
			if ( morph == null )
				morph = (MegaMorphBase)GetComponent<MegaMorphBase>();
		}

		if ( LinkedUpdate )
		{
			DoLinkedUpdate();
		}
		else
		{
			//if ( morph && clips.Count > 0 && current < clips.Count )
			if ( clips.Count > 0 && current < clips.Count )
			{
				UpdatePlayingClips();
				if ( t >= 0.0f )
				{
					t += Time.deltaTime * clips[current].speed;
					float dt = clips[current].end - clips[current].start;

					switch ( clips[current].loop )
					{
						case MegaRepeatMode.Loop:		at = Mathf.Repeat(t, dt);		break;
						case MegaRepeatMode.PingPong:	at = Mathf.PingPong(t, dt);		break;
						case MegaRepeatMode.Clamp:		at = Mathf.Clamp(t, 0.0f, dt);	break;
					}

					at += clips[current].start;

					if ( MultipleMorphs )
					{
						if ( morphs != null )
						{
							for ( int i = 0; i < morphs.Length; i++ )
							{
								morphs[i].SetAnim(at);
							}
						}
					}
					else
					{
						if ( morph )
							morph.SetAnim(at);
					}
				}
			}
		}
	}

	public bool MultipleMorphs = false;
	public bool LinkedUpdate = false;
	public bool PlayOnStart = true;

	void DoLinkedUpdate()
	{
		if ( GetComponent<Animation>() != null )
		{
			foreach ( AnimationState state in GetComponent<Animation>() )
			{
				if ( state.enabled )
				{
					AnimationClip clip = state.clip;

					if ( clip != null )
					{
						for ( int i = 0; i < clips.Count; i++ )
						{
							if ( clips[i].name == clip.name )
							{
								current = i;
								float ct = state.time;

								WrapMode wm = clip.wrapMode;

								if ( wm == WrapMode.Default )
								{
									wm = GetComponent<Animation>().wrapMode;
								}

								switch ( clip.wrapMode )
								{
									case WrapMode.Loop:
										ct = Mathf.Repeat(ct, clip.length);
										break;

									case WrapMode.PingPong:
										ct = Mathf.PingPong(ct, clip.length);
										break;

									case WrapMode.ClampForever:
										ct = Mathf.Clamp(ct, 0.0f, clip.length);
										break;

									case WrapMode.Once:
										if ( ct > clip.length )
											ct = 0.0f;
										break;
								}
								ct += clips[current].start;

								if ( MultipleMorphs )
								{
									if ( morphs != null )
									{
										for ( int m = 0; m < morphs.Length; m++ )
										{
											morphs[m].SetAnim(at);
										}
									}
								}
								else
								{
									if ( morph )
										morph.SetAnim(ct);	//state.time + clips[current].start);
								}
								return;
							}
						}
					}
				}
			}
		}
	}
}