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

using Common.Storage;
using Common.Tasks;
using Moq;
using NUnit.Framework;
using ZeroInstall.Store;

namespace ZeroInstall
{
    /// <summary>
    /// Common base class for test fixtures that use <see cref="AutoMockContainer"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to be instantiated and tested.</typeparam>
    public class TestWithContainer<T> where T : class
    {
        private LocationsRedirect _redirect;
        private MockRepository _repository;
        protected AutoMockContainer Container;
        protected Config Config;

        /// <summary>
        /// The object to be tested.
        /// </summary>
        protected T Target;

        [SetUp]
        public virtual void SetUp()
        {
            Container = new AutoMockContainer(_repository = new MockRepository(MockBehavior.Loose));
            Container.Register(Config = new Config());
            Target = Container.Create<T>();

            _redirect = new LocationsRedirect("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _redirect.Dispose();
            _repository.VerifyAll();
        }

        protected void ProvideCancellationToken()
        {
            Container.GetMock<IHandler>().SetupGet(x => x.CancellationToken).Returns(new CancellationToken());
        }
    }
}
