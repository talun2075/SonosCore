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
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using OpenSource.Utilities;

/* Important!!  StreamingBuffer size is set to 32768, which appears to work ok, 
 * however, some day, we'll need to figure out why a smaller buffer size screws up
 * Windows Media Player when played locally */

namespace OSTL.UPnP
{
    /// <summary>
    /// A useful HTTPSession Object
    /// </summary>
    public sealed class HTTPSession
    {
        private static readonly UTF8Encoding U = new();
        public struct Range
        {
            public long Position;
            public long Length;
            public long RangeLeft;

            public Range(long position, long length)
            {
                Position = position;
                Length = length;
                RangeLeft = length;
            }
        }
        public struct Info
        {
            public Stream CurrentStreamObject;
            public string StringPacket;

            public Range[] RangeList;
            public int RangeIndex;
            public string RangeSeparator;
            public string RangeContentType;
        }

        #region ActivityMonitor Definition
        public class ActivityMonitor
        {
            private DateTime TimeStamp;
            private bool Active = true;

            public void SetInactive()
            {
                lock (this)
                {
                    Active = false;
                    TimeStamp = DateTime.Now;
                }
            }
            public void SetActive()
            {
                lock (this)
                {
                    Active = true;
                }
            }
            public bool IsTimeout()
            {
                lock (this)
                {
                    if (!Active)
                    {
                        if (TimeStamp.AddSeconds(10).CompareTo(DateTime.Now) <= 0)
                        {
                            return (true);
                        }
                    }
                    return (false);
                }
            }
        }
        #endregion
        private int _Counter;
        public ActivityMonitor Monitor = new();

        private bool SetRequestAnswered;
        private bool IsLegacy;

        private readonly Queue StateQueue = new();
        private Stream PostStream;

        /// <summary>
        /// User Stream object
        /// <para>
        /// If this value is set, (usually in the HeaderHandler), the body
        /// will be written to this stream object, instead of calling the receive callback.
        /// </para>
        /// </summary>
        public Stream UserStream = null;
        private readonly Queue SendQueue = new();
        private readonly byte[] SendBuffer = new byte[4096];

        public bool IsConnected
        {
            get
            {
                return (Connected);
            }
        }
        private bool Connected;


        private MemoryStream SocketStream = new();
        private bool FinishedHeader;
        internal bool IsChunked = false;
        internal bool ConnectionCloseSpecified = false;
        private int BodySize;
        private int ChunkDataSize;
        private HTTPMessage Headers;
        public bool ChunkedHeadersWereAdded
        {
            get
            {
                return (ChunkedHeadersAdded);
            }
        }
        private bool ChunkedHeadersAdded;

        public const int FIN_CHUNK_CRLF = 60;
        public const int START_CHUNK = 55;
        public const int DATA_CHUNK = 56;
        public const int START_FOOTER_CHUNK = 57;
        public const int FOOTER_DATA = 58;
        public const int NO_CHUNK = 0;
        private int ChunkState = NO_CHUNK;

        public const bool CHUNK_ENABLED = true;


        private readonly IPEndPoint local_ep;
        private bool NeedToWaitToClose;

        private readonly WeakEvent OnReceiveEvent = new();
        public delegate void ReceiveHandler(HTTPSession sender, HTTPMessage msg);
        /// <summary>
        /// This is triggered when the entire packet is received
        /// </summary>
        public event ReceiveHandler OnReceive
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

        private readonly WeakEvent OnSniffPacketEvent = new();
        /// <summary>
        /// This is triggered whenever anything goes on the wire
        /// </summary>
        public event ReceiveHandler OnSniffPacket
        {
            add
            {
                OnSniffPacketEvent.Register(value);
            }
            remove
            {
                OnSniffPacketEvent.UnRegister(value);
            }
        }

        private readonly WeakEvent OnHeaderEvent = new();
        public delegate void ReceiveHeaderHandler(HTTPSession sender, HTTPMessage Headers, Stream StreamObj);
        /// <summary>
        /// This is triggered when the Headers are received
        /// </summary>
        public event ReceiveHeaderHandler OnHeader
        {
            add
            {
                OnHeaderEvent.Register(value);
            }
            remove
            {
                OnHeaderEvent.UnRegister(value);
            }
        }

        public delegate void SessionHandler(HTTPSession TheSession);
        public delegate void StreamDoneHandler(HTTPSession sender, Stream StreamObject);

        private readonly WeakEvent OnRequestAnsweredEvent = new();
        public event SessionHandler OnRequestAnswered
        {
            add
            {
                OnRequestAnsweredEvent.Register(value);
            }
            remove
            {
                OnRequestAnsweredEvent.UnRegister(value);
            }
        }

        private readonly WeakEvent OnStreamDoneEvent = new();
        /// <summary>
        /// This is triggered when the Stream object is finished sending/receiving
        /// </summary>
        public event StreamDoneHandler OnStreamDone
        {
            add
            {
                OnStreamDoneEvent.Register(value);
            }
            remove
            {
                OnStreamDoneEvent.UnRegister(value);
            }
        }

        private readonly WeakEvent OnCreateSessionEvent = new();
        /// <summary>
        /// This is triggered when a Session is sucessfully created
        /// </summary>
        public event SessionHandler OnCreateSession
        {
            add
            {
                OnCreateSessionEvent.Register(value);
            }
            remove
            {
                OnCreateSessionEvent.UnRegister(value);
            }
        }

