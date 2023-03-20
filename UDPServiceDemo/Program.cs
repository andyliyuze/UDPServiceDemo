using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    private const int BufferSize = 1024;

    public static void Main(string[] args2)
    {
        UDPServer.Main4(args2);

        var endpoint = new IPEndPoint(IPAddress.Any, 12345);
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(endpoint);

        var buffer = new byte[BufferSize];
        var pool = new SocketAsyncEventArgsPool();
        for (var i = 0; i < Environment.ProcessorCount * 2; i++)
        {
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[BufferSize], 0, BufferSize);
            args.Completed += ReceiveCompleted;
            args.UserToken = pool;
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            pool.Push(args);
        }

        while (true)
        {
            var args = pool.Pop();
            if (args == null)
            {
                Task.Delay(10).Wait();
                continue;
            }
            try
            {
                socket.ReceiveFromAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data: {ex.Message}");
                pool.Push(args);
            }
        }
    }

    private static void ReceiveCompleted(object sender, SocketAsyncEventArgs args)
    {
        Console.WriteLine("RemoteEndPoint:" + args?.RemoteEndPoint?.ToString());
        if (args.SocketError != SocketError.Success)
        {
            Console.WriteLine($"Error receiving data: {args.SocketError}");
        }
        else
        {
            var data = Encoding.ASCII.GetString(args.Buffer, args.Offset, args.BytesTransferred);
            Console.WriteLine($"Received data: {data}");
        }

        var pool = args.UserToken as SocketAsyncEventArgsPool;

        pool.Push(args);

    }
}

public class SocketAsyncEventArgsPool
{
    private readonly object _lock = new object();
    private readonly Stack<SocketAsyncEventArgs> _pool = new Stack<SocketAsyncEventArgs>();

    public int Count => _pool.Count;

    public void Push(SocketAsyncEventArgs item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        lock (_lock) _pool.Push(item);
    }

    public SocketAsyncEventArgs Pop()
    {
        lock (_lock)
        {
            return _pool.Count > 0 ? _pool.Pop() : null;
        }
    }
}
