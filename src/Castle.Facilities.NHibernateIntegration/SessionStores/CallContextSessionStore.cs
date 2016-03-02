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
namespace Castle.Facilities.NHibernateIntegration.SessionStores
{
	using System;
	using System.Collections;
	using System.Runtime.Remoting.Messaging;
	using Core.Logging;
	using Services.Transaction;

	/// <summary>
	/// Provides an implementation of <see cref="ISessionStore"/>
	/// which relies on <c>CallContext</c>
	/// </summary>
	public class CallContextSessionStore : AbstractDictStackSessionStore
	{
		/// <summary>
		/// Gets the dictionary.
		/// </summary>
		/// <returns></returns>
		protected override IDictionary GetDictionary()
		{
			var name = SlotKey;

			return GetDictionary(name, Logger);
		}

		internal static IDictionary GetDictionary(string name, ILogger log = null)
		{
			var txctx = TransactionCallContext.Get();

			if (txctx != null)
			{
				(log ?? NullLogger.Instance).Info("TxCtx = " + txctx.Id);

				object store;
				txctx.TryGetValue(name, out store);

				return store as IDictionary;
			}
			else
				return CallContext.LogicalGetData(name) as IDictionary;
		}

		/// <summary>
		/// Stores the dictionary.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		protected override void StoreDictionary(IDictionary dictionary)
		{
			var key = SlotKey;

			StoreDictionary(dictionary, key);
		}

		internal static void StoreDictionary(IDictionary dictionary, string key)
		{
			var txctx = TransactionCallContext.Get();

			if (txctx != null)
				txctx[key] = dictionary;
			else
				CallContext.LogicalSetData(key, dictionary);
		}

		/// <summary>
		/// Gets the IStatelessSession dictionary.
		/// </summary>
		/// <returns>A dictionary.</returns>
		protected override IDictionary GetStatelessSessionDictionary()
		{
			return GetDictionary(StatelessSessionSlotKey);
		}

		/// <summary>
		/// Stores the IStatelessSession dictionary.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		protected override void StoreStatelessSessionDictionary(IDictionary dictionary)
		{
			var statelessSessionSlotKey = StatelessSessionSlotKey;

			StoreStatelessSessionDictionary(dictionary, statelessSessionSlotKey);
		}

		internal static void StoreStatelessSessionDictionary(IDictionary dictionary, string statelessSessionSlotKey)
		{
			var txctx = TransactionCallContext.Get();

			if (txctx != null)
				txctx[statelessSessionSlotKey] = dictionary;
			else
				CallContext.LogicalSetData(statelessSessionSlotKey, dictionary);
		}
	}
}