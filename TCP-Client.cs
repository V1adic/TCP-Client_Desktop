using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Collections.Generic;
using TestGtkApp;
using System.Net.Http;
using System.Linq;


public class Connection : IDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly EndPoint _remoteEndPoint;
    private readonly Task _readingTask;
    private readonly Task _writingTask;
    private readonly Channel<byte[]> _channel;
    bool disposed;

    public Connection(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        _remoteEndPoint = client.Client.RemoteEndPoint;
        _channel = Channel.CreateUnbounded<byte[]>();
        _readingTask = RunReadingLoop();
        _writingTask = RunWritingLoop();
    }

    private async Task RunReadingLoop()
    {
        try
        {
            byte[] headerBuffer = new byte[4];
            while (true)
            {
                int bytesReceived = await _stream.ReadAsync(headerBuffer, 0, headerBuffer.Length);
                if (bytesReceived != 4)
                    break;
                int length = BinaryPrimitives.ReadInt32LittleEndian(headerBuffer);
                byte[] buffer = new byte[length];
                int count = 0;
                while (count < length)
                {
                    bytesReceived = await _stream.ReadAsync(buffer, count, buffer.Length - count);
                    count += bytesReceived;
                }
                string message = Encoding.UTF8.GetString(buffer);

                string[] services = [];

                if (message.Contains(' '))
                {
                    services = message.Split(' ', 2);
                }
                else
                {
                    services = [message];
                }
                switch (services[0])
                {
                    case "/messageSERV":
                        {
                            Console.WriteLine("WORK /messageSERV");
                            if (Program.if_in_Group_Chat)
                            {
                                Program.Chats.chat_label.Text += $"{services[1]}\n";
                            }

                        }
                        break;

                    case "/corr":
                        {
                            Program.Correct_Name = true;
                        }
                        break;

                    case "/updateSERV":
                        {
                            using FileStream request = new(Program.GroupChat, FileMode.Create);
                            request.Write(Encoding.UTF8.GetBytes(services[1]));
                        }
                        break;

                    case "/yourName":
                        {
                            Program.MyName = services[1];
                            Program.Correct_Name = true;
                            Console.WriteLine($"{Program.MyName}");
                        }
                        break;

                    case "/corrOnline":
                        {
                            Program.ifOnline = true;
                            Program.nameCL = Program.tempNameCL;
                            Program.tempNameCL = "";
                        }
                        break;

                    case "/fileServices":
                        {
                            var fileServices = services[1].Split(" ", 2);
                            var indexOf = 0;
                            List<byte> result = [];
                            bool flag = false;


                            for (int j = 0; j < buffer.Length && !flag; j++)
                            {
                                result.Add(buffer[j]);
                                if (Encoding.UTF8.GetString(result.ToArray<byte>()).Contains("<END>"))
                                {
                                    indexOf = j + 1;
                                    flag = true;
                                }
                            }

                            string FileName = fileServices[0];
                            int temp = FileName.LastIndexOf('.');
                            List<string> file = [FileName[..temp], FileName[temp..]];

                            int i = 0;
                            while (File.Exists($"Data/{file[0]} ({i}){file[1]}"))
                            {
                                i++;
                            }

                            File.WriteAllBytes($"Data/{file[0]} ({i}){file[1]}", buffer[indexOf .. ]);

                        }
                        break;
                }

            }
            Console.WriteLine($"Сервер закрыл соединение.");
            _stream.Close();
        }
        catch (IOException)
        {
            Console.WriteLine($"Подключение закрыто.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
        }
    }

    public async Task SendMessageAsync(byte[] message)
    {
        //Console.WriteLine($">> {_remoteEndPoint}: {message}");
        await _channel.Writer.WriteAsync(message);
    }

    private async Task RunWritingLoop()
    {
        byte[] header = new byte[4];
        await foreach (byte[] message in _channel.Reader.ReadAllAsync())
        {
            byte[] buffer = message;
            BinaryPrimitives.WriteInt32LittleEndian(header, buffer.Length);
            await _stream.WriteAsync(header, 0, header.Length);
            await _stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().FullName);
        disposed = true;
        if (_client.Connected)
        {
            _channel.Writer.Complete();
            _stream.Close();
            Task.WaitAll(_readingTask, _writingTask);
        }
        if (disposing)
        {
            _client.Dispose();
        }
    }

    ~Connection() => Dispose(false);
}