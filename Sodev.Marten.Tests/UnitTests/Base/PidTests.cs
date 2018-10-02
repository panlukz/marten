using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sodev.Marten.Base.Model;

namespace Sodev.Marten.Tests.UnitTests.Base
{
    [TestClass]
    public class PidTests
    {
        [TestMethod]
        public void GivenPidParameters_OnCreate_VerifyIfFormulaIsCorrect()
        {
            //Act 
            var pidParameters = new PidParameters
            {
                Id = 0x0A,
                Description = "Fuel pressure",
                ReturnedBytesNb = 1,
                MinValue = 0,
                MaxValue = 765,
                Unit = "kPa",
                Formula = "3*A"
            };

            //Arrange
            var pid = Pid.Create(pidParameters);

            //Assert
            Assert.AreEqual(pid.GetValue(new byte[] { }), 12);
        }
    }
}
