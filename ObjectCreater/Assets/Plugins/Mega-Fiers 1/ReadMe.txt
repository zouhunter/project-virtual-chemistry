
****************************************
Overview
****************************************
For more complete and up to date docs as well as tutorial videos please visit the web site at http://www.west-racing.com/mf

A system of mesh modifiers for Unity written in C# and provided as a dll Assembly for Unity. The modifiers included can be applied individually or multiple mods can be added to a mesh based object in Unity. The system is inspired by the 3ds max system so users of max should find most of the functionality and params familiar, and for others there is no coding required just setting of params to alter effects. Please note this is currently in beta so there could well be some issues that need sorting out. There may be a slight mismatch on Axis on some modifiers, the system was converted over from code for our own engine where Z was up.

I have other modifiers almost ready to be added and I will add these very shortly, they include FFD Cyl, FFD Box, Path deform and also a system of World Space modifiers such as ripple and wave. These will be added as updates to the system in the next few weeks. I may also add our code for exporting modifiers from Max if there is a need for it. It would be possible for the user to add their own Modifiers to the system and I will probably expose that shortly as well. And if anyone has anything they need then please feel free to email me at chris@west-racing.com.

All code is copyright Chris West 2012 and any use outside of the Unity engine is not permitted.

****************************************
How to use:
****************************************
To add a modifier to a mesh you first need to add the ModifyObject.cs component to the object then add the Modifiers you require. The ModifyObject.cs script is the manager for any modifiers on the object, it allows multiple modifiers to be stacked on top of one another to achieve different end results. Once the manager is in place drag and drop the modifier you want to the object. Each modifier is represented by a yellow box and the effect of the modifier is shown by this box so you can judge what will happen at runtime. If a modifier can be constrained to an area red and green boxes will show the extents of the constraint.

Each modifiers space is controlled by the gizmo pos, rot and scale params, usually you can leave these at default values of 0, but in case an effect doesnt quite happen in the way you could play with these values to get the desired result.

If you arnt planning on having any modifications happen in game and are just deforming meshes then you can delete all the modifier scripts once you are happy with a deformation and just be left with a deformed mesh.

****************************************
Modifier
****************************************
All the modifiers share some common params.
Class:
public class Modifier : MonoBehaviour
{
	public bool			ModEnabled;
	public bool			DisplayGizmo;
	public int			Order;
	public Vector3		Offset;
	public Vector3		gizmoPos;
	public Vector3		gizmoRot;
	public Vector3		gizmoScale;
}

Mod Enabled		-	Turns the modifier on/off
Display Gizmo	-	Shows the box gizmo for the modifier, usually you will have this on but if you have multiple							modifiers on a mesh it can get confusing.
Order			-	When more than modifier is in the stack the order they are processed in can be important, so a							modifier with Order value of -1 will be processed before one of Order 0
Offset			-	Offset added to each vertex before the modifier is applied, the effect of this depends on the modifier.
Gizmo Pos		-	A local offset added to the vertex, again its effect varies on the modifier in use.
Gizmo Rot		-	A local rotation of the modifier space.
Gizmo Scale		-	A local scale of the modifier space.

****************************************
ModifyObject
****************************************
ModifyObject is derived from Modifiers.

Class:
public class Modifiers : MonoBehaviour
{
	public bool			recalcnorms;
	public bool			recalcbounds;
	public bool			Enabled;
	public bool			DoLateUpdate;
	public bool			GrabVerts;
}

public class ModifyObject : Modifiers
{
}

Param description:
Enabled			-	When enabled the modifier stack is evaluated to get the final vertex positions, if not enabled the						last calculated positions will be used, so generally you will clear this when you have calculated a						desired	effect on a mesh to stop further calculations being done, and you will enable again if							further modifications need to be made.
RecalcNorms		-	Tell the system to recalculate normals for the mesh after the stack has been evaluated.
RecalcBounds	-	Tell the system to recalulate the bounds info for the mesh.
DoLateUpdate	-	Set if you require the modifier stack to be evaluated and applied in the LateUpdate instead of							Update
GrabVerts		-	Grabs the mesh verts again at start of modify. Use if something higher up the pipeline alters the						mesh vertices.

Modifiers:

****************************************
Bend:
****************************************
The Bend modifier lets you bend the current selection about a single axis, producing a uniform bend in an object's geometry. You can control the angle and direction of the bend on any of three axes. You can also limit the bend to a section of the geometry.

Class:
public class Bend : Modifier
{
	public float		angle;
	public float		dir;
	public Axis			axis;
	public bool			doRegion;
	public float		from;
	public float		to;
}

Param description:
Angle			- Sets the angle to bend from the viertical plane.
Direction		- Sets the direction of the bend relative to the horizontal plane.
Axis			- Specifies the axis to be bent.
DoRegion		- Limits the effect to a region of the mesh. The area of the effect will be shown by red and green boxes.
From			- Lower boundary of the effect
To				- Upper boundary of the effect

****************************************
Bubble:
****************************************
The Bubble modifier will inflate or pinch an object according to the transformation of the modifier's gizmo, the radius and falloff parameters.

Class:
public class Bubble : Modifier
{
	public float		radius;
	public float		falloff;
}

Param description:
Radius			-	Is the length of the offset a vertex would get if it's distance from the gizmo evaluates as zero. This						parameter can be set to a negative value as well to create a pinch effect.
Falloff			-	Determines how far from the gizmo will the bubble effect the deformation.

****************************************
Hump:
****************************************
Hump is a simpleMod scripted plug-in modifier which folding an object according to the transformation of the modifier's gizmo, the amount and heaping parameters over X, Y or Z axis.

Class:
public class Hump : Modifier
{
	public float		amount;
	public float		cycles;
	public float		phase;
	public bool			animate;
	public float		speed;
	public Axis			axis;
}

Param description:
Amount			-	Amount of hump
Cycles			-	Number of humps to apply
Phase			-	Shifts the wave
Animate			-	Animate the effect
Speed			-	Speed of animation
Axis			-	Axis to apply effect to

****************************************
Melt:
****************************************
The Melt modifier lets you apply a realistic melting effect to the mesh. Options include sagging of edges, spreading while melting, and a customizable set of substances ranging from a firm plastic surface to a jelly type that collapses in on itself.

Class:
public class Melt : Modifier
{
	public float		Amount;
	public float		Spread;
	public MeltMat		MaterialType;
	public float		Solidity;
	public Axis			axis;
	public bool			FlipAxis;
}

Param description:
Amount			-	Specifies the extent of the "decay," or melting effect applied to the gizmo, thus affecting the object.
Spread			-	Specifies how much the object and melt will spread as the Amount value increases. It's basically a "bulge"					along a	flat plane.
Material Type	- 	Ice. The default Solidity setting.
					Glass. Uses a high Solidity setting to simulate glass.
					Jelly. Causes a significant drooping effect in the center.
					Plastic. Relatively solid, but droops slightly in the center as it melts.
					Custom. User set value.

Solidity		-	If material is Custom sets how solid the mesh is.
Axis			-	Choose the axis on which the melt will occur.

Flip Axis		-	Normally, the melt occurs from the positive direction toward the negative along a given axis. Turn on Flip					Axis to	reverse this direction.

****************************************
Noise:
****************************************
The Noise modifier modulates the position of an object's vertices along any combination of three axes. This modifier simulates random variations in an object's shape. Using a fractal setting, you can achieve random, rippling patterns, like a flag in the wind. The results of the Noise modifier are most noticeable on objects that have greater numbers of faces.

Class:
public class Noise : Modifier
{
	public int			Seed;
	public float		Scale;
	public bool			Fractal;
	public float		Freq;
	public float		Iterations;
	public bool			Animate;
	public float		Phase;
	public float		Rough;
	public Vector3		Strength;
}

Param description:
Seed			-	Generates a random start point from the number you set.
Scale			-	Sets the size of the noise effect (not strength). Larger values produce smoother noise, lower values more					jagged noise
Strength		-	Set the strength of the noise effect along each of three axes. Enter a value for at least one of these						axes to produce a noise effect.
Animate			-	Regulates the combined effect of Noise and Strength parameters. The following parameters adjust the							underlying wave.
Freq			-	Sets the periodicity of the sine wave. Regulates the speed of the noise effect. Higher frequencies make						the noise quiver faster. Lower frequencies produce a smoother and more gentle noise.
Phase			-	Shifts the start and end points of the underlying wave.
Fractal			-	Produces a fractal effect based on current settings.

