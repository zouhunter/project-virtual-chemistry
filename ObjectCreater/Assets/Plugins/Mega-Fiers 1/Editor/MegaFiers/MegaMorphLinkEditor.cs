
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaMorphLink))]
public class MegaMorphLinkEditor : Editor
{
	int GetIndex(string name, string[] channels)
	{
		int index = -1;
		for ( int i = 0; i < channels.Length; i++ )
		{
			if ( channels[i] == name )
			{
				index = i;
				break;
			}
		}
		return index;
	}

	// TODO: Need none in the popup to clear a channel
	public override void OnInspectorGUI()
	{
		MegaMorphLink anim = (MegaMorphLink)target;

		anim.morph = (MegaMorph)EditorGUILayout.ObjectField("Morph", anim.morph, typeof(MegaMorph), true);

		MegaMorph morph = anim.morph;	//gameObject.GetComponent<MegaMorph>();

		if ( morph != null )
		{
			if ( GUILayout.Button("Add Link") )
			{
				MegaMorphLinkDesc desc = new MegaMorphLinkDesc();
				anim.links.Add(desc);
			}

			string[] channels = morph.GetChannelNames();

			for ( int i = 0; i < anim.links.Count; i++ )
			{
				MegaMorphLinkDesc md = anim.links[i];
				md.name = EditorGUILayout.TextField("Name", md.name);
				//md.active = EditorGUILayout.Toggle("Active", md.active);

				//if ( md.active )
				md.active = EditorGUILayout.BeginToggleGroup("Active", md.active);
				{
					md.channel = EditorGUILayout.Popup("Channel", md.channel, channels);

					md.target = (Transform)EditorGUILayout.ObjectField("Target", md.target, typeof(Transform), true);
					md.src = (MegaLinkSrc)EditorGUILayout.EnumPopup("Source", md.src);

					if ( md.src != MegaLinkSrc.Angle && md.src != MegaLinkSrc.DotRotation )
						md.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", md.axis);

					EditorGUILayout.LabelField("Val", md.GetVal().ToString());
					md.min = EditorGUILayout.FloatField("Min", md.min);
					md.max = EditorGUILayout.FloatField("Max", md.max);
					md.low = EditorGUILayout.FloatField("Low", md.low);
					md.high = EditorGUILayout.FloatField("High", md.high);

					md.useCurve = EditorGUILayout.BeginToggleGroup("Use Curve", md.useCurve);
					md.curve = EditorGUILayout.CurveField("Curve", md.curve);
					EditorGUILayout.EndToggleGroup();

					if ( md.src == MegaLinkSrc.Angle || md.src == MegaLinkSrc.DotRotation )
					{
						EditorGUILayout.BeginHorizontal();
						if ( GUILayout.Button("Set Start Rot") )
						{
							if ( md.target )
								md.rot = md.target.localRotation;
						}

						//if ( GUILayout.Button("Set End Rot") )
						//{
							//if ( md.target )
							//{
								//Quaternion rot = md.target.localRotation;
							//	md.max = md.GetVal();
							//}
						//}

						EditorGUILayout.EndHorizontal();
					}

					EditorGUILayout.BeginHorizontal();
					if ( GUILayout.Button("Set Min Val") )
					{
						if ( md.target )
							md.min = md.GetVal();
							//md.rot = md.target.localRotation;
					}

					if ( GUILayout.Button("Set Max Val") )
					{
						if ( md.target )
						{
							//Quaternion rot = md.target.localRotation;
							md.max = md.GetVal();
						}
					}

					EditorGUILayout.EndHorizontal();

				}
				EditorGUILayout.EndToggleGroup();
				if ( GUILayout.Button("Delete") )
				{
					anim.links.RemoveAt(i);
					i--;
				}
			}

			if ( GUI.changed )
			{
				EditorUtility.SetDirty(target);
			}
		}
	}
}
