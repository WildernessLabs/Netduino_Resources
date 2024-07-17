using System;
using System.IO.Ports;
using Microsoft.SPOT;
using SecretLabs.NETMF.Diagnostics;

namespace NetduinoMiniTransportSelector
{
    public class Program
    {
        public static void Main()
        {
            SerialPort serialPort;

            switch (Transport.GetInterface())
            {
                case TransportInterface.Com1:
                    serialPort = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
                    break;
                case TransportInterface.Com2:
                    serialPort = new SerialPort("COM2", 115200, Parity.None, 8, StopBits.One);
                    break;
                default:
                    // this should never happen; abort.
                    return;
            }

            // open serial port that is not being used for debugging
            serialPort.Open();

            byte[] readBuffer = new byte[1];
            int bytesRead = 0;

            while (true)
            {
                // discard all incoming characters
                serialPort.DiscardInBuffer();

                // wait for a byte of data to be received.
                bytesRead = serialPort.Read(readBuffer, 0, readBuffer.Length);

                // if the character is the ESCAPE character, then display our prompt
                if (bytesRead == 1 && readBuffer[0] == 27)
                {

                    // display the transport options
                    byte[] writeBuffer = System.Text.Encoding.UTF8.GetBytes(
                        "\r\n" +
                        "Netduino Mini\r\n" +
                        "\r\n" +
                        "1. TTL UART (COM1)\r\n" +
                        "2. RS232 UART (COM2)\r\n" +
                        "\r\n" +
                        "Which transport (1 or 2)? ");
                    serialPort.Write(writeBuffer, 0, writeBuffer.Length);

                    // discard all incoming characters
                    serialPort.DiscardInBuffer();

                    // read in the answer
                    bytesRead = serialPort.Read(readBuffer, 0, readBuffer.Length);

                    writeBuffer = new byte[] { readBuffer[0], (byte)'\r', (byte)'\n'};
                    serialPort.Write(writeBuffer, 0, writeBuffer.Length);

                    // if we received a valid answer, change our transport now.
                    if (bytesRead == 1)
                    {
                        // NOTE: the transport interface enumerations are currently reversed due to the logical swapping of COM1 and COM2 on the Netduino Mini
                        switch (readBuffer[0])
                        {
                            case (byte)'1':
                                writeBuffer = System.Text.Encoding.UTF8.GetBytes("\r\nSwitching transport to TTL UART (COM1)...\r\n");
                                serialPort.Write(writeBuffer, 0, writeBuffer.Length);
                                serialPort.Flush();

                                Transport.SetInterface(TransportInterface.Com1);
                                break;
                            case (byte)'2':
                                writeBuffer = System.Text.Encoding.UTF8.GetBytes("\r\nSwitching transport to RS232 UART (COM2)...\r\n");
                                serialPort.Write(writeBuffer, 0, writeBuffer.Length);
                                serialPort.Flush();

                                Transport.SetInterface(TransportInterface.Com2);
                                break;
                            default:
                                // invalid data; abort
                                writeBuffer = System.Text.Encoding.UTF8.GetBytes("\r\nInvalid response\r\n");
                                serialPort.Write(writeBuffer, 0, writeBuffer.Length);
                                serialPort.Flush();

                                break;
                        }
                    }
                }
            }



        }

    }
}
