// FlyingText3D 2.2
// ©2015 Starscene Software. All rights reserved. Redistribution without permission not allowed.

using UnityEngine;

namespace FlyingText3D {

public enum Command {None, Size, Color, Font, Zpos, Depth, Space, Justify}

public enum ColliderType {None, Box, ConvexMesh, Mesh}

public enum ZAnchor {Front, Middle, Back}

public enum Justify {Left, Center, Right}

public class CommandData {
	public int index;
	public Command command;
	public object data;
	
	public CommandData (int index, Command command, object data) {
		this.index = index;
		this.command = command;
		this.data = data;
	}
}

public struct KernPair {
	public int left;
	public int right;
	
	public KernPair (int left, int right) {
		this.left = left;
		this.right = right;
	}
}

[System.Serializable]
public class FontData {
	public TextAsset ttfFile;
	public string fontName;
	
	public FontData () {
		ttfFile = null;
		fontName = "";
	}
}

}