If you turn on Fractal, the following options are available: 
Rough			-	Determines the extent of fractal variation. Lower values are less rough than higher values.
Iterations		-	Controls the number of iterations (or octaves) used by the fractal function. Fewer iterations use less						fractal	energy and generate a smoother effect. An iteration of 1.0 is the same as turning Fractal off.

****************************************
Push:
****************************************
The Push modifier lets you "push" object vertices outward or inward along the average vertex normals. This produces an "inflation" effect.

Class:
public class Push : Modifier
{
	public float		amount;
	public NormType		method;
}

Param description:
Amount			-	How far to push vertices.
Method			-	Leave as normals for now, other values are being tested.

****************************************
RadialSkew:
****************************************
Radial Skew skews objects radially instead of along a single axis.

Class:
public class RadialSkew : Modifier
{
	public float		angle;
	public Axis			axis;
	public Axis			eaxis;
	public bool			biaxial;
}

Param description:
Angle			-	Angle of the Skew
Axis			-	Major axis of the skew
EAxis			-	Effective axis of the skew
BiAxial			-	This is similar to what Taper produces when both its primary and effect axes are set to single axes, but					here, the points are moved depending on which side of the center axis they lie along the effect axis. And,					again, the adjustment is described with an angular measurement, not a percentage.

****************************************
Ripple:
****************************************
The Ripple modifier lets you produce a concentric rippling effect in an object's geometry. You can set either of two ripples or a combination of both.

Class:
public class Ripple : Modifier
{
	public float		amp;
	public float		amp2;
	public float		flex;
	public float		wave;
	public float		phase;
	public float		Decay;
	public bool			animate;
}

Param description:
Amp/ Amp 2		-	Amplitude 1 produces a ripple across the object in one direction, while Amplitude 2 creates a similar						ripple at right angles to the first (that is, rotated 90 degrees about the vertical axis).
Wave			-	Specifies the distance between the peaks of the wave. The greater the length, the smoother and more							shallow the ripple for a given amplitude.
Phase			-	Shifts the ripple pattern over the object. Positive numbers move the pattern inward, while negative							numbers move it outward. This effect becomes especially clear when animated.
Decay			-	Limits the effect of the wave generated from its center. The value of 0.0 means that the wave will							generate infinitely from its center. Increasing the Decay value causes the wave amplitudes to decrease						with distance from the center, thus limiting the distance over which the waves are generated.
Animate			-	Animate the ripple
Radius			-	Radius of the gizmo
Segments		-	Number of segments in the gizmo
Circles			-	Number of circles in the gizmo

****************************************
Skew:
****************************************
The Skew modifier lets you produce a uniform offset in an object's geometry. You can control the amount and direction of the skew on any of three axes. You can also limit the skew to a section of the geometry.

Class:
public class Skew : Modifier
{
	public float		amount;
	public bool			doRegion;
	public float		to;
	public float		from;
	public float		dir;
	public Axis			axis;
}

Param description:
Amount			-	Sets the angle to skew from the vertical plane.
Direction		-	Sets the direction of the skew relative to the horizontal plane.
Axis			-	Specify the axis that will be skewed.
Do Region		-	Constrain modifier to a region
From			-	Start of region
To				-	End of region

****************************************
Spherify:
****************************************
The Spherify modifier distorts an object into a spherical shape. This modifier has only one parameter: a Percent spinner that deforms the object, as much as possible, into a spherical shape. The success of the operation depends on the topology of the geometry to which it's applied. For example, a cylinder with no height segments will result in little change. Adding height segments will result in a barrel at 100 percent. Adding cap segments will produce a sphere.

Class:
public class Spherify : Modifier
{
	public float		percent;
}

Param description:
Percent			-	Sets the percentage of spherical distortion to apply to an object.

****************************************
Stretch:
****************************************
The Stretch modifier simulates the traditional animation effect of "squash-and-stretch." Stretch applies a scale effect along a specified stretch axis and an opposite scale along the two remaining minor axes. The amount of opposite scaling on the minor axes varies, based on distance from the center of the scale effect. The maximum amount of scaling occurs at the center and falls off toward the ends.

Class:
public class Stretch : Modifier
{
	public float		amount;
	public bool			doRegion;
	public float		to;
	public float		from;
	public float		amplify;
	public Axis			axis;
}

Param description:
Amount			-	Sets the base scale factor for all three axes. The scale factor derived from the Stretch value varies						according to the sign of the value.

						Positive stretch values define a scale factor equal to Stretch+1. For example, a stretch value of 1.5						yields a scale factor of 1.5+1=2.5, or 250 percent.
					
						Negative stretch values define a scale factor equal to -1/(Stretch-1). For example, a stretch value of						-1.5 yields a scale factor of -1/(-1.5-1)=0.4, or 40 percent.
					
					The calculated scale factor is applied to the selected stretch axis and the inverse scale is applied to						the minor axes.

Amplify			-	Changes the scale factor applied to the minor axes. Amplify generates a multiplier using the same							technique as stretch. The multiplier is then applied to the Stretch value before the scale factor for the					minor axes is calculated.	Amplify values affect scaling along the minor axes in the following way:
						A value of 0 has no effect. It uses the default scale factor calculated from the Stretch amount.
						Positive values exaggerate the effect.
						Negative values reduce the effect.
Axis			-	Axis to stretch along
Do Region		-	Constrain the effect to a region
From			-	Start of the region
To				-	End of the region

****************************************
Taper:
****************************************
The Taper modifier produces a tapered contour by scaling both ends of an object's geometry; one end is scaled up, and the other is scaled down. You can control the amount and curve of the taper on two sets of axes. You can also limit the taper to a section of the geometry.

Class:
public class Taper : Modifier
{
	public float		amount;
	public bool			doRegion;
	public float		to;
	public float		from;
	public float		dir;
	public Axis			axis;
	public EffectAxis	EAxis;
	public float		crv;
	public bool			sym;
}

Param description:
Amount			-	The extent to which the ends are scaled.
Crv				-	Applies a curvature to the sides of the Taper gizmo, thus affecting the shape of the tapered object.						Positive values produce an outward curve along the tapered sides, negative values an inward curve. At 0,					the sides are unchanged.
Dir				-	Swivel amount around axis.
Axis			-	The central axis or spine of the taper: X, Y, or Z.
EAxis			-	The axis, or pair of axes, indicating the direction of the taper from the primary axis. The available						choices are determined by the choice of primary axis.
Sym				-	Produces a symmetrical taper around the primary axis. A taper is always symmetrical around the effect axis
Do Region		-	Limit effect to a region.
From			-	Start of region.
To				-	End of region.

****************************************
Twist:
****************************************
The Twist modifier produces a twirling effect (like wringing out a wet rag) in an object's geometry. You can control the angle of the twist on any of three axes, and set a bias that compresses the twist effect relative to the pivot point. You can also limit the twist to a section of the geometry.

Class:
public class Twist : Modifier
{
	public float		angle;
	public bool			doRegion;
	public float		from;
	public float		to;
	public float		Bias;
	public Axis			axis;
}

Param description:
Angle			-	Determines the amount of twist around the vertical axis.
Bias			-	Causes the twist rotation to bunch up at either end of the object. When the parameter is negative, the					object twists closer to the gizmo center. When the value is positive, the object twists more away from					the gizmo center. When the parameter is 0, the twisting is uniform.
Axis			-	Specify the axis along which the twist will occur. This is the local axis of the Twist gizmo.
Do Region		-	Limit effect to a region of the mesh
From			-	Start of the region
To				-	End of the region.

****************************************
Wave:
****************************************
The Wave modifier produces a wave effect in an object's geometry. You can use either of two waves, or combine them. Wave uses a standard gizmo and center, which you can transform to increase the possible wave effects.

Class:
public class Wave : Modifier
{
	public float		amp;
	public float		amp2;
	public float		flex;
	public float		wave;
	public float		phase;
	public float		Decay;
	public float		dir;
	public bool			animate;
}

Param description:
Amp/Amp 2		-	Amplitude 1 produces a sine wave along the gizmo's Y axis, while Amplitude 2 creates a wave along the X					axis (although peaks and troughs appear in the same direction with both). Switching a value from						positive to negative reverses the positions of peaks and troughs.
Wave			-	Specifies the distance in current units between the crests of both waves.
Phase			-	Shifts the wave pattern over the object. Positive numbers move the pattern in one direction, while						negative numbers move it in the other. This effect is especially clear when animated.
Decay			-	Limits the effect of the wave generated from its origin. A decay value decreases the amplitude at						increasing distance from the center. As this value increases, the wave is concentrated at the center					and flattened until it disappears (completely decays).
Dir				-	Direction of wave
Animate			-	Animate the wave
Divs			-	Number of divisions in the gizmo
NumSegs			-	Number of segements in the gizmo
NumSides		-	Number of sides in the gizmo

