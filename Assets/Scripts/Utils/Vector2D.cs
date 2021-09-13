using System;

/// <summary>
/// Represents 2D vectors and points (using double instead of float like UnityEngine.Vector2)
/// </summary>
public struct Vector2D
{
    /// <summary>
    /// Shorthand for writing Vector2D(0, -1)
    /// </summary>
    public static Vector2D Down { get => new Vector2D(0, -1); }

    /// <summary>
    /// Shorthand for writing Vector2D(-1, 0)
    /// </summary>
    public static Vector2D Left { get => new Vector2D(-1, 0); }

    /// <summary>
    /// Shorthand for writing Vector2D(double.NegativeInfinity, double.NegativeInfinity)
    /// </summary>
    public static Vector2D NegativeInfinity { get => new Vector2D(double.NegativeInfinity, double.NegativeInfinity); }

    /// <summary>
    /// Shorthand for writing Vector2D(1, 1)
    /// </summary>
    public static Vector2D One { get => new Vector2D(1, 1); }

    /// <summary>
    /// Shorthand for writing Vector2D(double.PositiveInfinity, double.PositiveInfinity)
    /// </summary>
    public static Vector2D PositiveInfinity { get => new Vector2D(double.PositiveInfinity, double.PositiveInfinity); }

    /// <summary>
    /// Shorthand for writing Vector2D(1, 0)
    /// </summary>
    public static Vector2D Right { get => new Vector2D(1, 0); }

    /// <summary>
    /// Shorthand for writing Vector2D(0, 1)
    /// </summary>
    public static Vector2D Up { get => new Vector2D(0, 1); }

    /// <summary>
    /// Shorthand for writing Vector2D(0, 0)
    /// </summary>
    public static Vector2D Zero { get => new Vector2D(0, 0); }

    /// <summary>
    /// Returns the length of this vector (Read Only).
    /// 
    /// The length of the vector is square root of (X*X+Y*Y).
    /// If you only need to compare magnitudes of some vectors, you can compare squared magnitudes of them using SqrMagnitude (computing squared magnitudes is faster).
    /// </summary>
    public double Magnitude { get => Math.Sqrt(X * X + Y * Y); }

    /// <summary>
    /// Returns this vector with a magnitude of 1 (Read Only).
    /// 
    /// When normalized, a vector keeps the same direction but its length is 1.0.
    /// Note that the current vector is unchanged and a new normalized vector is returned. If you want to normalize the current vector, use Normalize function.
    /// If the vector is too small to be normalized a zero vector will be returned.
    /// </summary>
    public Vector2D Normalized { get => throw new System.NotImplementedException(); } // TODO implement this

    /// <summary>
    /// Returns the squared length of this vector (Read Only).
    /// 
    /// Calculating the squared magnitude instead of the magnitude is much faster. Often if you are comparing magnitudes of two vectors you can just compare their squared magnitudes.

    /// </summary>
    public double SqrMagnitude { get => X * X + Y * Y; }

