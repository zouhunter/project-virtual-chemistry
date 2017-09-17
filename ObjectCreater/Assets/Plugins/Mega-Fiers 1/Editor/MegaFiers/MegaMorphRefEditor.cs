
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MegaMorphRef))]
public class MegaMorphRefEditor : Editor
{
	static public Color ChanCol1 = new Color(0.44f, 0.67f, 1.0f);
	static public Color ChanCol2 = new Color(1.0f, 0.67f, 0.44f);

	Stack<Color> bcol = new Stack<Color>();
	Stack<Color> ccol = new Stack<Color>();
	Stack<Color> col  = new Stack<Color>();

	bool extraparams = false;

	private MegaModifier	src;
	private MegaUndo		undoManager;
	bool showmodparams = false;
	bool showchannels = true;

	private void OnEnable()
	{
		src = target as MegaModifier;

		// Instantiate undoManager
		undoManager = new MegaUndo(src, src.ModName() + " change");
	}

	void PushCols()
	{
		bcol.Push(GUI.backgroundColor);
		ccol.Push(GUI.contentColor);
		col.Push(GUI.color);
	}

	void PopCols()
	{
		GUI.backgroundColor = bcol.Pop();
		GUI.contentColor = ccol.Pop();
		GUI.color = col.Pop();
	}

	void DisplayChannel(MegaMorphRef morph, MegaMorphChan channel, int num)
	{
		if ( GUILayout.Button(num + " - " + channel.mName) )
			channel.showparams = !channel.showparams;

		float min = 0.0f;
		float max = 100.0f;
		if ( morph.UseLimit )
		{
			min = morph.Min;
			max = morph.Max;
		}

		GUI.backgroundColor = new Color(1, 1, 1);
		if ( channel.showparams )
		{
			channel.mName = EditorGUILayout.TextField("Name", channel.mName);

			if ( morph.UseLimit )
				channel.Percent = EditorGUILayout.Slider("Percent", channel.Percent, min, max);	//0.0f, 100.0f);
			else
			{
				if ( channel.mUseLimit )
					channel.Percent = EditorGUILayout.Slider("Percent", channel.Percent, channel.mSpinmin, channel.mSpinmax);
				else
					channel.Percent = EditorGUILayout.Slider("Percent", channel.Percent, 0.0f, 100.0f);
			}
		}
		else
		{
			if ( morph.UseLimit )
				channel.Percent = EditorGUILayout.Slider("Percent", channel.Percent, min, max);	//0.0f, 100.0f);
			else
			{
				if ( channel.mUseLimit )
					channel.Percent = EditorGUILayout.Slider("Percent", channel.Percent, channel.mSpinmin, channel.mSpinmax);
				else
					channel.Percent = EditorGUILayout.Slider("Percent", channel.Percent, 0.0f, 100.0f);
			}
		}
	}

	void DisplayChannelLim(MegaMorphRef morph, MegaMorphChan channel, int num)
	{
		float min = 0.0f;
		float max = 100.0f;
		if ( morph.UseLimit )
		{
			min = morph.Min;
			max = morph.Max;
		}

		GUI.backgroundColor = new Color(1, 1, 1);
		if ( morph.UseLimit )
			channel.Percent = EditorGUILayout.Slider(channel.mName, channel.Percent, min, max);	//0.0f, 100.0f);
		else
		{
			if ( channel.mUseLimit )
				channel.Percent = EditorGUILayout.Slider(channel.mName, channel.Percent, channel.mSpinmin, channel.mSpinmax);
			else
				channel.Percent = EditorGUILayout.Slider(channel.mName, channel.Percent, 0.0f, 100.0f);
		}
	}

	public override void OnInspectorGUI()
	{
		undoManager.CheckUndo();
		MegaMorphRef morph = (MegaMorphRef)target;

		PushCols();

		MegaMorph src = (MegaMorph)EditorGUILayout.ObjectField("Source", morph.source, typeof(MegaMorph), true);
		if ( src != morph.source )
			morph.SetSource(src);

		// Basic mod stuff
		showmodparams = EditorGUILayout.Foldout(showmodparams, "Modifier Common Params");

		if ( showmodparams )
		{
			morph.Label = EditorGUILayout.TextField("Label", morph.Label);
			morph.MaxLOD = EditorGUILayout.IntField("MaxLOD", morph.MaxLOD);
			morph.ModEnabled = EditorGUILayout.Toggle("Mod Enabled", morph.ModEnabled);
			morph.useUndo = EditorGUILayout.Toggle("Use Undo", morph.useUndo);
			morph.DisplayGizmo = EditorGUILayout.Toggle("Display Gizmo", morph.DisplayGizmo);
			morph.Order = EditorGUILayout.IntField("Order", morph.Order);
			morph.gizCol1 = EditorGUILayout.ColorField("Giz Col 1", morph.gizCol1);
			morph.gizCol2 = EditorGUILayout.ColorField("Giz Col 2", morph.gizCol2);
		}

		morph.animate = EditorGUILayout.Toggle("Animate", morph.animate);

		if ( morph.animate )
		{
			morph.animtime = EditorGUILayout.FloatField("AnimTime", morph.animtime);
			morph.looptime = EditorGUILayout.FloatField("LoopTime", morph.looptime);
			morph.speed = EditorGUILayout.FloatField("Speed", morph.speed);
			morph.repeatMode = (MegaRepeatMode)EditorGUILayout.EnumPopup("RepeatMode", morph.repeatMode);
		}

		string bname = "Hide Channels";

		if ( !showchannels )
			bname = "Show Channels";

		if ( GUILayout.Button(bname) )
			showchannels = !showchannels;

		morph.limitchandisplay = EditorGUILayout.Toggle("Compact Display", morph.limitchandisplay);

		if ( showchannels && morph.chanBank != null )
		{
			if ( morph.limitchandisplay )
			{
				morph.startchannel = EditorGUILayout.IntField("Start", morph.startchannel);
				morph.displaychans = EditorGUILayout.IntField("Display", morph.displaychans);
				if ( morph.displaychans < 0 )
					morph.displaychans = 0;

				if ( morph.startchannel < 0 )
					morph.startchannel = 0;

				if ( morph.startchannel >= morph.chanBank.Count - 1 )
					morph.startchannel = morph.chanBank.Count - 1;

				int end = morph.startchannel + morph.displaychans;
				if ( end >= morph.chanBank.Count )
					end = morph.chanBank.Count;

				for ( int i = morph.startchannel; i < end; i++ )
				{
					PushCols();

					if ( (i & 1) == 0 )
						GUI.backgroundColor = ChanCol1;
					else
						GUI.backgroundColor = ChanCol2;

					DisplayChannelLim(morph, morph.chanBank[i], i);
					PopCols();
				}
			}
			else
			{
				for ( int i = 0; i < morph.chanBank.Count; i++ )
				{
					PushCols();

					if ( (i & 1) == 0 )
						GUI.backgroundColor = ChanCol1;
					else
						GUI.backgroundColor = ChanCol2;

					DisplayChannel(morph, morph.chanBank[i], i);
					PopCols();
				}
			}
		}

		extraparams = EditorGUILayout.Foldout(extraparams, "Extra Params");

		if ( extraparams )
		{
			ChanCol1 = EditorGUILayout.ColorField("Channel Col 1", ChanCol1);
			ChanCol2 = EditorGUILayout.ColorField("Channel Col 2", ChanCol2);
		}

		PopCols();

		if ( GUI.changed )
			EditorUtility.SetDirty(target);

		undoManager.CheckDirty();
	}
}
