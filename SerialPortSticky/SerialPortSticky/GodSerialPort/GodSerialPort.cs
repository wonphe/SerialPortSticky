﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using SerialPortSticky.GodSerialPort.Enums;
using SerialPortSticky.GodSerialPort.Extensions;

namespace SerialPortSticky.GodSerialPort
{
    /// <summary>
    /// GodSerialPort Util Class.
    /// </summary>
    /// <example>
    /// GodSerialPort serial= new GodSerialPort("COM1",9600);
    /// serial.UseDataReceived((sp,bytes)=>{});
    /// serial.Open();
    /// </example>
    public partial class GodSerialPort
    {
        #region Propertys
        
        private int tryReadNumber;

        private bool initialized = false;

        private bool onDataBind = false;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        public SerialConfigOptions Options { get; private set; }

        /// <summary>
        /// The method of execution that data has been received through a port represented by the SerialPort object.
        /// </summary>
        public Action<GodSerialPort, byte[]> OnData { get; set; }

        /// <summary>
        /// The method of execution that an error has occurred with a port represented by a SerialPort object.
        /// </summary>
        private Action<GodSerialPort,SerialError> onError;

        /// <summary>
        /// The method of execution that a non-data signal event has occurred on the port represented by the SerialPort object.
        /// </summary>
        private Action<GodSerialPort,SerialPinChange> onPinChange;

        /// <summary>
        /// Gets or sets the data format.
        /// </summary>
        /// <value>The data format.</value>
        // ReSharper disable once MemberCanBePrivate.Global
        public SerialPortDataFormat DataFormat { get; set; } = SerialPortDataFormat.Hex;

        /// <summary>
        /// Gets or sets the try count of receive.
        /// </summary>
        /// <value>The try count of receive,default is 3.</value>
        public int TryReadNumber
        {
            get => this.tryReadNumber;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(TryReadNumber), nameof(TryReadNumber) + " must be equal or greater than 1.");
                tryReadNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the try sleep time of receive,unit is ms.
        /// </summary>
        /// <value>The try sleep time of receive,default is 10.</value>
        // ReSharper disable once MemberCanBePrivate.Global
        public int TryReadSpanTime { get; set; } = 10;

        /// <summary>
        /// Gets or sets the terminator,hex value.
        /// </summary>
        /// <value>
        /// The terminator.
        /// </value>
        /// <example>
        /// Terminator = "0D 0A"
        /// </example>
        public string Terminator { get; set; } = null;

        /// <summary>
        /// The serial port
        /// </summary>
        private System.IO.Ports.SerialPort serialPort;

        /// <summary>
        /// SerialPort object.
        /// </summary>
        public System.IO.Ports.SerialPort SerialPort => serialPort;

        /// <summary>
        /// Determines whether this instance is open.
        /// </summary>
        /// <returns><c>true</c> if this serialport is open; otherwise, <c>false</c>.</returns>
        public bool IsOpen => serialPort != null && serialPort.IsOpen;

        /// <summary>
        /// Gets or sets the name of the port.
        /// </summary>
        /// <value>The name of the port.</value>
        public string PortName
        {
            get => serialPort.PortName;
            set => serialPort.PortName = value;
        }

