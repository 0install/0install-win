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
using NUnit.Framework;
using ZeroInstall.Services;
using ZeroInstall.Store;
using ZeroInstall.Store.Trust;

namespace ZeroInstall
{
    /// <summary>
    /// Common base class for test fixtures that use <see cref="AutoMockContainer"/>.
    /// </summary>
    /// <typeparam name="TTarget">The type of the object to be instantiated and tested.</typeparam>
    public class TestWithContainer<TTarget> : TestWithMocks
        where TTarget : class
    {
        private LocationsRedirect _redirect;
        private AutoMockContainer _container;

        /// <summary>
        /// The object to be tested.
        /// </summary>
        protected TTarget Target { get; private set; }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _redirect = new LocationsRedirect("0install-unit-tests");

            _container = new AutoMockContainer(MockRepository);
            Register(_container);

            Target = _container.Create<TTarget>();
        }

        /// <summary>
        /// Creates or retrieves a <see cref="Mock"/> for a specific type. Multiple requests for the same type return the same mock instance.
        /// These are the same mocks that are injected into the <see cref="Target"/>.
        /// </summary>
        /// <remarks>All created <see cref="Mock"/>s are automatically <see cref="Mock.Verify"/>d after the test completes.</remarks>
        protected Mock<T> GetMock<T>()
            where T : class
        {
            return _container.GetMock<T>();
        }

        /// <summary>
        /// Provides an instance of a specific type.
        /// Will usually be a <see cref="Mock"/> as provided by <see cref="GetMock{T}"/> unless a custom instance has been registered in <see cref="Register"/>.
        /// </summary>
        protected T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        protected MockTaskHandler Handler { get; private set; }
        protected Config Config { get; private set; }
        protected TrustDB TrustDB { get; private set; }

        /// <summary>
        /// Hook that can be used to register objects in the <see cref="AutoMockContainer"/> before the <see cref="Target"/> is constructed.
        /// </summary>
        protected virtual void Register(AutoMockContainer container)
        {
            container.Register<ITaskHandler>(Handler = new MockTaskHandler());
            container.Register(Config = new Config());
            container.Register(TrustDB = new TrustDB());
        }

        [TearDown]
        public override void TearDown()
        {
            _redirect.Dispose();

            var diposable = Target as IDisposable;
            if (diposable != null) diposable.Dispose();

            base.TearDown();
        }
    }
}
