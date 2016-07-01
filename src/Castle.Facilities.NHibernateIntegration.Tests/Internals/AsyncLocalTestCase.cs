namespace Castle.Facilities.NHibernateIntegration.Tests.Internals
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading;
	using System.Threading.Tasks;
	using Moq;
	using NHibernate;
	using NUnit.Framework;
	using SessionStores;

	public class Wrapper : ICloneable
	{
		public Wrapper()
		{
			Console.WriteLine("Ctor");
		}

		public int Counter;

		public object Clone()
		{
			Console.WriteLine("Clone");
			return new Wrapper();
		}

		public override string ToString()
		{
			return "Wrapper " + Counter;
		}
	}

	[TestFixture]
	public class AsyncLocalTestCase
	{
//		private AsyncLocalSessionStore _localSession;
		private AsyncLocal<Wrapper> _localSession;

		[SetUp]
		public void SetUp()
		{
			_localSession= new AsyncLocal<Wrapper>((args) =>
			{
				Console.WriteLine("Changed from {0} to {1} thread {2} ", args.PreviousValue, args.CurrentValue, args.ThreadContextChanged/*, new StackTrace()*/);
			});
//			_localSession = new AsyncLocalSessionStore();
		}

		// Case 1 : inner branch starts session (outter transaction holds it)
		// Case 2 : outter branch starts session (inner re-use it)
		// All cases, sessions are not shared among threads

		[Test]
		public void Case1()
		{
			var tasks = new List<Task>();

			for (int i = 0; i < 1; i++)
			{
				var task = new Task(async (s) => await EntryPoint(s), i);
				task.Start(TaskScheduler.Current);
				tasks.Add(task);
			}

			Task.WaitAll(tasks.ToArray());

//			var res = _localSession.Value;
//			Console.WriteLine("res " + res);
		}
		
		public async Task EntryPoint(object s)
		{
//			var sess = new SessionDelegate(true, new Mock<ISession>().Object, _localSession);
//			_localSession.Store("default", sess);
			if (_localSession.Value != null) throw new Exception("What?");
			_localSession.Value = new Wrapper();

			await Branch1();
			await Branch2();

			var res = _localSession.Value; // _localSession.FindCompatibleSession("default");
//			_localSession.Value = null;
			Console.WriteLine("[" + s + "] res " + res);

//			sess.Dispose();

//			res = _localSession.Value;
		}

		public Task Branch1()
		{
			// var a = _localSession.FindCompatibleSession("default");
			var a = _localSession.Value.Counter++;
			
			return Task.CompletedTask;
		}

		public Task Branch2()
		{
			var b = _localSession.Value.Counter++;

			var tcs = new TaskCompletionSource<bool>();

			ThreadPool.QueueUserWorkItem((state) =>
			{
				// var a = _localSession.FindCompatibleSession("default");
				var a = _localSession.Value.Counter++;

				tcs.SetResult(true);
			}, null);

//			tcs.SetResult(true);

//			Task.Run(() =>
//			{
//				var a = _localSession.FindCompatibleSession("default");
//
//				tcs.SetResult(true);
//			});

			return tcs.Task;
		}

	}
}