namespace Castle.Facilities.NHibernateIntegration.Tests.Transactions
{
	using System;
	using MicroKernel.Registration;
	using NUnit.Framework;

	[TestFixture]
	public class BugsTestCase : AbstractNHibernateTestCase
	{
		protected override string ConfigurationFile
		{
			get { return "Transactions/TwoDatabaseConfiguration.xml"; }
		}

		protected override void ConfigureContainer()
		{
			container.Register(Component.For<RootService>().Named("root"));
			container.Register(Component.For<FirstDao>().Named("myfirstdao"));
			container.Register(Component.For<SecondDao>().Named("myseconddao"));
		}

		[Test]
		public void Exception_inside_a_transactional_scope_nested_in_a_readonly_scope()
		{
			Assert.Throws<ArithmeticException>(() => container.Resolve<RootService>().SessionNotFlushedWithTransactionNested());
			
			Assert.Throws<ArithmeticException>(() => container.Resolve<RootService>().SessionNotFlushedWithTransactionNested());
		}
	}
}
