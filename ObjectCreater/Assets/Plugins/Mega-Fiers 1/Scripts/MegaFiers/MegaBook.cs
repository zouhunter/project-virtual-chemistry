
#if false
using UnityEngine;
using System.Collections.Generic;

// Have a pagemesh for each stack then scale to fit pages * thicknessperpage on each side

[ExecuteInEditMode]
public class MegaBook : MonoBehaviour
{
	public GameObject		front;
	public GameObject		back;
	public GameObject		page1;
	public GameObject		page2;
	public GameObject		page3;
	public List<Texture>	pages = new List<Texture>();
	public float			bookalpha;
	public float			covergap = 0.0f;
	public float			pagespace = 0.01f;

	MegaPageFlip			pf1;
	MegaPageFlip			pf2;
	MegaPageFlip			pf3;
	MeshRenderer			mrpg1;
	MeshRenderer			mrpg2;
	MeshRenderer			mrpg3;

	void SetPageTexture(MeshRenderer mr, int i, Texture t)
	{
		if ( mr.sharedMaterials[i].mainTexture != t )
			mr.sharedMaterials[i].mainTexture = t;
	}

	void Update()
	{
		if ( page1 == null || page2 == null || page3 == null )
			return;

		if ( page1 != null && pf1 == null )
			pf1 = page1.GetComponent<MegaPageFlip>();

		if ( mrpg1 == null )
			mrpg1 = page1.GetComponent<MeshRenderer>();

		if ( page2 != null && pf2 == null )
			pf2 = page2.GetComponent<MegaPageFlip>();

		if ( mrpg2 == null )
			mrpg2 = page2.GetComponent<MeshRenderer>();

		if ( page3 != null && pf3 == null )
			pf3 = page3.GetComponent<MegaPageFlip>();

		if ( mrpg3 == null )
			mrpg3 = page3.GetComponent<MeshRenderer>();

		if ( pf1 == null || pf2 == null || pf3 == null || front == null || back == null )
			return;

		int pagecount = (pages.Count / 2) + 2;

		if ( bookalpha < 0.0f )
			bookalpha = 0.0f;

		if ( bookalpha > 100.0f )
			bookalpha = 100.0f;

		if ( front.transform.childCount > 0 )
		{
			Transform child = front.transform.GetChild(0);
			if ( child != null )
			{
				Vector3 off = Vector3.zero;
				off.y = covergap * 0.5f;
				child.localPosition = off;
			}
		}

		if ( back.transform.childCount > 0 )
		{
			Transform child = back.transform.GetChild(0);
			if ( child != null )
			{
				Vector3 off = Vector3.zero;
				off.y = -covergap * 0.5f;
				child.localPosition = off;
			}
		}

		float alpha = bookalpha / 100.0f;

		int page = (int)((float)pagecount * alpha);

		float step = 1.0f / (float)pagecount;

		float turn = (alpha % step) / step;

		Vector3 ang = Vector3.zero;

		// Front cover
		if ( page == 0 )
			ang.z = 180.0f * turn;
		else
			ang.z = 180.0f;

		front.transform.localRotation = Quaternion.Euler(ang);

		// Back cover
		if ( page >= pagecount - 1 )
			ang.z = 180.0f * turn;
		else
			ang.z = 0.0f;

		back.transform.localRotation = Quaternion.Euler(ang);

		if ( pagecount < 3 )
			return;

		// Set PageFlip values
		if ( page == 1 )
		{
			pf1.turn = turn * 100.0f;
			pf2.turn = 0.0f;
			pf3.turn = 0.0f;
		}
		else
		{
			if ( page == pagecount - 2 )
			{
				pf1.turn = 100.0f;
				pf2.turn = 100.0f;
				pf3.turn = turn * 100.0f;
			}
			else
			{
				if ( page == 0 )
				{
					pf1.turn = 0.0f;
					pf2.turn = 0.0f;
					pf3.turn = 0.0f;
				}
				else
				{
					if ( page >= pagecount - 1 )
					{
						pf1.turn = 100.0f;
						pf2.turn = 100.0f;
						pf3.turn = 100.0f;
					}
					else
					{
						pf1.turn = 100.0f;
						pf2.turn = turn * 100.0f;
						pf3.turn = 0.0f;
					}
				}
			}
		}

		// Page offsets
		Vector3 poff = Vector3.zero;
		//float po = pagespace;	// * 2.0f;
		poff.y = Mathf.Lerp(pagespace, -pagespace, pf1.turn * 0.01f);
		page1.transform.localPosition = poff;

		poff.y = Mathf.Lerp(0.0f, 0.0f, pf2.turn * 0.01f);
		page2.transform.localPosition = poff;

		poff.y = Mathf.Lerp(-pagespace, pagespace, pf3.turn * 0.01f);
		page3.transform.localPosition = poff;

		// Page textures
		int pg = page - 2;
		if ( pg < 0 )
			pg = 0;

		pg *= 2;

		if ( pg < pages.Count - 1 )
		{
			SetPageTexture(mrpg1, 0, pages[pg]);
			SetPageTexture(mrpg1, 1, pages[pg + 1]);
		}

		if ( pg < pages.Count - 3 )
		{
			if ( pg < pages.Count - 5 )
			{
				SetPageTexture(mrpg2, 0, pages[pg + 2]);
				SetPageTexture(mrpg2, 1, pages[pg + 3]);
			}
		}

		if ( pg < pages.Count - 5 )
		{
			SetPageTexture(mrpg3, 0, pages[pg + 4]);
			SetPageTexture(mrpg3, 1, pages[pg + 5]);
		}
	}
}
#else
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Have a pagemesh for each stack then scale to fit pages * thicknessperpage on each side

