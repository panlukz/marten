using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sodev.Marten.Base.Model;
using Sodev.Marten.Base.Helpers;

namespace Sodev.Marten.Tests.UnitTests.Persistence
{
    [TestClass]
    public class XmlSerializeHelperTests
    {
        [TestMethod]
        public void GivenSerializedPidXML_OnDeserializeObject_VerifyCorrectIdsAreDeserialized()
        {
            //Arrange
            var xmlString = "<?xml version=\"1.0\"?><ArrayOfPidParameters xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><PidParameters><Id>10</Id><Description>Fuel pressure</Description><ReturnedBytesNb>1</ReturnedBytesNb><MinValue>0</MinValue><MaxValue>765</MaxValue><Unit>kPa</Unit><Formula>3*A</Formula></PidParameters></ArrayOfPidParameters>";
            var expectedPidId = new int[] { 10 };
            
            //Act
            var pidList = XmlSerializeHelper.DeSerializeObject<List<PidParameters>>(xmlString);
            var idList = pidList.Select(p => p.Id).ToArray();

            //Assert
            CollectionAssert.AreEquivalent(expectedPidId, idList);
        }

        [TestMethod]
        public void GivenPidParametersObject_OnSerializeObject_VerifyCorrectFileIsProduced()
        {
            //Arrange
            var expectedXml = "<?xml version=\"1.0\"?><ArrayOfPidParameters xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><PidParameters><Id>10</Id><Description>Fuel pressure</Description><ReturnedBytesNb>1</ReturnedBytesNb><MinValue>0</MinValue><MaxValue>765</MaxValue><Unit>kPa</Unit><Formula>3*A</Formula></PidParameters></ArrayOfPidParameters>";

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

            var listOfPidParameters = new List<PidParameters> { pidParameters };

            //Act
            var xmlContent = XmlSerializeHelper.SerializeObject<List<PidParameters>>(listOfPidParameters);

            //Assert
            Assert.AreEqual(expectedXml, xmlContent);
        }
    }
}
