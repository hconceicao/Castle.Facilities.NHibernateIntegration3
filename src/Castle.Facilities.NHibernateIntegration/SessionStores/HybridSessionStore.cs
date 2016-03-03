namespace Castle.Facilities.NHibernateIntegration.SessionStores
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.Remoting.Messaging;
	using System.Threading;
	using System.Web;
	using MicroKernel.Facilities;

	public class HybridSessionStore : AbstractDictStackSessionStore
	{
		private ThreadLocal< Dictionary<string, IDictionary>> stateful = new ThreadLocal< Dictionary<string, IDictionary>>(() => new Dictionary<string, IDictionary>() );
		private ThreadLocal< Dictionary<string, IDictionary>> stateless = new ThreadLocal< Dictionary<string, IDictionary>>(() => new Dictionary<string, IDictionary>() );

		protected override IDictionary GetDictionary()
		{
			return IsNonWeb()
				? ThreadLocalSessionStore.GetDictionary(SlotKey, stateful)
				: ObtainSessionContext().Items[SlotKey] as IDictionary;
		}

		protected override void StoreDictionary(IDictionary dictionary)
		{
			if (IsNonWeb())
				ThreadLocalSessionStore.StoreDictionary(dictionary, SlotKey, stateful);
			else
				ObtainSessionContext().Items[SlotKey] = dictionary;
		}

		protected override IDictionary GetStatelessSessionDictionary()
		{
			return IsNonWeb()
				? ThreadLocalSessionStore.GetDictionary(StatelessSessionSlotKey, stateless)
				: ObtainSessionContext().Items[StatelessSessionSlotKey] as IDictionary;
		}

		protected override void StoreStatelessSessionDictionary(IDictionary dictionary)
		{
			if (IsNonWeb())
				ThreadLocalSessionStore.StoreDictionary(dictionary, StatelessSessionSlotKey, stateless);
			else
				ObtainSessionContext().Items[StatelessSessionSlotKey] = dictionary;
		}

		private static bool IsNonWeb()
		{
			return HttpContext.Current == null;
		}

		private static HttpContext ObtainSessionContext()
		{
			HttpContext curContext = HttpContext.Current;

			if (curContext == null)
			{
				throw new FacilityException("WebSessionStore: Could not obtain reference to HttpContext");
			}

			return curContext;
		}
	}
}