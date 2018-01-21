using UnityEngine;
using System.Collections.Generic;
using System;
namespace WorldActionSystem
{
    public static class Bezier
    {
        /// 二阶贝塞尔曲线
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return Mathf.Pow((1 - t), 2) * p0 + 2 * t * (1 - t) * p1 + Mathf.Pow(t, 2) * p2;
        }
        /// <summary>
        /// 三阶
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return Mathf.Pow(1 - t, 3) * p0 +
                3 * t * Mathf.Pow(1 - t, 2) * p1 +
                3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
                Mathf.Pow(t, 3) * p3;
        }

        /// <summary>
        /// 通用bezier曲线计算公式
        /// </summary>
        /// <param name="t"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector3 CalculateBezierPoint(float t, params Transform[] points)
        {
            Vector3 point = Vector3.zero;
            int n = points.Length - 1;
            for (int i = 0; i <= n; i++)
            {
                point += C(n, i) * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i) * points[i].position;
            }
            return point;
        }
        /// <summary>
        /// 通用bezier曲线计算公式
        /// </summary>
        /// <param name="t"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector3 CalculateBezierPoint(float t, params Vector3[] points)
        {
            Vector3 point = Vector3.zero;
            int n = points.Length - 1;
            for (int i = 0; i <= n; i++)
            {
                point += C(n, i) * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i) * points[i];
            }
            return point;
        }
        /// <summary>  
        /// 排列循环方法  
        /// </summary>  
        /// <param name="N"></param>  
        /// <param name="R"></param>  
        /// <returns></returns>  
        static long P1(int N, int R)
        {
            if (R == 0) return 1;
            if (R > N || R <= 0 || N <= 0) Debug.LogError("N:" + N + "\nR:" + R);
            long t = 1;
            int i = N;
            while (i != N - R)
            {
                try
                {
                    checked
                    {
                        t *= i;
                    }
                }
                catch
                {
                    Debug.LogError("overflow happens!");
                }
                --i;
            }
            return t;
        }
        /// <summary>  
        /// 排列堆栈方法  
        /// </summary>  
        /// <param name="N"></param>  
        /// <param name="R"></param>  
        /// <returns></returns>  
        static long P2(int N, int R)
        {
            if (R > N || R <= 0 || N <= 0) Debug.LogError("arguments invalid!");
            Stack<int> s = new Stack<int>();
            long iRlt = 1;
            int t;
            s.Push(N);
            while ((t = s.Peek()) != N - R)
            {
                try
                {
                    checked
                    {
                        iRlt *= t;
                    }
                }
                catch
                {
                    Debug.LogError("overflow happens!");
                }
                s.Pop();
                s.Push(t - 1);
            }
            return iRlt;
        }
        /// <summary>  
        /// 组合  
        /// </summary>  
        /// <param name="N"></param>  
        /// <param name="R"></param>  
        /// <returns></returns>  
        static long C(int N, int R)
        {
            return P1(N, R) / P1(R, R);
        }

    }

}