****************************************
FFD
****************************************
FFD stands for Free-Form Deformation. The FFD modifier surrounds the selected geometry with a lattice. By adjusting the control points of the lattice, you deform the enclosed geometry.

There are three FFD modifiers, each providing a different lattice resolution: 2x2, 3x3, and 4x4. The 3x3 modifier, for example, provides a lattice with three control points across each of its dimensions or nine on each side of the lattice.

Class:
public class FFD : Modifier
{
	public bool			DrawGizmos;
	public float		KnotSize;
	public bool			inVol;
	public Vector3[]	pt;
}

Param Description:
DrawGizmos		-	Draw the control points and gizmo in edit mode, allows user to drag points
KnotSize		-	Relationship of knotsize to lattice grid size
inVol			-	Only deform points that lie inside the source volume.
pt				-	Array of 64 vector3 which hold the control points, for 2x2x2 first 8 are used, 3x3x3 first 27

****************************************
Displace
****************************************
Displace vertices based on their normal and a texture lookup for the amount.

Class:
public class Displace : Modifier
{
	public Texture2D	map;
	public float		amount;
	public Channel		channel;
	public Vector2		offset;
	public Vector2		scale;
}

Param Description:
map				-	Texture to control the displacement, uv coords for a vertex govern the lookup pos. Texture must be
					readable.
amount			-	Amount to displace vertex which is modulated by the texture lookup.
channel			-	Channel of the texture to use for the displacment.
offset			-	UV offset, use for scrolling.
scale			-	Scale uv coords.

****************************************
Bulge
****************************************
Bulge behaves like spherify but you have control over each axis and with a falloff per axis.

Class:
public class Bulge : Modifier
{
	public Vector3	Amount;
	public Vector3	FallOff;
	public bool		LinkFallOff;
}

Param Description:
Amount			-	Amount of bulge on each axis.
FallOff			-	Falloff of the bulge on each axis.
LinkFallOff		-	Link all falloff values to the x axis value

****************************************
Cylindrify
****************************************
This is like Spherify but only acts on 2 axis so a mesh will deform towards a cylinder.

Class:
public class Cylindrify : Modifier
{
	public float Percent = 0.0f;
	public float Decay = 0.0f;
}

Param Description:
Percent			-	Amount of transform towards a Cylinder
Decay			-	How quickly the effect fades with distance.

****************************************
Page Flip
****************************************
Start of a page turning system. This one is very much wip in progress, it works but there is room for improvement, I hope
to add a page mesh generator and also a proper book system. Iam not going to document this yet as it really is WIP.


****************************************
UV Adjust
****************************************
This is a quick general purpose UV modifier, you can change the uv offset, tiling amount and rotation of uvs. This will be improved in later versions to work on submeshes and other features.

Class:
public class UVAdjust : Modifier
{
	public Vector3		gizmoPos;
	public Vector3		gizmoRot;
	public Vector3		gizmoScale;
	public Vector3		Offset;
	public bool			animate;
	public float		rotspeed;
	public float		spiralspeed;
	public Vector3		speed;
	public float		spiral;
	public float		spirallim;
}

Param Description:
gizmoPos		-	Amount to add to uv coordinates
gizmoScale		-	Amount to scale/zoom uv coordinates
Offset			-	Offset to the pivot for the rotation effect
gizmoRot		-	Angle of rotation of uv
animate			-	Use speed values to alter uv coords
rotspeed		-	How fast uv will rotate when animate is on
speed			-	How fast uvs will translate when animate is on
spiral			-	Amount of spiral to add to the uv rotate
spirallim		-	Angular limit of spiral when Animate is on

****************************************
UV Tiles
****************************************
First test of a spritemanager style uv modifier, this can be used to alter the section of a texture displayed or to play animations. It is a wip so it may give odd results, for example the texture needs to be a power of 2.

Class:
public class UVTiles : Modifier
{
	public int		Frame;
	public int		TileWidth;
	public int		TileHeight;
	public Vector2	off;
	public Vector2	scale;
	public bool		Animate;
	public int		EndFrame;
	public float	fps;
	public bool		flipy;
	public bool		flipx;
}

Param Description:
Frame			-	Frame or tile to displat from sprite sheet
TileWidth		-	Width of the tiles on the sheet
TileHeight		-	Height of the tiles on the sheet
off				-	Adjust uv positions.
scale			-	Zoom uv coordinates
Animate			-	Play back the sprite sheet
EndFrame		-	Limit animation to first n tiles
fps				-	Desired playback speed
flipx			-	Flip uvs in the x direction
flipy			-	Flip uvs in the y direction

****************************************
WarpBind
****************************************
This modifier needs to be added to any object that you want to bind/connect to a world space warp ie ripple. You would first add a RippleWarp to an empty gamobject in the scene and set the desired params for that, then on the mesh object you want to be modified by that world space warp you would add ModifyObject as per normal and then add the WarpBind modifier selecting for the SourceWarpObj the game object with the ripple effect. The mesh will now deform based on the positions of the warp and the object. Both the world warp and the warpbind mod have decay values that work together to limit the range of the effect. The warp decay value will effect all objects bound to it whereas the WarpBind decay will only effect the object it is attached to.

Class:
public class WarpBind : Modifier
{
	public GameObject	SourceWarpObj;
	public float		decay;
}

Param Description:
SourceWarpObj	-	Space warp scene object to use for the deformation
decay			-	Limit the range of the deformation for this object

****************************************
Warp
****************************************
This is the base class for Space Warps, so most of these params will work across all space warps. Exceptions being ripple where it ignores the size and uses a radius value.

Class:
public class Warp : MonoBehaviour
{
	public float	Width;
	public float	Height;
	public float	Length;
	public float	Decay;
	public bool		Enabled;
	public bool		DisplayGizmo;
}

Param Description:
Width		-	Width of the warp
Height		-	Height of the warp
Length		-	Length(depth) of the warp
Decya		-	Decay value for the effect, this will effect all objects bound to the warp
Enabled		-	Is warp on
DisplaGizmo	-	Enable display of the gizmo in the editor

****************************************
Warps
****************************************
All the warps below behave and have the same params as their mesh based versions, so please refer to those of help;

The warps so far include are:
	Bend Warp
	Noise Warp
	Ripple Warp
	Skew Warp
	Stretch Warp
	Taper Warp
	Twist Warp
	Wave Warp

****************************************
PivotAdjust
****************************************
You can use this simple modifier to alter the pivot position of a mesh and or change its orientation. Use the normal Offset and gizmo params to adjust the pivot.

****************************************
Morph
****************************************
Use the morph modifier to change the shape of the mesh towards given target meshes. You can have multiple morphing channels and each channel can have multiple target meshes to generate super smooth morphs.

Moprh support has been added back in. I have changed the whole system from my first morph system and it now works via .obj files so making it compatible with any 3d software that exports to the .obj format.

To prepare your meshes for use by the morpher you should first align all the targets and the base mesh to 0,0,0 in the editor software. You should then select the base undeformed mesh and export just that to a .obj file calling it something like BaseMapping.obj. The morph system will use this to match the unity vertex information to the morph vertex data. This is required as unity will very often add new vertices to a mesh when it imports it. Next you will need to export your targets. The morph system allows for any number of morph channels and each channel can have any number of targets making up the morph, so you could export each individual target as a seperate obj file, but a better way is to select all the meshes that make a morph and export to a single obj file. When you load the targets you will be able to change the order and add or delete targets if you need.

When you apply the Morph modifier to an object you will see a red button called 'Load Mapping' you need to click this and load the base mapping obj file. If the import went ok the button will go green to show you have valid mapping imformation. If the import fails it is probably because you didnt centre everything on 0,0,0 before exporting or the scales are wrong. Unity will apply a scaling to a mesh on import and you need to make sure the obj export is set to the same scale. If you think it is a scale issue there is a importScale value in the Morph interface you can adjust and try again.

