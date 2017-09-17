using System;
using UnityEngine;
using System.Collections.Generic;
public class VectorUtility 
{
    public static Vector3 GetNormalToVectorByPoint(Vector3 point, Vector3 vector)
    {
        if (vector == Vector3.zero)
        {
            throw new ArgumentException("vector");
        }

        float numerator = point.x*vector.x + point.y*vector.y + point.z*vector.z;
        float denominator = vector.x*vector.x + vector.y*vector.y + vector.z + vector.z;

        float conffcient = numerator/denominator;

        Vector3 cross = conffcient*vector;

        return point - cross;
    }

    public static Vector3 NormalRotateAroundAxis(Vector3 normal, Vector3 axis, float angle)
    {
        if (axis == Vector3.zero)
        {
            throw new ArgumentException("axis");
        }

        Vector3 cross = Vector3.Cross(normal, axis);

        return Mathf.Cos(angle)*normal + Mathf.Sin(angle)*cross;
    }

    public static float CalculateScaleByVectorSumLength(Vector3 source, Vector3 target, float length)
    {
        if (length == 0) return 0f;
        
        var a = Mathf.Pow(target.x, 2) + Mathf.Pow(target.y, 2) + Mathf.Pow(target.z, 2);
        var b = 2*source.x*target.x + 2*source.y*target.y + 2*source.z*target.z;
        var c = Mathf.Pow(source.x, 2) + Mathf.Pow(source.y, 2) + Mathf.Pow(source.z, 2) - Mathf.Pow(length, 2);
        var b24ac = Mathf.Pow(b, 2) - 4*a*c;
        if (a == 0) return 0f;
        if (b24ac < 0f) return 0f;

        var sqrt = Mathf.Sqrt(b24ac);
        var x1 = (-b + sqrt) / 2*a;
        var x2 = (-b - sqrt) / 2*a;
        if (x1 < 0f && x2 < 0f)
        {
            Debug.LogError("this is impossible, please check out!");
        }

        return x1 > x2 ? x1 : x2;
    }

    public static List<GameObject> GetColliderObjectAround(Vector3 center,float maxdistence,ref List<GameObject> items, string tagfliter = null)
    {
        RaycastHit[] hits = Physics.SphereCastAll(center, maxdistence, Vector3.right, 0);
        Debug.DrawRay(center, Vector3.up, Color.red);
        items.Clear();
        for (int i = 0; i < hits.Length; i++)
        {
            if (tagfliter == null || hits[i].collider.CompareTag(tagfliter))
                items.Add(hits[i].collider.gameObject);
        }
        return items;
    } 
}
