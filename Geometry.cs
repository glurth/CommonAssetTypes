using System.Collections.Generic;
using UnityEngine;
namespace EyE.Geometry
{
    public class GeometryException : System.Exception
    {
        public GeometryException(string message) : base(message)
        {
        }
    }


    public static class Vector3Extensions
    {
        public static Vector3 AvgPos(this List<Vector3> points)
        {
            int count = 0;
            Vector3 vectorSum = Vector3.zero;
            foreach (Vector3 p in points)
            {
                vectorSum += p;
                count++;
            }
            return vectorSum / count;
        }

        /// <summary>
        /// Determines whether two floating-point numbers are approximately equal based on a fractional difference.
        /// This extension method is useful for comparing floating-point numbers where precision errors may occur.
        /// note/warning: if BOTH params a and b are computed zero values, plus/minus precision errors, this function may return false- as it will assume you are trying to compare tiny numbers.
        /// </summary>
        /// <param name="a">The first floating-point number.</param>
        /// <param name="b">The second floating-point number.</param>
        /// <param name="fractionalDiff">The maximum fractional difference allowed for the numbers to be considered equal. Default is 0.001.</param>
        /// <returns>True if the numbers are approximately equal; otherwise, false.</returns>
        public static bool CloseEqual(this float a, float b, float fractionalDiff = .001f)
        {
            if (a == b) return true;
            float aMinusB = a - b;
            if (aMinusB < 0)
                aMinusB *= -1; // Convert the difference to absolute value
            if (a < 0)
                a *= -1; // Convert 'a' to its absolute value
            if (b < 0)
                b *= -1; // Convert 'b' to its absolute value

            float larger = a;
            if (a < b) larger = b; // Determine the larger of the two values

            // Use the larger value as a reference for the fractional difference to ensure the comparison is scale-invariant.
            if (aMinusB <= fractionalDiff * larger)
                return true;
            return false;
        }

        /// <summary>
        /// Determines whether two Vector3 values are approximately equal based on a fractional difference.
        /// This extension method compares each component (x, y, z) of the vectors using the CloseEqual method.
        /// </summary>
        /// <param name="a">The first Vector3 value.</param>
        /// <param name="b">The second Vector3 value.</param>
        /// <param name="fractionalDiff">The maximum fractional difference allowed for each component to be considered equal. Default is 0.001.</param>
        /// <returns>True if all components are approximately equal; otherwise, false.</returns>
        public static bool CloseEqual(this Vector3 a, Vector3 b, float fractionalDiff = .001f)
        {
            if (!CloseEqual(a.x, b.x, fractionalDiff)) return false;
            if (!CloseEqual(a.y, b.y, fractionalDiff)) return false;
            if (!CloseEqual(a.z, b.z, fractionalDiff)) return false;
            return true;
        }


        /// <summary>
        /// Returns the circular index of the given integer `a` within the specified range of 0 to `size - 1`.
        /// If `a` exceeds or goes below the bounds, it wraps around to ensure the result is always between 0 and `size - 1`.
        /// For example:
        /// - If `a` is greater than or equal to `size`, it will wrap around to the beginning (0).
        /// - If `a` is negative, it will wrap around to the end (`size - 1`).
        /// </summary>
        /// <param name="a">The index to be wrapped around.</param>
        /// <param name="size">The size of the range (exclusive upper bound).</param>
        /// <returns>The circularly wrapped index.</returns>
        public static int CircularIndex(this int a, int size)
        {
            int b = a % size;
            if (b < 0) b += size;
            return b;
        }