Once you have the mapping in you will need to add a channel. You do this by clicking the Add Channel button. A new set of params will show up in the inspector. Click the Load Targets button and select the target obj file. The modifier will load all meshes in that file as targets and assign weights based on the order of meshes in the obj file. It may be that the order is not correct, if so use the Up/Dn buttons to reorganise the target list. You can also delete a target if you need to. If you select Load Targets again it will update any targets that are already imported and add any new ones it finds. You can also change the percentage at when each target is used to adust the animation. Also the morph uses a bezier interpolation system to give a much smoother animation result but sometimes this isnt desired, if so set the tension value to 0 to make it linear, a value of 0.5 usually gives a nice smooth result.

Each channel has a button to open and close the params just to tidy up the interface a little, the channel percentage slider will stay visible though so you can easily play with settings.

The morph system is still a work in progress so please report any issues or any new features you would like to see. The system uses mesh data from the 3d software as opposed to the unity mesh data for its processing, this gives a much more efficent system, for example the wood pipe example in the included scene has over a 1000 vertices one unity has imported it but only 666 vertices in the morph system. I will be adding further optimizations to this to remove any unmorphed verts in a channel which will really help memory and performance, not that its slow at the moment :)

****************************************
Path Deform
****************************************
The PathDeform modifier deforms an object using a spline as a path. You can move and stretch the object along the path, and rotate and twist it about the path. The usual gizmo and offset values can also be used to alter the look of the deformation.

Class:
public class PathDeform : Modifier
{
	public float		percent;
	public float		stretch;
	public float		twist;
	public float		rotate;
	public Axis			axis;
	public bool			flip;
	public MegaShape	path;
	public bool			animate;
	public float		speed;
	public bool			drawpath;
	public float		tangent;
}

Param Description:
percent		-	Moves the object along the path based on a percentage of the path length.
stretch		-	Scales the object along the path, using the object's pivot point as the base of the scale.
twist		-	Twists the object about the path. The twist angle is based on the rotation of one end of the overall					length of the path. Typically, the deformed object takes up only a portion of the path, so the effect can				be subtle.
rotate		-	Rotates the object about the path.
axis		-	Changes the alignment of the path to local axis of the object
flip		-	Reverses the direction
path		-	Path to deform along, must be a MegaShape object in the scene
animate		-	Automatically move the object along the path.
speed		-	Speed to move the object
drawpath	-	Display the deformation path
tangent		-	How far to look ahead to get the direction of the path

****************************************
MegaShape
****************************************
MegaShape is a spline object from another system Iam working on. It allows the user to build shapes using knots and control the shape with handles on each knot. To use create an empty object in the scene and add the MegaShape script to it and then click add knot. Each knot can be positioned and its handles altered to get the desired shape. There is an option to close the shape as well. I am still working on the making the MegaShape easier to use and will be adding features to it in further releases.

****************************************
Enumerations:
****************************************
public enum MegaAxis
{
	X = 0,
	Y = 1,
	Z = 2,
};

public enum MegaEffectAxis
{
	X = 0,
	Y = 1,
	XY = 2,
};

public enum MegaMeltMat
{
	Ice = 0,
	Glass,
	Jelly,
	Plastic,
	Custom,
}

public enum MegaNormType
{
	Normals = 0,
	Vertices,
	Average,
}

public enum MegaChannel
{
	Red = 0,
	Green,
	Blue,
	Alpha,
}

****************************************
Features to add if wanted or time permits:
****************************************
Apply deformation to child meshes
More examples
Submesh support to uv modifiers
Area support to uv modifiers
Option to instantiate or copy mesh when add mod obj

****************************************
Changes:
****************************************
v2.74
Added Squeeze Warp
Added Axis value to Squeeze and Squeeze Warp to give more control
Added a Flatten button to the inspector so you can quickly flatten any spline
Added a Remove twist button to the inspector to reset any twists on a spline
Added a SetHeight method to the API so you can quickly set all knots and handles for spline to the same height
Added a SetTwist method to the API so you can quickly set all the knots in a spline to the same twist angle
Added option to choose easing mode for twist values, either Linear or Sine at the moment.
Added id values to the spline knots.
Removed some Debug messages that were left in for Maya morph import.

v2.73
Knot twist values now used by World Path Deform.
Knot twist values now used by Path Deform.

v2.72
Changed the knot twist handle to make it simpler and less cluttered.
Fixed a slowdown that could happen with twist handles enabled.

v2.71
Added beta support for twist values per knot.
Knot twist value now used in the extruded mesh options box, tube and ribbon
Path Follow script can use knot twist values
Character Follow script fixed.
New methods in API to Interpolate and get twist along with position

v2.70
Added a BuildSplineWorld method to the api so splines can be built from world space points.
Added a beta of a new path follow script that will allow characters to be constrained to a path but otherwise be allowed to move freely and jump etc while on that path MegaCharacterFollow.cs

v2.69
Fixed a bug in constant speed open splines that have been reversed.

v2.68
Beta of new Create Prefab system added, will work with wrapped meshes as well.
Point Cache modifier can now use one cache file to define mapping then load another for actual cache data, so no longer does the first frame have to match the fbx file.

v2.67
Fixed a bug where constant speed interpolation was not being used for open splines.

v2.66
Added the ability to create animated splines inside Unity, you can now add keyframes for splines and have them played back for you.
Added BezPatch Mesh object, useful for image warping.

v2.65
Added MoveSpline methods to API for easy moving of entire spline
Added RotateSpline methods to API for easy rotation of entire spline
AutoCurve Button now only updates the current spline being edited
Added Gizmo to allow current spline to be moved in the editor for easy repositioning
Easier to add knots to a spline, click and drag the mid point circle to add new point
Added option to apply Bind Warp modifier to all siblings, right click inspector and click Add To Siblings

v2.64
Warp gizmos will now show up on newer versions of Unity.

v2.63
Dyanmic Hose system updated to match latest MegaShapes version.
Small fix to the tyreripple script, works correctly with box colliders now.

v2.62
Added TyreRipple.cs helper script to show how objects can leave wakes in Dynamic Ripple meshes.
Update to the SVG importer to handle path 'm' commands with multiple values ie fill a shape.
Improved peformance of spline interpolation when using constant speed.
Constant speed defaults to Normalized Interp for interpolation.

v2.61
Added Melt Warp Modifier
Flatness value added to Melt modifier to control the thickness of fully melted mesh
Flatness value added to Melt Warp to control the thickness of fully melted mesh

v2.60
Changes to Undo system.

v2.57
Fixed Undo error in 4.3 and other warnings.

v2.56
Added a 'smoothness' value to MegaShape which is used by the AutoCurve method, a value of 0 will result in sharp corners, a value of 2 will give very wide entries into knots, a value of 1.1 seems to be the best value for smooth curves.
Unity 4.3 new undo support.

v2.55
Removed unused meshes from package.

v2.54
Added a Label field to each modifier so making it easier to find the exact modifier you need in a complex stack.
Added code provided by Sergio  Barrera have a LOD value for each Modifier so they can be turned on and off via a lod manager.
Included the Mod Lod manager helper script for managing modifier lods.
Bug fix, when using meshes that change vertex counts the internal data structures were not being updated correctly, again thanks to Sergio May this has been fixed.
Train follow has debug lines for easier editing.

v2.53
Added helper script MegaTrainFollow that allows for multiple objects to follow eg like a train with carriages

v2.52
Conform added to fill spline meshing option.
Conform added to box spline meshing option.
Conform added to tube spline meshing option.
Conform added to ribbon spline meshing option.

v2.51
The debug mode for morph mapping failures should work correctly now.
Added beta of Conform modifier.

v2.50
MegaGrab removed for platforms that do not support it.
Fixed all warnings when importing into Unity 4.1 onwards

v2.49
Fixed a bug in the dynamic ripple causing an error when changing physics rows and col values.
Small change to dynamic ripple to work with Unity 4.2 better, box collider not being detected sometimes.
Maya exporter update to handle multiple targets (targets must be present in Maya)
Maya exporter update current blendshape weights now exported instead of being set to 0

v2.48
Mac Maya 2014 64 bit exporter available
Displace modifier now has a vertical offset value.
Displace Limits modifier now has a vertical offset value.
Option added to point cache modifier to map from the last frame of the data.
Added new modifier Point Cache Ref, this works the same as a point cache modifier but allows you to use another object in the scene as the data source so you dont need to duplicate the point cache data.
Point Cache Animator updated to work with Point Cache Ref as well.
Added new beta modifier Morph Ref, this works the same as a Morph modifier but allows you to use another object as the data source so you dont need to duplicate the morph vertex data.

