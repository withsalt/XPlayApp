using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XPlayApp.Services.XPlay.Client
{
    public class ErrorArgs : EventArgs
    {
        public bool IsSocketError { get; set; } = false;

        public SocketError SocketError { get; set; }

        public Exception Exception { get; set; }
    }

    public class ReceivedArgs : EventArgs
    {
        public byte[] Buffer { get; set; }

        public long Offset { get; set; }

        public long Size { get; set; }

        public string MessageString
        {
            get
            {
                if (this.Buffer != null && this.Buffer.Length > 0)
                {
                    return Encoding.UTF8.GetString(this.Buffer);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }


    class TcpClient : ITcpClient
    {
        private event EventHandler<EventArgs> _disconnectedEvent;
        private event EventHandler<EventArgs> _connectedEvent;
        private event EventHandler<ErrorArgs> _errorEvent;
        private event EventHandler<ReceivedArgs> _receivedEvent;

        private bool _isAutoReconnect = false;
        private bool _isStillWaiting = false;
        public byte[] _waitResult = null;

        public bool Status { get; private set; }

        public TcpClient(string ip, int port) : base(ip, port)
        {

        }

        #region with method

        public TcpClient WithAutoReconnect()
        {
            _isAutoReconnect = true;
            return this;
        }

        public TcpClient WithConnected(EventHandler<EventArgs> connectedEvent)
        {
            this._connectedEvent = connectedEvent;
            return this;
        }

        public TcpClient WithDisconnected(EventHandler<EventArgs> disconnectedEvent)
        {
            this._disconnectedEvent = disconnectedEvent;
            return this;
        }

        public TcpClient WithReceived(EventHandler<ReceivedArgs> receivedEvent)
        {
            this._receivedEvent = receivedEvent;
            return this;
        }

        public TcpClient WithError(EventHandler<ErrorArgs> errorEvent)
        {
            this._errorEvent = errorEvent;
            return this;
        }

        #endregion

        #region 事件

        protected override void OnConnected()
        {
            this.Status = true;
            _connectedEvent?.Invoke(this, new EventArgs());
        }

        protected override void OnDisconnected()
        {
            this.Status = false;
            _disconnectedEvent?.Invoke(this, new EventArgs());
            if (_isAutoReconnect)
            {
                Thread.Sleep(1500);
                _errorEvent?.Invoke(this, new ErrorArgs()
                {
                    SocketError = new SocketError(),
                    IsSocketError = false,
                    Exception = new Exception("Retry to connect server.")
                });
                this.ConnectAsync();
            }
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            if (_isStillWaiting)
            {
                this._waitResult = buffer;
                this._isStillWaiting = false;
            }
            else
            {
                _receivedEvent?.Invoke(this, new ReceivedArgs()
                {
                    Buffer = buffer,
                    Offset = offset,
                    Size = size
                });
            }
        }

        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            _errorEvent?.Invoke(this, new ErrorArgs()
            {
                SocketError = error,
                IsSocketError = true,
            });
        }

        public override bool SendAsync(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new ArgumentNullException(nameof(message));
                }
                if (!this.Status)
                {
                    throw new Exception("Server not connected.");
                }

                byte[] buffer = Encoding.UTF8.GetBytes(message.ToString());
                return this.SendAsync(buffer);
            }
            catch (Exception ex)
            {
                throw new Exception($"Send message({message}) failed, {ex.Message}", ex);
            }
        }

        private static readonly object locker = new object();

        public string SendAndWait(string message, int timeout = 3000)
        {
            try
            {
                if (timeout < 1000)
                {
                    throw new Exception("timeout value can not less than 1000");
                }
                if (_isStillWaiting)
                {
                    throw new Exception("Client still in send process.");
                }
                if (!this.SendAsync(message))
                {
                    return null;
                }
                lock (locker)
                {
                    this._isStillWaiting = true;
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    while (_isStillWaiting)
                    {
                        if (sw.Elapsed.TotalMilliseconds > timeout)
                            break;
                    }
                    sw.Stop();
                    if (_isStillWaiting)
                    {
                        throw new Exception("Send timeout.");
                    }
                    if (_waitResult != null && _waitResult.Length > 0)
                    {
                        string result = Encoding.UTF8.GetString(this._waitResult);
                        this._waitResult = null;
                        return result;
                    }
                }
                if (this._isStillWaiting)
                {
                    this._isStillWaiting = false;
                }
                return null;
            }
            catch (Exception ex)
            {
                if (this._isStillWaiting)
                {
                    this._isStillWaiting = false;
                }
                throw new Exception($"Send message and wait for server response failed, {ex.Message}", ex);
            }
        }

        #endregion
    }
}
