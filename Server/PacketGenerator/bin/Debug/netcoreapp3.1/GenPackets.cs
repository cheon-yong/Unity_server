
class PlayerInfoReq
{
    public long playerId
public string name


    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        
this.playerId = id = BitConverter.ToInt64(s.Slice(count, s.Length - count));
count += sizeof(long);


ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
count += nameLen;


    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketId.PlayerInfoReq);
        count += sizeof(ushort);
        
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
count += sizeof(long);


ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
count += sizeof(ushort);
count += nameLen;


        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }
}
