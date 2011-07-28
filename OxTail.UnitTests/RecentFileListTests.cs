using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OxTail.Controls;
using OxTailHelpers.Data;
using NUnit.Mocks;
using OxTailHelpers;
using OxTail.Helpers;
using OxTailLogic;
using OxTailLogic.PatternMatching;
using System.Text.RegularExpressions;

namespace OxTail.UnitTests
{
    [TestFixture]    
    public class PatternMatchingTests
    {
        [Test]
        public void PostCode_Pattern_Succesfully_Matched()
        {
            IStringPatternMatching pattern = OxTailLogic.PatternMatching.StringPatternMatching.CreatePatternMatching();
            if(pattern.MatchPattern("B124TR", @"([a-zA-Z]{1,2}\w{1,2})+(\d{1}[a-zA-Z]{2})+"))
            {
                Assert.Pass();
            }

            else
            {
                Assert.Fail();
            }
        }
    }
}
