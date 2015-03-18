﻿using System;
using System.Diagnostics;
using Hjerpbakk.FermiContainer;
using NUnit.Framework;

namespace TestHjerpbakk.FermiContainer {
	[TestFixture]
	public class PerformanceTests {
		const int Iterations = 20000000;

		IFermiContainer m_fermiContainer;

		[SetUp]
		public void Init() {
			m_fermiContainer = new Hjerpbakk.FermiContainer.FermiContainer();
		}

		[Test]
		public void NewOperator() {
			var time = Time(() => {
				for (int i = 0; i < Iterations; i++) {
					// Analysis disable once ObjectCreationAsStatement
					new Calculator();
				}
			});

			Console.WriteLine("Baseline: " + time);
		}

		[Test]
		public void NoConstructorArguments() {
			m_fermiContainer.Register<ICalculator, Calculator>();

			var time = Time(() => {
				for (int i = 0; i < Iterations; i++) {
					m_fermiContainer.Resolve<ICalculator>();
				}
			});
					
			Console.WriteLine("No constructor args: " + time);
		}

		[Test]
		public void Singleton() {
			m_fermiContainer.Register<ICalculator, Calculator>();

			var time = Time(() => {
				for (int i = 0; i < Iterations; i++) {
					m_fermiContainer.Singleton<ICalculator>();
				}
			});
					
			Console.WriteLine("Singleton: " + time);
		}

		[Test]
		public void PreConstructed() {
			var calculator = new Calculator();
			m_fermiContainer.Register<ICalculator, Calculator>(() => calculator);

			var time = Time(() => {
				for (int i = 0; i < Iterations; i++) {
					m_fermiContainer.Resolve<ICalculator>();
				}
			});
					
			Console.WriteLine("Pre constructed: " + time);
		}

		[Test]
		public void ComplexClasses() {
			m_fermiContainer.Register<IEvenMoreComplex, EvenMoreComplex>();
			m_fermiContainer.Register<ICalculator, Calculator>();
			m_fermiContainer.Register<IComplex, ComplexClass>();
			m_fermiContainer.Register<ClassWithoutInterface>();

			var time = Time(() => {
				for (int i = 0; i < Iterations; i++) {
					m_fermiContainer.Resolve<IEvenMoreComplex>();
				}
			});
					
			Console.WriteLine("Complex classes: " + time);
		}

		public static long Time(Action action) {
			Stopwatch stopwatch = Stopwatch.StartNew();
			action();
			stopwatch.Stop();
			return stopwatch.ElapsedMilliseconds;
		}
	}
}

