﻿using System;
using System.IO.Ports;

namespace SerialPortSticky.GodSerialPort
{
    public partial class GodSerialPort
    {
        #region DataReceived event
        /// <summary>
        /// Handles the DataReceived event of the SerialPort.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialDataReceivedEventArgs"/> instance containing the event data.</param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] bytes = TryRead();
                OnData?.Invoke(this, bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        #region ErrorReceived event
        /// <summary>
        /// Handles the ErrorReceived event of the SerialPort.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialErrorReceivedEventArgs"/> instance containing the event data.</param>
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            try
            {
                onError?.Invoke(this, e.EventType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        #region PinChanged event
        /// <summary>
        /// Handles the PinChanged event of the SerialPort.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialPinChangedEventArgs"/> instance containing the event data.</param>
        private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            try
            {
                onPinChange?.Invoke(this, e.EventType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}