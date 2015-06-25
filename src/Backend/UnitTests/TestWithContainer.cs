/*
 * Copyright 2010-2015 Bastian Eicher
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

using Moq;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NUnit.Framework;
using ZeroInstall.Services;
using ZeroInstall.Store;

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

        protected MockTaskHandler MockHandler { get; private set; }

        /// <summary>
        /// The object to be tested.
        /// </summary>
        protected TTarget Target;

        /// <summary>
        /// Hook that can be used to register objects in the <see cref="AutoMockContainer"/> before the <see cref="Target"/> is constructed.
        /// </summary>
        protected virtual void Register(AutoMockContainer container)
        {
            container.Register<ITaskHandler>(MockHandler = new MockTaskHandler());

            container.Register(new Config());
        }

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
        /// Provides an instance of <typeparamref name="T"/> from the underlying <see cref="AutoMockContainer"/>.
        /// </summary>
        protected T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        [TearDown]
        public override void TearDown()
        {
            _redirect.Dispose();

            base.TearDown();
        }
    }
}
