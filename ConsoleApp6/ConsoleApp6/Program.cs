using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;

namespace SocketTcpServer
{
    class Program
    {
        // адрес и порт сервера, к которому будем подключаться
        static int port = 8005; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера

        static void Main(string[] args)
        {
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            // создаем сокет, использующий протокол Tcp
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try { 

                //после создания сокета связываем его с локальной точкой
                listenSocket.Bind(ipPoint);
                //cокет будет прослушивать подключения по 8005 порту на локальном адресе 127.0.0.1.
                //далее начинаем прослушивание
                listenSocket.Listen(10);
                Console.WriteLine("The server is running. Waiting for connections...");

                while (true)
                {
                    //после вызова метода Listen начинается прослушивание входящих подключений, и если подключения приходят на сокет, то их можно получить с помощью метода Accept
                    //метод Accept извлекает из очереди ожидающих запрос первый запрос и создает для его обработки объект Socket. Если очередь запросов пуста, то метод Accept блокирует вызывающий поток до появления нового подключения.
                    Socket handler = listenSocket.Accept();

                    //получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    //количество полученных байтов
                    int bytes = 0;
                    //буфер для обмена данными
                    byte[] data = new byte[256];

                    int number;
                    bool cont = true;
                    string numberString = "";
                    string mode = "";
                    string des;
                    //прием метода задания числа (генерация на сервере или ввод клиентом)
                    bytes = handler.Receive(data);
                    mode = Encoding.Unicode.GetString(data, 0, bytes);
                    while (cont)
                    {
                       switch (mode)
                        {
                            case "rnd":
                            {
                                Random rnd = new Random();
                                //генерируем число от 1000 до 9999, которое клиент будет угадывать
                                number = rnd.Next(1000, 10000);                      
                                numberString = number.ToString();
                                break;
                            }

                            case "man":
                            {
                                //получение числа, которые клиент будет угадывать
                                bytes = handler.Receive(data);     
                                //перевод данных в int
                                int.TryParse(Encoding.Unicode.GetString(data, 0, bytes), out number);     
                                numberString = number.ToString();
                                break;
                            }

                            default:
                                 return;
                         }

                        do
                        {
                            //получаем число от клиента
                            bytes = handler.Receive(data);                       
                            string guess = Encoding.Unicode.GetString(data, 0, bytes);
                            int quantOfRightNums = 0;
                            int quantOfRightPlaces = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                for (int k = 0; k < 4; k++)
                                {
                                     if (numberString[i] == guess[k])
                                     {
                                         quantOfRightNums++;
                                         if (i == k)
                                                quantOfRightPlaces++;
                                      }
                                }
                            }


                            //отсылаем клиенту колво правильных цифр
                            handler.Send(Encoding.Unicode.GetBytes(quantOfRightNums.ToString()));              
                            //отсылаем клиенту колво цифр на своих местах
                            handler.Send(Encoding.Unicode.GetBytes(quantOfRightPlaces.ToString()));            
                            //ожидание ответа, продолжать ли игру (не хочет ли клиент узнать число)
                            bytes = handler.Receive(data);                                                     
                            des = Encoding.Unicode.GetString(data, 0, bytes);

                             if (des == "Surrender")
                             {
                                    handler.Send(Encoding.Unicode.GetBytes(numberString));
                                    //после показаний числа, ожидание продолжать ли игру
                                    bytes = handler.Receive(data);                                                 
                                    des = Encoding.Unicode.GetString(data, 0, bytes);
                                    break;
                             }


                             if (des == "Continue")
                             {
                                    cont = true;
                                des = "";
                                    //continue;
                                    break;
                             }
                             if (des == "Stop")
                             {
                                    cont = false;
                                des = "";
                                    break;
                             }
                             /*if (des == "New")
                             {
                                    cont = true;
                                    break;
                             }*/
                        } while (handler.Available > 0);

                    }
                    // закрываем сокет
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}