[ExecuteInEditMode]
public class MegaBook : MonoBehaviour
{
	public float					dragsensi = 1.0f;
	public float					keysensi = 1.0f;
	public GameObject               front;
	public GameObject               back;
	public GameObject               page1;
	public GameObject               page2;
	public GameObject               page3;
	public List<Texture>			pages = new List<Texture>();
	public float                    bookalpha;
	public float                    covergap = 0.0f;
	public float                    pagespace = 0.01f;
	public bool						interactive = false;
	public bool						useMouse = false;
	public KeyCode					prevPageKey = KeyCode.A;
	public KeyCode					nextPageKey = KeyCode.D;

	MegaPageFlip                    pf1;
	MegaPageFlip                    pf2;
	MegaPageFlip                    pf3;
	MeshRenderer                    mrpg1;
	MeshRenderer                    mrpg2;
	MeshRenderer                    mrpg3;
	int								currentPage = 0;
	bool							pageTurning = false;

	MegaModifyObject	pobj1;
	MegaModifyObject	pobj2;
	MegaModifyObject	pobj3;

	float page1turn = -1.0f;
	float page2turn = -1.0f;
	float page3turn = -1.0f;

	void SetPageTexture(MeshRenderer mr, int i, Texture t)
	{
		if ( mr.sharedMaterials[i].mainTexture != t )
			mr.sharedMaterials[i].mainTexture = t;
	}

