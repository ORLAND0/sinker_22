// FlyingText3D 2.2
// ©2015 Starscene Software. All rights reserved. Redistribution without permission not allowed.
//#define VECTROSITYDEBUG

using UnityEngine;
using System;
using System.Collections.Generic;
#if VECTROSITYDEBUG
using Vectrosity;
#endif

namespace FlyingText3D {

public enum Side {In, Out}

public class ContourData {
	public Vector2 maxPoint;
	public Vector2 minPoint;
	public Vector2[] points;
	public bool[] onCurves;
	public Vector2[] renderedPoints;
	public Side side;
	
	public ContourData (Vector2[] points, bool[] onCurves) {
		this.points = points;
		this.onCurves = onCurves;
	}
}

public class InsideData {
	public Vector2 xMaxPoint;
	public int xMaxVertex;
	public Vector2[] points;
}

public class GlyphData {
	private int _glyphIndex;
	public int glyphIndex {
		get {return _glyphIndex;}
	}
	private bool _isVisibleChar = false;
	public bool isVisibleChar {
		get {return _isVisibleChar;}
	}
	private int _resolution;
	public int resolution {
		get {return _resolution;}
	}
	private float _scaleFactor;
	public float scaleFactor {
		get {return _scaleFactor;}
	}
	private float _extrudeDepth = 0.0f;
	public float extrudeDepth {
		get {return _extrudeDepth;}
	}
	private int _vertexCount;
	public int vertexCount {
		get {return _vertexCount;}
	}
	private int _triCount;
	public int triCount {
		get {return _triCount;}
	}
	private int _triCount2;
	public int triCount2 {
		get {return _triCount2;}
	}
	private Vector3[] _baseVertices;
	private Vector3[] _vertices;
	public Vector3[] vertices {
		get {return _vertices;}
	}
	private int[] _baseTriangles;
	private int[] _triangles;
	public int[] triangles {
		get {return _triangles;}
	}
	private int[] _triangles2;
	public int[] triangles2 {
		get {return _triangles2;}
	}
	private bool _useSubmesh;
	public bool useSubmesh {
		get {return _useSubmesh;}
	}
	private bool _useBack;
	public bool useBack {
		get {return _useBack;}
	}
	private bool _triDataComputed = false;
	public bool triDataComputed {
		get {return _triDataComputed;}
	}
	private int _frontTriIndex;
	private int _frontVertIndex;
	public int frontVertIndex {
		get {return _frontVertIndex;}
	}
	private int _xMin;
	public int xMin {
		get {return _xMin;}
	}
	private int _yMin;
	public int yMin {
		get {return _yMin;}
	}
	private int _xMax;
	public int xMax {
		get {return _xMax;}
	}
	private int _yMax;
	public int yMax {
		get {return _yMax;}
	}
	private int _unitsPerEm;
	private bool _reverse;
	private List<ContourData> _contourList;
	
	public GlyphData (List<Vector2[]> pointsList, List<bool[]> onCurvesList, int xMin, int yMin, int xMax, int yMax, int unitsPerEm, int glyphIndex) {
		_glyphIndex = glyphIndex;
		if (pointsList != null) {
			_xMin = xMin; _yMin = yMin; _xMax = xMax; _yMax = yMax;
			_unitsPerEm = unitsPerEm;
			_contourList = SortPointsList (pointsList, onCurvesList);
			_isVisibleChar = true;
		}
		_scaleFactor = -1.0f;
		_resolution = -1;
	}
	
