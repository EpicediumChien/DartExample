using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.UserControls;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class KVMPIPPBPFullViewTests
    {
        private KvmViewModel? kvmViewModel;
        private KVMPIPPBPFullView? kVMPIPPBPFullView;
        private PrivateObject? privateObject;

        //[SetUp]
        //public void Setup()
        //{
        //    kvmViewModel = new KvmViewModel();
        //    kVMPIPPBPFullView = new KVMPIPPBPFullView(kvmViewModel);
        //}

        //[Test]
        //public void TestConstructor_KVMPIPPBPFullView()
        //{
        //    var pipOffSplitOwner = eSplitOwner.PxpOff;
        //    var pipSmallSplitOwner = eSplitOwner.PipList;
        //    var pipLargeSplitOwner = eSplitOwner.PipList;
        //    var splitListView_PbpSplitOwner = eSplitOwner.PbpList;
        //    Assert.That(kVMPIPPBPFullView, Is.Not.Null);
        //    Assert.That(kVMPIPPBPFullView.DataContext, Is.EqualTo(kvmViewModel));
        //    Assert.That(pipOffSplitOwner, Is.EqualTo(eSplitOwner.PxpOff));
        //    Assert.That(pipSmallSplitOwner, Is.EqualTo(eSplitOwner.PipList));
        //    Assert.That(pipLargeSplitOwner, Is.EqualTo(eSplitOwner.PipList));
        //    Assert.That(splitListView_PbpSplitOwner, Is.EqualTo(eSplitOwner.PbpList));
        //}
    }
}
