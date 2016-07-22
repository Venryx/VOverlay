using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using ProtoBuf;
using VDFN;
using VectorStructExtensions;

// maybe todo: make custom versions of the Vector4 and Quaternion classes as well

[ProtoContract] public struct Vector2i
{
	public static readonly Vector2i zero = new Vector2i(0, 0);
	public static readonly Vector2i one = new Vector2i(1, 1);

	// VDF
	// ==========

	[VDFSerialize] VDFNode Serialize() { return x + " " + y; }
	[VDFDeserialize] void Deserialize(VDFNode node)
	{
		var parts = node.primitiveValue.ToString().Split(new[] {' '});
		x = int.Parse(parts[0]);
		y = int.Parse(parts[1]);
	}

	// operators and overrides
	// ==========

	/*int ShiftAndWrap(int value, int positions)
	{
		positions = positions & 0x1F;
		uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0); // save the existing bit pattern, but interpret it as an unsigned integer
		uint wrapped = number >> (32 - positions); // preserve the bits to be discarded
		return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0); // shift and wrap the discarded bits
	}
	public override int GetHashCode()
	{
		return ((1789 + x.GetHashCode()) * 1789) + y.GetHashCode();
		//return ShiftAndWrap(x.GetHashCode(), 2) ^ y.GetHashCode();
	}*/
	//public override int GetHashCode() { return ToString().GetHashCode(); } // self/forum
	//public override int GetHashCode() { return (x << 16) ^ y; } // self/forum
	//public override int GetHashCode() { return x.GetHashCode() ^ y.GetHashCode()<<2 /*^ z.GetHashCode()>>2*/; } // Unity
	//public override int GetHashCode() { return (x * 397) ^ y; } // Resharper
	//public override int GetHashCode() { return (17*23 + x.GetHashCode())*23 + y.GetHashCode(); } // book/forum
	public override int GetHashCode() { return (17*23 + x)*23 + y; }
	public override bool Equals(object other)
	{
		if (!(other is Vector2i))
			return false;
		return this == (Vector2i)other;
	}
	public override string ToString() { return x + " " + y; }

	public static bool operator ==(Vector2i s, Vector2i b) { return s.x == b.x && s.y == b.y; }
	public static bool operator !=(Vector2i s, Vector2i b) { return !(s.x == b.x && s.y == b.y); }
	public static Vector2i operator -(Vector2i s, Vector2i b) { return new Vector2i(s.x - b.x, s.y - b.y); }
	public static Vector2i operator -(Vector2i s) { return new Vector2i(-s.x, -s.y); }
	public static Vector2i operator +(Vector2i s, Vector2i b) { return new Vector2i(s.x + b.x, s.y + b.y); }
	public static Vector2i operator *(Vector2i s, int amount) { return new Vector2i(amount * s.x, amount * s.y); }
	public static Vector2i operator *(int amount, Vector2i s) { return new Vector2i(amount * s.x, amount * s.y); }
	public static Vector2i operator /(Vector2i s, int amount) { return new Vector2i(s.x / amount, s.y / amount); }

	// export
	public VVector2 ToVVector2() { return this; }
	public Vector2 ToVector2(bool fromYForwardToUp = true)
	{
		if (fromYForwardToUp)
			return new Vector2(x, 0);
		return new Vector2(x, y);
	}
	public Vector3i ToVector3i() { return new Vector3i(x, y, 0); }

	// general
	// ==========

	public static Vector2i Null { get { return new Vector2i(V.Int_FakeNaN, V.Int_FakeNaN); } }

	//public Vector2i(Vector2 obj) : this(obj.x, obj.y) {}
	public Vector2i(double x, double y) : this((int)Math.Floor(x), (int)Math.Floor(y)) { }
	public Vector2i(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	[ProtoMember(1, IsRequired = true)] public int x;
	[ProtoMember(2, IsRequired = true)] public int y;

	public double Distance(Vector2i other) { return (other - this).magnitude; }
	public double DistanceSquared(Vector2i other) { return (other - this).magnitudeSquared; }

	public double magnitude { get { return Math.Sqrt((x * x) + (y * y)); } }
	public double magnitudeSquared { get { return (x * x) + (y * y); } }

	public Vector2i NewX(int val) { return new Vector2i(val, y); }
	public Vector2i NewY(int val) { return new Vector2i(x, val); }

	public Vector2i FloorToMultipleOf(int val) { return new Vector2i(x.FloorToMultipleOf(val), y.FloorToMultipleOf(val)); }
	public Vector2i RoundToMultipleOf(int val) { return new Vector2i(x.RoundToMultipleOf(val), y.RoundToMultipleOf(val)); }

	//public Vector2i AsPositive() { return new Vector2i(x >= 0 ? x : -x, y >= 0 ? y : -1); }
}

[ProtoContract] public struct VVector2
{
	public static readonly VVector2 zero = new VVector2(0, 0);
	public static readonly VVector2 one = new VVector2(1, 1);

	// VDF
	// ==========

	[VDFSerialize] VDFNode Serialize() { return x + " " + y; }
	[VDFDeserialize] void Deserialize(VDFNode node)
	{
		var parts = node.primitiveValue.ToString().Split(new[] {' '});
		x = double.Parse(parts[0]);
		y = double.Parse(parts[1]);
	}

	// operators and overrides
	// ==========

	//public override int GetHashCode() { return ToString().GetHashCode(); }
	//public override int GetHashCode() { return (x << 16) | y; }
	public override int GetHashCode() { return (17*23 + x.GetHashCode())*23 + y.GetHashCode(); }
	public override bool Equals(object other)
	{
		if (!(other is VVector2))
			return false;
		return this == (VVector2)other;
	}
	public override string ToString() { return x + " " + y; }

	// import
	public static implicit operator VVector2(Vector2i obj) { return new VVector2(obj.x, obj.y); }

	// export
	public Vector2i ToVector2i(bool round = false)
	{
		if (round)
			return new Vector2i((int)Math.Round(x), (int)Math.Round(y));
		return new Vector2i(x, y);
	}
	public Vector2 ToVector2(bool fromYForwardToUp = true)
	{
		if (fromYForwardToUp)
			return new Vector2((float)x, 0);
		return new Vector2((float)x, (float)y);
	}
	public VVector3 ToVVector3() { return new VVector3(x, y, 0); }
	public VVector4 ToVVector4() { return new VVector4(x, y, 0, 0); }

	public static bool operator ==(VVector2 s, VVector2 b)
	{
		if (double.IsNaN(s.x)) // only check x
			return double.IsNaN(b.x);
		return s.x == b.x && s.y == b.y;
	}
	public static bool operator !=(VVector2 s, VVector2 b)
	{
		if (double.IsNaN(s.x)) // only check x
			return !double.IsNaN(b.x);
		return !(s.x == b.x && s.y == b.y);
	}
	public static VVector2 operator -(VVector2 s, VVector2 b) { return new VVector2(s.x - b.x, s.y - b.y); }
	public static VVector2 operator -(VVector2 s) { return new VVector2(-s.x, -s.y); }
	public static VVector2 operator +(VVector2 s, VVector2 b) { return new VVector2(s.x + b.x, s.y + b.y); }
	public static VVector2 operator *(VVector2 s, double amount) { return new VVector2(amount * s.x, amount * s.y); }
	public static VVector2 operator *(double amount, VVector2 s) { return new VVector2(amount * s.x, amount * s.y); }
	public static VVector2 operator /(VVector2 s, double amount) { return new VVector2(s.x / amount, s.y / amount); }

	// others
	public bool EqualsAbout(VVector2 other, float maxDifForEquals = .000001f)
		{ return Math.Abs(x - other.x) <= maxDifForEquals && Math.Abs(y - other.y) <= maxDifForEquals; }

	// general
	// ==========

	public static VVector2 Null { get { return new VVector2(double.NaN, double.NaN); } }

	//public VVector2(Vector2 obj) : this(obj.x, obj.y) {}
	public VVector2(double x, double y)
	{
		this.x = x;
		this.y = y;
	}

	[ProtoMember(1, IsRequired = true)] public double x;
	[ProtoMember(2, IsRequired = true)] public double y;

	public double Distance(VVector2 other) { return (other - this).magnitude; }
	public double DistanceSquared(VVector2 other) { return (other - this).magnitudeSquared; }
	public bool DistanceToXAtLeastY(VVector2 posX, double minDistance) { return DistanceSquared(posX) >= minDistance.ToPower(2); }
	public bool DistanceToXAtMostY(VVector2 posX, double maxDistance) { return DistanceSquared(posX) <= maxDistance.ToPower(2); }

	public double magnitude { get { return Math.Sqrt((x * x) + (y * y)); } }
	public double magnitudeSquared { get { return (x * x) + (y * y); } }

	public VVector2 normalized { get { return ToVector2(false).normalized.ToVVector2(false); } }

	public VVector2 NewX(double val) { return new VVector2(val, y); }
	public VVector2 NewY(double val) { return new VVector2(x, val); }

	// for size vectors
	// maybe todo: change arg to work as amount-on-each-side
	public VVector2 Grow(double amountOnEachAxis) { return this + new VVector2(amountOnEachAxis, amountOnEachAxis); }
	public VVector2 Shrink(double amountOnEachAxis) { return this + new VVector2(-amountOnEachAxis, -amountOnEachAxis); }

	public VVector2 ExtendAwayFrom(VVector2 originPoint, double distance)
	{
		var extensionDirection = (this - originPoint).normalized;
		return this + (extensionDirection * distance);
	}

	public VVector2 FloorToMultipleOf(double val) { return new VVector2(x.FloorToMultipleOf(val), y.FloorToMultipleOf(val)); }
	public VVector2 RoundToMultipleOf(double val) { return new VVector2(x.RoundToMultipleOf(val), y.RoundToMultipleOf(val)); }
	public VVector2 CeilingToMultipleOf(double val) { return new VVector2(x.CeilingToMultipleOf(val), y.CeilingToMultipleOf(val)); }
}

[ProtoContract] public struct Vector3i
{
	// special (for Chunk class)
	// ==========
	// ==========

	public static readonly Vector3i forward = new Vector3i(0, 0, 1);
	public static readonly Vector3i back = new Vector3i(0, 0, -1);
	public static readonly Vector3i up = new Vector3i(0, 1, 0);
	public static readonly Vector3i down = new Vector3i(0, -1, 0);
	public static readonly Vector3i left = new Vector3i(-1, 0, 0);
	public static readonly Vector3i right = new Vector3i(1, 0, 0);
	public static readonly Vector3i forward_right = new Vector3i(1, 0, 1);
	public static readonly Vector3i forward_left = new Vector3i(-1, 0, 1);
	public static readonly Vector3i forward_up = new Vector3i(0, 1, 1);
	public static readonly Vector3i forward_down = new Vector3i(0, -1, 1);
	public static readonly Vector3i back_right = new Vector3i(1, 0, -1);
	public static readonly Vector3i back_left = new Vector3i(-1, 0, -1);
	public static readonly Vector3i back_up = new Vector3i(0, 1, -1);
	public static readonly Vector3i back_down = new Vector3i(0, -1, -1);
	public static readonly Vector3i up_right = new Vector3i(1, 1, 0);
	public static readonly Vector3i up_left = new Vector3i(-1, 1, 0);
	public static readonly Vector3i down_right = new Vector3i(1, -1, 0);
	public static readonly Vector3i down_left = new Vector3i(-1, -1, 0);
	public static readonly Vector3i forward_right_up = new Vector3i(1, 1, 1);
	public static readonly Vector3i forward_right_down = new Vector3i(1, -1, 1);
	public static readonly Vector3i forward_left_up = new Vector3i(-1, 1, 1);
	public static readonly Vector3i forward_left_down = new Vector3i(-1, -1, 1);
	public static readonly Vector3i back_right_up = new Vector3i(1, 1, -1);
	public static readonly Vector3i back_right_down = new Vector3i(1, -1, -1);
	public static readonly Vector3i back_left_up = new Vector3i(-1, 1, -1);
	public static readonly Vector3i back_left_down = new Vector3i(-1, -1, -1);
	public static readonly Vector3i[] directions =
	{
		left, right,
		back, forward,
		down, up,
	};
	public static readonly Vector3i[] allDirections =
	{
		left,
		right,
		back,
		forward,
		down,
		up,
		forward_right,
		forward_left,
		forward_up,
		forward_down,
		back_right,
		back_left,
		back_up,
		back_down,
		up_right,
		up_left,
		down_right,
		down_left,
		forward_right_up,
		forward_right_down,
		forward_left_up,
		forward_left_down,
		back_right_up,
		back_right_down,
		back_left_up,
		back_left_down,
	};

	// normal
	// ==========
	// ==========

	public static Vector3i Null { get { return new Vector3i(V.Int_FakeNaN, V.Int_FakeNaN, V.Int_FakeNaN); } }

	public static Vector3i zero { get { return new Vector3i(0, 0, 0); } }
	public static Vector3i one { get { return new Vector3i(1, 1, 1); } }
	public static Vector3i two { get { return new Vector3i(2, 2, 2); } }

	// VDF
	// ==========

	[VDFSerialize] VDFNode Serialize() { return x + " " + y + " " + z; }
	[VDFDeserialize] void Deserialize(VDFNode node)
	{
		var parts = node.primitiveValue.ToString().Split(new[] {' '});
		x = int.Parse(parts[0]);
		y = int.Parse(parts[1]);
		z = int.Parse(parts[2]);
	}

	// operators and overrides
	// ==========

	//public override int GetHashCode() { return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2; } // this gave overlap of keys, which caused actual problems
	//public override int GetHashCode() { return ToString().GetHashCode(); }
	//public override int GetHashCode() { return (x << 20) | (y << 10) | z; }
	public override int GetHashCode() { return ((17*23 + x)*23 + y)*23 + z; }
	public override bool Equals(object other)
	{
		if (!(other is Vector3i))
			return false;
		return this == (Vector3i)other;
	}
	public override string ToString() { return x + " " + y + " " + z; }

	// import
	//public static implicit operator Vector3i(Vector2i obj) { return new Vector3i(obj.x, obj.y, 0); }

	// export
	public Vector2i ToVector2i() { return new Vector2i(x, y); }
	public VVector3 ToVVector3() { return this; }
	public Vector3 ToVector3(bool fromYForwardToUp = true)
	{
		if (fromYForwardToUp)
			return new Vector3(x, z, y);
		return new Vector3(x, y, z);
	}

	public static bool operator ==(Vector3i a, Vector3i b) { return a.x == b.x && a.y == b.y && a.z == b.z; }
	public static bool operator !=(Vector3i a, Vector3i b) { return !(a.x == b.x && a.y == b.y && a.z == b.z); }
	public static Vector3i operator -(Vector3i a) { return new Vector3i(-a.x, -a.y, -a.z); }
	public static Vector3i operator -(Vector3i a, Vector3i b) { return new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z); }
	public static Vector3i operator +(Vector3i a, Vector3i b) { return new Vector3i(a.x + b.x, a.y + b.y, a.z + b.z); }
	public static Vector3i operator *(Vector3i s, int amount) { return new Vector3i(amount * s.x, amount * s.y, amount * s.z); }
	public static VVector3 operator *(Vector3i s, double amount) { return new VVector3(amount * s.x, amount * s.y, amount * s.z); }
	public static Vector3i operator *(int amount, Vector3i s) { return new Vector3i(amount * s.x, amount * s.y, amount * s.z); }
	public static VVector3 operator *(double amount, Vector3i s) { return new VVector3(amount * s.x, amount * s.y, amount * s.z); }
	public static Vector3i operator /(Vector3i s, int b) { return new Vector3i(s.x / b, s.y / b, s.z / b); }
	public static Vector3i operator /(Vector3i s, double b) { return new Vector3i(s.x / b, s.y / b, s.z / b); }
	public static Vector3i operator %(Vector3i s, double b) { return new Vector3i(s.x % b, s.y % b, s.z % b); }

	// general
	// ==========

	public static Vector3i Parse(string str)
	{
		var parts = str.Split(new[] {' '});
		return new Vector3i(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
	}
	public static Vector3i Floor(Vector3 vec) { return new Vector3i(Mathf.FloorToInt(vec.x), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z)); }

	public static Vector3i Min(Vector3i min, Vector3i value) { return new Vector3i(Mathf.Min(value.x, min.x), Mathf.Min(value.y, min.y), Mathf.Min(value.z, min.z)); }
	public static Vector3i Max(Vector3i max, Vector3i value) { return new Vector3i(Mathf.Max(value.x, max.x), Mathf.Max(value.y, max.y), Mathf.Max(value.z, max.z)); }
	public static Vector3i Clamp(Vector3i min, Vector3i max, Vector3i value) { return new Vector3i(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y), Mathf.Clamp(value.z, min.z, max.z)); }

	//public Vector3i(Vector3 v) : this(v.x, v.y, v.z) {}
	public Vector3i(double x, double y, double z) : this((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z)) {}
	public Vector3i(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	[ProtoMember(1, IsRequired = true)] public int x;
	[ProtoMember(2, IsRequired = true)] public int y;
	[ProtoMember(3, IsRequired = true)] public int z;

	//public int DistanceSquared(Vector3i v) { return DistanceSquared(this, v); }
	/*public int DistanceSquared(Vector3i b)
	{
		int dx = b.x - x;
		int dy = b.y - y;
		int dz = b.z - z;
		return dx * dx + dy * dy + dz * dz;
	}
	public static float Distance(Vector3i a, Vector3i b)
	{
		int dx = b.x - a.x;
		int dy = b.y - a.y;
		int dz = b.z - a.z;
		return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
	}*/
	public double DistanceSquared(Vector3i other) { return (other - this).magnitudeSquared; }
	public double Distance(Vector3i other) { return (other - this).magnitude; }

	public double magnitude { get { return Math.Sqrt((x * x) + (y * y) + (z * z)); ; } }
	public double magnitudeSquared { get { return (x * x) + (y * y) + (z * z); } }

	public Vector3i NewX(int val) { return new Vector3i(val, y, z); }
	public Vector3i NewY(int val) { return new Vector3i(x, val, z); }
	public Vector3i NewZ(int val) { return new Vector3i(x, y, val); }
	public Vector3i SwappedYZ() { return new Vector3i(x, z, y); }
	public void SwapYZ()
	{
		var temp = y;
		y = z;
		z = temp;
	}

	// apparently, due to a Mono compiler issue, we need to have different return types for the Vector3i variants, so that the Vector3<>Vector3i method calls are not 'ambiguous'
	public VVector3_WorldWrapper AsWorldPos() { return new VVector3_WorldWrapper(this); }
}