v2.47
Added wave length value to Sinus Curve and Sinus Curve warp modifier
Warp objects can be created from the Game Object Create other menu now.
Compact mode in Morph inspector even more compact
Fixed a bug in mega wrap that might stop it wrapping onto non skinned meshes.
Now compatible with Windows App and Phone
Alpha of Animation blending in Morph Animator, use PlayClip(clip, blendtime) or PlayClip(clipname, blendtime)
Fixed some issues in the demo scene.

v2.46
You can disable the Unity undo feature per modifier this may increase the inspector speed on slower machines not using Unity 4.3
Text Labels no longer display if they are behind the camera.
Added new Globe Warp modifier, which is the Globe modifier but as a space warp.
Added new Waving warp modifier.
Added new Sinus Curve warp modifier.
Added new Hump Warp modifier.
Added new Cylindrify Warp modifier.
Added new Spherify Warp modifier.
Added new Bubble Warp modifier.
Added Deformable modifier for doing collision and crater type effects (alpha version only for testing)
Book script improved, pages should lay flat when pages controlled by keys.

v2.45
Added ability to view mapping data for point cache modifier to help fix mapping issues.
Fixed a bug in the point cache importer which was causing mapping failures on some meshes.
Add tolerance value to Point Cache modifier so if you get mapping failures try increasing this.
Speed option add to Point Cache Animator Clips
Speed option add to Morph Animator Clips

v2.44
Scrubbing time value on Point Cache modifier will now update mesh if animate is turned off.
Added option to morph inspector to show mapping data so in case of failures it can help show what the issue is.
Tolerance value added to inspector, for some very large meshes with morphs you may need to increase this value.
Fixed error in the Blender exporter if your morph had no animations.
Fixed Max 2013 and 2014 morph exporters, were crashing on progressive morph targets.
Fixed missing names on progressive morph targets on Max 2013 and 14 exporters.
Fixed missing first target percent value in progressive morphs when using Max 2013 and 14 exporters.
Maya exporter updated so only selected blendshape is exported instead of all blendshapes.

v2.43
Added a waveaxis option to the Waving modifier to make it easier to setup the wave direction.
Beta version of Maya 2014 64 bit blendshape exporter available.

v2.42
multi vol select missing position handle bug fixed
Selections now update automatically when you drag volumes around in multi volume select
Selection now update automatically when you drag volumes around in volume select
Adding a volume from multi volume select now updates selection automaitcally
Deleting a volume from multi vol select updates the selections
Changing order value in multi volume select works now
new waving modifier added
deleting all volumes from multi volume select now clears the selection
Enable value added to each volume in multi volume select
help page added for Multi Volume Select modifier
Help page added for Waving modifier

v2.41
Added Material Selection modifier allowing deformation of sub material vertices only in a mesh.
Added scaling option to tube meshing params so you can easily make tubes that get fatter or thiner
Added scaling option to box meshing params so you can easily make box tubes that get fatter or thiner
Added scaling option to ribbon meshing params so you can easily make ribbons that get wider or thinner
Max 2013 and 2014 exporters have had a memory bug fix.
Bug in the MCC point cache file parser fixed.

v2.40
Order value added to volume select modifier
Order value added to multi volume select modifier
Order value added to vert col select modifier
Curve Deforms Max Deviation value now works
Added the Pos value back in to the Curve Deform modifier
Fixed Vert Index slider on Skin Wrap causing error when slid to right extent.
Fixed bug in Morph Animator editor script that stopped some frame numbers from being entered
Fixed bug in Point Cache Animator editor script that stopped some frame numbers from being entered

v2.39
Enable option added to Mega Wrap so you can easily turn it off.
Beta Shrink option added to Mega Wrap so you can make wrapped meshes fit more closely to the target object, useful for clothing.
Mega Wrap now supports wrapping of non skinned meshes onto skinned ones, so all cases now work.
Wrap help page updated.

v2.38
Changed WaitOne method so valid on IOS stripped builds.
Added basic Line Shape to the helper shapes so you can create a simple straight line spline and set the number of points as a useful starting spline.
Added Beta of Constant Speed option to splines, you can choose per spline if it is a constant speed bezier spline or a normal bezier spline.
Calc Subdivs value added this will make length calculations more accurate the higher the value, will also make the constant speed interpolation more accurate. This can be set per spline.
Tank Track help page added.
Hose help page added.

v2.37
Added a check to mor loader to not load empty morph channels.
Added back in the global UseLimits on morph channels option, somehow got lost in an update.
Bug fixed in Bouyancy script which didnt take height of water object into account.
Dynamic Ripple improved to handle different orientations of mesh better.
No ripples appearing bug fixed in bouyancy script.

v2.35
MegaMorph Animator help page updated.
Point Cache animator help page updated.
Added a gap value to the Mega Wrap system so you can easily offset the wrap away from the surface or sink it in.
Mega Wrap help page added.
PlayClip methods on Morph and Point Cache animator can be called from Unity Animation events, bug in Unity 4.x work around.
Beta of Globe modifier added to deform objects to sit on a sphere.
Updated the docs for the MegaShapes system.
Added docs for the meshing options for MegaShapes.
Help page for Globe modifier added.

v2.34
MegaWarp can now wrap a skinned mesh onto another skinned mesh so making things like facial hair even easier as you only need to do the morphs etc for the head mesh then you can swap in new hair meshes that are wrapped to the head and they will deform alongside the head.

v2.33
DeepCopy and InstanceObject methods in MegaCopyObjects now return the GameObject they create.
Morph Animator can now control multiple morphs in a hierachy, so if you have a hierachy with multiple morphs you can animate all of them with one Morph Animator.
Morph animator now has option to describe clips in frames or in seconds.
Point Cache animator now has option to describe clips in frames or in seconds.

v2.32
Sped up display of vertex weights in volume selects
Fixed SetActive() error

v2.31
Added weighting value to Volume Select.
Added weighting value to Multi Volume select volumes.
Added Freeze Selection option to Volume select so current selection is frozen so saving time.
Added Freeze Selection option to Multi Volume select so current selection is frozen so saving time.

v2.30
Added a Multi volume Select modify for more advanced vertex selections, using sphere or box volumes.
Added box volume type to Vol Select modifier
Updated MegaGrab to latest version.
Fixed Mesh Fill option not working correctly if shapes were generated with an axis value or X or Z.
Fixed errors on 4.0 and 4.1 for SetActiveRecursively
Cursor position is now a per spline value instead of a single static.

v2.29
Added a error dialog when malformed PC2 files are loaded.
Added Do Late Update option to Hose system
Added option to disable hose updates if not visible
Added Tank Tracks System
Added Tank Wheels helper system

v2.28
Option to toggle spline display for Hose system.
Hoses named correctly when created.
Fixed hose not initializing correctly on creation.
The hose system now works correctly with prefabs.
Hose Freecreate mode disables automatically if end objects defined.

v2.27
Small fix to Morph Animator inspector.
Small fix to Point Cache Animator inspector.
Volume Select now has a target value so you can link the volume location to a gameobject.

v2.26
Imported Maya blendshape animations now 100% correct due to new Hermite animation controllers.
Maya exporter updated.
Point Cache modifier has improved mapping detection options.

v2.25
Removed error when platform set to Flash.
Mega Deep Copy added to GameObject menu which will allow copying of objects and skinned objects in the scene, MegaCopyObject.DeepCopy(obj) can be used from script.
Fixed bug in Point Cache Animator that stopped it working.
Added Mega Create Instance to GameObject menu to make proper instance copies of Modified objects in the scene. MegaCopyObject.InstanceObject(obj) can be used from script.
Added Mega Hose system to MegaShapes, easily connect two objects with a flexible hose.
Added Mega Hose Attach to MegaShapes, attach objects to the hose.
Fixed a bug in the morph animation playback when used with mor files exported from Maya.
Maya morph exporter updated to fix animation issue.

v2.24
NormMap renamed to MegaNormMap to avoid naming clashes.
Fixed Load Mapping bug when working with OBJ files and morphs.
Mega Wrap improved you can now set the max number of mapping points to use and the best ones will be used.
Fixed bug in Mega Copy Object when used with skinned meshes.

v2.23
Optimized stretch modifier.
Optimized taper modifier.
Optimized squeeze modifier.

v2.22
Fixed bug in Helix shape with one of the knot handles being wrong.
Improvements to the Create Prefab system.

