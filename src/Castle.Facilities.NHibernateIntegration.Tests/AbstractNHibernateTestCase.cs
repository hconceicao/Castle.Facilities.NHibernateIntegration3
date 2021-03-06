#region License
//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  
#endregion
namespace Castle.Facilities.NHibernateIntegration.Tests
{
	using System;
	using AutoTx;
	using Core.Resource;
	using NHibernate.Tool.hbm2ddl;
	using NUnit.Framework;
	using Windsor;
	using Windsor.Configuration.Interpreters;

	public abstract class AbstractNHibernateTestCase
	{
		protected IWindsorContainer container;

		protected virtual string ConfigurationFile
		{
			get { return "DefaultConfiguration.xml"; }
		}

		protected virtual void ExportDatabaseSchema()
		{
			NHibernate.Cfg.Configuration[] cfgs = container.ResolveAll<NHibernate.Cfg.Configuration>();
			foreach (var cfg in cfgs)
			{
				var export = new SchemaExport(cfg);
				export.Create(false, true);
			}
		}

		protected virtual void DropDatabaseSchema()
		{
			var cfgs = container.ResolveAll<NHibernate.Cfg.Configuration>();
			foreach (var cfg in cfgs)
			{
				var export = new SchemaExport(cfg);
				export.Drop(false, true);
			}
		}

		[SetUp]
		public virtual void SetUp()
		{
			container = new WindsorContainer(new XmlInterpreter(new AssemblyResource(GetContainerFile())));
			container.AddFacility<AutoTxFacility>();
			ConfigureContainer();
			ExportDatabaseSchema();
			OnSetUp();
		}

		[TearDown]
		public virtual void TearDown()
		{
			Console.WriteLine("Tear down");

			OnTearDown();
			DropDatabaseSchema();
			container.Dispose();
			container = null;
		}

		protected virtual void ConfigureContainer()
		{
		}


		public virtual void OnSetUp()
		{
		}

		public virtual void OnTearDown()
		{
		}


		protected string GetContainerFile()
		{
			return "Castle.Facilities.NHibernateIntegration.Tests/" + ConfigurationFile;
		}
	}
}