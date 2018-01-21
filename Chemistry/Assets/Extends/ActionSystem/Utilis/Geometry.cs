using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public static class GeometryUtil
    {
        /// <summary>
        /// 点到直线距离
        /// </summary>
        /// <param name="point">点坐标</param>
        /// <param name="linePoint1">直线上一个点的坐标</param>
        /// <param name="linePoint2">直线上另一个点的坐标</param>
        /// <returns></returns>
        public static float DisPoint2Line(Vector3 point, Vector3 linePoint1, Vector3 linePoint2)
        {
            Vector3 vec1 = point - linePoint1;
            Vector3 vec2 = linePoint2 - linePoint1;
            Vector3 vecProj = Vector3.Project(vec1, vec2);
            float dis = Mathf.Sqrt(Mathf.Pow(Vector3.Magnitude(vec1), 2) - Mathf.Pow(Vector3.Magnitude(vecProj), 2));
            return dis;
        }
        /// <summary>
        /// 点到平面的距离 自行推演函数
        /// </summary>
        /// <param name="point"></param>
        /// <param name="surfacePoint1"></param>
        /// <param name="surfacePoint2"></param>
        /// <param name="surfacePoint3"></param>
        /// <returns></returns>
        public static float DisPoint2Surface(Vector3 point, Vector3 surfacePoint1, Vector3 surfacePoint2, Vector3 surfacePoint3)
        {
            //空间直线一般式方程 Ax + By + Cz + D = 0;
            //假定 A = 1 ，推演B C D用A来表示，约去A，可得方程
            float BNumerator = (surfacePoint1.x - surfacePoint2.x) * (surfacePoint2.z - surfacePoint3.z) - (surfacePoint2.x - surfacePoint3.x) * (surfacePoint1.z - surfacePoint2.z);
            float BDenominator = (surfacePoint2.y - surfacePoint3.y) * (surfacePoint1.z - surfacePoint2.z) - (surfacePoint1.y - surfacePoint2.y) * (surfacePoint2.z - surfacePoint3.z);
            float B = BNumerator / BDenominator;
            float C = (B * (surfacePoint1.y - surfacePoint2.y) + (surfacePoint1.x - surfacePoint2.x)) / (surfacePoint2.z - surfacePoint1.z);
            float D = -surfacePoint1.x - B * surfacePoint1.y - C * surfacePoint1.z;

            return DisPoint2Surface(point, 1f, B, C, D);
        }

        public static float DisPoint2Surface(Vector3 point, float FactorA, float FactorB, float FactorC, float FactorD)
        {
            //点到平面的距离公式 d = |Ax + By + Cz + D|/sqrt(A2 + B2 + C2);
            float numerator = Mathf.Abs(FactorA * point.x + FactorB * point.y + FactorC * point.z + FactorD);
            float denominator = Mathf.Sqrt(Mathf.Pow(FactorA, 2) + Mathf.Pow(FactorB, 2) + Mathf.Pow(FactorC, 2));
            float dis = numerator / denominator;
            return dis;
        }

        /// <summary>
        /// 点到平面距离 调用U3D Plane类处理
        /// </summary>
        /// <param name="point"></param>
        /// <param name="surfacePoint1"></param>
        /// <param name="surfacePoint2"></param>
        /// <param name="surfacePoint3"></param>
        /// <returns></returns>
        public static float DisPoint2Surface2(Vector3 point, Vector3 surfacePoint1, Vector3 surfacePoint2, Vector3 surfacePoint3)
        {
            Plane plane = new Plane(surfacePoint1, surfacePoint2, surfacePoint3);

            return DisPoint2Surface2(point, plane);
        }

        public static float DisPoint2Surface2(Vector3 point, Plane plane)
        {
            return plane.GetDistanceToPoint(point);
        }

        /// <summary>
        /// 平面夹角
        /// </summary>
        /// <param name="surface1Point1"></param>
        /// <param name="surface1Point2"></param>
        /// <param name="surface1Point3"></param>
        /// <param name="surface2Point1"></param>
        /// <param name="surface2Point2"></param>
        /// <param name="surface2Point3"></param>
        /// <returns></returns>
        public static float SurfaceAngle(Vector3 surface1Point1, Vector3 surface1Point2, Vector3 surface1Point3, Vector3 surface2Point1, Vector3 surface2Point2, Vector3 surface2Point3)
        {
            Plane plane1 = new Plane(surface1Point1, surface1Point1, surface1Point1);
            Plane plane2 = new Plane(surface2Point1, surface2Point1, surface2Point1);
            return SurfaceAngle(plane1, plane2);
        }

        public static float SurfaceAngle(Plane plane1, Plane plane2)
        {
            return Vector3.Angle(plane1.normal, plane2.normal);
        }
        /// <summary>
        /// 利用线上一个点，线的方向，面上一个点，及面的法向量 判断线上一点到面的距离
        /// </summary>
        /// <param name="linePoint"></param>
        /// <param name="lineVec"></param>
        /// <param name="planePoint"></param>
        /// <param name="planeNormal"></param>
        /// <returns></returns>
        public static float LinePlaneDistance(Vector3 linePoint, Vector3 lineVec, Vector3 planePoint, Vector3 planeNormal)
        {
            //calculate the distance between the linePoint and the line-plane intersection point
            float dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
            float dotDenominator = Vector3.Dot(lineVec, planeNormal);
            //line and plane are not parallel
            if (dotDenominator != 0f)
            {
                return dotNumerator / dotDenominator;
            }

            return 0;
        }

        /// <summary>
        /// 判断面与线的焦点
        /// </summary>
        /// <param name="linePoint"></param>
        /// <param name="lineVec"></param>
        /// <param name="planePoint"></param>
        /// <param name="planeNormal"></param>
        /// <returns></returns>
        //Note that the line is infinite, this is not a line-segment plane intersect
        public static Vector3 LinePlaneIntersect(Vector3 linePoint, Vector3 lineVec, Vector3 planePoint, Vector3 planeNormal)
        {
            float distance = LinePlaneDistance(linePoint, lineVec, planePoint, planeNormal);

            //line and plane are not parallel
            if (distance != 0f)
            {
                return linePoint + (lineVec * distance);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 返回两线最短距离的两个点
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point1Direction"></param>
        /// <param name="point2"></param>
        /// <param name="point2Direction"></param>
        /// <returns></returns>
        //Returns 2 points since on line 1 there will be a closest point to line 2, and on line 2 there will be a closest point to line 1.
        public static KeyValuePair<Vector3, Vector3> ClosestPointsOnTwoLines(Vector3 point1, Vector3 point1Direction, Vector3 point2, Vector3 point2Direction)
        {
            //I dont think we need to normalize
            //point1Direction.Normalize();
            //point2Direction.Normalize();

            float a = Vector3.Dot(point1Direction, point1Direction);
            float b = Vector3.Dot(point1Direction, point2Direction);
            float e = Vector3.Dot(point2Direction, point2Direction);

            float d = a * e - b * b;

            //This is a check if parallel, howeverm since we are not normalizing the directions, it seems even if they are parallel they will not == 0
            //so they will get past this point, but its seems to be alright since it seems to still give a correct point (although a point very fary away).
            //Also, if they are parallel and we dont normalize, the deciding point seems randomly choses on the lines, which while is still correct,
            //our ClosestPointsOnTwoLineSegments gets undesireable results when on corners. (for example when using it in our ClosestPointOnTriangleToLine).
            Vector3 first;
            Vector3 second;
            if (d != 0f)
            {
                Vector3 r = point1 - point2;
                float c = Vector3.Dot(point1Direction, r);
                float f = Vector3.Dot(point2Direction, r);

                float s = (b * f - c * e) / d;
                float t = (a * f - c * b) / d;

                first = point1 + point1Direction * s;
                second = point2 + point2Direction * t;
            }
            else
            {
                //Lines are parallel, select any points next to eachother
                first = point1;
                second = point2 + Vector3.Project(point1 - point2, point2Direction);
            }

            KeyValuePair<Vector3, Vector3> pair = new KeyValuePair<Vector3, Vector3>(first, second);

            return pair;
        }
        /// <summary>
        /// 返回两点，其中一点被限定为segment0 或segment1
        /// </summary>
        /// <param name="segment0"></param>
        /// <param name="segment1"></param>
        /// <param name="linePoint"></param>
        /// <param name="lineDirection"></param>
        /// <returns></returns>
        public static KeyValuePair<Vector3, Vector3> ClosestPointsOnSegmentToLine(Vector3 segment0, Vector3 segment1, Vector3 linePoint, Vector3 lineDirection)
        {
            var closests = ClosestPointsOnTwoLines(segment0, segment1 - segment0, linePoint, lineDirection);
            closests = new KeyValuePair<Vector3, Vector3>(ClampToSegment(closests.Key, segment0, segment1), closests.Value);

            return closests;
        }

        //Assumes the point is already on the line somewhere
        public static Vector3 ClampToSegment(Vector3 point, Vector3 linePoint1, Vector3 linePoint2)
        {
            Vector3 lineDirection = linePoint2 - linePoint1;

            if (!GeometryUtil.IsInDirection(point - linePoint1, lineDirection))
            {
                point = linePoint1;
            }
            else if (GeometryUtil.IsInDirection(point - linePoint2, lineDirection))
            {
                point = linePoint2;
            }

            return point;
        }
        /// <summary>
        /// 点乘并取方向为单位向量（用于表示vector在direction方向上投影的长度）
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="direction"></param>
        /// <param name="normalizeParameters"></param>
        /// <returns></returns>
        public static float MagnitudeInDirection(Vector3 vector, Vector3 direction, bool normalizeParameters = true)
        {
            if (normalizeParameters) direction.Normalize();
            return Vector3.Dot(vector, direction);
        }
        /// <summary>
        /// 向量每一边都取绝对值的扩展方法
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 Abs(this Vector3 vector)
        {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }
        /// <summary>
        /// 判断两个向量叉乘得到的向量长度的平方来判断是否平行
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="otherDirection"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static bool IsParallel(Vector3 direction, Vector3 otherDirection, float precision = .0001f)
        {
            return Vector3.Cross(direction, otherDirection).sqrMagnitude < precision;
        }
        /// <summary>
        /// 判断两个向量夹角是否小于90度
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="otherDirection"></param>
        /// <returns></returns>
        public static bool IsInDirection(Vector3 direction, Vector3 otherDirection)
        {
            return Vector3.Dot(direction, otherDirection) > 0f;
        }

    }
}