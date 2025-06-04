// FlyingText3D 2.2
// ©2015 Starscene Software. All rights reserved. Redistribution without permission not allowed.

using UnityEngine;
using System;
using System.Collections.Generic;

namespace FlyingText3D {

public class Triangulate {

	public static bool Compute (List<ContourData> contourList, List<int[]> vertexList, int[] vertexCounts, int[] xMaxVertices, Vector2[] xMaxPoints,
								List<Vector2[]> pointsList, int[] meshTriangles, ref int triIdx, ref int triAdd) {
		
		for (int i = 0; i < pointsList.Count; i++) {
			if (contourList[i].side == Side.In || vertexCounts[i] < 3) continue;
			
			// For each outside line, use any inside lines to make an array that has all vertex references for the entire combined line
			var vertexArray = vertexList[i];
			int vertexCount = vertexCounts[i];
			int triVertCount = vertexCounts[i];
			
			var hasInside = false;
			Vector2[] points = null;
			// If there are any inside lines, copy all points from the appropriate arrays in pointsList into a combined array
			if (i+1 < pointsList.Count && contourList[i+1].side == Side.In) {
				hasInside = true;
				for (int j = i+1; j < pointsList.Count; j++) {
					if (contourList[j].side == Side.Out) break;
					
					triVertCount += vertexCounts[j];
				}
				
				points = new Vector2[triVertCount];
				int idx = vertexCounts[i];
				Array.Copy (pointsList[i], points, idx);
				for (int j = i+1; j < pointsList.Count; j++) {
					if (contourList[j].side == Side.Out) break;
					
					Array.Copy (pointsList[j], 0, points, idx, vertexCounts[j]);
					idx += vertexCounts[j];
				}
			}
			// If there is only an outside line, it can just be a reference
			else {
				points = pointsList[i];
			}
						
			if (hasInside) {
				// Combine inside lines with outside line
				for (i++; i < pointsList.Count; i++) {
					if (contourList[i].side == Side.Out) {
						i--;
						break;
					}
					if (vertexCounts[i] < 3) continue;
					
					int xMaxVertex = xMaxVertices[i];
					var vertexArrayI = vertexList[i];
					int vertexCountI = vertexCounts[i];
					
					// p1, p2 is a line segment starting at the xMax point and extending to the right (essentially a ray)
					Vector2 p1 = xMaxPoints[i];
					Vector2 p2 = new Vector2(999999.0f, p1.y);
					// p3, p4 is another line segment, used when going through all points
					Vector2 p3, p4, p5, thisP;
					Vector2 intersectionP = Vector2.zero;
					float xMax = p1.x;
					float closestDistance = float.MaxValue;
					int cutVertex = 0;
					int a = vertexCount-1;
					// Go through line segments of outside line that have at least 1 point > xMax
					for (int j = 0; j < vertexCount; j++) {
						p3 = points[vertexArray[j]];
						p4 = points[vertexArray[a]];
						if (p3.x >= xMax || p4.x >= xMax) {
							// If p3 is exactly to the right, it counts as an intersection (special case)
							if (p3.y == p1.y) {
								float thisDistance = (p1.x-p3.x) * (p1.x-p3.x);	// Square distance, since .sqrMagnitude is used below
								if (thisDistance <= closestDistance && Vector3.Cross(p3 - p4, p1 - p3).z < 0) {
									intersectionP = p3;
									closestDistance = thisDistance;
									cutVertex = j;
								}
							}
							// Do a test to see if the line segments intersect at all, and that the xMax point is on the correct side of the line if so
							else if ( ((p4.y-p1.y)*(p3.x-p1.x) > (p3.y-p1.y)*(p4.x-p1.x)) != ((p4.y-p2.y)*(p3.x-p2.x) > (p3.y-p2.y)*(p4.x-p2.x)) &&
								 	  ((p3.y-p1.y)*(p2.x-p1.x) > (p2.y-p1.y)*(p3.x-p1.x)) != ((p4.y-p1.y)*(p2.x-p1.x) > (p2.y-p1.y)*(p4.x-p1.x)) &&
								 	  Vector3.Cross(p3 - p4, p1 - p3).z < 0) {
								// They did intersect, so get the intersection point
								p5.x = -(p2.y - p1.y); p5.y = p2.x - p1.x;
								float h = Vector2.Dot(p1 - p3, p5) / Vector2.Dot(p4 - p3, p5);
								thisP = p3 + (p4 - p3)*h;
								float thisDistance = (p1 - thisP).sqrMagnitude;
								if (thisDistance <= closestDistance) {
									intersectionP = thisP;
									closestDistance = thisDistance;
									cutVertex = (p3.x >= xMax)? j : a;	// In case only one point in the intersecting line > xMax, use that one
								}
							}
						}
						a = j;
					}
					
					// See if any reflex points are inside a triangle made from xMax point (p1), intersectionP, and the cut point (p2)
					p2 = points[vertexArray[cutVertex]];
					float minY, maxY;
					if (intersectionP.y < p2.y) {
						minY = intersectionP.y;
						maxY = p2.y;
					}
					else {
						minY = p2.y;
						maxY = intersectionP.y;
					}
					closestDistance = float.MaxValue;
					for (int j = 0; j < vertexCount; j++) {
						p3 = points[vertexArray[j]];
						// Early out for points which can't possibly be inside the triangle
						if (j == cutVertex || p3.x < xMax || p3.y > maxY || p3.y < minY) {
							continue;
						}
						a = j-1;
						if (a < 0) a = vertexCount-1;
						int b = j+1;
						if (b == vertexCount) b = 0;
						if (IsReflex(ref points[vertexArray[a]], ref points[vertexArray[j]], ref points[vertexArray[b]]) &&
								PointInsideTriangle (ref p3, ref p1, ref intersectionP, ref p2) ) {
							float thisDistance = (p1 - p3).sqrMagnitude;
							if (thisDistance <= closestDistance) {
								closestDistance = thisDistance;
								cutVertex = j;
							}
						}
					}
					
					// Make room for the interior line at the cut point
					Array.Resize (ref vertexArray,  vertexCount + vertexCountI + 2);
					Array.Copy (vertexArray,  cutVertex + 1,  vertexArray,  cutVertex + vertexCountI + 3,  vertexCount - cutVertex - 1);
					
					// Copy the interior line into the outside line; if the max x vertex was 0 then it can be done in one go
					if (xMaxVertex == 0) {
						Array.Copy (vertexArrayI,  0,  vertexArray,  cutVertex + 1,  vertexCountI);
					}
					else {
						Array.Copy (vertexArrayI,  xMaxVertex,  vertexArray,  cutVertex + 1,  vertexCountI - xMaxVertex);
						Array.Copy (vertexArrayI,  0,  vertexArray,  cutVertex + (vertexCountI - xMaxVertex) + 1,  xMaxVertex);
					}
					
					// Add duplicate vertex references for the cut
					vertexArray[cutVertex + vertexCountI + 1] = vertexArrayI[xMaxVertex];
					vertexArray[cutVertex + vertexCountI + 2] = vertexArray[cutVertex];
					vertexCount += vertexCountI + 2;
				}
			}
			
			// Debug combined outline ---
//			var outlinePoints = new Vector2[vertexArray.Length];
//			for (int j = 0; j < outlinePoints.Length; j++) {
//				outlinePoints[j] = points[vertexArray[j]];
//			}
//			var outlines = new VectorLine("outline", outlinePoints, null, 4.0f, LineType.Continuous);
//			outlines.Draw();
			
			if (vertexCount > 3) {
				// Find all reflex points
				var reflexArray = new int[vertexCount];
				int reflexIdx = 0;
				int reflexCount = 0;
				int a = vertexCount-1;
				for (int j = 0; j < vertexCount; j++) {
					int b = j+1;
					if (b > vertexCount-1) b = 0;
					if (IsReflex (ref points[vertexArray[a]], ref points[vertexArray[j]], ref points[vertexArray[b]])) {
						reflexArray[reflexIdx++] = vertexArray[j];
						reflexCount++;
					}
					a = j;
				}
				
				// Go through all points to find all triangles
				int vIdx = 0;
				int loopCount = -1;
				Vector2 p1, p0A, p0B, p2A, p2B;
				
				while (true) {
					LoopStart:
					if (++loopCount == vertexCount) {
						Debug.LogWarning ("Couldn't complete triangulation");
						return false;
					}
					
					if (vIdx == vertexCount) vIdx = 0;
					int vP1 = vertexArray[vIdx];
					p1 = points[vP1];
					
					// Neighboring points of p1 that make up a triangle
					int vIdx2 = vIdx-1;
					if (vIdx2 < 0) vIdx2 = vertexCount-1;
					int vP0 = vertexArray[vIdx2];
					p0A = points[vP0];
					int vIdx3 = vIdx+1;
					if (vIdx3 > vertexCount-1) vIdx3 = 0;
					int vP2 = vertexArray[vIdx3];
					p2A = points[vP2];
					
					// If p1 is a reflex point, it can't be an ear
					if (IsReflex (ref p0A, ref p1, ref p2A)) {
						vIdx++;
						continue;
					}
					
					int p0ReflexIdx = -1;
					int p2ReflexIdx = -1;
					// If any reflex points are inside this triangle, it can't be an ear
					for (int j = 0; j < reflexCount; j++) {
						int thisReflexVal = reflexArray[j];
						// But don't include reflex points that actually make up this triangle
						if (thisReflexVal == vP0) {
							p0ReflexIdx = j;
							continue;
						}
						if (thisReflexVal == vP2) {
							p2ReflexIdx = j;
							continue;
						}
						if (thisReflexVal == vP1) continue;	// Technically not needed, but can allow some malformed contours to basically work
						
						if (PointInsideTriangle (ref points[thisReflexVal], ref p0A, ref p1, ref p2A)) {						
							vIdx++;
							goto LoopStart;
						}
					}
					
					loopCount = -1;
					
					// Neighboring points + 1
					if (--vIdx2 < 0) vIdx2 = vertexCount-1;
					p0B = points[vertexArray[vIdx2]];
					if (++vIdx3 > vertexCount-1) vIdx3 = 0;
					p2B = points[vertexArray[vIdx3]];
					
					// Add this triangle to the mesh
					meshTriangles[triIdx  ] = vP0 + triAdd;
					meshTriangles[triIdx+1] = vP1 + triAdd;
					meshTriangles[triIdx+2] = vP2 + triAdd;
					triIdx += 3;
					
					// More or less the same as using RemoveAt for a List, but using an int array is faster than List<int>, or even a linked list
					if (vIdx < vertexCount-1) {	// If it's the last entry, no need to do anything except decrement vertexCount
						Array.Copy (vertexArray, vIdx+1, vertexArray, vIdx, --vertexCount-vIdx);
					}
					else {
						--vertexCount;
					}
					
					// If we're down to 3 vertices, finish up the last triangle
					if (vertexCount == 3) {
						meshTriangles[triIdx  ] = vertexArray[0] + triAdd;
						meshTriangles[triIdx+1] = vertexArray[1] + triAdd;
						meshTriangles[triIdx+2] = vertexArray[2] + triAdd;
						triIdx += 3;
						break;
					}
					
					// If either of the neighbors of the removed point were reflex and now aren't, remove them from the reflex array
					if (p0ReflexIdx != -1 && !IsReflex (ref p0B, ref p0A, ref p2A)) {
						if (p0ReflexIdx < reflexCount-1) {
							Array.Copy (reflexArray, p0ReflexIdx+1, reflexArray, p0ReflexIdx, reflexCount-p0ReflexIdx-1);
						}
						reflexCount--;
						if (p2ReflexIdx > p0ReflexIdx) {
							p2ReflexIdx--;
						}
					}
					if (p2ReflexIdx != -1 && !IsReflex (ref p0A, ref p2A, ref p2B)) {
						if (p2ReflexIdx < reflexCount-1) {
							Array.Copy (reflexArray, p2ReflexIdx+1, reflexArray, p2ReflexIdx, reflexCount-p2ReflexIdx-1);
						}
						reflexCount--;
					}
				}
			}
			// There were exactly 3 vertices
			else {
				meshTriangles[triIdx  ] = triAdd;
				meshTriangles[triIdx+1] = triAdd + 1;
				meshTriangles[triIdx+2] = triAdd + 2;
				triIdx += 3;
				triVertCount = 3;
			}
			triAdd += triVertCount;
		}
		
		return true;
	}
	
	private static bool IsReflex (ref Vector2 a, ref Vector2 b, ref Vector2 c) {
		var d = new Vector2(-(b.y - a.y), b.x - a.x);
		var e = new Vector2(c.x - b.x, c.y - b.y);
		return Vector2.Dot(d, e) > 0.0f;
	}
	
	private static bool PointInsideTriangle (ref Vector2 p, ref Vector2 a, ref Vector2 b, ref Vector2 c) {
		float apX = p.x - a.x;
		float apY = p.y - a.y;
		bool pAB = (b.x-a.x) * apY - (b.y-a.y) * apX > 0.0f;
		if ((c.x-a.x) * apY - (c.y-a.y) * apX > 0.0f == pAB) return false;
		if ((c.x-b.x) * (p.y-b.y) - (c.y-b.y) * (p.x-b.x) > 0.0f != pAB) return false;
		return true;
	}
}

}