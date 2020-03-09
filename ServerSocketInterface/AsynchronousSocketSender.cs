using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerSocketInterface {
    public class AsynchronousSocketSender {

        private static Queue<String> queue = new Queue<String>();

        private static String msg = "";

        private static Boolean running = true;

        public static void StartLoop() {
            while(running) {

                while(queue.Count < 1) {
                    Thread.Sleep(100);
                    if (!running) return;
                }

                msg = queue.Dequeue();

                SendToExternalServer(msg);

            }
        }

        public static void Send(String message) {
            queue.Enqueue(message);
        }

        public static void Stop() {
            running = false;
        }

        private static void SendToExternalServer(String message) {
            try {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 11001;
                TcpClient client = new TcpClient("localhost", port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.Unicode.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Close everything.
                stream.Close();
                client.Close();
            } catch (ArgumentNullException e) {
                Console.WriteLine("ArgumentNullException: {0}", e);
            } catch (SocketException e) {
                Console.WriteLine("SocketException: {0}", e);
            }

        }
    }
}