        private readonly WeakEvent OnCreateFailedEvent = new();
        /// <summary>
        /// This is triggered when a Session creation failed.
        /// </summary>
        public event SessionHandler OnCreateFailed
        {
            add
            {
                OnCreateFailedEvent.Register(value);
            }
            remove
            {
                OnCreateFailedEvent.UnRegister(value);
            }
        }

        private readonly WeakEvent OnClosedEvent = new();
        /// <summary>
        /// This is triggered when the session closed
        /// </summary>
        public event SessionHandler OnClosed
        {
            add
            {
                OnClosedEvent.Register(value);
            }
            remove
            {
                OnClosedEvent.UnRegister(value);
            }
        }

        private readonly WeakEvent OnSendReadyEvent = new();
        /// <summary>
        /// This is triggered when the underlying AsyncSocket's SendQueue is empty
        /// </summary>
        public event AsyncSocket.OnSendReadyHandler OnSendReady
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

        private readonly WeakEvent OnSniffEvent = new();
        internal delegate void SniffHandler(byte[] Raw, int offset, int length);
        internal event SniffHandler OnSniff
        {
            add
            {
                OnSniffEvent.Register(value);
            }
            remove
            {
                OnSniffEvent.UnRegister(value);
            }
        }

        private AsyncSocket MainSocket;

        private readonly Byte[] StreamSendBuffer = new Byte[32768];
        private long EndPosition;

        /// <summary>
        /// State Object, for custom use by the end-user/developer
        /// </summary>
        public Object StateObject;
        internal Object InternalStateObject = null;

        ~HTTPSession()
        {
            if (MainSocket != null)
            {
                MainSocket.Close();
                MainSocket = null;
            }
        }

        /// <summary>
        /// Creates a new HTTPSession
        /// </summary>
        /// <param name="Local">Source IPEndPoint to use</param>
        /// <param name="Remote">Remote IPEndPoint to connect to</param>
        /// <param name="CreateCallback">Succussful callback</param>
        /// <param name="CreateFailedCallback">Failed callback</param>
        /// <param name="State">StateObject</param>
        public HTTPSession(IPEndPoint Local, IPEndPoint Remote, SessionHandler CreateCallback, SessionHandler CreateFailedCallback, Object State)
        {
            //InstanceTracker.Add(this);
            local_ep = Local;

            OnCreateSession += CreateCallback;
            OnCreateFailed += CreateFailedCallback;

            StateObject = State;

            MainSocket = new AsyncSocket(4096);
            MainSocket.Attach(Local, ProtocolType.Tcp);
            MainSocket.OnConnect += HandleConnect;
            MainSocket.OnConnectFailed += HandleConnectFailed;
            MainSocket.OnDisconnect += HandleDisconnect;
            MainSocket.OnSendReady += HandleReady;

            MainSocket.Connect(Remote);
        }

        /// <summary>
        /// Attach to an existing Socket
        /// </summary>
        /// <param name="TheSocket">The socket to use</param>
        /// <param name="HeaderCallback">Header Callback</param>
        /// <param name="RequestCallback">Request/Responses callback</param>
        public HTTPSession(Socket TheSocket, ReceiveHeaderHandler HeaderCallback, ReceiveHandler RequestCallback) : this((IPEndPoint)TheSocket.LocalEndPoint, TheSocket, HeaderCallback, RequestCallback)
        {
        }
        public HTTPSession(IPEndPoint Local, Socket TheSocket)
        {
            /* Eveything calls this */

            //InstanceTracker.Add(this);

            Connected = TheSocket.Connected;
            local_ep = Local;

            MainSocket = new AsyncSocket(4096);
            MainSocket.Attach(TheSocket);
            MainSocket.OnReceive += HandleReceive;
            MainSocket.OnDisconnect += HandleDisconnect;
            MainSocket.OnSendReady += HandleReady;
            /* This is New */
            //MainSocket.BufferReadLength = 1;
            MainSocket.BufferReadLength = 1024;
        }

        /// <summary>
        /// Attach to an existing Socket
        /// </summary>
        /// <param name="Local">A Specific Local IPEndPoint</param>
        /// <param name="TheSocket">The Socket</param>
        /// <param name="HeaderCallback">Header Callback</param>
        /// <param name="RequestCallback">Request/Response callback</param>
        public HTTPSession(IPEndPoint Local, Socket TheSocket, ReceiveHeaderHandler HeaderCallback, ReceiveHandler RequestCallback) : this(Local, TheSocket)
        {
            if (HeaderCallback != null)
            {
                OnHeader += HeaderCallback;
            }
            if (RequestCallback != null)
            {
                OnReceive += RequestCallback;
            }

            MainSocket.Begin();
        }

        public void StopReading()
        {
            MainSocket.StopReading();
        }
        public void StartReading()
        {
            MainSocket.Begin();
        }

        /// <summary>
        /// An uniquely generated Session ID
        /// </summary>
        public int SessionID
        {
            get
            {
                return (GetHashCode());
            }
        }

        public void FinishedProcessing()
        {
            MainSocket.Begin();
        }


