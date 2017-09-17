using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace DigitalOpus.MB.Core{
	public class MB2_TexturePacker {
		
		public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;
		
		class PixRect{
			public int x;
			public int y;
			public int w;
			public int h;
			
			public PixRect(){}
			public PixRect(int xx, int yy, int ww, int hh){
				x = xx;
				y = yy;
				w = ww;
				h = hh;
			}
		}
		
		class Image{
			public int imgId;
			public int w;
			public int h;
			public int x;
			public int y;
	
			public Image(int id, int tw, int th, int padding){
				imgId = id;
				w = tw + padding * 2;
				h = th + padding * 2;
			}
		}
		
		class ImgIDComparer : IComparer<Image>{
			public int Compare(Image x, Image y){
				if (x.imgId > y.imgId)
					return 1;
				if (x.imgId == y.imgId)
					return 0;
				return -1;
			}			
		}
		
		class ImageHeightComparer : IComparer<Image>{
			public int Compare(Image x, Image y){
				if (x.h > y.h)
					return -1;
				if (x.h == y.h)
					return 0;
				return 1;
			}
		}
	
		class ImageWidthComparer : IComparer<Image>{
			public int Compare(Image x, Image y){
				if (x.w > y.w)
					return -1;
				if (x.w == y.w)
					return 0;
				return 1;
			}
		}
	
		class ImageAreaComparer : IComparer<Image>{
			public int Compare(Image x, Image y){
				int ax = x.w * x.h;
				int ay = y.w * y.h;
				if (ax > ay)
					return -1;
				if (ax == ay)
					return 0;
				return 1;
			}
		}
		
		class ProbeResult{
			public int w;
			public int h;
			public Node root;
			public bool fitsInMaxSize;
			public float efficiency;
			public float squareness;
			
			public void Set(int ww, int hh, Node r, bool fits, float e, float sq){
				w = ww;
				h = hh;
				root = r;
				fitsInMaxSize = fits;
				efficiency = e;
				squareness = sq;
			}
			
			public float GetScore(){
				float fitsScore = fitsInMaxSize ? 1f : 0f;
				return squareness + 2 * efficiency + fitsScore;	
			}
		}
		
		class Node {
		    public Node[] child = new Node[2];
		    public PixRect r;
		    public Image img;
			
			bool isLeaf(){
				if (child[0] == null || child[1] == null){
					return true;
				}
				return false;
			}
			
			public Node Insert(Image im, bool handed){
				int a,b;
				if (handed){
				  a = 0;
				  b = 1;
				} else {
				  a = 1;
				  b = 0;
				}
				if (!isLeaf()){
					//try insert into first child
					Node newNode = child[a].Insert(im,handed);
					if (newNode != null)
						return newNode;
					//no room insert into second
					return child[b].Insert(im,handed);
				} else {
			        //(if there's already a lightmap here, return)
			        if (img != null) 
						return null;
			
			        //(if we're too small, return)
			        if (r.w < im.w || r.h < im.h)
			            return null;
					
			        //(if we're just right, accept)
			        if (r.w == im.w && r.h == im.h){
						img = im;
//						img.x = r.x;
//						img.y = r.y;
			            return this;
					}
			        
			        //(otherwise, gotta split this node and create some kids)
			        child[a] = new Node();
			        child[b] = new Node();
			        
			        //(decide which way to split)
			        int dw = r.w - im.w;
			        int dh = r.h - im.h;
			        
			        if (dw > dh){
			            child[a].r = new PixRect(r.x, r.y, im.w, r.h);
			            child[b].r = new PixRect(r.x + im.w, r.y, r.w - im.w, r.h);
					} else {
			            child[a].r = new PixRect(r.x, r.y, r.w, im.h);
			            child[b].r = new PixRect(r.x, r.y+ im.h, r.w, r.h - im.h);
					}
			        return child[a].Insert(im,handed);				
				}
			}
		}
		
		static void printTree(Node r, string spc){
			if (r.child[0] != null)
				printTree(r.child[0], spc + "  ");
			if (r.child[1] != null)
				printTree(r.child[1], spc + "  ");		
		}
		
		static void flattenTree(Node r, List<Image> putHere){
			if (r.img != null){
				r.img.x = r.r.x;
				r.img.y = r.r.y;				
				putHere.Add(r.img);
			}
			if (r.child[0] != null)
				flattenTree(r.child[0], putHere);
			if (r.child[1] != null)
				flattenTree(r.child[1], putHere);		
		}
		
		static void drawGizmosNode(Node r){
			Vector3 extents = new Vector3(r.r.w, r.r.h, 0);
			Vector3 pos = new Vector3(r.r.x + extents.x/2, -r.r.y - extents.y/2, 0f);
			Gizmos.DrawWireCube(pos,extents);
			if (r.img != null){
				Gizmos.color = Color.blue;
				extents = new Vector3(r.img.w, r.img.h, 0);
				pos = new Vector3(r.img.x + extents.x/2, -r.img.y - extents.y/2, 0f);
				Gizmos.DrawCube(pos,extents);
			}
			if (r.child[0] != null){
				Gizmos.color = Color.red;
				drawGizmosNode(r.child[0]);
			}
			if (r.child[1] != null){
				Gizmos.color = Color.green;
				drawGizmosNode(r.child[1]);
			}
		}
	    	
		static Texture2D createFilledTex(Color c, int w, int h){
			Texture2D t = new Texture2D(w,h);
			for (int i = 0; i < w; i++){
				for (int j = 0; j < h; j++){
					t.SetPixel(i,j,c);
				}
			}
			t.Apply();
			return t;
		}
		
		public void DrawGizmos(){
			if (bestRoot != null)
				drawGizmosNode(bestRoot.root);
		}
		
		ProbeResult bestRoot;
		
		bool Probe(Image[] imgsToAdd, int idealAtlasW, int idealAtlasH, float imgArea, int maxAtlasDim, ProbeResult pr){
			Node root = new Node();
			root.r = new PixRect(0,0,idealAtlasW,idealAtlasH);
			for (int i = 0; i < imgsToAdd.Length; i++){
				Node n = root.Insert(imgsToAdd[i],false);
				if (n == null){
					return false;
				} else if (i == imgsToAdd.Length -1){
					int usedW = 0;
					int usedH = 0;
					GetExtent(root,ref usedW, ref usedH);
					float squareness;
					float efficiency = 1f - (usedW * usedH - imgArea) / (usedW * usedH);
					if (usedW < usedH) squareness = (float) usedW / (float) usedH;
					else squareness = (float) usedH / (float) usedW;
					bool fitsInMaxDim = usedW <= maxAtlasDim && usedH <= maxAtlasDim;
					pr.Set(usedW,usedH,root,fitsInMaxDim,efficiency,squareness);
					if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Probe success efficiency w=" + usedW + " h=" + usedH + " e=" + efficiency + " sq=" + squareness + " fits=" + fitsInMaxDim);
					return true;
				}
			}	
			Debug.LogError("Should never get here.");
			return false;
		}
		
		void GetExtent(Node r, ref int x, ref int y){
			if (r.img != null){
				if (r.r.x + r.img.w > x){
					x = r.r.x + r.img.w;
				}
				if (r.r.y + r.img.h > y) y = r.r.y + r.img.h; 
			}
			if (r.child[0] != null)
				GetExtent(r.child[0], ref x, ref y);
			if (r.child[1] != null)
				GetExtent(r.child[1], ref x, ref y);		
		}
			
		public Rect[] GetRects(List<Vector2> imgWidthHeights, int maxDimension, int padding, out int outW, out int outH){
			float area = 0;
			int maxW = 0;
			int maxH = 0;
			Image[] imgsToAdd = new Image[imgWidthHeights.Count];
			for (int i = 0; i < imgsToAdd.Length; i++){
				Image im = imgsToAdd[i] = new Image(i,(int)imgWidthHeights[i].x, (int)imgWidthHeights[i].y, padding);
				area += im.w * im.h;
				maxW = Mathf.Max(maxW, im.w);
				maxH = Mathf.Max(maxH, im.h);
			}
			
			if ((float)maxH/(float)maxW > 2){
				if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Using height Comparer");
				Array.Sort(imgsToAdd,new ImageHeightComparer());
			}
			else if ((float)maxH/(float)maxW < .5){
				if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Using width Comparer");
				Array.Sort(imgsToAdd,new ImageWidthComparer());
			}
			else{
				if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Using area Comparer");
				Array.Sort(imgsToAdd,new ImageAreaComparer());
			}
//			List<Node> ns = new List<Node>();
			
			//explore the space to find a resonably efficient packing
			int sqrtArea = (int) Mathf.Sqrt(area);
			int idealAtlasW = sqrtArea; 
			int idealAtlasH = sqrtArea; 
			if (maxW > sqrtArea){
				idealAtlasW = maxW;
				idealAtlasH = Mathf.Max(Mathf.CeilToInt(area / maxW), maxH);
			}
			if (maxH > sqrtArea){
				idealAtlasW = Mathf.Max(Mathf.CeilToInt(area / maxH), maxW);
				idealAtlasH = maxH;
			}
			if (idealAtlasW == 0) idealAtlasW = 1;
			if (idealAtlasH == 0) idealAtlasH = 1;
			int stepW = (int)(idealAtlasW * .15f);
			int stepH = (int)(idealAtlasH * .15f);
			if (stepW == 0) stepW = 1;
			if (stepH == 0) stepH = 1;
//			bool doStepHeight = true;
//			bool successH = false;
			int numWIterations=2;
			int steppedHeight = idealAtlasH;
			while (numWIterations > 1 && steppedHeight < sqrtArea * 1000){	
				bool successW = false;
				numWIterations = 0;
				int steppedWidth = idealAtlasW;
				while (!successW && steppedWidth < sqrtArea * 1000){	
					ProbeResult pr = new ProbeResult();
					if (Probe(imgsToAdd, steppedWidth, steppedHeight, area, maxDimension, pr)){
						successW = true;
						if (bestRoot == null) bestRoot = pr;
						else if (pr.GetScore() > bestRoot.GetScore()) bestRoot = pr;
					} else {
						numWIterations++;
						steppedWidth += stepW;
						if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("increasing Width h=" + steppedHeight + " w=" + steppedWidth);
					}			
				}
				steppedHeight += stepH;
				if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("increasing Height h=" + steppedHeight + " w=" + steppedWidth);
			}
			
			outW = 0;
			outH = 0;
			if (bestRoot == null) return null;
			if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Best fit found: w=" + bestRoot.w + " h=" + bestRoot.h + " efficiency=" + bestRoot.efficiency + " squareness=" + bestRoot.squareness + " fits in max dimension=" + bestRoot.fitsInMaxSize);
			outW = bestRoot.w;
			outH = bestRoot.h;
			List<Image> images = new List<Image>();
			flattenTree(bestRoot.root, images);
			images.Sort(new ImgIDComparer());
			if (images.Count != imgsToAdd.Length) Debug.LogError("Result images not the same lentgh as source");
			
			//scale images if too large
			float padX = (float)padding / (float)bestRoot.w;
			if (bestRoot.w > maxDimension){
				padX = (float)padding / (float)maxDimension;
				float scaleFactor = (float) maxDimension / (float) bestRoot.w;
				if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Packing exceeded atlas width shrinking to " + scaleFactor);
				for (int i = 0; i < images.Count; i++){
					Image im = images[i];
					int right = (int) ((im.x + im.w) * scaleFactor);
					im.x = (int) (scaleFactor * im.x);
					im.w = right - im.x;
					if (im.w == 0) Debug.LogError("rounding scaled image w to zero");
				}
				outW = maxDimension;
			}
			
			float padY = (float)padding / (float)bestRoot.h;
			if (bestRoot.h > maxDimension){
				padY = (float)padding / (float)maxDimension;
				float scaleFactor = (float) maxDimension / (float) bestRoot.h;
				if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Packing exceeded atlas height shrinking to " + scaleFactor);
				for (int i = 0; i < images.Count; i++){
					Image im = images[i];
					int bottom = (int) ((im.y + im.h) * scaleFactor);
					im.y = (int) (scaleFactor * im.y);
					im.h = bottom - im.y;
					if (im.h == 0) Debug.LogError("rounding scaled image h to zero");
				}
				outH = maxDimension;
			}
			
			Rect[] rs = new Rect[images.Count];
			for (int i = 0; i < images.Count; i++){
				Image im = images[i];
				Rect r = rs[i] = new Rect((float)im.x/(float)outW + padX, 
								 (float)im.y/(float)outH + padY, 
								 (float)im.w/(float)outW - padX*2, 
							     (float)im.h/(float)outH - padY*2);
				if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Image: " + i + " imgID=" + im.imgId + " x=" + r.x * outW +
						   " y=" + r.y * outH + " w=" + r.width * outW +
					       " h=" + r.height * outH + " padding=" + padding);
			}
					
			if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Done GetRects");
			return rs;			
		}
	
		public void RunTestHarness(){
//			int numTex = 16;
//			int min = 128;
//			int max = 256;
			//txs[0] = new Image(0,5,800);
			//txs[1] = new Image(1,5,5000);
			
			List<Vector2> imgsToAdd = new List<Vector2>();
//			for (int i = 0; i < numTex; i++){
//				imgsToAdd.Add(new Vector2(UnityEngine.Random.Range(min,max), UnityEngine.Random.Range(min,max)*5));
//			}
			
			
			
			imgsToAdd.Add(new Vector2(128,128));
			imgsToAdd.Add(new Vector2(256,256));
			imgsToAdd.Add(new Vector2(512,512));
			
			int padding = 1;
			int w;
			int h;
			GetRects(imgsToAdd, 2048, padding, out w, out h);
		}	
	}
}