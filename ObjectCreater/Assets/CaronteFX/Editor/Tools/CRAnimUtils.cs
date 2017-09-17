using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public static class CRAnimUtils
  {
    public static bool IsCoordinatedWith(this Quaternion q, Quaternion other)
    {
      bool ret = (Quaternion.Dot(q, other) >= 0);
      return (ret);
    }

    public static Quaternion Negate(this Quaternion q)
    {
      return (new Quaternion(-q.x, -q.y, -q.z, -q.w));
    }

    public static void CoordinateWith(this Quaternion q, Quaternion other)
    {
      if (!IsCoordinatedWith(q, other))
      {
        q = Negate(q);
      }
    }

    public static bool ClampCircularReal(ref float value, float min, float max)
    {
      float diameter = max - min;
      if (value < min)
      {
        float delta = min - value;
        if (delta > diameter)
        {
          delta = delta % diameter;
        }
        value = max - delta;
        return (true);
      }
      else if (value > max)
      {
        float delta = value - max;
        if (delta > diameter)
        {
          delta = delta % diameter;
        }
        value = min + delta;
        return true;
      }
      else
      {
        return false;
      }
    }

    public static void MakeAngleCompatible(ref float angle, float fixed_angle)
    {
      ClampCircularReal(ref angle,
                         fixed_angle - 180,
                         fixed_angle + 180);
    }


    public static void SetKeysLinear(Keyframe[] arrKeyframe)
    {
      for (int i = 0; i < arrKeyframe.Length; ++i)
      {
        float intangent = 0;
        float outtangent = 0;
        bool intangent_set = false;
        bool outtangent_set = false;

        Vector2 point1;
        Vector2 point2;

        Vector2 deltapoint;

        if (i == 0) { intangent = 0; intangent_set = true; }
        if (i == arrKeyframe.Length - 1) { outtangent = 0; outtangent_set = true; }

        if (!intangent_set)
        {
          point1.x = arrKeyframe[i - 1].time;
          point1.y = arrKeyframe[i - 1].value;
          point2.x = arrKeyframe[i].time;
          point2.y = arrKeyframe[i].value;

          deltapoint = point2 - point1;

          intangent = deltapoint.y / deltapoint.x;
        }
        if (!outtangent_set)
        {
          point1.x = arrKeyframe[i].time;
          point1.y = arrKeyframe[i].value;
          point2.x = arrKeyframe[i + 1].time;
          point2.y = arrKeyframe[i + 1].value;

          deltapoint = point2 - point1;

          outtangent = deltapoint.y / deltapoint.x;
        }

        arrKeyframe[i].inTangent = intangent;
        arrKeyframe[i].outTangent = outtangent;
      }
    } 

  }
}