        /// <summary>
        /// Closes an Object that was being streamed. <see cref="HTTPSession"/> checks
        /// to see that the provided stream matches the CurrentStreamObject. In either
        /// case, the stream is closed. If the streams match then CurrentStreamObject is additionally
        /// set to null.
        /// </summary>
        /// <param name="stream">Tells the <see cref="HTTPSession"/> object to close this stream.</param>
        public void CloseStreamObject(Stream stream)
        {
            stream.Close();
            lock (StreamLock)
            {
                if (PostStream == stream)
                {
                    PostStream = null;
                }
            }
            lock (StateQueue)
            {
                if (StateQueue.Count > 0)
                {
                    if (((Info)StateQueue.Peek()).CurrentStreamObject == stream)
                    {
                        Info x = (Info)StateQueue.Peek();
                        x.CurrentStreamObject = null;
                    }
                }
            }
        }
        /// <summary>
        /// This object is used simply for locking the CurrentStreamObject field.
        /// </summary>
        private readonly object StreamLock = new();
        /// <summary>
        /// HTTP Post a stream
        /// </summary>
        /// <param name="SObject">Stream Object</param>
        /// <param name="PostWhat">The POST data</param>
        /// <param name="ContentType">Content-type</param>
        public void PostStreamObject(Stream SObject, string PostWhat, string ContentType)
        {
            PostStream = SObject;
            OnSendReady += HandleStreamReady;

            String Packet = "POST " + HTTPMessage.EscapeString(PostWhat) + " HTTP/1.1\r\n";
            Packet += "Server: MiniWebServer\r\n";
            Packet += "Content-Type:" + ContentType + "\r\n";

            long length = SObject.Length - SObject.Position;

            EndPosition = SObject.Position + length;
            if (EndPosition > SObject.Length)
            {
                EndPosition = SObject.Length;
                length = SObject.Length - SObject.Position;
            }
            Packet += "Content-Length:" + length.ToString() + "\r\n\r\n";
            MainSocket.Send(U.GetBytes(Packet));
        }

        private void SendChunked(byte[] buffer, int offset, int count, object Tag)
        {
            if (count == 0) return;
            lock (this)
            {
                if (MainSocket != null)
                {
                    MainSocket.Send(U.GetBytes(count.ToString("X") + "\r\n"), false);
                }
                if (MainSocket != null)
                {
                    MainSocket.Send(buffer, offset, count, false);
                }
                if (MainSocket != null)
                {
                    MainSocket.Send(U.GetBytes("\r\n"), Tag);
                }
            }
        }
        private void FinishChunked(object Tag)
        {
            lock (this)
            {
                if (MainSocket != null)
                {
                    MainSocket.Send(U.GetBytes("0\r\n\r\n"), Tag);
                }
            }
        }

        private void StreamSendCallback(IAsyncResult result)
        {
            Stream SObject = (Stream)result.AsyncState;
            Info infoObj;

            lock (StateQueue)
            {
                infoObj = (Info)StateQueue.Peek();
            }

            int BytesRead = 0;
            try
            {
                BytesRead = SObject.EndRead(result);
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "HttpSessions");
            }

            if (BytesRead > 0)
            {
                if (infoObj.RangeList == null)
                {
                    if (MainSocket != null)
                    {
                        if (Headers.Version == "1.0")
                        {
                            // 1.0
                            lock (this)
                            {
                                if (MainSocket != null)
                                {
                                    MainSocket.Send(SendBuffer, 0, BytesRead, SObject);
                                }
                            }
                        }
                        else
                        {
                            // 1.1+ , so lets CHUNK
                            SendChunked(SendBuffer, 0, BytesRead, SObject);
                        }
                    }
                }
                else
                {
                    //Range Request
                    if (infoObj.RangeList.Length > 1)
                    {
                        if (infoObj.RangeList[infoObj.RangeIndex].RangeLeft == infoObj.RangeList[infoObj.RangeIndex].Length)
                        {
                            lock (this)
                            {
                                if (MainSocket != null)
                                {
                                    MainSocket.Send(U.GetBytes(infoObj.RangeSeparator));
                                    MainSocket.Send(U.GetBytes("Content-Type: " + infoObj.RangeContentType + "\r\nContent-range: bytes " + infoObj.RangeList[infoObj.RangeIndex].Position.ToString() + "-" + infoObj.RangeList[infoObj.RangeIndex].Length.ToString() + "/" + SObject.Length.ToString() + "\r\n"));
                                }
                            }
                        }
                    }


                    lock (this)
                    {
                        if (MainSocket != null)
                        {
                            if (infoObj.RangeList.Length == 1)
                            {
                                //Single Response
                                MainSocket.Send(SendBuffer, 0, BytesRead, SObject);
                            }
                            else
                            {
                                //Multipart Response
                                MainSocket.Send(SendBuffer, 0, BytesRead, SObject);
                            }
                        }
                    }
                    infoObj.RangeList[infoObj.RangeIndex].RangeLeft -= BytesRead;
                    if (infoObj.RangeList[infoObj.RangeIndex].RangeLeft == 0)
                    {
                        ++infoObj.RangeIndex;
                        if (infoObj.RangeIndex != infoObj.RangeList.Length)
                        {
                            try
                            {
                                SObject.Seek(infoObj.RangeList[infoObj.RangeIndex].Position, SeekOrigin.Begin);
                            }
                            catch (Exception ex)
                            {
                                EventLogger.Log(ex, "HttpSessions");
                            }
                        }
                    }
                }
            }
            else
            {
                //End of Stream

                if (infoObj.RangeList == null)
                {
                    if (Headers.Version != "1.0")
                    {
                        // 1.1+
                        FinishChunked(false);
                    }
                }
                else
                {
                    if (infoObj.RangeList.Length == 1)
                    {
                        //Single Response
                    }
                }
                OnStreamDoneEvent.Fire(this, SObject);
                // Send Complete Event

                lock (StateQueue)
                {
                    StateQueue.Dequeue();
                }

                lock (SendQueue)
                {
                    SendQueue.Dequeue();
                    if (SendQueue.Count > 0)
                    {
                        object tempObject = SendQueue.Peek();
                        if (tempObject is Info info)
                        {
                            // Another Stream Object is in the Pipeline
                            lock (this)
                            {
                                if (MainSocket != null)
                                {
                                    MainSocket.Send(U.GetBytes(info.StringPacket), info.CurrentStreamObject);
                                }
                            }
                        }
                    }
                    else
                    {
                        SET_REQUEST_ANSWERED();
                    }
                }


            }

        }

