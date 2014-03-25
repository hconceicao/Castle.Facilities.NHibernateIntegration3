namespace Castle.Facilities.NHibernateIntegration.Internal
{
	using System;
	using System.Collections;
	using System.Data;
	using System.Data.Common;
	using System.Transactions;
	using NHibernate;
	using NHibernate.Engine;
	using NHibernate.Engine.Transaction;
	using NHibernate.Exceptions;
	using NHibernate.Impl;
	using NHibernate.Transaction;

	public class CastleFriendlyScopelessTxFactory : ITransactionFactory
	{
		private static readonly IInternalLogger logger = LoggerProvider.LoggerFor(typeof(CastleFriendlyScopelessTxFactory));

		public void Configure(IDictionary props)
		{
		}

		public ITransaction CreateTransaction(ISessionImplementor session)
		{
			return new AdoBoundTransaction(session);
		}

		public void EnlistInDistributedTransactionIfNeeded(ISessionImplementor session)
		{
			if (session.TransactionContext != null)
				return;

			var ambientTx = Transaction.Current;

			if (ambientTx == null)
				return;

			var transactionContext = new DistributedTransactionContext(session);
			session.TransactionContext = transactionContext;

			logger.DebugFormat("Enlisted into ambient transaction: {0}. Id: {1}.", ambientTx.IsolationLevel, 
				ambientTx.TransactionInformation.LocalIdentifier);

			session.AfterTransactionBegin(null);

			if (!session.ConnectionManager.Transaction.IsActive)
			{
				transactionContext.ShouldCloseSessionOnDistributedTransactionCompleted = true;
				session.ConnectionManager.Transaction.Begin(ambientTx.IsolationLevel.AsDataIsolationLevel());
			} 
			else
			{
				logger.Debug("Tx is active");
			}
			
			ambientTx.EnlistVolatile(transactionContext, EnlistmentOptions.EnlistDuringPrepareRequired);
		}

		public bool IsInDistributedActiveTransaction(ISessionImplementor session)
		{
			var distributedTransactionContext = ((DistributedTransactionContext) session.TransactionContext);

			return distributedTransactionContext != null && distributedTransactionContext.IsInActiveTransaction;
		}

		public void ExecuteWorkInIsolation(ISessionImplementor session, IIsolatedWork work, bool transacted)
		{
			logger.Debug("ExecuteWorkInIsolation Session: " + session.SessionId);

			IDbConnection connection = null;
			IDbTransaction trans = null;
			// bool wasAutoCommit = false;
			try
			{
				connection = session.Factory.ConnectionProvider.GetConnection();

				if (transacted)
				{
					trans = connection.BeginTransaction();
				}

				work.DoWork(connection, trans);

				if (transacted)
				{
					trans.Commit();
				}
			}
			catch (Exception t)
			{
                using (new SessionIdLoggingContext(session.SessionId))
                {
                    try
                    {
                        if (trans != null && connection.State != ConnectionState.Closed)
                        {
                            trans.Rollback();
                        }
                    }
                    catch (Exception ignore)
                    {
                        logger.Debug("unable to release connection on exception [" + ignore + "]");
                    }

                    if (t is HibernateException)
                    {
                        throw;
                    }
                    else if (t is DbException)
                    {
                        throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, t,
                                                         "error performing isolated work");
                    }
                    else
                    {
                        throw new HibernateException("error performing isolated work", t);
                    }
                }
			}
			finally
			{
				session.Factory.ConnectionProvider.CloseConnection(connection);
			}
		}

		public class DistributedTransactionContext : ITransactionContext, IEnlistmentNotification
		{
			private readonly ISessionImplementor session;
			private readonly AdoBoundTransaction nhtx;

			public bool IsInActiveTransaction;
			
			public DistributedTransactionContext(ISessionImplementor session)
			{
				this.session = session;

				nhtx =  session.ConnectionManager.Transaction as AdoBoundTransaction;
				
				IsInActiveTransaction = true;
			}

			//public System.Transactions.Transaction AmbientTransation { get; set; }
			
			public bool ShouldCloseSessionOnDistributedTransactionCompleted { get; set; }
			
			#region IEnlistmentNotification Members

			void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
			{
				using (new SessionIdLoggingContext(session.SessionId))
				{
					try
					{
						logger.Debug("Preparing NHibernate resource");

						nhtx.Prepare();
						
						preparingEnlistment.Prepared();
					}
					catch (Exception exception)
					{
						logger.Error("Transaction prepare phase failed", exception);

						preparingEnlistment.ForceRollback(exception);
					}
				}
			}

			void IEnlistmentNotification.Commit(Enlistment enlistment)
			{
				//Console.WriteLine("ctxn c");

				using (new SessionIdLoggingContext(session.SessionId))
				{
					logger.Debug("Committing NHibernate resource");
					
					nhtx.Commit();
					End(true);
					
					enlistment.Done();

					IsInActiveTransaction = false;
				}
			}

			void IEnlistmentNotification.Rollback(Enlistment enlistment)
			{
				//Console.WriteLine("ctxn r");

				using (new SessionIdLoggingContext(session.SessionId))
				{
					//session.AfterTransactionCompletion(false, null);
					logger.Debug("Rolled back NHibernate resource");

					nhtx.Rollback();
					End(false);
					
					enlistment.Done();
					
					IsInActiveTransaction = false;
				}
			}

			void IEnlistmentNotification.InDoubt(Enlistment enlistment)
			{
				using (new SessionIdLoggingContext(session.SessionId))
				{
					session.AfterTransactionCompletion(false, null);
					logger.Debug("NHibernate resource is in doubt");
					
					End(false);
					
					enlistment.Done();
					IsInActiveTransaction = false;
				}
			}

			void End(bool wasSuccessful)
			{
				using (new SessionIdLoggingContext(session.SessionId))
				{
					((DistributedTransactionContext) session.TransactionContext).IsInActiveTransaction = false;
							
					session.AfterTransactionCompletion(wasSuccessful, null);

					if (ShouldCloseSessionOnDistributedTransactionCompleted)
					{
						session.CloseSessionFromDistributedTransaction();
					}

					session.TransactionContext = null;
				}
			}

			#endregion

			public void Dispose()
			{
				//nhtx.Dispose();
			}
		}
	}
}