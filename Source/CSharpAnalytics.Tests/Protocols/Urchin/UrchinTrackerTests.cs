﻿using System;
using System.Linq;
using CSharpAnalytics.Activities;
using CSharpAnalytics.CustomVariables;
using CSharpAnalytics.Protocols.Urchin;
using CSharpAnalytics.Sessions;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Urchin
{
    [TestClass]
    public class UrchinTrackerTests
    {
        [TestMethod]
        public void UrchinTracker_GetParameters_For_Configuration_Returns_Correct_Keys()
        {
            var configuration = new UrchinConfiguration("UA-1234-5", "hostName");

            var keys = UrchinTracker.GetParameters(configuration).Select(k => k.Key).ToArray();

            CollectionAssert.AreEquivalent(new[] { "utmac", "utmhn", "aip" }, keys);
        }

        [TestMethod]
        public void UrchinTracker_GetParameters_For_Configuration_Returns_No_Aip_Value_When_False()
        {
            var configuration = new UrchinConfiguration("UA-1234-5", "hostName") { AnonymizeIp = false };

            var keys = UrchinTracker.GetParameters(configuration).Select(k => k.Key).ToArray();

            CollectionAssert.DoesNotContain(keys, "aip");
        }

        [TestMethod]
        public void UrchinTracker_GetParameters_For_Environment_Returns_Correct_Keys()
        {
            var environment = new Environment("utf-8");

            var keys = UrchinTracker.GetParameters(environment).Select(k => k.Key).ToArray();

            CollectionAssert.AreEquivalent(new[] { "utmul", "utmcs", "utmje", "utmfl" }, keys);
        }

        [TestMethod]
        public void UrchinTracker_GetParameters_For_Environment_Returns_Correct_Values()
        {
            var environment = new Environment("en-gb")
                {
                    CharacterSet = "ISO-8550-1",
                    FlashVersion = "11.0.1b",
                    ScreenColorDepth = 32,
                    IpAddress = "127.0.0.1",
                    JavaEnabled = true,
                    ScreenHeight = 1050,
                    ScreenWidth = 1920,
                    ViewportHeight = 768,
                    ViewportWidth = 1024
                };

            var parameters = UrchinTracker.GetParameters(environment).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual("ISO-8550-1", parameters["utmcs"]);
            Assert.AreEqual("en-gb", parameters["utmul"]);
            Assert.AreEqual("11.0.1b", parameters["utmfl"]);
            Assert.AreEqual("32-bit", parameters["utmsc"]);
            Assert.AreEqual("127.0.0.1", parameters["utmip"]);
            Assert.AreEqual("1", parameters["utmje"]);
            Assert.AreEqual("1024x768", parameters["utmvp"]);
            Assert.AreEqual("1920x1050", parameters["utmsr"]);
        }

        [TestMethod]
        public void UrchinTracker_GetParameters_For_Environment_Returns_Correct_Utmje_Value()
        {
            var environment = new Environment("en-gb") { JavaEnabled = false };

            var utmjeValue = UrchinTracker.GetParameters(environment).First(f => f.Key == "utmje").Value;

            Assert.AreEqual("0", utmjeValue);
        }

        [TestMethod]
        public void UrchinTracker_CreateCookieSubstituteParameter()
        {
            var @event = new EventActivity("action", "catgory", "label", 123, true);
            var sessionState = new SessionState
                {
                    SessionNumber = 5,
                    VisitorId = new Guid("FFFCCBCB-9A87-4987-BD20-CE7C81F96CD2"),
                    FirstVisitAt = new DateTimeOffset(2012, 10, 10, 13, 14, 15, TimeSpan.Zero),
                    PreviousSessionStartedAt = new DateTimeOffset(2012, 12, 10, 13, 14, 15, TimeSpan.Zero),
                    SessionStartedAt = new DateTimeOffset(2012, 12, 14, 13, 14, 15, TimeSpan.Zero),
                };
            var sessionManager = new SessionManager(TimeSpan.FromMinutes(5), sessionState);

            var cookieSubstitute = UrchinTracker.CreateCookieSubstituteParameter(sessionManager, 1);

            Assert.AreEqual("__utma=1.1159017511.1349874855.1355145255.1355490855.5;", cookieSubstitute);
        }

        [TestMethod]
        public void UrchinTracker_GetFinalCustomVariables_Selects_Correct_Final_Variables()
        {
            var sessionScopedVariables = new ScopedCustomVariableSlots(CustomVariableScope.Session);
            sessionScopedVariables[0] = new CustomVariable("session-one-name", "session-one-value");
            sessionScopedVariables[2] = new CustomVariable("session-three-name", "session-three-value");

            var visitorScopedVariables = new ScopedCustomVariableSlots(CustomVariableScope.Visitor);
            visitorScopedVariables[0] = new CustomVariable("Visitor-one-name", "Visitor-one-value");
            visitorScopedVariables[1] = new CustomVariable("Visitor-two-name", "Visitor-two-value");

            var activityScopedVariables = new ScopedCustomVariableSlots(CustomVariableScope.Activity);
            activityScopedVariables[0] = new CustomVariable("activity-one-name", "activity-one-value");
            activityScopedVariables[1] = new CustomVariable("activity-two-name", "activity-two-value");
            activityScopedVariables[3] = new CustomVariable("activity-four-name", "activity-four-value");

            var final = UrchinTracker.GetFinalCustomVariables(sessionScopedVariables, visitorScopedVariables, activityScopedVariables);

            Assert.AreEqual(CustomVariableScope.Visitor, final[0].Scope);
            Assert.AreEqual(CustomVariableScope.Visitor, final[1].Scope);
            Assert.AreEqual(CustomVariableScope.Session, final[2].Scope);
            Assert.AreEqual(CustomVariableScope.Activity, final[3].Scope);

            Assert.AreEqual("Visitor-one-name", final[0].Variable.Name);
            Assert.AreEqual("Visitor-two-name", final[1].Variable.Name);
            Assert.AreEqual("session-three-name", final[2].Variable.Name);
            Assert.AreEqual("activity-four-name", final[3].Variable.Name);

            Assert.AreEqual(3, final.GetUpperBound(0));
        }
    }
}