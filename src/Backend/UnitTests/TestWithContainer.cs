/*
 * Copyright 2010-2016 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Moq;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using Xunit;
using ZeroInstall.Services;
using ZeroInstall.Store;
using ZeroInstall.Store.Trust;

namespace ZeroInstall
{
    /// <summary>
    /// Common base class for test fixtures that use <see cref="AutoMockContainer"/>.
    /// </summary>
    /// <typeparam name="TSut">The type of the object to be instantiated and tested (system under test).</typeparam>
    [Collection("LocationsRedirect")]
    public class TestWithContainer<TSut> : TestWithMocks
        where TSut : class
    {
        private readonly LocationsRedirect _redirect = new LocationsRedirect("0install-unit-tests");

        public override void Dispose()
        {
            _redirect.Dispose();
            if (_sut.IsValueCreated) (_sut.Value as IDisposable)?.Dispose();
            base.Dispose();
        }

        protected readonly AutoMockContainer Container;
        protected readonly MockTaskHandler Handler;
        protected readonly Config Config;
        protected readonly TrustDB TrustDB;

        protected TestWithContainer()
        {
            Container = new AutoMockContainer(MockRepository);
            Container.Register<ITaskHandler>(Handler = new MockTaskHandler());
            Container.Register(Config = new Config());
            Container.Register(TrustDB = new TrustDB());

            _sut = new Lazy<TSut>(() => Container.Create<TSut>());
        }

        private readonly Lazy<TSut> _sut;

        /// <summary>
        /// The object to be tested (system under test).
        /// </summary>
        protected TSut Sut => _sut.Value;

        /// <summary>
        /// Creates or retrieves a <see cref="Mock"/> for a specific type. Multiple requests for the same type return the same mock instance.
        /// These are the same mocks that are injected into the <see cref="Sut"/>.
        /// </summary>
        /// <remarks>All created <see cref="Mock"/>s are automatically <see cref="Mock.Verify"/>d after the test completes.</remarks>
        protected Mock<T> GetMock<T>() where T : class => Container.GetMock<T>();

        /// <summary>
        /// Provides an instance of a specific type.
        /// Will usually be a <see cref="Mock"/> as provided by <see cref="GetMock{T}"/> unless a custom instance has been registered in the constructor.
        /// </summary>
        protected T Resolve<T>() where T : class => Container.Resolve<T>();
    }
}
