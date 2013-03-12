﻿using System;
using CSharpAnalytics.Protocols.Measurement;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Measurement
{
    [TestClass]
    public class MeasurementConfigurationTests
    {
        [TestMethod]
        public void MeasurementConfiguration_Constructor_With_Required_Parameters_Sets_Correct_Properties()
        {
            var configuration = new MeasurementConfiguration("UA-1234-5", "ApplicationName", "1.2.3.4");

            Assert.AreEqual("UA-1234-5", configuration.AccountId);
            Assert.AreEqual("ApplicationName", configuration.ApplicationName);
            Assert.AreEqual("1.2.3.4", configuration.ApplicationVersion);
        }

        [TestMethod]
        public void MeasurementConfiguration_Constructor_With_Required_Parameters_Sets_Correct_Defaults()
        {
            var configuration = new MeasurementConfiguration("UA-1234-5", "ApplicationName", "1.2.3.4");

            Assert.IsTrue(configuration.AnonymizeIp);
            Assert.IsFalse(configuration.UseSsl);
        }

#if WINDOWS_STORE
        [TestMethod]
        public void MeasurementConfiguration_Constructor_Throws_ArgumentException_If_AccountID_Does_Not_Start_With_UA()
        {
            Assert.ThrowsException<ArgumentException>(() => new MeasurementConfiguration("NO-1234-5", "ApplicationName", "1.2.3.4"));
        }

        [TestMethod]
        public void MeasurementConfiguration_Constructor_Throws_ArgumentException_If_AccountID_Does_Not_Have_Two_Numeric_Parts()
        {
            Assert.ThrowsException<ArgumentException>(() => new MeasurementConfiguration("UA-1234", "ApplicationName", "1.2.3.4"));
        }

#endif

#if NET45
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CustomVariable_Constructor_Throws_ArgumentOutOfRange_If_Enum_Undefined()
        {
            var configuration = new MeasurementConfiguration("NO-1234-5", "ApplicationName", "1.2.3.4");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MeasurementConfiguration_Constructor_Throws_ArgumentException_If_AccountID_Does_Not_Have_Two_Numeric_Parts()
        {
            var configuration = new MeasurementConfiguration("UA-1234", "ApplicationName", "1.2.3.4");
        }
#endif
    }
}