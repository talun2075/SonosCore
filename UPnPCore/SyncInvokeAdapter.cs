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

using System.Threading;

namespace OSTL.UPnP
{
	/// <summary>
	/// A helper class used by UPnPService to build a blocking implementation on top of non-blocking code
	/// </summary>
	public sealed class SyncInvokeAdapter
	{
		public ManualResetEvent Result = new(false);
		public object ReturnValue = null;
		public UPnPArgument[] Arguments = System.Array.Empty<UPnPArgument>();
		public UPnPInvokeException InvokeException = null;

		public UPnPService.UPnPServiceInvokeHandler InvokeHandler = null;
		public UPnPService.UPnPServiceInvokeErrorHandler InvokeErrorHandler = null;

		public SyncInvokeAdapter()
		{
			InvokeHandler = InvokeSink;
			InvokeErrorHandler = InvokeFailedSink;
		}

		private void InvokeSink(UPnPService sender, string MethodName, UPnPArgument[] Args, object Val, object Tag)
		{
			ReturnValue = Val;
			Arguments = Args;
			Result.Set();
		}
		private void InvokeFailedSink(UPnPService sender, string MethodName, UPnPArgument[] Args, UPnPInvokeException e, object Tag)
		{
			Arguments = Args;
			InvokeException = e;
			Result.Set();
		}
	}
}