	private List<ContourData> SortPointsList (List<Vector2[]> pointsList, List<bool[]> onCurvesList) {
		var contourList = new List<ContourData>(pointsList.Count);
		for (int i = 0; i < pointsList.Count; i++) {
			contourList.Add (new ContourData(pointsList[i], onCurvesList[i]));
		}
		
		int contourCount = contourList.Count;
		if (contourCount > 1) {
			// Render contours at lowest resolution (the original bezier points aren't accurate enough), then sort by size,
			// in order to get outside line and make sure interior lines are inside the proper exterior lines
			for (int i = 0; i < contourCount; i++) {
				var points = RenderContourPoints (contourList[i], 1, true);
				contourList[i].renderedPoints = points;
				float xMax = points[0].x;
				float xMin = points[0].x;
				float yMax = points[0].y;
				float yMin = points[0].y;
				int end = points.Length;
				for (int j = 1; j < end; j++) {
					if (points[j].x > xMax) xMax = points[j].x;
					else if (points[j].x < xMin) xMin = points[j].x;
					if (points[j].y > yMax) yMax = points[j].y;
					else if (points[j].y < yMin) yMin = points[j].y;
				}
				contourList[i].maxPoint = new Vector2(xMax, yMax);
				contourList[i].minPoint = new Vector2(xMin, yMin);
			}
			contourList.Sort (AreaCompare);
		}
		
		// If outside line is not clockwise, then the winding order is backwards, so set reverse flag to make all contours be reversed when triangulated
		_reverse = !IsClockwise (contourList[contourCount-1].points);
		for (int i = 0; i < contourCount; i++) {
			if (!_reverse) {
				contourList[i].side = IsClockwise (contourList[i].points)? Side.Out : Side.In;
			}
			else {
				contourList[i].side = IsClockwise (contourList[i].points)? Side.In : Side.Out;
				if (contourCount > 1) Array.Reverse (contourList[i].renderedPoints);
			}
		}
		
		if (contourCount == 1) return contourList;
		
		// Create another list where outside lines are followed by appropriate inside lines
		var contourList2 = new List<ContourData>(contourCount);
		for (int i = 0; i < contourList.Count; i++) {
			var contour = contourList[i];
			if (contour.side == Side.Out) {
				contourList2.Add (contour);
				contourList.RemoveAt (i--);
				for (int j = 0; j < contourList.Count; j++) {
					if (contourList[j].side == Side.In && PolyContainsPoint (contour.renderedPoints, contourList[j].renderedPoints[0])) {
						contourList2.Add (contourList[j]);
						contourList.RemoveAt (j--);
					}
				}
				i = -1;
			}
		}
		
		return contourList2;
	}
	
