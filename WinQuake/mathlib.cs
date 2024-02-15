namespace Quake;

public unsafe class mathlib_c
{
    public static Vector3 vec3_origin = new Vector3(0, 0, 0);
    public int nanmask = 255 << 23;


    public static float DEG2RAD(float a)
    {
        return (a * MathF.PI) / 180.0f;
    }

    public static void ProjectPointOnPlane(Vector3 dst, Vector3 p, Vector3 normal)
    {
        float d;
        Vector3 n;
        float inv_denom;

        inv_denom = 1.0f / DotProduct(normal, normal);

        d = DotProduct(normal, p) * inv_denom;

        n.X = normal.X * inv_denom;
        n.Y = normal.Y * inv_denom;
        n.Z = normal.Z * inv_denom;

        dst.X = p.X - d * n.X;
        dst.Y = p.Y - d * n.Y;
        dst.Z = p.Z - d * n.Z;
    }

    public static void PerpendicularVector(Vector3 dst, Vector3 src)
    {
        int pos;
        int i;
        float minelem = 1.0f;
        Vector3 tempvec;

        for (pos = 0, i = 0; i < 3; i++)
        {
            if (MathF.Abs(src[i]) < minelem)
            {
                pos = i;
                minelem = MathF.Abs(src[i]);
            }
        }

        tempvec.X = tempvec.Y = tempvec.Z = 0.0f;
        tempvec[pos] = 1.0f;

        ProjectPointOnPlane(dst, tempvec, src);

        VectorNormalize(dst);
    }

    public static void RotatePointAroundVector(Vector3 dst, Vector3 dir, Vector3 point, float degrees)
    {
        float[][] m = new float[3][];
        float[][] im = new float[3][];
        float[][] zrot = new float[3][];
        float[][] tmpmat = new float[3][];
        float[][] rot = new float[3][];
        int i;
        Vector3 vr = new Vector3(0), vup = new Vector3(0), vf = new Vector3(0);

        vf.X = dir.X;
        vf.Y = dir.Y;
        vf.Z = dir.Z;

        PerpendicularVector(vr, dir);
        CrossProduct(vr, vf, vup);

        m[0][0] = vr[0];
        m[1][0] = vr[1];
        m[2][0] = vr[2];

        m[0][1] = vup[0];
        m[1][1] = vup[1];
        m[2][1] = vup[2];

        m[0][2] = vf[0];
        m[1][2] = vf[1];
        m[2][2] = vf[2];

        common_c.Q_memcpy(im, m, 6);

        im[0][1] = m[1][0];
        im[0][2] = m[2][0];
        im[1][0] = m[0][1];
        im[1][2] = m[2][1];
        im[2][0] = m[0][2];
        im[2][1] = m[1][2];

        common_c.Q_memset(zrot, 0, 6);
        zrot[0][0] = zrot[1][1] = zrot[2][2] = 1.0f;

        zrot[0][0] = MathF.Cos(DEG2RAD(degrees));
        zrot[0][1] = MathF.Sin(DEG2RAD(degrees));
        zrot[1][0] = -MathF.Sin(DEG2RAD(degrees));
        zrot[1][1] = MathF.Cos(DEG2RAD(degrees));

        R_ConcatRotations(m, zrot, tmpmat);
        R_ConcatRotations(tmpmat, im, rot);

        for (i = 0; i < 3; i++)
        {
            dst[i] = rot[i][0] * point[0] + rot[i][1] * point[1] + rot[i][2] * point[2];
        }
    }


    public static float anglemod(float a)
    {
        //if (a >= 0) 
        //{
        //	a -= 360*(int)(a/360);
        //}
        //else 
        //{
        //	a += 360(1 + (int)(-a/360));
        //}

        a = (360.0f / 65536) * ((int)(a * (65536 / 360.0f)) & 65535);
        return a;
    }