        private void ParseQueue()
        {
            lock (SendQueue)
            {
                SendQueue.Dequeue();
                if (SendQueue.Count > 0)
                {
                    object obj = SendQueue.Peek();
                    if (obj.GetType().FullName == "OSTL.UPnP.HTTPMessage")
                    {
                        MainSocket.Send(((HTTPMessage)obj).RawPacket, true);
                        if (((HTTPMessage)obj).StatusCode >= 200) SetRequestAnswered = true;
                    }
                    else
                    {
                        object[] pair = (object[])obj;
                        Stream sobj = (Stream)pair[0];
                        string Packet = (string)pair[1];

                        MainSocket.Send(U.GetBytes(Packet), sobj);
                    }
                }
                else if (SetRequestAnswered)
                {
                    // Send CompleteEvent
                    SET_REQUEST_ANSWERED();
                }
            }
        }

        public void SendStreamObject(Stream SObject, Range[] Ranges, string ContentType)
        {
            Info infoObj = new();

            infoObj.CurrentStreamObject = SObject;

            if (Ranges != null && Ranges.Length > 1)
            {
                infoObj.RangeSeparator = "**" + Guid.NewGuid().ToString() + "**";
                infoObj.RangeContentType = ContentType;
            }

            String Packet = "";

            if (Ranges == null && Headers.Version == "1.0")
            {
                // Oldskool Server
                Packet = "HTTP/1.0 200 OK\r\n";
            }
            else if (Ranges == null)
            {
                // 1.1 or better server
                Packet = "HTTP/1.1 200 OK\r\n";
                Packet += "Transfer-Encoding: Chunked\r\n";
            }
            if (Ranges != null)
            {
                Packet = "HTTP/1.1 206 Partial Content\r\n";
                infoObj.RangeList = Ranges;

                try
                {
                    SObject.Seek(infoObj.RangeList[0].Position, SeekOrigin.Begin);
                }
                catch (Exception ex)
                {
                    EventLogger.Log(ex, "HttpSessions");
                    //2ToDo: Fail This for invalid range
                }
                if (SObject.Length - SObject.Position < infoObj.RangeList[0].Length)
                {
                    infoObj.RangeList[0].Length = SObject.Length - SObject.Position;
                }

                if (infoObj.RangeList.Length == 1)
                {
                    Packet += "Content-Range: bytes " + infoObj.RangeList[0].Position.ToString() + "-" + (infoObj.RangeList[0].Position + infoObj.RangeList[0].Length - 1).ToString() + "/" + SObject.Length.ToString() + "\r\nContent-Length: " + infoObj.RangeList[0].Length.ToString() + "\r\n";

                    Packet += "Content-Type: " + ContentType + "\r\n";
                }
                else
                {
                    Packet += "Content-type: multipart/byteranges; boundary=" + infoObj.RangeSeparator + "\r\n";
                }
            }
            else
            {
                Packet += "Content-Type: " + ContentType + "\r\n";
            }
            Packet += "Server: MiniWebServer\r\n";
            Packet += "\r\n";


            infoObj.StringPacket = Packet;
            lock (StateQueue)
            {
                StateQueue.Enqueue(infoObj);
            }

            lock (SendQueue)
            {
                SendQueue.Enqueue(infoObj);
                if (SendQueue.Count == 1)
                {
                    // InitiateRead
                    infoObj.StringPacket = null;
                    MainSocket.Send(U.GetBytes(Packet), SObject);
                }
            }
        }
        /// <summary>
        /// HTTP/1.1 200 OK, a stream object
        /// </summary>
        /// <param name="SObject">The Stream Object</param>
        /// <param name="length">How much of the object to send? 0 if all</param>
        /// <param name="ContentType">Content Type</param>
        public void SendStreamObject(Stream SObject, long length, string ContentType)
        {
            Info InfoObj = new();
            InfoObj.CurrentStreamObject = SObject;

            String Packet;
            if (Headers.Version == "1.0")
            {
                // Oldskool Server
                Packet = "HTTP/1.0 200 OK\r\n";
                // NKIDD - Added Content-Length for 1.0 GET requests
                if (length > 0)
                {
                    Packet += "Content-Length: " + length.ToString() + "\r\n";
                }
            }
            else
            {
                // 1.1 or better server
                Packet = "HTTP/1.1 200 OK\r\n";
                Packet += "Transfer-Encoding: Chunked\r\n";
            }
            Packet += "Server: MiniWebServer\r\n";
            Packet += "Content-Type: " + ContentType + "\r\n\r\n";

            InfoObj.StringPacket = Packet;

            lock (StateQueue)
            {
                StateQueue.Enqueue(InfoObj);
            }

            lock (SendQueue)
            {

                SendQueue.Enqueue(InfoObj);
                if (SendQueue.Count == 1)
                {
                    // InitiateRead
                    InfoObj.StringPacket = null;
                    MainSocket.Send(U.GetBytes(Packet), SObject);
                }
            }
        }
        /// <summary>
        /// HTTP/1.1 200 OK an entire StreamObject
        /// </summary>
        /// <param name="SObject">The Stream Object</param>
        /// <param name="ContentType">Content-type</param>
        public void SendStreamObject(Stream SObject, string ContentType)
        {
            //			SendStreamObject(SObject,0,ContentType);
            SendStreamObject(SObject, null, ContentType);
        }

