using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.RSControls.Helpers
{
    public static class MainThread
    {
        public static System.Threading.Tasks.Task InvokeOnMainThread(System.Action action)
        {
            if (Xamarin.Essentials.MainThread.IsMainThread)
            {
                action();
                return System.Threading.Tasks.Task.CompletedTask;
            }

            System.Threading.Tasks.TaskCompletionSource<bool> tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            tcs.Task.Wait();
            return tcs.Task;
        }
    }
}
