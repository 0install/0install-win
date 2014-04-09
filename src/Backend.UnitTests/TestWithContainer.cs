/*
 * Copyright 2010-2014 Bastian Eicher
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
using NanoByte.Common;
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

        protected AutoMockContainer Container { get; private set; }
        protected Config Config { get; private set; }
        protected MockHandler MockHandler { get; private set; }

        /// <summary>
        /// The object to be tested.
        /// </summary>
        protected TTarget Target;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Container = new AutoMockContainer(MockRepository);

            MockHandler = new MockHandler();
            Container.Register<ICommandHandler>(MockHandler);
            Container.Register<IInteractionHandler>(MockHandler);
            Container.Register<ITaskHandler>(MockHandler);

            Container.Register(Config = new Config());

            Target = Container.Create<TTarget>();

            _redirect = new LocationsRedirect("0install-unit-tests");
        }

        [TearDown]
        public override void TearDown()
        {
            _redirect.Dispose();

            base.TearDown();
        }
    }
}
