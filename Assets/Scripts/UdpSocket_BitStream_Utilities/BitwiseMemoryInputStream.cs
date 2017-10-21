using System;
using System.Text;


/**
 * This class is a bit-wise memory output stream implementation
 */
public class BitwiseMemoryInputStream {

    readonly private int _capacity;
    private int _head;
    readonly private byte[] _buffer;

    public BitwiseMemoryInputStream(byte[] buffer) {

        _buffer = buffer;
        _capacity = buffer.Length;
        _head = 0;
    }

    /**
     * reads an int value from buffer by 32 bits
     */
    public int readInt() {
        return readInt(32);
    }

    /**
     * reads an int value from buffer by specified bits count
     */
    public int readInt(int bitCount) {

        int byteCount = bitCount / 8;
        if (bitCount % 8 > 0) {
            byteCount++;
        }
        byte[] resBytes = new byte[byteCount];

        int bitsToRead = (bitCount % 8 == 0) ? 8 : bitCount % 8;
        resBytes[0] = readBits(bitsToRead);

        int i = 1;
        //write all the bytes
        while (i < byteCount) {
            resBytes[i] = readBits(8);
            ++i;
        }
        return bytesToInt(resBytes);
    }

    /**
     * reads a float value from buffer by 32 bits
     */
    public float readFloat() {
        return readFloat(32);
    }

    /**
     * reads a float value from buffer by specified bits count
     */
    private float readFloat(int bitCount) {

        int byteCount = bitCount / 8;
        if (bitCount % 8 > 0) {
            byteCount++;
        }
        byte[] resBytes = new byte[byteCount];

        int bitsToRead = (bitCount % 8 == 0) ? 8 : bitCount % 8;
        resBytes[0] = readBits(bitsToRead);

        int i = 1;
        //write all the bytes
        while (i < byteCount) {
            resBytes[i] = readBits(8);
            ++i;
        }
        int intRepresentation = BitConverter.ToInt32(resBytes, 0);
        return BitConverter.ToSingle(intToBytes(intRepresentation, resBytes.Length), 0);
    }

    /**
     * reads a long value from buffer by 64 bits
     */
    public long readLong() {
        return readLong(64);
    }

    /**
    * reads a long value from buffer by specified bits count
    */
    public long readLong(int bitCount) {

        int byteCount = bitCount / 8;
        if (bitCount % 8 > 0) {
            byteCount++;
        }
        byte[] resBytes = new byte[byteCount];

        int bitsToRead = (bitCount % 8 == 0) ? 8 : bitCount % 8;
        resBytes[0] = readBits(bitsToRead);

        int i = 1;
        //write all the bytes
        while (i < byteCount) {
            resBytes[i] = readBits(8);
            ++i;
        }
        return bytesToLong(resBytes);
    }

    /**
     * reads a double value from buffer by 64 bits
     */
    public double readDouble() {
        return readDouble(64);
    }

    /**
    * reads a double value from buffer by specified bits count
    */
    private double readDouble(int bitCount) {

        int byteCount = bitCount / 8;
        if (bitCount % 8 > 0) {
            byteCount++;
        }
        byte[] resBytes = new byte[byteCount];

        int bitsToRead = (bitCount % 8 == 0) ? 8 : bitCount % 8;
        resBytes[0] = readBits(bitsToRead);

        int i = 1;
        //write all the bytes
        while (i < byteCount) {
            resBytes[i] = readBits(8);
            ++i;
        }
        long longRepresentation = BitConverter.ToInt64(resBytes, 0);
        return BitConverter.ToDouble(longToBytes(longRepresentation, resBytes.Length), 0);
    }

    /**
     * reads a signed int value from buffer by 32 bits
     */
    public int readSignedInt() {
        return readSignedInt(32);
    }

    /**
    * reads a signed int value from buffer by specified bits count
    * it first reads number's sign as a bool and then reads number's absolute value as a long
    */
    public int readSignedInt(int bitCount) {
        bool isNegative = readBool();
        int res = readInt(bitCount);
        if (isNegative) {
            res *= -1;
        }
        return res;
    }

    /**
     * reads a signed long value from buffer by 64 bits
     */
    public long readSignedLong() {
        return readSignedLong(64);
    }

    /**
    * reads a signed long value from buffer by specified bits count
    * it first reads number's sign as a bool and then reads number's absolute value as a long
    */
    public long readSignedLong(int bitCount) {
        bool isNegative = readBool();
        long res = readLong(bitCount);
        if (isNegative) {
            res *= -1;
        }
        return res;
    }

    /**
     * reads a bool value from buffer by 1 bit
     */
    public bool readBool() {
        return (readBits(1) == 1);
    }

    /**
     * reads a byte value from buffer by 8 bits
     */
    public byte readByte() {
        return readBits(8);
    }

    /**
     * reads a string value from buffer based on string's length
     * it first reads string's length as an int value, then reads string's bytes from buffer
     */
    public string readString() {
        int bytesCount = readInt(32);
        byte[] resArr = new byte[bytesCount];
        for (int i = 0; i < bytesCount; i++) {
            resArr[i] = readBits(8);
        }
        return Encoding.UTF8.GetString(resArr);
    }

    private byte readBits(int bitCount) {

        //// example for _head=9, bitCount=3, currentByte{01110101}:
        // _head{00001001} >> 3 = {00000001} = 1
        int byteOffset = _head >> 3; // or _head/8

        //// example
        // _head{00001001} & {00000111} = {00000001} = 1
        int bitOffsetInCurrentByte = _head & 0x7;

        //// example
        // currentByte{01110101} << 1 = resultByte{11101010}
        byte resultByte = (byte)(_buffer[byteOffset] << bitOffsetInCurrentByte);

        //// example
        //  8 - 1 = 7
        int numberOfBitsAreReadableInCurrentByte = 8 - bitOffsetInCurrentByte;

        //// example
        // if condition fails
        if (numberOfBitsAreReadableInCurrentByte < bitCount) {
            //need to read from next byte
            int readablePortionOfNextByte = (_buffer[byteOffset + 1] >> numberOfBitsAreReadableInCurrentByte);
            resultByte = (byte)(resultByte | readablePortionOfNextByte);
        }

        //// example
        // resultByte{11101010} >> 5 = {000111}
        resultByte = (byte)(resultByte >> (8 - bitCount));
        //// example
        // _head = 12
        _head += bitCount;
        //// example
        // return {000111}
        return resultByte;
    }

    private byte[] intToBytes(int num, int byteCount) {

        byte[] result = new byte[byteCount];
        for (int i = 0; byteCount > 0; i++, byteCount--) {
            result[i] = (byte)(num >> ((byteCount - 1) * 8));
        }
        return result;
    }

    private int bytesToInt(byte[] num) {

        int result = 0;
        for (int i = 0; i < num.Length; i++) {
            result |= (num[i] << ((num.Length - i - 1) * 8));
        }
        return result;
    }

    private long bytesToLong(byte[] num) {

        long result = 0;
        for (int i = 0; i < num.Length; i++) {
            result |= (((long)num[i]) << ((num.Length - i - 1) * 8));
        }
        return result;
    }

    private byte[] longToBytes(long num, int byteCount) {

        byte[] result = new byte[byteCount];

        for (int i = 0; byteCount > 0; i++, byteCount--) {
            result[i] = (byte)(num >> ((byteCount - 1) * 8));
        }
        return result;
    }

    /**
     * returns buffer as a byte array
     */
    public byte[] getBuffer() {
        return _buffer;
    }

    /**
     * returns buffer length in bits
     */
    public int getRemainingBytes() {
        return _capacity - (_head/8) + 1;
    }    
}
