/*   
Copyright 2006 - 2010 Intel Corporation

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Net.Sockets;
using OpenSource.Utilities;

namespace OSTL.UPnP
{
    /// <summary>
    /// An event based AsyncSocket Object
    /// </summary>
    public sealed class AsyncSocket
    {
        public class StopReadException : Exception
        {
            public StopReadException()
                : base("User initiated StopRead")
            {
            }
        }

        private Thread StopThread;
        private bool SentDisconnect;


        public void StopReading()
        {
            StopThread = Thread.CurrentThread;
        }

        /// <summary>
        /// The number of bytes read
        /// </summary>
        public int BufferReadLength = 0;
        /// <summary>
        /// The index to begin reading
        /// <para>
        /// setting the BeginPointer equal to the BufferSize has
        /// the same result as setting it to zero
        /// </para>
        /// </summary>
        public int BufferBeginPointer = 0;
        internal int BufferEndPointer = 0;
        /// <summary>
        /// The size of the data chunk
        /// </summary>
        public int BufferSize = 0;

        private Socket MainSocket;
        private IPEndPoint endpoint_local;

        private object SendLock;

        private AsyncCallback ReceiveCB;
        private AsyncCallback SendCB;
        private AsyncCallback ConnectCB;

        private int PendingBytesSent;
        private object CountLock;
        private long TotalBytesSent;

        private EndPoint rEP;
        private readonly Byte[] MainBuffer;
        private Queue SendQueue;

        public delegate void OnReceiveHandler(AsyncSocket sender, Byte[] buffer, int HeadPointer, int BufferSize, int BytesRead, IPEndPoint source, IPEndPoint remote);
        private readonly WeakEvent OnReceiveEvent = new();
        /// <summary>
        /// This is triggered when there is data to be processed
        /// </summary>
        public event OnReceiveHandler OnReceive
        {
            add
            {
                OnReceiveEvent.Register(value);
            }
            remove
            {
                OnReceiveEvent.UnRegister(value);
            }
        }

        public delegate void OnSendReadyHandler(object Tag);
        private readonly WeakEvent OnSendReadyEvent = new();
        /// <summary>
        /// This is triggered when the SendQueue is ready for more
        /// </summary>
        public event OnSendReadyHandler OnSendReady
        {
            add
            {
                OnSendReadyEvent.Register(value);
            }
            remove
            {
                OnSendReadyEvent.UnRegister(value);
            }
        }

        public delegate void ConnectHandler(AsyncSocket sender);
        private readonly WeakEvent OnConnectEvent = new();
        private readonly WeakEvent OnConnectFailedEvent = new();
        private readonly WeakEvent OnDisconnectEvent = new();
        /// <summary>
        /// This is triggered when a Connection attempt was successful
        /// </summary>
        public event ConnectHandler OnConnect
        {
            add
            {
                OnConnectEvent.Register(value);
            }
            remove
            {
                OnConnectEvent.UnRegister(value);
            }
        }
        /// <summary>
        /// This is triggered when a Connection attempt failed
        /// </summary>
        public event ConnectHandler OnConnectFailed
        {
            add
            {
                OnConnectFailedEvent.Register(value);
            }
            remove
            {
                OnConnectFailedEvent.UnRegister(value);
            }
        }
        /// <summary>
        /// This is triggered when the underlying socket closed
        /// </summary>
        public event ConnectHandler OnDisconnect
        {
            add
            {
                OnDisconnectEvent.Register(value);
            }
            remove
            {
                OnDisconnectEvent.UnRegister(value);
            }
        }

        //private EndPoint LocalEP;
        //private EndPoint RemoteEP;

        private readonly Stream _WriteStream = null;


        private struct SendInfo
        {
            public Byte[] buffer;
            public int offset;
            public int count;
            public object Tag;
            public IPEndPoint dest;
        }


        /// <summary>
        /// Creates a new AsyncSocket, with a stream object to write to
        /// </summary>
        /// <param name="WriteStream">The Stream to use</param>
        public AsyncSocket(Stream WriteStream)
        {
            _WriteStream = WriteStream;
            MainBuffer = new byte[4096];
        }
        /// <summary>
        /// Creates a new AsyncSocket, with a fixed size buffer to write to
        /// </summary>
        /// <param name="BufferSize">Size of buffer</param>
        public AsyncSocket(int BufferSize)
        {
            MainBuffer = new byte[BufferSize];
        }
        /// <summary>
        /// Attaches this AsyncSocket to a new Socket instance, using the given info.
        /// </summary>
        /// <param name="local">Local interface to use</param>
        /// <param name="PType">Protocol Type</param>
        public void Attach(IPEndPoint local, ProtocolType PType)
        {
            endpoint_local = local;
            TotalBytesSent = 0;
            //LocalEP = local;
            Init();

            MainSocket = null;

            if (PType == ProtocolType.Tcp)
            {
                try
                {
                    MainSocket = new Socket(local.AddressFamily, SocketType.Stream, PType);
                }
                catch
                {
                    //ignore
                }
            }

            if (PType == ProtocolType.Udp)
            {
                MainSocket = new Socket(local.AddressFamily, SocketType.Dgram, PType);
                MainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            }

            if (MainSocket != null)
            {
                MainSocket.Bind(local);
                PropertyInfo pi = MainSocket.GetType().GetProperty("UseOnlyOverlappedIO");
                if (pi != null)
                {
                    pi.SetValue(MainSocket, true, null);
                }
            }
            else
            {
                throw (new Exception(PType + " not supported"));
            }
        }
        /// <summary>
        /// Attach this AsyncSocket to an existing Socket
        /// </summary>
        /// <param name="UseThisSocket">The Socket</param>
        public void Attach(Socket UseThisSocket)
        {
            endpoint_local = (IPEndPoint)UseThisSocket.LocalEndPoint;
            TotalBytesSent = 0;
            //LocalEP = UseThisSocket.LocalEndPoint;
            if (UseThisSocket.SocketType == SocketType.Stream)
            {
                //RemoteEP = UseThisSocket.RemoteEndPoint;
                endpoint_local = (IPEndPoint)UseThisSocket.LocalEndPoint;
            }
            else
            {
                //RemoteEP = null;
            }
            MainSocket = UseThisSocket;
            PropertyInfo pi = MainSocket.GetType().GetProperty("UseOnlyOverlappedIO");
            if (pi != null)
            {
                pi.SetValue(MainSocket, true, null);
            }
            Init();
        }


        public void SetTTL(int TTL)
        {
            MainSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, TTL);
        }
        /// <summary>
        /// Join a multicast group
        /// </summary>
        /// <param name="local">Interface to use</param>
        /// <param name="MulticastAddress">MulticastAddress to join</param>
        public void AddMembership(IPEndPoint local, IPAddress MulticastAddress)
        {
            try
            {
                MainSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, 1);
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "AsyncSocket:AddMemberShip1:Local:"+local.Address+" MulticastAddress:"+MulticastAddress);
                // This will only fail if the network stack does not support this
                // Which means you are probably running Win9x
            }

            try
            {
                MainSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(MulticastAddress));
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "AsyncSocket:AddMemberShip2:Local:" + local.Address + " MulticastAddress:" + MulticastAddress);
                EventLogger.Log(this, EventLogEntryType.Error, "Cannot AddMembership to IPAddress: " + MulticastAddress);
            }
            try
            {
                MainSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, local.Address.GetAddressBytes());
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "AsyncSocket:AddMemberShip3:Local:" + local.Address + " MulticastAddress:" + MulticastAddress);
                EventLogger.Log(this, EventLogEntryType.Error, "Cannot Set Multicast Interface to IPAddress: " + local.Address);
            }
        }
        /// <summary>
        /// Leave a multicast group
        /// </summary>
        /// <param name="MulticastAddress">Multicast Address to leave</param>
        public void DropMembership(IPAddress MulticastAddress)
        {
            MainSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(MulticastAddress));
        }

        /// <summary>
        /// Number of bytes in the send queue pending
        /// </summary>
        public int Pending
        {
            get { return (PendingBytesSent); }
        }

        /// <summary>
        /// Total bytes send
        /// </summary>
        public long Total
        {
            get { return (TotalBytesSent); }
        }

        /// <summary>
        /// The Local EndPoint
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get
            {
                if (MainSocket.LocalEndPoint != null)
                {
                    return MainSocket.LocalEndPoint;
                }
                return (endpoint_local);
            }
        }

        /// <summary>
        /// The Remote EndPoint
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get { return (MainSocket.RemoteEndPoint); }
        }

        /// <summary>
        /// Connect to a remote socket
        /// </summary>
        /// <param name="Remote">IPEndPoint to connect to</param>
        public void Connect(IPEndPoint Remote)
        {
            if (MainSocket.SocketType != SocketType.Stream)
            {
                throw (new Exception("Cannot connect a non StreamSocket"));
            }
            PropertyInfo pi = MainSocket.GetType().GetProperty("UseOnlyOverlappedIO");
            if (pi != null)
            {
                pi.SetValue(MainSocket, true, null);
            }
            try
            {
                MainSocket.BeginConnect(Remote, ConnectCB, null);
            }
            catch
            {
                //ignore
            }
        }

        private void HandleConnect(IAsyncResult result)
        {
            bool IsOK = false;
            try
            {
                MainSocket.EndConnect(result);
                IsOK = true;
                //RemoteEP = MainSocket.RemoteEndPoint;
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "AsyncSocket:HandleConnect");
            }

            if (IsOK && MainSocket.Connected)
            {
                OnConnectEvent.Fire(this);
            }
            else
            {
                OnConnectFailedEvent.Fire(this);
            }
        }

        /// <summary>
        /// Start AsyncReads
        /// </summary>
        /// <returns>Successfully started</returns>
        public void Begin()
        {
            bool Disconnect = false;
            IPEndPoint src, from;

            if (MainSocket.SocketType == SocketType.Stream)
            {
                from = (IPEndPoint)MainSocket.RemoteEndPoint;
            }
            else
            {
                from = (IPEndPoint)rEP;
            }

            try
            {
                src = (IPEndPoint)MainSocket.LocalEndPoint;
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "AsyncSocket:Begin");
                src = new IPEndPoint(IPAddress.Any, 0);
            }

            while ((BufferBeginPointer != 0) &&
                (BufferBeginPointer != BufferEndPointer))
            {
                Array.Copy(MainBuffer, BufferBeginPointer, MainBuffer, 0, BufferEndPointer - BufferBeginPointer);
                BufferEndPointer -= BufferBeginPointer;
                BufferBeginPointer = 0;
                BufferSize = BufferEndPointer;
                try
                {
                    OnReceiveEvent.Fire(this, MainBuffer, BufferBeginPointer, BufferSize, 0, src, from);
                }
                catch (StopReadException ex)
                {
                    EventLogger.Log(ex, "AsyncSocket:Begin2");
                    return;
                }
                if (StopThread != null)
                {
                    if (Thread.CurrentThread.GetHashCode() == StopThread.GetHashCode())
                    {
                        StopThread = null;
                        return;
                    }
                }
            }

            try
            {
                if (MainSocket.SocketType == SocketType.Stream)
                {
                    MainSocket.BeginReceive(MainBuffer, BufferEndPointer, BufferReadLength, SocketFlags.None, ReceiveCB, null);
                }
                else
                {
                    MainSocket.BeginReceiveFrom(MainBuffer, BufferEndPointer, BufferReadLength, SocketFlags.None, ref rEP, ReceiveCB, null);
                }
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "AsyncSocket:Begin3");
                Disconnect = true;
            }

            if (Disconnect)
            {
                bool OK = false;
                lock (this)
                {
                    if (SentDisconnect == false)
                    {
                        OK = true;
                        SentDisconnect = true;
                    }
                }
                if (OK)
                {
                    MainSocket = null;
                }
                if (OK) OnDisconnectEvent.Fire(this);
            }
        }

        private void Init()
        {
            BufferReadLength = MainBuffer.Length;
            CountLock = new object();
            PendingBytesSent = 0;
            SendLock = new object();
            SendQueue = new Queue();
            ReceiveCB = HandleReceive;
            SendCB = HandleSend;
            ConnectCB = HandleConnect;
            rEP = new IPEndPoint(0, 0);
        }

        /// <summary>
        /// Closes the socket
        /// </summary>
        public void Close()
        {
            //SendLock.WaitOne();
            //SendResult = null;
            //SendLock.ReleaseMutex();
            if (MainSocket != null)
            {
                try
                {
                    MainSocket.Shutdown(SocketShutdown.Both);
                    MainSocket.Close();
                }
                catch (Exception ex)
                {
                    EventLogger.Log(ex, "AsyncSocket:Close");
                }
            }
        }
        /// <summary>
        /// Asynchronously send bytes
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(Byte[] buffer)
        {
            Send(buffer, null);
        }

        /// <summary>
        /// Asynchronously send bytes
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="Tag"></param>
        public void Send(Byte[] buffer, object Tag)
        {
            Send(buffer, 0, buffer.Length, Tag);
        }

        /// <summary>
        /// Asyncronously send bytes
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="Tag"></param>
        public void Send(Byte[] buffer, int offset, int length, object Tag)
        {
            Send(buffer, offset, length, null, Tag);
        }

        public void Send(Byte[] buffer, int offset, int length, IPEndPoint dest)
        {
            Send(buffer, offset, length, dest, null);
        }

        /// <summary>
        /// Asynchronously send a UDP payload
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="dest"></param>
        /// <param name="Tag"></param>
        public void Send(Byte[] buffer, int offset, int length, IPEndPoint dest, object Tag)
        {
            bool Disconnect = false;

            SendInfo SI;

            lock (SendLock)
            {
                lock (CountLock)
                {
                    if (PendingBytesSent > 0)
                    {
                        SI = new SendInfo
                        {
                            buffer = buffer,
                            offset = offset,
                            count = length,
                            dest = dest,
                            Tag = Tag
                        };
                        SendQueue.Enqueue(SI);
                    }
                    else
                    {
                        PendingBytesSent += length;
                        try
                        {
                            if (MainSocket.SocketType == SocketType.Stream)
                            {
                                MainSocket.BeginSend(buffer, offset, length, SocketFlags.None, SendCB, Tag);
                            }
                            else
                            {
                                MainSocket.BeginSendTo(buffer, offset, length, SocketFlags.None, dest, SendCB, Tag);
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLogger.Log(ex, "AsyncSocket:Send");
                            EventLogger.Log(this, EventLogEntryType.Error, "Send Failure [Normal for non-pipelined connection]");
                            Disconnect = true;
                        }
                    }
                }
            }

            if (Disconnect)
            {
                bool OK = false;
                lock (this)
                {
                    if (SentDisconnect == false)
                    {
                        OK = true;
                        SentDisconnect = true;
                    }
                }
                if (OK)
                {
                    MainSocket = null;
                    OnDisconnectEvent.Fire(this);
                }
            }
        }

        private void HandleSend(IAsyncResult result)
        {
            int sent = 0;
            bool Ready = false;
            bool Disconnect = false;

            try
            {
                SendInfo SI;
                lock (SendLock)
                {
                    try
                    {
                        if (MainSocket == null)
                        {
                            Disconnect = true;
                        }
                        else
                        {
                            if (MainSocket.SocketType == SocketType.Stream)
                            {
                                try
                                {
                                    sent = MainSocket.EndSend(result);
                                }
                                catch
                                {
                                    //Disconnect = true;
                                    //ignore, Das Disconnect ist durch mich gesetzt, keine AHnung was das beduetet.";
                                }
                            }
                            else
                            {
                                sent = MainSocket.EndSendTo(result);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.Log(ex, "AsyncSocket:HandleSend");
                        Disconnect = true;
                    }
                    lock (CountLock)
                    {
                        PendingBytesSent -= sent;
                        TotalBytesSent += sent;
                    }

                    if (SendQueue.Count > 0)
                    {
                        SI = (SendInfo)SendQueue.Dequeue();
                        try
                        {
                            if (MainSocket.SocketType == SocketType.Stream)
                            {
                                MainSocket.BeginSend(SI.buffer, SI.offset, SI.count, SocketFlags.None, SendCB, SI.Tag);
                            }
                            else
                            {
                                MainSocket.BeginSendTo(SI.buffer, SI.offset, SI.count, SocketFlags.None, SI.dest, SendCB, SI.Tag);
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLogger.Log(ex, "AsyncSocket:HandleSend2");
                            EventLogger.Log(this, EventLogEntryType.Error, "Send Failure [Normal for non-pipelined connection]");
                            Disconnect = true;
                        }
                    }
                    else
                    {
                        Ready = true;
                    }

                }
                if (Disconnect)
                {
                    bool OK = false;
                    lock (this)
                    {
                        if (SentDisconnect == false)
                        {
                            OK = true;
                            SentDisconnect = true;
                        }
                    }
                    if (OK)
                    {
                        MainSocket = null;
                    }
                    if (OK) OnDisconnectEvent.Fire(this);
                }
                else
                {
                    if (Ready)
                    {
                        OnSendReadyEvent.Fire(result.AsyncState);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "AsyncSocket:HandleSend3");
            }
        }

        private void HandleReceive(IAsyncResult result)
        {
            int BytesReceived;
            IPEndPoint from;
            IPEndPoint src;
            bool Disconnect = false;

            try
            {
                if (MainSocket.SocketType == SocketType.Stream)
                {
                    from = (IPEndPoint)MainSocket.RemoteEndPoint;
                    BytesReceived = MainSocket.EndReceive(result);
                }
                else
                {
                    BytesReceived = MainSocket.EndReceiveFrom(result, ref rEP);
                    from = (IPEndPoint)rEP;
                }
            }
            catch (Exception ex)
            {
                // Socket Error
                bool _OK = false;
                var k = result.AsyncState;
                if (k == null)
                    k = "NullConvertToString";
                EventLogger.Log(ex, "AsyncSocket:HandleReceive \r\n"+k.ToString());
                lock (this)
                {
                    if (SentDisconnect == false)
                    {
                        _OK = true;
                        SentDisconnect = true;
                    }
                }
                if (_OK)
                {
                    MainSocket = null;
                }
                if (_OK) OnDisconnectEvent.Fire(this);
                return;
            }

            if (BytesReceived <= 0)
            {
                Disconnect = true;
            }

            if (BytesReceived != 0)
            {
                try
                {
                    src = (IPEndPoint)MainSocket.LocalEndPoint;
                }
                catch (Exception ex)
                {
                    EventLogger.Log(ex, "AsyncSocket:HandleReceive2");
                    src = new IPEndPoint(IPAddress.Any, 0);
                }


                BufferEndPointer += BytesReceived;

                BufferSize = BufferEndPointer - BufferBeginPointer;
                BufferReadLength = MainBuffer.Length - BufferEndPointer;

                if (_WriteStream == null)
                {
                    try
                    {
                        OnReceiveEvent.Fire(this, MainBuffer, BufferBeginPointer, BufferSize, BytesReceived, src, from);
                    }
                    catch (StopReadException ex)
                    {
                        EventLogger.Log(ex, "AsyncSocket:HandleReceive3");
                        return;
                    }
                }
                else
                {
                    _WriteStream.Write(MainBuffer, 0, BytesReceived);
                    BufferBeginPointer = BufferEndPointer;
                    BufferReadLength = MainBuffer.Length;
                }

                while ((BufferBeginPointer != 0) &&
                    (BufferBeginPointer != BufferEndPointer))
                {
                    Array.Copy(MainBuffer, BufferBeginPointer, MainBuffer, 0, BufferEndPointer - BufferBeginPointer);
                    BufferEndPointer -= BufferBeginPointer;
                    BufferBeginPointer = 0;
                    BufferSize = BufferEndPointer;
                    try
                    {
                        OnReceiveEvent.Fire(this, MainBuffer, BufferBeginPointer, BufferSize, 0, src, from);
                    }
                    catch (StopReadException ex)
                    {
                        EventLogger.Log(ex, "AsyncSocket:HandleReceive4");
                        return;
                    }
                    if (StopThread != null)
                    {
                        if (Thread.CurrentThread.GetHashCode() == StopThread.GetHashCode())
                        {
                            StopThread = null;
                            return;
                        }
                    }
                }

                if (BufferBeginPointer == BufferEndPointer)
                {
                    // ResetBuffer then continue reading
                    BufferBeginPointer = 0;
                    BufferEndPointer = 0;
                }

                if (StopThread != null)
                {
                    if (Thread.CurrentThread.GetHashCode() == StopThread.GetHashCode())
                    {
                        StopThread = null;
                        return;
                    }
                }
                try
                {
                    if (MainSocket != null && MainSocket.Connected)
                    {
                        if (MainSocket.SocketType == SocketType.Stream)
                        {
                            MainSocket.BeginReceive(MainBuffer, BufferEndPointer, BufferReadLength, SocketFlags.None, ReceiveCB, MainSocket);
                        }
                        else
                        {
                            MainSocket.BeginReceiveFrom(MainBuffer, BufferEndPointer, BufferReadLength, SocketFlags.None, ref rEP, ReceiveCB, MainSocket);
                        }
                    }
                    else
                    {
                        Disconnect = true;
                    }
                }
                catch (Exception ex)
                {
                    EventLogger.Log(ex, "AsyncSocket:HandleReceive5");
                    Disconnect = true;
                }
            }

            if (Disconnect)
            {
                bool OK = false;
                lock (this)
                {
                    if (SentDisconnect == false)
                    {
                        OK = true;
                        SentDisconnect = true;
                    }
                }
                if (OK)
                {
                    MainSocket = null;
                }
                if (Disconnect && OK) OnDisconnectEvent.Fire(this);
            }
        }

    }
}
