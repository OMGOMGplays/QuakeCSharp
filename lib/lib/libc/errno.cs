﻿namespace lib.libc;

public unsafe class errno_c
{
    public const int EPERM = 1;
    public const int ENOENT = 2;
    public const int ESRCH = 3;
    public const int EINTR = 4;
    public const int EIO = 5;
    public const int ENXIO = 6;
    public const int E2BIG = 7;
    public const int ENOEXEC = 8;
    public const int EBADF = 9;
    public const int ECHILD = 10;
    public const int EAGAIN = 11;
    public const int ENOMEM = 12;
    public const int EACCES = 13;
    public const int EFAULT = 14;
    public const int ENOTBLK = 15;
    public const int EBUSY = 16;
    public const int EEXIST = 17;
    public const int EXDEV = 18;
    public const int ENODEV = 19;
    public const int ENOTDIR = 20;
    public const int EISDIR = 21;
    public const int EINVAL = 22;
    public const int ENFILE = 23;
    public const int EMFILE = 24;
    public const int ENOTTY = 25;
    public const int ETXTBSY = 26;
    public const int EFBIG = 27;
    public const int ENOSPC = 28;
    public const int ESPIPE = 29;
    public const int EROFS = 30;
    public const int EMLINK = 31;
    public const int EPIPE = 32;
    public const int EDOM = 33;
    public const int ERANGE = 34;
    public const int ENOMSG = 35;
    public const int EIDRM = 36;
    public const int ECHRNG = 37;
    public const int EL2NSYNC = 38;
    public const int EL3HLT = 39;
    public const int EL3RST = 40;
    public const int ELNRNG = 41;
    public const int EUNATCH = 42;
    public const int ENOCSI = 43;
    public const int EL2HLT = 44;
    public const int EDEADLK = 45;
    public const int ENOLCK = 46;
    public const int EBADE = 50;
    public const int EBADR = 51;
    public const int EXFULL = 52;
    public const int ENOANO = 53;
    public const int EBADRQC = 54;
    public const int EBADSLT = 55;
    public const int EDEADLOCK = 56;
    public const int EBFONT = 57;
    public const int ENOSTR = 60;
    public const int ENODATA = 61;
    public const int ETIME = 62;
    public const int ENOSR = 63;
    public const int ENONET = 64;
    public const int ENOPKG = 65;
    public const int EREMOTE = 66;
    public const int ENOLINK = 67;
    public const int EADV = 68;
    public const int ESRMNT = 69;
    public const int ECOMM = 70;
    public const int EPROTO = 71;
    public const int EMULTIHOP = 74;
    public const int ELBIN = 75;
    public const int EDOTDOT = 76;
    public const int EBADMSG = 77;
    public const int EFTYPE = 79;
    public const int ENOTUNIQ = 80;
    public const int EBADFD = 81;
    public const int EREMCHG = 82;
    public const int ELIBACC = 83;
    public const int ELIBBAD = 84;
    public const int ELIBSCN = 85;
    public const int ELIBMAX = 86;
    public const int ELIBEXEC = 87;
    public const int ENOSYS = 88;
    public const int ENMFILE = 89;
    public const int ENOTEMPTY = 90;
    public const int ENAMETOOLONG = 91;
    public const int ELOOP = 92;
    public const int EOPNOTSUPP = 95;
    public const int EPFNOSUPPORT = 96;
    public const int ECONNRESET = 104;
    public const int ENOBUGS = 105;
    public const int EAFNOSUPPORT = 106;
    public const int EPROTOTYPE = 107;
    public const int ENOTSOCK = 108;
    public const int ENOPROTOOPT = 109;
    public const int ESHUTDOWN = 110;
    public const int ECONNREFUSED = 111;
    public const int EADDRINUSE = 112;
    public const int ECONNABORTED = 113;
    public const int ENETUNREACH = 114;
    public const int ENETDOWN = 115;
    public const int ETIMEDOUT = 116;
    public const int EHOSTDOWN = 117;
    public const int EHOSTUNREACH = 118;
    public const int EINPROGRESS = 119;
    public const int EALREADY = 120;
    public const int EDESTADDRREQ = 121;
    public const int EMSGSIZE = 122;
    public const int EPROTONOSUPPORT = 123;
    public const int ESOCKTNOSUPPORT = 124;
    public const int EADDRNOTAVAIL = 125;
    public const int ENETRESET = 126;
    public const int EISCONN = 127;
    public const int ENOTCONN = 128;
    public const int ETOOMANYREFS = 129;
    public const int EPROCLIM = 130;
    public const int EUSERS = 131;
    public const int EDQUOT = 132;
    public const int ESTALE = 133;
    public const int ENOTSUP = 134;
    public const int ENOMEDIUM = 135;
    public const int ENOSHARE = 136;
    public const int ECASECLASH = 137;
    public const int ELISEQ = 138;
    public const int EOVERFLOW = 139;

    public const int EWOULDBLOCK = EAGAIN;

    public const int __ELASTERROR = 2000;

    public static int _errno;

    public static int __geterrno()
    {
        return _errno;
    }
}