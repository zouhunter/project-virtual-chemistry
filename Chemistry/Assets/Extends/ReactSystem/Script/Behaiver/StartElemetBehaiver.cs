using UnityEngine;
using System.Collections;
namespace ReactSystem
{
    [RequireComponent(typeof(IContainer))]
    public class StartElemetBehaiver : MonoBehaviour
    {
        public string[] element;
        IContainer container;
        // Use this for initialization
        void Start()
        {
            container = GetComponent<IContainer>();
            container.AddStartElement(element);

        }
    }
}