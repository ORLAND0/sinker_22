// FlyingText3D 2.2
// ©2015 Starscene Software. All rights reserved. Redistribution without permission not allowed.

using UnityEngine;
using System;
using System.Globalization;
using System.Collections.Generic;
using FlyingText3D;

public class EdgeData {
	public int frontVertIndex = 0;
	public int vertexCount = 0;
	
	public EdgeData (int frontVertIndex, int vertexCount) {
		this.frontVertIndex = frontVertIndex;
		this.vertexCount = vertexCount;
	}
}

[AddComponentMenu("FlyingText3D/FlyingText")]
public class FlyingText : MonoBehaviour {
	
	public List<FontData> m_fontData;
	public int m_defaultFont = 0;
	public Material m_defaultMaterial;
	public Material m_defaultEdgeMaterial;
	public bool m_useEdgeMaterial = false;
	public Color m_defaultColor = Color.white;
	public bool m_computeTangents = false;
	public int m_defaultResolution = 5;
	public float m_defaultSize = 2.0f;
	public float m_defaultDepth = 0.25f;
	public float m_defaultLetterSpacing = 1.0f;
	public float m_defaultLineSpacing = 1.0f;
	public float m_defaultLineWidth = 0.0f;
	public float m_tabStop = 0.0f;
	public bool m_wordWrap = true;
	public Justify m_defaultJustification = Justify.Left;
	public bool m_verticalLayout = false;
	public bool m_includeBackface = true;
	public bool m_texturePerLetter = true;
	public TextAnchor m_anchor = TextAnchor.UpperLeft;
	public ZAnchor m_zAnchor = ZAnchor.Front;
	public ColliderType m_colliderType = ColliderType.None;
	public bool m_addRigidbodies = false;
	public PhysicMaterial m_physicsMaterial;
	public float m_smoothingAngle = 50.0f;
	
	public static int defaultFont;
	public static Material defaultMaterial;
	public static Material defaultEdgeMaterial;
	public static bool useEdgeMaterial = false;
	public static Color defaultColor;
	public static bool computeTangents = false;
	public static int defaultResolution;
	public static float defaultSize;
	public static float defaultDepth;
	public static float defaultLetterSpacing;
	public static float defaultLineSpacing;
	public static float defaultLineWidth;
	public static float tabStop;
	public static bool wordWrap;
	public static Justify defaultJustification;
	public static bool verticalLayout;
	public static bool includeBackface;
	public static bool texturePerLetter;
	public static TextAnchor anchor;
	public static ZAnchor zAnchor;
	public static ColliderType colliderType;
	public static bool addRigidbodies;
	public static PhysicMaterial physicsMaterial;
	public static float smoothingAngle;
		
	private static FlyingText _instance;
	public static FlyingText instance {
		get {
			if (_instance == null) {
				_instance = FindObjectOfType (typeof (FlyingText)) as FlyingText;
			}
			return _instance;
		}
	}
	private static bool _initialized = false;
	private static bool _noFontsAvailable = false;
	private static TTFFontInfo[] _fontInfo;
	private static string[] _fontNames;
	private static char[] _removeChars = {' ', '\n', '\r', '"', '\'', '\t'};
	private static Dictionary<string, Color32> _colorDictionary;
	private static GameObject[] objectArray;
	
	private const int PREPROCESS = 2;
	private const int BUILDMESH = 1;
	
	private void Awake () {
		if (FindObjectsOfType (typeof(FlyingText)).Length > 1) {
			Destroy (this);
			return;
		}
		if (!_initialized) {
			Initialize();
		}
	}
	
	public void Initialize () {
		defaultFont = m_defaultFont;
		defaultMaterial = m_defaultMaterial;
		defaultEdgeMaterial = m_defaultEdgeMaterial;
		useEdgeMaterial = m_useEdgeMaterial;
		computeTangents = m_computeTangents;
		defaultColor = m_defaultColor;
		defaultResolution = m_defaultResolution;
		defaultSize = m_defaultSize;
		defaultDepth = m_defaultDepth;
		defaultLetterSpacing = m_defaultLetterSpacing;
		defaultLineSpacing = m_defaultLineSpacing;
		defaultLineWidth = m_defaultLineWidth;
		tabStop = m_tabStop;
		wordWrap = m_wordWrap;
		defaultJustification = m_defaultJustification;
		verticalLayout = m_verticalLayout;
		includeBackface = m_includeBackface;
		texturePerLetter = m_texturePerLetter;
		anchor = m_anchor;
		zAnchor = m_zAnchor;
		colliderType = m_colliderType;
		addRigidbodies = m_addRigidbodies;
		physicsMaterial = m_physicsMaterial;
		smoothingAngle = m_smoothingAngle;
		if (defaultMaterial == null) {
			SetMaterial (ref defaultMaterial);
		}
		if (defaultEdgeMaterial == null) {
			SetMaterial (ref defaultEdgeMaterial);
		}
				
		if (m_fontData.Count == 0) {
			_noFontsAvailable = true;
			_initialized = false;
			return;
		}
		_noFontsAvailable = false;
		_fontInfo = new TTFFontInfo[m_fontData.Count];
		_fontNames = new string[m_fontData.Count];
		
		for (int i = 0; i < m_fontData.Count; i++) {
			if (m_fontData[i].ttfFile != null) {
				_fontInfo[i] = new TTFFontInfo (m_fontData[i].ttfFile.bytes);
				var name = _fontInfo[i].name;
				name = name.Replace(" ", "");
				name = name.ToLower();
				_fontNames[i] = name;
			}
		}

		_colorDictionary = new Dictionary<string, Color32>(){{"red", Color.red}, {"green", Color.green}, {"blue", Color.blue}, {"white", Color.white}, {"black", Color.black}, {"yellow", Color.yellow}, {"cyan", Color.cyan}, {"magenta", Color.magenta}, {"gray", Color.gray}, {"grey", Color.grey}};
		DontDestroyOnLoad (this);
		_initialized = true;
	}
	