v2.21
Alpha of Mega Wrap system released
Small update to the book script will now disable MegaFiers on pages that dont need updating making it much faster for mobile devices.
Option added to stop deformation updates when object not visible.
Added a Create MegaShape Prefab option to the GameObject menu so you can easily create prefabs out of lofts or shape meshes.
Removed some debug logging messages when loading SVG and SXL files.
Center Offset right click option works correctly now
Center GizmoPos right click option works correctly now

v2.18
Added curve selection option to Path Deform Modifier
Added curve selection option to World Path Deform Modifier
Added new spline import file format.
Support added for SXL file import, sxl is a xml based spline format allowing users to easily write their own spline exporters.
Bezier curve Exporter for Maya available

v2.17
Editing of splines works correctly for scaled and rotated splines
SVG import now uses Axis value on import for orientation.

v2.16
Added Ribbon option to mesh types when converting Spline to a mesh.
Fixed MeshRenderer being added to object when it wasn't required.
Added Late Update option to shapes.
Pivot offset value now works for tube, box and ribbon meshing options.
Added UV offset param to tube, box and ribbon meshing options.
When loading SPL or SVG files you now have the option to Replace existing splines or add to the splines making the shape.

v2.15
Limits work correctly for MorphOMatic morphs now.
Changing limits on MorphOMatic morph no longs alters target percents and messes up morphing.
Added option to limit number of MorphOMatic channels displayed in inspector.

v2.14
Added Elipse support to SVG importer.
Fixed splines not showing up when selected in Unity 4.0
Added a Reverse spline option to the Shapes inspector, will reverse the currently selected spline
Added an Apply Scaling button to the inspector so if you scale the shape using the transform click this to correctly scale the splines
Added an Outline spline system, you can now ask the system to make a new spline that outlines the current one with control over the outline distance.

v2.13
Added an Up option to the path deform modifier.
Added an Up option to the world path deform modifier.

v2.12
SVG importer rewritten and greatly improved.
CursorPos on shapes now works correctly for the selected curve.
Added Help option to FFD Animate component and added page to docs site.

v2.11
Added ability to limit number of channels displayed at one time in the inspector so as not to slow down when morphs have lots of channels.
Added start of SVG import support.

v2.10
Optimized the path deform modifiers.
Added Box type to meshing options for splines.
Added Tube type to meshing options for splines.

v2.09
Added help page to the MegaAttach system.
Increased MegaMorph Animchannels from 20 to 50
Fixed issue that would cause MorphAnimator to play on start even when option not checked.
Fixed issue that would cause Point Cache Animator to play on start even when option not checked.
Point Cache time value works correctly without having animated box checked
Alpha version of Mega Collision Deform modifier added (very early WIP)
Alpha version of Mega tree Bend modifier added (very early WIP)
Added more angle options to the Morph Link system.

v2.08
Multithreaded Curve Sculpt support added
Multithreaded Curve Sculpt Layered support added

v2.07
FFD modifiers work correctly with multi threading again.
Multithreaded Curve Deform support added.
MegaBook script updated to fix texture flips due to float precision on larger books.

v2.06
Added custom inspector for Bend Warp
Added custom inspector for Noise Warp
Added custom inspector for Ripple Warp
Added custom inspector for Skew Warp
Added custom inspector for Stretch Warp
Added custom inspector for Taper Warp
Added custom inspector for Twist Warp
Added custom inspector for Wave Warp
Adding warps from the MegaFiers window now works
Added missing docs for Warps to website
All Warp Help now go to their help pages
Ripple Warp gizmo not matching warp fixed.
Added autosmooth option to shapes.
Added BuildSpline method.
Added various methods to aid in spline building via script.

v2.05
Mega Attach now works with skinned meshes
Added a gizmo to Dynamic Ripple to show the physics grid
Changed Path to System.IO.Path to stop a clash with TDK that has an internal class called 'Path' The author is fixing his code.

v2.04
Added new modifier, Displace Limits, works the same as displace but has a bounding box to limit the area of effect.
Updated Dynamic Ripple Modifier to stop errors
Dynamic ripple automatically updates its physics system on changing params
Added support for box collider instead of just mesh collider for ripple interaction.
Added option for different handle types on Shapes as some people reported big slowdowns in 3.5.3

v2.03
Fixed error when trying to do link animation on morph and point cache animators when no animation is present on the object.

v2.02
Bug fixed of objects not being created if a scene view isn't selected.
Fixed bug that stopped Util classes being added from MegaFier window to objects with no meshfilter.

v2.01
Added in the missing MegaCurveSculptLayered modifier

v2.0
Relocated MegaWaveMesh script so it is part of the main system, you may need to select the script again if you are using the object in your scene.

v1.99
You can now add channels to a morph by loading other mor files to the same object.
Morph mapping sped up.
ModBut renamed to MegaModBut to avoid ay potential naming conflicts with other packages

v1.98
Made sliders in Morph Animate script match the min max limits of the channel they control
Added Copy Object and Copy Objects button to Inspector to allow creation of new objects that will modify indpendantly
FFD Gizmos now respect the Modify Object Draw Gizmo value
Copying FFD components now works correctly.

v1.97
Add support for FFD lattices to be animated via the Unity Animation system
Curve Sculpt modifier now has layers of curves
Curve Sculpt modifier can now be limited to regions of a mesh
Can add and delete curves to a shape
Shape editor allows you to select the curve to edit
Improvements to Shape editing and inspector

v1.94
Fixed a bug in the SetKnot method where tangents weren't being set.
Added Curve Sculpt Modifier
Spline handles no longer scale with the object
FFD handles no longer scale with the object
Improvements to the Spline and FFD handle code

v1.92
Improvements to the shape editing code.
Undo works better on spline editing
Undo and Redo works for all modifiers now
Undo on FFD lattice point movements works properly

v1.91
Added collider support to 2D Wave mesh
Fixed Copy Object problem with Morphed objects

v1.89
Fixed a very rare point cache exception on some HW.
Point cache animator play on start fixed
Point cache animator works again, was broken in previous update
Added a 2D Wave mesh object

v1.88
Added an axis param to the Cylindrify modifier
CopyObject now copies MegaModifyObject params as well
FFD now has option for position handles instead of free handles for easier editing
FFD has a size option for handles
FFD has mirror x/y/z option to allow easier adjusting of lattice
Rubber modifier now has an option to pick a target transform to control the effect
Added color option to FFD Gizmo
Added undo for FFD lattice point move
Greatly improved performance of FFD modifiers
FFD will fit lattice to selection is present
Context menu item for fitting FFD to selection
Context menu item for fitting FFD to mesh
World Path Deform optimized
Copy Object no longer adds MegaModifyObject if not needed
Added Copy Object Hier to copy an object and all its children
Morph now supports values outside of 0 to 100 percent and respects Min and Max values
Beta of Morph Link system which allows morphs to be easily driven by other object positions or rotations so easy to drive a bicep muscle bulge from arm bone rotation for example

v1.87
Some changes to the beta dynamic ripple modifier
Added a Reset Physics button to dynamic ripple modifier

v1.86
Channel 8 and 9 typos in MegaMorphAnim fixed.
Some source files were in the wrong place so have been moved, if you get errors about duplicated names delete the dup files from the MegaFier Test Scene/Script folder

v1.85
Modify Object component automatically added when adding first modifier
Added a public LoadFile() method to point cache editor to allow for files to be loaded via scripts.
Added a point cache instance script to allow for easy instancing of point cache objects without overheads of data storage.
Knot handles etc orientation bug fixed.
Physical UV generation works for shape meshes now
Added linked update option to Morph Animator so can link morph animation playback to current animaton.
Added Play on Start option to Morph Animator
Added beta version of dynamic Water modifier
Added webcam version displacement modifier
Added linked update option to Point Cache Animator so can link moprh animation playback to current animaton.
Added Play on Start option to Point Cache Animator
Added a Mega Copy Mesh so you can use a mesh with different modifiers on without interference.
Check added to MegaShape DrawGizmo to limit number of segs used to display to avoid slowdown.
Number of channel Morph Anim helper supports increased to 20
Added MegaAttach system to allow objects to be linked to a point on a deforming objects surface.
Added a MegaCopyObject system to allow you to make copies of objects with ot without MegaFier components on that will work independantly.

