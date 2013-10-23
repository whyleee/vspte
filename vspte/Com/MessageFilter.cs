using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace vspte.Com
{
    public class MessageFilter : MarshalByRefObject, IDisposable, IMessageFilter
    {
        [DllImport("ole32.dll")]
        [PreserveSig]
        private static extern int CoRegisterMessageFilter(
            IMessageFilter lpMessageFilter,
            out IMessageFilter lplpMessageFilter
        );

        private readonly IMessageFilter oldFilter;
        private const int SERVERCALL_ISHANDLED = 0;
        private const int PENDINGMSG_WAITNOPROCESS = 2;
        private const int SERVERCALL_RETRYLATER = 2;

        public MessageFilter()
        {
            // Starting IMessageFilter for COM objects
            var hr = CoRegisterMessageFilter(this, out oldFilter);
            Debug.Assert(hr >= 0, "Registering COM IMessageFilter failed!");
        }

        public void Dispose()
        {
            // Disabling IMessageFilter
            IMessageFilter dummy;
            var hr = CoRegisterMessageFilter(oldFilter, out dummy);
            Debug.Assert(hr >= 0, "De-Registering COM IMessageFilter failed!");
            GC.SuppressFinalize(this);
        }

        int IMessageFilter.HandleInComingCall(int dwCallType, IntPtr threadIdCaller,
            int dwTickCount, IntPtr lpInterfaceInfo)
        {
            // Return the ole default (don't let the call through).
            return SERVERCALL_ISHANDLED;
        }

        int IMessageFilter.RetryRejectedCall(IntPtr threadIDCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == SERVERCALL_RETRYLATER)
            {
                // Retry the thread call immediately if return >=0 & <100.
                return 150; // waiting 150 mseconds until retry
            }
            // Too busy; cancel call. SERVERCALL_REJECTED
            return -1;
            // Call was rejected by callee. 
            // (Exception from HRESULT: 0x80010001 (RPC_E_CALL_REJECTED))
        }

        int IMessageFilter.MessagePending(IntPtr threadIDCallee, int dwTickCount, int dwPendingType)
        {
            // Perform default processing.
            return PENDINGMSG_WAITNOPROCESS;
        }
    }
}