	void Update()
	{
		if ( interactive )
			Interactive();

		if ( page1 == null || page2 == null || page3 == null )
			return;

		if ( page1 != null && pf1 == null )
			pf1 = page1.GetComponent<MegaPageFlip>();

		if ( mrpg1 == null )
			mrpg1 = page1.GetComponent<MeshRenderer>();

		if ( page2 != null && pf2 == null )
			pf2 = page2.GetComponent<MegaPageFlip>();

		if ( mrpg2 == null )
			mrpg2 = page2.GetComponent<MeshRenderer>();

		if ( page3 != null && pf3 == null )
			pf3 = page3.GetComponent<MegaPageFlip>();

		if ( mrpg3 == null )
			mrpg3 = page3.GetComponent<MeshRenderer>();

		if ( pf1 == null || pf2 == null || pf3 == null || front == null || back == null )
			return;

		if ( pobj1 == null )
			pobj1 = page1.GetComponent<MegaModifyObject>();

		if ( pobj2 == null )
			pobj2 = page2.GetComponent<MegaModifyObject>();

		if ( pobj3 == null )
			pobj3 = page3.GetComponent<MegaModifyObject>();

		int pagecount = (pages.Count / 2) + 2;

		if ( bookalpha < 0.0f )
			bookalpha = 0.0f;

		if ( bookalpha > 100.0f )
			bookalpha = 100.0f;

		if ( front.transform.childCount > 0 )
		{
			Transform child = front.transform.GetChild(0);
			if ( child != null )
			{
				Vector3 off = Vector3.zero;
				off.y = covergap * 0.5f;
				child.localPosition = off;
			}
		}

		if ( back.transform.childCount > 0 )
		{
			Transform child = back.transform.GetChild(0);
			if ( child != null )
			{
				Vector3 off = Vector3.zero;
				off.y = -covergap * 0.5f;
				child.localPosition = off;
			}
		}

		double alpha = (double)bookalpha / 100.0;

		int page = (int)((double)pagecount * (double)alpha);

		double step = 1.0 / (double)pagecount;

		double turn = (alpha % step) / step;

		//turn = Mathf.Clamp(turn, 0.0f, 0.999f);
		//Debug.Log("turn " + turn.ToString("0.00000"));

		Vector3 ang = Vector3.zero;

		// Front cover
		if ( page == 0 )
			ang.z = 180.0f * (float)turn;
		else
			ang.z = 180.0f;

		front.transform.localRotation = Quaternion.Euler(ang);

		// Back cover
		if ( page >= pagecount - 1 )
			ang.z = 180.0f * (float)turn;
		else
			ang.z = 0.0f;

		back.transform.localRotation = Quaternion.Euler(ang);

		if ( pagecount < 3 )
			return;

		// Set PageFlip values
		if ( page == 1 )
		{
			pf1.turn = Mathf.Clamp((float)(turn * 100.0), 0.0f, 100.0f);
			pf2.turn = 0.0f;
			pf3.turn = 0.0f;
		}
		else
		{
			if ( page == pagecount - 2 )
			{
				pf1.turn = 100.0f;
				pf2.turn = 100.0f;
				pf3.turn = Mathf.Clamp((float)(turn * 100.0), 0.0f, 100.0f); //(float)turn * 100.0f;
			}
			else
			{
				if ( page == 0 )
				{
					pf1.turn = 0.0f;
					pf2.turn = 0.0f;
					pf3.turn = 0.0f;
				}
				else
				{
					if ( page >= pagecount - 1 )
					{
						pf1.turn = 100.0f;
						pf2.turn = 100.0f;
						pf3.turn = 100.0f;
					}
					else
					{
						pf1.turn = 100.0f;
						pf2.turn = Mathf.Clamp((float)(turn * 100.0), 0.0f, 100.0f); //(float)turn * 100.0f;
						pf3.turn = 0.0f;
					}
				}
			}
		}

		// Page offsets
		Vector3 poff = Vector3.zero;
		//float po = pagespace; // * 2.0f;
		poff.y = Mathf.Lerp(pagespace, -pagespace, pf1.turn * 0.01f);
		page1.transform.localPosition = poff;

		poff.y = Mathf.Lerp(0.0f, 0.0f, pf2.turn * 0.01f);
		page2.transform.localPosition = poff;

		poff.y = Mathf.Lerp(-pagespace, pagespace, pf3.turn * 0.01f);
		page3.transform.localPosition = poff;

		// Page textures
		int pg = page - 2;
		if ( pg < 0 )
			pg = 0;

		pg *= 2;

		if ( pg < pages.Count - 1 )
		{
			SetPageTexture(mrpg1, 0, pages[pg]);
			SetPageTexture(mrpg1, 1, pages[pg + 1]);
		}

		if ( pg < pages.Count - 3 )
		{
			if ( pg < pages.Count - 5 )
			{
				SetPageTexture(mrpg2, 0, pages[pg + 2]);
				SetPageTexture(mrpg2, 1, pages[pg + 3]);
			}
		}

		if ( pg < pages.Count - 5 )
		{
			SetPageTexture(mrpg3, 0, pages[pg + 4]);
			SetPageTexture(mrpg3, 1, pages[pg + 5]);
		}

		if ( pf1.turn != page1turn )
		{
			page1turn = pf1.turn;
			pobj1.Enabled = true;
		}
		else
		{
			if ( page1turn == 100.0f || page1turn == 0.0f )
				pobj1.Enabled = false;
			else
				pobj1.Enabled = true;
		}
		//Debug.Log("Enabled " + pobj1.Enabled);


		if ( pf2.turn != page2turn )
		{
			page2turn = pf2.turn;
			pobj2.Enabled = true;
		}
		else
		{
			if ( page2turn == 100.0f || page2turn == 0.0f )
				pobj2.Enabled = false;
			else
				pobj2.Enabled = true;
		}

		if ( pf3.turn != page3turn )
		{
			page3turn = pf3.turn;
			pobj3.Enabled = true;
		}
		else
		{
			if ( page3turn == 100.0f || page3turn == 0.0f )
				pobj3.Enabled = false;
			else
				pobj3.Enabled = true;
		}
	}

#if false
	void Interactive()
	{
		if ( !pageTurning )
		{
			// Mouse version
			//bookalpha += Input.GetAxis("Mouse X") * dragsensi;
			//bookalpha = Mathf.Clamp(bookalpha, 0.0f, 100.0f);

			// Keyboard version
			// bookalpha += Input.GetKey(KeyCode.A) * keysensi;
			//bookalpha += (Input.GetKey(KeyCode.A) ? 1 : 0) * keysensi;
			//bookalpha += Input.GetKey(KeyCode.D) * keysensi;
			//bookalpha -= (Input.GetKey(KeyCode.D) ? 1 : 0) * keysensi;
			if ( Input.GetKeyDown(KeyCode.A) ) FlipToPage(currentPage - 1);
			if ( Input.GetKeyDown(KeyCode.D) ) FlipToPage(currentPage + 1);



			//bookalpha = Mathf.Clamp(bookalpha, 0.0f, 100.0f);
		}
		else
		{
			//Flipping();

		}
	}

