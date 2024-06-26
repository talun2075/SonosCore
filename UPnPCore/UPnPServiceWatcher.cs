///*   
//Copyright 2006 - 2010 Intel Corporation

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//   http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
//*/

//namespace OSTL.UPnP
//{
//	/// <summary>
//	/// Summary description for UPnPServiceWatcher.
//	/// </summary>
//	public class UPnPServiceWatcher
//	{
//		public delegate void SniffHandler(UPnPServiceWatcher sender, byte[] raw, int offset, int length);
//		public delegate void SniffPacketHandler(UPnPServiceWatcher sender, HTTPMessage MSG);

//		public event SniffHandler OnSniff;
//		public event SniffPacketHandler OnSniffPacket;

//		public UPnPService ServiceThatIsBeingWatched
//		{
//			get
//			{
//				return(_S);
//			}
//		}

//		private readonly UPnPService _S;

//		~UPnPServiceWatcher()
//		{
//			_S.OnSniff -= SniffSink;
//			_S.OnSniffPacket -= SniffPacketSink;
//		}
 
//		public UPnPServiceWatcher(UPnPService S, SniffHandler cb):this(S,cb,null)
//		{
//		}
//		public UPnPServiceWatcher(UPnPService S, SniffHandler cb, SniffPacketHandler pcb)
//		{
//			OnSniff += cb;
//			OnSniffPacket += pcb;
//			_S = S;

//			_S.OnSniff += SniffSink;
//			_S.OnSniffPacket += SniffPacketSink;
//		}

//		protected void SniffSink(byte[] raw, int offset, int length)
//		{
//            OnSniff?.Invoke(this, raw, offset, length);
//        }
//		protected void SniffPacketSink(UPnPService sender, HTTPMessage MSG)
//		{
//            OnSniffPacket?.Invoke(this, MSG);
//        }
//	}
//}
