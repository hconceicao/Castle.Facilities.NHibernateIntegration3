namespace Castle.Facilities.NHibernateIntegration.SessionStores
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading;

	public class AsyncLocalSessionStore : AbstractDictStackSessionStore
	{
		private readonly AsyncLocal<Dictionary<string, IDictionary>> stateful;
		private readonly AsyncLocal<Dictionary<string, IDictionary>> stateless;

		public AsyncLocalSessionStore()
		{
			stateful = new AsyncLocal<Dictionary<string, IDictionary>>();
			stateless = new AsyncLocal<Dictionary<string, IDictionary>>();
		}

		/// <summary>
		/// Gets the dictionary.
		/// </summary>
		/// <returns></returns>
		protected override IDictionary GetDictionary()
		{
			var name = SlotKey;
			return GetDictionary(name, stateful);
		}

		internal static IDictionary GetDictionary(string name, AsyncLocal<Dictionary<string, IDictionary>> source)
		{
			IDictionary dic;

			if (source.Value == null)
				source.Value = new Dictionary<string, IDictionary>();

			source.Value.TryGetValue(name, out dic);

			return dic;
		}

		/// <summary>
		/// Stores the dictionary.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		protected override void StoreDictionary(IDictionary dictionary)
		{
			var key = SlotKey;

			StoreDictionary(dictionary, key, stateful);
		}

		internal static void StoreDictionary(IDictionary dictionary, string key, AsyncLocal<Dictionary<string, IDictionary>> source)
		{
			if (source.Value == null)
				source.Value = new Dictionary<string, IDictionary>();

			source.Value[key] = dictionary;
		}

		/// <summary>
		/// Gets the IStatelessSession dictionary.
		/// </summary>
		/// <returns>A dictionary.</returns>
		protected override IDictionary GetStatelessSessionDictionary()
		{
			return GetDictionary(StatelessSessionSlotKey, stateless);
		}

		/// <summary>
		/// Stores the IStatelessSession dictionary.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		protected override void StoreStatelessSessionDictionary(IDictionary dictionary)
		{
			var statelessSessionSlotKey = StatelessSessionSlotKey;

			StoreDictionary(dictionary, statelessSessionSlotKey, stateless);
		}
	}
}