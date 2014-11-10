﻿using Hjerpbakk.FermiContainer;
using NUnit.Framework;
using System.Collections.Generic;

namespace TestHjerpbakk.FermiContainer
{
    [TestFixture]
    public class FermiContainerTests
    {
        private IFermiContainer m_fermiContainer;

        [SetUp]
        public void Init()
        {
            m_fermiContainer = new Hjerpbakk.FermiContainer.FermiContainer();
        }

        [Test]
        public void Resolve_ClassRegistered_ReturnsNewObject()
        {
            m_fermiContainer.Register<ICalculator, Calculator>();

            var calculator = m_fermiContainer.Resolve<ICalculator>();

            var sum = calculator.Add(1, 2);
            Assert.AreEqual(3, sum);
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void Resolve_ClassNotRegistered_ThrowsException()
        {
            m_fermiContainer.Resolve<ICalculator>();
        }

        [Test]
        public void ResolveTwoTimes_ClassRegistered_TwoObjectsCreated()
        {
            m_fermiContainer.Register<ICalculator, Calculator>();
            var calculator = m_fermiContainer.Resolve<ICalculator>();

            var calculator2 = m_fermiContainer.Resolve<ICalculator>();

            Assert.AreNotSame(calculator, calculator2);
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void Singleton_ClassNotRegistered_ThrowsException()
        {
            m_fermiContainer.Singleton<ICalculator>();
        }

        [Test]
        public void SingletonTwoTimes_ClassRegistered_OneObjectCreated()
        {
            m_fermiContainer.Register<ICalculator, Calculator>();
            var calculator = m_fermiContainer.Singleton<ICalculator>();

            var calculator2 = m_fermiContainer.Singleton<ICalculator>();

            Assert.AreSame(calculator, calculator2);
        }

        [Test]
        public void Resolve_ClassRegisteredWithCustomCtor_ReturnsNewObject()
        {
            m_fermiContainer.Register<ICalculator, Calculator>(() => new Calculator());

            var calculator = m_fermiContainer.Resolve<ICalculator>();

            var sum = calculator.Add(1, 2);
            Assert.AreEqual(3, sum);
        }

        private interface ICalculator
        {
            int Add(int a, int b);
        }

        private class Calculator : ICalculator
        {
            public int Add(int a, int b)
            {
                return a + b;
            }
        }
    }
}

