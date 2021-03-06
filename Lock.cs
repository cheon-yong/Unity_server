using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
    // 재귀적 락을 허용할 지 (Yes)
    // 스핀락 정책 (5000번 -> Yield)
    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7fff0000;
        const int READ_MASK = 0x0000ffff;
        const int MAX_SPINT_COUNT = 5000;

        // [unused(1)] [WriteThreadId(15)] [ReadCount(16)]
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {
            // 동일 스레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (lockThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                _writeCount++;
                return;
            }

            // 아무도 WriteLock Or ReadLock을 획득하고 있지 않을 때
            // 경합해서 소유권을 획득
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for (int i = 0; i < MAX_SPINT_COUNT; i++)
                {
                    // 시도를 해서 성공하면 리턴
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --_writeCount;
            if (lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (lockThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            while (true)
            {
                for (int i = 0; i < MAX_SPINT_COUNT; i++)
                {
                    // 시도를 해서 성공하면 리턴
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                            return;
                }
            }
        }

        public void ReadUnlock()
        {
             Interlocked.Decrement(ref _flag);
        }
    }
}
