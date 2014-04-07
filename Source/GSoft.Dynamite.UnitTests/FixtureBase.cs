using System;
using NMock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests
{
	[TestClass]
	public class FixtureBase
	{
		private MockFactory _factory = new MockFactory();

		[TestCleanup]
		public void Cleanup()
		{
			_factory.VerifyAllExpectationsHaveBeenMet();
			_factory.ClearExpectations();
		}

        //[TestMethod]
        //public void PropertyTest()
        //{
        //    var mock = _factory.CreateMock<ITest>();
        //    mock.Expects.One.GetProperty(_ => _.Prop).WillReturn("Hello");
        //    mock.Expects.One.SetPropertyTo(_ => _.Prop = ", World");

        //    var controller = new Controller(mock.MockObject);
        //    Assert.AreEqual("Hello, World", controller.PropActions(", World"));
        //}

        //[TestMethod]
        //public void MethodTest()
        //{
        //    var mock = _factory.CreateMock<ITest>();
        //    mock.Expects.One.MethodWith(_ => _.Method(1, 2, 3, 4)).WillReturn(new Version(5, 6, 7, 8));

        //    var controller = new Controller(mock.MockObject);
        //    var version = controller.GetVersion(1, 2, 3, 4);

        //    mock.Expects.One.Method(_ => _.Method(null)).With(Is.TypeOf<Version>()).WillReturn("3, 4, 5, 6");

        //    var result = controller.GetVersion(version);
        //    Assert.AreEqual("3, 4, 5, 6", result);
        //}

        //[TestMethod]
        //public void EventTest()
        //{
        //    var mock = _factory.CreateMock<ITest>();
        //    var invoker = mock.Expects.One.EventBinding(_ => _.Event += null);

        //    var controller = new Controller(mock.MockObject);
        //    controller.InitEvents();

        //    Assert.IsNull(controller.Status);
        //    invoker.Invoke();
        //    Assert.AreEqual("Event Fired!", controller.Status);

        //}

	
	}
}