v1.84
Added a Point Cache animation clip system like the morph system.
Fixed bug on Path Deform path drawing which showed knots too big and path step to large.
Fixed uv mapping bug on Page Mesh.
Fixed wrong check for UNITY_FLASH builds that stopped multithreading from being enabled
Removed workarounds for Unity bug of Handles.matrix not being used that was fixed in 3.5
Tangents optionally calculated on Shape meshes now
Added custom inspector for Rope Deform and updated webpage
Fixed error in Megafiers window when adding warps.
Adding warps from the window no longer adds an undeeded Modify Object component.
Fixed display of FFD lattices in 3.5
Added a SetPoint and SetPointLocal methods to FFD so making it easier to control FFD lattice from script.

v1.83
Vertical noise gizmo fixed to fit mesh correctly.
Vertical noise works correctly in multithreaded mode.
Noise modifier works correctly for multithreaded mode.
Fixed Rubber Modifier not using Enabled value
Added a cursor to the MegaShapes so you can create new knots in a more user friendly way
Added a findnearest point method to MegaShapes
Added MegaShapes window to GameObject menu for easier creation of splines
Added a MegaFiers Window for a simpler way to add components.
Improved the knot manipulation interface for MegaShapes, ie when dragging knots or handles they change values based on the dragplane of the current camera, ie if camera is pointing down dragging only happens on the x z plane.
Knotsize value changed to give larger useable range
You can now build meshed from your splines.
Added a height value to the MegaShape mesh builder to add depth to shapes.
UV support for megashape meshes
New modifier, Squeeze, beta.
MegaGrab added to the package.

v1.79
Fixed a bug in Morph-O-Matic exporter for non triObject based meshes, ie edit poly.
Added a Gizmo Detail value to control draw steps on gizmos.
Updated more help file links to new website
Updated more modifiers to new inspector layout
Book script updated to use local pos and rots to make easier to use
Removed all errors when compiling to Flash (note first 3.5 preview has a bug with serializing meshes)

v1.78
Updated a lot of the inspectors for the modifiers to tidy them up.
Relinked help pages to the new website.
Added beta versions of Curve Deform modifier
Added beta version of Rope deform modifier

v1.77
Doubled the speed of progressive morphs (ie more than one target on a channel)
Fixed bug in pagemesh that could mess uv coords up for last strip
Fixed a Morph bug where if there was a mix of single target channels and progressive some channels possibly didnt function.
Removed more Debug.Log calls from Morph Loader.

v1.75
Tidied up folders and moved entire project to a single folder for easier addition/removal. You should bacup your project before importing new version and you may need to relink scripts.
Removed some debug.logs from the code.
Change FFD handles to freehandles.
Add animation methods to morph to play clips.
Added MegaMorphAnimator component to define animclips (beta).
Fixed bug where Modifiers added at runtime in the editor were not being initialised correctly.
Fixed crash in vertex anim and point cache modifiers in Multithreaded mode.
Added start of support for vertex selection sets for modifiers.
Added Vol Select modifier for vertex selection (beta).
Added RGB color vertex select (beta).
Added Sinus Curve modifier.
Added Vertex Paint Modifier (beta).
Added Crumple modifier.
Added Morph-O-Matic morpher (beta, requires 3ds max exporter).
Added Scroll.cs script to package.

v1.66
Added channel id to channel buttons to aid scipting of morph channels.

v1.65
Fixed issue with the displace modifier, now works again.
Added displace example to test scene.
Fixed bug that corrupted normals when more than one displace modifier added to a mesh

v1.64
Fixed the values in the UVTile test object to show correct functionality.
Added animation looping option to UVTile playback;
Added UpdateCol methods to MegaModifyObject so colors can be adjusted for col/weight based modifiers.
Added helper methods to rubber modifier to alter color/weights with auto update of physics. (Currently update physics is slow so ideally not to be called often).

v1.63
Added a stiffness channel to rubber so vertex colors can define areas of stiffness on a rubber object.
Changed rubber modifier so a mesh with no colors will have no effect.
Added a 'None' weight channel.

v1.61
Tidied scene up and removed unused materials.
Fixed null reference error when an imported spline had no animations.
Fixed melt modifier to stop -ve melt amounts being used.
Fixed normals getting altered if normal mode is Mega and user selects 'reset mesh info' by right clicking the modify object bar in the inspector.

v1.59
Fixed bug where morph wasnt updating when channel percent to zero.
Fixed bug where morphed object could vanish if all channels were set to 0 percent.
Added Rubber modifier.

v1.58
Added support for Maya .mc geometry cache animation files to the point cache modifier

v1.57
Added weight param for Addtive blend on Point Cache modifier
Added weight param for Addtive blend on Vertex Cache modifier
Added support for .mdd files to Point Cache Modifier
Auto mapping of vertices for importing point cache files

v1.53
Added blend mode to Point Cache modifier so it works nicely with other modifiers and you can stack multiple anims.
Added blend mode to vertex anim modifier.

v1.52
Point Cache modifier added with support for loading .pc2 files.

v1.51
Support for animated splines.
Added shape export to Max exporter, supports animated shapes as well.
Added master point controller modifier.
Added master point controller export to the Max exporter.
Added Morph-O-Matic export to the Max exporter.

v1.50
New method of recalcultaing normals has been added, this is an alternative to the Unity method and unlike that method it preserves smoothing group info from the original imported model.
Normal calculator has a multicore version.
Tangent calculator has been optimized.

v1.48
Morpher has been multithreaded.

v1.47
Multithreaded noise working properly.
More optimizing to the morpher can now be anything up to 40 times faster than it was.

v1.46
On loading morph file the inspector channels are shown in condensed form.
Button to show or hide the Morph channel data in the inspector.
Morpher has been optimized with over a 300 percent boost in performance, more to come.

v1.41
Added multi core support. Only tested on Windows so far. Multi Core pipeline can be globally enabled as well as per modified object. Multi core only works in Play mode and is turned off by default so you will have to turn it on in the inspector after play mode or have a little script to activate it by setting the static bool MegaModifiers.ThreadingOn param to true;

v1.40
Demo scene updated so Gizmos show up after rename.

v1.39
All classes have been renamed with a 'Mega' prefix to avoid conflict with other assets. This will break existing scenes so be aware of that before updating to this new version.
Changed MegaShape Position handles for knots etc to FreeHandles as PositionHandles are very very slow in Unity 3.4
Changed my tab settings to 4 instead of 2 so code will look nicer for standard 4 space tab settings.
ModifyObject.cs file moved to correct folder, deleting test scene wont cause an error now.
Added the book maker script MegaBook.cs to the system.

v1.37
Fixed bug where path deforms animated twice as fast with gizmo on and moved while editing
Added Blender ShapeKey import support via .mor files
Added morph animation import support
Started modify group of objects, not finished yet.
Blender 2.58 morph exporter
More pages added to help web site.

v1.36
Fixed a bug in the Morph Anim system where the sliders were not being connected to the channels until runtime.
Max exporter for morph data now has 64bit versions.
Max exporter also handles Local Offsets on morphed meshes now.

v1.35
Added direct import of morph data from 3ds max via custom .mor file. Email me with invoice details from the Asset Store for the 3ds max plugin. Currently for Max 2010 and 2011 32bit.

v1.34
Added collider support. Has to use a mesh collider and no proxy meshes yet, and if the mesh collider is convex that can be quite slow.
Added tangent support. Check box on the modify object gui.
Fixed bug where you couldnt create more than one knot on a basic spline.
Fixed missing gizmo icons on shape objects.

v1.33
Added option to Path Deforms for curve based twist and stretch.
World Path Deform modifier added.
Online help page started www.west-racing.com/megadoc
Help built into each script, right click the toolbar for a script and selct help to open its page.
Icons moved back to gizmo directory as they didnt work in a sub directory.
Removed help images from package as on web site now.

v1.32
Directory structure tidied up to make identifying what is part of MegaFiers easier.
PageMesh automatically adds 3 materials on creation. First will be front of page, second is back and thrid is page edge if the page has a height.
Updating the PageMesh no longer needs a GrabVerts change in ModifyObject, all handled automatically. All changes to the mesh work right away with MegaFiers.

v1.31
Added a Page Mesh object to make double sided pages. The mesh can have a height but this will get flattened in the page turn modifier. I will be improving the page turn modifier. You can find the Page Mesh in the Mega Shapes menu.
Added a FlipX param to the page flip mod, fixes a problem when when meshes are orietated the wrong way, if you cant get a true page turn try turning this on. It should be on for Page Mesh meshes.