	private bool PolyContainsPoint (Vector2[] polyPoints, Vector2 p) {
		var inside = false;
		int j = polyPoints.Length-1;
		int end = polyPoints.Length;
		for (int i = 0; i < end; j = i++) {
			if ( ((polyPoints[i].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[i].y)) &&
					(p.x < (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x))
				inside = !inside;
		}
		return inside;
	}
	
	private int AreaCompare (ContourData a, ContourData b) {
		return ( (a.maxPoint.x - a.minPoint.x) * (a.maxPoint.y - a.minPoint.y) ).
			CompareTo ( (b.maxPoint.x - b.minPoint.x) * (b.maxPoint.y - b.minPoint.y) );
	}
	
	private int XMaxCompare (InsideData a, InsideData b) {
		return (a.xMaxPoint.x < b.xMaxPoint.x).
			CompareTo (b.xMaxPoint.x < a.xMaxPoint.x);
	}
	
	private Vector2[] RenderContourPoints (ContourData contour, int resolution, bool initialTest) {
#if VECTROSITYDEBUG
		var baseLine = new VectorLine("base", contour.points, new Color(1, 1, 1, .15f), null, 2.0f, LineType.Continuous);
		baseLine.depth = 1;
		baseLine.Draw();
		var col = new Color[contour.points.Length];
		for (var i = 0; i < col.Length; i++) {
			col[i] = (contour.onCurves[i] == true)? Color.green : Color.red;
		}
		var pts = new VectorPoints("points", contour.points, col, null, 15.0f);
		pts.Draw();
#endif
		
		_resolution = resolution;
		var originalPoints = contour.points;
		var onCurves = contour.onCurves;
		int numberOfPoints = originalPoints.Length;
		var points = new List<Vector2>(numberOfPoints * resolution);
		float baseDist = (_unitsPerEm / 2) / (float)(resolution + 2);
		Vector2 p0, p1, p2, p = Vector2.zero;
		int iAdd = 0;
		int i0 = 0;
		while (i0 < numberOfPoints) {
			int i1 = i0 + 1;
			if (i1 == numberOfPoints) i1 = 0;
			
			if (onCurves[i0]) {
				// Straight line (two on-curve points in a row)
				if (onCurves[i1]) {
					points.Add (originalPoints[i0]);
					i0++;
					continue;
				}
				else {
					// Curve that starts with an on-curve point, and might end with one
					int i2 = i1 + 1;
					if (i2 == numberOfPoints) i2 = 0;
					p0 = originalPoints[i0];
					p1 = originalPoints[i1];
					if (onCurves[i2]) {
						p2 = originalPoints[i2];
					}
					else {
						p2 = (originalPoints[i1] + originalPoints[i2]) / 2;
					}
					iAdd = 2;
				}
			}
			else {
				// If the line starts with an off-curve point and doesn't end with one, skip over the first point since it's not supposed to be rendered
				if (i0 == 0 && (numberOfPoints > 1 && onCurves[numberOfPoints-1])) {
					i0++;
					continue;
				}
				// Curve that doesn't start with an on-curve point, but might end with one
				int ip = i0 - 1;
				if (ip == -1) ip = numberOfPoints-1;
				p0 = (originalPoints[ip] + originalPoints[i0]) / 2;
				p1 = originalPoints[i0];
				if (onCurves[i1]) {
					p2 = originalPoints[i1];
				}
				else {
					p2 = (originalPoints[i0] + originalPoints[i1]) / 2;
				}
				iAdd = 1;
			}
			
			// Limit subdivision for short segments
			int thisRes;
			float segmentLength = (p0 - p2).magnitude;
			if (initialTest || segmentLength < baseDist) {
				thisRes = 1;
			}
			else {
				thisRes = (int)(segmentLength / baseDist);
			}
			
			// Do Bezier curve
			float t = 0.0f;
			float tAdd = 1.0f / (thisRes+1);
			for (int i = 0; i <= thisRes; i++) {
				float ti = (1.0f-t);
				p.x = ti*ti*p0.x + 2*t*ti*p1.x + t*t*p2.x;
				p.y = ti*ti*p0.y + 2*t*ti*p1.y + t*t*p2.y;
				points.Add (p);
				t += tAdd;
			}
			
			i0 += iAdd;
		}
		
		// Un-subdivide subsequent segments that are nearly a straight line anyway
		if (!initialTest) {
			int pCount = points.Count;
			baseDist *= 4;
			for (int i = 0; i < pCount;) {
				int ia = i - 1;
				if (ia < 0) ia = pCount - 1;
				int ib = i + 1;
				if (ib > pCount - 1) ib = 0;
				p0 = points[ia];
				p1 = points[i];
				p2 = points[ib];
				if ((p0 - p2).magnitude > baseDist && LineToPointSqrDistance (ref p0, ref p2, ref p1) < .1f) {
					points.RemoveAt (i);
					pCount--;
				}
				else {
					i++;
				}
			}
		}
		
#if VECTROSITYDEBUG
		if (points.Count > 1 && !initialTest) {
			var renderedLine = new VectorLine("render", points.ToArray(), Color.cyan, null, 2.0f, LineType.Continuous);
			renderedLine.depth = 0;
			renderedLine.Draw();
			var renderedLinePoints = new VectorPoints("render", points.ToArray(), Color.white, null, 8.0f);
			renderedLinePoints.depth = 50;
			renderedLinePoints.SetColor (Color.magenta, points.Count-1);
			renderedLinePoints.Draw();
		}
#endif
		
		var pointsArray = points.ToArray();
		if (_reverse) {
			Array.Reverse (pointsArray);
		}
		return pointsArray;
	}

	public bool SetMeshData (int resolution) {
		_triDataComputed = false;	
		var pointsList2 = new List<Vector2[]>(_contourList.Count);
		
		// Sort any inside lines by largest X value
		var xMaxVertices = new int[_contourList.Count];
		var xMaxPoints = new Vector2[_contourList.Count];
		for (int i = 0; i < _contourList.Count; i++) {
			if (_contourList[i].side == Side.Out) {
				pointsList2.Add (RenderContourPoints (_contourList[i], resolution, false));
				continue;
			}
			
			var insideList = new List<InsideData>();
			int arrayIdx = i;
			for (; i < _contourList.Count; i++) {
				var inside = new InsideData();
				inside.points = RenderContourPoints (_contourList[i], resolution, false);
				var points = inside.points;
				int xMaxVertex = 0;
				float xMax = points[0].x;
				float yVal = points[0].y;
				int end = points.Length;
				for (int j = 1; j < end; j++) {
					if (points[j].x > xMax) {
						xMax = points[j].x;
						yVal = points[j].y;
						xMaxVertex = j;
					}
				}
				
				inside.xMaxPoint = new Vector2(xMax, yVal);
				inside.xMaxVertex = xMaxVertex;
				insideList.Add (inside);
				
				if (i+1 == _contourList.Count || _contourList[i+1].side == Side.Out) {
					insideList.Sort (XMaxCompare);
					for (int j = 0; j < insideList.Count; j++) {
						pointsList2.Add (insideList[j].points);
						xMaxVertices[arrayIdx] = insideList[j].xMaxVertex;
						xMaxPoints[arrayIdx++] = insideList[j].xMaxPoint;
					}
					break;
				}
			}
		}
		
		// Find out how many triangles/vertices there will be, and make a list that holds an array of vertex references for each line
		var vertexCounts = new int[pointsList2.Count];
		var vertexList = new List<int[]>(pointsList2.Count);
		int vertexIndex = 0;
		int totalVertices = 0;
		int pointCount = 0;
		int totalTriangles = 0;
		
		for (int i = 0; i < pointsList2.Count; i++) {
			if (_contourList[i].side == Side.Out && i > 0) {
				totalTriangles += (pointCount-2) * 3;
				pointCount = 0;
			}
			
			var points = pointsList2[i];
			
			int vertexCount = points.Length;
			if (points.Length > 2 && points[0] == points[vertexCount-1]) vertexCount--;
			if (vertexCount < 3) {
				vertexList.Add (null);
				continue;
			}
			vertexCounts[i] = vertexCount;
			totalVertices += vertexCount;
			
			if (_contourList[i].side == Side.Out) vertexIndex = 0;
			var vertexArray = new int[vertexCount];
			for (var j = 0; j < vertexCount; j++) {
				vertexArray[j] = j + vertexIndex;
			}
			vertexIndex += vertexCount;
			vertexList.Add (vertexArray);
			
			pointCount += vertexCount;
			if (_contourList[i].side == Side.In) pointCount += 2;	// Because each interior line requires a "cut" with 2 additional points
		}
		
		// Debug outlines ---
//		for (int i = 0; i < pointsList2.Count; i++) {
//			if (vertexCounts[i] < 3) continue;
//			var lineColor = _contourList[i].side == Side.Out? Color.green : Color.red;
//			var outline = new VectorLine("contour "+i, pointsList2[i], lineColor, null, 2.0f, LineType.Continuous);
//			outline.depth = 1;
//			outline.Draw();
//		}
		
		totalTriangles += (pointCount-2) * 3;
		int totalFrontVertices = totalVertices;
		int totalFrontTriangles = totalTriangles;
		// Allow for backface
		totalVertices *= 2;
		totalTriangles *= 2;
		// Allow for extrusion
		for (int i = 0; i < vertexCounts.Length; i++) {
			totalVertices += vertexCounts[i] * 4;
			totalTriangles += vertexCounts[i] * 6;
		}
		
		if (totalVertices > 65534) {
			Debug.LogError ("Too many points...resolution is too high or character is too complex");
			return false;
		}
		var meshVertices = new Vector3[totalVertices];
		var meshTriangles = new int[totalTriangles];
		
		// Copy all Vector2 points to the mesh vertices array
		int idx = 0;
		for (int i = 0; i < pointsList2.Count; i++) {
			var points = pointsList2[i];
			int vertexCount = vertexCounts[i];
			for (int j = 0; j < vertexCount; j++) {
				meshVertices[idx  ].x = points[j].x;
				meshVertices[idx++].y = points[j].y;
			}
		}
		
		int triIdx = 0;
		int triAdd = 0;
		if (!Triangulate.Compute (_contourList, vertexList, vertexCounts, xMaxVertices, xMaxPoints, pointsList2, meshTriangles, ref triIdx, ref triAdd)) {
			return false;
		}
		
		// Do verts and triangles for back face
		Array.Copy (meshVertices, 0, meshVertices, totalFrontVertices, totalFrontVertices);
		for (int i = 0; i < totalFrontTriangles; i += 3) {
			meshTriangles[i + totalFrontTriangles    ] = meshTriangles[i + 2] + totalFrontVertices;
			meshTriangles[i + totalFrontTriangles + 1] = meshTriangles[i + 1] + totalFrontVertices;
			meshTriangles[i + totalFrontTriangles + 2] = meshTriangles[i    ] + totalFrontVertices;
		}
		
		// Do edges for extrusion ... arrangement is |front|back|edge|
		int frontIdx = 0;
		int backIdx = totalFrontVertices;
		int edgeIdx = totalFrontVertices * 2;
		Vector2 l1, l2;
		float p2x, p2y;
		float smoothAngle = Mathf.Clamp (FlyingText.smoothingAngle, 0.0f, 180.0f);
		triIdx = totalFrontTriangles * 2;
		triAdd *= 2;
		int edgeVertCount = 0, edgeTriCount = 0;
		
		for (int i = 0; i < pointsList2.Count; i++) {
			if (vertexCounts[i] < 3) continue;
			int vEnd = vertexCounts[i], j1 = 0, j2 = 0;
			int originalTriAdd = triAdd;
			int vCount = 0;
			for (int j = 0; j < vEnd; j++) {
				j1 = (j + 1 == vEnd)? 0 : j + 1;
				j2 = (j1 + 1 == vEnd)? 0 : j1 + 1;
				
				// Get this segment and the next one, so the angle can be computed
				p2x = meshVertices[frontIdx + j1].x; 		p2y = meshVertices[frontIdx + j1].y;
				l1.x = meshVertices[frontIdx + j].x - p2x; 	l1.y = meshVertices[frontIdx + j].y - p2y;
				l2.x = meshVertices[frontIdx + j2].x - p2x;	l2.y = meshVertices[frontIdx + j2].y - p2y;
				
				meshVertices[edgeIdx    ] = meshVertices[frontIdx + j];
				meshVertices[edgeIdx + 1] = meshVertices[backIdx + j];
				if (vCount != 0) {
					AddTriangle (meshTriangles, ref triAdd, ref triIdx, ref edgeTriCount, ref vCount);
				}
				if (180.0f - Vector2.Angle(l1, l2) >= smoothAngle) {
					meshVertices[edgeIdx + 2] = meshVertices[frontIdx + j1];
					meshVertices[edgeIdx + 3] = meshVertices[backIdx + j1];
					edgeIdx += 4;
					vCount = 4;
					edgeVertCount += 4;
				}
				else {
					edgeIdx += 2;
					vCount = 2;
					edgeVertCount += 2;
				}
			}
			if (vCount == 4) {
				AddTriangle (meshTriangles, ref triAdd, ref triIdx, ref edgeTriCount, ref vCount);
			}
			else {
				meshTriangles[triIdx    ] = triAdd;
				meshTriangles[triIdx + 1] = triAdd + 1;
				meshTriangles[triIdx + 2] = originalTriAdd;
				
				meshTriangles[triIdx + 3] = triAdd + 1;
				meshTriangles[triIdx + 4] = originalTriAdd + 1;
				meshTriangles[triIdx + 5] = originalTriAdd;
				
				triIdx += 6;
				triAdd += vCount;
				edgeTriCount += 6;				
			}
			idx += vEnd;
			frontIdx += vEnd;
			backIdx += vEnd;
		}
		
		if (meshVertices.Length != totalFrontVertices * 2 + edgeVertCount) {
			Array.Resize (ref meshVertices, totalFrontVertices * 2 + edgeVertCount);
		}
		if (meshTriangles.Length != totalFrontTriangles * 2 + edgeTriCount) {
			Array.Resize (ref meshTriangles, totalFrontTriangles * 2 + edgeTriCount);
		}
		_baseVertices = meshVertices;
		_baseTriangles = meshTriangles;
		_frontVertIndex = totalFrontVertices;
		_frontTriIndex = totalFrontTriangles;
		_scaleFactor = -1.0f;
		return true;
	}
	
	private void AddTriangle (int[] meshTriangles, ref int triAdd, ref int triIdx, ref int edgeTriCount, ref int vCount) {
		meshTriangles[triIdx    ] = triAdd;
		meshTriangles[triIdx + 1] = triAdd + 1;
		meshTriangles[triIdx + 2] = triAdd + 2;
		
		meshTriangles[triIdx + 3] = triAdd + 1;
		meshTriangles[triIdx + 4] = triAdd + 3;
		meshTriangles[triIdx + 5] = triAdd + 2;
		
		triIdx += 6;
		triAdd += vCount;
		edgeTriCount += 6;
	}
	
	public int GetVertexCount (bool extrude, bool includeBackface) {
		if (!extrude) {
			return _frontVertIndex;
		}
		if (includeBackface) {
			return _baseVertices.Length;
		}
		return _baseVertices.Length - _frontVertIndex;
	}
	
	// The original vertices are kept intact (_baseVertices), so they don't have to be recomputed when the character is scaled
	public void ScaleVertices (float scaleFactor, bool extrude, bool includeBackface) {
		if (!extrude) {
			CopyAndScale (scaleFactor, _frontVertIndex);
		}
		else {
			if (includeBackface) {
				CopyAndScale (scaleFactor, _baseVertices.Length);
			}
			else {
				CopyAndScale (scaleFactor, _frontVertIndex);
				CopyAndScale (scaleFactor, _frontVertIndex * 2,  _frontVertIndex,  _baseVertices.Length - _frontVertIndex * 2);
			}
		}
		_scaleFactor = scaleFactor;
	}
	
	private void CopyAndScale (float scaleFactor, int length) {
		for (var i = 0; i < length; i++) {
			_vertices[i].x = _baseVertices[i].x * scaleFactor;
			_vertices[i].y = _baseVertices[i].y * scaleFactor;
		}
	}
	
	private void CopyAndScale (float scaleFactor, int source, int dest, int length) {
		for (var i = 0; i < length; i++) {
			_vertices[dest + i].x = _baseVertices[source + i].x * scaleFactor;
			_vertices[dest + i].y = _baseVertices[source + i].y * scaleFactor;
		}
	}
	
	// Only frontface
	public void SetFrontTriData () {
		_triangles = new int[_frontTriIndex];
		Array.Copy (_baseTriangles, _triangles, _frontTriIndex);
		_triCount = _frontTriIndex;
		
		_vertices = new Vector3[_frontVertIndex];
		_vertexCount = _frontVertIndex;
		_scaleFactor = -1.0f;
		_triDataComputed = true;
	}
	
	// Extruded, no backface
	public void SetFrontAndEdgeTriData (bool doSubmesh) {
		if (!doSubmesh) {
			// Front
			_triangles = new int[_baseTriangles.Length - _frontTriIndex];
			Array.Copy (_baseTriangles, _triangles, _frontTriIndex);
			
			// Edge
			int end = _baseTriangles.Length;
			int tIdx = _frontTriIndex;
			for (int i = _frontTriIndex * 2; i < end; i++) {
				_triangles[tIdx++] = _baseTriangles[i] - _frontVertIndex;
			}
			_triCount = _triangles.Length;
		}
		else {
			// Front
			_triangles = new int[_frontTriIndex];
			Array.Copy (_baseTriangles, _triangles, _frontTriIndex);
			_triCount = _frontTriIndex;
			
			// Edge
			_triangles2 = new int[_baseTriangles.Length - _frontTriIndex*2];
			int end = _baseTriangles.Length;
			int tIdx = 0;
			for (int i = _frontTriIndex * 2; i < end; i++) {
				_triangles2[tIdx++] = _baseTriangles[i] - _frontVertIndex;
			}
			_triCount2 = _triangles2.Length;
		}
		
		_useSubmesh = doSubmesh;
		_vertices = new Vector3[_baseVertices.Length - _frontVertIndex];
		_vertexCount = _vertices.Length;
		_scaleFactor = -1.0f;
		_extrudeDepth = -1.0f;
		_triDataComputed = true;
	}

	// Extruded + backface
	public void SetTriData (bool doSubmesh) {
		if (!doSubmesh) {
			_triangles = _baseTriangles;
			_triCount = _triangles.Length;
		}
		else {
			// Front
			_triangles = new int[_frontTriIndex*2];
			Array.Copy (_baseTriangles, _triangles, _frontTriIndex*2);
			_triCount = _triangles.Length;
			
			// Edge
			_triangles2 = new int[_baseTriangles.Length - _frontTriIndex*2];
			Array.Copy (_baseTriangles, _frontTriIndex*2, _triangles2, 0, _baseTriangles.Length - _frontTriIndex*2);
			_triCount2 = _triangles2.Length;
		}
		
		_useSubmesh = doSubmesh;
		_vertices = new Vector3[_baseVertices.Length];
		_vertexCount = _vertices.Length;
		_scaleFactor = -1.0f;	// Force ScaleVertices to run, so _vertices contains data
		_extrudeDepth = -1.0f;
		_triDataComputed = true;
	}
	
	public void SetExtrudeDepth (float depth, bool includeBackface) {
		if (includeBackface) {
			int i = 0;
			int vEnd = _frontVertIndex * 2;
			for (i = _frontVertIndex; i < vEnd; i++) {
				_vertices[i].z = depth;
			}
			vEnd = _vertices.Length;
			for (i++; i < vEnd; i += 2) {
				_vertices[i].z = depth;
			}
		}
		else {
			int vEnd = _vertices.Length;
			for (int i = _frontVertIndex + 1; i < vEnd; i += 2) {
				_vertices[i].z = depth;
			}
		}
		_extrudeDepth = depth;
		_useBack = includeBackface;
	}
	
	private float LineToPointSqrDistance (ref Vector2 p1, ref Vector2 p2, ref Vector2 p) {
		float l2 = (p2 - p1).sqrMagnitude;
		if (l2 == 0.0f) return (p - p1).sqrMagnitude;
		float t = Vector2.Dot(p - p1, p2 - p1) / l2;
		if (t < 0.0f) return (p - p1).sqrMagnitude;
		else if (t > 1.0f) return (p - p2).sqrMagnitude;
		Vector2 projection = p1 + t * (p2 - p1);
		return (p - projection).sqrMagnitude;
	}
	
	public static bool IsClockwise (Vector2[] points) {
		int numberOfPoints = points.Length;
		float area = 0.0f;
		for (int p = numberOfPoints - 1, q = 0; q < numberOfPoints; p = q++) {
			area += points[p].x * points[q].y - points[q].x * points[p].y;
		}
		return (area <= 0.0f);
	}
}

}