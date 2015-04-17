namespace Castle.Facilities.NHibernateIntegration.SessionStores
{
	using System.Collections;
	using System.Runtime.Remoting.Messaging;
	using System.Web;
	using MicroKernel.Facilities;

	public class HybridSessionStore : AbstractDictStackSessionStore
	{
		protected override IDictionary GetDictionary()
		{
			return IsNonWeb()
				? CallContext.GetData(SlotKey) as IDictionary
				: ObtainSessionContext().Items[SlotKey] as IDictionary;
		}

		protected override void StoreDictionary(IDictionary dictionary)
		{
			if (IsNonWeb())
				CallContext.SetData(SlotKey, dictionary);
			else
				ObtainSessionContext().Items[SlotKey] = dictionary;
		}

		protected override IDictionary GetStatelessSessionDictionary()
		{
			return IsNonWeb()
				? CallContext.GetData(StatelessSessionSlotKey) as IDictionary
				: ObtainSessionContext().Items[StatelessSessionSlotKey] as IDictionary;
		}

		protected override void StoreStatelessSessionDictionary(IDictionary dictionary)
		{
			if (IsNonWeb())
				CallContext.SetData(StatelessSessionSlotKey, dictionary);
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