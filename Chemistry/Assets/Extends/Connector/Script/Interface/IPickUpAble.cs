using UnityEngine;
namespace Connector
{
    public interface IPickUpAble
    {
        GameObject Go { get;}
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        void OnPickUp();
        void OnPickStay();
        void OnPickDown();
    }
}