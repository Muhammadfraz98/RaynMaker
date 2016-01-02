﻿using System;
using System.IO;
using NUnit.Framework;
using RaynMaker.Modules.Import.Spec;
using RaynMaker.Modules.Import.Spec.v2.Locating;

namespace RaynMaker.Modules.Import.UnitTests.Spec.Locating
{
    [TestFixture]
    public class DocumentLocatorTests
    {
        [Test]
        public void Clone_WhenCalled_AllMembersAreCloned()
        {
            var navi = new DocumentLocator(
                new Request( "http://test1.org" ),
                new Response( "http://test2.org" ) );

            var clone = FigureDescriptorFactory.Clone( navi );

            Assert.That( clone.FragmentsHashCode, Is.EqualTo( navi.FragmentsHashCode ) );

            Assert.That( clone.Fragments[ 0 ].UrlString, Is.EqualTo( "http://test1.org" ) );
            Assert.That( clone.Fragments[ 1 ].UrlString, Is.EqualTo( "http://test2.org" ) );
        }
    }
}