        const float radToRots = 1.0f / (2f * Mathf.PI);
        /// <summary>
        /// Converts a 3D vertex on the unit sphere to its corresponding cylindrical UV coordinates.
        /// The longitude corresponds to the X coordinate remapped to a [0, 1] range. It represents the rotation around the Y-axis (north/south pole), 
        /// and the latitude corresponds to the Y-coordinate remapped to a [0, 1] range. It represent the distance from the north pole to the south pole.
        /// </summary>
        /// <param name="vertex">The 3D vertex on the unit sphere to convert to cylindrical UV coordinates.</param>
        /// <returns>A Vector2 representing the cylindrical UV coordinates, with longitude [0, 1] and latitude [0, 1].</returns>
        public static Vector2 CylindricalUV(this Vector3 vertex)
        {
            vertex.Normalize();

            // Longitude is the rotation about the north pole/y-axis (0-1)
            float longitude = Mathf.Atan2(vertex.z, vertex.x) * radToRots;
            // Remap -.5->+.5 to 0,1
            longitude += 0.5f;

            // Latitude is degrees from the north pole, towards the south pole (0-1)
            //float latitude = vertex.y * 0.5f + 0.5f;//remap from -1->+1 to 0 -> 1
            float latitude = (Mathf.Asin(vertex.y) * radToRots * 2) + 0.5f;// + 0.25f) * 2f;//remap from -.25->+.25 to 0 -> 1
            return new Vector2(longitude, latitude);
        }

        /// <summary>
        /// Projects a 3D point onto a 2D plane defined by a normal and an origin point.
        /// The resulting 2D point is represented in the plane's local coordinate system.
        /// </summary>
        /// <param name="point">The 3D point to project onto the plane.</param>
        /// <param name="planeNormal">The normal vector defining the plane.</param>
        /// <param name="planeOrigin">A point on the plane used to define its position.</param>
        /// <returns>A 2D vector representing the projected point in the plane's local coordinate system.</returns>
        public static Vector2 ProjectPointOntoPlane(this Vector3 point, Vector3 planeNormal, Vector3 planeOrigin)
        {
            // Ensure planeNormal is not a zero vector
            if (planeNormal == Vector3.zero) throw new System.ArgumentException("ProjectPointOntoPlane failed: Plane normal cannot be zero.");

            Vector3 planeXAxis;
            Vector3 planeYAxis;

            if (planeNormal.CloseEqual(Vector3.up))
            {
                planeXAxis = Vector3.right;
                planeYAxis = Vector3.forward;  // Use a standard axis when normal is up
            }
            else
            {
                // Compute both axes with one cross product
                planeXAxis = Vector3.Cross(Vector3.up, planeNormal).normalized;
                planeYAxis = Vector3.Cross(planeNormal, planeXAxis);  // Only this second cross is needed
            }

            Vector3 offset = point - planeOrigin;
            // Project the point onto the plane
            Vector2 planePoint = new Vector2(
                Vector3.Dot(offset, planeXAxis),
                Vector3.Dot(offset, planeYAxis));

            return planePoint;
        }

        public class Vector3CloseComparer : IEqualityComparer<Vector3>
        {
            private readonly float tolerance;

            public Vector3CloseComparer(float tolerance = 0.0001f)
            {
                this.tolerance = tolerance;
            }

            public bool Equals(Vector3 v1, Vector3 v2)
            {
                return v1.CloseEqual(v2, tolerance);
            }

            /// <summary>
            /// Computes a hash code for a Vector3 based on a tolerance value for approximate equality.
            /// This method scales each component by the tolerance to create hash "buckets" for similar values.
            /// 
            /// Note:
            /// - Different Vector3 values may produce the same hash (collisions), which reduces efficiency.
            /// - Vector3 values considered equal by CloseEqual must produce the same hash for correctness.
            /// </summary>
            /// <param name="v">The Vector3 instance for which to compute the hash code.</param>
            /// <returns>An integer hash code that reflects the approximate equality of the Vector3.</returns>
            public int GetHashCode(Vector3 v)
            {
                // Scale components down by tolerance to bucket similar values
                int hashX = Mathf.RoundToInt(v.x / tolerance);
                int hashY = Mathf.RoundToInt(v.y / tolerance);
                int hashZ = Mathf.RoundToInt(v.z / tolerance);

                // Combine component hashes (e.g., using a tuple hash pattern)
                int hash = hashX;
                hash = (hash * 397) ^ hashY;
                hash = (hash * 397) ^ hashZ;
                return hash;
            }

        }

    }
}