﻿/*
    Copyright (C) 2006-2022. Aardvark Platform Team. http://github.com/aardvark-platform.
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using NUnit.Framework;
using static Aardvark.Data.E57.ASTM_E57;

namespace Aardvark.Geometry.Tests
{
    [TestFixture]
    public class E57Tests
    {
        [Test]
        public void E57_Addresses_InvalidPhysicalOffsets()
        {
            new E57PhysicalOffset(1019);
            Assert.That(() => new E57PhysicalOffset(1020), Throws.Exception);
            Assert.That(() => new E57PhysicalOffset(1021), Throws.Exception);
            Assert.That(() => new E57PhysicalOffset(1022), Throws.Exception);
            Assert.That(() => new E57PhysicalOffset(1023), Throws.Exception);
            new E57PhysicalOffset(1024);

            new E57PhysicalOffset(2043);
            Assert.That(() => new E57PhysicalOffset(2044), Throws.Exception);
            Assert.That(() => new E57PhysicalOffset(2045), Throws.Exception);
            Assert.That(() => new E57PhysicalOffset(2046), Throws.Exception);
            Assert.That(() => new E57PhysicalOffset(2047), Throws.Exception);
            new E57PhysicalOffset(2048);
        }

        [Test]
        public void E57_Addresses_PhysicalPlusPhysical()
        {
            Assert.IsTrue((new E57PhysicalOffset(100) + new E57PhysicalOffset(234)).Value == 334);
            Assert.IsTrue((new E57PhysicalOffset(600) + new E57PhysicalOffset(1000)).Value == 1600);
        }
        [Test]
        public void E57_Addresses_PhysicalPlusLogical()
        {
            E57LogicalOffset x = new E57PhysicalOffset(1000) + new E57LogicalOffset(100);
            Assert.IsTrue(x.Value == 1100);
        }
        [Test]
        public void E57_Addresses_PhysicalToLogical_1()
        {
            Assert.IsTrue(((E57LogicalOffset)new E57PhysicalOffset(0)).Value == 0);
            Assert.IsTrue(((E57LogicalOffset)new E57PhysicalOffset(1019)).Value == 1019);
            Assert.IsTrue(((E57LogicalOffset)new E57PhysicalOffset(1024)).Value == 1020);
            Assert.IsTrue(((E57LogicalOffset)new E57PhysicalOffset(2048)).Value == 2040);
            Assert.IsTrue(((E57LogicalOffset)new E57PhysicalOffset(3072)).Value == 3060);

            Assert.IsTrue(((E57LogicalOffset)new E57PhysicalOffset(1500)).Value == 1496);
        }
        [Test]
        public void E57_Addresses_PhysicalToLogical_2()
        {
            Assert.IsTrue(((E57LogicalOffset)new E57PhysicalOffset(1019)).Value == 1019);
            Assert.IsTrue(((E57LogicalOffset)new E57PhysicalOffset(1024)).Value == 1020);
            Assert.IsTrue(((E57LogicalOffset)new E57PhysicalOffset(1025)).Value == 1021);
        }

        [Test]
        public void E57_Addresses_LogicalPlusLogical()
        {
            Assert.IsTrue((new E57LogicalOffset(100) + new E57LogicalOffset(234)).Value == 334);
            Assert.IsTrue((new E57LogicalOffset(600) + new E57LogicalOffset(1000)).Value == 1600);
        }
        public void E57_Addresses_LogicalToPhysical()
        {
            Assert.IsTrue(((E57PhysicalOffset)new E57LogicalOffset(0)).Value == 0);
            Assert.IsTrue(((E57PhysicalOffset)new E57LogicalOffset(1019)).Value == 1019);
            Assert.IsTrue(((E57PhysicalOffset)new E57LogicalOffset(1020)).Value == 1024);
            Assert.IsTrue(((E57PhysicalOffset)new E57LogicalOffset(2039)).Value == 2043);
            Assert.IsTrue(((E57PhysicalOffset)new E57LogicalOffset(2040)).Value == 2048);
            Assert.IsTrue(((E57PhysicalOffset)new E57LogicalOffset(3059)).Value == 3067);
            Assert.IsTrue(((E57PhysicalOffset)new E57LogicalOffset(3060)).Value == 3072);
        }
    }
}