    public static void BOPS_Error()
    {
        sys_win_c.Sys_Error("BoxOnPlaneSide: Bad signbits");
    }

#if !id386
    public static int BOX_ON_PLANE_SIDE(Vector3 emins, Vector3 emaxs, model_c.mplane_t* p)
    {
        float dist1, dist2;
        int sides;

        //if (p->type < 3) 
        //{
        //	if (p->dist <= emins[p->type]) 
        //	{
        //		return 1;
        //	}

        //	if (p->dist >= emaxs[p->type])
        //	{
        //		return 2;
        //	}

        //	return 3;
        //}

        switch (p->signbits)
        {
            case 0:
                dist1 = p->normal[0] * emaxs[0] + p->normal[1] * emaxs[1] + p->normal[2] * emaxs[2];
                dist2 = p->normal[0] * emins[0] + p->normal[1] * emins[1] + p->normal[2] * emins[2];
                break;

            case 1:
                dist1 = p->normal[0] * emins[0] + p->normal[1] * emaxs[1] + p->normal[2] * emaxs[2];
                dist2 = p->normal[0] * emaxs[0] + p->normal[1] * emins[1] + p->normal[2] * emins[2];
                break;

            case 2:
                dist1 = p->normal[0] * emaxs[0] + p->normal[1] * emins[1] + p->normal[2] * emaxs[2];
                dist2 = p->normal[0] * emins[0] + p->normal[1] * emaxs[1] + p->normal[2] * emins[2];
                break;

            case 3:
                dist1 = p->normal[0] * emins[0] + p->normal[1] * emins[1] + p->normal[2] * emaxs[2];
                dist2 = p->normal[0] * emaxs[0] + p->normal[1] * emaxs[1] + p->normal[2] * emins[2];
                break;

            case 4:
                dist1 = p->normal[0] * emaxs[0] + p->normal[1] * emaxs[1] + p->normal[2] * emins[2];
                dist2 = p->normal[0] * emins[0] + p->normal[1] * emins[1] + p->normal[2] * emaxs[2];
                break;

            case 5:
                dist1 = p->normal[0] * emins[0] + p->normal[1] * emaxs[1] + p->normal[2] * emins[2];
                dist2 = p->normal[0] * emaxs[0] + p->normal[1] * emins[1] + p->normal[2] * emaxs[2];
                break;

            case 6:
                dist1 = p->normal[0] * emaxs[0] + p->normal[1] * emins[1] + p->normal[2] * emins[2];
                dist2 = p->normal[0] * emins[0] + p->normal[1] * emaxs[1] + p->normal[2] * emaxs[2];
                break;

            case 7:
                dist1 = p->normal[0] * emins[0] + p->normal[1] * emins[1] + p->normal[2] * emins[2];
                dist2 = p->normal[0] * emaxs[0] + p->normal[1] * emaxs[1] + p->normal[2] * emaxs[2];
                break;

            default:
                dist1 = dist2 = 0;
                BOPS_Error();
                break;
        }

        //int i;
        //Vector3[] corners = new Vector3[2];

        //for (i = 0; i< 3; i++)
        //{
        //	if (plane->normal[i] < 0)
        //	{
        //		corners[0][i] = emins[i];
        //		corners[1][i] = emaxs[i];
        //	}
        //	else
        //	{
        //		corners[1][i] = emins[i];
        //		corners[0][i] = emaxs[i];
        //	}
        //}

        //dist = DotProduct(plane->normal, corners[0]) - plane->dist;
        //dist2 = DotProduct(plane->normal, corners[1]) - plane->dist;
        //sides = 0;

        //if (dist1 >= 0)
        //{
        //	sides = 1;
        //}

        //if (dist2 < 0)
        //{
        //	sides |= 2;
        //}

        sides = 0;

        if (dist1 >= p->dist)
        {
            sides = 1;
        }

        if (dist2 < p->dist)
        {
            sides |= 2;
        }

#if PARANOID
		if (sides == 0) 
		{
			sys_win_c.Sys_Error("BoxOnPlaneSide: sides == 0");
		}
#endif

        return sides;
    }
#endif

