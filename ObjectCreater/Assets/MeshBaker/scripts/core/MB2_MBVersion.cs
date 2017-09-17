/**
 *	\brief Hax!  DLLs cannot interpret preprocessor directives, so this class acts as a "bridge"
 */
using System;
using UnityEngine;
using System.Collections;

namespace DigitalOpus.MB.Core{
	public class MBVersion
	{
		public static string version{
			get{return "3.3";}	
		}
		
		public static int GetMajorVersion(){
	#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5	
			return 3;
	#else
			return 4;
	#endif	
		}

		public static int GetMinorVersion(){
			#if UNITY_3_0 || UNITY_3_0_0 
				return 0;
			#elif UNITY_3_1 
				return 1;
			#elif UNITY_3_2 
				return 2;
			#elif UNITY_3_3 
				return 3;
			#elif UNITY_3_4 
				return 4;
			#elif UNITY_3_5
				return 5;
			#elif UNITY_4_0 || UNITY_4_0_1
				return 0;
			#elif UNITY_4_1
				return 1;
			#elif UNITY_4_2
				return 2;
			#elif UNITY_4_3
				return 3;
			#else
				return 0;
			#endif	
		}

		public static bool GetActive(GameObject go){
	#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5	
			return go.active;
	#else
			return go.activeInHierarchy;
	#endif			
		}
	
		public static void SetActive(GameObject go, bool isActive){
	#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5	
			go.active = isActive;
	#else
			go.SetActive(isActive);
	#endif
		}
		
		public static void SetActiveRecursively(GameObject go, bool isActive){
	#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5	
			go.SetActiveRecursively(isActive);
	#else
			go.SetActive(isActive);
	#endif
		}
		
		public static UnityEngine.Object[] FindSceneObjectsOfType(Type t){
	#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5	
			return GameObject.FindSceneObjectsOfType(t);
	#else
			return GameObject.FindObjectsOfType(t);
	#endif				
		}
	}
}