	private void SetMaterial (ref Material thisMaterial) {
		var mat = Resources.Load ("VertexColored") as Material;
		if (mat) {	
			thisMaterial = mat;
		}
		else {
			var shader = Shader.Find ("Diffuse");
			if (shader) {
				thisMaterial = new Material(shader);
			}
		}
	}
	
	private static bool CheckSetup () {
		if (!_initialized) {
			if (_noFontsAvailable) {
				Debug.LogError ("No fonts have been defined. Please add at least one font to the FlyingText3D inspector.");
				return false;
			}
			Debug.LogError ("FlyingText hasn't been initialized yet...use script execution order to make sure your Awake functions run first, or use Start");
			return false;
		}
		return true;
	}

	public static void PrimeText (string text) {
		if (!CheckSetup()) return;
		GetObject (text, defaultMaterial, defaultEdgeMaterial, defaultSize, defaultDepth, defaultResolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, true, true, false, Vector3.zero, Quaternion.identity, null);
	}
	
	public static void PrimeText (string text, float size, float extrudeDepth, int resolution) {
		if (!CheckSetup()) return;
		GetObject (text, defaultMaterial, defaultEdgeMaterial, size, extrudeDepth, resolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, true, true, false, Vector3.zero, Quaternion.identity, null);
	}
	
	// GetObject
	public static GameObject GetObject (string text) {
		if (!CheckSetup()) return null;
		return GetObject (text, defaultMaterial, defaultEdgeMaterial, defaultSize, defaultDepth, defaultResolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, false, false, Vector3.zero, Quaternion.identity, null);
	}
	
