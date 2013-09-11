﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NNanomsg
{
    public partial class NN
    {
        public static int Socket(Domain domain, Protocol protocol)
        {
            return UsingWindows
                       ? Interop_Windows.nn_socket((int) domain, (int) protocol)
                       : Interop_Linux.nn_socket((int) domain, (int) protocol);
        }

        public static int Connect(int s, string addr)
        {
            return UsingWindows ? Interop_Windows.nn_connect(s, addr + '\0') : Interop_Linux.nn_connect(s, addr + '\0');
        }

        public static int Bind(int s, string addr)
        {
            return UsingWindows ? Interop_Windows.nn_bind(s, addr + '\0') : Interop_Linux.nn_bind(s, addr + '\0');
        }

        public static int SetSocketOpt(int s, SocketOptions option, string val)
        {
            // todo: unsure if \0 termination is necessary. remove if not.
            return UsingWindows
                       ? Interop_Windows.nn_setsockopt_string(s, Constants.NN_SOL_SOCKET, (int)option, val + '\0', val.Length)
                       : Interop_Linux.nn_setsockopt_string(s, Constants.NN_SOL_SOCKET, (int)option, val + '\0', val.Length);
        }

        public static int SetSocketOpt(int s, Protocol level, int option, string val)
        {
            // todo: unsure if \0 termination is necessary. remove if not.
            return UsingWindows
                       ? Interop_Windows.nn_setsockopt_string(s, (int)level, option, val + '\0', val.Length)
                       : Interop_Linux.nn_setsockopt_string(s, (int)level, option, val + '\0', val.Length);
        }

        public static int SetSocketOpt(int s,SocketOptions option, int val)
        {
            return UsingWindows
                       ? Interop_Windows.nn_setsockopt_int(s, Constants.NN_SOL_SOCKET, (int)option, ref val, sizeof(int))
                       : Interop_Linux.nn_setsockopt_int(s, Constants.NN_SOL_SOCKET, (int)option, ref val, sizeof(int));
        }

        public static int SetSocketOpt(int s, Protocol level, int option, int val)
        {
            return UsingWindows
                       ? Interop_Windows.nn_setsockopt_int(s, (int)level, option, ref val, sizeof(int))
                       : Interop_Linux.nn_setsockopt_int(s, (int)level, option, ref val, sizeof(int));
        }

        public static int GetSocketOpt(int s, SocketOptions option, out int val)
        {
            // for some reason this is not working, and I don't understand why. 

            int optvallen = 0;
            int optval = 0;

            int rc = UsingWindows
                         ? Interop_Windows.nn_getsockopt_int(s, Constants.NN_SOL_SOCKET, (int)option, ref optval, ref optvallen)
                         : Interop_Linux.nn_getsockopt_int(s, Constants.NN_SOL_SOCKET, (int)option, ref optval, ref optvallen);

            val = optval;

            return rc;
        }

        public static int GetSocketOpt(int s, Protocol level, int option, out int val)
        {
            // for some reason this is not working, and I don't understand why.

            int optvallen = 0;
            int optval = 0;

            int rc = UsingWindows
                         ? Interop_Windows.nn_getsockopt_int(s, (int)level, option, ref optval, ref optvallen)
                         : Interop_Linux.nn_getsockopt_int(s, (int)level, option, ref optval, ref optvallen);

            val = optval;

            return rc;
        }

        public static int GetSocketOpt(int s, SocketOptions option, out string val)
        {
            throw new NotImplementedException("When I understand why the int version of this method isn't working, I'll implement this.");
        }

        public static int GetSocketOpt(int s, Protocol level, int option, out string val)
        {
            throw new NotImplementedException("When I understand why the int version of this method isn't working, I'll implement this.");
        }

        /// <summary>
        ///     Receives a message into an already allocated buffer
        ///     If the message is longer than the allocated buffer, it is truncated.
        /// </summary>
        /// <returns>
        ///     The length, in bytes, of the message.
        /// </returns>
        public static int Recv(int s, byte[] buf, SendRecvFlags flags)
        {
            return UsingWindows
                       ? Interop_Windows.nn_recv(s, buf, buf.Length, (int)flags)
                       : Interop_Linux.nn_recv(s, buf, buf.Length, (int)flags);
        }

        /// <summary>
        ///     Receives a message of unknown length into a new buffer.
        /// </summary>
        /// <returns>
        ///     The error code from nn_freemsg. Should always be 0. Probably safe to ignore.
        /// </returns>
        public static int Recv(int s, out byte[] buf, SendRecvFlags flags)
        {
            IntPtr buffer = IntPtr.Zero;
            int rc = UsingWindows
                         ? Interop_Windows.nn_recv(s, out buffer, Constants.NN_MSG, (int)flags)
                         : Interop_Linux.nn_recv(s, out buffer, Constants.NN_MSG, (int)flags);

            if (rc < 0)
            {
                buf = null;
                return rc;
            }

            int numberOfBytes = rc;

            // this inefficient, I'm sure there must be a better way.
            buf = new byte[numberOfBytes];
            for (int i = 0; i < numberOfBytes; ++i)
            {
                buf[i] = Marshal.ReadByte(buffer, i);
            }

            int rc2 = UsingWindows
                         ? Interop_Windows.nn_freemsg(buffer)
                         : Interop_Windows.nn_freemsg(buffer);

            Debug.Assert(rc2 == 0);

            return rc;
        }

        public static int Send(int s, byte[] buf, SendRecvFlags flags)
        {
            return UsingWindows
                       ? Interop_Windows.nn_send(s, buf, buf.Length, (int)flags)
                       : Interop_Linux.nn_send(s, buf, buf.Length, (int)flags);
        }

        public static int Errno()
        {
            return UsingWindows
                       ? Interop_Windows.nn_errno()
                       : Interop_Linux.nn_errno();
        }

        public static int Close(int s)
        {
            return UsingWindows
                       ? Interop_Windows.nn_close(s)
                       : Interop_Linux.nn_close(s);
        }

        public static int Shutdown(int s, int how)
        {
            return UsingWindows
                       ? Interop_Windows.nn_shutdown(s, how)
                       : Interop_Linux.nn_shutdown(s, how);
        }

        public static int Device(int s1, int s2)
        {
            return UsingWindows
                       ? Interop_Windows.nn_device(s1, s2)
                       : Interop_Linux.nn_device(s1, s2);
        }

        public static void Term()
        {
            if (UsingWindows)
            {
                Interop_Windows.nn_term();
            }
            else
            {
                Interop_Linux.nn_term();
            }
        }

        public static string StrError(int errnum)
        {
            return UsingWindows
                       ? Marshal.PtrToStringAnsi(Interop_Windows.nn_strerror(errnum))
                       : Marshal.PtrToStringAnsi(Interop_Linux.nn_strerror(errnum));
        }

        private static bool UsingWindows
        {
            get
            {
                if (_usingWindows == null)
                {
                    string os_s = Environment.OSVersion.ToString().ToLower();
                    _usingWindows = os_s.Contains("windows");
                }

                return _usingWindows.Value;
            }
        }

        private static bool? _usingWindows;
    }

}