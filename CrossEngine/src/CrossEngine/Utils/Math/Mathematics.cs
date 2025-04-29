using System;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using CrossEngine.Logging;
using System.Runtime.Versioning;

namespace CrossEngine.Utils
{
    static public class MathExtension
    {
        public const float ToDegConstF = 180.0f / MathF.PI;
        public const double ToDegConst = 180.0 / MathF.PI;
        public const float ToRadConstF = MathF.PI / 180.0f;
        public const double ToRadConst = MathF.PI / 180.0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float ToRadians(float degrees)
        {
            return ToRadConstF * degrees;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float ToDegrees(float radians)
        {
            return ToDegConstF * radians;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public double ToRadians(double degrees)
        {
            return ToRadConst * degrees;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public double ToDegrees(double radians)
        {
            return ToDegConst * radians;
        }

        public static float Lerp(float first, float second, float amount)
        {
            return first * (1 - amount) + second * amount;
        }

        public static bool Compare(float a, float b, float precision = float.Epsilon)
        {
            return Math.Abs(a - b) <= precision * Math.Max(1.0f, Math.Max(Math.Abs(a), Math.Abs(b)));
        }
    }

    static class Vector2Extension
    {
        public static Vector2 Rotate(Vector2 vec, float angleRad)
        {
            float cos = MathF.Cos(angleRad);
            float sin = MathF.Sin(angleRad);

            float xPrime = vec.X * cos - vec.Y * sin;
            float yPrime = vec.X * sin - vec.Y * cos;

            vec.X = xPrime;
            vec.Y = yPrime;

            return vec;
        }
        public static Vector2 RotateAroundOrigin(Vector2 vec, float angleRad, Vector2 origin)
        {
            float x = vec.X - origin.X;
            float y = vec.Y - origin.Y;

            float cos = MathF.Cos(angleRad);
            float sin = MathF.Sin(angleRad);

            vec.X = (x * cos + y * sin) + origin.X;
            vec.Y = (x * sin - y * cos) + origin.Y;

            return vec;
        }

        public static bool Compare(Vector2 a, Vector2 b, float precision = float.Epsilon)
        {
            return MathExtension.Compare(a.X, b.X, precision) && MathExtension.Compare(a.Y, b.Y, precision);
        }
    }

    public static class Vector3Extension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 XY(this Vector3 v) => new Vector2(v.X, v.Y);

        static Random random = new Random();

        // vectors between 1 and -1;

        public static Vector3 RandomSphere()
        {
            return Vector3.Normalize(new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()) - new Vector3(0.5f));
        }

        public static Vector3 RandomSphereVolume()
        {
            return Vector3.Normalize(new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()) - new Vector3(0.5f)) * (float)random.NextDouble();
        }

        public static Vector3 RandomCube()
        {
            return (new Vector3(random.Next(0, 2), random.Next(0, 2), random.Next(0, 2)) - new Vector3(0.5f)) * 2;
        }

