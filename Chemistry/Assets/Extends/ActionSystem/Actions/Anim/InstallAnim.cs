using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace WorldActionSystem
{
    public class InstallAnim : MonoBehaviour, AnimPlayer
    {
        [SerializeField]
        private Transform targetTrans;
        private Vector3 targetPos;
        [SerializeField]
        private float time = 2f;
        private Vector3 initPos;
        private Coroutine coroutine;
        private void Awake()
        {
            initPos = transform.position;
            if(targetTrans != null){
                targetPos = targetTrans.transform.position;
            }
        }

   

        public void Play(float speed, UnityAction onAutoPlayEnd)
        {
            time = 1f / speed;
            transform.position = initPos;
            coroutine = StartCoroutine(MoveAnim(onAutoPlayEnd));
        }

        public void EndPlay()
        {
            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            transform.position = targetPos;
        }

        public void UnDoPlay()
        {
            Debug.Log("UnDoPlay");
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            transform.position = initPos;
        }
        private IEnumerator MoveAnim(UnityAction onComplete)
        {
            var startPos = transform.position;
            for (float i = 0; i < time; i += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, i / time);
                yield return null;
            }
            if (onComplete != null)
            {
                onComplete.Invoke();
                onComplete = null;
            }
            transform.position = targetPos;
        }
    }
}
