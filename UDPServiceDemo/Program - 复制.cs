using System.Net;
using System.Net.Sockets;
using System.Text;

class UDPServer
{
    //Select
    public static void Main2(string[] args)
    {
        // 创建一个 UDP Socket
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // 绑定本地 IP 地址和端口号
        IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 12345);
        socket.Bind(localEP);

        // 设置 Select 方法需要监听的 Socket 集合
        var checkRead = new[] { socket };

        // 循环监听
        while (true)
        {
            // 使用 Select 方法监听可读状态的 Socket
            Socket.Select(checkRead, null, null, 1000);

            // 从收到数据的 Socket 中读取数据
            byte[] buffer = new byte[1024];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            int length = socket.ReceiveFrom(buffer, ref remoteEP);

            // 处理接收到的数据
            string data = Encoding.ASCII.GetString(buffer, 0, length);
            Console.WriteLine("Received: {0} from {1}", data, remoteEP.ToString());
        }
    }

    //Poll
    public static void Main3(string[] args)
    {
        // 创建一个 UDP Socket
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // 绑定本地 IP 地址和端口号
        IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 12345);
        socket.Bind(localEP);

        // 循环监听
        while (true)
        {
            // 使用 Poll 方法监听可读状态的 Socket
            if (socket.Poll(1000, SelectMode.SelectRead))
            {
                // 从收到数据的 Socket 中读取数据
                byte[] buffer = new byte[1024];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                int length = socket.ReceiveFrom(buffer, ref remoteEP);

                // 处理接收到的数据
                string data = Encoding.ASCII.GetString(buffer, 0, length);
                Console.WriteLine("Received: {0} from {1}", data, remoteEP.ToString());
            }
        }
    }

    //
    public static void Main4(string[] args)
    {
        // 创建一个 Socket 对象
        var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        // 绑定套接字到本地终结点
        var localEndPoint = new IPEndPoint(IPAddress.Any, 12345);
        serverSocket.Bind(localEndPoint);

        Console.WriteLine("Server started.");

        while (true)
        {
            // 使用 Select 方法等待套接字的可读事件
            var readList = new List<Socket>() { serverSocket };
            Socket.Select(readList, null, null, 10000);
            if (readList.Any())
            {
                foreach (var item in readList)
                {
                    byte[] buffer = new byte[1024];
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    int length = item.ReceiveFrom(buffer, ref remoteEP);
                }
            }
        }
    }
}