        public static Vector3 RandomCubeVolume()
        {
            return (new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()) - new Vector3(0.5f)) * 2;
        }
    }

    public static class Vector4Extension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 XYZ(this Vector4 v) => new Vector3(v.X, v.Y, v.Z);

        public static Vector4 FromColor(System.Drawing.Color color)
        {
            return new Vector4((float)color.R / byte.MaxValue, (float)color.G / byte.MaxValue, (float)color.B / byte.MaxValue, (float)color.A / byte.MaxValue);
        }
    }

    public static class Matrix4x4Extension
    {
        //public static Matrix4x4 CreateShearX(float value)
        //{
        //    Matrix4x4 shear = Matrix4x4.Identity;
        //    shear.M21 = value;
        //    return shear;
        //}
        //public static Matrix4x4 CreateShearY(float value)
        //{
        //    Matrix4x4 shear = Matrix4x4.Identity;
        //    shear.M32 = value;
        //    return shear;
        //}
        //public static Matrix4x4 CreateShearZ(float value)
        //{
        //    Matrix4x4 shear = Matrix4x4.Identity;
        //    shear.M23 = value;
        //    return shear;
        //}

        public static Matrix4x4 CreateBillboard(Vector3 right, Vector3 up, Vector3 look, Vector3 pos)
        {
            Matrix4x4 matrix = new Matrix4x4();

            //   2 and 1 can be swapped to change the noraml
            //      |
            //      |
            matrix.M21 = right.X;
            matrix.M22 = right.Y;
            matrix.M23 = right.Z;
            matrix.M24 = 0;

            matrix.M11 = up.X;
            matrix.M12 = up.Y;
            matrix.M13 = up.Z;
            matrix.M14 = 0;

            matrix.M31 = look.X;
            matrix.M32 = look.Y;
            matrix.M33 = look.Z;
            matrix.M34 = 0;

            matrix.M41 = pos.X;
            matrix.M42 = pos.Y;
            matrix.M43 = pos.Z;
            matrix.M44 = 1;

            return matrix;
        }

        [Obsolete("opengl specific")]
        public static Matrix4x4 CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
        {
            // diary time [8.4.2025]: today i learned that this was crazily broken...
            // https://en.wikipedia.org/wiki/Orthographic_projection
            // also i'm not donating shit
            Matrix4x4 result = Matrix4x4.Identity;

            result.M11 = 2.0f / width;
            result.M22 = 2.0f / height;
            // this is one of the magical bugs of the classical stndard System.Numerics.Matrix4x4 struct that makes it non-viable for opengl
            // the fix is 2.0f instead of 1.0f
            result.M33 = 2.0f / (zNearPlane - zFarPlane);
            result.M43 = (zFarPlane + zNearPlane) / (zFarPlane - zNearPlane);

            return result;
        }

        [Obsolete("opengl specific")]
        public static Matrix4x4 CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
        {
            Matrix4x4 result = Matrix4x4.Identity;

            result.M11 = 2.0f / (right - left);

            result.M22 = 2.0f / (top - bottom);

            // same here
            result.M33 = 2.0f / (zNearPlane - zFarPlane);

            result.M41 = (left + right) / (left - right);
            result.M42 = (top + bottom) / (bottom - top);
            result.M43 = zNearPlane / (zNearPlane - zFarPlane);

            return result;
        }

        // does not throw
        public static Matrix4x4 CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            /*
            if (fieldOfView <= 0.0f || fieldOfView >= MathF.PI)
                throw new ArgumentOutOfRangeException(nameof(fieldOfView));

            if (nearPlaneDistance <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

            if (farPlaneDistance <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));

            if (nearPlaneDistance >= farPlaneDistance)
                throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));
            */

            float yScale = 1.0f / MathF.Tan(fieldOfView * 0.5f);
            float xScale = yScale / aspectRatio;

            Matrix4x4 result;

            result.M11 = xScale;
            result.M12 = result.M13 = result.M14 = 0.0f;

            result.M22 = yScale;
            result.M21 = result.M23 = result.M24 = 0.0f;

            result.M31 = result.M32 = 0.0f;
            float negFarRange = float.IsPositiveInfinity(farPlaneDistance) ? -1.0f : farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M33 = negFarRange;
            result.M34 = -1.0f;

            result.M41 = result.M42 = result.M44 = 0.0f;
            result.M43 = nearPlaneDistance * negFarRange;

            return result;
        }

        /*
        public static Matrix4x4 Ortho(float left, float right, float bottom, float top)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.M11 = 2 / (right - left);
            matrix.M22 = 2 / (top - bottom);
            matrix.M33 = 1;
            matrix.M41 = -(right + left) / (right - left);
            matrix.M42 = -(top + bottom) / (top - bottom);
            return matrix;
        }
        public static Matrix4x4 Ortho(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            Matrix4x4 matrix = Matrix4x4.Identity;
            matrix.M11 = 2 / (right - left);
            matrix.M22 = 2 / (top - bottom);
            matrix.M33 = 2 / (zFar - zNear);
            matrix.M41 = -(right + left) / (right - left);
            matrix.M42 = -(top + bottom) / (top - bottom);
            matrix.M43 = -(zFar + zNear) / (zFar - zNear);
            //matrix.M44 = 1;
            return matrix;
        }

        // System.Matrix4x4.CreatePerspectiveFieldOfView accounts for LH and negative Z
        static public Matrix4x4 Perspective(float fovyRad, float aspect, float zNear, float zFar)
        {
            Debug.Assert(aspect > 0);
            Debug.Assert(fovyRad > 0);

            float h = MathF.Cos(0.5f * fovyRad) / MathF.Sin(0.5f * fovyRad);
            float w = h * 1 / aspect;

            Matrix4x4 result = new Matrix4x4();
            result.M11 = w;
            result.M22 = h;
            result.M33 = (zFar + zNear) / (zFar - zNear);
            result.M34 = 1;
            result.M43 = -(2 * zFar * zNear) / (zFar - zNear);
            return result;
        }
        */

        static public Matrix4x4 SafeInvert(in Matrix4x4 matrix)
        {
            if (Matrix4x4.Invert(matrix, out Matrix4x4 result))
                return result;
            
            Log.Default.Warn("matrix is not invertible");
            return Matrix4x4.Identity;
        }

        static public void EulerDecompose(out Vector3 translation, out Vector3 rotation, out Vector3 scale, Matrix4x4 matrix)
        {
            Vector4 matvright = new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14);
            Vector4 matvup = new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M34);
            Vector4 matvdir = new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34);
            Vector4 matvposition = new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44);

            scale.X = matvright.Length();
            scale.Y = matvup.Length();
            scale.Z = matvdir.Length();

            //mat.OrthoNormalize();
            matvright = Vector4.Normalize(matvright);
            matvup = Vector4.Normalize(matvup);
            matvdir = Vector4.Normalize(matvdir);

            rotation.X = MathF.Atan2(matrix.M23, matrix.M33);
            rotation.Y = MathF.Atan2(-matrix.M13, MathF.Sqrt(matrix.M23 * matrix.M23 + matrix.M33 * matrix.M33));
            rotation.Z = MathF.Atan2(matrix.M12, matrix.M11);

            translation.X = matvposition.X;
            translation.Y = matvposition.Y;
            translation.Z = matvposition.Z;
        }

        public static bool HasNaNElement(Matrix4x4 matrix)
        {
            // faster than using pointer in loop
            return
                matrix.M11 is float.NaN ||
                matrix.M12 is float.NaN ||
                matrix.M13 is float.NaN ||
                matrix.M14 is float.NaN ||
                matrix.M21 is float.NaN ||
                matrix.M22 is float.NaN ||
                matrix.M23 is float.NaN ||
                matrix.M24 is float.NaN ||
                matrix.M31 is float.NaN ||
                matrix.M32 is float.NaN ||
                matrix.M33 is float.NaN ||
                matrix.M34 is float.NaN ||
                matrix.M41 is float.NaN ||
                matrix.M42 is float.NaN ||
                matrix.M43 is float.NaN ||
                matrix.M44 is float.NaN;
        }

        /*
        public static bool Decompose(Matrix4x4 ModelMatrix,
            out Vector3 Scale,
            out Quaternion Orientation,
            out Vector3 Translation,
            out Vector3 Skew,
            out Vector4 Perspective)
        {
            bool EpsilonEqual(float x, float y, float epsilon)
            {
                return Math.Abs(x - y) < epsilon;
            }

            bool EpsilonNotEqual(float x, float y, float epsilon)
            {
                return Math.Abs(x - y) >= epsilon;
            }

            float Determinant(Matrix4x4 matrix)
            {
                float DeterminantIn(float[,] matrix)
                {
                    float[,] fillNewArr(float[,] originalArr, int row, int col)
                    {
                        float[,] tempArray = new float[originalArr.GetLength(0) - 1, originalArr.GetLength(1) - 1];

                        for (int i = 0, newRow = 0; i < originalArr.GetLength(0); i++)
                        {
                            if (i == row)
                                continue;
                            for (int j = 0, newCol = 0; j < originalArr.GetLength(1); j++)
                            {
                                if (j == col) continue;
                                tempArray[newRow, newCol] = originalArr[i, j];

                                newCol++;
                            }
                            newRow++;
                        }
                        return tempArray;
                    }

                    float det = 0;
                    float total = 0;
                    float[,] tempArr = new float[array.GetLength(0) - 1, array.GetLength(1) - 1];

                    if (array.GetLength(0) == 2)
                    {
                        det = array[0, 0] * array[1, 1] - array[0, 1] * array[1, 0];
                    }

                    else
                    {

                        for (int i = 0; i < 1; i++)
                        {
                            for (int j = 0; j < array.GetLength(1); j++)
                            {
                                if (j % 2 != 0) array[i, j] = array[i, j] * -1;
                                tempArr = fillNewArr(array, i, j);
                                det += DeterminantIn(tempArr);
                                total = total + (det * array[i, j]);
                            }
                        }
                    }
                    return det;
                }

                float[,] array = new float[,]
                {
                    { matrix.M11, matrix.M12, matrix.M13, matrix.M14 },
                    { matrix.M21, matrix.M22, matrix.M23, matrix.M24 },
                    { matrix.M31, matrix.M32, matrix.M33, matrix.M34 },
                    { matrix.M41, matrix.M42, matrix.M43, matrix.M44 },
                };

                return DeterminantIn(array);
            }

            

            Scale = Vector3.One;
            Orientation = Quaternion.Identity;
            Translation = Vector3.Zero;
            Skew = Vector3.Zero;
            Perspective = Vector4.Zero;


            Matrix4x4 LocalMatrix = ModelMatrix;

            // Normalize the matrix.
            if(EpsilonEqual(LocalMatrix.M44, 0, float.Epsilon))
                return false;

            LocalMatrix.M11 /= LocalMatrix.M44;
            LocalMatrix.M12 /= LocalMatrix.M44;
            LocalMatrix.M13 /= LocalMatrix.M44;
            LocalMatrix.M14 /= LocalMatrix.M44;
            LocalMatrix.M21 /= LocalMatrix.M44;
            LocalMatrix.M22 /= LocalMatrix.M44;
            LocalMatrix.M23 /= LocalMatrix.M44;
            LocalMatrix.M24 /= LocalMatrix.M44;
            LocalMatrix.M31 /= LocalMatrix.M44;
            LocalMatrix.M32 /= LocalMatrix.M44;
            LocalMatrix.M33 /= LocalMatrix.M44;
            LocalMatrix.M34 /= LocalMatrix.M44;
            LocalMatrix.M41 /= LocalMatrix.M44;
            LocalMatrix.M42 /= LocalMatrix.M44;
            LocalMatrix.M43 /= LocalMatrix.M44;
            LocalMatrix.M44 /= LocalMatrix.M44;

            // perspectiveMatrix is used to solve for perspective, but it also provides
            // an easy way to test for singularity of the upper 3x3 component.
            Matrix4x4 PerspectiveMatrix = LocalMatrix;

            PerspectiveMatrix.M14 = 0;
            PerspectiveMatrix.M24 = 0;
            PerspectiveMatrix.M34 = 0;
            PerspectiveMatrix.M44 = 1;

            /// TODO: Fixme!
            if(EpsilonEqual(Determinant(PerspectiveMatrix), 0, float.Epsilon))
                return false;

            // First, isolate perspective.  This is the messiest.
            if(
                EpsilonNotEqual(LocalMatrix.M14, 0, float.Epsilon) ||
                EpsilonNotEqual(LocalMatrix.M24, 0, float.Epsilon) ||
                EpsilonNotEqual(LocalMatrix.M34, 0, float.Epsilon))
            {
                // rightHandSide is the right hand side of the equation.
                vec<4, T, Q> RightHandSide;
                RightHandSide[0] = LocalMatrix[0][3];
                RightHandSide[1] = LocalMatrix[1][3];
                RightHandSide[2] = LocalMatrix[2][3];
                RightHandSide[3] = LocalMatrix[3][3];

                // Solve the equation by inverting PerspectiveMatrix and multiplying
                // rightHandSide by the inverse.  (This is the easiest way, not
                // necessarily the best.)
                mat<4, 4, T, Q> InversePerspectiveMatrix = glm::inverse(PerspectiveMatrix);//   inverse(PerspectiveMatrix, inversePerspectiveMatrix);
                mat<4, 4, T, Q> TransposedInversePerspectiveMatrix = glm::transpose(InversePerspectiveMatrix);//   transposeMatrix4(inversePerspectiveMatrix, transposedInversePerspectiveMatrix);

                Perspective = TransposedInversePerspectiveMatrix * RightHandSide;
                //  v4MulPointByMatrix(rightHandSide, transposedInversePerspectiveMatrix, perspectivePoint);

                // Clear the perspective partition
                LocalMatrix[0][3] = LocalMatrix[1][3] = LocalMatrix[2][3] = static_cast<T>(0);
                LocalMatrix[3][3] = static_cast<T>(1);
            }
            else
            {
                // No perspective.
                Perspective = vec<4, T, Q>(0, 0, 0, 1);
            }

            // Next take care of translation (easy).
            Translation = vec<3, T, Q>(LocalMatrix[3]);
            LocalMatrix[3] = vec<4, T, Q>(0, 0, 0, LocalMatrix[3].w);

            vec<3, T, Q> Row[3], Pdum3;

            // Now get scale and shear.
            for(length_t i = 0; i < 3; ++i)
            for(length_t j = 0; j < 3; ++j)
                Row[i][j] = LocalMatrix[i][j];

            // Compute X scale factor and normalize first row.
            Scale.x = length(Row[0]);// v3Length(Row[0]);

            Row[0] = detail::scale(Row[0], static_cast<T>(1));

            // Compute XY shear factor and make 2nd row orthogonal to 1st.
            Skew.z = dot(Row[0], Row[1]);
            Row[1] = detail::combine(Row[1], Row[0], static_cast<T>(1), -Skew.z);

            // Now, compute Y scale and normalize 2nd row.
            Scale.y = length(Row[1]);
            Row[1] = detail::scale(Row[1], static_cast<T>(1));
            Skew.z /= Scale.y;

            // Compute XZ and YZ shears, orthogonalize 3rd row.
            Skew.y = glm::dot(Row[0], Row[2]);
            Row[2] = detail::combine(Row[2], Row[0], static_cast<T>(1), -Skew.y);
            Skew.x = glm::dot(Row[1], Row[2]);
            Row[2] = detail::combine(Row[2], Row[1], static_cast<T>(1), -Skew.x);

            // Next, get Z scale and normalize 3rd row.
            Scale.z = length(Row[2]);
            Row[2] = detail::scale(Row[2], static_cast<T>(1));
            Skew.y /= Scale.z;
            Skew.x /= Scale.z;

            // At this point, the matrix (in rows[]) is orthonormal.
            // Check for a coordinate system flip.  If the determinant
            // is -1, then negate the matrix and the scaling factors.
            Pdum3 = cross(Row[1], Row[2]); // v3Cross(row[1], row[2], Pdum3);
            if(dot(Row[0], Pdum3) < 0)
            {
                for(length_t i = 0; i < 3; i++)
                {
                    Scale[i] *= static_cast<T>(-1);
                    Row[i] *= static_cast<T>(-1);
                }
            }

            // Now, get the rotations out, as described in the gem.

            // FIXME - Add the ability to return either quaternions (which are
            // easier to recompose with) or Euler angles (rx, ry, rz), which
            // are easier for authors to deal with. The latter will only be useful
            // when we fix https://bugs.webkit.org/show_bug.cgi?id=23799, so I
            // will leave the Euler angle code here for now.

            // ret.rotateY = asin(-Row[0][2]);
            // if (cos(ret.rotateY) != 0) {
            //     ret.rotateX = atan2(Row[1][2], Row[2][2]);
            //     ret.rotateZ = atan2(Row[0][1], Row[0][0]);
            // } else {
            //     ret.rotateX = atan2(-Row[2][0], Row[1][1]);
            //     ret.rotateZ = 0;
            // }

            int i, j, k = 0;
            T root, trace = Row[0].x + Row[1].y + Row[2].z;
            if(trace > static_cast<T>(0))
            {
                root = sqrt(trace + static_cast<T>(1.0));
                Orientation.w = static_cast<T>(0.5) * root;
                root = static_cast<T>(0.5) / root;
                Orientation.x = root * (Row[1].z - Row[2].y);
                Orientation.y = root * (Row[2].x - Row[0].z);
                Orientation.z = root * (Row[0].y - Row[1].x);
            } // End if > 0
            else
            {
                static int Next[3] = {1, 2, 0};
                i = 0;
                if(Row[1].y > Row[0].x) i = 1;
                if(Row[2].z > Row[i][i]) i = 2;
                j = Next[i];
                k = Next[j];

#               ifdef GLM_FORCE_QUAT_DATA_XYZW
                    int off = 0;
#               else
                    int off = 1;
#               endif

                root = sqrt(Row[i][i] - Row[j][j] - Row[k][k] + static_cast<T>(1.0));

                Orientation[i + off] = static_cast<T>(0.5) * root;
                root = static_cast<T>(0.5) / root;
                Orientation[j + off] = root * (Row[i][j] + Row[j][i]);
                Orientation[k + off] = root * (Row[i][k] + Row[k][i]);
                Orientation.w = root * (Row[j][k] - Row[k][j]);
            } // End if <= 0

            return true;
        }
        */
    }

    public static class QuaternionExtension
    {
        public static Vector3 ToEuler(Quaternion q)
        {
            // black box content provided by:
            // https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles

            Vector3 angles = new Vector3(0.0f);

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp); // use 90 degrees if out of range
            else
                angles.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        //public static Quaternion ToQuaternion(Vector3 euler)
        //{
        //    (float yaw, float pitch, float roll) = (euler.X, euler.X, euler.Z);
        //    float qx = MathF.Sin(roll / 2) * MathF.Cos(pitch / 2) * MathF.Cos(yaw / 2) - MathF.Cos(roll / 2) * MathF.Sin(pitch / 2) * MathF.Sin(yaw / 2);
        //    float qy = MathF.Cos(roll / 2) * MathF.Sin(pitch / 2) * MathF.Cos(yaw / 2) + MathF.Sin(roll / 2) * MathF.Cos(pitch / 2) * MathF.Sin(yaw / 2);
        //    float qz = MathF.Cos(roll / 2) * MathF.Cos(pitch / 2) * MathF.Sin(yaw / 2) - MathF.Sin(roll / 2) * MathF.Sin(pitch / 2) * MathF.Cos(yaw / 2);
        //    float qw = MathF.Cos(roll / 2) * MathF.Cos(pitch / 2) * MathF.Cos(yaw / 2) + MathF.Sin(roll / 2) * MathF.Sin(pitch / 2) * MathF.Sin(yaw / 2);
        //    return new Quaternion(qx, qy, qz, qw);
        //
        //    float cy = MathF.Cos(euler.X * 0.5f);
        //    float sy = MathF.Sin(euler.X * 0.5f);
        //    float cp = MathF.Cos(euler.Y * 0.5f);
        //    float sp = MathF.Sin(euler.Y * 0.5f);
        //    float cr = MathF.Cos(euler.Z * 0.5f);
        //    float sr = MathF.Sin(euler.Z * 0.5f);
        //    Quaternion q;
        //    q.W = (float)(cr * cp * cy + sr * sp * sy);
        //    q.X = (float)(sr * cp * cy - cr * sp * sy);
        //    q.Y = (float)(cr * sp * cy + sr * cp * sy);
        //    q.Z = (float)(cr * cp * sy - sr * sp * cy);
        //    return q;
        //}

        public static Quaternion RotateX(float angle)
        {
            return Quaternion.CreateFromAxisAngle(Vector3.UnitX, angle);
        }
        public static Quaternion RotateY(float angle)
        {
            return Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
        }
        public static Quaternion RotateZ(float angle)
        {
            return Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);
        }

        public static Quaternion RotateXYZ(float x, float y, float z)
        {
            // pitch yaw roll
            return RotateX(x) * RotateY(y) * RotateZ(z);
        }

        public static Quaternion RotateXYZ(Vector3 vec)
        {
            return RotateX(vec.X) * RotateY(vec.Y) * RotateZ(vec.Z);
        }
    }

    // todo: merge with VecColor
    public static class ColorHelper
    {
        //public static Vector3 RGBFromUInt(uint color)
        //{
        //    return new Vector3((float)(byte)color / 255, (float)(byte)(color >> 8) / 255, (float)(byte)(color >> 16) / 255);
        //}
        //
        //public static Vector4 RGBAFromUInt(uint color)
        //{
        //    return new Vector4((float)(byte)color / 255, (float)(byte)(color >> 8) / 255, (float)(byte)(color >> 16) / 255, (float)(byte)(color >> 24) / 255);
        //}

        public static Vector3 HSVToRGB(Vector3 hsvColor)
        {
            //while (hsvColor.X < 0) { hsvColor.X += 360; };
            //while (hsvColor.X >= 360) { hsvColor.X -= 360; };
            hsvColor.X = (hsvColor.X % 1.0f);
            if (hsvColor.X < 0)
                hsvColor.X += 1;
            hsvColor.X *= 6;
            Vector3 rgbColor = new Vector3();
            if (hsvColor.Z <= 0)
            {
                rgbColor.X = rgbColor.Y = rgbColor.Z = 0;
            }
            else if (hsvColor.Y <= 0)
            {
                rgbColor.X = rgbColor.Y = rgbColor.Z = hsvColor.Z;
            }
            else
            {
                float hf = hsvColor.X;
                int i = (int)Math.Floor(hf);
                float f = hf - i;
                float pv = hsvColor.Z * (1 - hsvColor.Y);
                float qv = hsvColor.Z * (1 - hsvColor.Y * f);
                float tv = hsvColor.Z * (1 - hsvColor.Y * (1 - f));
                switch (i)
                {
                    case 0:
                        rgbColor.X = hsvColor.Z;
                        rgbColor.Y = tv;
                        rgbColor.Z = pv;
                        break;
                    case 1:
                        rgbColor.X = qv;
                        rgbColor.Y = hsvColor.Z;
                        rgbColor.Z = pv;
                        break;
                    case 2:
                        rgbColor.X = pv;
                        rgbColor.Y = hsvColor.Z;
                        rgbColor.Z = tv;
                        break;
                    case 3:
                        rgbColor.X = pv;
                        rgbColor.Y = qv;
                        rgbColor.Z = hsvColor.Z;
                        break;
                    case 4:
                        rgbColor.X = tv;
                        rgbColor.Y = pv;
                        rgbColor.Z = hsvColor.Z;
                        break;
                    case 5:
                        rgbColor.X = hsvColor.Z;
                        rgbColor.Y = pv;
                        rgbColor.Z = qv;
                        break;
                    case 6:
                        rgbColor.X = hsvColor.Z;
                        rgbColor.Y = tv;
                        rgbColor.Z = pv;
                        break;
                    case -1:
                        rgbColor.X = hsvColor.Z;
                        rgbColor.Y = pv;
                        rgbColor.Z = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        Log.Default.Error("color conversion fucked up!");
                        rgbColor.X = rgbColor.Y = rgbColor.Z = hsvColor.Z;
                        break;
                }
            }
            return rgbColor;
        }

        public static Vector3 RGBToHSV(Vector3 rgbColor)
        {
            Vector3 hsvColor = new Vector3();
            float min, max, delta;

            min = rgbColor.X < rgbColor.Y ? rgbColor.X : rgbColor.Y;
            min = min < rgbColor.Z ? min : rgbColor.Z;

            max = rgbColor.X > rgbColor.Y ? rgbColor.X : rgbColor.Y;
            max = max > rgbColor.Z ? max : rgbColor.Z;

            hsvColor.Z = max;
            delta = max - min;

            if (delta < 0.00001f)
            {
                hsvColor.Y = 0.0f;
                hsvColor.X = 0.0f;
                return hsvColor;
            }

            if (max > 0.0f)
            {
                hsvColor.Y = (delta / max);
            }
            else
            {
                hsvColor.Y = 0.0f;
                hsvColor.X = 0.0f;
                return hsvColor;
            }

            if (rgbColor.X >= max)
                hsvColor.X = (rgbColor.Y - rgbColor.Z) / delta;
            else if (rgbColor.Y >= max)
                hsvColor.X = 2.0f + (rgbColor.Z - rgbColor.X) / delta;
            else
                hsvColor.X = 4.0f + (rgbColor.X - rgbColor.Y) / delta;

            hsvColor.X /= 6.0f;

            if (hsvColor.X < 0.0f)
                hsvColor.X += 1.0f;

            return hsvColor;
        }

        // 0xAARRGGBB
        private const int U32_R_SHIFT = 16;
        private const int U32_G_SHIFT = 8;
        private const int U32_B_SHIFT = 0;
        private const int U32_A_SHIFT = 24;

        public static Vector4 U32ToVec4(uint value)
        {
            float s = 1.0f / 255.0f;
            return new Vector4(
                ((value >> U32_R_SHIFT) & 0xFF) * s,
                ((value >> U32_G_SHIFT) & 0xFF) * s,
                ((value >> U32_B_SHIFT) & 0xFF) * s,
                ((value >> U32_A_SHIFT) & 0xFF) * s);
        }

        public static Vector3 U32ToVec3(uint value)
        {
            float s = 1.0f / 255.0f;
            return new Vector3(
                ((value >> U32_R_SHIFT) & 0xFF) * s,
                ((value >> U32_G_SHIFT) & 0xFF) * s,
                ((value >> U32_B_SHIFT) & 0xFF) * s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ColF32ToU8(float v)
        {
            return (int)(Math.Clamp(v, 0, 1) * 255.0f + 0.5f);
        }

        public static uint Vec4ToU32(Vector4 value)
        {
            uint o;
            o  = ((uint)ColF32ToU8(value.X)) << U32_R_SHIFT;
            o |= ((uint)ColF32ToU8(value.Y)) << U32_G_SHIFT;
            o |= ((uint)ColF32ToU8(value.Z)) << U32_B_SHIFT;
            o |= ((uint)ColF32ToU8(value.W)) << U32_A_SHIFT;
            return o;
        }

        public static uint Vec3ToU32(Vector3 value, float alpha = 1f)
        {
            uint o;
            o  = ((uint)ColF32ToU8(value.X)) << U32_R_SHIFT;
            o |= ((uint)ColF32ToU8(value.Y)) << U32_G_SHIFT;
            o |= ((uint)ColF32ToU8(value.Z)) << U32_B_SHIFT;
            o |= ((uint)ColF32ToU8(alpha)) << U32_A_SHIFT;
            return o;
        }
    }
}