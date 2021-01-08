# SerialPortSticky
将串口收到的消息进行合并，并根据预定的协议进行分包

## 消息协议
消息体为：
  > [0xAA 0xBB] [0x08] [01 02 03 04 05 06 07 08] [FF]
  
消息体解析：
  > [包头] [数据长度] [数据] [校验位]

## 主要代码
```csharp
 #region 解析串口接收到的消息

        /// <summary>
        /// 不完整的数据包，即用户自定义缓冲区
        /// </summary>
        private byte[] _receivedBuffer;
        /// <summary>
        /// 消息包头
        /// </summary>
        private readonly byte[] _header = { 0xAA, 0xBB };
        /// <summary>
        /// 校验位
        /// </summary>
        private readonly byte _footer = 0xFF;//未校验

        /// <summary>
        /// 解析接收到的消息，粘包或者分包成预定的消息体
        /// 消息体为：[0xAA 0xBB] [0x08] [01 02 03 04 05 06 07 08] [FF]
        /// 消息体解析：[包头] [数据长度] [数据] [校验位]
        /// </summary>
        /// <param name="bt">单次收到的消息</param>
        private void ParseReceiveData(byte[] bt)
        {
            //readBytesLength为系统缓冲区长度
            var readBytesLength = bt.Length;

            if (readBytesLength <= 0) return;

            //判断缓冲区里面是否有数据 添加到当前数据
            if (_receivedBuffer != null && _receivedBuffer.Length > 0)
            {
                //var container = new byte[readBytesLength];
                //Array.Copy(bt, 0, container, 0, readBytesLength);
                _receivedBuffer = _receivedBuffer.Concat(bt).ToArray(); //拼接上一次剩余的包,已经完成读取每个数据包长度
            }
            else
            {
                _receivedBuffer = new byte[readBytesLength];
                Array.Copy(bt, 0, _receivedBuffer, 0, readBytesLength);
            }

            var p = 0;//当前位置
            //这里totalLen的长度有可能大于缓冲区大小的(因为 这里的surplusBuffer 是系统缓冲区+不完整的数据包)
            var totalLen = _receivedBuffer.Length;
            while (p <= totalLen)
            {
                //p = CheckHead(p);
                var pos = GetHeadPosition(p);

                //找不到消息头，将消息缓存起来
                if (pos < 0)
                {
                    var size = totalLen - p;
                    var temp = new byte[size];
                    Array.Copy(_receivedBuffer, p, temp, 0, size);
                    _receivedBuffer = temp;
                    return;
                }

                p = pos;

                //只有消息头，将消息缓存起来
                if (p == totalLen - _header.Length)
                {
                    var size = totalLen - p;
                    var temp = new byte[size];
                    Array.Copy(_receivedBuffer, p, temp, 0, size);
                    _receivedBuffer = temp;
                    return;
                }

                //获取数据长度
                var dataLen = _receivedBuffer[p + _header.Length];//

                if (dataLen < 4) continue;//小于4位数据格式不合法

                //判断剩余长度是否满足一个消息体
                if (p + _header.Length + 1 + dataLen + 1 > totalLen)
                {
                    var size = totalLen - p;
                    var temp = new byte[size];
                    Array.Copy(_receivedBuffer, p, temp, 0, size);
                    _receivedBuffer = temp;
                    return;
                }

                //校验位位置
                var sumPosition = p + _header.Length + 1 + dataLen;

                var b = _receivedBuffer[sumPosition] == _footer;//校验位检查

                if (b)//校验成功
                {
                    var dataLength = _header.Length + 1 + dataLen + 1;//消息体的长度
                    var recBytes = new byte[dataLength];
                    Array.Copy(_receivedBuffer, p, recBytes, 0, dataLength);
                    Logger.Info("【Parse】:: " + recBytes.ToHexString());
                    p += dataLength; //下标移动到 数据结束位 继续校验
                }
                else//校验失败
                {
                    p += _header.Length; //下标移动到 下一个头位置 校验
                }
            }
        }

        /// <summary>
        /// 获取第一个包头的位置
        /// </summary>
        /// <param name="pos">上一次的起始位置</param>
        /// <returns>包头的位置</returns>
        private int GetHeadPosition(int pos)
        {
            var p = pos;
            var hLen = _header.Length;
            var size = _receivedBuffer.Length;

            var rst = -1;
            for (var i = 0; i < hLen; i++)
            {
                for (var j = p; j < size; j++)
                {
                    //找到第一个位置
                    if (_header[i] == _receivedBuffer[j])
                    {
                        if (i == hLen - 1)
                        {
                            rst = j - hLen + 1;
                            break;
                        }
                        p = ++j;//赋值当前位置 第二次循环从下个一个位置开始
                        break;//退出循环找第二个位置
                    }
                }
            }

            return rst;
        }

        #endregion
```