[ProtoContract, Serializable] public struct VVector3 {
	public static readonly VVector3 zero = new VVector3(0, 0, 0);
	public static readonly VVector3 one = new VVector3(1, 1, 1);

	// VDF
	// ==========

	[VDFSerialize] VDFNode Serialize() { return x + " " + y + " " + z; }
	[VDFDeserialize] void Deserialize(VDFNode node) {
		var parts = node.primitiveValue.ToString().Split(new[] {' '});
		x = double.Parse(parts[0]);
		y = double.Parse(parts[1]);
		z = double.Parse(parts[2]);
	}

	// operators and overrides
	// ==========

	//public override int GetHashCode() { return ToString().GetHashCode(); }
	//public override int GetHashCode() { return (x << 20) | (x << 10) | y; }
	//public override int GetHashCode() { return (x << 20) ^ (x << 10) ^ y; }
	public override int GetHashCode() { return ((17*23 + x.GetHashCode())*23 + y.GetHashCode())*23 + z.GetHashCode(); }
	public override bool Equals(object other)
	{
		if (!(other is VVector3))
			return false;
		return this == (VVector3)other;
	}
	public override string ToString() { return x + " " + y + " " + z; }

	// import
	//public static implicit operator VVector3(VVector2 obj) { return new VVector3(obj.x, obj.y, 0); }
	public static implicit operator VVector3(Vector3i obj) { return new VVector3(obj.x, obj.y, obj.z); }

	// export
	public VVector2 ToVVector2() { return new VVector2(x, y); }
	// make-so: the other ones like this are added
	public Vector2 ToVector2(bool fromYForwardToUp = true) { return ToVector3(fromYForwardToUp).ToVector2(); }
	public Vector3i ToVector3i() { return new Vector3i(x, y, z); }
	public Vector3 ToVector3(bool fromYForwardToUp = true)
	{
		if (fromYForwardToUp)
			return new Vector3((float)x, (float)z, (float)y);
		return new Vector3((float)x, (float)y, (float)z);
	}

	public static bool operator ==(VVector3 s, VVector3 b)
	{
		if (double.IsNaN(s.x)) // only check x
			return double.IsNaN(b.x);
		return s.x == b.x && s.y == b.y && s.z == b.z;
	}
	public static bool operator !=(VVector3 s, VVector3 b)
	{
		if (double.IsNaN(s.x)) // only check x
			return !double.IsNaN(b.x);
		return !(s.x == b.x && s.y == b.y && s.z == b.z);
	}
	public static VVector3 operator -(VVector3 s, VVector3 b) { return new VVector3(s.x - b.x, s.y - b.y, s.z - b.z); }
	public static VVector3 operator -(VVector3 s) { return new VVector3(-s.x, -s.y, -s.z); }
	public static VVector3 operator +(VVector3 s, VVector3 b) { return new VVector3(s.x + b.x, s.y + b.y, s.z + b.z); }
	public static VVector3 operator *(double amount, VVector3 s) { return new VVector3(s.x * amount, s.y * amount, s.z * amount); }
	public static VVector3 operator *(VVector3 s, double amount) { return new VVector3(s.x * amount, s.y * amount, s.z * amount); }
	public static VVector3 operator *(VVector3 s, VVector3 other) { return new VVector3(s.x * other.x, s.y * other.y, s.z * other.z); }
	public static VVector3 operator /(VVector3 s, double amount) { return new VVector3(s.x / amount, s.y / amount, s.z / amount); }

	// general
	// ==========

	public static VVector3 Null { get { return new VVector3(double.NaN, double.NaN, double.NaN); } }
	public static VVector3 left { get { return new VVector3(-1, 0, 0); } }
	public static VVector3 right { get { return new VVector3(1, 0, 0); } }
	public static VVector3 back { get { return new VVector3(0, -1, 0); } }
	public static VVector3 forward { get { return new VVector3(0, 1, 0); } }
	public static VVector3 down { get { return new VVector3(0, 0, -1); } }
	public static VVector3 up { get { return new VVector3(0, 0, 1); } }

	//public static double Angle(VVector3 from, VVector3 to) { return Vector3.Angle(from.ToVector3(), to.ToVector3()); }
	public static double Angle(VVector3 from, VVector3 to) { return -Vector3.Angle(from.ToVector3(), to.ToVector3()); } // todo; make sure this is correct
	public static double Dot(VVector3 from, VVector3 to) { return Vector3.Dot(from.ToVector3(), to.ToVector3()); }
	//public static VVector3 Cross(VVector3 from, VVector3 to) { return Vector3.Cross(from.ToVector3(), to.ToVector3()).ToVVector3(); }
	//public static VVector3 Cross(VVector3 from, VVector3 to) { return new VVector3((from.y * to.z - from.z * to.y), (from.z * to.x - from.x * to.z), (from.x * to.y - from.y * to.x)); } (Vector3 code)
	public static VVector3 Cross(VVector3 from, VVector3 to) { return new VVector3((from.z * to.y - from.y * to.z), (from.x * to.z - from.z * to.x), (from.y * to.x - from.x * to.y)); }
	public static VVector3 Project(VVector3 from, VVector3 to) { return Vector3.Project(from.ToVector3(), to.ToVector3()).ToVVector3(); }
	public static VVector3 Min(VVector3 from, VVector3 to) { return Vector3.Min(from.ToVector3(), to.ToVector3()).ToVVector3(); }
	public static VVector3 Max(VVector3 from, VVector3 to) { return Vector3.Max(from.ToVector3(), to.ToVector3()).ToVVector3(); }

	public static VVector3 Average(List<VVector3> vectors) { return Average(vectors.ToArray()); }
	public static VVector3 Average(params VVector3[] vectors)
	{
		VVector3 total = VVector3.zero;
		foreach (VVector3 vector in vectors)
			total += vector;
		return total / vectors.Length;
	}

	//public VVector2(Vector2 obj) : this(obj.x, obj.y) {}
	public VVector3(double x, double y, double z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	[ProtoMember(1, IsRequired = true)] public double x;
	[ProtoMember(2, IsRequired = true)] public double y;
	[ProtoMember(3, IsRequired = true)] public double z;

	public double DistanceSquared(VVector3 other) { return (other - this).magnitudeSquared; }
	public double Distance(VVector3 other) { return (other - this).magnitude; }

	public double magnitude { get { return Math.Sqrt((x * x) + (y * y) + (z * z)); } }
	public double magnitudeSquared { get { return (x * x) + (y * y) + (z * z); } }

	public VVector3 Modulus(double b, bool keepResultPositive = true) { return new VVector3(x.Modulus(b, keepResultPositive), y.Modulus(b, keepResultPositive), z.Modulus(b, keepResultPositive)); }
	public VVector3 Times(VVector3 other) { return new VVector3(x * other.x, y * other.y, z * other.z); }

	public VVector3 normalized { get { return ToVector3().normalized.ToVVector3(); } }

	public VVector3 NewX(double val) { return new VVector3(val, y, z); }
	public VVector3 NewY(double val) { return new VVector3(x, val, z); }
	public VVector3 NewZ(double val) { return new VVector3(x, y, val); }
	public VVector3 NewX(Func<double, double> func) { return new VVector3(func(x), y, z); }
	public VVector3 NewY(Func<double, double> func) { return new VVector3(x, func(y), z); }
	public VVector3 NewZ(Func<double, double> func) { return new VVector3(x, y, func(z)); }
	public VVector3 NewXYZ(Func<double, double> func) { return new VVector3(func(x), func(y), func(z)); }
	public VVector3 SwappedYZ() { return new VVector3(x, z, y); }
	public void SwapYZ()
	{
		var temp = y;
		y = z;
		z = temp;
	}
	public VVector3 ClampTo(VBounds bounds) { return Min(Max(this, bounds.position), bounds.max); }

	public VVector3 FloorToMultipleOf(double val) { return new VVector3(x.FloorToMultipleOf(val), y.FloorToMultipleOf(val), z.FloorToMultipleOf(val)); }
	public VVector3 RoundToMultipleOf(double val) { return new VVector3(x.RoundToMultipleOf(val), y.RoundToMultipleOf(val), z.RoundToMultipleOf(val)); }
	public VVector3 CeilingToMultipleOf(double val) { return new VVector3(x.CeilingToMultipleOf(val), y.CeilingToMultipleOf(val), z.CeilingToMultipleOf(val)); }

	//public VVector3 RotateAround(VVector3 pivot, double degrees, VVector3? axis = null)
	public VVector3 RotateAround(VVector3 pivot, double degrees, VVector3 axis) {
		var relativeToPivot = this - pivot; // get point relative to pivot
		//var relativeToPivot_rotated = (Quaternion.Euler((float)angles) * relativeToPivot); // rotate it
		var relativeToPivot_rotated = (Quaternion.AngleAxis((float)degrees, axis.ToVector3()) * relativeToPivot.ToVector3()).ToVVector3(); // rotate it
		return pivot + relativeToPivot_rotated;
	}

	/// <summary>Set transform to {null} for world space.</summary>
	public VVector3 InSpaceOf(Transform transform, Transform sourceTransform = null) {
		var pos_world = sourceTransform != null ? sourceTransform.TransformPoint(ToVector3()).ToVVector3() : this;
		if (transform == null)
			return pos_world;
		return transform.InverseTransformPoint(pos_world.ToVector3()).ToVVector3();
	}

	public VVector3_WorldWrapper AsWorldPos() { return new VVector3_WorldWrapper(this); }

	//public static VVector3 AsLocalPos_ToWorldPos(this VVector3 self, Transform containerTransform) { return containerTransform.TransformPoint(self); }
	//public static VVector3 AsWorldPos_ToLocalPos(this VVector3 self, Transform containerTransform) { return containerTransform.InverseTransformPoint(self); }
}

[ProtoContract] public struct VVector4 {
	public static readonly VVector4 zero = new VVector4(0, 0, 0, 0);
	public static readonly VVector4 one = new VVector4(1, 1, 1, 1);

	// VDF
	// ==========

	[VDFSerialize] VDFNode Serialize() { return x + " " + y + " " + z + " " + w; }
	[VDFDeserialize] void Deserialize(VDFNode node) {
		var parts = node.primitiveValue.ToString().Split(new[] {' '});
		x = double.Parse(parts[0]);
		y = double.Parse(parts[1]);
		z = double.Parse(parts[2]);
		w = double.Parse(parts[3]);
	}

	// operators and overrides
	// ==========

	//public override int GetHashCode() { return ToString().GetHashCode(); }
	public override int GetHashCode() { return (((17 * 23 + x.GetHashCode()) * 23 + y.GetHashCode()) * 23 + z.GetHashCode()) * 23 + z.GetHashCode(); }
	//public override int GetHashCode() { return (x << 16) | y; }
	public override bool Equals(object other) {
		if (!(other is VVector4))
			return false;
		return this == (VVector4)other;
	}
	public override string ToString() { return x + " " + y + " " + z + " " + w; }

	// import
	//public static implicit operator VVector4(VVector2 obj) { return new VVector4(obj.x, obj.y, 0, 0); }
	//public static implicit operator VVector4(VVector3 obj) { return new VVector4(obj.x, obj.y, obj.z, 0); }

	// export
	public VVector2 ToVVector2() { return new VVector2(x, y); }
	public VVector3 ToVVector3() { return new VVector3(x, y, z); }
	public Vector4 ToVector4() { return new Vector4((float)x, (float)y, (float)z, (float)w); }
	public Quaternion ToQuaternion(bool fromYForwardToUp = true, bool fromRightToLeftHanded = true) {
		if (fromYForwardToUp) {
			if (fromRightToLeftHanded)
				return new Quaternion((float)-x, (float)-z, (float)-y, (float)w);
			return new Quaternion((float)x, (float)z, (float)y, (float)w);
		}
		if (fromRightToLeftHanded)
			return new Quaternion((float)-x, (float)-y, (float)-z, (float)w);
		return new Quaternion((float)x, (float)y, (float)z, (float)w);
	}

	public static bool operator ==(VVector4 s, VVector4 b) {
		if (double.IsNaN(s.x)) // only check x
			return double.IsNaN(b.x);
		return s.x == b.x && s.y == b.y && s.z == b.z && s.w == b.w;
	}
	public static bool operator !=(VVector4 s, VVector4 b) {
		if (double.IsNaN(s.x)) // only check x
			return !double.IsNaN(b.x);
		return !(s.x == b.x && s.y == b.y && s.z == b.z && s.w == b.w);
	}
	public static VVector4 operator -(VVector4 s, VVector4 b) { return new VVector4(s.x - b.x, s.y - b.y, s.z - b.z, s.w - b.w); }
	public static VVector4 operator -(VVector4 s) { return new VVector4(-s.x, -s.y, -s.z, -s.w); }
	public static VVector4 operator +(VVector4 s, VVector4 b) { return new VVector4(s.x + b.x, s.y + b.y, s.z + b.z, s.w + b.w); }
	public static VVector4 operator *(double amount, VVector4 s) { return new VVector4(s.x * amount, s.y * amount, s.z * amount, s.w * amount); }
	public static VVector4 operator *(VVector4 s, double amount) { return new VVector4(s.x * amount, s.y * amount, s.z * amount, s.w * amount); }
	public static VVector4 operator /(VVector4 s, double amount) { return new VVector4(s.x / amount, s.y / amount, s.z / amount, s.w / amount); }

	// Quaternion operations
	public static VVector4 operator *(VVector4 s, VVector4 other) { return (s.ToQuaternion() * other.ToQuaternion()).ToVVector4(); }

	// general
	// ==========

	public static VVector4 Null { get { return new VVector4(double.NaN, double.NaN, double.NaN, double.NaN); } }
	/*public static VVector3 left { get { return new VVector3(-1, 0, 0); } }
	public static VVector3 right { get { return new VVector3(1, 0, 0); } }
	public static VVector3 back { get { return new VVector3(0, -1, 0); } }
	public static VVector3 forward { get { return new VVector3(0, 1, 0); } }
	public static VVector3 down { get { return new VVector3(0, 0, -1); } }
	public static VVector3 up { get { return new VVector3(0, 0, 1); } }*/
	public static VVector4 identity { get { return new VVector4(0, 0, 0, 1); } }

	// to process
	public static double Quaternion_Dot(VVector4 from, VVector4 to) {
		return Quaternion.Dot(from.ToQuaternion(), to.ToQuaternion());
	}
	public static VVector4 Quaternion_Euler(double x, double y, double z) {
		// reverse rotation amounts, since switching from right to left handed
		return Quaternion.Euler(-(float)x, -(float)z, -(float)y).ToVVector4();
	}
	public static VVector4 Quaternion_Inverse(VVector4 s) {
		return Quaternion.Inverse(s.ToQuaternion()).ToVVector4();
	}
	public static VVector4 Quaternion_AngleAxis(double angle, VVector3 axis) {
		// reverse rotation amount, since switching from right to left handed
		return Quaternion.AngleAxis(-(float)angle, axis.ToVector3()).ToVVector4();
	}

	// processed
	public static VVector4 FromToRotation(VVector3 fromDirection, VVector3 toDirection) {
		return Quaternion.FromToRotation(fromDirection.ToVector3(), toDirection.ToVector3()).ToVVector4();
	}
	public static VVector4 LookRotation(VVector3 forward, VVector3? upward = null) {
		return upward.HasValue ? Quaternion.LookRotation(forward.ToVector3(), upward.Value.ToVector3()).ToVVector4() : Quaternion.LookRotation(forward.ToVector3()).ToVVector4();
	}

	public VVector2 Q_Transform(VVector2 point) { return Q_Transform(point.ToVVector3()).ToVVector2(); }
	public VVector3 Q_Transform(VVector3 point) { return (ToQuaternion() * point.ToVector3()).ToVVector3(); }
	// gets the angle between dirA and dirB around axis
	public double GetRotation_AroundAxis(VVector3 axis) {
		var direction = Q_Transform(VVector3.one);
		return V.Angle_AroundAxis(VVector3.one, direction, axis);
	}

	//public VVector2(Vector2 obj) : this(obj.x, obj.y) {}
	public VVector4(double x, double y, double z, double w) {
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	[ProtoMember(1, IsRequired = true)] public double x;
	[ProtoMember(2, IsRequired = true)] public double y;
	[ProtoMember(3, IsRequired = true)] public double z;
	[ProtoMember(3, IsRequired = true)] public double w;

	public double DistanceSquared(VVector4 other) { return (other - this).magnitudeSquared; }
	public double Distance(VVector4 other) { return (other - this).magnitude; }

	public double magnitude { get { return Math.Sqrt((x * x) + (y * y) + (z * z) + (w * w)); } }
	public double magnitudeSquared { get { return (x * x) + (y * y) + (z * z) + (w * w); } }

	public VVector4 Modulus(double b, bool keepResultPositive = true) { return new VVector4(x.Modulus(b, keepResultPositive), y.Modulus(b, keepResultPositive), z.Modulus(b, keepResultPositive), w.Modulus(b, keepResultPositive)); }
	public VVector4 Times(VVector4 other) { return new VVector4(x * other.x, y * other.y, z * other.z, w * other.w); }

	public VVector4 normalized { get { return ToVector4().normalized.ToVVector4(); } }

	public VVector4 NewX(double val) { return new VVector4(val, y, z, w); }
	public VVector4 NewY(double val) { return new VVector4(x, val, z, w); }
	public VVector4 NewZ(double val) { return new VVector4(x, y, val, w); }
	public VVector4 NewW(double val) { return new VVector4(x, y, z, val); }
}

public struct VRect // the natural props would be "position" and "size"; we're doing a non-standard set of base-props, mostly to be consistent with the built-in Rect class
{
	public static VRect Null { get { return new VRect(double.NaN, double.NaN, double.NaN, double.NaN); } }

	// VDF
	// ==========

	[VDFSerialize] VDFNode Serialize() { return x + " " + y + " " + width + " " + height; }
	[VDFDeserialize] void Deserialize(VDFNode node)
	{
		var parts = node.primitiveValue.ToString().Split(new[] {' '});
		x = double.Parse(parts[0]);
		y = double.Parse(parts[1]);
		width = double.Parse(parts[2]);
		height = double.Parse(parts[3]);
	}

	// operators and overrides
	// ==========

	public static bool operator ==(VRect s, VRect b)
	{
		if (double.IsNaN(s.x)) // only check x
			return double.IsNaN(b.x);
		return s.x == b.x && s.y == b.y && s.width == b.width && s.height == b.height;
	}
	public static bool operator !=(VRect s, VRect b)
	{
		/*if (double.IsNaN(s.x)) // only check x
			return !double.IsNaN(b.x);
		return !(s.x == b.x && s.y == b.y && s.width == b.width && s.height == b.height);*/
		return !(s == b);
	}

	public override int GetHashCode() { return (((17*23 + x.GetHashCode())*23 + y.GetHashCode())*23 + width.GetHashCode())*23 + height.GetHashCode(); }
	public override bool Equals(object other)
	{
		if (!(other is VRect))
			return false;
		return this == (VRect)other;
	}
	//public override string ToString() { return x.ToVString() + " " + y.ToVString() + " " + width.ToVString() + " " + height.ToVString(); }
	//public override string ToString() { return x.ToVString() + " " + y.ToVString() + "|" + right.ToVString() + " " + top.ToVString(); }
	public override string ToString() { return x.ToVString() + " " + y.ToVString() + " " + width.ToVString() + " " + height.ToVString(); }

	// export
	public Rect ToRect() { return new Rect((float)x, (float)y, (float)width, (float)height); }

	// others
	public bool EqualsAbout(VRect other, float maxDifForEquals = .000001f)
		{ return Math.Abs(x - other.x) <= maxDifForEquals && Math.Abs(y - other.y) <= maxDifForEquals && Math.Abs(width - other.width) <= maxDifForEquals && Math.Abs(height - other.height) <= maxDifForEquals; }

	// general
	// =========

	public static VRect New_ByCenter(VVector2 center, VVector2 size) { return new VRect(center - (size / 2), size); }
	public VRect(VVector2 position, VVector2 size) : this(position.x, position.y, size.x, size.y) {
		isSet = true;
	}
	public VRect(double x, double y, double width, double height) {
		isSet = true;

		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;

		init_right = x + width;
		init_top = y + height;
	}

	public bool isSet;

	public double x;
	public double y;
	public double width;
	public double height;

	public double right { get { return x + width; } }
	public double init_right; // maybe temp
	public double top { get { return y + height; } }
	public double init_top;

	public VVector2 position
	{
		get { return new VVector2(x, y); }
		set
		{
			x = value.x;
			y = value.y;
		}
	}
	public VVector2 size
	{
		get { return new VVector2(width, height); }
		set
		{
			width = value.x;
			height = value.y;
		}
	}

	//public Vector2 min { get { return new Vector2((float)x, (float)y); } }
	public VVector2 center { get { return position + (size * .5); } }
	public VVector2 max { get { return new VVector2((float)right, (float)top); } }

	public VRect NewX(double val) { return new VRect(val, y, width, height); }
	public VRect NewY(double val) { return new VRect(x, val, width, height); }
	public VRect NewWidth(double val) { return new VRect(x, y, val, height); }
	public VRect NewHeight(double val) { return new VRect(x, y, width, val); }

	public VRect NewLeft(double val) { return new VRect(val, y, right - val, height); }
	public VRect NewTop(double val) { return new VRect(x, y, width, val - y); }
	public VRect NewRight(double val) { return new VRect(x, y, val - x, height); }
	public VRect NewBottom(double val) { return new VRect(x, val, width, top - val); }

	public VRect NewPosition(VVector2 val) { return new VRect(val, size); }
	public VRect NewSize(VVector2 val) { return new VRect(position, val); }
	public VRect NewCenter(VVector2 val) { return new VRect(val - (size / 2), size); }
	public VRect NewMax(VVector2 val) { return new VRect(val - size, size); }

	public VRect Grow(double amountOnEachSide) { return new VRect(x - amountOnEachSide, y - amountOnEachSide, width + (amountOnEachSide * 2), height + (amountOnEachSide * 2)); }
	public VRect Shrink(double amountOnEachSide) { return Grow(-amountOnEachSide); }

	public void Set(VRect rect)
	{
		x = rect.x;
		y = rect.y;
		width = rect.width;
		height = rect.height;
	}
	public VRect Encapsulating(VRect rect)
	{
		if (this == Null)
			return rect;
		var newX = Math.Min(x, rect.x);
		var newY = Math.Min(y, rect.y);
		return new VRect(newX, newY, Math.Max(right, rect.right) - newX, Math.Max(top, rect.top) - newY);
	}
	public VRect Encapsulating(VVector2 point) { return Encapsulating(new VRect(point, VVector2.zero)); }
	public void Encapsulate(VRect rect) { Set(Encapsulating(rect)); }
	public void Encapsulate(VVector2 point) { Set(Encapsulating(point)); }

	//public static VVector3 FloorToMultipleOf(this VVector3 s, double val) { return new VVector3(s.x.FloorToMultipleOf(val), s.y.FloorToMultipleOf(val), s.z.FloorToMultipleOf(val)); }
	public VRect RoundToMultipleOf(double val) { return new VRect(x.RoundToMultipleOf(val), y.RoundToMultipleOf(val), width.RoundToMultipleOf(val), height.RoundToMultipleOf(val)); }

	//public bool Intersects(VRect other) { return ToRect().Overlaps(other.ToRect()); }
	public bool Intersects(VRect other) { return right > other.x && x < other.right && top > other.y && y < other.top; }
	public bool Intersects_Init(VRect other) { return init_right > other.x && x < other.init_right && init_top > other.y && y < other.init_top; }
	//public bool Contains(Vector2 point) { return Overlaps(new VRect(point.x, point.y, 1, 1)); }
	public bool Contains(VVector2 point) { return x <= point.x && right > point.x && y <= point.y && top > point.y; }
	//public bool Contains(VVector2 point) { return x <= point.x && right >= point.x && y <= point.y && top >= point.y; } // maybe todo: use this version, to make consistent with VBounds
	public bool Contains(VRect other) { return x <= other.x && right >= other.right && y <= other.y && top >= other.top; }

	public List<VVector2> GetCorners() { return new List<VVector2> {new VVector2(x, y), new VVector2(right, y), new VVector2(x, top), new VVector2(right, top)}; }
}

public struct VBounds
{
	// VDF
	// ==========
	
	[VDFSerialize] VDFNode Serialize() { return position.x + " " + position.y + " " + position.z + "|" + size.x + " " + size.y + " " + size.z; }
	[VDFDeserialize] void Deserialize(VDFNode node)
	{
		var parts = node.primitiveValue.ToString().Split(new[] {'|'});
		var posParts = parts[0].Split(new[] {' '});
		position = new VVector3(double.Parse(posParts[0]), double.Parse(posParts[1]), double.Parse(posParts[2]));
		var sizeParts = parts[1].Split(new[] {' '});
		size = new VVector3(double.Parse(sizeParts[0]), double.Parse(sizeParts[1]), double.Parse(sizeParts[2]));
	}

	// operators and overrides
	// ==========

	public static bool operator ==(VBounds s, VBounds b)
	{
		if (s.position == VVector3.Null) //&& s.size == VVector3.Null) // only check pos
			return b.position == VVector3.Null;
		return s.position == b.position && s.size == b.size;
	}
	public static bool operator !=(VBounds s, VBounds b) { return !(s == b); }

	public override int GetHashCode() { return (17*23 + position.GetHashCode())*23 + size.GetHashCode(); }
	public override bool Equals(object other)
	{
		if (!(other is VBounds))
			return false;
		return this == (VBounds)other;
	}
	//public override string ToString() { return x.ToVString() + " " + y.ToVString() + " " + width.ToVString() + " " + height.ToVString(); }

	// export
	public static implicit operator Bounds(VBounds s) { return new Bounds((s.position + (s.size / 2)).ToVector3(), s.size.ToVector3()); }

	// general
	// ==========

	public static VBounds Null { get { return new VBounds(VVector3.Null, VVector3.Null); } }

	//public static VBounds Min(VBounds a, VBounds b) { return new VBounds(VVector3.Min(a.position, b.position), VVector3.Min(a.size, b.size)); }
	//public static VBounds Max(VBounds a, VBounds b) { return new VBounds(VVector3.Max(a.position, b.position), VVector3.Max(a.size, b.size)); }

	public VBounds(VVector3 position, VVector3 size)
	{
		this.position = position;
		this.size = size;
	}

	public VVector3 position;
	public VVector3 size;

	public VVector3 min
	{
		get { return position; }
		set
		{
			var oldMax = max;
			position = value;
			max = oldMax;
		}
	}
	public VVector3 center
	{
		get { return position + (size / 2); }
		set { position = value - (size / 2); }
	}
	public VVector3 max
	{
		get { return position + size; }
		set { size = value - position; }
	}

	public List<VVector3> GetCorners(bool inParent_vsInSelf = true)
	{
		var result = new List<VVector3>();
		for (var x = 0; x <= 1; x++)
			for (var y = 0; y <= 1; y++)
				for (var z = 0; z <= 1; z++)
					result.Add((inParent_vsInSelf ? position : VVector3.zero) + size.Times(new VVector3(x, y, z)));
		return result;
	}

	public VBounds NewPosition(VVector3 val) { return new VBounds(val, size); }
	public VBounds NewSize(VVector3 val) { return new VBounds(position, val); }

	public void Set(VBounds bounds)
	{
		position = bounds.position;
		size = bounds.size;
	}
	public VBounds Encapsulating(VBounds bounds)
	{
		if (this == Null)
			return bounds;
		var newPosition = new VVector3(Math.Min(position.x, bounds.position.x), Math.Min(position.y, bounds.position.y), Math.Min(position.z, bounds.position.z));
		return new VBounds(newPosition, new VVector3(Math.Max(max.x, bounds.max.x), Math.Max(max.y, bounds.max.y), Math.Max(max.z, bounds.max.z)) - newPosition); // (a bound's at-max point is considered encapsulated by that bounds)
	}
	public VBounds Encapsulating(VVector3 point) { return Encapsulating(new VBounds(point, VVector3.zero)); }
	public void Encapsulate(VBounds bounds) { Set(Encapsulating(bounds)); }
	public void Encapsulate(VVector3 point) { Set(Encapsulating(point)); }

	public VBounds RoundMinAndMaxToMultipleOf(double val)
	{
		var pos = min.RoundToMultipleOf(val);
		return new VBounds(pos, max.RoundToMultipleOf(val) - pos);
	}

	// maybe temp; currently we consider the ender-point of one bounds to intersect the first-point of another
	public bool Intersects(VBounds other) { return position.x <= other.max.x && max.x >= other.position.x && position.y <= other.max.y && max.y >= other.position.y && position.z <= other.max.z && max.z >= other.position.z; }
	public Bounds ToBounds(bool fromYForwardToUp = true) { return new Bounds(center.ToVector3(fromYForwardToUp), size.ToVector3(fromYForwardToUp)); }
	public VRect ToVRect() { return new VRect(position.ToVVector2(), size.ToVVector2()); }
}

public class VRay
{
	public Ray ToRay(bool topView = true) { return new Ray(origin.ToVector3(topView), direction.ToVector3(topView)); }

	public VRay(VVector3 origin, VVector3 direction)
	{
		this.origin = origin;
		this.direction = direction;
	}

	public VVector3 origin;
	public VVector3 direction;
}
namespace VectorStructExtensions // make the file opt-in to adding these extension methods
{
	public class VVector3_WorldWrapper {
		public VVector3 vector;
		public VVector3_WorldWrapper(VVector3 vector) { this.vector = vector; }

		public VVector3 ToWorldPos(double toUnit_worldSize = 1) { return vector / toUnit_worldSize; }
	}

	public static class VectorStructExtensions {
		// Vector2
		public static VVector2 ToVVector2(this Vector2 s, bool fromYUpToForward = true) {
			if (fromYUpToForward)
				return new VVector2(s.x, 0);
			return new VVector2(s.x, s.y);
		}
		public static Vector3 ToVector3(this Vector2 s) { return s; }
		public static Vector4 ToVector4(this Vector2 s) { return s; }

		// Vector3
		public static Vector2 ToVector2(this Vector3 s) { return s; }
		public static VVector3 ToVVector3(this Vector3 s, bool fromYUpToForward = true)
		{
			if (fromYUpToForward)
				return new VVector3(s.x, s.z, s.y);
			return new VVector3(s.x, s.y, s.z);
		}
		public static Vector4 ToVector4(this Vector3 s) { return s; }

		// Vector4
		public static Vector2 ToVector2(this Vector4 s) { return s; }
		public static Vector3 ToVector3(this Vector4 s) { return s; }
		public static VVector4 ToVVector4(this Vector4 s) { return new VVector4(s.x, s.y, s.z, s.w); }

		// Quaternion
		public static VVector4 ToVVector4(this Quaternion s, bool fromYUpToForward = true, bool fromLeftToRightHanded = true)
		{
			if (fromYUpToForward)
			{
				if (fromLeftToRightHanded)
					return new VVector4(-s.x, -s.z, -s.y, s.w);
				return new VVector4(s.x, s.z, s.y, s.w);
			}
			if (fromLeftToRightHanded)
				return new VVector4(-s.x, -s.y, -s.z, s.w);
			return new VVector4(s.x, s.y, s.z, s.w);
		}

		// Rect
		public static VRect ToVRect(this Rect s, bool fromYUpToForward = true) { return new VRect(s.position.ToVVector2(fromYUpToForward), s.size.ToVVector2(fromYUpToForward)); }

		// Bounds
		public static VBounds ToVBounds(this Bounds s, bool fromYUpToForward = true) { return new VBounds(s.min.ToVVector3(fromYUpToForward), s.size.ToVVector3(fromYUpToForward)); }

		// Ray
		public static VRay ToVRay(this Ray s, bool fromYUpToForward = true) { return new VRay(s.origin.ToVVector3(fromYUpToForward), s.direction.ToVVector3(fromYUpToForward)); }

		// for classes that depend on vector-structures, from other files
		// ==========
	}
}