v1.30
Displace wasnt appearing in modifier list in the components menu
Added a popup in the inspector for Morph Anim to make it easier to select channels.
Note there is a bug in the Unity animation system that doesnt allow more than one of the same component to be animated
Morph Anim works in editor mode now.
Added various shapes to the spline system to make making paths a little easier. Shapes are Circle, Arc, Helix, Rectangle, Ellipse, NGon, Star.
Added a window for the shapes.
Shapes can be added from the GameObject menu now.

v1.28
More improvements to the GUI systems.
Spline creates a circle to start with and has a radius button. Add and delete knots works better.

v1.26
Didn't quite get the scaling bug fixed, thanks to p6r its sorted now.

v1.25
Fixed some gui items not having immediate effect
Added PathDeform modifier
Added MegaShape spline editor to create paths for Path Deform modifier
Fixed bug in morph mapping that meant scaled obj files weren't detected correctly.

v1.24
Commom params for modifiers grouped into a foldout and laid out better
Sort button removed as will automatically sort on changing an order value
Mapping or morph verts will find correct obj in file with more than one obj file in it
Added a MorphAnim script, Unity Animation system doesnt pick up values in arrays etc, so to be able to make use of the animation system drop a MoprhAnim script on the morph object, set the name of the channel to one in the morph object then you can use the Percent value in the Anim scipt to set the morph amont.

v1.23
Added auto detection of import params for obj files when adding mapping to a morph
Fixed slow loading on some obj files.
Deleted modifiers correctly removed from internal list
Fixed bug that stopped a morphed mesh with further modifiers applied from being deformed.

v1.21
Fixed bug with FFD mods where deformations wern't happening until a reset was applied.

v1.20
Added import settings for obj files as some packages export with a different up axis and handiness.
Added a progress bar to show mapping progress on very detailed meshes.
Note, obj files with /r line endings as opposed to /n/r parse very slowly due to a Mono bug, if you are going to do a lot of work with obj files and loading the targets or mapping is taking many seconds try converting the line endings.

v1.19
Fixed bug reading obj files from different packages.

v1.18
Shows memory usage for morph
Added common modifier params to morph gui
Loading targets to an empty channel will set the channel name to the filename of the targets
Error boxes shown for mapping or target import issues.
Added limits to a morph channel so you can use only part of a morph or reverse a morph etc.

v1.17
Fixed bug with a single target morph channel.

v1.15
Added morph modifier.
Added a PivotAdjust modifier, allows you to reposition the pivot, and or rotate/scale the mesh to get a new starting mesh.
Added gizmos to show gizmo offset and position details.
Changing a warp source on a WarpBind modifier now works properly.
Tidied up the UVAdjust mod, uses gizmo values and Offset now instead of duplicating params;
Added gizmo display for UVAdjust

v1.12
Added wip page flip modifier
Added UV Adjust modifier
Added UV Tile/Animate modifier
Added Space Warp support
Added Bend warp modifier
Added Noise Warp modifier
Added Ripple Warp modifier
Added Skew Warp modifier
Added Stretch Warp Modifier
Added Taper Warp modifier
Added Twist Warp modifier
Added Wave Warp modifier
Added colors to gizmo display
Region gizmos positioned correctly now.
Instancing bug fixed. You will get an error about leaked meshes in edit mode, currently not a lot can be done about this, ezgui, ragespline etc have the same message.
Hid some params from the animator.
Added support for uv modifiers.
Added gizmo icons for scripts.
Added help images to each script.
Added editor scripts and gizmo to move offset.
Added falloff to Spherify modifier.
Added Bulge modifier, works like Spherify but per axis.
Added Cylindrify modifier.
Added context functions to reset/center Offsets and gizmo positions and rotations. (Right click script bar)
Fixed chance of gimzo orientation being wrong when modifiers attached to parent of mesh object.

v1.11
Changed how system works so everything works seemlessly in edit mode or when scene is running.Fixed bug in FFD
Added new test scene
Ripple gizmo shows correct rotation now
Wave gizmo shows correct rotation now
Added DoRegion gizmo to bend
Added a better example scene
Started in Editor support for mesh modification
Added speed param to ripple
Added speed param to wave
Fixed bug where multiple Modifiers classes were being added to copied objects.


END-USER LICENSE AGREEMENT FOR Mega-Fiers Script

IMPORTANT PLEASE READ THE TERMS AND CONDITIONS OF THIS LICENSE AGREEMENT CAREFULLY BEFORE CONTINUING WITH THIS PROGRAM INSTALL: Chris West End-User License Agreement ("EULA") is a legal agreement between you (either an individual or a single entity) and Chris West for the Chris West software product(s) identified above which may include associated software components, media, printed materials, and "online" or electronic documentation ("Mega-Fiers Script"). By installing, copying, or otherwise using the Mega-Fiers Script, you agree to be bound by the terms of this EULA. This license agreement represents the entire agreement concerning the program between you and Chris West, (referred to as "licenser"), and it supersedes any prior proposal, representation, or understanding between the parties. If you do not agree to the terms of this EULA, do not install or use the Mega-Fiers Script.

The Mega-Fiers Script is protected by copyright laws and international copyright treaties, as well as other intellectual property laws and treaties. The Mega-Fiers Script is licensed, not sold.

1. BUGS IN SOFTWARE
Mega-Fiers does not claim that the software has no bugs and does not guarantee that it will work in any environment.

2. GRANT OF LICENSE.
The SOFTWARE PRODUCT is licensed as follows:
(a) Installation and Use.
Chris West grants you the right to install and use copies of the Mega-Fiers Script on your computer running a validly licensed copy of the operating system for which the Mega-Fiers Script was designed [e.g., Windows 95, Windows NT, Windows 98, Windows 2000, Windows 2003, Windows XP, Windows ME, Windows Vista, Windows 7, OSX].
(b) Backup Copies.
You may also make copies of the Mega-Fiers Script as may be necessary for backup and archival purposes.

3. DESCRIPTION OF OTHER RIGHTS AND LIMITATIONS.
(a) Maintenance of Copyright Notices.
You must not remove or alter any copyright notices on any and all copies of the Mega-Fiers Script.
(b) Distribution.
You may not distribute copies of the Mega-Fiers Script to third parties.
(c) Rental.
You may not rent, lease, or lend the Mega-Fiers Script.
(d) Support Services.
Chris West may provide you with support services related to the Mega-Fiers Script ("Support Services"). Any supplemental software code provided to you as part of the Support Services shall be considered part of the Mega-Fiers Script and subject to the terms and conditions of this EULA.
(e) Compliance with Applicable Laws.
You must comply with all applicable laws regarding use of the Mega-Fiers Script.

4. TERMINATION
Without prejudice to any other rights, Chris West may terminate this EULA if you fail to comply with the terms and conditions of this EULA. In such event, you must destroy all copies of the Mega-Fiers Script in your possession.

5. COPYRIGHT
All title, including but not limited to copyrights, in and to the Mega-Fiers Script and any copies thereof are owned by Chris West. All title and intellectual property rights in and to the content which may be accessed through use of the Mega-Fiers Script is the property of the respective content owner and may be protected by applicable copyright or other intellectual property laws and treaties. This EULA grants you no rights to use such content. All rights not expressly granted are reserved by Chris West.

6. NO WARRANTIES
Chris West expressly disclaims any warranty for the Mega-Fiers Script. The Mega-Fiers Script is provided 'As Is' without any express or implied warranty of any kind, including but not limited to any warranties of merchantability, noninfringement, or fitness of a particular purpose. Chris West does not warrant or assume responsibility for the accuracy or completeness of any information, text, graphics, links or other items contained within the Mega-Fiers Script. Chris West makes no warranties respecting any harm that may be caused by the transmission of a computer virus, worm, time bomb, logic bomb, or other such computer program. Chris West further expressly disclaims any warranty or representation to Authorized Users or to any third party.

7. LIMITATION OF LIABILITY
In no event shall Chris West be liable for any damages (including, without limitation, lost profits, business interruption, or lost information) rising out of 'Authorized Users' use of or inability to use the Mega-Fiers Script, even if Chris West has been advised of the possibility of such damages. In no event will Chris West be liable for loss of data or for indirect, special, incidental, consequential (including lost profit), or other damages based in contract, tort or otherwise. Chris West shall have no liability with respect to the content of the Mega-Fiers Script or any part thereof, including but not limited to errors or omissions contained therein, libel, infringements of rights of publicity, privacy, trademark rights, business interruption, personal injury, loss of privacy, moral rights or the disclosure of confidential information. 