    /// <summary>
    /// Access the X or Y component using [0] or [1] respectively.
    /// </summary>
    /// <param name="index">The component index</param>
    /// <returns>The compontent</returns>
    public double this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return this.X;
                case 1:
                    return this.Y;
                default:
                    throw new System.IndexOutOfRangeException("Index was outside the bounds of the array.");
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                    this.X = value;
                    break;
                case 1:
                    this.Y = value;
                    break;
                default:
                    throw new System.IndexOutOfRangeException("Index was outside the bounds of the array.");
            }
        }
    }

    /// <summary>
    /// X component of the vector
    /// </summary>
    public double X { get; set; }
    /// <summary>
    /// Y component of the vector
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Constructs a new vector with given x, y components
    /// </summary>
    /// <param name="x">X component of the vector</param>
    /// <param name="y">Y component of the vector</param>
    public Vector2D(double x, double y)
    {
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Returns true if the given vector is exactly equal to this vector.
    /// 
    /// Due to floating point inaccuracies, this might return false for vectors which are essentially (but not exactly) equal. Use the == operator to test two vectors for approximate equality.
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>true if the given vector is exactly equal to this vector</returns>
    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != typeof(Vector2D))
        {
            return false;
        }

        Vector2D other = (Vector2D)obj;

        return (this.X == other.X && this.Y == other.Y);
    }

    /// <summary>
    /// Get the position hash code
    /// </summary>
    /// <returns>A hash code for the current position</returns>
    public override int GetHashCode()
    {
        int prime = 31;
        return (int)(prime * X + prime * Y);
    }

    /// <summary>
    /// Makes this vector have a magnitude of 1.
    /// 
    /// When normalized, a vector keeps the same direction but its length is 1.0.
    /// Note that this function will change the current vector. If you want to keep the current vector unchanged, use normalized variable.
    /// If this vector is too small to be normalized it will be set to zero.
    /// </summary>
    public void Normalize()
    {
        throw new System.NotImplementedException(); // TODO implement this
    }

    /// <summary>
    /// Set X and Y components of an existing Vector2D
    /// </summary>
    /// <param name="newX">New X component value</param>
    /// <param name="newY">New Y component value</param>
    public void Set(double newX, double newY)
    {
        this.X = newX;
        this.Y = newY;
    }

    /// <summary>
    /// Returns a formatted string for this vector
    /// </summary>
    /// <returns>String representation of the vector</returns>
    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    /// <summary>
    /// Returns the unsigned angle in degrees between from and to.
    /// 
    /// The angle returned is the unsigned acute angle between the two vectors. This means the smaller of the two possible angles between the two vectors is used. The result is never greater than 180 degrees.
    /// </summary>
    /// <param name="from">The vector from which the angular difference is measured</param>
    /// <param name="to">The vector to which the angular difference is measured</param>
    /// <returns>The unsigned angle in degrees between from and to</returns>
    public static double Angle(Vector2D from, Vector2D to)
    {
        throw new System.NotImplementedException(); // TODO implement this
    }

    /// <summary>
    /// Returns a copy of vector with its magnitude clamped to maxLength
    /// </summary>
    /// <param name="vector">Vector to be clamped</param>
    /// <param name="maxLength">Length to clamp</param>
    /// <returns>A copy of vector with its magnitude clamped to maxLength</returns>
    public static Vector2D ClampMagnitude(Vector2D vector, double maxLength)
    {
        throw new System.NotImplementedException(); // TODO implement this
    }

    /// <summary>
    /// Returns the distance between a and b.
    /// 
    /// Vector2.Distance(a,b) is the same as (a-b).Magnitude.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <param name="b">Vector b</param>
    /// <returns>Distance between a and b</returns>
    public static double Distance(Vector2D a, Vector2D b)
    {
        return (a - b).Magnitude;
    }

    /// <summary>
    /// Dot Product of two vectors.
    /// 
    /// Returns lhs . rhs.
    /// For normalized vectors Dot returns 1 if they point in exactly the same direction; -1 if they point in completely opposite directions; and a number in between for other cases (e.g. Dot returns zero if vectors are perpendicular).
    /// For vectors of arbitrary length the Dot return values are similar: they get larger when the angle between vectors decreases.
    /// </summary>
    /// <param name="lhs">Left hand side vector</param>
    /// <param name="rhs">Right hand side vector</param>
    /// <returns>lhs . rhs</returns>
    public static double Dot(Vector2D lhs, Vector2D rhs)
    {
        throw new System.NotImplementedException(); // TODO implement this
    }

    /// <summary>
    /// Linearly interpolates between vectors a and b by t.
    /// 
    /// The parameter t is clamped to the range [0, 1].
    /// When t = 0 returns a.
    /// When t = 1 return b.
    /// When t = 0.5 returns the midpoint of a and b.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <param name="b">Vector b</param>
    /// <param name="t">Parameter t</param>
    /// <returns>Interpolated vector</returns>
    public static Vector2D Lerp(Vector2D a, Vector2D b, double t)
    {
        throw new System.NotImplementedException(); // TODO implement this
    }

    /// <summary>
    /// Linearly interpolates between vectors a and b by t.
    /// When t = 0 returns a.
    /// When t = 1 return b.
    /// When t = 0.5 returns the midpoint of a and b.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <param name="b">Vector b</param>
    /// <param name="t">Parameter t</param>
    /// <returns>Interpolated vector</returns>
    public static Vector2D LerpUnclamped(Vector2D a, Vector2D b, double t)
    {
        throw new System.NotImplementedException(); // TODO implement this
    }

    /// <summary>
    /// Returns a vector that is made from the largest components of two vectors
    /// </summary>
    /// <param name="lhs">Left hand side vector</param>
    /// <param name="rhs">Right hand side vector</param>
    /// <returns>A vector that is made from the largest components of two vectors</returns>
    public static Vector2D Max(Vector2D lhs, Vector2D rhs)
    {
        return new Vector2D(Math.Max(lhs.X, rhs.X), Math.Max(lhs.Y, rhs.Y));
    }

    /// <summary>
    /// Returns a vector that is made from the smallest components of two vectors
    /// </summary>
    /// <param name="lhs">Left hand side vector</param>
    /// <param name="rhs">Right hand side vector</param>
    /// <returns>A vector that is made from the smallest components of two vectors</returns>
    public static Vector2D Min(Vector2D lhs, Vector2D rhs)
    {
        return new Vector2D(Math.Min(lhs.X, rhs.X), Math.Min(lhs.Y, rhs.Y));
    }

    /// <summary>
    /// Moves a point current towards target.
    /// 
    /// This is essentially the same as Vector2D.Lerp but instead the function will ensure that the distance never exceeds maxDistanceDelta. Negative values of maxDistanceDelta pushes the vector away from target.
    /// </summary>
    /// <param name="current">Current point</param>
    /// <param name="target">Target point</param>
    /// <param name="maxDistanceDelta">Maximum distance delta</param>
    /// <returns></returns>
    public static Vector2D MoveTowards(Vector2D current, Vector2D target, double maxDistanceDelta)
    {
        throw new System.NotImplementedException(); // TODO implement this
    }

    /// <summary>
    /// Returns the 2D vector perpendicular to this 2D vector. The result is always rotated 90-degrees in a counter-clockwise direction for a 2D coordinate system where the positive Y axis goes up.
    /// </summary>
    /// <param name="inDirection">The input direction</param>
    /// <returns>The perpendicular direction</returns>
    public static Vector2D Perpendicular(Vector2D inDirection)
    {
        throw new System.NotImplementedException(); // TODO implement this
    }

    /// <summary>
    /// Reflects a vector off the vector defined by a normal
    /// </summary>
    /// <param name="inDirection">The input direction</param>
    /// <param name="inNormal">The input normal</param>
    /// <returns>Reflected vector</returns>
    public static Vector2D Reflect(Vector2D inDirection, Vector2D inNormal)
    {
        throw new System.NotImplementedException(); // TODO implement this
    }

    /// <summary>
    /// Multiplies two vectors component-wise.
    /// 
    /// Every component in the result is a component of a multiplied by the same component of b.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <param name="b">Vector b</param>
    /// <returns>Scaled vector</returns>
    public static Vector2D Scale(Vector2D a, Vector2D b)
    {
        return new Vector2D(a.X * b.X, a.Y * b.Y);
    }

    /// <summary>
    /// Multiplies every component of this vector by the same component of scale
    /// </summary>
    /// <param name="scale">Scale vector</param>
    public void Scale(Vector2D scale)
    {
        this.X *= scale.X;
        this.Y *= scale.Y;
    }

    /// <summary>
    /// Returns the signed angle in degrees between from and to.
    /// 
    /// The angle returned is the signed acute clockwise angle between the two vectors. This means the smaller of the two possible angles between the two vectors is used. The result is never greater than 180 degrees or smaller than -180 degrees.
    /// </summary>
    /// <param name="from">The vector from which the angular difference is measured</param>
    /// <param name="to">The vector to which the angular difference is measured</param>
    /// <returns>The signed angle in degrees between from and to</returns>
    public static double SignedAngle(Vector2D from, Vector2D to)
    {
        throw new System.NotImplementedException(); // TODO implement this
    }

    /// <summary>
    /// Subtracts one vector from another.
    /// 
    /// Subtracts each component of b from a.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <param name="b">Vector b</param>
    /// <returns>Subtracted vector</returns>
    public static Vector2D operator -(Vector2D a, Vector2D b)
    {
        return new Vector2D(a.X - b.X, a.Y - b.Y);
    }

    /// <summary>
    /// Negates a vector.
    /// 
    /// Each component in the result is negated.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <returns>Negated vector</returns>
    public static Vector2D operator -(Vector2D a)
    {
        return new Vector2D(-a.X, -a.Y);
    }

    /// <summary>
    /// Multiplies a vector by a number.
    /// 
    /// Multiplies each component of a by a number d.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <param name="d">Scalar d</param>
    /// <returns>Multiplied vector</returns>
    public static Vector2D operator *(Vector2D a, double d)
    {
        return new Vector2D(a.X * d, a.Y * d);
    }

    /// <summary>
    /// Multiplies a vector by a number.
    /// 
    /// Multiplies each component of a by a number d.
    /// </summary>
    /// <param name="d">Scalar d</param>
    /// <param name="a">Vector a</param>
    /// <returns>Multiplied vector</returns>
    public static Vector2D operator *(double d, Vector2D a)
    {
        return a * d;
    }

    /// <summary>
    /// Multiplies a vector by another vector.
    /// 
    /// Multiplies each component of a by its matching component from b.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <param name="b">Vector b</param>
    /// <returns>Multiplied vector</returns>
    public static Vector2D operator *(Vector2D a, Vector2D b)
    {
        return new Vector2D(a.X * b.X, a.Y * b.Y);
    }

    /// <summary>
    /// Divides a vector by a number.
    /// 
    /// Divides each component of a by a number d.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <param name="d">Scalar d</param>
    /// <returns>Divided vector</returns>
    public static Vector2D operator /(Vector2D a, double d)
    {
        return new Vector2D(a.X / d, a.Y / d);
    }

    /// <summary>
    /// Divides a vector by another vector.
    /// 
    /// Divides each component of a by its matching component from b.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <param name="b">Vector b</param>
    /// <returns>Divided vector</returns>
    public static Vector2D operator /(Vector2D a, Vector2D b)
    {
        return new Vector2D(a.X / b.X, a.Y / b.Y);
    }

    /// <summary>
    /// Adds two vectors.
    /// 
    /// Adds corresponding components together.
    /// </summary>
    /// <param name="a">Vector a</param>
    /// <param name="b">Vector b</param>
    /// <returns>Added vector</returns>
    public static Vector2D operator +(Vector2D a, Vector2D b)
    {
        return new Vector2D(a.X + b.X, a.Y + b.Y);
    }

    /// <summary>
    /// Returns true if two vectors are approximately equal.
    /// 
    /// To allow for floating point inaccuracies, the two vectors are considered equal if the magnitude of their difference is less than 1e-5.
    /// </summary>
    /// <param name="lhs">Left hand side vector</param>
    /// <param name="rhs">Right hand side vector</param>
    /// <returns>true if two vectors are approximately equal</returns>
    public static bool operator ==(Vector2D lhs, Vector2D rhs)
    {
        bool a = UnityEngine.Vector2.one != UnityEngine.Vector2.zero;
        double tolerance = 1e-5; // TODO are we sure we should keep the same tolerance as Unity considering we're using double instead of float?
        return Math.Abs(lhs.X - rhs.X) < tolerance && Math.Abs(lhs.Y - rhs.Y) < tolerance;
    }

    /// <summary>
    /// Returns true if vectors different.
    /// 
    /// Very close vectors are treated as being equal.
    /// </summary>
    /// <param name="lhs">Left hand side vector</param>
    /// <param name="rhs">Right hand side vector</param>
    /// <returns>true if two vectors are approximately not equal</returns>
    public static bool operator !=(Vector2D lhs, Vector2D rhs)
    {
        return !(lhs == rhs);
    }
}