        /// <summary>
        /// Gets or sets the baudrate.
        /// </summary>
        /// <value>The baudrate.</value>
        public int BaudRate
        {
            get => serialPort.BaudRate;
            set => serialPort.BaudRate = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [break state].
        /// </summary>
        /// <value><c>true</c> if [break state]; otherwise, <c>false</c>.</value>
        public bool BreakState
        {
            get => serialPort.BreakState;
            set => serialPort.BreakState = value;
        }
        /// <summary>
        /// Gets or sets the databits.
        /// </summary>
        /// <value>The databits.</value>
        public int DataBits
        {
            get => serialPort.DataBits;
            set => serialPort.DataBits = value;
        }

        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        /// <value>The encoding.</value>
        public Encoding Encoding
        {
            get => serialPort.Encoding;
            set => serialPort.Encoding = value;
        }
        
        /// <summary>
        /// Gets or sets the handshake.
        /// </summary>
        /// <value>The handshake.</value>
        public Handshake Handshake
        {
            get => serialPort.Handshake;
            set => serialPort.Handshake = value;
        }

        /// <summary>
        /// Gets or sets the parity.
        /// </summary>
        /// <value>The parity.</value>
        public Parity Parity
        {
            get => serialPort.Parity;
            set => serialPort.Parity = value;
        }

        /// <summary>
        /// Gets or sets the stopbits.
        /// </summary>
        /// <value>The stopbits.</value>
        public StopBits StopBits
        {
            get => serialPort.StopBits;
            set => serialPort.StopBits = value;
        }

        /// <summary>
        /// Gets or sets the read timeout.
        /// </summary>
        /// <value>The read timeout.</value>
        public int ReadTimeout
        {
            get => serialPort.ReadTimeout;
            set => serialPort.ReadTimeout = value;
        }

        /// <summary>
        /// Gets or sets the write timeout.
        /// </summary>
        /// <value>The write timeout.</value>
        public int WriteTimeout
        {
            get => serialPort.WriteTimeout;
            set => serialPort.WriteTimeout = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [DTR enable].
        /// </summary>
        /// <value><c>true</c> if [DTR enable]; otherwise, <c>false</c>.</value>
        public bool DtrEnable
        {
            get => serialPort.DtrEnable;
            set => serialPort.DtrEnable = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [RTS enable].
        /// </summary>
        /// <value><c>true</c> if [RTS enable]; otherwise, <c>false</c>.</value>
        public bool RtsEnable
        {
            get => serialPort.RtsEnable;
            set => serialPort.RtsEnable = value;
        }

        /// <summary>
        /// Gets a value indicating whether [CTS holding].
        /// </summary>
        /// <value><c>true</c> if [CTS holding]; otherwise, <c>false</c>.</value>
        public bool CtsHolding => serialPort.CtsHolding;

        /// <summary>
        /// Gets a value indicating whether [DSR holding].
        /// </summary>
        /// <value><c>true</c> if [DSR holding]; otherwise, <c>false</c>.</value>
        public bool DsrHolding => serialPort.DsrHolding;

        /// <summary>
        /// Gets a value indicating whether [cd holding].
        /// </summary>
        /// <value><c>true</c> if [cd holding]; otherwise, <c>false</c>.</value>
        public bool CdHolding => serialPort.CDHolding;

        /// <summary>
        /// Gets or sets a value indicating whether [discard null].
        /// </summary>
        /// <value><c>true</c> if [discard null]; otherwise, <c>false</c>.</value>
        public bool DiscardNull
        {
            get => serialPort.DiscardNull;
            set => serialPort.DiscardNull = value;
        }

        /// <summary>
        /// Gets or sets the size of the read buffer.
        /// </summary>
        /// <value>The size of the read buffer.</value>
        public int ReadBufferSize
        {
            get => serialPort.ReadBufferSize;
            set => serialPort.ReadBufferSize = value;
        }

        /// <summary>
        /// Gets or sets the parity replace.
        /// </summary>
        /// <value>The parity replace.</value>
        public byte ParityReplace
        {
            get => serialPort.ParityReplace;
            set => serialPort.ParityReplace = value;
        }

        /// <summary>
        /// Gets or sets the received bytes threshold.
        /// </summary>
        /// <value>The received bytes threshold.</value>
        public int ReceivedBytesThreshold
        {
            get => serialPort.ReceivedBytesThreshold;
            set => serialPort.ReceivedBytesThreshold = value;
        }

        /// <summary>
        /// Gets or sets the size of the write buffer.
        /// </summary>
        /// <value>The size of the write buffer.</value>
        public int WriteBufferSize
        {
            get => serialPort.WriteBufferSize;
            set => serialPort.WriteBufferSize = value;
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GodSerialPort"/> class.
        /// </summary>
        private GodSerialPort()
        {
            this.tryReadNumber = 3;
            this.serialPort = new System.IO.Ports.SerialPort();

            Options = new SerialConfigOptions()
            {
                Handshake = serialPort.Handshake,
                Parity = serialPort.Parity,
                StopBits = serialPort.StopBits
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GodSerialPort"/> class.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        /// <param name="baudRate">The baudrate,default is 9600.</param>
        /// <param name="parity">The string parity,default is none,Parity.None.
        /// <para>Parity.None：0|n|none</para>
        /// <para>Parity.Odd：1|o|odd</para>
        /// <para>Parity.Even：2|e|even</para>
        /// <para>Parity.Mark：3|m|mark</para>
        /// <para>Parity.Space：4|s|space</para>
        /// </param>
        /// <param name="dataBits">The databits,default is 8.</param>
        /// <param name="stopBits">The string stopbits,default is one,StopBits.One.
        /// <para>StopBits.None：0|n|none</para>
        /// <para>StopBits.One：1|o|one</para>
        /// <para>StopBits.Two：2|t|two</para>
        /// <para>StopBits.OnePointFive：3|1.5|f|of|opf</para>
        /// </param>
        /// <param name="handshake">The string handshake,default is none,Handshake.None.
        /// <para>Handshake.None：0|n|none</para>
        /// <para>Handshake.XOnXOff：1|x|xoxo</para>
        /// <para>Handshake.RequestToSend：2|r|rst</para>
        /// <para>Handshake.RequestToSendXOnXOff：3|rx|rtsxx</para>
        /// </param>
        public GodSerialPort(string portName, int baudRate = 9600, string parity = "none", int dataBits = 8, string stopBits = null, string handshake = null) : this(new SerialConfigOptions(portName, baudRate, dataBits, stopBits, parity, handshake))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GodSerialPort"/> class.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        /// <param name="baudRate">The baudrate,default is 9600.</param>
        /// <param name="parity">The int parity,default is 0,Parity.None.
        /// <para>Parity.None：0</para>
        /// <para>Parity.Odd：1</para>
        /// <para>Parity.Even：2</para>
        /// <para>Parity.Mark：3</para>
        /// <para>Parity.Space：4</para>
        /// </param>
        /// <param name="dataBits">The databits,default is 8.</param>
        /// <param name="stopBits">The int stopbits,default is 1,StopBits.One.
        /// <para>StopBits.None：0</para>
        /// <para>StopBits.One：1</para>
        /// <para>StopBits.Two：2</para>
        /// <para>StopBits.OnePointFive：3</para>
        /// </param>
        /// <param name="handshake">The int handshake,default is 0,Handshake.None.
        /// <para>Handshake.None：0</para>
        /// <para>Handshake.XOnXOff：1</para>
        /// <para>Handshake.RequestToSend：2</para>
        /// <para>Handshake.RequestToSendXOnXOff：3</para>
        /// </param>
        public GodSerialPort(string portName, int baudRate = 9600, int parity = 0, int dataBits = 8, int stopBits = 1, int handshake = 0) : this(new SerialConfigOptions(portName, baudRate, dataBits, stopBits, parity, handshake))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GodSerialPort"/> class.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        /// <param name="baudRate">The baudrate,default is 9600.</param>
        /// <param name="parity">The int parity,default is Parity.None.</param>
        /// <param name="dataBits">The databits,default is 8.</param>
        /// <param name="stopBits">The int stopbits,default is StopBits.One.</param>
        /// <param name="handshake">The int handshake,default is Handshake.None.</param>
        public GodSerialPort(string portName, int baudRate = 9600, Parity parity =  Parity.None, int dataBits = 8, StopBits stopBits = StopBits.None, Handshake handshake = Handshake.None) : this(new SerialConfigOptions(portName,baudRate,dataBits,stopBits,parity,handshake))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GodSerialPort"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public GodSerialPort(SerialConfigOptions options) : this()
        {
            this.Initialize(options);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GodSerialPort"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public GodSerialPort(Action<SerialConfigOptions> action) : this()
        {
            action?.Invoke(Options);
            this.Initialize(Options);
        }
        #endregion

        #region Initializes the SerialPort method
        /// <summary>
        /// Initializes the <see cref="SerialPort"/> with the action of data receive.
        /// </summary>
        private void Initialize(SerialConfigOptions options)
        {
            try
            {
                if (initialized) throw new InvalidOperationException("Already initialized");

                Options = options ?? throw new ArgumentNullException(nameof(options));

                serialPort.PortName = Options.PortName;
                serialPort.BaudRate = Options.BaudRate;
                serialPort.DataBits = Options.DataBits;
                serialPort.Handshake = Options.Handshake;
                serialPort.Parity = Options.Parity;
                serialPort.StopBits = Options.StopBits;
                serialPort.PinChanged += SerialPort_PinChanged;
                serialPort.ErrorReceived += SerialPort_ErrorReceived;

                initialized = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("Init SerialPort Exception:" + PortName + "\r\nMessage:" + ex.Message); 
#endif
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion

        /// <summary>
        /// Use DataReceived event with data received action or not.
        /// </summary>
        /// <param name="flag">The action which process data.</param>
        public void UseDataReceived(bool flag)
        {
            if (flag && !onDataBind)
            {
                onDataBind = true;
                serialPort.DataReceived += SerialPort_DataReceived;

                return;
            }

            if (!flag && onDataBind)
            {
                onDataBind = false;
                serialPort.DataReceived -= SerialPort_DataReceived;
            }
        }

        /// <summary>
        /// Use DataReceived event with data received action or not.
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="action"></param>
        public void UseDataReceived(bool flag, Action<GodSerialPort, byte[]> action = null)
        {
            UseDataReceived(flag);
            OnData = action;
        }

        /// <summary>
        /// Use DataReceived event with data received action.
        /// </summary>
        /// <param name="action"></param>
        [Obsolete("This method is obsolete,will be removed next release version.")]
        public void UseDataReceived(Action<GodSerialPort, byte[]> action)=> UseDataReceived(true, action);

        #region Open SerialPort method        
        /// <summary>
        /// Opens this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Not initialized</exception>
        public bool Open()
        {
            if (!initialized) throw new InvalidOperationException("Not initialized");

            bool rst = false;
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                }
                else
                {
#if DEBUG
                    Console.WriteLine("the port is opened!"); 
#endif
                    return true;
                }
            }
#if !DEBUG
            catch (Exception)
            {
#elif DEBUG
            catch (Exception ex)
            {
                Console.WriteLine("Open SerialPort Exception:" + PortName + "\r\nMessage:" + ex.Message); 
#endif
            }

            if (serialPort.IsOpen)
            {
#if DEBUG
                Console.WriteLine("successed to open the port!"); 
#endif
                rst = true;
            }
            return rst;
        }

#endregion

        #region Set the method when error
        /// <summary>
        /// Set the method when [error].
        /// </summary>
        /// <param name="action">The action.</param>
        public void OnError(Action<GodSerialPort, SerialError> action)
        {
            this.onError = action;
        }
        #endregion

        #region Set the method when pin changed
        /// <summary>
        /// Set the method when [pin changed].
        /// </summary>
        /// <param name="action">The action.</param>
        public void OnPinChange(Action<GodSerialPort, SerialPinChange> action)
        {
            this.onPinChange = action;
        }
        #endregion

        #region Close SerialPort method
        /// <summary>
        /// Close the <see cref="System.IO.Ports.SerialPort"/>.
        /// </summary>
        /// <returns><c>true</c> if closed, <c>false</c> otherwise.</returns>
        public bool Close()
        {
            try
            {
                if (serialPort.IsOpen) serialPort.Close();

                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("Close SerialPort Exception:" + PortName + "\r\nMessage:" + ex.Message + "\r\nStackTrace:" + ex.StackTrace); 
#endif
                throw new Exception(ex.Message, ex);
            }
        }

#endregion

        #region Reads method
        
        /// <summary>
        /// Synchronously reads one byte from the SerialPort input buffer.
        /// </summary>
        /// <returns>The byte, cast to an Int32, or -1 if the end of the stream has been read.</returns>
        public int ReadByte() => serialPort.ReadByte();
        
        /// <summary>
        /// Synchronously reads one character from the SerialPort input buffer.
        /// </summary>
        /// <returns>The character that was read.</returns>
        public int ReadChar() => serialPort.ReadChar();
        
        /// <summary>
        /// Reads up to the NewLine value in the input buffer.
        /// </summary>
        /// <returns>The contents of the input buffer up to the first occurrence of a NewLine value.</returns>
        public string ReadLine() => serialPort.ReadLine();

        /// <summary>
        /// Reads all immediately available bytes, based on the encoding, in both the stream and the input buffer of the SerialPort object.
        /// </summary>
        /// <returns>The contents of the stream and the input buffer of the SerialPort object.</returns>
        public string ReadExisting() => serialPort.ReadExisting();

        /// <summary>
        /// Reads a string up to the specified value in the input buffer.
        /// </summary>
        /// <param name="value">A value that indicates where the read operation stops.</param>
        /// <returns>The contents of the input buffer up to the specified value.</returns>
        public string ReadTo(string value) => serialPort.ReadTo(value);
        
        /// <summary>
        /// Reads data from the input buffer.
        /// </summary>
        /// <returns>System.String,hex or ascii format.</returns>
        public string ReadString()
        {
            try
            {
                string str = null;

                byte[] bytes = this.TryRead();

                if (bytes != null && bytes.Length > 0)
                {
                    switch (DataFormat)
                    {
                        case SerialPortDataFormat.Char:
                            str = serialPort.Encoding.GetString(bytes);
                            break;
                        case SerialPortDataFormat.Hex:
                            str = bytes.ToHexString();
                            break;
                    }
                }

                return str;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Reads data from the input buffer.
        /// </summary>
        /// <returns>The byte array.</returns>
        public byte[] Read()
        {
            try
            {
                return this.TryRead();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion

        #region //Try Read Data
        /// <summary>
        /// Try Read Data
        /// </summary>
        /// <returns>The byte array.</returns>
        private byte[] TryRead()
        {
            int tryCount = 0;
            int endingLength = 0;
            bool found = false;
            List<byte> list = new List<byte>();
            byte[] bytes;
            byte[] ending = null;
            byte[] currentEnding;

            if (Terminator != null)
            {
                ending = Terminator.HexToByte();
                endingLength = ending.Length;
            }

            int dataLength;

            while ((serialPort.BytesToRead > 0 || !found) && tryCount < tryReadNumber)
            {
                dataLength = serialPort.BytesToRead < serialPort.ReadBufferSize
                    ? serialPort.BytesToRead
                    : serialPort.ReadBufferSize;
                bytes = new byte[dataLength];
                serialPort.Read(bytes, 0, bytes.Length);
                list.AddRange(bytes);

                if (ending != null && endingLength > 0)
                {
                    currentEnding = new byte[endingLength];

                    if (bytes.Length >= endingLength)
                    {
                        Buffer.BlockCopy(bytes, bytes.Length - endingLength, currentEnding, 0, endingLength);
                    }
                    else if (list.ToArray().Length >= endingLength)
                    {
                        byte[] temp = list.ToArray();
                        Buffer.BlockCopy(temp, temp.Length - endingLength, currentEnding, 0, endingLength);
                    }
                    else
                    {
                        continue;
                    }

                    found = ending.Length > 0 && currentEnding.SequenceEqual(ending);
                }

                if (TryReadSpanTime > 0) Thread.Sleep(TryReadSpanTime);

                tryCount++;
            }

            return list.Count > 0 ? list.ToArray() : null;
        } 
        #endregion

        #region Writes method
        /// <summary>
        /// Writes the specified hex string.
        /// </summary>
        /// <param name="str">The hex string with space.example:'30 31 32'.</param>
        /// <example>sp.WriteHexString("30 31 32");</example>
        /// <returns>The <see cref="Int32"/> byte number to be written.</returns>
        public int WriteHexString(string str)
        {
            try
            {
                byte[] bytes = str.HexToByte();

                return Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Writes the specified ascii string.
        /// </summary>
        /// <param name="str">The ascii string.</param>
        /// <example>sp.WriteHexString("123");</example>
        /// <returns>The <see cref="Int32"/> byte number to be written.</returns>
        public int WriteAsciiString(string str)
        {
            try
            {
                byte[] bytes = serialPort.Encoding.GetBytes(str);

                return Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Writes the byte array.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The <see cref="Int32"/> byte number to be written.</returns>
        public int Write(byte[] bytes)
        {
            return Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes the byte array with offset.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="offset">The number of offset.</param>
        /// <param name="count">The length of write.</param>
        /// <returns>The <see cref="Int32"/> byte number to be written.</returns>
        public int Write(byte[] bytes, int offset, int count)
        {
            try
            {
                serialPort.Write(bytes, offset, count);

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Writes the specified string and the NewLine value to the output buffer.
        /// </summary>
        /// <param name="str"></param>
        public void WriteLine(string str) => serialPort.WriteLine(str);

        #endregion

        #region SerialPort other method
        /// <summary>
        /// Discards the input buffer.
        /// </summary>
        public void DiscardInBuffer() => serialPort.DiscardInBuffer();

        /// <summary>
        /// Discards the output buffer.
        /// </summary>
        public void DiscardOutBuffer() => serialPort.DiscardOutBuffer();
        #endregion
    }
}