﻿using System;
using System.Data;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Implements <see cref="IScope"/> when there is no scope.
    /// </summary>
    internal class NoScope : IScopeInternal
    {
        private readonly ScopeProvider _scopeProvider;
        private bool _disposed;

        private UmbracoDatabase _database;

        public NoScope(ScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
#if DEBUG_SCOPES
            _scopeProvider.RegisterScope(this);
#endif
        }

        private readonly Guid _instanceId = Guid.NewGuid();
        public Guid InstanceId { get { return _instanceId; } }

        /// <inheritdoc />
        public bool CallContext { get { return false; } }

        /// <inheritdoc />
        public RepositoryCacheMode RepositoryCacheMode
        {
            get { return RepositoryCacheMode.Default; }
        }

        /// <inheritdoc />
        public IsolatedRuntimeCache IsolatedRuntimeCache { get { throw new NotSupportedException(); } }

        /// <inheritdoc />
        public UmbracoDatabase Database
        {
            get
            {
                EnsureNotDisposed();
                return _database ?? (_database = _scopeProvider.DatabaseFactory.CreateNewDatabase());
            }
        }

        public UmbracoDatabase DatabaseOrNull
        {
            get
            {
                EnsureNotDisposed();
                return _database;
            }
        }

        /// <inheritdoc />
        public EventMessages Messages
        {
            get { throw new NotSupportedException(); }
        }

        public EventMessages MessagesOrNull
        {
            get { throw new NotSupportedException(); }
        }

        /// <inheritdoc />
        public IEventDispatcher Events
        {
            get { throw new NotSupportedException(); }
        }

        /// <inheritdoc />
        public bool Complete()
        {
            throw new NotSupportedException();
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("this");
        }

        public void Dispose()
        {
            EnsureNotDisposed();

            if (this != _scopeProvider.AmbientScope)
                throw new InvalidOperationException("Not the ambient scope.");

#if DEBUG_SCOPES
            _scopeProvider.Disposed(this);
#endif

            if (_database != null)
                _database.Dispose();

            _scopeProvider.SetAmbient(null);

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public IScopeInternal ParentScope { get { return null; } }
        public IsolationLevel IsolationLevel { get {return IsolationLevel.Unspecified; } }
        public bool ScopedFileSystems { get { return false; } }
        public void ChildCompleted(bool? completed) { }
        public void Reset() { }
    }
}
