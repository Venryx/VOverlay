using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine.UI;
using VTree;
using Object = UnityEngine.Object;

public static class EmojiAdder_ClassExtensions {
	public static void AddEmojiToText(this Text s) { EmojiAdder.AddEmojiToText(s); }
}

struct PosStringTuple {
	public PosStringTuple(int pos, string emojiStr) {
		this.pos = pos;
		this.emojiStr = emojiStr;
	}
	public int pos;
	public string emojiStr;
}

public static class EmojiAdder {
	static EmojiAdder() { emojiRects = GetEmojiRects(VO.main.emojiAdderScript.emojiInfo.text); }
	static Dictionary<string, Rect> emojiRects;
	static Dictionary<string, Rect> GetEmojiRects(string emojiInfoStr) {
		var result = new Dictionary<string, Rect>();
		using (StringReader reader = new StringReader(emojiInfoStr)) {
			string line = reader.ReadLine();
			while (line != null && line.Length > 1) {
				// We add each emoji to emojiRects
				string[] split = line.Split(' ');
				float x = float.Parse(split[1], System.Globalization.CultureInfo.InvariantCulture);
				float y = float.Parse(split[2], System.Globalization.CultureInfo.InvariantCulture);
				float width = float.Parse(split[3], System.Globalization.CultureInfo.InvariantCulture);
				float height = float.Parse(split[4], System.Globalization.CultureInfo.InvariantCulture);
				result[GetConvertedString(split[0])] = new Rect(x, y, width, height);

				line = reader.ReadLine();
			}
		}
		return result;
	}
	static string GetConvertedString(string inputString) {
		string[] converted = inputString.Split('-');
		for (int j = 0; j < converted.Length; j++)
			converted[j] = char.ConvertFromUtf32(Convert.ToInt32(converted[j], 16));
		return string.Join(string.Empty, converted);
	}
	public static Rect? GetUVRectForEmojiChar(string emojiChar) { return emojiRects.GetValueOrX(emojiChar, null); }

	static char emSpaceChar = '\u2001';
	public static void AddEmojiToText(Text textComp) {
		var text = textComp.text;

		int i = 0;
		var emojiReplacements = new List<PosStringTuple>();
		var sb = new StringBuilder();
		while (i < text.Length) {
			string singleChar = text.Substring(i, 1);
			string doubleChar = "";
			string fourChar = "";
			if (i < (text.Length - 1))
				doubleChar = text.Substring(i, 2);
			if (i < (text.Length - 3))
				fourChar = text.Substring(i, 4);

			// check 64 bit emojis first
			if (emojiRects.ContainsKey(fourChar)) {
				sb.Append(emSpaceChar);
				emojiReplacements.Add(new PosStringTuple(sb.Length - 1, fourChar));
				i += 4;
			}
			// then check 32 bit emojis
			else if (emojiRects.ContainsKey(doubleChar)) {
				sb.Append(emSpaceChar);
				emojiReplacements.Add(new PosStringTuple(sb.Length - 1, doubleChar));
				i += 2;
			}
			// finally check 16 bit emojis
			else if (emojiRects.ContainsKey(singleChar)) {
				sb.Append(emSpaceChar);
				emojiReplacements.Add(new PosStringTuple(sb.Length - 1, singleChar));
				i++;
			}
			else {
				sb.Append(text[i]);
				i++;
			}
		}

		// set text
		textComp.text = sb.ToString();

		Debug.Log("SetText:" + textComp.text + ";" + sb);

		VO.main.WaitXFramesThenCall(5, ()=> {
			// And spawn RawImages as emojis
			TextGenerator textGen = textComp.cachedTextGenerator;

			// One rawimage per emoji
			for (int j = 0; j < emojiReplacements.Count; j++) {
				int emojiIndex = emojiReplacements[j].pos;
				GameObject newRawImage = Object.Instantiate(VO.main.emojiAdderScript.emojiFont_atlas.gameObject);
				newRawImage.transform.SetParent(textComp.transform);
				Vector3 imagePos = new Vector3(textGen.verts[emojiIndex * 4].position.x, textGen.verts[emojiIndex * 4].position.y, 0);
				newRawImage.transform.localPosition = imagePos;

				RawImage ri = newRawImage.GetComponent<RawImage>();
				//ri.uvRect = emojiRects[emojiReplacements[j].emojiStr];
				ri.uvRect = GetUVRectForEmojiChar(emojiReplacements[j].emojiStr).Value;
			}
		});
	}
}