	void Flipping()
	{
		if ( bookalpha < target )
		{
			bookalpha += keysensi * Time.deltaTime;
			//bookalpha = Mathf.MoveTowards(bookalpha, (float)target, keysensi * Time.deltaTime);
			if ( (double)bookalpha >= target )
			{
				bookalpha = (float)target;
				currentPage = opage;
				pageTurning = false;
			}
		}
		else
		{
			bookalpha -= keysensi * Time.deltaTime;
			//bookalpha = Mathf.MoveTowards(bookalpha, (float)target, keysensi * Time.deltaTime);
			if ( (double)bookalpha <= target )
			{
				bookalpha = (float)target;
				currentPage = opage;
				pageTurning = false;
			}
		}
	}

	int opage = 0;
	double target = 0.0;

	void FlipToPage(int _page)
	{
		opage = _page;
		pageTurning = true;
		double totalPageCount = (double)((pages.Count + 4) / 2);
		//Debug.Log(totalPageCount);
		target = (double)opage / totalPageCount * 100.0;
		//target = Mathf.Clamp(target, 0.0f, 100.0f);
	}
#endif

	void Interactive()
	{
		if ( !pageTurning )
		{
			// Mouse version
			if ( useMouse )
			{
				bookalpha += Input.GetAxis("Mouse X") * dragsensi;
				bookalpha = Mathf.Clamp(bookalpha, 0.0f, 100.0f);
			}
			else
			{
				// Keyboard version
				// bookalpha += Input.GetKey(KeyCode.A) * keysensi;
				//bookalpha += (Input.GetKey(KeyCode.A) ? 1 : 0) * keysensi;
				//bookalpha += Input.GetKey(KeyCode.D) * keysensi;
				//bookalpha -= (Input.GetKey(KeyCode.D) ? 1 : 0) * keysensi;
				if ( Input.GetKeyDown(prevPageKey) )
					StartCoroutine(FlipToPage(currentPage - 1));

				if ( Input.GetKeyDown(nextPageKey) )
					StartCoroutine(FlipToPage(currentPage + 1));
			}
			////bookalpha = Mathf.Clamp(bookalpha, 0.0f, 100.0f);
		}
	}

	IEnumerator FlipToPage(int page)
	{
		pageTurning = true;
		double totalPageCount = (double)((pages.Count + 4) / 2);
		//Debug.Log(totalPageCount);
		double target = (double)page / totalPageCount * 99.999;
		if ( target < 0.0 )
			target = 0.0;

		if ( target > 100.0 )
			target = 100.0;

		while ( bookalpha != (float)target )
		{
			bookalpha = Mathf.MoveTowards(bookalpha, (float)target, keysensi * Time.deltaTime);
			yield return 0;
		}
		currentPage = page;
		pageTurning = false;
	}
}
#endif