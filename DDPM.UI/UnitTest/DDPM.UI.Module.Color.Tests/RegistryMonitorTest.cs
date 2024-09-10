using NGA.UnitTest.PrivateObject;
using NUnit.Framework.Internal;
//using RegistryUtils;

namespace DDPM.UI.Module.Color.Tests
{
    [Apartment(ApartmentState.STA)]
    public class RegistryMonitorTest
    {
        private PrivateObject? privateObject;
        //private RegistryMonitor_NightLight? registryMonitor_NightLight;

        [SetUp]
        public void Setup()
        {
            //registryMonitor_NightLight = new RegistryMonitor_NightLight("HKEY_CLASSES_ROOT");
            //privateObject = new PrivateObject(registryMonitor_NightLight);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestRegistryMonitor_NightLight()
        {
            //Assert.That(registryMonitor_NightLight, Is.Not.Null);
        }

        [Test]
        public void TestDispose()
        {
            try
            {
                //registryMonitor_NightLight.Dispose();
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("_disposed"), Is.EqualTo(true));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestRegChangeNotifyFilter()
        {
            //var regChangeNotifyFilter = new RegChangeNotifyFilter();
            //registryMonitor_NightLight.RegChangeNotifyFilter = regChangeNotifyFilter;
            //Assert.That(registryMonitor_NightLight.RegChangeNotifyFilter, Is.EqualTo(regChangeNotifyFilter));

            privateObject.SetFieldOrProperty("_thread", new Thread(Test));
            try
            {
                //registryMonitor_NightLight.RegChangeNotifyFilter = regChangeNotifyFilter;
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("Monitoring thread is already running"));
            }
        }

        [Test]
        public void TestIsMonitoring()
        {
            //var result = registryMonitor_NightLight.IsMonitoring;
            Assert.That(privateObject.GetFieldOrProperty("_thread"), Is.EqualTo(null));
            //Assert.That(result, Is.EqualTo(false));

            privateObject.SetFieldOrProperty("_thread", new Thread(Test));
            //result = registryMonitor_NightLight.IsMonitoring;
            Assert.That(privateObject.GetFieldOrProperty("_thread"), Is.Not.Null);
            //Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestStart()
        {
            ////if (!IsMonitoring)
            //registryMonitor_NightLight.Start();
            //var threadstart=false;
            //var getthread = (Thread)privateObject.GetFieldOrProperty("_thread");
            //Assert.IsTrue(getthread.ThreadState == ThreadState.Running);

            //int i = 0;
            //bool isstart = false;
            //while (i < 10)
            //{
            //    var getthread = (Thread)privateObject.GetFieldOrProperty("_thread");
            //    i++;
            //    Thread.Sleep(1000);
            //    isstart = getthread.IsAlive;
            //    if (isstart)
            //    {
            //        break;
            //    }
            //    Assert.IsTrue(isstart);
            //}

            //if (_disposed)
            privateObject.SetFieldOrProperty("_disposed", true);
            try
            {
                //registryMonitor_NightLight.Start();
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("This instance is already disposed"));
            }
        }

        [Test]
        public void TestStop()
        {
            //_thread==null
            //registryMonitor_NightLight.Stop();
            var getthread = (Thread)privateObject.GetFieldOrProperty("_thread");
            Assert.That(getthread, Is.EqualTo(null));

            //thread!=null
            Thread thread = new Thread(Test);
            thread.Start();
            privateObject.SetFieldOrProperty("_thread", thread);
            //registryMonitor_NightLight.Stop();
            getthread = (Thread)privateObject.GetFieldOrProperty("_thread");
            Assert.IsTrue(getthread.ThreadState == ThreadState.Stopped);

            //if (_disposed)
            privateObject.SetFieldOrProperty("_disposed", true);
            try
            {
                //registryMonitor_NightLight.Stop();
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("This instance is already disposed"));
            }
        }

        public void Test()
        {
            Thread.Sleep(5000);
            int x = 112;
        }

        //[Test]
        //[Apartment(ApartmentState.STA)]
        //public void TestStart()
        //{
        //    var registryMonitor_NightLight = new RegistryMonitor_NightLight("HKEY_CLASSES_ROOT");
        //    privateObject = new PrivateObject(registryMonitor_NightLight);
        //    registryMonitor_NightLight.Start();
        //    int i = 0;
        //    bool isstart = false;

        //    while (i < 10)
        //    {
        //        var getthread = (Thread)privateObject.GetFieldOrProperty("_thread");
        //        i++;
        //        Thread.Sleep(1000);
        //        isstart = getthread.IsAlive ;
        //        if (isstart)
        //        {
        //            break;
        //        }
        //    }

        //    Assert.IsTrue(isstart);
        //}