	public static GameObject GetObject (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution) {
		if (!CheckSetup()) return null;
		return GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, false, false, Vector3.zero, Quaternion.identity, null);
	}
	
	public static GameObject GetObject (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution, float characterSpacing, float lineSpacing, float lineWidth) {
		if (!CheckSetup()) return null;
		return GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, characterSpacing, lineSpacing, lineWidth, false, false, false, Vector3.zero, Quaternion.identity, null);
	}

	public static GameObject GetObject (string text, Vector3 position, Quaternion rotation) {
		if (!CheckSetup()) return null;
		return GetObject (text, defaultMaterial, defaultEdgeMaterial, defaultSize, defaultDepth, defaultResolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, false, false, position, rotation, null);
	}
	
	public static GameObject GetObject (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution, Vector3 position, Quaternion rotation) {
		if (!CheckSetup()) return null;
		return GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, false, false, position, rotation, null);
	}
	
	public static GameObject GetObject (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution, float characterSpacing, float lineSpacing, float lineWidth, Vector3 position, Quaternion rotation) {
		if (!CheckSetup()) return null;
		return GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, characterSpacing, lineSpacing, lineWidth, false, false, false, position, rotation, null);
	}

	// GetObjects
	public static GameObject GetObjects (string text) {
		if (!CheckSetup()) return null;
		return GetObject (text, defaultMaterial, defaultEdgeMaterial, defaultSize, defaultDepth, defaultResolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, true, false, Vector3.zero, Quaternion.identity, null);
	}
	
	public static GameObject GetObjects (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution) {
		if (!CheckSetup()) return null;
		return GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, true, false, Vector3.zero, Quaternion.identity, null);
	}
	
	public static GameObject GetObjects (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution, float characterSpacing, float lineSpacing, float lineWidth) {
		if (!CheckSetup()) return null;
		return GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, characterSpacing, lineSpacing, lineWidth, false, true, false, Vector3.zero, Quaternion.identity, null);
	}

	public static GameObject GetObjects (string text, Vector3 position, Quaternion rotation) {
		if (!CheckSetup()) return null;
		return GetObject (text, defaultMaterial, defaultEdgeMaterial, defaultSize, defaultDepth, defaultResolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, true, false, position, rotation, null);
	}
	
	public static GameObject GetObjects (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution, Vector3 position, Quaternion rotation) {
		if (!CheckSetup()) return null;
		return GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, true, false, position, rotation, null);
	}
	
	public static GameObject GetObjects (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution, float characterSpacing, float lineSpacing, float lineWidth, Vector3 position, Quaternion rotation) {
		if (!CheckSetup()) return null;
		return GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, characterSpacing, lineSpacing, lineWidth, false, true, false, position, rotation, null);
	}

	// GetObjectsArray
	public static GameObject[] GetObjectsArray (string text) {
		if (!CheckSetup()) return null;
		GetObject (text, defaultMaterial, defaultEdgeMaterial, defaultSize, defaultDepth, defaultResolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, true, true, Vector3.zero, Quaternion.identity, null);
		return objectArray;
	}

	public static GameObject[] GetObjectsArray (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution) {
		if (!CheckSetup()) return null;
		GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, true, true, Vector3.zero, Quaternion.identity, null);
		return objectArray;
	}

	public static GameObject[] GetObjectsArray (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution, float characterSpacing, float lineSpacing, float lineWidth) {
		if (!CheckSetup()) return null;
		GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, characterSpacing, lineSpacing, lineWidth, false, true, true, Vector3.zero, Quaternion.identity, null);
		return objectArray;
	}

	public static GameObject[] GetObjectsArray (string text, Vector3 position, Quaternion rotation) {
		if (!CheckSetup()) return null;
		GetObject (text, defaultMaterial, defaultEdgeMaterial, defaultSize, defaultDepth, defaultResolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, true, true, position, rotation, null);
		return objectArray;
	}

	public static GameObject[] GetObjectsArray (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution, Vector3 position, Quaternion rotation) {
		if (!CheckSetup()) return null;
		GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, true, true, position, rotation, null);
		return objectArray;
	}

	public static GameObject[] GetObjectsArray (string text, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution, float characterSpacing, float lineSpacing, float lineWidth, Vector3 position, Quaternion rotation) {
		if (!CheckSetup()) return null;
		GetObject (text, material, edgeMaterial, size, extrudeDepth, resolution, characterSpacing, lineSpacing, lineWidth, false, true, true, position, rotation, null);
		return objectArray;
	}
	
	// UpdateObject
	public static void UpdateObject (GameObject go, string text) {
		if (go == null) {
			Debug.LogError ("UpdateObject can't use a null GameObject");
			return;
		}
		GetObject (text, defaultMaterial, defaultEdgeMaterial, defaultSize, defaultDepth, defaultResolution, defaultLetterSpacing, defaultLineSpacing, defaultLineWidth, false, false, false, Vector3.zero, Quaternion.identity, go);
	}
	
	private static GameObject GetObject (string s, Material material, Material edgeMaterial, float size, float extrudeDepth, int resolution, float characterSpacing, float lineSpacing, float lineWidth, bool prime, bool separateObjects, bool useObjectsArray, Vector3 position, Quaternion rotation, GameObject gObject) {
		if (!_initialized) {
			Debug.LogError ("FlyingText: No font information available");
			return null;
		}
		if (s == null) {
			Debug.LogError ("FlyingText: String can't be null");
			return null;
		}
		bool updateObject = (gObject != null);
		bool separateEdge;
		Mesh mesh;
		
		if (updateObject) {
			var mf = gObject.GetComponent<MeshFilter>();
			if (mf == null) {
				Debug.LogError ("The GameObject must have a MeshFilter component");
				return null;
			}
			mesh = mf.mesh;
			separateEdge = (mesh.subMeshCount == 2);
			
			var dataScript = gObject.GetComponent<TextObjectData>();
			if (dataScript == null) {
				Debug.LogError ("The GameObject must have a TextObjectData component");
				return null;
			}
			dataScript.InitializeData (ref size, ref extrudeDepth, ref resolution, ref characterSpacing, ref lineSpacing, ref lineWidth);
		}
		else {
			mesh = null;
			separateEdge = useEdgeMaterial && extrudeDepth > 0.0f;
			if (material == null) {
				material = defaultMaterial;
			}
			if (edgeMaterial == null) {
				edgeMaterial = defaultEdgeMaterial;
			}
		}
		Material[] materialsArray = separateEdge && !updateObject? new Material[]{material, edgeMaterial} : null;
		if (resolution < 1) {
			resolution = 1;
		}
		if (size < 0.001f) {
			size = 0.001f;
		}
		if (extrudeDepth < 0.0f) {
			extrudeDepth = 0.0f;
		}
		if (lineWidth < 0.0f) {
			lineWidth = 0.0f;
		}
		if (tabStop < 0.0f) {
			tabStop = 0.0f;
		}
		bool useWordWrap = wordWrap;
		if (verticalLayout) {
			useWordWrap = false;
			lineWidth = 0.0f;
		}
		defaultFont = Mathf.Clamp (defaultFont, 0, _fontInfo.Length-1);
		
		List<CommandData> commandData;
		int commandIndex = 0;
		List<char> chars = ParseString (s, out commandData);
		var glyphIndices = new List<int>(chars.Count);
		int totalVertCount = 0;
		int totalTriCount = 0;
		int totalTriCount2 = 0;
		bool extrude = (extrudeDepth > 0.0f);
		float spacePercent, spaceAdd;
		if (characterSpacing > 0.0f) {
			spacePercent = (characterSpacing < 1.0f)? characterSpacing : 1.0f;
			spaceAdd = (characterSpacing > 1.0f)? characterSpacing - 1.0f : 0.0f;
		}
		else {
			spacePercent = (characterSpacing > -1.0f)? -characterSpacing : 1.0f;
			spaceAdd = (characterSpacing < -1.0f)? characterSpacing + 1.0f : 0.0f;
		}
		TTFFontInfo thisFont = _fontInfo[defaultFont];
		int fontNumber = defaultFont;
		
		// Get total vertex and triangle count, initializing glyph data as necessary
		for (int i = 0; i < chars.Count; i++) {
			var thisCommand = commandData[commandIndex];
			while (thisCommand.index == i) {
				if (thisCommand.command == Command.Font) {
					int thisFontNumber = (int)thisCommand.data;
					if (thisFontNumber >= 0 && thisFontNumber < _fontInfo.Length) {
						fontNumber = thisFontNumber;
						thisFont = _fontInfo[fontNumber];
					}
				}
				thisCommand = commandData[++commandIndex];
			}
			
			if (thisFont == null) {
				Debug.LogError ("Font is null");
				return null;
			}
			
			// Set up glyph if it hasn't been previously initialized
			var character = chars[i];
			if (character == '\0') continue;
			
			if (!thisFont.glyphDictionary.ContainsKey (character)) {
				if (!thisFont.SetGlyphData (character)) return null;
			}
			
			var glyphData = thisFont.glyphDictionary[character];
			glyphIndices.Add (glyphData.glyphIndex);
			
			if (glyphData.isVisibleChar) {
				if (glyphData.resolution != resolution) {
					if (!glyphData.SetMeshData (resolution)) {
						Debug.LogWarning ("Triangulation failed for char code " + Convert.ToInt32 (character) + " (" + character + ")");
						continue;
					}
				}
				if (!glyphData.triDataComputed || glyphData.useSubmesh != separateEdge || glyphData.useBack != includeBackface) {
					if (!extrude) {
						glyphData.SetFrontTriData();
					}
					else {
						if (includeBackface) {
							glyphData.SetTriData (separateEdge);
						}
						else {
							glyphData.SetFrontAndEdgeTriData (separateEdge);
						}
					}
				}
				if (!separateObjects) {
					totalVertCount += glyphData.vertexCount;
					totalTriCount += glyphData.triCount;
					if (separateEdge) {
						totalTriCount2 += glyphData.triCount2;
					}
				}
			}
		}
		
		if (totalVertCount > 65534) {
			Debug.LogError ("Too many vertices...use fewer characters or reduce resolution");
			return null;
		}
		
		if (prime) return null;
		GameObject goParent = (separateObjects && !useObjectsArray && !updateObject)? new GameObject() : null;
		List<GameObject> objectList = useObjectsArray? new List<GameObject>() : null;
		
		// Use vertex colors if anything other than Color.white is specified anywhere, or if using the default material
		Color32 thisColor = defaultColor;
		bool useColors = false;		
		if (thisColor == Color.white) {
			for (int i = 0; i < commandData.Count; i++) {
				if (commandData[i].command == Command.Color) {
					if ((Color32)commandData[i].data != Color.white) {
						useColors = true;
						break;
					}
					
				}
			}
		}
		else {
			useColors = true;
		}
		
		var totalVerts = new Vector3[totalVertCount];
		var totalTris = new int[totalTriCount];
		var totalEdgeTris = new int[totalTriCount2];
		var meshUVs = new Vector2[totalVertCount];
		var meshColors = new Color32[totalVertCount];
		int vertIndex = 0;
		int triIndex = 0;
		int edgeTriIndex = 0;
		float baseScale = 1.0f / thisFont.unitsPerEm;
		bool uvsPerLetter = separateObjects? true : texturePerLetter;
		Vector3[] thisVerts;
		int[] thisTris;
		var kernPair = new KernPair();
		float smallestX = float.MaxValue;
		float largestX = -float.MaxValue;
		float smallestY = float.MaxValue;
		float largestY = -float.MaxValue;
		var lineLengths = new List<float>();
		var lineJustifies = new List<Justify>();
		var edgeIndices = new List<EdgeData>();
		int loopType = BUILDMESH;
		bool hasMultipleLines = false;
		int lineCount = 0;
		float longestLength = 0.0f;
		var thisJustify = verticalLayout? Justify.Left : defaultJustification;
			
		if (chars.Contains ('\n')) {
			hasMultipleLines = true;
			loopType = PREPROCESS;
		}
		if (lineWidth > 0.0f) {
			loopType = PREPROCESS;
		}
		
		while (loopType > 0) {
			float horizontalPosition = 0.0f;
			float previousPosition = 0.0f;
			float lastFullWordPosition = 0.0f;
			int spaceCharIndex = 0;
			float verticalPosition = 0.0f;
			float zPosition = 0.0f;
			float thisSize = size;
			float lastFullWordSize = size;
			char character = ' ';
			char previousCharacter = ' ';
			thisFont = _fontInfo[defaultFont];
			var lastFullWordFont = thisFont;
			commandIndex = 0;
			int lastFullWordCommandIndex = 0;
			if (loopType == BUILDMESH && hasMultipleLines) {
				thisJustify = lineJustifies[0];
			}
			var thisCommand = commandData[0];
			var lastFullWordCommand = thisCommand;
			var newLine = true;
			float maxCharWidth = 0.0f;
						
			for (int i = 0; i < chars.Count; i++) {
				while (thisCommand.index == i) {
					switch (thisCommand.command) {
						case Command.Size:
							thisSize = (float)thisCommand.data;
							if (thisSize < .001f) {
								thisSize = .001f;
							}
							break;
						case Command.Color:
							if (loopType == BUILDMESH) {
								thisColor = (Color32)thisCommand.data;
							}
							break;
						case Command.Font:
							fontNumber = (int)thisCommand.data;
							if (fontNumber >= 0 && fontNumber < _fontInfo.Length) {
								thisFont = _fontInfo[fontNumber];
								baseScale = 1.0f / thisFont.unitsPerEm;
							}
							break;
						case Command.Zpos:
							if (loopType == BUILDMESH) {
								zPosition = (float)thisCommand.data;
							}
							break;
						case Command.Depth:
							if (loopType == BUILDMESH) {
								extrudeDepth = (float)thisCommand.data;
								if (extrudeDepth < 0.0f) {
									extrudeDepth = 0.0f;
								}
							}
							break;
						case Command.Space:
							if (!verticalLayout) {
								previousPosition = horizontalPosition;
								horizontalPosition += (float)thisCommand.data * thisSize;
							}
							break;
						case Command.Justify:
							if (loopType == PREPROCESS && !verticalLayout) {
								thisJustify = ((Justify)thisCommand.data);
							}
							break;
					}
					thisCommand = commandData[++commandIndex];
				}
				
				float scaleFactor = baseScale * thisSize;
				previousCharacter = character;
				character = chars[i];
				if (character == '\0') {
					continue;
				}
				// Store current state in case we need to "back up" when word wrapping
				if (character == ' ') {
					if (!verticalLayout) {
						lastFullWordPosition = horizontalPosition;
						lastFullWordSize = thisSize;
						lastFullWordFont = thisFont;
						lastFullWordCommandIndex = commandIndex;
						lastFullWordCommand = thisCommand;
						spaceCharIndex = i;
					}
					else {
						verticalPosition -= thisFont.lineHeight * lineSpacing * scaleFactor;
					}
				}
				// Tab
				else if (character == '\t') {
					if (tabStop > 0.0f && !verticalLayout) {
						horizontalPosition = Mathf.Ceil (horizontalPosition / tabStop) * tabStop;
						continue;
					}
				}
				// End of line
				else if (character == '\n') {
					if (loopType == PREPROCESS) {
						lineLengths.Add (horizontalPosition);
						lineJustifies.Add (thisJustify);
						lastFullWordPosition = 0.0f;
					}
					else {
						if (++lineCount < lineJustifies.Count) {
							thisJustify = lineJustifies[lineCount];
						}
					}
					if (!verticalLayout) {
						verticalPosition -= thisFont.lineHeight * lineSpacing * scaleFactor;
						horizontalPosition = 0.0f;
					}
					else {
						verticalPosition = 0.0f;
						horizontalPosition += (maxCharWidth + spaceAdd / baseScale) * (scaleFactor * spacePercent);
						maxCharWidth = 0.0f;
					}
					newLine = true;
					continue;
				}
				
				int thisGlyphIdx = glyphIndices[i];
				if (!verticalLayout) {
					horizontalPosition -= thisFont.lsbArray[thisGlyphIdx] * scaleFactor;
				}
				
				// Kerning
				if (thisFont.hasKerning && !newLine && !verticalLayout) {
					kernPair.left = glyphIndices[i-1];
					kernPair.right = thisGlyphIdx;
					if (thisFont.kernDictionary.ContainsKey (kernPair)) {
						horizontalPosition += thisFont.kernDictionary[kernPair] * scaleFactor;
					}
				}
				
				var glyphData = thisFont.glyphDictionary[character];
				int vertexCount = glyphData.vertexCount;
				
				// Copy tris/verts to combined mesh
				if (vertexCount > 0 && loopType == BUILDMESH) {
					if (verticalLayout && !newLine) {
						verticalPosition -= (glyphData.yMax * scaleFactor) - (glyphData.yMin * scaleFactor) +
											(thisFont.lineHeight * (lineSpacing - 1.0f) * scaleFactor);
					}
				
					if (glyphData.scaleFactor != scaleFactor) {
						glyphData.ScaleVertices (scaleFactor, extrude, includeBackface);
					}
					
					if (extrude && glyphData.extrudeDepth != extrudeDepth) {
						glyphData.SetExtrudeDepth (extrudeDepth, includeBackface);
					}
					
					thisVerts = glyphData.vertices;
					
					if (separateObjects) {
						totalVerts = new Vector3[vertexCount];
						totalTris = new int[glyphData.triCount];
						if (separateEdge) {
							totalEdgeTris = new int[glyphData.triCount2];
							edgeTriIndex = 0;
						}
						meshUVs = new Vector2[vertexCount];
						if (useColors) {
							meshColors = new Color32[vertexCount];
						}
						vertIndex = 0;
						triIndex = 0;
					}
					
					// Get min/max bounds (for UVs if not per-letter, plus anchor position)
					float max = glyphData.xMax * scaleFactor + horizontalPosition;
					float min = glyphData.xMin * scaleFactor + horizontalPosition;
					if (max > largestX) {
						largestX = max;
					}
					if (min < smallestX) {
						smallestX = min;
					}
					max = thisFont.fontYMax * scaleFactor + verticalPosition;
					min = thisFont.fontYMin * scaleFactor + verticalPosition;
					if (max > largestY) {
						largestY = max;
					}
					if (min < smallestY) {
						smallestY = min;
					}
					if (verticalLayout) {
						float thisCharWidth = glyphData.xMax - glyphData.xMin;
						if (thisCharWidth > maxCharWidth) {
							maxCharWidth = thisCharWidth;
						}
					}
					
					if (uvsPerLetter) {
						float xMax = glyphData.xMax * scaleFactor;
						float xMin = glyphData.xMin * scaleFactor;
						float yMax = glyphData.yMax * scaleFactor;
						float yMin = glyphData.yMin * scaleFactor;
						float xRange = xMax - xMin;
						float yRange = yMax - yMin;
						if (!separateEdge) {
							for (int j = 0; j < vertexCount; j++) {
								meshUVs[j + vertIndex].x = (thisVerts[j].x - xMin) / xRange;
								meshUVs[j + vertIndex].y = (thisVerts[j].y - yMin) / yRange;
							}
						}
						else {
							int edgeIndex = glyphData.frontVertIndex * (includeBackface? 2 : 1);
							for (int j = 0; j < edgeIndex; j++) {
								meshUVs[j + vertIndex].x = (thisVerts[j].x - xMin) / xRange;
								meshUVs[j + vertIndex].y = (thisVerts[j].y - yMin) / yRange;
							}
							for (int j = edgeIndex; j < vertexCount; j += 2) {
								meshUVs[j + vertIndex    ].x = 0.0f;
								meshUVs[j + vertIndex    ].y = (thisVerts[j    ].y - yMin) / yRange;
								meshUVs[j + vertIndex + 1].x = 1.0f;
								meshUVs[j + vertIndex + 1].y = (thisVerts[j + 1].y - yMin) / yRange;
							}
						}
					}
					if (!uvsPerLetter && separateEdge) {
						edgeIndices.Add (new EdgeData(glyphData.frontVertIndex * (includeBackface? 2 : 1), vertexCount));
					}
					
					if (useColors) {
						for (int j = 0; j < vertexCount; j++) {
							meshColors[j + vertIndex] = thisColor;
						}
					}
					
					thisTris = glyphData.triangles;
					int triCount = glyphData.triCount;
					for (int j = 0; j < triCount; j += 3) {
						totalTris[triIndex  ] = thisTris[j  ] + vertIndex;
						totalTris[triIndex+1] = thisTris[j+1] + vertIndex;
						totalTris[triIndex+2] = thisTris[j+2] + vertIndex;
						triIndex += 3;
					}
					if (separateEdge) {
						thisTris = glyphData.triangles2;						
						triCount = glyphData.triCount2;
						for (int j = 0; j < triCount; j += 3) {
							totalEdgeTris[edgeTriIndex  ] = thisTris[j  ] + vertIndex;
							totalEdgeTris[edgeTriIndex+1] = thisTris[j+1] + vertIndex;
							totalEdgeTris[edgeTriIndex+2] = thisTris[j+2] + vertIndex;
							edgeTriIndex += 3;
						}						
					}
					
					// Set vertices with appropriate line justification
					if (thisJustify != Justify.Left) {
						float addSpace = 0.0f;
						if (thisJustify == Justify.Right) {
							if (lineLengths.Count > 0) {
								addSpace = longestLength - lineLengths[lineCount];
							}
						}
						else if (thisJustify == Justify.Center) {
							if (lineLengths.Count > 0) {
								addSpace = (longestLength - lineLengths[lineCount]) / 2;
							}
						}
						if (!separateObjects) {
							for (int j = 0; j < vertexCount; j++) {
								totalVerts[vertIndex  ].x = thisVerts[j].x + horizontalPosition + addSpace;
								totalVerts[vertIndex  ].y = thisVerts[j].y + verticalPosition;
								totalVerts[vertIndex++].z = thisVerts[j].z + zPosition;
							}
						}
						else {
							for (int j = 0; j < vertexCount; j++) {
								totalVerts[vertIndex  ].x = thisVerts[j].x + addSpace;
								totalVerts[vertIndex  ].y = thisVerts[j].y;
								totalVerts[vertIndex++].z = thisVerts[j].z + zPosition;
							}
						}
					}
					else {
						if (!separateObjects) {
							for (int j = 0; j < vertexCount; j++) {
								totalVerts[vertIndex  ].x = thisVerts[j].x + horizontalPosition;
								totalVerts[vertIndex  ].y = thisVerts[j].y + verticalPosition;
								totalVerts[vertIndex++].z = thisVerts[j].z + zPosition;
							}
						}
						else {
							for (int j = 0; j < vertexCount; j++) {
								totalVerts[vertIndex  ].x = thisVerts[j].x;
								totalVerts[vertIndex  ].y = thisVerts[j].y;
								totalVerts[vertIndex++].z = thisVerts[j].z + zPosition;
							}
						}
					}
					
					// Create mesh and game object for individual letters
					if (separateObjects) {
						var charMesh = new Mesh();
						charMesh.name = character.ToString();
						charMesh.vertices = totalVerts;
						charMesh.uv = meshUVs;
						if (useColors) {
							charMesh.colors32 = meshColors;
						}
						if (separateEdge) {
							charMesh.subMeshCount = 2;
							charMesh.SetTriangles (totalTris, 0);
							charMesh.SetTriangles (totalEdgeTris, 1);
						}
						else {
							charMesh.triangles = totalTris;
						}
						CalculateNormalsAndTangents (charMesh);
						
						var charGo = new GameObject(character.ToString(), typeof(MeshFilter), typeof(MeshRenderer));
						charGo.GetComponent<MeshFilter>().mesh = charMesh;
						if (colliderType == ColliderType.Mesh || colliderType == ColliderType.ConvexMesh) {
							var meshCollider = charGo.AddComponent<MeshCollider>();
							meshCollider.sharedMesh = charMesh;
							meshCollider.convex = (colliderType == ColliderType.ConvexMesh);
							meshCollider.sharedMaterial = physicsMaterial;
						}
						else if (colliderType == ColliderType.Box) {
							charGo.AddComponent<BoxCollider>().sharedMaterial = physicsMaterial;
						}
						if (addRigidbodies) {
							charGo.AddComponent (typeof(Rigidbody));
						}
						if (separateEdge) {
							charGo.GetComponent<Renderer>().sharedMaterials = materialsArray;
						}
						else {
							charGo.GetComponent<Renderer>().sharedMaterial = material;
						}
						if (!useObjectsArray) {
							charGo.transform.parent = goParent.transform;
							charGo.transform.position = new Vector3(horizontalPosition, verticalPosition, zPosition);
						}
						else {
							charGo.transform.rotation = rotation;
							charGo.transform.Translate (new Vector3(horizontalPosition, verticalPosition, zPosition) + position);
							objectList.Add (charGo);
						}
					}
				}
				previousPosition = horizontalPosition;
				if (!verticalLayout) {
					horizontalPosition += (thisFont.advanceArray[thisGlyphIdx] + spaceAdd / baseScale) * (scaleFactor * spacePercent);
				}
				
				// Line width / word wrap
				if (loopType == PREPROCESS && lineWidth > 0.0f && horizontalPosition > lineWidth && !newLine) {
					if (useWordWrap && lastFullWordPosition > 0.0f) {
						chars[spaceCharIndex] = '\n';
						i = spaceCharIndex-1;
						horizontalPosition = lastFullWordPosition;
						thisSize = lastFullWordSize;
						thisFont = lastFullWordFont;
						commandIndex = lastFullWordCommandIndex;
						thisCommand = lastFullWordCommand;
						lastFullWordPosition = 0.0f;
					}
					else {
						if (character == ' ') {
							chars[i] = '\n';
						}
						else {
							chars.Insert (i, '\n');
							glyphIndices.Insert (i, thisGlyphIdx);
							for (int j = commandIndex; j < commandData.Count; j++) {
								if (commandData[j].index != -1) {
									commandData[j].index++;
								}
							}
						}
						horizontalPosition = (previousCharacter == ' ')? lastFullWordPosition : previousPosition;
						i--;
					}
					hasMultipleLines = true;
				}
				
				newLine = false;
			}
			
			if (loopType-- == PREPROCESS) {
				lineLengths.Add (horizontalPosition);
				lineJustifies.Add (thisJustify);
				longestLength = lineLengths[0];
				for (int i = 1; i < lineLengths.Count; i++) {
					if (lineLengths[i] > longestLength) {
						longestLength = lineLengths[i];
					}
				}
				if (longestLength < lineWidth) {
					longestLength = lineWidth;
				}
			}
		}
		
		// Set UVs for complete mesh, if not per-letter
		if (!uvsPerLetter) {
			float xRange = largestX - smallestX;
			float yRange = largestY - smallestY;
			if (!separateEdge) {
				for (int i = 0; i < totalVertCount; i++) {
					meshUVs[i].x = (totalVerts[i].x - smallestX) / xRange;
					meshUVs[i].y = (totalVerts[i].y - smallestY) / yRange;
				}
			}
			else {
				vertIndex = 0;
				for (int i = 0; i < edgeIndices.Count; i++) {
					int edgeIndex = edgeIndices[i].frontVertIndex;
					for (int j = 0; j < edgeIndex; j++) {
						meshUVs[j + vertIndex].x = (totalVerts[j + vertIndex].x - smallestX) / xRange;
						meshUVs[j + vertIndex].y = (totalVerts[j + vertIndex].y - smallestY) / yRange;
					}
					int vertexCount = edgeIndices[i].vertexCount;
					for (int j = edgeIndex; j < vertexCount; j += 2) {
						meshUVs[j + vertIndex    ].x = 0.0f;
						meshUVs[j + vertIndex    ].y = (totalVerts[j + vertIndex    ].y - smallestY) / yRange;
						meshUVs[j + vertIndex + 1].x = 1.0f;
						meshUVs[j + vertIndex + 1].y = (totalVerts[j + vertIndex + 1].y - smallestY) / yRange;
					}
					vertIndex += vertexCount;
				}
			}
		}
		
		var addPos = Vector3.zero;
		switch (anchor) {
			case TextAnchor.UpperLeft:
				addPos.y = largestY;
				break;
			case TextAnchor.UpperCenter:
				addPos.x = (largestX - smallestX) * .5f;
				addPos.y = largestY;
				break;
			case TextAnchor.UpperRight:
				addPos.x = largestX - smallestX;
				addPos.y = largestY;
				break;
			case TextAnchor.MiddleLeft:
				addPos.y = (smallestY - largestY) * .5f + largestY;
				break;
			case TextAnchor.MiddleCenter:
				addPos.x = (largestX - smallestX) * .5f;
				addPos.y = (smallestY - largestY) * .5f + largestY;
				break;
			case TextAnchor.MiddleRight:
				addPos.x = largestX - smallestX;
				addPos.y = (smallestY - largestY) * .5f + largestY;
				break;
			case TextAnchor.LowerLeft:
				addPos.y = (smallestY - largestY) + largestY;
				break;
			case TextAnchor.LowerCenter:
				addPos.x = (largestX - smallestX) * .5f;
				addPos.y = (smallestY - largestY) + largestY;
				break;
			case TextAnchor.LowerRight:
				addPos.x = largestX - smallestX;
				addPos.y = (smallestY - largestY) + largestY;
				break;
		}
		if (extrude) {
			switch (zAnchor) {
				case ZAnchor.Middle:
					addPos.z = defaultDepth * .5f;
					break;
				case ZAnchor.Back:
					addPos.z = defaultDepth;
					break;
			}
		}
		if (!separateObjects) {
			for (int i = 0; i < totalVertCount; i++) {
				totalVerts[i] -= addPos;
			}
		}
		else {
			if (!useObjectsArray) {
				foreach (Transform go in goParent.transform) {
					go.position -= addPos;
				}
			}
			else {
				for (int i = 0; i < objectList.Count; i++) {
					objectList[i].transform.Translate (-addPos);
				}
			}
		}
		
		// Get gameobject name from string, and create mesh and game object if not separate
		var charString = new string(chars.ToArray());
		var name = charString.Substring(0, Mathf.Min(20, charString.Length));
		name = name.Replace ("\n", " ");
		name = name.Replace ("\0", "");
		
		if (separateObjects) {
			if (!useObjectsArray) {
				goParent.name = "3DText " + name;
				goParent.transform.position = position;
				goParent.transform.rotation = rotation;
				return goParent;
			}
			objectArray = objectList.ToArray();
			return null;
		}
		
		if (!updateObject) {
			mesh = new Mesh();
			mesh.name = name;
		}
		else {
			mesh.Clear();
		}
		
		mesh.vertices = totalVerts;
		mesh.uv = meshUVs;
		if (useColors) {
			mesh.colors32 = meshColors;
		}
		else {
			int vertexCount = totalVerts.Length;
			meshColors = new Color32[vertexCount];
			Color32 whiteColor = Color.white;
			for (int i = 0; i < vertexCount; i++) {
				meshColors[i] = whiteColor;
			}
			mesh.colors32 = meshColors;
		}
		if (separateEdge) {
			mesh.subMeshCount = 2;
			mesh.SetTriangles (totalTris, 0);
			mesh.SetTriangles (totalEdgeTris, 1);
		}
		else {
			mesh.triangles = totalTris;
		}
		
		CalculateNormalsAndTangents (mesh);
		if (updateObject) {
			mesh.RecalculateBounds();
			var meshCollider = gObject.GetComponent<MeshCollider>();
			if (meshCollider != null) {
				meshCollider.sharedMesh = mesh;
			}
			return null;
		}
		
		var textGo = new GameObject("3DText " + name, typeof(MeshFilter), typeof(MeshRenderer));
		textGo.GetComponent<MeshFilter>().mesh = mesh;
		if (separateEdge) {
			textGo.GetComponent<Renderer>().sharedMaterials = materialsArray;
		}
		else {
			textGo.GetComponent<Renderer>().sharedMaterial = material;
		}
		if (colliderType == ColliderType.Mesh || colliderType == ColliderType.ConvexMesh) {
			var meshCollider = textGo.AddComponent<MeshCollider>();
			meshCollider.sharedMesh = mesh;
			meshCollider.convex = (colliderType == ColliderType.ConvexMesh);
			meshCollider.sharedMaterial = physicsMaterial;
		}
		else if (colliderType == ColliderType.Box) {
			textGo.AddComponent<BoxCollider>().sharedMaterial = physicsMaterial;
		}
		if (addRigidbodies) {
			textGo.AddComponent<Rigidbody>();
		}
		
		textGo.transform.position = position;
		textGo.transform.rotation = rotation;
		textGo.AddComponent<TextObjectData>().SetData (size, extrudeDepth, resolution, characterSpacing, lineSpacing, lineWidth);		
		return textGo;
	}
	
	private static void CalculateNormalsAndTangents (Mesh mesh) {
		mesh.RecalculateNormals();
		if (computeTangents) {
			ComputeTangents (mesh);
		}
	}
	
	private static void ComputeTangents (Mesh mesh) {
		var vertices = mesh.vertices;
		var uvs = mesh.uv;
		var triangles = mesh.triangles;
		int vertexCount = vertices.Length;
		int triCount = triangles.Length;
		var tan1 = new Vector3[vertexCount];
		var tan2 = new Vector3[vertexCount];
		
		for (int i = 0; i < triCount; i += 3) {
			int i1 = triangles[i];
			int i2 = triangles[i+1];
			int i3 = triangles[i+2];
			
			Vector3 v1 = vertices[i1];
			Vector3 v2 = vertices[i2];
			Vector3 v3 = vertices[i3];
			
			Vector2 w1 = uvs[i1];
			Vector2 w2 = uvs[i2];
			Vector2 w3 = uvs[i3];
			
			float x1 = v2.x - v1.x;
			float x2 = v3.x - v1.x;
			float y1 = v2.y - v1.y;
			float y2 = v3.y - v1.y;
			float z1 = v2.z - v1.z;
			float z2 = v3.z - v1.z;
			
			float s1 = w2.x - w1.x;
			float s2 = w3.x - w1.x;
			float t1 = w2.y - w1.y;
			float t2 = w3.y - w1.y;
			
			float r = 1.0f / (s1 * t2 - s2 * t1);
			Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
			
			tan1[i1] += sdir;
			tan1[i2] += sdir;
			tan1[i3] += sdir;
			
			tan2[i1] += tdir;
			tan2[i2] += tdir;
			tan2[i3] += tdir;
		}
		
		Vector3[] normals = mesh.normals;
		Vector4[] tangents = new Vector4[vertexCount];
		for (int i = 0; i < vertexCount; i++) {
			Vector3 n = normals[i];
			Vector3 t = tan1[i];
			tangents[i] = (t - n * Vector3.Dot(n, t)).normalized;
			tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
		}
		
		mesh.tangents = tangents;
	}
	
	private static List<char> ParseString (string s, out List<CommandData> commandData) {
		commandData = new List<CommandData>();
		
		s = s.Replace ("\0", "");
		s = s.Replace ("<<", "\01");
		s = s.Replace (">>", "\02");
		s = s.Replace ("<br>", "\n");
		s = s.Replace ("<BR>", "\n");
		
		int startIndex = 0;
		int i = s.IndexOf ("<", startIndex);
		while (i != -1) {
			int j = s.IndexOf (">", startIndex);
			if (j == -1 || j < i) break;
			
			string tagData = s.Substring (i+1, j-i-1);
			s = s.Remove (i, j-i+1);
			startIndex = i;
			string tag, data;
			if (GetTagData (ref tagData, out tag, out data)) {
				tag = tag.ToLower();
				switch (tag) {
					case "size":
						float thisVal;
						if (Single.TryParse (data, out thisVal)) {
							commandData.Add (new CommandData(i, Command.Size, thisVal));
						}
						break;
					case "color":
						Color32 thisColor;
						if (TryParseColor (ref data, out thisColor)) {
							commandData.Add (new CommandData(i, Command.Color, thisColor));
						}
						else {
							data = data.ToLower();
							if (_colorDictionary.ContainsKey (data)) {
								commandData.Add (new CommandData(i, Command.Color, _colorDictionary[data]));
							}
						}
						break;
					case "font":
						int fontNumber;
						if (Int32.TryParse (data, out fontNumber)) {
							commandData.Add (new CommandData(i, Command.Font, fontNumber));
						}
						else {
							data = data.ToLower();
							for (int k = 0; k < _fontNames.Length; k++) {
								if (data == _fontNames[k]) {
									commandData.Add (new CommandData(i, Command.Font, k));
									break;
								}
							}
						}
						break;
					case "zpos":
						if (Single.TryParse (data, out thisVal)) {
							commandData.Add (new CommandData(i, Command.Zpos, thisVal));
						}
						break;
					case "depth":
						if (Single.TryParse (data, out thisVal)) {
							commandData.Add (new CommandData(i, Command.Depth, thisVal));
						}
						break;
					case "space":
						if (Single.TryParse (data, out thisVal)) {
							commandData.Add (new CommandData(i, Command.Space, thisVal));
						}
						break;
					case "justify":
						if (data == "left") {
							commandData.Add (new CommandData(i, Command.Justify, Justify.Left));
						}
						else if (data == "right") {
							commandData.Add (new CommandData(i, Command.Justify, Justify.Right));
						}
						else if (data == "center" || data == "centre") {
							commandData.Add (new CommandData(i, Command.Justify, Justify.Center));
						}
						break;
					default:
						Debug.LogWarning ("Unknown tag: " + tag);
						break;
				}
			}
			
			i = s.IndexOf ("<", startIndex);
		}
		
		commandData.Add (new CommandData(-1, Command.None, null));
		s = s.Replace ("\01", "\0<");
		s = s.Replace ("\02", "\0>");

		var charList = new List<char>(s.ToCharArray());
		charList.Add ('\0');
		return charList;
	}
	
	private static bool GetTagData (ref string s, out string tag, out string data) {
		if (s.IndexOfAny (_removeChars) != -1) {
			s = string.Join("", s.Split(_removeChars)); // Prefer not to use Regex for just this one thing, given how much size it adds
		}
		var strings = s.Split('=');
		if (strings.Length != 2) {
			tag = ""; data = "";
			return false;
		}
		tag = strings[0];
		data = strings[1];
		return true;
	}

	private static bool TryParseColor (ref string s, out Color32 color) {
		color = Color.white;
		if (s.Length != 7 || !s.StartsWith("#")) {
			return false;
		}
		int value;
		if (Int32.TryParse (s.Substring(1, 6), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)) {
			color = new Color32((byte)(value >> 16), (byte)((value >> 8) & 255), (byte)(value & 255), (byte)255);
			return true;
		}
		return false;
	}
}