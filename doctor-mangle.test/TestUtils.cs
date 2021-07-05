using Moq;
using System;

namespace doctor_mangle.test
{
    public static class TestUtils
    {
        public static Mock<Random> GetMockRandom_Next(int expected)
        {
            var mRandom = new Mock<Random>();
            mRandom.Setup(x => x.Next(
                It.IsAny<int>(),
                It.IsAny<int>()))
                .Returns(expected);
            mRandom.Setup(x => x.Next(
                It.IsAny<int>()))
                .Returns(expected);
            return mRandom;
        }

        public static Mock<Random> GetMockRandom_NextDouble(double expected)
        {
            var mRandom = new Mock<Random>();
            mRandom.Setup(x => x.NextDouble())
                .Returns(expected);
            return mRandom;
        }
    }
}
