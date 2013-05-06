/*
 * Copyright 2010-2013 Bastian Eicher
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
using Moq;
using NUnit.Framework;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Feeds;

namespace ZeroInstall
{
    /// <summary>
    /// Common base class for test fixtures that use <see cref="AutoMockContainer"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to be instantiated and tested.</typeparam>
    public class TestWithResolver<T> where T : class
    {
        private LocationsRedirect _redirect;
        private MockRepository _repository;
        protected AutoMockContainer Resolver;
        protected Config Config;

        /// <summary>
        /// The object to be tested.
        /// </summary>
        protected T Target;

        [SetUp]
        public virtual void SetUp()
        {
            Resolver = new AutoMockContainer(_repository = new MockRepository(MockBehavior.Loose));
            Resolver.Register(Config = new Config());
            Resolver.Register(Resolver.Create<TrustManager>());
            Target = Resolver.Create<T>();

            _redirect = new LocationsRedirect("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _redirect.Dispose();
            _repository.VerifyAll();
        }
    }
}
