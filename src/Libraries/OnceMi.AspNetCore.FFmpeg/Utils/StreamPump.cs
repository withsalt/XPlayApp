using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.FFmpeg.Utils
{
    public enum StreamPumpResult
    {
        Aborted,
        Delivered
    };

    public sealed class StreamPump : IDisposable
    {
        private readonly byte[] buffer;

        private readonly SemaphoreSlim sem = new SemaphoreSlim(0, 1);

        private EventHandler<EventArgs> _streamPumpFinishedEvent = null;

        public StreamPump(Stream inputStream, Stream outputStream, int bufferSize)
        {
            buffer = new byte[bufferSize];
            Input = inputStream;
            Output = outputStream;
        }

        public Stream Input { get; private set; }

        public Stream Output { get; private set; }

        private void Finish(StreamPumpResult result)
        {
            if (_streamPumpFinishedEvent != null)
            {
                _streamPumpFinishedEvent?.Invoke(this, new EventArgs());
            }
            try
            {
                sem.Release();
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Dispose()
        {
            sem.Dispose();
        }

        public void Pump()
        {
            try
            {
                Input.BeginRead(buffer, 0, buffer.Length, readResult =>
                {
                    try
                    {
                        var read = Input.EndRead(readResult);
                        if (read <= 0)
                        {
                            Finish(StreamPumpResult.Delivered);
                            return;
                        }

                        try
                        {
                            Output.BeginWrite(buffer, 0, read, writeResult =>
                            {
                                try
                                {
                                    Output.EndWrite(writeResult);
                                    Pump();
                                }
                                catch (Exception)
                                {
                                    Finish(StreamPumpResult.Aborted);
                                }
                            }, null);
                        }
                        catch (Exception)
                        {
                            Finish(StreamPumpResult.Aborted);
                        }
                    }
                    catch (Exception)
                    {
                        Finish(StreamPumpResult.Aborted);
                    }
                }, null);
            }
            catch (Exception)
            {
                Finish(StreamPumpResult.Aborted);
            }
        }

        public bool Wait(int timeout)
        {
            return sem.Wait(timeout);
        }

        public StreamPump WithFinished(EventHandler<EventArgs> @event)
        {
            this._streamPumpFinishedEvent = @event;
            return this;
        }
    }
}
