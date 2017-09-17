using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{

  class CRSpheresGenerator
  {
    const int maxVertices_ = 65535;

    List< Vector3 > listPos;
    List< int >     listIndexPos;


    public CRSpheresGenerator()
    {
      listPos      = new List<Vector3>();
      listIndexPos = new List<int>();
    }

    private void Clear()
    {
      listPos     .Clear();
      listIndexPos.Clear();
    }

    public void AddSphere8Faces6Vertices(Vector3 position, float radius)
    {
      int curIndex = listPos.Count;

      listPos.Add( new Vector3( position.x - radius, position.y          , position.z           ) );
      listPos.Add( new Vector3( position.x,          position.y - radius , position.z           ) );
      listPos.Add( new Vector3( position.x,          position.y          , position.z - radius  ) );
      listPos.Add( new Vector3( position.x + radius, position.y          , position.z           ) );
      listPos.Add( new Vector3( position.x,          position.y + radius , position.z           ) );
      listPos.Add( new Vector3( position.x,          position.y          , position.z + radius  ) );

      int x_n = curIndex + 0;
      int y_n = curIndex + 1;
      int z_n = curIndex + 2;
      int x_p = curIndex + 3;
      int y_p = curIndex + 4;
      int z_p = curIndex + 5;

      listIndexPos.AddRange( new int[] { z_n, y_n, x_n } );
      listIndexPos.AddRange( new int[] { x_p, y_n, z_n } );
      listIndexPos.AddRange( new int[] { x_n, y_p, z_n } );
      listIndexPos.AddRange( new int[] { z_p, y_p, x_n } ); 

      listIndexPos.AddRange( new int[] { x_n, y_n, z_p } );
      listIndexPos.AddRange( new int[] { z_p, y_n, x_p } ); 
      listIndexPos.AddRange( new int[] { z_n, y_p, x_p } ); 
      listIndexPos.AddRange( new int[] { x_p, y_p, z_p } );
    }

    public bool CanAddSphere()
    {
      int currentVertices = listPos.Count;

      if ( (currentVertices + 6 > maxVertices_ ) )
      {
        return false;
      }

      return true;
    }

    public Mesh GenerateMesh()
    {
      Mesh mesh = new Mesh();

      mesh.SetVertices(listPos);
      mesh.triangles = listIndexPos.ToArray();

      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
      mesh.Optimize();

      Clear();

      return mesh;
    }

    public bool HasMeshAvailable()
    {
      return ( listPos.Count > 0 );
    }
  }


}
