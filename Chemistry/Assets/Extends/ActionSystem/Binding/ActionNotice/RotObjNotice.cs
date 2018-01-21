using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem.Binding
{
    public class RotObjNotice : ActionObjBinding
    {
        public float circleDetail = 40;
        private List<Vector3> _lines = new List<Vector3>();
        private LineRenderer lineRender;
        public float flashSpeed = 1f;
        public bool highLight = true;
        private Transform Trans { get { return transform; } }
        public float triggerRadius = 1;
        private float flash = 0;
        public Color color = Color.green;
        private bool right;
        private float rangleCercle = .1f;
        private Vector3 direction;
        private bool Active;

        private void Start()
        {
            InitLineRenderer();
            InitDirection();
        }
        void InitLineRenderer()
        {
            lineRender = gameObject.GetComponent<LineRenderer>();
            if (lineRender == null) lineRender = gameObject.AddComponent<LineRenderer>();
            lineRender.material = new Material(Shader.Find("Sprites/Default"));
#if UNITY_5_6_OR_NEWER
            lineRender.startWidth = 0.1f;
            lineRender.endWidth = 0.01f;
#else
            lineRender.SetWidth(.1f, .01f);
#endif
        }

        void InitDirection()
        {
            direction = (actionObj as RotObj).Direction;
        }

        void AddCircle(Vector3 origin, Vector3 axisDirection, float size, List<Vector3> resultsBuffer)
        {
            Vector3 up = axisDirection.normalized * size;
            Vector3 forward = Vector3.Slerp(up, -up, .5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * size;
            //Camera myCamera = Camera.main;

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = right.x;
            matrix[1] = right.y;
            matrix[2] = right.z;

            matrix[4] = up.x;
            matrix[5] = up.y;
            matrix[6] = up.z;

            matrix[8] = forward.x;
            matrix[9] = forward.y;
            matrix[10] = forward.z;

            Vector3 lastPoint = origin + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 nextPoint = Vector3.zero;
            float multiplier = 360f / circleDetail;

            //Plane plane = new Plane((myCamera.transform.position - transform.position).normalized, transform.position);

            for (var i = 0; i < circleDetail + 1; i++)
            {
                nextPoint.x = Mathf.Cos((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.z = Mathf.Sin((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.y = 0;

                nextPoint = origin + matrix.MultiplyPoint3x4(nextPoint);

                resultsBuffer.Add(lastPoint);
                resultsBuffer.Add(nextPoint);

                lastPoint = nextPoint;
            }
        }

        protected void Update()
        {
            if (Flash())
            {
                _lines.Clear();
                AddCircle(Trans.position, direction, triggerRadius + flash, _lines);
                if (highLight)
                {
                    DrawCircle(_lines, color);
                }
            }
            else
            {
                ClearCircle();
            }
        }
        protected override void OnBeforeActive(bool forceAuto)
        {
            base.OnBeforeActive(forceAuto);
            Active = true;
        }
        protected override void OnBeforeComplete(bool force)
        {
            base.OnBeforeComplete(force);
            Active = false;
        }
        internal void SetHighLight(bool on)
        {
            this.highLight = on;
        }
        private bool Flash()
        {
            if (Active)
            {
                if (right)
                {
                    flash += Time.deltaTime * flashSpeed;
                    if (flash > rangleCercle)
                    {
                        right = false;
                    }
                }
                else
                {
                    flash -= Time.deltaTime * flashSpeed;
                    if (flash < -rangleCercle)
                    {
                        right = true;
                    }
                }
                return true;
            }
            else
            {
                flash = 0;
                return false;
            }
        }

        void ClearCircle()
        {
#if UNITY_5_6_OR_NEWER
            lineRender.positionCount = 0;
#else
            lineRender.SetVertexCount(0);
#endif
        }
        void DrawCircle(List<Vector3> lines, Color color)
        {
#if UNITY_5_6_OR_NEWER
            lineRender.startColor = color;
            lineRender.endColor = color;
            lineRender.positionCount = lines.Count;
#else
            lineRender.SetColors(color, color);
            lineRender.SetVertexCount(lines.Count);
#endif
            lineRender.SetPositions(lines.ToArray());
        }

    }

}