
using UnityEngine;

public class TyreRipple : MonoBehaviour
{
	public MegaDynamicRipple	surface;
	public float				force = 1.0f;

	bool		lastdown = false;
	float		lastrow = 0.0f;
	float		lastcol = 0.0f;
	Collider	col;
	Vector3		lastpos = Vector3.zero;

	void Update()
	{
		if ( surface )
		{
			col = surface.GetComponent<Collider>();

			if ( col )
			{
				Vector3 pos = transform.position;

				if ( pos != lastpos )
				{
					lastpos = pos;
					RaycastHit hit;
					Ray ray = new Ray(pos, Vector3.down);

					if ( col.Raycast(ray, out hit, 20.0f) )
					{
						if ( col is BoxCollider )
						{
							Vector3 p = surface.transform.worldToLocalMatrix.MultiplyPoint(hit.point);
							BoxCollider bc = (BoxCollider)col;
							if ( bc.size.x != 0.0f )
								p.x /= bc.size.x;

							if ( bc.size.y != 0.0f )
								p.y /= bc.size.y;

							if ( bc.size.z != 0.0f )
								p.z /= bc.size.z;

							p.x += 0.5f;
							p.y += 0.5f;
							p.z += 0.5f;

							float column = 0.0f;
							float row = 0.0f;

							switch ( surface.axis )
							{
								case MegaAxis.X:
									column = (p.y) * (surface.cols - 1);
									row = p.z * (surface.rows - 1);
									break;

								case MegaAxis.Y:
									column = (p.x) * (surface.cols - 1);
									row = p.z * (surface.rows - 1);
									break;

								case MegaAxis.Z:
									column = (p.x) * (surface.cols - 1);
									row = p.y * (surface.rows - 1);
									break;
							}

							if ( lastdown )
								surface.Line(lastcol, lastrow, column, row, -force);
							else
								surface.wakeAtPointAdd1((int)column, (int)row, -force);

							lastdown = true;
							lastrow = row;
							lastcol = column;
							return;
						}
						else
						{
							float column = (1.0f - hit.textureCoord.x) * (surface.cols - 1);
							float row = hit.textureCoord.y * (surface.rows - 1);

							if ( lastdown )
								surface.Line(lastcol, lastrow, column, row, -force);
							else
								surface.wakeAtPointAdd1((int)column, (int)row, -force);

							lastdown = true;
							lastrow = row;
							lastcol = column;
						}
					}
					else
						lastdown = false;
				}
				else
					lastdown = false;
			}
			else
				lastdown = false;
		}
	}
}