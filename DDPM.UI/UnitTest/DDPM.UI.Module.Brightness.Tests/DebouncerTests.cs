using NUnit.Framework;
using Moq;
using DDPM.UI.Module.Brightness;

namespace DebouncerTests
{
    public class DebouncerTests
    {
        [Test]
        public void TestDebouncerAfterDelay()
        {
            var delayMilliseconds = 100;
            var mockAction = new Mock<Action<double>>();
            var debouncer = new Debouncer(delayMilliseconds, mockAction.Object);

            debouncer.Debounce(1.0);

            Thread.Sleep(delayMilliseconds + 50);

            mockAction.Verify(action => action(1.0), Times.Once);
        }

        [Test]
        public void TestDebouncerMultiple()
        {
            var delayMilliseconds = 100;
            var mockAction = new Mock<Action<double>>();
            var debouncer = new Debouncer(delayMilliseconds, mockAction.Object);

            debouncer.Debounce(1.0);
            Thread.Sleep(delayMilliseconds / 2);//第一次

            debouncer.Debounce(2.0);
            Thread.Sleep(delayMilliseconds + 50);//第二次

            mockAction.Verify(action => action(It.IsAny<double>()), Times.Once);
            mockAction.Verify(action => action(2.0), Times.Once);
        }

        [Test]
        public void TestDebouncerDelay()
        {
            var delayMilliseconds = 100;
            var mockAction = new Mock<Action<double>>();
            var debouncer = new Debouncer(delayMilliseconds, mockAction.Object);

            debouncer.Debounce(1.0);//第一次
            Thread.Sleep(delayMilliseconds / 2);

            mockAction.Verify(action => action(It.IsAny<double>()), Times.Never);
        }
    }
}