        private void HandleStreamReady(object Tag)
        {
            if (PostStream != null)
            {
                try
                {
                    if (PostStream.Position == EndPosition)
                    {
                        OnStreamDoneEvent.Fire(this, PostStream);
                        return;
                    }

                    int BytesRead = PostStream.Read(StreamSendBuffer, 0, StreamSendBuffer.Length);
                    if (BytesRead > 0)
                    {
                        MainSocket.Send(StreamSendBuffer, 0, BytesRead, Tag);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    EventLogger.Log(ex, "HttpSessions");
                }
            }
        }

        private void HandleReady(object Tag)
        {
            Stream SObject;
            Info infoObj;
            infoObj.RangeList = null;
            infoObj.RangeIndex = 0;

            lock (StateQueue)
            {
                if (StateQueue.Count > 0)
                {
                    infoObj = (Info)StateQueue.Peek();
                }
            }

            if (Tag != null)
            {
                if (Tag.GetType().FullName == "System.Boolean")
                {
                    if ((bool)Tag)
                    {
                        ParseQueue();
                    }
                }
                else
                {
                    SObject = (Stream)Tag;
                    if (IsConnected)
                    {
                        int BytesToRead = 4096;
                        if (infoObj.RangeList != null)
                        {
                            if (infoObj.RangeIndex == infoObj.RangeList.Length)
                            {
                                BytesToRead = 0;
                            }
                            else if (infoObj.RangeList[infoObj.RangeIndex].RangeLeft < 4096)
                            {
                                BytesToRead = (int)infoObj.RangeList[infoObj.RangeIndex].RangeLeft;
                            }
                        }
                        SObject.BeginRead(SendBuffer, 0, BytesToRead, StreamSendCallback, SObject);
                    }
                }

            }
            OnSendReadyEvent.Fire(Tag);
        }
        private void HandleDisconnect(AsyncSocket sender)
        {
            Connected = false;
            lock (this)
            {
                MainSocket = null;
            }
            if (NeedToWaitToClose)
            {
                if (UserStream == null)
                {
                    SocketStream.Flush();
                    Headers.BodyBuffer = SocketStream.ToArray();
                    EventLogger.Log(this, EventLogEntryType.Information, Headers.StringPacket);
                    OnSniffPacketEvent.Fire(this, Headers);
                    OnReceiveEvent.Fire(this, Headers);
                }
                DONE_ReadingPacket();
            }

            if (PostStream != null)
            {
                OnStreamDoneEvent.Fire(this, PostStream);
            }
            Info infoObj;
            infoObj.CurrentStreamObject = null;
            lock (StateQueue)
            {
                if (StateQueue.Count > 0)
                {
                    infoObj = (Info)StateQueue.Peek();
                }
            }
            if (infoObj.CurrentStreamObject != null)
            {
                OnStreamDoneEvent.Fire(this, infoObj.CurrentStreamObject);
            }

            OnClosedEvent.Fire(this);
        }

        /// <summary>
        /// Unhooks all events
        /// </summary>
        public void CancelAllEvents()
        {
            OnClosedEvent.UnRegisterAll();
            OnCreateFailedEvent.UnRegisterAll();
            OnCreateSessionEvent.UnRegisterAll();
            OnHeaderEvent.UnRegisterAll();
            OnSendReadyEvent.UnRegisterAll();
            OnSniffEvent.UnRegisterAll();
            OnSniffPacketEvent.UnRegisterAll();
            OnStreamDoneEvent.UnRegisterAll();
            OnRequestAnsweredEvent.UnRegisterAll();
            OnReceiveEvent.UnRegisterAll();
        }
        private void HandleConnectFailed(AsyncSocket sender)
        {
            OnCreateFailedEvent.Fire(this);
        }
        private void HandleConnect(AsyncSocket sender)
        {
            MainSocket.OnReceive += HandleReceive;
            /* This is New */
            //MainSocket.BufferReadLength = 1;
            MainSocket.BufferReadLength = 1024;

            Connected = true;
            OnCreateSessionEvent.Fire(this);
            MainSocket.Begin();
        }

        private void SET_START_OF_REQUEST()
        {
            Interlocked.Increment(ref _Counter);
            SetRequestAnswered = false;
            Monitor.SetActive();
        }
        private void DONE_ReadingPacket()
        {
            OnRequestAnsweredEvent.Fire(this);
        }
        private void SET_REQUEST_ANSWERED()
        {
            OnRequestAnsweredEvent.Fire(this);
            if (Interlocked.Decrement(ref _Counter) == 0)
            {
                if (Headers.Version == "1.0" || Headers.Version == "0.9")
                {
                    Close();
                }
                else if (Headers.GetTag("connection").ToUpper() == "CLOSE")
                {
                    Close();
                }
                Monitor.SetInactive();
            }
        }

        private void HandleReceive(AsyncSocket sender, Byte[] buffer, int BeginPointer, int BufferSize, int BytesRead, IPEndPoint source, IPEndPoint remote)
        {

            if (BytesRead != 0)
            {
                OnSniffEvent.Fire(buffer, BufferSize - BytesRead, BytesRead);
            }


            if (FinishedHeader == false)
            {
                if (BufferSize < 4)
                {
                    sender.BufferReadLength = 1;
                    sender.BufferBeginPointer = 0;
                    return;
                }
                /* This is New */
                for (int i = 4; i < BufferSize - 4; ++i)
                {
                    if ((buffer[i - 4] == 13) && (buffer[i - 3] == 10) &&
                        (buffer[i - 2] == 13) && (buffer[i - 1] == 10))
                    {
                        BufferSize = i;
                        break;
                    }
                }


                if ((buffer[BufferSize - 4] == 13) && (buffer[BufferSize - 3] == 10) &&
                    (buffer[BufferSize - 2] == 13) && (buffer[BufferSize - 1] == 10))
                {
                    // End Of Headers
                    Headers = HTTPMessage.ParseByteArray(buffer, 0, BufferSize);


                    if (Headers.StatusCode != -1)
                    {
                        if (Headers.StatusCode >= 100 && Headers.StatusCode <= 199)
                        {
                            //Informational

                            EventLogger.Log(this, EventLogEntryType.Information, "<<IGNORING>>\r\n" + Headers.StringPacket);

                            //							OnHeaderEvent.Fire(this,Headers, UserStream);
                            OnSniffPacketEvent.Fire(this, Headers);
                            //							OnReceiveEvent.Fire(this, Headers);
                            //							DONE_ReadingPacket();
                            BeginHeader(BufferSize);
                            return;
                        }

                        if (Headers.StatusCode == 204 || Headers.StatusCode == 304)
                        {
                            //No Body or No Change

                            EventLogger.Log(this, EventLogEntryType.Information, Headers.StringPacket);

                            OnHeaderEvent.Fire(this, Headers, UserStream);
                            OnSniffPacketEvent.Fire(this, Headers);
                            OnReceiveEvent.Fire(this, Headers);
                            DONE_ReadingPacket();
                            BeginHeader(BufferSize);
                            return;
                        }
                    }
                    else
                    {
                        SET_START_OF_REQUEST();
                    }

                    FinishedHeader = true;
                    if (Headers.GetTag("Content-Length") == "")
                    {
                        if (Headers.GetTag("Transfer-Encoding").ToUpper() == "CHUNKED")
                        {
                            IsChunked = true;
                        }
                        else
                        {
                            if (Headers.StatusCode != -1)
                            {
                                NeedToWaitToClose = true;
                            }
                        }
                    }
                    else
                    {
                        if (Headers.GetTag("Transfer-Encoding").ToUpper() == "CHUNKED")
                        {
                            IsChunked = true;
                        }
                        else
                        {
                            BodySize = int.Parse(Headers.GetTag("Content-Length"));
                        }
                    }
                    if (Headers.GetTag("Connection").ToUpper() == "CLOSE")
                    {
                        ConnectionCloseSpecified = true;
                    }

                    if (!IsChunked && NeedToWaitToClose && !ConnectionCloseSpecified && !IsLegacy && Headers.Version != "1.0")
                    {
                        NeedToWaitToClose = false;
                        BodySize = 0;
                    }

                    OnHeaderEvent.Fire(this, Headers, UserStream);
                    if (NeedToWaitToClose)
                    {
                        sender.BufferBeginPointer = BufferSize;
                        sender.BufferReadLength = 4096;
                    }
                    else
                    {
                        if (IsChunked)
                        {
                            // Chunked
                            BeginChunk(BufferSize);
                        }
                        else
                        {
                            // Normal
                            if (BodySize == 0)
                            {
                                // Already have the packet

                                EventLogger.Log(this, EventLogEntryType.Information, Headers.StringPacket);

                                OnSniffPacketEvent.Fire(this, Headers);
                                OnReceiveEvent.Fire(this, Headers);
                                if (UserStream != null)
                                {
                                    UserStream.Flush();
                                    OnStreamDoneEvent.Fire(this, UserStream);
                                }
                                DONE_ReadingPacket();
                                BeginHeader(BufferSize);
                            }
                            else
                            {
                                if (BodySize <= 4096)
                                {
                                    sender.BufferBeginPointer = BufferSize;
                                    sender.BufferReadLength = BodySize;
                                }
                                else
                                {
                                    sender.BufferBeginPointer = BufferSize;
                                    sender.BufferReadLength = 4096;
                                }
                            }
                        } // End Normal Else Clause
                    } // End Non HTTP/1.0 Else Clause
                    return;
                } // End of Processing Header
                  // Need to read more of the header
                sender.BufferBeginPointer = 0;
                sender.BufferReadLength = 1;
                return;
            } // End of If FinishedHeader


            // Have some body data
            if (IsChunked == false)
            {
                /* This is New */
                if (NeedToWaitToClose == false)
                {
                    if (BufferSize > BodySize) BufferSize = BodySize;
                }

                // Normal Data
                if (UserStream != null)
                {
                    UserStream.Write(buffer, 0, BufferSize);
                }
                else
                {
                    //					OpenSource.Utilities.EventLogger.Log(this,System.Diagnostics.EventLogEntryType.SuccessAudit,"NONChunk Data: " + BufferSize.ToString() + " bytes");
                    //					OpenSource.Utilities.EventLogger.Log(this,System.Diagnostics.EventLogEntryType.SuccessAudit,"Writing["+SocketStream.GetHashCode().ToString()+"]: " + U.GetString(buffer,0,BufferSize));

                    SocketStream.Write(buffer, 0, BufferSize);
                }

                if (NeedToWaitToClose == false)
                {
                    BodySize -= BufferSize;
                    if (BodySize > 0)
                    {
                        // More To Read
                        sender.BufferBeginPointer = BufferSize;
                        if (BodySize < 4096)
                        {
                            sender.BufferReadLength = BodySize;
                        }
                        else
                        {
                            sender.BufferReadLength = 4096;
                        }
                    }
                    else
                    {
                        // Finished Reading
                        if (UserStream == null)
                        {
                            SocketStream.Flush();
                            Headers.BodyBuffer = SocketStream.ToArray();

                            EventLogger.Log(this, EventLogEntryType.Information, Headers.StringPacket);

                            OnSniffPacketEvent.Fire(this, Headers);
                            OnReceiveEvent.Fire(this, Headers);
                        }
                        else
                        {
                            UserStream.Flush();
                            OnStreamDoneEvent.Fire(this, UserStream);
                        }
                        DONE_ReadingPacket();
                        BeginHeader(BufferSize);
                    }
                }
                else
                {
                    // HTTP/1.0 Socket
                    sender.BufferReadLength = 4096;
                    sender.BufferBeginPointer = BufferSize;
                }
            }
            else
            {
                // Chunked Data
                ProcessChunk(buffer, BufferSize);
            }
        }
        private void BeginHeader(int StartPointer)
        {
            if (MainSocket != null)
            {
                MainSocket.BufferBeginPointer = StartPointer;
                //MainSocket.BufferReadLength = 1;
                /* This is New */
                MainSocket.BufferReadLength = 1024;

                FinishedHeader = false;
                IsChunked = false;
                ChunkedHeadersAdded = false;
                ConnectionCloseSpecified = false;

                UserStream = null;
                SocketStream = new MemoryStream();
            }
        }
        /*
                private void BeginHeader()
                {
                    if (MainSocket != null)
                    {
                        BeginHeader(MainSocket.BufferSize);
                    }
                }
        */

        private void BeginChunk(int StartPosition)
        {
            MainSocket.BufferReadLength = 1024;
            MainSocket.BufferBeginPointer = StartPosition;
            ChunkState = START_CHUNK;
        }
        private void ProcessChunk(byte[] buffer, int BufferSize)
        {

            if (ChunkState == FIN_CHUNK_CRLF)
            {
                if (BufferSize >= 2)
                {
                    BeginChunk(2);
                    return;
                }
                // BYR Buffer needs to accumulate, this caused it to overwrite :)
                //					MainSocket.BufferBeginPointer = BufferSize;
                MainSocket.BufferReadLength = 2 - BufferSize;
                return;
            }

            if (ChunkState == START_CHUNK)
            {
                //Process Start of Chunk
                if (BufferSize < 3)
                {
                    MainSocket.BufferReadLength = 1;
                    MainSocket.BufferBeginPointer = 0;
                    return;
                }
                /* This is New */
                for (int i = 2; i < BufferSize; ++i)
                {
                    if ((buffer[i - 2] == 13) && (buffer[i - 1] == 10))
                    {
                        BufferSize = i;
                        break;
                    }
                }
                if ((buffer[BufferSize - 2] == 13) && (buffer[BufferSize - 1] == 10))
                {
                    string hn = U.GetString(buffer, 0, BufferSize - 2);
                    if (hn.IndexOf(";", StringComparison.Ordinal) != -1)
                    {
                        hn = hn.Substring(0, hn.IndexOf(";", StringComparison.Ordinal));
                    }
                    //ChunkDataSize = HTTPMessage.IntFromHex(hn.ToUpper());
                    ChunkDataSize = int.Parse(hn.ToUpper(), NumberStyles.HexNumber);

                    if (ChunkDataSize != 0)
                    {
                        ChunkState = DATA_CHUNK;
                        MainSocket.BufferBeginPointer = BufferSize;
                        if (ChunkDataSize < 4096)
                        {
                            MainSocket.BufferReadLength = ChunkDataSize;
                        }
                        else
                        {
                            MainSocket.BufferReadLength = 4096;
                        }
                        return;
                    }
                    ChunkState = START_FOOTER_CHUNK;
                    MainSocket.BufferBeginPointer = BufferSize;
                    MainSocket.BufferReadLength = 1;
                    return;
                }
                // More of the StartSection to read
                MainSocket.BufferReadLength = 1;
                MainSocket.BufferBeginPointer = 0;
                return;
            }

            if (ChunkState == DATA_CHUNK)
            {
                /* This is New */
                if (BufferSize > ChunkDataSize) BufferSize = ChunkDataSize;

                // Data Section
                if (UserStream != null)
                {
                    UserStream.Write(buffer, 0, BufferSize);
                }
                else
                {
                    SocketStream.Write(buffer, 0, BufferSize);
                }

                ChunkDataSize -= BufferSize;
                if (ChunkDataSize == 0)
                {
                    ChunkState = FIN_CHUNK_CRLF;
                    MainSocket.BufferReadLength = 2;
                    MainSocket.BufferBeginPointer = BufferSize;
                    return;
                }
                if (ChunkDataSize < 4096)
                {
                    MainSocket.BufferReadLength = ChunkDataSize;
                }
                else
                {
                    MainSocket.BufferReadLength = 4096;
                }
                MainSocket.BufferBeginPointer = BufferSize;
                return;
            }

            if (ChunkState == START_FOOTER_CHUNK)
            {
                if (BufferSize < 2)
                {
                    MainSocket.BufferBeginPointer = 0;
                    MainSocket.BufferReadLength = 1;
                    return;
                }
                /* This is New */
                for (int i = 2; i < BufferSize; ++i)
                {
                    if ((buffer[i - 2] == 13) && (buffer[i - 1] == 10))
                    {
                        BufferSize = i;
                        break;
                    }
                }
                if ((buffer[BufferSize - 2] == 13) && (buffer[BufferSize - 1] == 10))
                {
                    if (BufferSize == 2)
                    {
                        // End of Chunk Session
                        if (UserStream == null)
                        {
                            Headers.BodyBuffer = SocketStream.ToArray();
                            Headers.RemoveTag("Transfer-Encoding");

                            EventLogger.Log(this, EventLogEntryType.Information, Headers.StringPacket);

                            OnSniffPacketEvent.Fire(this, Headers);
                            OnReceiveEvent.Fire(this, Headers);
                            DONE_ReadingPacket();
                            BeginHeader(BufferSize);
                            return;
                        }
                        UserStream.Flush();
                        OnHeaderEvent.Fire(this, Headers, UserStream);
                        OnStreamDoneEvent.Fire(this, UserStream);
                        DONE_ReadingPacket();
                        BeginHeader(BufferSize);
                        return;
                    }
                    // Process 'da Footers
                    string f = U.GetString(buffer, 0, BufferSize - 2);
                    string h, hv;
                    ChunkedHeadersAdded = true;

                    h = f.Substring(0, f.IndexOf(":", StringComparison.Ordinal));
                    hv = f.Substring(f.IndexOf(":", StringComparison.Ordinal) + 1);
                    Headers.AddTag(h.Trim(), hv.Trim());

                    // There Are More Footers
                    MainSocket.BufferBeginPointer = BufferSize;
                    MainSocket.BufferReadLength = 1;
                    return;
                }
                // Continue Reading Footer
                MainSocket.BufferBeginPointer = 0;
                MainSocket.BufferReadLength = 1;
            }
        }
        /// <summary>
        /// The IPEndPoint that initiated this Session.
        /// </summary>
        public IPEndPoint Source
        {
            get
            {
                if (MainSocket.LocalEndPoint != null)
                {
                    return ((IPEndPoint)MainSocket.LocalEndPoint);
                }
                return (local_ep);
            }
        }

        /// <summary>
        /// The IPEndPoint that this Session was created for
        /// </summary>
        public IPEndPoint Remote
        {
            get
            {
                return ((IPEndPoint)MainSocket.RemoteEndPoint);
            }
        }

        public void SendChunkedPacketHeaders(HTTPMessage Header)
        {
            string P = Header.Directive + " " + Header.DirectiveObj + " HTTP/" + Header.Version + "\r\n";
            IDictionaryEnumerator i = Header.GetHeaderEnumerator();
            while (i.MoveNext())
            {
                if (((string)i.Key).ToUpper() != "CONTENT-LENGTH")
                {
                    P += (string)i.Value + "\r\n";
                }
            }
            P += "Transfer-Encoding: Chunked\r\n\r\n";
            MainSocket.Send(U.GetBytes(P));
        }
        public void SendChunkedPacketBody(byte[] buffer, int offset, int count, object Tag)
        {
            SendChunked(buffer, offset, count, Tag);
        }
        public void SendEndChunkPacket(object Tag)
        {
            FinishChunked(Tag);
        }

        /// <summary>
        /// Sends a Packet to the computer connected to this session
        /// </summary>
        /// <param name="Packet">The Packet to Send</param>
        public void Send(HTTPMessage Packet)
        {
            OnSniffEvent.Fire(Packet.RawPacket, 0, Packet.RawPacket.Length);
            OnSniffPacketEvent.Fire(this, Packet.Clone());
            EventLogger.Log(this, EventLogEntryType.Information, Packet.StringPacket);


            if (Packet.Version == "1.0")
            {
                IsLegacy = true;
            }
            else
            {
                IsLegacy = false;
            }

            lock (SendQueue)
            {
                if (SendQueue.Count == 0)
                {
                    if (MainSocket != null)
                    {
                        MainSocket.Send(Packet.RawPacket);
                        if (Packet.StatusCode >= 200) SET_REQUEST_ANSWERED();
                    }
                }
                else
                {
                    SendQueue.Enqueue(Packet);
                }
            }

            /*
			if (Packet.StatusCode>=200)
			{
				// Send Complete Event
				this.SET_REQUEST_ANSWERED();
			}
			*/
        }

        /// <summary>
        /// Close the session
        /// </summary>
        public void Close()
        {
            try
            {
                if (MainSocket != null)
                {
                    MainSocket.Close();
                }
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "HttpSessions");
            }
            MainSocket = null;
        }
    }
}