        //[Test]
        //[Apartment(ApartmentState.STA)]
        //public void TestStop()
        //{
        //    var registryMonitor_NightLight = new RegistryMonitor_NightLight("HKEY_CLASSES_ROOT");
        //    privateObject = new PrivateObject(registryMonitor_NightLight);
        //    registryMonitor_NightLight.Start();
        //    registryMonitor_NightLight.Stop();
        //    int i = 0;
        //    bool isstart = true;

        //    while (i < 10)
        //    {
        //        var getthread = (Thread)privateObject.GetFieldOrProperty("thread");
        //        i++;
        //        Thread.Sleep(1000);
        //        isstart = getthread.IsAlive;
        //        if (!isstart)
        //        {
        //            break;
        //        }
        //    }

        //    Assert.IsFalse(isstart);
        //}

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestRegistryMonitor_ICC()
        {
            //var registryMonitor_ICC = new RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
            //Assert.That(registryMonitor_ICC, Is.Not.Null);
        }

        [Test]
        public void TestDispose_ICC()
        {
            //var registryMonitor_ICC = new RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
            //var privateObject_ICC = new PrivateObject(registryMonitor_ICC);
            try
            {
                //registryMonitor_ICC.Dispose();
                Assert.True(true);
                //var result = privateObject_ICC.GetFieldOrProperty("_disposed");
                //Assert.That(result, Is.EqualTo(true));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestRegChangeNotifyFilter_ICC()
        {
            //var registryMonitor_ICC = new RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
            //var regChangeNotifyFilter = new RegChangeNotifyFilter();
            //registryMonitor_ICC.RegChangeNotifyFilter = regChangeNotifyFilter;
            //Assert.That(registryMonitor_ICC.RegChangeNotifyFilter, Is.EqualTo(regChangeNotifyFilter));

            privateObject.SetFieldOrProperty("_thread", new Thread(Test));
            try
            {
                //registryMonitor_ICC.RegChangeNotifyFilter = regChangeNotifyFilter;
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("Monitoring thread is already running"));
            }
        }

        [Test]
        public void TestIsMonitoring_ICC()
        {
            //var registryMonitor_ICC = new RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
            //var privateObject_ICC = new PrivateObject(registryMonitor_ICC);
            //var result = registryMonitor_ICC.IsMonitoring;
            //Assert.That(privateObject_ICC.GetFieldOrProperty("_thread"), Is.EqualTo(null));
            //Assert.That(result, Is.EqualTo(false));

            //privateObject_ICC.SetFieldOrProperty("_thread", new Thread(Test));
            //result = registryMonitor_ICC.IsMonitoring;
            //Assert.That(privateObject_ICC.GetFieldOrProperty("_thread"), Is.Not.Null);
            //Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestStart_ICC()
        {
            //var registryMonitor_ICC = new RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
            //var privateObject_ICC = new PrivateObject(registryMonitor_ICC);
            ////if (!IsMonitoring)
            //registryMonitor_ICC.Start();
            //var threadstart=false;
            //var getthread = (Thread)privateObject.GetFieldOrProperty("_thread");
            //Assert.IsTrue(getthread.ThreadState == ThreadState.Running);

            //int i = 0;
            //bool isstart = false;
            //while (i < 10)
            //{
            //    var getthread = (Thread)privateObject.GetFieldOrProperty("_thread");
            //    i++;
            //    Thread.Sleep(1000);
            //    isstart = getthread.IsAlive;
            //    if (isstart)
            //    {
            //        break;
            //    }
            //    Assert.IsTrue(isstart);
            //}

            //if (_disposed)
            //privateObject_ICC.SetFieldOrProperty("_disposed", true);
            try
            {
                //registryMonitor_ICC.Start();
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("This instance is already disposed"));
            }
        }

        [Test]
        public void TestStop_ICC()
        {
            //var registryMonitor_ICC = new RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
            //var privateObject_ICC = new PrivateObject(registryMonitor_ICC);
            //_thread==null
            //registryMonitor_ICC.Stop();
            //var getthread = (Thread)privateObject_ICC.GetFieldOrProperty("_thread");
            //Assert.That(getthread, Is.EqualTo(null));

            //thread!=null
            Thread thread = new Thread(Test);
            thread.Start();
            //privateObject_ICC.SetFieldOrProperty("_thread", thread);
            //registryMonitor_ICC.Stop();
            //getthread = (Thread)privateObject_ICC.GetFieldOrProperty("_thread");
            //Assert.IsTrue(getthread.ThreadState == ThreadState.Stopped);

            //if (_disposed)
            //privateObject_ICC.SetFieldOrProperty("_disposed", true);
            try
            {
                //registryMonitor_ICC.Stop();
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("This instance is already disposed"));
            }
        }
    }
}