
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaPointCacheAnimClip
{
	public string			name;
	public float			start;
	public float			end;
	public MegaRepeatMode	loop;
	public float			speed = 1.0f;

	public MegaPointCacheAnimClip(string _name, float _start, float _end, MegaRepeatMode _loop)
	{
		name = _name;
		start = _start;
		end = _end;
		loop = _loop;
	}
}

[AddComponentMenu("Modifiers/Point Cache Animator")]
[ExecuteInEditMode]
public class MegaPointCacheAnimator : MonoBehaviour
{
	public MegaPointCache	mod;
	public MegaPointCacheRef	modref;

	public List<MegaPointCacheAnimClip>	clips = new List<MegaPointCacheAnimClip>();

	public int current = 0;
	public float t = -1.0f;	// Current clip time
	public float at = 0.0f;

	public int sourceFPS = 30;
	public bool	useFrames = true;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=1802");
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

	public void Stop()
	{
		t = -1.0f;
	}

	public int AddClip(string name, float start, float end, MegaRepeatMode loop)
	{
		MegaPointCacheAnimClip clip = new MegaPointCacheAnimClip(name, start, end, loop);
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
		if ( PlayOnStart )
		{
			t = 0.0f;
		}
		else
			t = -1.0f;
	}

	void Update()
	{
		if ( mod == null && modref == null )
		{
			mod = (MegaPointCache)GetComponent<MegaPointCache>();
			modref = (MegaPointCacheRef)GetComponent<MegaPointCacheRef>();
		}

		if ( mod != null || modref != null )
		{
			if ( LinkedUpdate )
			{
				DoLinkedUpdate();
			}
			else
			{
				if ( clips.Count > 0 && current < clips.Count )
				{
					if ( t >= 0.0f )
					{
						t += Time.deltaTime * clips[current].speed;
						float dt = clips[current].end - clips[current].start;

						switch ( clips[current].loop )
						{
							case MegaRepeatMode.Loop: at = Mathf.Repeat(t, dt); break;
							case MegaRepeatMode.PingPong: at = Mathf.PingPong(t, dt); break;
							case MegaRepeatMode.Clamp: at = Mathf.Clamp(t, 0.0f, dt); break;
						}

						at += clips[current].start;

						if ( mod )
							mod.SetAnim(at);
						else
							modref.SetAnim(at);
					}
				}
			}
		}
	}

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

								if ( mod )
									mod.SetAnim(ct);	//state.time + clips[current].start);
								else
									modref.SetAnim(ct);
								return;
							}
						}
					}
				}
			}
		}
	}
}