    public static void AngleVectors(Vector3 angles, Vector3 forward, Vector3 right, Vector3 up)
    {
        float angle;
        float sr, sp, sy, cr, cp, cy;

        angle = angles[quakedef_c.YAW] * (MathF.PI * 2 / 360);
        sy = MathF.Sin(angle);
        cy = MathF.Cos(angle);
        angle = angles[quakedef_c.PITCH] * (MathF.PI * 2 / 360);
        sp = MathF.Sin(angle);
        cp = MathF.Cos(angle);
        angle = angles[quakedef_c.ROLL] * (MathF.PI * 2 / 360);
        sr = MathF.Sin(angle);
        cr = MathF.Cos(angle);

        forward[0] = cp * cy;
        forward[1] = cp * sy;
        forward[2] = -sp;
        right[0] = (-1 * sr * sp * cy + -1 * cr * -sy);
        right[1] = (-1 * sr * sp * sy + -1 * cr * cy);
        right[2] = -1 * sr * cp;
        up[0] = (cr * sp * cy + -sr * -sy);
        up[1] = (cr * sp * sy + -sr * cy);
        up[2] = cr * cp;
    }

    public static int VectorCompare(Vector3 v1, Vector3 v2)
    {
        int i;

        for (i = 0; i < 3; i++)
        {
            if (v1[i] != v2[i])
            {
                return 0;
            }
        }

        return 1;
    }

    public static void VectorMA(Vector3 veca, float scale, Vector3 vecb, Vector3 vecc)
    {
        vecc[0] = veca[0] + scale * vecb[0];
        vecc[1] = veca[1] + scale * vecb[1];
        vecc[2] = veca[2] + scale * vecb[2];
    }

    public static float DotProduct(float[] a, float[] b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
    }

    public static float DotProduct(Vector3 a, Vector3 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    public static float DotProduct(float[] a, Vector3 b)
    {
        return a[0] * b.X + a[1] * b.Y + a[2] * b.Z;
    }

    public static float DotProduct(float* a, Vector3 b)
    {
        return a[0] * b.X + a[1] * b.Y + a[2] * b.Z;
    }

    public static float DotProduct(Vector3 a, float* b)
    {
        return a.X + b[0] + a.Y + b[1] + a.Z + b[2];
    }

    public static float DotProduct(float* a, float* b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
    }

    public static float DotProduct(Vector3 a, float[] b)
    {
        return a.X * b[0] + a.Y * b[1] + a.Z * b[2];
    }

    public static float DotProduct(byte[] a, float* b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
    }

    public static void VectorSubtract(Vector3 veca, Vector3 vecb, Vector3 output)
    {
        output[0] = veca[0] - vecb[0];
        output[1] = veca[1] - vecb[1];
        output[2] = veca[2] - vecb[2];
    }

    public static void VectorSubtract(float* f, Vector3 vec, Vector3 output)
    {
        output[0] = f[0] - vec[0];
        output[1] = f[1] - vec[1];
        output[2] = f[2] - vec[2];
    }

    public static void VectorAdd(Vector3 veca, Vector3 vecb, Vector3 output)
    {
        output[0] = veca[0] + vecb[0];
        output[1] = veca[1] + vecb[1];
        output[2] = veca[2] + vecb[2];
    }

    #region VectorCopy

    public static void VectorCopy(Vector3 input, Vector3 output)
    {
        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];
    }

