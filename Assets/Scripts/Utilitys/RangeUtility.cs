using UnityEngine;
using System.Collections;

public struct RectRange
{
    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;

    public RectRange(float _xMin, float _xMax, float _yMin, float _yMax)
    {
        xMin = _xMin;
        xMax = _xMax;
        yMin = _yMin;
        yMax = _yMax;
    }
}

public static class RangeUtility
{
    private static RectRange[] m_Ranges;

    static RangeUtility()
    {
        m_Ranges = new RectRange[14];

        m_Ranges[0] = new RectRange(-50, -6, -60, -30);
        m_Ranges[1] = new RectRange(-50, -6, -30, -4);
        m_Ranges[2] = new RectRange(-50, -6, -4, 0);
        m_Ranges[3] = new RectRange(-50, -6, 0, 20);
        m_Ranges[4] = new RectRange(-50, 3, 20, 190);
        m_Ranges[5] = new RectRange(-6, 3, -60, -30);
        m_Ranges[6] = new RectRange(-6, 3, -30, 20);
        m_Ranges[7] = new RectRange(3, 16, -60, -30);
        m_Ranges[8] = new RectRange(3, 16, -30, -20);
        m_Ranges[9] = new RectRange(3, 16, -20, -6);
        m_Ranges[10] = new RectRange(3, 16, -6, 0);
        m_Ranges[11] = new RectRange(3, 16, 0, 10);
        m_Ranges[12] = new RectRange(3, 16, 10, 20);
        m_Ranges[13] = new RectRange(3, 16, 20, 190);
    }

    public static int GetRange(Vector2 position)
    {
        for (int i = 0; i < m_Ranges.Length; ++i)
        {
            var range = m_Ranges[i];
            if (((position.x > range.xMin) && (position.x < range.xMax))
                && ((position.y  > range.yMin) && (position.y < range.yMax)))
            {
                return i;
            }
        }

        return -1;
    }
}