    public static void VectorCopy(float* input, Vector3 output)
    {
        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];
    }

    public static void VectorCopy(float[] input, Vector3 output)
    {
        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];
    }

    public static void VectorCopy(Vector3 input, float* output)
    {
        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];
    }

    public static void VectorCopy(Vector3 input, float[] output)
    {
        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];
    }

    public static void VectorCopy(float* input, float* output)
    {
        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];
    }

    public static void VectorCopy(float[] input, float[] output)
    {
        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];
    }

    public static void VectorCopy(float[] input, float* output)
    {
        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];
    }

    public static void VectorCopy(float* input, float[] output)
    {
        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];
    }

    #endregion

    public static float* VecToFloatPtr(Vector3 input)
    {
        float* output = null;

        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];

        return output;
    }

    public static Vector3 FloatPtrToVec(float* input)
    {
        Vector3 output = new();

        output[0] = input[0];
        output[1] = input[1];
        output[2] = input[2];

        return output;
    }

    public static void CrossProduct(Vector3 v1, Vector3 v2, Vector3 cross)
    {
        cross[0] = v1[1] * v2[2] - v1[2] * v2[1];
        cross[1] = v1[2] * v2[0] - v1[0] * v2[2];
        cross[2] = v1[0] * v2[1] - v1[1] * v2[0];
    }

    public static double sqrt(double x)
    {
        return MathF.Sqrt((float)x);
    }

    public static float Length(Vector3 v)
    {
        int i;
        float length;

        length = 0;

        for (i = 0; i < 3; i++)
        {
            length += v[i] * v[i];
        }

        length = (float)sqrt(length);

        return length;
    }

    public static float Length(float[] f)
    {
        int i;
        float length;

        length = 0;

        for (i = 0; i < 3; i++)
        {
            length += f[i] * f[i];
        }

        length = (float)sqrt(length);

        return length;
    }

    public static float Length(float* f)
    {
        int i;
        float length;

        length = 0;

        for (i = 0; i < 3; i++)
        {
            length += f[i] * f[i];
        }

        length = (float)sqrt(length);

        return length;
    }

    public static float VectorNormalize(Vector3 v)
    {
        float length, ilength;

        length = v[0] * v[0] + v[1] * v[1] + v[2] * v[2];
        length = (float)sqrt(length);

        if (length != 0)
        {
            ilength = 1 / length;
            v[0] *= ilength;
            v[1] *= ilength;
            v[2] *= ilength;
        }

        return length;
    }

    public static void VectorInverse(Vector3 v)
    {
        v[0] = -v[0];
        v[1] = -v[1];
        v[2] = -v[2];
    }

    public static void VectorInverse(float* v)
    {
        v[0] = -v[0];
        v[1] = -v[1];
        v[2] = -v[2];
    }

    public static void VectorScale(Vector3 input, float scale, Vector3 output)
    {
        output[0] = input[0] * scale;
        output[1] = input[1] * scale;
        output[2] = input[2] * scale;
    }

    public static int Q_Log2(int val)
    {
        int answer = 0;

        while ((val >>= 1) != 0)
        {
            answer++;
        }

        return answer;
    }

    public static void R_ConcatRotations(float[][] in1, float[][] in2, float[][] output)
    {
        output[0][0] = in1[0][0] * in2[0][0] + in1[0][1] * in2[1][0] + in1[0][2] * in2[2][0];
        output[0][1] = in1[0][0] * in2[0][1] + in1[0][1] * in2[1][1] + in1[0][2] * in2[2][1];
        output[0][2] = in1[0][0] * in2[0][2] + in1[0][1] * in2[1][2] + in1[0][2] * in2[2][2];
        output[1][0] = in1[1][0] * in2[0][0] + in1[1][1] * in2[1][0] + in1[1][2] * in2[2][0];
        output[1][1] = in1[1][0] * in2[0][1] + in1[1][1] * in2[1][1] + in1[1][2] * in2[2][1];
        output[1][2] = in1[1][0] * in2[0][2] + in1[1][1] * in2[1][2] + in1[1][2] * in2[2][2];
        output[2][0] = in1[2][0] * in2[0][0] + in1[2][1] * in2[1][0] + in1[2][2] * in2[2][0];
        output[2][1] = in1[2][0] * in2[0][1] + in1[2][1] * in2[1][1] + in1[2][2] * in2[2][1];
        output[2][2] = in1[2][0] * in2[0][2] + in1[2][1] * in2[1][2] + in1[2][2] * in2[2][2];
    }

    public static void R_ConcatTransforms(float[][] in1, float[][] in2, float[][] output)
    {
        output[0][0] = in1[0][0] * in2[0][0] + in1[0][1] * in2[1][0] + in1[0][2] * in2[2][0];
        output[0][1] = in1[0][0] * in2[0][1] + in1[0][1] * in2[1][1] + in1[0][2] * in2[2][1];
        output[0][2] = in1[0][0] * in2[0][2] + in1[0][1] * in2[1][2] + in1[0][2] * in2[2][2];
        output[0][3] = in1[0][0] * in2[0][3] + in1[0][1] * in2[1][3] + in1[0][2] * in2[2][3] + in1[0][3];
        output[1][0] = in1[1][0] * in2[0][0] + in1[1][1] * in2[1][0] + in1[1][2] * in2[2][0];
        output[1][1] = in1[1][0] * in2[0][1] + in1[1][1] * in2[1][1] + in1[1][2] * in2[2][1];
        output[1][2] = in1[1][0] * in2[0][2] + in1[1][1] * in2[1][2] + in1[1][2] * in2[2][2];
        output[1][3] = in1[1][0] * in2[0][3] + in1[1][1] * in2[1][3] + in1[1][2] * in2[2][3] + in1[1][3];
        output[2][0] = in1[2][0] * in2[0][0] + in1[2][1] * in2[1][0] + in1[2][2] * in2[2][0];
        output[2][1] = in1[2][0] * in2[0][1] + in1[2][1] * in2[1][1] + in1[2][2] * in2[2][1];
        output[2][2] = in1[2][0] * in2[0][2] + in1[2][1] * in2[1][2] + in1[2][2] * in2[2][2];
        output[2][3] = in1[2][0] * in2[0][3] + in1[2][1] * in2[1][3] + in1[2][2] * in2[2][3] + in1[2][3];
    }

    public static void R_ConcatTransforms(float** in1, float** in2, float** output)
    {
        output[0][0] = in1[0][0] * in2[0][0] + in1[0][1] * in2[1][0] + in1[0][2] * in2[2][0];
        output[0][1] = in1[0][0] * in2[0][1] + in1[0][1] * in2[1][1] + in1[0][2] * in2[2][1];
        output[0][2] = in1[0][0] * in2[0][2] + in1[0][1] * in2[1][2] + in1[0][2] * in2[2][2];
        output[0][3] = in1[0][0] * in2[0][3] + in1[0][1] * in2[1][3] + in1[0][2] * in2[2][3] + in1[0][3];
        output[1][0] = in1[1][0] * in2[0][0] + in1[1][1] * in2[1][0] + in1[1][2] * in2[2][0];
        output[1][1] = in1[1][0] * in2[0][1] + in1[1][1] * in2[1][1] + in1[1][2] * in2[2][1];
        output[1][2] = in1[1][0] * in2[0][2] + in1[1][1] * in2[1][2] + in1[1][2] * in2[2][2];
        output[1][3] = in1[1][0] * in2[0][3] + in1[1][1] * in2[1][3] + in1[1][2] * in2[2][3] + in1[1][3];
        output[2][0] = in1[2][0] * in2[0][0] + in1[2][1] * in2[1][0] + in1[2][2] * in2[2][0];
        output[2][1] = in1[2][0] * in2[0][1] + in1[2][1] * in2[1][1] + in1[2][2] * in2[2][1];
        output[2][2] = in1[2][0] * in2[0][2] + in1[2][1] * in2[1][2] + in1[2][2] * in2[2][2];
        output[2][3] = in1[2][0] * in2[0][3] + in1[2][1] * in2[1][3] + in1[2][2] * in2[2][3] + in1[2][3];
    }

    public static void FloorDivMod(double numer, double denom, int* quotient, int* rem)
    {
        int q, r;
        double x;

#if PARANOID
		if (denom <= 0.0f)
		{
			sys_win_c.Sys_Error($"FloorDivMod: bad denominator {denom}\n");
		}
#endif

        if (numer >= 0.0f)
        {
            x = MathF.Floor((float)(numer / denom));
            q = (int)x;
            r = (int)MathF.Floor((float)(numer - (x * denom)));
        }
        else
        {
            x = MathF.Floor((float)(-numer / denom));
            q = -(int)x;
            r = (int)MathF.Floor((float)(-numer - (x * denom)));

            if (r != 0)
            {
                q--;
                r = (int)denom - r;
            }
        }

        *quotient = q;
        *rem = r;
    }

    public static int GreatestCommonDivisor(int i1, int i2)
    {
        if (i1 > i2)
        {
            if (i2 == 0)
            {
                return i1;
            }

            return GreatestCommonDivisor(i2, i1 % i2);
        }
        else
        {
            if (i1 == 0)
            {
                return i2;
            }

            return GreatestCommonDivisor(i1, i2 % i1);
        }
    }

#if !id386
    public int Invert24To16(int val)
    {
        if (val < 256)
        {
            return 0xFFFFFFF;
        }

        return (int)((0x1000 * (double)0x10000000 / (double)val) + 0.5f);
    }
#endif
}